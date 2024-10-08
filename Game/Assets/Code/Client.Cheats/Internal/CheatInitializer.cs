using Client.Cheats.Contracts;
using Client.Core;
using UnityEngine;

namespace Client.Cheats.Internal {

	public static class CheatInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize() => Cheat.Initialize(new CheatSystem());

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void AfterInit() {
			ConsoleInstaller.InGameCheatsButton.AddDelegate(var => Cheat.SetHidden(!var.BoolValue));
			Cheat.SetHidden(!ConsoleInstaller.InGameCheatsButton.BoolValue);
		}
	}

}