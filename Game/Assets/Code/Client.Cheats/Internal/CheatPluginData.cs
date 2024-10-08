using System;
using System.Linq;
using System.Reflection;
using Client.Cheats.Contracts;
using Sirenix.Utilities;
using UnityEngine;
using XLib.Core.Utils;

namespace Client.Cheats.Internal {

	[Flags]
	public enum CheatPluginFlags {
		None = 0,
		Hidden = 1 << 0, // скрытая команда
		Method = 1 << 1, // метод
		Toggle = 1 << 2, // переключатель
		AutoHideConsole = 1 << 3, // автоматически скрывать консоль
		AutoResetConsole = 1 << 4, // автоматически сбрасывать консоль
		
	}
	
	public class CheatPluginData {
		public readonly string FullName;
		public readonly string Category;
		public readonly string RootName;
		public readonly string Name;
		public readonly int Priority;
		public readonly KeyCode HotKey;
		public readonly bool IsStatic;

		public readonly Type[] ArgumentType;
		public readonly Type OwnerType;

		public readonly CheatPluginFlags Flags;

		public bool IsMethod => Flags.Has(CheatPluginFlags.Method);
		public bool IsToggle => Flags.Has(CheatPluginFlags.Toggle);
		public bool Hidden => Flags.Has(CheatPluginFlags.Hidden);
		

		private readonly MethodInfo _methodInfo;
		private readonly FieldInfo _fieldInfo;
		private readonly PropertyInfo _propertyInfo;

		private CheatArgumentData _arguments;
		public CheatArgumentData Arguments => _arguments;
		public string Caption => GetCaption();

		private string GetCaption() {
			if (Arguments.IsEmpty) return $"<color=#707372>{Name}</color>";
			if (IsToggle) return $"<color=#3bdb9e>{Name}: {(GetToggleValue() ? "<color=green>ON</color>" : "<color=red>OFF</color>") }</color>";
			if (IsMethod) return $"<color=#3bc4d9>{Name}</color>";
			return $"<color=#3bdb9e>{Name}</color>";
		}

		private bool GetToggleValue() {
			if (!IsToggle) return  false;
			if (_fieldInfo != null) return (bool)_fieldInfo.GetValue(null);
			if (_propertyInfo != null) return (bool)_propertyInfo.GetValue(null);
			return false;
		}

		public CheatPluginData(Type type, MemberInfo memberInfo, CheatPluginGUIAttribute plugin) {
			if (plugin.Name.IsNullOrEmpty()) {
				var name = type.Name;
				name = name.Replace("Cheats", string.Empty);
				FullName = $"{name}/{memberInfo.Name}".PrettyName();
			}
			else
				FullName = plugin.Name;

			Flags = plugin.Flags;
			HotKey = plugin.HotKey;
			OwnerType = type;
			DeclaringType = memberInfo.DeclaringType;

			var parts = FullName.Split("/", 2);
			Name = parts.Last();

			Category = parts.Length < 2 ? "Global" : parts.First();
			Priority = plugin.Priority;
			IsStatic = memberInfo.IsStatic();

			RootName = memberInfo.DeclaringType.GetCustomAttribute<CheatCategoryAttribute>()?.Name ?? memberInfo.DeclaringType?.Name ?? "Other";
			
			if (memberInfo is MethodInfo methodInfo) {
				_methodInfo = methodInfo;
				var args = methodInfo.GetParameters();
				ArgumentType = args.Select(info => info.ParameterType).ToArray();
			} else if (memberInfo is FieldInfo fieldInfo) {
				_fieldInfo = fieldInfo;
				ArgumentType = Array.Empty<Type>();
				Flags = Flags.With(CheatPluginFlags.Toggle, _fieldInfo.FieldType == TypeOf<bool>.Raw);
			} else if (memberInfo is PropertyInfo propertyInfo) {
				_propertyInfo = propertyInfo;
				ArgumentType = Array.Empty<Type>();
				Flags = Flags.With(CheatPluginFlags.Toggle, _propertyInfo.PropertyType == TypeOf<bool>.Raw);
			}

			_arguments = new CheatArgumentData(this);
		}

		public Type DeclaringType { get; set; }

		public void Invoke(object[] args) {
			if (_methodInfo != null) {
				if (!IsMethod || GUILayout.Button(Name)) {
					if (Flags.Has(CheatPluginFlags.AutoResetConsole)) {
						Cheat.ResetSelect();
					}
					if (Flags.Has(CheatPluginFlags.AutoHideConsole)) {
						Cheat.Minimize();
					}
					_methodInfo.Invoke(null, ArgumentType == null ? Array.Empty<object>() : args);
				}
			}

			if (IsToggle) {
				if (_fieldInfo != null) {
					var value = (bool)_fieldInfo.GetValue(null);
					var valueOut = GUILayout.Toggle(value, value ? "ON" : "OFF", GUI.skin.button);
					if (value != valueOut) _fieldInfo.SetValue(null, valueOut);
				} else if (_propertyInfo != null) {
					var value = (bool)_propertyInfo.GetValue(null);
					var valueOut = GUILayout.Toggle(value, value ? "ON" : "OFF", GUI.skin.button);
					if (value != valueOut) _propertyInfo.SetValue(null, valueOut);
				}
			}
		}

		public void InvokeMethod(object[] args) {
			if (_methodInfo != null) {
				if(Flags.Has(CheatPluginFlags.AutoResetConsole)) Cheat.ResetSelect();
				if (Flags.Has(CheatPluginFlags.AutoHideConsole)) Cheat.Minimize();
				_methodInfo.Invoke(null, ArgumentType == null ? Array.Empty<object>() : args);
			}
			else if (IsToggle) {
				if(_fieldInfo != null)
					_fieldInfo.SetValue(null, !GetToggleValue());
				else if(_propertyInfo != null)
					_propertyInfo.SetValue(null, !GetToggleValue());
			}
		}

		public void ResolveArguments(CheatDiResolver container) => _arguments.Update(container);
	}

}