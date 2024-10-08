using System;
using Client.Cheats.Internal;
using UnityEngine;

namespace Client.Cheats.Contracts {

	[AttributeUsage(AttributeTargets.Method)]
	public class CheatPluginGUIAttribute : Attribute {
		public readonly string Name;
		public KeyCode HotKey;
		public readonly int Priority = 0;
		public readonly CheatPluginFlags Flags = CheatPluginFlags.None;
		public CheatPluginGUIAttribute(string name = null, KeyCode hotKey = KeyCode.None, int priority = 0, bool hidden = false) {
			Name = name;
			Priority = priority;
			HotKey = hotKey;
			Flags = Flags.With(CheatPluginFlags.Hidden, hidden);
		}
		protected CheatPluginGUIAttribute(string name, KeyCode hotKey, int priority, CheatPluginFlags flags) {
			Name = name;
			Priority = priority;
			HotKey = hotKey;
			Flags = flags;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class CheatMethodAttribute : CheatPluginGUIAttribute {
		public CheatMethodAttribute(string name = null, KeyCode hotKey = KeyCode.None, int priority = 0, CheatPluginFlags flags = CheatPluginFlags.AutoHideConsole | CheatPluginFlags.AutoResetConsole) : base(name, hotKey,
			priority, flags.With(CheatPluginFlags.Method, true)) {}

	}


	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CheatToggleAttribute : CheatPluginGUIAttribute {
		public CheatToggleAttribute(string name = null, KeyCode hotKey = KeyCode.None, int priority = 0, CheatPluginFlags flags = CheatPluginFlags.None) : base(name, hotKey, priority, flags.With(CheatPluginFlags.Toggle, true)) { }
	}
	
	[AttributeUsage(AttributeTargets.Class)]
	public class CheatCategoryAttribute : Attribute {
		public readonly string Name;
		public CheatCategoryAttribute(string name) => Name = name;
	}

}