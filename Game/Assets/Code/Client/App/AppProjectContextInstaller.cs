using Client.App.Internal;
using XLib.Unity.Installers;

namespace Client.App {

	public class AppProjectContextInstaller : ProjectContextInstaller<AppProjectContextInstaller> {

		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<ApplicationLoader>().AsSingle();
		}

		protected override void OnInitialize() { }

		protected override void OnDispose() { }
	}

}