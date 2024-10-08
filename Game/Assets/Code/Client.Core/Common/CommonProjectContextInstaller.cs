using Client.Core.Common.Internal;
using Client.Core.Common.UI.LoadingScreen;
using Client.Core.Common.UI.SystemNotification;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.TransitionScreen;
using XLib.Unity.Installers;

namespace Client.Core.Common {

	public class CommonProjectContextInstaller : ProjectContextInstaller<CommonProjectContextInstaller> {
		[SerializeField, Required, AssetsOnly] private LoadingView _loadingViewPrefab;
		[SerializeField, Required, AssetsOnly] private TransitionView _transitionViewPrefab;
		[SerializeField, Required, AssetsOnly] private SystemNotificationView _systemNotificationViewPrefab;

		protected override void OnInstallBindings() {
			Container.Bind(typeof(LazyBinding<>)).AsSingle();
			Container.BindInterfacesTo<SceneLoader>().AsSingle();
			Container.BindInterfacesTo<AppEventsListener>().FromInstance(AppEventsListener.Instantiate());
			
#if FEATURE_CONSOLE || FEATURE_CHEATS || UNITY_EDITOR
			Container.BindInterfacesTo<DebugLogCollector.Controls.DebugLogCollectorController>().AsSingle().NonLazy();
#endif
			// loading view
			Container.Bind<LoadingView>().FromComponentInNewPrefab(_loadingViewPrefab).AsSingle().NonLazy();
			Container.BindInterfacesTo<LoadingScreen>().AsSingle().NonLazy();
			
			// transition view
			Container.Bind<TransitionView>().FromComponentInNewPrefab(_transitionViewPrefab).AsSingle().NonLazy();
			Container.BindInterfacesTo<TransitionScreen>().AsSingle().NonLazy();
			
			//TODO: Uncomment this code to enable system notification
			// system notification view
			// Container.Bind<SystemNotificationView>().FromComponentInNewPrefab(_systemNotificationViewPrefab).AsSingle().NonLazy();
			// Container.BindInterfacesTo<SystemNotification>().AsSingle().NonLazy();
		}

		protected override void OnInitialize() {
			SystemEnvironment.Initialize();
		}

		protected override void OnDispose() { }
	}

}