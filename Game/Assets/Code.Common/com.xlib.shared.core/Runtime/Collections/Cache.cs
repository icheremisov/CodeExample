using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XLib.Core.Collections {
	
	/// <summary>
	/// Source https://github.com/jitbit/FastCache/
	/// </summary>
	public sealed class Cache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable where TKey : notnull {
		private readonly ConcurrentDictionary<TKey, TtlValue> _dict = new ConcurrentDictionary<TKey, TtlValue>();
		private readonly Timer _cleanUpTimer;
		private readonly SemaphoreSlim _lock = new(1);
		private readonly TimeSpan _timeToLive;

		/// <summary>
		/// Initializes a new empty instance of <see cref="Cache{TKey,TValue}"/>
		/// </summary>
		/// <param name="timeToLive">time that entity will live</param>
		/// <param name="cleanupJobInterval">cleanup interval is 10 sec</param>
		public Cache(TimeSpan timeToLive, TimeSpan? cleanupJobInterval = null) {
			_timeToLive = timeToLive;
			cleanupJobInterval ??= TimeSpan.FromSeconds(10);
			_cleanUpTimer = new Timer(s => { _ = EvictExpiredJob(); }, null, cleanupJobInterval.Value, cleanupJobInterval.Value);
		}

		private async Task EvictExpiredJob() {
			//if an applicaiton has many-many instances of FastCache objects, make sure the timer-based
			//cleanup jobs don't clash with each other, i.e. there are no clean-up jobs running in parallel
			//so we don't waste CPU resources, because cleanup is a busy-loop that iterates a collection and does calculations
			//so we use a lock to "throttle" the job and make it serial
			//HOWEVER, we still allow the user to execute eviction explicitly

			//use Semaphore instead of a "lock" to free up thread, otherwise - possible thread starvation

			await _lock.WaitAsync()
				.ConfigureAwait(false);
			try {
				EvictExpired();
			}
			finally {
				_lock.Release();
			}
		}

		/// <summary>
		/// Cleans up expired items (dont' wait for the background job)
		/// There's rarely a need to execute this method, b/c getting an item checks TTL anyway.
		/// </summary>
		public void EvictExpired() {
			//Eviction already started by another thread? forget it, lets move on
			if (Monitor.TryEnter(_cleanUpTimer)) //use the timer-object for our lock, it's local, private and instance-type, so its ok
			{
				try {
					//cache current tick count in a var to prevent calling it every iteration inside "IsExpired()" in a tight loop.
					//On a 10000-items cache this allows us to slice 30 microseconds: 330 vs 360 microseconds which is 10% faster
					//On a 50000-items cache it's even more: 2.057ms vs 2.817ms which is 35% faster!!
					//the bigger the cache the bigger the win
					var currTime = Environment.TickCount;

					foreach (var p in _dict) {
						if (currTime > p.Value.TickCountWhenToKill) //instead of calling "p.Value.IsExpired" we're essentially doing the same thing manually
							_dict.TryRemove(p.Key, out _);
					}
				}
				finally {
					Monitor.Exit(_cleanUpTimer);
				}
			}
		}

		/// <summary>
		/// Returns total count, including expired items too, if they were not yet cleaned by the eviction job
		/// </summary>
		public int Count => _dict.Count;

		/// <summary>
		/// Removes all items from the cache
		/// </summary>
		public void Clear() => _dict.Clear();

		/// <summary>
		/// Adds an item to cache if it does not exist, updates the existing item otherwise. Updating an item resets its TTL.
		/// </summary>
		/// <param name="key">The key to add</param>
		/// <param name="value">The value to add</param>
		public void AddOrUpdate(TKey key, TValue value) {
			var ttlValue = new TtlValue(value, _timeToLive);

			_dict.AddOrUpdate(key, (k, c) => c, (k, v, c) => c, ttlValue);
		}

		/// <summary>
		/// Attempts to get a value by key
		/// </summary>
		/// <param name="key">The key to get</param>
		/// <param name="value">When method returns, contains the object with the key if found, otherwise default value of the type</param>
		/// <returns>True if value exists, otherwise false</returns>
		public bool TryGet(TKey key, out TValue value) {
			value = default;

			if (!_dict.TryGetValue(key, out var ttlValue)) return false; //not found

			if (ttlValue.IsExpired()) //found but expired
			{
				var kv = new KeyValuePair<TKey, TtlValue>(key, ttlValue);

				//secret atomic removal method (only if both key and value match condition
				//https://devblogs.microsoft.com/pfxteam/little-known-gems-atomic-conditional-removals-from-concurrentdictionary/
				//so that we don't need any locks!! woohoo
				_dict.TryRemove(kv.Key, out _);

				/* EXPLANATION:
				 * when an item was "found but is expired" - we need to treat as "not found" and discard it.
				 * One solution is to use a lock
				 * so that the three steps "exist? expired? remove!" are performed atomically.
				 * Otherwise another tread might chip in, and ADD a non-expired item with the same key while we're evicting it.
				 * And we'll be removing a non-expired key that was just added.
				 * 
				 * BUT instead of using locks we can remove by key AND value. So if another thread has just rushed in 
				 * and added another item with the same key - that other item won't be removed.
				 * 
				 * basically, instead of doing this
				 * 
				 * lock {
				 *		exists?
				 *		expired?
				 *		remove by key!
				 * }
				 * 
				 * we do this
				 * 
				 * exists? (if yes returns the value)
				 * expired?
				 * remove by key AND value
				 * 
				 * If another thread has modified the value - it won't remove it.
				 * 
				 * Locks suck becasue add extra 50ns to benchmark, so it becomes 110ns instead of 70ns which sucks.
				 * So - no locks then!!!
				 * 
				 * */

				return false;
			}

			value = ttlValue.Value;
			return true;
		}

		/// <summary>
		/// Attempts to add a key/value item
		/// </summary>
		/// <param name="key">The key to add</param>
		/// <param name="value">The value to add</param>
		/// <returns>True if value was added, otherwise false (already exists)</returns>
		public bool TryAdd(TKey key, TValue value) {
			if (TryGet(key, out _)) return false;

			return _dict.TryAdd(key, new TtlValue(value, _timeToLive));
		}

		/// <summary>
		/// Adds a key/value pair by using the specified function if the key does not already exist, or returns the existing value if the key exists.
		/// </summary>
		/// <param name="key">The key to add</param>
		/// <param name="valueFactory">The factory function used to generate the item for the key</param>
		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
			if (TryGet(key, out var value)) return value;

			return _dict.GetOrAdd(key, (k, v) => new TtlValue(v.valueFactory(k), _timeToLive), (_timeToLive, valueFactory)).Value;
		}

		/// <summary>
		/// Adds a key/value pair by using the specified function if the key does not already exist, or returns the existing value if the key exists.
		/// </summary>
		/// <param name="key">The key to add</param>
		/// <param name="value">The value to add</param>
		public TValue GetOrAdd(TKey key, TValue value) {
			if (TryGet(key, out var existingValue)) return existingValue;

			return _dict.GetOrAdd(key, new TtlValue(value, _timeToLive)).Value;
		}

		/// <summary>
		/// Tries to remove item with the specified key
		/// </summary>
		/// <param name="key">The key of the element to remove</param>
		public void Remove(TKey key) {
			_dict.TryRemove(key, out _);
		}

		/// <summary>
		/// Tries to remove item with the specified key, also returns the object removed in an "out" var
		/// </summary>
		/// <param name="key">The key of the element to remove</param>
		/// <param name="value">Contains the object removed or the default value if not found</param>
		public bool TryRemove(TKey key, out TValue value) {
			var res = _dict.TryRemove(key, out var ttlValue) && !ttlValue.IsExpired();
			value = res ? ttlValue.Value : default(TValue);
			return res;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			foreach (var kvp in _dict) {
				if (!kvp.Value.IsExpired()) yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		private class TtlValue {
			public readonly TValue Value;
			public readonly long TickCountWhenToKill;

			public TtlValue(TValue value, TimeSpan ttl) {
				Value = value;
				TickCountWhenToKill = Environment.TickCount + (long)ttl.TotalMilliseconds;
			}

			public bool IsExpired() {
				return Environment.TickCount > TickCountWhenToKill;
			}
		}

		//IDispisable members
		private bool _disposedValue;
		public void Dispose() => Dispose(true);

		private void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					_cleanUpTimer.Dispose();
				}

				_disposedValue = true;
			}
		}
	}

}