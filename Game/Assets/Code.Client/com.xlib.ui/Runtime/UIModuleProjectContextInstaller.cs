using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Assets.Contracts;
using XLib.UI.Animation.Configs;
using XLib.UI.Config;
using XLib.UI.ConnectionBlocker;
using XLib.UI.Contracts;
using XLib.UI.DragDrop.Internal;
using XLib.UI.Internal;
using XLib.UI.Views;
using XLib.Unity.Core;
using XLib.Unity.Installers;
using XLib.Unity.Utils;

namespace XLib.UI {

	public class UIModuleProjectContextInstaller : ProjectContextInstaller<UIModuleProjectContextInstaller>, IAsyncInitializable {

		[Header("Views"), SerializeField, Required, ViewReference]
		private UIScreenLockView _screenLockView;

		[Header("Views"), SerializeField, Required, AssetsOnly]
		private BlockerView _screenBlockerPrefab;

		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<ScreenLocker>().AsSingle().NonLazy();
			Container.BindInterfacesAndSelfTo<UIScreenLockView>().FromInstance(_screenLockView).WhenInjectedInto<ScreenLocker>();
			Container.BindInterfacesTo<DragDropRoot>().FromMethod(_ => DragDropRoot.S);
			
			Container.BindInterfacesTo<ScreenManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<UIScreenLoader>().AsSingle();
			
			Container.BindInterfacesTo<BlockerView>().FromComponentInNewPrefab(_screenBlockerPrefab).AsSingle().NonLazy();
			
		}

		protected override void OnInitialize() {
			Container.InitLazyBindingsTo<IScreenLocker>();
		}

		protected override void OnDispose() {
			Container.ClearLazyBindingsTo<IScreenLocker>();
		}
		
		public async UniTask InitializeAsync(CancellationToken ct) {
			var assetProvider = Container.Resolve<IAssetProvider>();
			
			var uiGlobalAnimations = await assetProvider.LoadByKeyAsync<UIGlobalAnimations>(UIGlobalAnimations.AssetName);
			uiGlobalAnimations.Initialize();
			
			UIGlobals.S = await assetProvider.LoadByKeyAsync<UIGlobals>(UIGlobals.AssetName);
			UIGlobals.S.ApplyUIScale();
		}
	}

}