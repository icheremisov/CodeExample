#if UNITY_WEBGL
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.Core.AsyncEx
{
	public class AsyncEvent
	{
		private bool _isCompleted;
		private Exception _exception; 
		
		public void Reset()
		{
			_isCompleted = false;
			_exception = null;
		}

		public void FireEvent()
		{
			_isCompleted = true;
			_exception = null;
		}

		public void FireException(Exception ex)
		{
			_isCompleted = false;
			_exception = ex;
		}

		public async UniTask WaitAsync(CancellationToken ct = default)
		{
			while (!_isCompleted)
			{
				ct.ThrowIfCancellationRequested();

				if (_exception != null)
				{
					var ex = _exception;
					_exception = null;
					throw ex;
				}

				await UniTask.NextFrame(ct);

			}
		}
		
	}
}

#endif