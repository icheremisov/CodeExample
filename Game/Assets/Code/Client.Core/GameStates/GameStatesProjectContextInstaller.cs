using Client.Core.GameStates.Internal;
using XLib.Unity.Core;
using XLib.Unity.Installers;

namespace Client.Core.GameStates {

	public class GameStatesProjectContextInstaller : ProjectContextInstaller<GameStatesProjectContextInstaller> {

		protected override void OnInstallBindings() {
			ZenjectGameStateFactory.RegisterRootStates(Container);

			Container.BindInterfacesTo<ZenjectGameStateFactory>()
				.AsSingle()
				.NonLazy();
			Container.BindInterfacesTo<GameStateMachine>().AsSingle();
		}

		protected override void OnInitialize() { }

		protected override void OnDispose() { }

	}

}