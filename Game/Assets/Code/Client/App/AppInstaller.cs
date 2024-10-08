using System;
using Client.App.Contracts;
using Client.App.Internal;
using Client.Core.Common.Contracts;
using Client.Core.Common.DI;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Storage;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.App {

	[InstallerContainer(InstallerContainer.Main)]
	public class AppInstaller : SceneInstaller<AppInstaller> {

		// private ClientGameApplication _gameApplication;

		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<UnityGameDatabaseProvider>().AsSingle();
			Container.BindInterfacesTo<AddressableStorageProvider>().AsSingle().WithArguments("default");
			Container.BindInterfacesTo<UnityApplication>().AsSingle();
			Container.Bind<IGameDatabase>()
				.FromMethod(ctx => {
					try {
						return Container.Resolve<IClientGameDatabaseProvider>().Get();
					}
					catch (Exception e) {
						Debug.LogError($"Resolving {ctx.ObjectType}: {e.Message}");
						throw;
					}
				});

			// _gameApplication = new ClientGameApplication(new ZenjectContainerBuilder(new ZenjectContainer(Container)));

			InstallLoginBindings();
		}

		private void InstallLoginBindings() {
			// Container.BindInterfacesTo<TermsController>().AsSingle();
		}

		protected override void OnInitialize() {
			Container.InitLazyBindingsTo<IUnityApplication>();
		}

		protected override void OnDispose() {
			Container.ClearLazyBindingsTo<IUnityApplication>();
		}
	}

}