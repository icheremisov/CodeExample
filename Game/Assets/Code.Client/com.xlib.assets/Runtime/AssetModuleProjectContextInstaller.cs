using XLib.Assets.Internal;
using XLib.Unity.Core;
using XLib.Unity.Installers;

namespace XLib.Assets {

	public class AssetModuleProjectContextInstaller : ProjectContextInstaller<AssetModuleProjectContextInstaller> {

		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<AddressablesAssetProvider>().AsSingle();
			Container.BindInterfacesTo<ModelsCacheService>().AsSingle();
		}

		protected override void OnInitialize() { }

		protected override void OnDispose() { }

	}

}