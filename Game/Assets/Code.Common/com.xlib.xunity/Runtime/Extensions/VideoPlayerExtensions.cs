using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using XLib.Core.AsyncEx;

// ReSharper disable once CheckNamespace
public static class VideoPlayerExtensions {

	public static async UniTask WaitForEndAsync(this VideoPlayer videoPlayer, CancellationToken ct = default) {
		var ev = new AsyncEvent();

		videoPlayer.loopPointReached += Completed;
		videoPlayer.errorReceived += Error;

		void Completed(VideoPlayer source) {
			videoPlayer.loopPointReached -= Completed;
			videoPlayer.errorReceived -= Error;
			ev.FireEvent();
		}

		void Error(VideoPlayer source, string message) {
			Debug.LogError($"[VideoPlayer] {message}");
			videoPlayer.loopPointReached -= Completed;
			videoPlayer.errorReceived -= Error;
			ev.FireEvent();
		}

		try {
			await ev.WaitAsync(ct);
		}
		catch (OperationCanceledException) {
			videoPlayer.Stop();
			videoPlayer.loopPointReached -= Completed;
			videoPlayer.errorReceived -= Error;
			throw;
		}
	}

}