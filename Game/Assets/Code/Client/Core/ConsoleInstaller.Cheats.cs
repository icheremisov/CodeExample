#pragma warning disable 0067

#if FEATURE_CONSOLE && FEATURE_CHEATS

#if UNITY_EDITOR || PLATFORM_STANDALONE
#define _TIMESCALE_CHEAT_ENABLED
#endif

using System;
using Client.Core.Contracts;
using Client.Core.Utils;
using LunarConsolePlugin;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Client.Core {

	public partial class ConsoleInstaller : IConsoleCheatsInitializer {
#if DEVELOPMENT_BUILD && !PLATFORM_STANDALONE
		private const bool ShowCheatsButton = true;
#else
		private const bool ShowCheatsButton = false; 
#endif		
		
		public static readonly CVar InGameCheatsButton = new("Show Cheats", ShowCheatsButton);
		
		public static event Action TimeScaleChanged;
		public static bool DisableTimeScaling { get; set; }

#if _TIMESCALE_CHEAT_ENABLED		
		public static float CurrentTimeScale => TimeScale[_currentSpeed];
		private const int PauseSpeed = 0;
		private const int DefaultSpeed = 3;
		private static int _currentSpeed = DefaultSpeed; // x1
		private static readonly float[] TimeScale = {
			0,
			0.1f,
			0.5f,
			1.0f,
			5.0f,
			10.0f
		};
		
		private void UpdateSpeed() {
			if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus)) SetSpeed(_currentSpeed - 1);
			if (Input.GetKeyUp(KeyCode.Plus) || Input.GetKeyUp(KeyCode.KeypadPlus)) SetSpeed(_currentSpeed + 1);
			if (Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.KeypadEquals) || Input.GetKeyUp(KeyCode.KeypadEnter)) SetSpeed(DefaultSpeed);
			if (Input.GetKeyUp(KeyCode.KeypadMultiply) || Input.GetKeyUp(KeyCode.Pause)) SetSpeed(PauseSpeed);
		}

		private static void SetSpeed(int index) {
			_currentSpeed = TimeScale.ClampIndex(index);
			if (!DisableTimeScaling) Time.timeScale = CurrentTimeScale;
			TimeScaleChanged?.Invoke();
		}
#else
		public static float CurrentTimeScale => 1.0f;
		private void UpdateSpeed() { }
#endif

		private void UpdateCheats() {
			UpdateSpeed();
		}

		private void SetupCheats() {
			LunarConsole.RegisterAction("Send Logs to Discord", SendLogsToDiscord);
			LunarConsole.RegisterAction("Delete Save", DeleteProfile);
		}
		
		private void DeleteProfile() {
			PlayerCheatHelper.DeleteAllProfiles();
			PlayerCheatHelper.StopApplication();
		}

		public void InitializeCheatVars() {}

		public static void SaveVariables() {
			if (_console) _console.MarkVariablesDirty();
		}
	}

}

#endif