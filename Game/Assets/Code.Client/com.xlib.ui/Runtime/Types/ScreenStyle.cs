using System;

namespace XLib.UI.Types {

	/// <summary>
	///     UI screen configuration flags
	/// </summary>
	[Flags]
	public enum ScreenStyle {
		/// <summary>
		///     no specific styles set
		/// </summary>
		None = 0,

		/// <summary>
		///     call GoBack() method when android Hardware back button pressed
		/// </summary>
		HandleAndroidBackButton = 1 << 0,

		PauseInput = 1 << 1,

		PauseGame = 1 << 2,

		FullScreen = 1 << 3,

		AlwaysOnTop = 1 << 4,

		System = 1 << 5,
		
		UnloadOnClose = 1 << 6,

		Default = HandleAndroidBackButton | PauseGame | PauseInput,
		DefaultFullscreen = HandleAndroidBackButton | PauseGame | PauseInput | FullScreen
	}

}