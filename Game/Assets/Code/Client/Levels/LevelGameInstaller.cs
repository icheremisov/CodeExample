using System.Linq;
using Client.Ecs.Core;
using Client.Ecs.Core.Utils;
using Client.Entitas.Components;
using Client.Levels.Contracts;
using Client.Levels.Internal;
using Client.Levels.View.Factory;
using Entitas;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;
using XLib.Unity.Utils;

namespace Client.Levels
{
    [InstallerContainer(InstallerContainer.Level)]
    public class LevelGameInstaller : SceneInstaller<LevelGameInstaller> {
        [SerializeField, Required] private LevelSceneTransforms _levelSceneTransforms;
        [SerializeField, Required, ViewReference]
        private Camera _mainBattleCamera;
        [SerializeField, Required, ViewReference]
        private Camera _bgBattleCamera;

        protected override void OnInstallBindings() {
            var levelContext = Container.Resolve<LevelContext>();
            var globalContext = Container.Resolve<GlobalContext>();
            Debug.Assert(levelContext != null);
            Debug.Assert(globalContext != null);

            Container.BindInstance(_levelSceneTransforms);
            
            Container.BindInterfacesAndSelfTo<PlayLevelState>().AsSingle();
            Container.RegisterEcsFeature(FeaturesEcs.Feature.Level);
            
            Container.BindInterfacesTo<ViewFactory>().AsSingle();
            Container.BindInterfacesTo<AssetPreloader>().AsSingle();

        }

        protected override void OnInitialize() {
            var levelContext = Container.Resolve<LevelContext>();
            levelContext.SubscribeId();

            // var animationContext = Container.Resolve<AnimationContext>();
            // animationContext.ReplaceCamera(_mainBattleCamera);
			
            // foreach (var tweenAnimation in Container.ResolveAll<ICameraPersistentAnimation>()) {
            //     tweenAnimation.SetCamera(ICameraPersistentAnimation.CameraType.Main, _mainBattleCamera);
            //     tweenAnimation.SetCamera(ICameraPersistentAnimation.CameraType.Background, _bgBattleCamera);
            // }
            //
            InitializeObservers();
        }

        protected override void OnDispose() {
            Container.ResolveAll<IContext>().Where(context => context is not GlobalContext).ForEach(x => x.Reset());
			
            ShutdownObservers();
        }

#if UNITY_EDITOR && !ENTITAS_DISABLE_VISUAL_DEBUGGING
        private EcsObserver[] _contextObservers;

        private void InitializeObservers() {
            _contextObservers ??= Container.ResolveAll<IContext>().Where(x => x is not GlobalContext).SelectToArray(x => new EcsObserver(x));
        }

        private void ShutdownObservers() {
            _contextObservers?.ForEach(x => x.Shutdown());
            _contextObservers = null;
        }
#else
		public void InitializeObservers()
		{
			
		}

		public void ShutdownObservers()
		{
			
		}
#endif
    }
}