using System.Linq;
using Client.Ecs.Core.Contracts;
using Client.Ecs.Core.Internal;
using Client.Ecs.Core.Utils;
using Entitas;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.Ecs.Core {

	[InstallerContainer(InstallerContainer.Main)]
	public class EcsInstaller : SceneInstaller<EcsInstaller> {
		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<EcsRunner>().AsSingle();

			var contexts = Contexts.sharedInstance;
			Container.BindInterfacesAndSelfTo<Contexts>().FromInstance(contexts);
			Container.BindInterfacesAndSelfTo<GlobalContext>().FromMethod(_ => contexts.global);
			Container.BindInterfacesAndSelfTo<LevelContext>().FromMethod(_ => contexts.level);
			
			Container.RegisterEcsFeature(FeaturesEcs.Feature.Global);
		}

		protected override void OnInitialize() {
			InitializeObservers();
		}

		protected override void OnDispose() {
			var ecsRunner = Container.Resolve<IEcsRunner>();
			ecsRunner.Stop(FeaturesEcs.Feature.Global);
			Container.ResolveAll<IContext>().Where(context => context is GlobalContext).ForEach(x => x.Reset());
			ecsRunner.Start(FeaturesEcs.Feature.Global);
			ShutdownObservers();
		}

#if UNITY_EDITOR && !ENTITAS_DISABLE_VISUAL_DEBUGGING
		private EcsObserver _globalContextObserver;

		private void InitializeObservers() {
			_globalContextObserver ??= new EcsObserver(Container.ResolveAll<IContext>().FirstOrDefault(x => x is GlobalContext));
		}

		private void ShutdownObservers() {
			_globalContextObserver?.Shutdown();
			_globalContextObserver = null;
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