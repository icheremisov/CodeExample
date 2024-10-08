using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Utils;
using XLib.Configs.Sheets.Types;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Core {

	public class SheetRowProperty {
		public bool CanWrite { get; internal set; }
		public string Name => Names.First();
		public string[] Names { get; internal set; }
		public string Description { get; set; } = null;
		public Type Type { get; internal set; }
		public Type ElementType { get; internal set; }

		public bool IsKey { get; set; } = false;
		public bool IsProtected { get; set; } = false;
		public bool PreserveEmptyElements { get; set; } = false;
		public int Priority { get; set; } = -1;

		public Color? Color { get; set; } = null;
		public Color? BackgroundColor { get; set; } = null;

		public string Tooltip { get; set; } = null;

		public ValueValidationRule ValidationRule { get; private set; }
		public bool WithValidation { get; set; } = true;
		public int FixedSize { get; set; } = 0;
		public WrapStrategy WrapStrategy { get; set; } = WrapStrategy.WRAP;
		public bool OnlyWrite { get; set; } = false;

		private readonly Action<object, object> _setValue;
		private readonly Func<object, object> _getValue;

		public bool IsEnumerable => Type != typeof(string) && Type.IsImplementGeneric<IEnumerable>();

		public int Index { get; set; } = 0;
		public bool HasFormat => Color != null || BackgroundColor != null;

		public IReadOnlyList<Attribute> Attributes => _attributes;
		public string InlineProperty { get; set; } = null;
		public bool IsMergeEqual { get; set; } = false;

		private Attribute[] _attributes;
		private readonly string _realName;
		private List<string> _tagList = new();
		private bool _ignoreTagAlways = false;
		private string _visibleCheckMethod = null;
		private int _skipFirstCount = 0;
		public int SkipFirstCount => _skipFirstCount;

		public static SheetRowProperty Create(PropertyInfo property, IEnumerable<Attribute> attributes) =>
			new(property.Name, property.PropertyType, property.SetValue, property.GetValue, property.CanWrite, attributes);

		public static SheetRowProperty Create(FieldInfo field, IEnumerable<Attribute> attributes) =>
			new(field.Name, field.FieldType, field.SetValue, field.GetValue, true, attributes);

		public static SheetRowProperty Create(string name, Type type, IEnumerable<Attribute> attributes) => new(name, type, null, null, true, attributes);

		public static SheetRowProperty Create<T>(string name, params Attribute[] attributes) => new(name, TypeOf<T>.Raw, null, null, true, attributes);

		private SheetRowProperty(string name, Type type, Action<object, object> setValue, Func<object, object> getValue, bool canWrite, IEnumerable<Attribute> attributes) {
			_setValue = setValue;
			_getValue = getValue;
			_realName = name;
			CanWrite = canWrite;
			Type = type;
			if (Type.IsArray)
				ElementType = Type.GetElementType();
			else if (IsEnumerable) {
				ElementType = null;
				foreach (var tint in Type.GetInterfaces()) {
					if (!tint.IsGenericType || !typeof(IEnumerable<>).IsAssignableFrom(tint.GetGenericTypeDefinition())) continue;

					ElementType = tint.GenericTypeArguments[0];
					break;
				}
			}

			if (attributes != null) {
				_attributes = attributes.ToArray();
				foreach (var attribute in _attributes.OfType<SheetAttribute>()) attribute.Apply(this);
			}

			if (Names is not { Length: > 0 }) Names = new[] { ConfigUtils.PrettyName(name) };

			var t = ElementType ?? type;
			SetValidationRule(t.IsEnum ? Enum.GetNames(t) : null);
		}

		public void SetValidationRule(IEnumerable<object> values) {
#if UNITY_EDITOR
			var req = _attributes != null && _attributes.Any(x => x is RequiredAttribute);
			if (values != null) {
				ValidationRule = new ValueValidationRule(ConditionType.ONE_OF_LIST, values, true, req);
			}
			else {
				if (_attributes == null) return;
				foreach (var attribute in _attributes) {
					switch (attribute) {
						case ValueDropdownAttribute t: {
							// это залипуха. для честного вызова value resolver-a нужно использовать
							// конкретный экземпляр объекта и конкретное свойство, но это сложно сюда прокинуть
							// коэтому использую фейковое свойство. будет работать если в коде дергается статичный метод
							// который в большинстве случаев используется
							using var propertyTree = Sirenix.OdinInspector.Editor.PropertyTree.CreateStatic(Type);
							var res = Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver.Get<object>(propertyTree.GetRootProperty(0), t.ValuesGetter);
							if (res.GetValue() is IEnumerable source) {
								values = source.Cast<object>()
									.Where(x => x != null)
									.Select(x => x switch {
										ValueDropdownItem el   => el.Value,
										IValueDropdownItem iel => iel.GetValue(),
										_                      => x
									})
									.ToArray();
							}

							if (values == null) return;
							ValidationRule = new ValueValidationRule(ConditionType.ONE_OF_LIST, values, true, req);
							return;
						}

						case RangeAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_BETWEEN, new object[] { t.min, t.max }, false, req);
							return;

						case NotInRangeAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_NOT_BETWEEN, new object[] { t.Min, t.Max }, false, req);
							return;

						case ExceptValueAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_NOT_EQ, new object[] { t.ValueToExcept }, false, req);
							return;

						case StringContainsAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.TEXT_CONTAINS, new object[] { t.Substring }, false, req);
							return;

						case StringNotContainsAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.TEXT_NOT_CONTAINS, new object[] { t.Substring }, false, req);
							return;

						case MinValueAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_GREATER_THAN_EQ, new object[] { t.MinValue }, false, req);
							return;

						case MinAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_GREATER_THAN_EQ, new object[] { t.min }, false, req);
							return;

						case MaxValueAttribute t:
							ValidationRule = new ValueValidationRule(ConditionType.NUMBER_LESS_THAN_EQ, new object[] { t.MaxValue }, false, req);
							return;
					}
				}
			}
#endif
		}

		public object GetValue(object obj) {
			try {
				if (_getValue != null) return _getValue.Invoke(obj);
				if (obj is IDynamicSheetRow dynamicRow) return dynamicRow.GetValue(_realName);
				return null;
			}
			catch (Exception ex) {
				Debug.LogError($"Get Value from property {Name} ({obj}) with error{ex.Message}");
				throw;
			}
		}

		public void SetValue(object obj, object value) {
			if (_setValue != null)
				_setValue.Invoke(obj, value);
			else if (obj is IDynamicSheetRow dynamicRow) dynamicRow.SetValue(_realName, value);
		}

		public void SetTagFilter(bool always, IEnumerable<string> tagsName) {
			if (tagsName != null) _tagList.AddRange(tagsName);
			_ignoreTagAlways |= always;
		}

		public bool HasTag(string tag) {
			if (_ignoreTagAlways) return true;
			return string.IsNullOrEmpty(tag) ? _tagList.Count <= 0 : _tagList.Any(s => s.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
		}

		public void SetVisibleMethod(string method) => _visibleCheckMethod = method;

		public bool? IsVisible(SheetColumnData columnData) {
			if (string.IsNullOrEmpty(_visibleCheckMethod)) return null;

#if UNITY_EDITOR
			using var propertyTree = Sirenix.OdinInspector.Editor.PropertyTree.CreateStatic(Type);
			var res = Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver.Get<object>(propertyTree.GetRootProperty(0), _visibleCheckMethod,
				new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("index", typeof(int), columnData.Index.ArrayIndex ?? 0));
			return (bool)res.GetValue();
#else
			return null;
#endif
		}

		public void SetSkipFirstCount(int count) => _skipFirstCount = count;

		public bool? IsSkip(SheetColumnData columnData) {
			if (_skipFirstCount == 0) return null;

#if UNITY_EDITOR
			return columnData.Index.ArrayIndex < _skipFirstCount;
#else
			return null;
#endif
		}
	}

}