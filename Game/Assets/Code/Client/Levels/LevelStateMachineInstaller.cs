using Client.Levels.Internal;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.Levels.States {

	[InstallerContainer(InstallerContainer.Level)]
	public class LevelStateMachineInstaller : SceneInstaller<LevelStateMachineInstaller> {
		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<LevelStateController>().AsSingle();

			InstallBattleStates();
		}

		private void InstallBattleStates() {
			Container.BindInterfacesTo<EndLevelState>().AsSingle();
			Container.BindInterfacesTo<ExitLevelState>().AsSingle();
			Container.BindInterfacesTo<ActiveLevelState>().AsSingle();
			Container.BindInterfacesTo<StartLevelState>().AsSingle();
			Container.BindInterfacesTo<NoneLevelState>().AsSingle();

			// Container.BindInterfacesTo<PreviewAnimationBattleState>().AsSingle();
			// Container.BindInterfacesTo<PreviewBattleState>().AsSingle();
			// Container.BindInterfacesTo<PreviewSendBattleState>().AsSingle();
			// Container.BindInterfacesTo<PreviewStartBattleState>().AsSingle();

			// Container.BindInterfacesTo<TurnAnimationBattleState>().AsSingle();
			// Container.BindInterfacesTo<TurnBattleState>().AsSingle();
			// Container.BindInterfacesTo<TurnEndBattleState>().AsSingle();
			// Container.BindInterfacesTo<TurnSendBattleState>().AsSingle();
			// Container.BindInterfacesTo<TutorialPauseBattleState>().AsSingle();
			// Container.BindInterfacesTo<TurnStartBattleState>().AsSingle();
		}
		
		protected override void OnInitialize() {}

		protected override void OnDispose() {}
	}

}