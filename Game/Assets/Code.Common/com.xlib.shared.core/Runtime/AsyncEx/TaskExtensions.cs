using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using XLib.Core.AsyncEx;

/// <summary>
///     Provides extension methods for the <see cref="Task" /> and <see cref="Task{T}" /> types.
/// </summary>
// ReSharper disable once CheckNamespace
public static class TaskExtensions {

	[SuppressMessage("ReSharper", "VariableHidesOuterVariable", Justification = "Pass params explicitly to async local function or it will allocate to pass them")]
	public static void Forget(this Task task, Action<Exception> exceptionHandler = null, [CallerMemberName] string callingMethodName = "") {
		if (task == null) throw new ArgumentNullException(nameof(task));

		// Allocate the async/await state machine only when needed for performance reasons.
		// More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/?WT.mc_id=DT-MVP-5003978
		// Pass params explicitly to async local function or it will allocate to pass them
		static async Task ForgetAwaited(Task task, Action<Exception> exceptionHandler = null, string callingMethodName = "") {
			try {
				await task;
			}
			catch (OperationCanceledException) { }
			catch (Exception e) {
				if (exceptionHandler == null) {
					// log a message if we were given a logger to use
					Debug.LogError($"Fire and forget task failed for calling method '{callingMethodName}': {e.Message}\n{e.StackTrace}");

					var ex = e.InnerException;

					while (ex != null) {
						Debug.LogError($"Inner exception: Fire and forget task failed for calling method '{callingMethodName}': {ex.Message}\n{ex.StackTrace}");
						ex = ex.InnerException;
					}
				}
				else {
					exceptionHandler(e);
				}
			}
		}

		// note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
		// Only care about tasks that may fault (not completed) or are faulted,
		// so fast-path for SuccessfullyCompleted and Canceled tasks.
		if (!task.IsCanceled && (!task.IsCompleted || task.IsFaulted)) {
			// use "_" (Discard operation) to remove the warning IDE0058: Because this call is not awaited, execution of the
			// current method continues before the call is completed - https://docs.microsoft.com/en-us/dotnet/csharp/discards#a-standalone-discard
			_ = ForgetAwaited(task, exceptionHandler, callingMethodName);
		}
	}

	public static IEnumerator AsIEnumerator(this Task task) {
		while (!task.IsCompleted) yield return null;

		if (task.IsFaulted) throw task.Exception ?? new Exception("Task faulted");
	}

	public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
		where T : class {
		while (!task.IsCompleted) yield return null;

		if (task.IsFaulted) throw task.Exception ?? new Exception("Task faulted");

		yield return task.Result;
	}

	/// <summary>
	///     Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
	/// </summary>
	/// <param name="this">The task to wait for. May not be <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
	public static Task WaitAsync(this Task @this, CancellationToken cancellationToken) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		if (!cancellationToken.CanBeCanceled) return @this;
		if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
		return DoWaitAsync(@this, cancellationToken);
	}

	private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken) {
		using (var cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken)) await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
	}

	/// <summary>
	///     Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
	/// </summary>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
	public static Task<Task> WhenAny(this IEnumerable<Task> @this, CancellationToken cancellationToken) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAny(@this).WaitAsync(cancellationToken);
	}

	/// <summary>
	///     Asynchronously waits for any of the source tasks to complete.
	/// </summary>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	public static Task<Task> WhenAny(this IEnumerable<Task> @this) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAny(@this);
	}

	/// <summary>
	///     Asynchronously waits for all of the source tasks to complete.
	/// </summary>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	public static Task WhenAll(this IEnumerable<Task> @this) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAll(@this);
	}

	/// <summary>
	///     Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
	/// </summary>
	/// <typeparam name="TResult">The type of the task result.</typeparam>
	/// <param name="this">The task to wait for. May not be <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
	public static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		if (!cancellationToken.CanBeCanceled) return @this;
		if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
		return DoWaitAsync(@this, cancellationToken);
	}

	private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken) {
		using (var cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
			return await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
	}

	/// <summary>
	///     Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
	/// </summary>
	/// <typeparam name="TResult">The type of the task results.</typeparam>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
	public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this, CancellationToken cancellationToken) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAny(@this).WaitAsync(cancellationToken);
	}

	/// <summary>
	///     Asynchronously waits for any of the source tasks to complete.
	/// </summary>
	/// <typeparam name="TResult">The type of the task results.</typeparam>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAny(@this);
	}

	/// <summary>
	///     Asynchronously waits for all of the source tasks to complete.
	/// </summary>
	/// <typeparam name="TResult">The type of the task results.</typeparam>
	/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
	public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> @this) {
		if (@this == null) throw new ArgumentNullException(nameof(@this));

		return Task.WhenAll(@this);
	}

	public static CancellationTokenSource WithTimeout(this CancellationTokenSource cts, int ms) {
		cts.CancelAfter(ms);
		return cts;
	}

	public static CancellationTokenSource WithTimeoutSec(this CancellationTokenSource cts, float seconds) {
		cts.CancelAfter((int)(seconds * 1000));
		return cts;
	}

}