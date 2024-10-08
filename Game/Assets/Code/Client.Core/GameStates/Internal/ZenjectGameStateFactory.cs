using System;
using System.Collections.Generic;
using System.Linq;
using Client.Core.GameStates.Attributes;
using Client.Core.GameStates.Contracts;
using XLib.Core.Reflection;
using XLib.Core.Utils;
using XLib.States.Contracts;
using XLib.Unity.Core;
using Zenject;

namespace Client.Core.GameStates.Internal {

	internal class ZenjectGameStateFactory : IStateFactory<IGameState>, IContainerListener {

		private readonly List<DiContainer> _containers = new();

		public void OnInstall(DiContainer container) {
			_containers.AddOnce(container);
		}

		public void OnUninstall(DiContainer container) {
			_containers.Remove(container);
		}

		public IGameState Create(Type stateType) {
			return _containers
				.Select(diContainer => (IGameState)diContainer.TryResolve(stateType))
				.FirstOrDefault(state => state != null);
		}

		public static IGameState FirstOrDefault<TAttribute>(DiContainer container)
			where TAttribute : Attribute {
			return container.ResolveAll<IGameState>().FirstOrDefault(x => x.GetType().HasAttribute<TAttribute>());
		}

		public static void RegisterRootStates(DiContainer container) {
			//Debug.Log(EnumerateTypes().Select(x => x.FullName).JoinToString());

			foreach (var type in EnumerateTypes()) container.BindInterfacesAndSelfTo(type).AsSingle();
		}

		private static IEnumerable<Type> EnumerateTypes() {
			var baseType = TypeOf<IGameState>.Raw;
			return TypeUtils.EnumerateAll(x =>
				x.IsClass && !x.IsAbstract && baseType.IsAssignableFrom(x) && (x.HasAttribute<EntryPointAttribute>() || x.HasAttribute<BindToRootInstallerAttribute>()));
		}

	}

}