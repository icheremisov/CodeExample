#if !UNITY_WEBGL

using System;
using System.Threading;
using System.Threading.Tasks;

namespace XLib.Core.AsyncEx {

	public class AsyncEvent {
		private TaskCompletionSource<bool> _tcs = new();

		public void Reset() {
			var oldTcs = _tcs;
			oldTcs?.TrySetCanceled();

			_tcs = new TaskCompletionSource<bool>();
		}

		public void FireEvent() {
			_tcs.TrySetResult(true);
		}

		public void FireException(Exception ex) {
			_tcs.TrySetException(ex);
		}

		public Task WaitAsync() {
			return _tcs.Task;
		}

		public Task WaitAsync(CancellationToken ct) {
			return _tcs.Task.WaitAsync(ct);
		}
	}

}

#endif