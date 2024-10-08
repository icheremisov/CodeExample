using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLib.Configs.Sheets.Core;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Contracts {

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class AllowMultipleSheetsAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SheetsConverterAttribute : Attribute {
		public readonly Type Type;
		public readonly bool ExportCanBeSkipped;

		public SheetsConverterAttribute(Type type, bool exportCanBeSkipped = true) {
			Type = type;
			ExportCanBeSkipped = exportCanBeSkipped;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public abstract class SheetAttribute : PropertyAttribute {
		public abstract void Apply(SheetRowProperty property);
	}

	public class ShtPriorityAttribute : SheetAttribute {
		private readonly int _index = 0;
		public ShtPriorityAttribute(int index) => _index = index;
		public override void Apply(SheetRowProperty property) => property.Priority = _index;
	}

	public class ShtKeyAttribute : SheetAttribute {
		public override void Apply(SheetRowProperty property) {
			property.IsKey = true;
			property.IsMergeEqual = true;
			property.SetTagFilter(true, null);
		}
	}

	public class ShtNoValidationAttribute : SheetAttribute {
		public override void Apply(SheetRowProperty property) => property.WithValidation = false;
	}

	public class ShtProtectedAttribute : SheetAttribute {
		private readonly bool _protected;
		public ShtProtectedAttribute(bool @protected = true) => _protected = @protected;
		public override void Apply(SheetRowProperty property) => property.IsProtected = _protected;
	}

	public class ShtPreserveEmptyElementsAttribute : SheetAttribute {
		private readonly bool _preserve;
		public ShtPreserveEmptyElementsAttribute(bool preserve = true) => _preserve = preserve;
		public override void Apply(SheetRowProperty property) => property.PreserveEmptyElements = _preserve;
	}

	public class ShtNameAttribute : SheetAttribute {
		private readonly string[] _names;
		public ShtNameAttribute(string name, params string[] names) => _names = names.Length > 0 ? new[] { name }.Concat(names).ToArray() : new[] { name };
		public override void Apply(SheetRowProperty property) => property.Names = _names;
	}

	public class ShtDescriptionAttribute : SheetAttribute {
		private readonly string _description;
		public ShtDescriptionAttribute(string description) => _description = description;
		public override void Apply(SheetRowProperty property) => property.Description = _description;
	}

	//https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets/cells#wrapstrategy
	public enum WrapStrategy {
		OVERFLOW_CELL,
		CLIP,
		WRAP,
	}
	
	public class ShtFixedSizeAttribute : SheetAttribute {
		private readonly int _pixelSize;
		private readonly WrapStrategy _strategy;

		public ShtFixedSizeAttribute(int pixelSize, WrapStrategy strategy = WrapStrategy.WRAP) {
			_pixelSize = pixelSize;
			_strategy = strategy;
		}

		public override void Apply(SheetRowProperty property) {
			property.FixedSize = _pixelSize;
			property.WrapStrategy = _strategy;
		}
	}

	public class ShtColorAttribute : SheetAttribute {
		private readonly Color _color;
		public ShtColorAttribute(float r, float g, float b) => _color = new Color(r, g, b);
		public ShtColorAttribute(float c) => _color = new Color(c, c, c);
		public override void Apply(SheetRowProperty property) => property.Color = _color;
	}

	public class ShtBackgroundAttribute : SheetAttribute {
		private readonly Color _color;
		public ShtBackgroundAttribute(float r, float g, float b) => _color = new Color(r, g, b);
		public ShtBackgroundAttribute(float c) => _color = new Color(c, c, c);
		public override void Apply(SheetRowProperty property) => property.BackgroundColor = _color;
	}

	public class ShtTooltipAttribute : SheetAttribute {
		private readonly Type _classType;
		private readonly string _methodName;
		private readonly string _tooltip;

		public ShtTooltipAttribute(string tooltip = null, Type classType = null, string methodName = null) {
			_tooltip = tooltip;
			_classType = classType;
			_methodName = methodName;
		}

		public override void Apply(SheetRowProperty property) {
			if (_classType != null && !_methodName.IsNullOrEmpty()) {
				var method = _classType.GetMethod(_methodName);
				if (method != null && method.ReturnType == TypeOf<string>.Raw) {
					property.Tooltip = (string)method.Invoke(this, null);
					return;
				}
			}

			property.Tooltip = _tooltip;
		}
	}

	public class ShtValuesAttribute : SheetAttribute {
		private readonly string _funcName;
		public ShtValuesAttribute(string funcName) => _funcName = funcName;

		public override void Apply(SheetRowProperty property) {
			var method = property.Type.GetMethod(_funcName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			property.SetValidationRule((IEnumerable<object>)method.Invoke(null, Array.Empty<object>()));
		}
	}
	
	public class ShtOnlyWriteAttribute : SheetAttribute {
		public ShtOnlyWriteAttribute() { }
		public override void Apply(SheetRowProperty property) => property.OnlyWrite = true;
	}
	
	public class ShtTagAttribute : SheetAttribute {
		private readonly string[] _tagsName;
		private bool _always = false;
		public ShtTagAttribute(params string[] tagName) => _tagsName = tagName;
		public ShtTagAttribute(bool always) => _always = true;

		public override void Apply(SheetRowProperty property) => property.SetTagFilter(_always, _tagsName);
	}

	public class ShtInlineAttribute : SheetAttribute {
		private readonly string _propKey;
		public ShtInlineAttribute(string propKey) => _propKey = propKey;

		public override void Apply(SheetRowProperty property) => property.InlineProperty = _propKey;
	}

	public class ShtVisibleAttribute : SheetAttribute {
		private readonly string _funcName;
		public ShtVisibleAttribute(string funcName) => _funcName = funcName;

		public override void Apply(SheetRowProperty property) => property.SetVisibleMethod(_funcName);
	}
	
	public class ShtSkipFirstCountAttribute : SheetAttribute {
		private readonly int _count;
		public ShtSkipFirstCountAttribute(int count) => _count = count;

		public override void Apply(SheetRowProperty property) => property.SetSkipFirstCount(_count);
	}
	
}