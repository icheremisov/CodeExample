using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Core.Common.Contracts {

	public interface ILoadingScreen : IProgress<float> {

		bool IsVisible { get; }
		bool IsBarVisible { get; }
		UniTask ShowAsync(bool force = false, CancellationToken ct = default);
		UniTask HideAsync(bool force = false, CancellationToken ct = default);
		void SetContentVisible(bool v);

	}

}