using System;

namespace Client.Core.Common.Scene {

	public static class AppLogger {
		private static readonly Logger Logger = new("App", RichLog.Color.yellow);

		public static void Log(FormattableString message) => Logger.Log(message);
		public static void LogWarning(FormattableString message) => Logger.LogWarning(message);
		public static void LogError(FormattableString message) => Logger.LogError(message);
		public static void LogException(Exception ex) => Logger.LogException(ex);

		public static void Log(string message) => Logger.Log(message);
		public static void LogWarning(string message) => Logger.LogWarning(message);
		public static void LogError(string message) => Logger.LogError(message);
	}

}