using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Configs.Contracts {


	public class AutoGeneratedGuidAttribute : PropertyAttribute { }

	[AttributeUsage(AttributeTargets.Class)]
	public class TypeManifestIgnoreAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class)]
	public class ItemCategoryAttribute : Attribute {
		public string Name { get; }
		public ItemCategoryAttribute(string name) => Name = name;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ItemDescriptionAttribute : Attribute {
		public string Description { get; }
		public ItemDescriptionAttribute(string description) => Description = description;
	}

	[AttributeUsage(AttributeTargets.All)]
	public class AssetListDropdownAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
	public class ValidateAssetNameAttribute : Attribute {
		private readonly string _format;
		private Regex _regex;
		private readonly ValidatorSeverity _severity;
		private readonly bool _checkRootName;
		public bool CheckRootName => _checkRootName;
		public string Format => _format;
		public ValidatorSeverity Severity => _severity;

		public ValidateAssetNameAttribute(string format, bool checkRootName = false, ValidatorSeverity severity = ValidatorSeverity.Error) {
			_format = format;
			_severity = severity;
			_checkRootName = checkRootName;
		}

		public bool CheckValid(string fileName, out Dictionary<string, string> captures) {
			if (string.IsNullOrEmpty(fileName)) {
				captures = null;
				return false;
			}

			if (_regex == null) {
				var pattern = _format
					.Replace("{fraction}", "(?<fraction>[a-z]{2,3})")
					.Replace("{name}", "(?<name>[a-z]+)")
					.Replace("{lname}", "(?<name>[a-z_]+)")
					.Replace("{namex}", "(?<name>[a-z_0-9]+)")
					.Replace("{variant}", "(?<variant>[a-z]+)")
					.Replace("{xxx}", "(?<xxx>[0-9]{2,3})")
					.Replace("{color}", "(?<color>[a-z]+)");
				_regex = new Regex($"^{pattern}$");
			}

			try {
				var match = _regex.Match(fileName);
				if (match.Success) {
					captures = new Dictionary<string, string>();
					foreach (Group group in match.Groups) {
						if (group.Success && !string.IsNullOrEmpty(group.Name) && group.Name.Length > 1 && !string.IsNullOrEmpty(group.Value)) captures[group.Name] = group.Value;
					}

					return true;
				}
			}
			catch (Exception ex) {
				Debug.LogException(ex);
			}

			captures = null;
			return false;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class EnumBurgerAttribute : Attribute {
		public bool IsHorizontal { get; }

		public EnumBurgerAttribute(bool isHorizontal = false) {
			IsHorizontal = isHorizontal;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum)]
	public class ShtIgnoreAttribute : PropertyAttribute { }

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum)]
	public class ShtSerializeAttribute : PropertyAttribute { }

	[AttributeUsage(AttributeTargets.Class)]
	public class WithTabNameAttribute : Attribute {
		public string Name { get; }

		public WithTabNameAttribute(string name) {
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class IgnoreTabDecorator : Attribute { }

	
	public class SceneFilterAttribute : Attribute {
		public string Filter { get; }

		public enum SceneBuildModes
		{
			None,
			AddInBuildSettings,
			AddInAddressable,
		}

		public SceneBuildModes BuildMode { get; }

		public SceneFilterAttribute(string filter, SceneBuildModes buildMode = SceneBuildModes.None) {
			Filter = filter;
			BuildMode = buildMode;
		}
	}
}