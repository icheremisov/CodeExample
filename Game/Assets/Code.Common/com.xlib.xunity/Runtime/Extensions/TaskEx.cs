using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[SuppressMessage("ReSharper", "CheckNamespace")]
public static class TaskEx {
	public static Task DelaySec(float sec, CancellationToken ct = default) {
		if (!(sec <= 0)) return Task.Delay((int)(sec * 1000), ct);
		ct.ThrowIfCancellationRequested();
		return Task.CompletedTask;

	}

	public static async void Done(this Task task) {
		try {
			await task;
		}
		catch (OperationCanceledException) { }
		catch (Exception e) {
			Debug.LogError(e);
		}
	}

	[SuppressMessage("ReSharper", "VariableHidesOuterVariable", Justification = "Pass params explicitly to async local function or it will allocate to pass them")]
	public static void Forget(this Tween tween, [CallerMemberName] string callingMethodName = "") {
		if (tween == null) throw new ArgumentNullException(nameof(tween));
		if (!tween.IsActive()) return;

		static async UniTask ForgetAwaited(UniTask task, string callingMethodName = "") {
			try {
				await task;
			}
			catch (TaskCanceledException) { }
			catch (Exception e) {
				Debug.LogError($"Fire and forget task failed for calling method '{callingMethodName}': {e.Message}\n{e.StackTrace}");
			}
		}

		var task = tween.ToUniTask();
		if (task.Status == UniTaskStatus.Pending) _ = ForgetAwaited(task, callingMethodName);
	}
}