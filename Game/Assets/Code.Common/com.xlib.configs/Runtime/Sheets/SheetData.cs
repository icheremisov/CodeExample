using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Core;
using XLib.Core.Utils;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using XLib.Configs.Core;
using XLib.Unity.Utils;
#endif

namespace XLib.Configs.Sheets {

	[HideMonoScript, TypeManifestIgnore]
	public abstract partial class SheetData : ScriptableObject, ISheetData {
		[ShowInInspector, HideLabel, PropertyOrder(-1)]
		private string ImporterType => GetType().Name;

		[HideLabel, HorizontalGroup]
		[SerializeField] private string _title;

		[SerializeField, HideInInspector]
		private bool _withFilter = true;

		[SerializeField, HideInInspector] private SheetsSettings _owner;

		public SheetFormatSettings Settings => _owner.FormatSettings;
		public virtual bool CheckBranch => true;

		public string Title => _title;

		[SerializeField, HideInInspector]
		private DirectionType _direction = DirectionType.None;
		public DirectionType Direction { get => _direction == DirectionType.None ? GetDefaultDirection() : _direction; set => _direction = value; }

		public bool WithFilter { get => _withFilter && Direction == DirectionType.Horizontal; set => _withFilter = value; }

		protected virtual DirectionType GetDefaultDirection() => DirectionType.Horizontal;

		public abstract IEnumerable<SheetColumnValues> Export(SheetContext context);
		public abstract void Import(IEnumerable<SheetRowValues> values, SheetContext context);

		public virtual void Setup(SheetsSettings owner) {
			name = GetType().Name;
			_owner = owner;
#if UNITY_EDITOR
			_title = ObjectNames.NicifyVariableName(name.Replace("Sheet", string.Empty)
				.Replace("Definition", string.Empty)
				.Replace("Importer", string.Empty));
#endif
		}

#if UNITY_EDITOR
		public virtual void SelectAssets() { }
		public SheetsSettings Owner => _owner;
#endif
	}

	public enum DirectionType {
		None,
		Horizontal,
		Vertical
	}

	public class WildcardPattern {
		private readonly Regex _regex;

		public WildcardPattern(string pattern) {
			if (string.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));
			_regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		public bool IsMatch(string value) => _regex.IsMatch(value);
	}

	public enum FilterType {
		Name,
		Path,
		Type
	}

	[Serializable]
	public class Rule {
		[SerializeField]
		private FilterType _type;
		[SerializeField]
		private string _pattern;

		private WildcardPattern _wildcardPattern;

		public void Prepare() => _wildcardPattern = string.IsNullOrEmpty(_pattern) ? null : new WildcardPattern(_pattern);

		public bool IsMatch(Object o) =>
			_wildcardPattern?.IsMatch(_type switch {
				FilterType.Name => o.name,
#if UNITY_EDITOR
				FilterType.Path => UnityEditor.AssetDatabase.GetAssetPath(o),
#endif
				FilterType.Type => o.GetType().Name,
				_               => throw new ArgumentOutOfRangeException(nameof(_type), _type, null)
			}) ?? true;
	}

	public abstract class SheetData<T> : SheetData, ISheetPropertyFilter where T : Object, IOrderBy {
		[SerializeField, TableList(AlwaysExpanded = true), ShowIf("$_assetFilter"), Space, Indent(5)]
		private Rule[] _assetFilters;
		[SerializeField, HideInInspector]
		private bool _assetFilter = false;

		private void OnValidate() {
			if (_assetFilter && _assetFilters is not { Length: > 0 }) _assetFilters = new[] { new Rule() };
		}

		private void PrepareFilters() => _assetFilters.ForEach(rule => rule.Prepare());
		private bool IsFilterMatch(Object obj) => !_assetFilter || _assetFilters.Length <= 0 || _assetFilters.Any(rule => rule.IsMatch(obj));

		protected virtual IEnumerable<T> GetRows() {
			if (TypeOf<GameItemCore>.IsAssignableFrom<T>()) 
				return Assets<T>().OrderBy(o => (o as GameItemCore).FileName);
			return Assets<T>().OrderBy(o => o != null ? o.OrderByValue : 0);
		}

		protected virtual IExportObjectHandler ExportObjectHandler { get; } = null;
		protected virtual IImportObjectHandler ImportObjectHandler { get; } = null;

		public virtual bool IsFilterProperty(SheetRowProperty property, object o, SheetRowProperty sheetRowProperty) => true;

		public override IEnumerable<SheetColumnValues> Export(SheetContext context) {
			var rows = GetRows().ToArray();
			try {
				PreExport(rows);
				return context.CreateExporter(this, ExportObjectHandler).GetValues(rows);
			}
			finally {
				PostExport(rows);
			}
		}

		protected override DirectionType GetDefaultDirection() => TypeOf<IGameItemSingleton>.IsAssignableFrom<T>() ? DirectionType.Vertical : DirectionType.Horizontal;

		protected virtual void PreExport(T[] rows) { }
		protected virtual void PostExport(T[] rows) { }

		protected virtual void PreImport(T[] rows) { }
		protected virtual void PostImport(T[] rows) { }

		public override void Import(IEnumerable<SheetRowValues> values, SheetContext context) {
			var rows = GetRows().ToArray();
			try {
				PreImport(rows);
				context
					.CreateImporter(ImportObjectHandler)
					.SetValues(rows, values);
			}
			finally {
				PostImport(rows);
			}
		}

#if UNITY_EDITOR

		[HorizontalGroup(30), PropertyTooltip("Use asset filtering"), Button("", ButtonSizes.Small, Icon = SdfIconType.Search), GUIColor(nameof(AssetFilterColor))]
		private void AssetFilter() {
			_assetFilter = !_assetFilter;
			OnValidate();
		}

		private Color AssetFilterColor() => _assetFilter ? new Color(0.75f, 0.66f, 1f) : new Color(0.45f, 0.38f, 0.61f);

		[HorizontalGroup(30), PropertyTooltip("Add filters and sorting to spreadsheets"),
		 Button("", ButtonSizes.Small, Icon = SdfIconType.FilterSquare), GUIColor(nameof(TableWithFilterColor))]
		private void TableWithFilter() => WithFilter = !WithFilter;

		private Color TableWithFilterColor() => WithFilter ? new Color(1f, 0.84f, 0.66f) : new Color(0.61f, 0.48f, 0.4f);

		protected IEnumerable<TAsset> Assets<TAsset>() where TAsset : Object {
			PrepareFilters();
			return EditorUtils.LoadAssets<TAsset>().Where(IsFilterMatch); // .OrderBy(OrderByAssets);
		}

		protected TAsset Once<TAsset>() where TAsset : Object => EditorUtils.LoadAssets<TAsset>().FirstOrDefault();

		public class AssetSelectorBase : SimpleSelectorBase<T> {
			private readonly List<T> _source;
			private readonly bool _supportsMultiSelect;
			private readonly GUIStyle _helper;

			public AssetSelectorBase(List<T> source, bool supportsMultiSelect) {
				_source = source;
				_supportsMultiSelect = supportsMultiSelect;
				_helper = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Italic, normal = new GUIStyleState() { textColor = Color.gray }
				};
			}

			protected override void BuildSelectionTree(OdinMenuTree tree) {
				tree.Selection.SupportsMultiSelect = _supportsMultiSelect;
				tree.Config.DrawSearchToolbar = true;
				tree.Config.AutoFocusSearchBar = true;

				foreach (var item in tree.AddRange(_source, o => {
							 if (o == null) return "*";
							 var main = o.GetRootAsset();
							 return (main != null && main != o) ? $"{main.name}/{o.name}" : $"{o.name}";
						 })) {
					item.OnDrawItem += OnDrawItem;
				}
			}

			private void OnDrawItem(OdinMenuItem item) {
				if (item.Value is not GameItem gi) return;
				var name = gi.Name;
				var text = new GUIContent(name.Length > 20 ? $"{name[..20]}..." : name);
				var rc = item.LabelRect;
				GUI.Label(new Rect(rc.xMax - rc.width / 2 - 20, rc.y, rc.width / 2, rc.height), text, _helper);
			}
		}

		public override void SelectAssets() {
			var selector = new AssetSelectorBase(GetRows().ToList(), false);
			selector.SelectionConfirmed += OnSelectAsset;
			selector.ShowInPopup(400, 400);
		}

		private void OnSelectAsset(IEnumerable<T> obj) => Selection.objects = obj.ToArray();

#else
		protected IEnumerable<TAsset> Assets<TAsset>() where TAsset : Object => Array.Empty<TAsset>();
		protected TAsset Once<TAsset>() where TAsset : Object => default;
#endif
	}

}