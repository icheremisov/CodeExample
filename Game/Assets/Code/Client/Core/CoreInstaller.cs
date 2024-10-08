using Client.Core.GameFlow.Contracts;
using Client.Levels;
using Client.Levels.Internal;
using Client.Meta;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.Core {

	[InstallerContainer(InstallerContainer.Main)]
	public class CoreInstaller : SceneInstaller<CoreInstaller> {
		
		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<LevelFlowController>().AsSingle();
			Container.BindInterfacesTo<MetaFlowController>().AsSingle();
			Container.BindInterfacesTo<LevelEditorFlowController>().AsSingle();
		}

		protected override void OnInitialize() {
			Container.InitLazyBindingsTo<ILevelFlowController>();
			Container.InitLazyBindingsTo<IMetaFlowController>();
			Container.InitLazyBindingsTo<ILevelEditorFlowController>();
		}

		protected override void OnDispose() {
			Container.ClearLazyBindingsTo<ILevelFlowController>();
			Container.ClearLazyBindingsTo<IMetaFlowController>();
			Container.ClearLazyBindingsTo<ILevelEditorFlowController>();
		}
	}

}