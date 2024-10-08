using System;
using System.Threading;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Cysharp.Threading.Tasks
{
	public static class UniEx
	{
		/// <summary>
		/// delay specific amount of seconds 
		/// </summary>
		public static UniTask DelaySec(float secondsDelay, CancellationToken cancellationToken = default)
		{
			var delayTimeSpan = TimeSpan.FromSeconds(secondsDelay);
			return UniTask.Delay(delayTimeSpan, true, PlayerLoopTiming.Update, cancellationToken);
		}

		/// <summary>
		/// delay specific amount of seconds 
		/// </summary>
		public static UniTask DelaySec(float secondsDelay, bool ignoreTimeScale, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default)
		{
			var delayTimeSpan = TimeSpan.FromSeconds(secondsDelay);
			return UniTask.Delay(delayTimeSpan, ignoreTimeScale, delayTiming, cancellationToken);
		}

		/// <summary>
		/// delay specific amount of seconds 
		/// </summary>
		public static UniTask DelaySec(float secondsDelay, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default)
		{
			var delayTimeSpan = TimeSpan.FromSeconds(secondsDelay);
			return UniTask.Delay(delayTimeSpan, delayType, delayTiming, cancellationToken);
		}
	}

	public static partial class UniTaskExtensions
	{
		/// <summary>
		/// execute onFinally always after task completed (success or failed)
		/// </summary>
		public static async UniTask Finally(this UniTask task, Action onFinally)
		{
			try
			{
				await task;
			}
			finally
			{
				onFinally();
			}
		}

		/// <summary>
		/// execute onFinally always after task completed (success or failed)
		/// </summary>
		public static async UniTask<T> Finally<T>(this UniTask<T> task, Action onFinally)
		{
			try
			{
				return await task;
			}
			finally
			{
				onFinally();
			}
		}

		/// <summary>
		/// execute onError in case of exception and continue chain
		/// </summary>
		public static async UniTask Catch<T>(this UniTask<T> task, Action<Exception> onError)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				onError(ex);
			}
		}

		/// <summary>
		/// execute onError in case of exception and continue chain
		/// </summary>
		public static async UniTask Catch<T, TException>(this UniTask<T> task, Action<TException> onError)
			where TException : Exception
		{
			try
			{
				await task;
			}
			catch (TException ex)
			{
				onError(ex);
			}
		}

		/// <summary>
		/// execute onError in case of exception and continue chain
		/// </summary>
		public static async UniTask Catch(this UniTask task, Action<Exception> onError)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				onError(ex);
			}
		}

		/// <summary>
		/// execute onError in case of exception and continue chain
		/// </summary>
		public static async UniTask Catch<T>(this UniTask task, Action<T> onError)
			where T : Exception
		{
			try
			{
				await task;
			}
			catch (T ex)
			{
				onError(ex);
			}
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void Forget(this UniTask task, Action completeHandler)
		{
			task.ContinueWith(completeHandler).Forget();
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void OnComplete(this UniTask task, Action completeHandler)
		{
			task.ContinueWith(completeHandler).Forget();
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void OnComplete<T>(this UniTask<T> task, Action<T> completeHandler)
		{
			task.ContinueWith(completeHandler).Forget();
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void Forget(this UniTask task, Action completeHandler, Action<Exception> exceptionHandler,
			bool handleExceptionOnMainThread = true)
		{
			task.ContinueWith(completeHandler).Forget(exceptionHandler, handleExceptionOnMainThread);
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void Forget<T>(this UniTask<T> task, Action<T> completeHandler)
		{
			task.ContinueWith(completeHandler).Forget();
		}

		/// <summary>
		/// finalize chain and catch all exceptions. execute completeHandler in case of completed task
		/// </summary>
		public static void Forget<T>(this UniTask<T> task, Action<T> completeHandler, Action<Exception> exceptionHandler,
			bool handleExceptionOnMainThread = true)
		{
			task.ContinueWith(completeHandler).Forget(exceptionHandler, handleExceptionOnMainThread);
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask Log(this UniTask t, object message)
		{
			return t.ContinueWith(() => Debug.Log(message));
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask LogWarning(this UniTask t, object message)
		{
			return t.ContinueWith(() => Debug.LogWarning(message));
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask LogError(this UniTask t, object message)
		{
			return t.ContinueWith(() => Debug.LogError(message));
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask<T> Log<T>(this UniTask<T> t, object message)
		{
			return t.ContinueWith(x =>
			{
				Debug.Log(message);
				return x;
			});
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask<T> LogWarning<T>(this UniTask<T> t, object message)
		{
			return t.ContinueWith(x =>
			{
				Debug.LogWarning(message);
				return x;
			});
		}

		/// <summary>
		/// output message to Unity log after task completed and continue
		/// </summary>
		public static UniTask<T> LogError<T>(this UniTask<T> t, object message)
		{
			return t.ContinueWith(x =>
			{
				Debug.LogError(message);
				return x;
			});
		}
	}
}