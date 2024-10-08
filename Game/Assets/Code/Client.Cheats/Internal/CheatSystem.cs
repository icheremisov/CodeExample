using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Client.Cheats.Contracts;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Core.Collections;
using XLib.Core.Utils;
using XLib.Unity.LocalStorage;
using XLib.Unity.Utils;
using Zenject;
using Object = UnityEngine.Object;

namespace Client.Cheats.Internal {

	public class CheatSystem : ICheatSystem {
		private readonly List<CheatPluginData> _plugins = new(32);
		private readonly Dictionary<KeyCode, CheatPluginData> _pluginsHotkeys = new(32);
		private readonly OrderedDictionary<string, List<CheatPluginData>> _pluginsCategory = new(32);

		private GUIContent[] _categories;
		private int _category;
		private bool _submenu;
		private Vector2 _scrollPosition;
		private string _searchQuery = "";

		private CheatPluginData _selectedPlugin;
		private List<CheatPluginData> _filteredPluginList;
		private OrderedDictionary<string, CheatCategoryList> _filteredPluginNames;

		// private (CheatPluginData data, object[])[] _commonPluginArgs;
		private CheatDiResolver _container;
		private DebugView _view;
		private readonly ILockable _locker = new LockerCounter();
		private readonly StoredValue<bool> _cheatHidden = new("CheatHidden", true);
		private readonly Stack<CheatStoreState> _stateStack = new(10);
		private Vector2 _menuScrollPosition;

		public void Initialize() {
			InitializeTypes();
			var go = Object.Instantiate(Resources.Load<GameObject>("DebugView"));
			Object.DontDestroyOnLoad(go);
			_view = go.GetComponent<DebugView>();
			_view.SetCheatSystem(this);
			_view.SetHidden(_cheatHidden);

			_container = new CheatDiResolver(_locker, this);
			ProjectContext.Instance.Container.BindInterfacesAndSelfTo<CheatDiResolver>().FromInstance(_container);
		}

		private void InitializeTypes() {
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				foreach (var memberInfo in type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
					var plugin = memberInfo.GetCustomAttribute<CheatPluginGUIAttribute>();
					if (plugin == null) continue;
					AddPlugin(new CheatPluginData(type, memberInfo, plugin));
				}
			}

			foreach (var value in _pluginsCategory.Values) value.Sort((a, b) => Comparer<string>.Default.Compare(a.Category, b.Category) * 1000 + (a.Priority - b.Priority));
		}

		private void AddPlugin(CheatPluginData pluginData) {
			if (!_pluginsCategory.ContainsKey(pluginData.RootName)) _pluginsCategory[pluginData.RootName] = new List<CheatPluginData>(16);
			_plugins.Add(pluginData);
			if (pluginData.Hidden) return;
			_pluginsCategory[pluginData.RootName].Add(pluginData);
			if (pluginData.HotKey != KeyCode.None) _pluginsHotkeys[pluginData.HotKey] = pluginData;
		}

		public void DoGui(Rect rect) {
			GUI.enabled = !_locker.IsLocked;
			var inspectRefresh = false;

			if (_categories == null) {
				_categories = _pluginsCategory.Keys.Select(x => x.PrettyContent()).ToArray();
				_category = 0;
				inspectRefresh = true;
			}

			var reset = false;
			var prevIndex = _category;
			GUILayout.BeginHorizontal();
			if (_selectedPlugin == null) {
				_menuScrollPosition = GUILayout.BeginScrollView(_menuScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(120));

				using (GuiEx.BackgroundColor(Color.green))
				using (new GUILayout.VerticalScope()) {
					var newIndex = GUILayout.SelectionGrid(prevIndex, _categories, 1, GUILayout.Width(100));
					reset = prevIndex != newIndex;
					if (reset) {
						_submenu = false;
						_category = newIndex;
						inspectRefresh = true;
					}
				}

				GUILayout.EndScrollView();
			}

			var plugins = _pluginsCategory.GetAt(Mathf.Clamp(_category, 0, _pluginsCategory.Count));
			if (reset) SelectPlugin(null);
			if (inspectRefresh) {
				foreach (var type in plugins.Select(data => data.OwnerType).Where(type => type != null).ToHashSet()) {
					InjectStaticData(type);
				}
			}

			using (new GUILayout.VerticalScope()) {
				if (_selectedPlugin != null) {
					if (DoHeader()) {
						try {
							_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
							_selectedPlugin.Arguments.DrawCheat(_searchQuery);
						}
						catch (Exception ex) {
							DoError(ex);
						}
						finally {
							GUILayout.FlexibleSpace();
							GUILayout.EndScrollView();
						}
					}
				}
				else {
					_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
					DoPluginList(plugins);
					GUILayout.FlexibleSpace();
					GUILayout.EndScrollView();
				}
			}

			using (GuiEx.Color(new Color(1, 0.3f, 0.3f))) {
				if (GUILayout.Button(new GUIContent($"x", "Close [Esc]"), GUILayout.Width(25))) Cheat.Minimize();
			}

			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(GUI.tooltip)) {
				using (GuiEx.ContentColor(Color.yellow)) {
					var size = GUI.skin.label.CalcSize(new GUIContent(GUI.tooltip)) + Vector2.one * 10;
					var offset = Event.current.mousePosition;

					if (offset.x + size.x > rect.width) offset.x = rect.width - size.x;
					if (offset.y + size.y > rect.height) offset.y = rect.height - size.y;

					using (GuiEx.BackgroundColor(Color.black)) {
						GUI.Box(new Rect(offset, size), GUI.tooltip, GUI.skin.button);
					}
				}
			}

			if (GUI.changed) {
				// change profile
			}

			GUI.enabled = true;

			CheckHotkeys();
		}

		private void InjectStaticData(Type type) {
			type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(info => info.GetCustomAttribute<InjectAttribute>() != null)
				.ForEach(info => {
					try {
						if (info is PropertyInfo propertyInfo && propertyInfo.CanWrite) {
							var value = _container.Resolve(propertyInfo.PropertyType);
							propertyInfo.SetValue(null, value);
						}
						else if (info is FieldInfo fieldInfo) {
							var value = _container.Resolve(fieldInfo.FieldType);
							fieldInfo.SetValue(null, value);
						}
						else if (info is MethodInfo methodInfo) {
							var args = methodInfo.GetParameters().Select(param => _container.Resolve(param.ParameterType)).ToArray();
							methodInfo.Invoke(null, args);
						}
					}
					catch (Exception ex) {
						Debug.LogException(ex);
					}
				});
		}

		private static void DoError(Exception ex) {
			using (GuiEx.Color(Color.red)) {
#if UNITY_EDITOR
				using (new GUILayout.HorizontalScope()) {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Copy")) {
						UnityEditor.EditorGUIUtility.systemCopyBuffer = ex.ToLog();
					}
				}
#endif
				GUILayout.TextArea($"{ex.Message}\n\n{ex.StackTrace}");

				GUILayout.Space(10);
				while (ex.InnerException != null) {
					ex = ex.InnerException;
					GUILayout.TextArea($"{ex.Message}\n\n{ex.StackTrace}");
				}
			}
		}

		private void CheckHotkeys() {
			var @event = Event.current;
			if (@event.KeyUp(KeyCode.Escape)) {
				((ICheatSystem)this).Minimize(false);
			}
		}

		public void Scroll(Vector2 delta) {
			_scrollPosition.y += delta.y;
			if (_scrollPosition.y < 0) _scrollPosition.y = 0;
		}

		private bool DoHeader() {
			using (new GUILayout.HorizontalScope()) {
				if (_stateStack.Count > 0) {
					if (GUILayout.Button("< Back")) {
						((ICheatSystem)this).PopState();
						return false;
					}
				}

				using (GuiEx.Color(Color.green)) {
					if (GUILayout.Button(_selectedPlugin.Category, GUI.skin.label)) {
						_selectedPlugin = null;
						_searchQuery = string.Empty;
						return false;
					}
				}

				GUILayout.Label($" > {_selectedPlugin.Name}");
				GUILayout.FlexibleSpace();
			}

			if (_selectedPlugin.Arguments.Iterations > 3) DoSearchQuery();
			return true;
		}

		private void DoPluginList(List<CheatPluginData> plugins) {
			if (DoSearchQuery() || _filteredPluginList == null) {
				IEnumerable<CheatPluginData> filteredPluginsEnum = _plugins;
				filteredPluginsEnum = (!_searchQuery.IsNullOrEmpty()
					? filteredPluginsEnum.Where(plugin => plugin.Name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
					: plugins).Where(data => !data.Hidden);

				_filteredPluginList = filteredPluginsEnum
					.Select(data => {
						data.ResolveArguments(_container);
						return data;
					})
					.OrderByDescending(data => data.Arguments.Priority)
					.ToList();
				_filteredPluginNames = null;
			}

			if (_filteredPluginNames == null) {
				_filteredPluginNames = _filteredPluginList.GroupBy(data => data.Category)
					.ToOrderedDictionary(pairs => pairs.Key, pairs =>
						new CheatCategoryList(pairs.Select(data => data.Caption)
							.ToArray(), pairs.ToArray()));
			}

			foreach (var filteredPluginName in _filteredPluginNames) {
				using (GuiEx.Color(Color.yellow)) GUILayout.Label(filteredPluginName.Key);

				var selected = GUILayout.SelectionGrid(-1, filteredPluginName.Value.Names, 2, GUI.skin.button);
				if (selected >= 0) {
					var pluginData = filteredPluginName.Value.Plugins.At(selected);
					if (pluginData.IsToggle) {
						pluginData.InvokeMethod(null);
						_filteredPluginNames = null;
						return;
					}
					else {
						SelectPlugin(pluginData);

						if (pluginData.Arguments.IsEmpty) return;
						_submenu = true;
						_searchQuery = string.Empty;
					}
				}
			}
		}

		private bool DoSearchQuery() {
			using (CheatGui.HorizontalGroup("Search")) return CheatGui.Input(_searchQuery, out _searchQuery, false);
		}

		public void DoHotkeyGui() {
			GUILayout.Label("Hotkeys");
			GUILayout.Label("Cheat panel: [1]");

			foreach (var cheatPluginData in _pluginsHotkeys) {
				GUILayout.Button(cheatPluginData.Value.Name);
			}
		}

		void ICheatSystem.SetCommand(string menuName, string searchQuery, object args) {
			var pluginData = _plugins.FirstOrDefault(data => data.FullName == menuName);
			if (pluginData == null) return;
			StoreCurrentState(args);
			_scrollPosition = Vector2.zero;
			SelectPlugin(pluginData);
			_filteredPluginList = null;
			_searchQuery = searchQuery ?? string.Empty;
			_category = _pluginsCategory.Keys.IndexOf(s => pluginData.Category == s);
		}

		void ICheatSystem.GetCurrentState(ref CheatStoreState state) {
			state ??= new CheatStoreState();
			state.Args = _stateStack.FirstOrDefault()?.Args;
			state.ScrollPosition = _scrollPosition;
			state.SelectedPlugin = _selectedPlugin;
			state.FilteredPluginList = _filteredPluginList;
			state.SearchQuery = _searchQuery;
			state.Category = _category;

		}

		private void StoreCurrentState(object args) {
			_stateStack.Push(new CheatStoreState() {
				Args = args,
				ScrollPosition = _scrollPosition,
				SelectedPlugin = _selectedPlugin,
				FilteredPluginList = _filteredPluginList,
				SearchQuery = _searchQuery,
				Category = _category,
			});
		}

		public object TryResolve(Type type) => _stateStack.FirstOrDefault(state => state.Args != null && type.IsInstanceOfType(state.Args))?.Args;

		void ICheatSystem.PopState() {
			var state = _stateStack.Pop();
			_scrollPosition = state.ScrollPosition;
			SelectPlugin(state.SelectedPlugin);
			_filteredPluginList = state.FilteredPluginList;
			_searchQuery = state.SearchQuery;
			_category = state.Category;
		}

		private void SelectPlugin(CheatPluginData pluginData) {
			_selectedPlugin = pluginData;
			_selectedPlugin?.ResolveArguments(_container);
			_filteredPluginList = null;
		}

		void ICheatSystem.Minimize(bool reset) {
			_filteredPluginList = null;
			if (reset) {
				_category = 0;
				_selectedPlugin = null;
			}

			_view.SetExpanded(false);
			foreach (var consoleListener in _container.ResolveAll(TypeOf<ICheatConsoleListener>.Raw).OfType<ICheatConsoleListener>()) {
				consoleListener.HideConsole();
			}
		}

		void ICheatSystem.ResetSelect() {
			_selectedPlugin = null;
		}

		void ICheatSystem.Maximize() {
			// update select plugin state
			SelectPlugin(_selectedPlugin);
			_view.SetExpanded(true);
			foreach (var consoleListener in _container.ResolveAll(TypeOf<ICheatConsoleListener>.Raw).OfType<ICheatConsoleListener>()) {
				consoleListener.ShowConsole();
			}
		}

		void ICheatSystem.SetHidden(bool hidden) {
			_cheatHidden.Value = hidden;
			_view.SetHidden(hidden);
		}

		void ILockableInternal.Unlock(LockInstance inst) => _locker.Unlock(inst);
		bool ILockableInternal.IsLocked => _locker.IsLocked;
		LockInstance ILockable.Lock() => _locker.Lock();
	}

}