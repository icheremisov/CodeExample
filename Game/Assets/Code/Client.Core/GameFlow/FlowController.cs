using Client.Core.Common.Contracts;
using Client.Core.GameFlow.Contracts;
using Client.Core.GameStates.Contracts;
using XLib.UI.Contracts;
using XLib.Unity.Installers;
using Zenject;

namespace Client.Core.GameFlow {

	public abstract class FlowController {
		
		[Inject] private ISceneLoader _sceneLoader;
		[Inject] private IGameStateMachine _gameStateMachine;
		[Inject] private ILoadingScreen _loadingScreen;
		[Inject] private IScreenManager _screenManager;
		
		[Inject] private LazyBinding<ILevelFlowController> _levelFlowController;
		[Inject] private LazyBinding<IMetaFlowController> _metaFlowController;
		[Inject] private LazyBinding<ILevelEditorFlowController> _levelEditorFlowController;
		
		protected ISceneLoader SceneLoader => _sceneLoader;
		protected IGameStateMachine GameStateMachine => _gameStateMachine;
		protected ILoadingScreen LoadingScreen => _loadingScreen;
		protected IScreenManager ScreenManager => _screenManager;

		protected ILevelFlowController LevelFlowController => _levelFlowController.Value;
		protected IMetaFlowController MetaFlowController => _metaFlowController.Value;
		protected ILevelEditorFlowController LevelEditorFlowController => _levelEditorFlowController.Value;
	}

}