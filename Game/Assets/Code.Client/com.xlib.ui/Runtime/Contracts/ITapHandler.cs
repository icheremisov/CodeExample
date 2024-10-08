namespace XLib.UI.Contracts {

	public interface ITapHandler {
		void OnTapStarted();
		void Tap();
		void LongTap();
		void OnLongTapEnded();
        
		int TapPriority { get; } // 0 - default, int.MinValue = skip tap
	}

}