using Client.Cheats.Internal;
using UnityEngine;
using XLib.Core.Utils;

namespace Client.Cheats.Contracts {

	public interface ICheatSystem : ILockable {
		void Initialize();
		void SetCommand(string menuName, string searchQuery, object args);
		void PopState();
		void Minimize(bool reset);
		void Maximize();
		void SetHidden(bool hidden);
		void Scroll(Vector2 delta);
		void DoHotkeyGui();
		void DoGui(Rect rect);
		void ResetSelect();
		void GetCurrentState(ref CheatStoreState state);
	}

}