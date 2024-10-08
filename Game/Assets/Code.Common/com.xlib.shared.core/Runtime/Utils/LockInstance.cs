using System;
using System.Collections.Generic;

namespace XLib.Core.Utils {

	public interface ILockableInternal {
		void Unlock(LockInstance inst);

		void ThrowIfLocked() {
			if (IsLocked) throw new InvalidOperationException("The object is blocked");
		}

		bool IsLocked { get; }
	}

	public interface ILockable : ILockableInternal {
		LockInstance Lock();
	}
	public interface ILockable<in TArg> : ILockableInternal {
		LockInstance Lock(TArg arg);
	}

	public class LockInstance : IDisposable {
		public static readonly LockInstance Fake = new(null);

		private ILockableInternal _lockable;
		public LockInstance(ILockableInternal lockable) => _lockable = lockable;

		void IDisposable.Dispose() => Unlock();

		public void Unlock() {
			if (_lockable == null) return;
			_lockable.Unlock(this);
			_lockable = null;
		}
	}

	public class LockerCounter : ILockable {
		private int _locks;
		public bool IsLocked => _locks > 0;
		
		public LockInstance Lock() {
			_locks++;
			return new LockInstance(this);
		}

		void ILockableInternal.Unlock(LockInstance inst) {
			_locks--;
			if (_locks < 0) throw new InvalidOperationException();
		}
	}

	public class LockerCounter<T> : LockerCounter {
		public static LockerCounter<T> Default = new();
	}

	public class LockerStack : ILockable {
		private List<LockInstance> _locks = new();

		public void Unlock(LockInstance inst) {
			_locks.Remove(inst);
		}

		public bool IsLocked => _locks.Count > 0;

		public LockInstance Lock() {
			var @lock = new LockInstance(this);
			_locks.Add(@lock);
			return @lock;
		}
	}
	
}