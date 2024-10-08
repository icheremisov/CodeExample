using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace XLib.Core.AsyncEx {

	public interface IAwaitable : INotifyCompletion {
		bool IsCompleted { get; }
		void GetResult();
		IAwaitable GetAwaiter(); // should probably return this
		void Cancel(Action continuation);
	}

	public interface IAwaitable<out T> : INotifyCompletion {
		bool IsCompleted { get; }
		T GetResult();
		IAwaitable<T> GetAwaiter(); // should probably return this
		void Cancel(Action continuation);
	}

	public abstract class Awaitable : IAwaitable {
		public static readonly NeverAwaitable Never = new();
		public static readonly AlwaysAwaitable Always = new();
		public abstract bool IsCompleted { get; }
		public virtual void GetResult() { }
		public abstract void OnCompleted(Action continuation);
		public virtual IAwaitable GetAwaiter() => this;
		public abstract void Cancel(Action continuation);
	}

	public abstract class Awaitable<T> : IAwaitable<T> {
		public abstract bool IsCompleted { get; }
		public abstract T GetResult();
		public abstract void OnCompleted(Action continuation);
		public IAwaitable<T> GetAwaiter() => this;
		public abstract void Cancel(Action continuation);
	}

	public class CallbackAwaitable : Awaitable {
		private readonly CancellationToken _ctx;
		private bool _completed;
		private Action _continuation;
		private Exception _completeException;
 
		public override bool IsCompleted => _completed || _ctx.IsCancellationRequested;

		public CallbackAwaitable(CancellationToken ctx) {
			_ctx = ctx;
			_ctx.Register(SetComplete);
		}

		public override void OnCompleted(Action continuation) => _continuation += continuation;
		public override void Cancel(Action continuation) => _continuation -= continuation;

		public void SetComplete() {
			_completed = true;
			var action = _continuation;
			_continuation = null;
			action?.Invoke();
		}

		public void Throw(Exception exception) => _completeException = exception;

		public override void GetResult() {
			if (_completeException != null) throw _completeException;
			_ctx.ThrowIfCancellationRequested();
			base.GetResult();
		}
	}

	public class CallbackAwaitable<T> : Awaitable<T> {
		private bool _completed;
		private T _result;
		private Action _continuation;

		public override bool IsCompleted => _completed;

		public void Callback(T value) {
			_result = value;
			_completed = true;
			_continuation?.Invoke();
		}

		public override void OnCompleted(Action continuation) => _continuation += continuation;

		public override T GetResult() => _result;

		public override void Cancel(Action continuation) => _continuation -= continuation;
	}

	public class NeverAwaitable : Awaitable {
		public override void OnCompleted(Action continuation) { }
		public override bool IsCompleted => false;
		public override void Cancel(Action continuation) { }
	}

	public class AlwaysAwaitable : Awaitable {
		public override void OnCompleted(Action continuation) => continuation();
		public override bool IsCompleted => true;
		public override void Cancel(Action continuation) { }
	}

}