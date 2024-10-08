using System;
using System.Collections;
using System.Collections.Generic;

namespace XLib.Core.Collections {

	public class PeekableEnumerator<T> : IEnumerator<T> {

		private readonly IEnumerator<T> _enumerator;

		private bool _didPeek;
		private T _peek;

		public PeekableEnumerator(IEnumerator<T> enumerator) {
			_enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
			TryFetchPeek();
		}

		public T Peek {
			get {
				TryFetchPeek();
				return _didPeek ? _peek : default;
			}
		}

#region IDisposable implementation

		public void Dispose() {
			_enumerator.Dispose();
		}

#endregion

#region IEnumerator implementation

		public T Current => _didPeek ? _peek : _enumerator.Current;

#endregion

		private void TryFetchPeek() {
			if (!_didPeek && (_didPeek = _enumerator.MoveNext())) _peek = _enumerator.Current;
		}

#region IEnumerator implementation

		public bool HasNextItem {
			get {
				if (!_didPeek) TryFetchPeek();

				return _didPeek;
			}
		}

		public bool MoveNext() {
			if (_didPeek) {
				_didPeek = false;
				return true;
			}

			return _enumerator.MoveNext();
		}

		public void Reset() {
			_enumerator.Reset();
			_didPeek = false;
		}

		object IEnumerator.Current => Current;

#endregion

	}

}