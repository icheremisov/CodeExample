using Zenject;

namespace XLib.Unity.Core {

	public interface IContainerListener {

		public void OnInstall(DiContainer container);
		public void OnUninstall(DiContainer container);

	}

}