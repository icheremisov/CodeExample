#if UNITY_IOS

using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Advertisement.IosSupport;

namespace Client.App.Internal {

	internal partial class UnityApplication {
		
		private UniTask HandleATTracking(CancellationToken ct) {
			
			if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
				ATTrackingStatusBinding.RequestAuthorizationTracking();
			}			

			return UniTask.CompletedTask;
		}
	}

}

#endif