using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XLib.Core.Utils;
using XLib.Unity.LocalStorage;

namespace XLib.Unity.Runtime.Utils {

	public class StoredLogger : Logger {
		private readonly StoredValue<LogLevel> _logLevel;

		public StoredLogger(string tag, RichLog.Color? color = null, LogLevel level = LogLevel.Info) : base(tag, color, level) {
			_logLevel = new(tag, level);
			base.SetLevel(_logLevel.Value);
			InitializeOnEditor();
		}

		public override void SetLevel(LogLevel level) {
			base.SetLevel(level);
			_logLevel.Value = level;
			InitializeOnEditor();
		}

		public LogLevel Level => _logLevel.Value;

#if UNITY_EDITOR
		private static MethodInfo _removeMenuItem;
		private static MethodInfo _addMenuItem;
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private void InitializeOnEditor() {
			
			if (_removeMenuItem == null) _removeMenuItem = TypeOf<UnityEditor.Menu>.Raw.GetMethod("RemoveMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
			
			// internal static extern void AddMenuItem(
			// 	string name,
			// 	string shortcut,
			// 	bool @checked,
			// 	int priority,
			// 	Action execute,
			// 	Func<bool> validate);
			if (_addMenuItem == null) _addMenuItem = TypeOf<UnityEditor.Menu>.Raw.GetMethod("AddMenuItem", BindingFlags.Static | BindingFlags.NonPublic);

			var menuName = $"Tools/Logging/{_tag}";

			_removeMenuItem.Invoke(null, new object[] { menuName });

			foreach (var value in Enums.Values<LogLevel>()) {
				_addMenuItem.Invoke(null, new object[] {
					$"{menuName}/{value}",
					"",
					_logLevel.Value == value,
					value.AsInt(),
					InternalEditorSetLevel(value),
					null
				});
			}

			return;
			Action InternalEditorSetLevel(LogLevel value) => () => SetLevel(value);
		}
#else
		private void InitializeOnEditor() {}
#endif
	}

}