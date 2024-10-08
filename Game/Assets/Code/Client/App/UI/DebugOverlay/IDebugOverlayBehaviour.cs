namespace Client.Meta.UI.GlobalOverlay {

	public interface IDebugOverlayBehaviour {
		void Show();
		void Hide();
		void SetInfo(string infoStr);
	}

}