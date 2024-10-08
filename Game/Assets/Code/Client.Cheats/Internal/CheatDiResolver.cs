using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Core.Utils;
using XLib.Unity.Core;
using Zenject;
using Component = System.ComponentModel.Component;
using Object = UnityEngine.Object;

namespace Client.Cheats.Internal {

	public class CheatDiResolver : IContainerListener {
		private readonly CheatSystem _cheatSystem;
		private readonly HashSet<DiContainer> _containers = new(8);
		private readonly object[] _singleNull = { null };
		private readonly object[] _empty = { };
		private readonly object[] _lockerCache;

		public CheatDiResolver(ILockable locker, CheatSystem cheatSystem) {
			_cheatSystem = cheatSystem;
			_lockerCache = new object[] { locker };
		}

		void IContainerListener.OnInstall(DiContainer container) {
			_containers.Add(container);
		}

		public void OnUninstall(DiContainer container) {
			_containers.Remove(container);
		}

		public T Resolve<T>() => (T)Resolve(TypeOf<T>.Raw);

		public object Resolve(Type type) {
			var args = _cheatSystem.TryResolve(type);
			if (args != null) return args;

			// special case for resolving hosts for more convenient usage in cheats
			// if (TypeOf<IHost>.IsAssignableFrom(type)) return Resolve<ISharedLogicService>()?.GetHost(type);

			return _containers
				.Select(diContainer => diContainer.TryResolve(type))
				.FirstOrDefault(result => result != null);
		}

		private IEnumerable<object> ResolveAllDi(Type type) {
			var hashSet = new HashSet<object>();
			foreach (var diContainer in _containers) {
				foreach (var o in diContainer.ResolveAll(type)) hashSet.Add(o);
			}

			return hashSet;
		}

		public object[] ResolveAll(Type type) {
			if (type == null) return _empty;
			try {
				if (TypeOf<ILockable>.IsAssignableFrom(type)) return _lockerCache;
				// if (TypeOf<IHost>.IsAssignableFrom(type)) {
					// var service = Resolve<ISharedLogicService>();
					// if (service == null) return _empty;
					// return new object[] { service.TryGetHost(type) };
				// }

				// if (TypeOf<IModule>.IsAssignableFrom(type)) {
					// var service = Resolve<ISharedLogicService>();
					// if (service == null) return _empty;
					// if (TypeOf<IModuleHost<MetaHost>>.IsAssignableFrom(type)) {
						// var module = service.TryGetHost<IMetaHost>()?.GetModule(type.Name);
						// return module == null ? _empty : new object[] { module };
					// }

					// if (TypeOf<IModuleHost<BattleHost>>.IsAssignableFrom(type)) {
						// var module = service.TryGetHost<IBattleHost>()?.GetModule(type.Name);
						// return module == null ? _empty : new object[] { module };
					// }
				// }

				// ReSharper disable once CoVariantArrayConversion
				if (TypeOf<Component>.IsAssignableFrom(type)) return Object.FindObjectsOfType(type, false);
				if (TypeOf<GameItemCore>.IsAssignableFrom(type))
					// ReSharper disable once CoVariantArrayConversion
					return (GameData.InstanceUnsafe?.All<GameItemCore>() ?? Enumerable.Empty<GameItemCore>()).Where(type.IsInstanceOfType).OrderByAlphaNumeric(core => core.FileName).ToArray();
				if (TypeOf<GameItemComponent>.IsAssignableFrom(type))
					// ReSharper disable once CoVariantArrayConversion
					return (GameData.InstanceUnsafe?.All<GameItemBaseContainer>() ?? Enumerable.Empty<GameItemBaseContainer>())
						.SelectMany(x => x.RawElements)
						.Where(type.IsInstanceOfType)
						.OrderBy(core => core.OrderByValue)
						.ToArray();
			
				// if (TypeOf<GlobalContext>.IsAssignableFrom(type)) return new object[] { Contexts.sharedInstance.global };
				// if (TypeOf<AnimationContext>.IsAssignableFrom(type)) return new object[] { Contexts.sharedInstance.animation };
				// if (TypeOf<BattleContext>.IsAssignableFrom(type)) {
				// 	var service = Resolve<ISharedLogicService>();
				// 	return service?.TryGetHost<IBattleHost>() == null ? Array.Empty<object>() : new object[] { Contexts.sharedInstance.battle };
				// }

				return new[] { Resolve(type) };
			}
			catch (Exception ex) {
				Debug.LogException(ex);
				return _empty;
			}
		}
	}

}