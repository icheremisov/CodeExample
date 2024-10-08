using LunarConsolePlugin;
using UnityEngine;
using Zenject;

namespace Client.Core {

	[CreateAssetMenu(menuName = "Installers/ConsoleInstaller", fileName = "ConsoleInstaller", order = 0), CVarContainer]
	public partial class ConsoleInstaller : ScriptableObjectInstaller<ConsoleInstaller>, IInitializable
	{
		
		public override void InstallBindings()
		{
#if FEATURE_CONSOLE
			Container.BindInterfacesTo<ConsoleInstaller>().FromInstance(this);
#endif // FEATURE_CONSOLE
		}

		public void Initialize() {
			
			// disable URP debugger
#if !(FEATURE_CONSOLE && FEATURE_CHEATS)
			{
				var urpDebug = GameObject.Find("[Debug Updater]");
				if (urpDebug) GameObject.Destroy(urpDebug);
			}
#endif
			
#if FEATURE_CONSOLE
			SetupConsole();
#endif // FEATURE_CONSOLE
#if FEATURE_CONSOLE && FEATURE_CHEATS
			SetupCheats();
#endif // FEATURE_CONSOLE && FEATURE_CHEATS
		}
	}
}
