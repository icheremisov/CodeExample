using Client.Cheats.Internal;
using Cysharp.Threading.Tasks;
using XLib.Core.Utils;

namespace Client.Cheats.Contracts {

	public static class Cheat {
		private static ICheatSystem _system;
		public static void Initialize(ICheatSystem system) => (_system = system).Initialize();
		public static void Minimize(bool reset = false) => _system.Minimize(reset);
		public static void Maximize() => _system.Maximize();
		public static void SetState(string menuName, string searchQuery = null, object args = null) => _system.SetCommand(menuName, searchQuery, args);
		public static void PopState() => _system.PopState();
		public static void SetHidden(bool hidden) => _system.SetHidden(hidden);
		public static void GetCurrentState(ref CheatStoreState state) => _system.GetCurrentState(ref state);
		public static ILockable Locker => _system;

		public static void ResetSelect() => _system.ResetSelect();

		public static void Refresh() {
			Minimize();
			UniTask.DelayFrame(1).OnComplete(Maximize);
		}
	}

}