using XLib.UI.ConnectionBlocker;
using XLib.UI.Types;

namespace XLib.UI.ConnectionBlocker {

	public interface IBlockerView {
		bool IsBlocked { get; }

		void UnlockAll();
		
		bool HasTag(ScreenLockTag t);
		void Open(ScreenLockTag t);
		void Close(ScreenLockTag t);
		void DisableOpening(ScreenLockTag t);
		void EnableOpening(ScreenLockTag t);
	}

}

public static class BlockerViewExtensions {
	public static void SetVisible(this IBlockerView view, ScreenLockTag t, bool v) {
		if (!(BlockerView)view) return;

		if (v)
			view.Open(t);
		else
			view.Close(t);
	}

	public static void DisableOpening(this IBlockerView view, ScreenLockTag t, bool disabled) {
		if (!(BlockerView)view) return;

		if (disabled)
			view.DisableOpening(t);
		else
			view.EnableOpening(t);
	}
}