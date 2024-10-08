using System;
using XLib.Core.Utils;
using XLib.Unity.Core;
using Zenject;

namespace XLib.Unity.Installers {

	public abstract partial class BaseInstaller<TDerived> : MonoInstaller<TDerived>, IInitializable, IDisposable
		where TDerived : MonoInstaller<TDerived> {

		protected readonly Logger Logger = new(TypeOf<TDerived>.Name, RichLog.Color.darkgreen);

		public void Initialize() {
			Logger.Log("Initialize");

			Container.QueueAsyncInitializers();

			foreach (var containerListener in Container.ResolveAll<IContainerListener>()) containerListener.OnInstall(Container);

			OnInitialize();
		}

		public void Dispose() {
			Logger.Log("Dispose");

			foreach (var containerListener in Container.ResolveAll<IContainerListener>()) containerListener.OnUninstall(Container);

			OnDispose();
		}

		public override void InstallBindings() {
			Logger.Log("InstallBindings");

			if (GetType() != TypeOf<TDerived>.Raw) throw new Exception($"{GetType().FullName}: invalid generic base param: must be <{GetType().Name}> but actual is <{TypeOf<TDerived>.Name}>!");

			Container.BindInterfacesTo(GetType()).FromInstance(this);

			OnInstallBindings();
		}

		protected abstract void OnInstallBindings();

		protected abstract void OnInitialize();
		protected abstract void OnDispose();


	}

}