using System.Threading;
using Client.Core.Common.Internal;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Crashlytics;
using UnityEngine;
using XLib.Unity.Core;
using XLib.Unity.Installers;

namespace Client.Core.Crashlitycs {

	public class FirebaseProjectContextInstaller : ProjectContextInstaller<FirebaseProjectContextInstaller>, IAsyncInitializable {

		protected override void OnInstallBindings() {}

		protected override void OnInitialize() {
			Application.logMessageReceivedThreaded += FirebaseWrapper.OnLogCallback;
			AppEventsListener.ApplicationPauseStatic += FirebaseWrapper.OnApplicationPause;
		}

		protected override void OnDispose() {
			Application.logMessageReceivedThreaded -= FirebaseWrapper.OnLogCallback;
			AppEventsListener.ApplicationPauseStatic -= FirebaseWrapper.OnApplicationPause;
		}
		
		public async UniTask InitializeAsync(CancellationToken ct) {
			Debug.Log("[FirebaseRootInstaller] Initialize");
			await FirebaseWrapper.Init();
		}
	}

}