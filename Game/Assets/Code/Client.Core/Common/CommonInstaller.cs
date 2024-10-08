using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.Core.Common {

	[InstallerContainer(InstallerContainer.Main)]
	public class CommonInstaller : SceneInstaller<CommonInstaller> {
		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<SystemDialog>().AsSingle();
		}

		protected override void OnInitialize() {
			Container.InitLazyBindingsTo<ISystemDialog>();
		}

		protected override void OnDispose() {
			Container.ClearLazyBindingsTo<ISystemDialog>();
			
		}
	}

}