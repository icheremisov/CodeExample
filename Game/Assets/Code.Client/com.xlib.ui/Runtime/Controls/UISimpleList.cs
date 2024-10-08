using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using XLib.Core.Utils;
using XLib.Core.Utils.Attributes;
using XLib.UI.Contracts;
using XLib.UI.Controls;
using Object = UnityEngine.Object;

namespace XLib.UI.Controls {

	public interface IDataSelect {
		void SetSelectIndex(int selectElementIndex) { }
	}

	public interface IDataView { }

	public interface IDataGet<out T> {
		T GetData();
		GameObject GetGameObject();
	}

	public interface IDataView<in T> : IDataView, IDataSelect {
		void SetData(T data, int index = default);
	}

	public interface IDataInit<in T> {
		void Setup(T args);
	}

	public interface IListObserver {
		void Register(IViewObservable listener);
		void Unregister(IViewObservable listener);
		void DispatchListeners();
	}

	public interface IViewObservable {
		public string Id { get; }
		void Dispatch(object view);
	}

	public interface IViewObservable<in TView> : IViewObservable where TView : class {
		void IViewObservable.Dispatch(object view) {
			var obj = view as TView;
			if (obj != null)
				this.Dispatch(obj);
			else
				throw new InvalidCastException("Type " + view.GetType().Name + " not implement " + TypeOf<TView>.FullName);
		}

		void Dispatch(TView view);
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UIListFilterSettingsAttribute : Attribute {
		public Type TypeFilter { get; }
		public bool WithIndex { get; }

		public UIListFilterSettingsAttribute(Type typeFilter, bool withIndex) {
			TypeFilter = typeFilter;
			WithIndex = withIndex;
		}
	}

	public interface IUIListFilter {
		public GameObject FindElement(IUIList list, Object data, int index);

		public GameObject FindElement(IList list, Object data, int index) {
			if (list is IUIList uilist) return FindElement(uilist, data, index);
			return null;
		}
	}

	[UIListFilterSettings(null, true), TypeId("19149962-5302-4BD3-B7C0-2EE953366449"), UsedImplicitly]
	public class IndexFilter : IUIListFilter {
		public GameObject FindElement(IUIList list, Object data, int index) => list.GetElement(index).ToGameObject();

		public GameObject FindElement(IList list, Object data, int index) {
			if (index >= 0 && index < list.Count) return (list[index] as Component).ToGameObject();
			return null;
		}
	}

	public interface IUIList {
		Type ViewType { get; }
		Type DataType { get; }

		public int Count { get; }
		public MonoBehaviour GetElement(int index);

		public IEnumerable<MonoBehaviour> Elements();
	}

	public interface IListItemFactory {
		MonoBehaviour Instantiate(MonoBehaviour prefab, Transform root);
	}

	public interface IUIListWithFactory : IUIList {
		void SetItemFactory(IListItemFactory factory);
	}

	[Serializable, InlineProperty, HideLabel, BoxGroup]
	public class UISimpleList<TView, TData> : IListObserver, IUIListWithFactory where TView : MonoBehaviour, IDataView<TData> {
		[SerializeField, Required, LabelWidth(150), RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance), SceneObjectsOnly, InlineProperty,
		 LabelText("@$property.Parent.NiceName + \": Cell\""), Tooltip("List cell prefab")] // LabelWidth(50), LabelText("Prefab"), HorizontalGroup, 
		protected TView _view;
		[SerializeField, SceneObjectsOnly, Indent(3), Tooltip("Prefab separator between cells")]
		protected GameObject _separator;
		[SerializeField, Indent(5), ShowIf("@_separator != null")]
		private bool _showSeparatorBeforeSelected = true;
		[SerializeField, Indent(5), ShowIf("@_separator != null")]
		private bool _showSeparatorAfterSelected = true;
		[SerializeField, Indent(5), ShowIf("@_separator != null"), Tooltip("Separator should have CanvasGroup component")]
		private bool _switchOnlyCanvasGroupAlpha = false;
		[SerializeField, Indent(3), SceneObjectsOnly, Tooltip("Object shown when the list is empty")]
		protected GameObject _emptyView;

		// Помечено как SerializeField чтобы бехавиор с SimpleList можно было копировать через Instantiate()
		[SerializeField, HideInInspector] protected List<TView> _instances = new();
		[SerializeField, HideInInspector] protected List<GameObject> _separators = new() { null };
		[SerializeField, HideInInspector] protected List<CanvasGroup> _separatorsCanvasGroups = new() { null };
		[SerializeField, HideInInspector] private int _curLength = 0;
		private Transform _container;
		private int _selectIndex = -1;

		private IListItemFactory _itemFactory;

		private Dictionary<string, IViewObservable> _listeners = new();

		void IListObserver.Register(IViewObservable listener) {
			Init();
			if (listener != null) {
				_listeners[listener.Id] = listener;
				foreach (var element in Elements()) listener.Dispatch(element);
			}
		}

		void IListObserver.Unregister(IViewObservable listener) => _listeners.Remove(listener.Id);

		void IListObserver.DispatchListeners() {
			foreach (var listener in _listeners.Values) {
				foreach (var element in Elements()) listener.Dispatch(element);
			}
		}

		private bool _inited;

		public UISimpleList() { }

		public UISimpleList(TView prefab, GameObject separator = null) {
			this._view = prefab;
			this._separator = separator;
		}

		public TView GetPrefab() {
			Init();
			return _view;
		}

		public int ItemsCount => _curLength;

		private void Awake() => Init();

		public Transform Container {
			get {
				Init();
				return _container;
			}
		}

		public bool IsValid => Container != null;

		private void Init() {
			if (_inited) return;
			_inited = true;
			OnInit();
		}

		protected virtual void OnInit() {
			if (_view == null) return;
			_container = _view.transform.parent;
			_view.gameObject.SetActive(false);
		}

		public int Count => _curLength;
		IEnumerable<MonoBehaviour> IUIList.Elements() => Elements();

		public void SetItemFactory(IListItemFactory factory) => _itemFactory = factory;

		MonoBehaviour IUIList.GetElement(int index) => _curLength == 0 ? _view : GetElement(index);
		Type IUIList.ViewType => TypeOf<TView>.Raw;
		Type IUIList.DataType => TypeOf<TData>.Raw;

		public TView GetElement(int index) => _instances.GetOrDefault(index);

		public IEnumerable<TView> Elements() => _instances.Take(_curLength);

		public TView Find(Predicate<TView> predicate) {
			for (var i = 0; i < _curLength; i++) {
				var component = _instances[i];
				if (component != null && predicate(component)) return component;
			}

			return null;
		}

		public int FindIndex(Predicate<TView> predicate) {
			for (var i = 0; i < _curLength; i++) {
				var component = _instances[i];
				if (component != null && predicate(component)) return i;
			}

			return -1;
		}

		public void Clear() {
			for (var i = 0; i < _curLength; i++) _instances[i].gameObject.SetActive(false);
			_curLength = 0;
			_selectIndex = -1;
			if (_emptyView != null) _emptyView.SetActive(true);
		}

		public void Reset() {
			Clear();

			foreach (var instance in _instances) {
				if (instance) GameObject.Destroy(instance.gameObject);
			}

			foreach (var s in _separators) {
				if (s) GameObject.Destroy(s.gameObject);
			}

			_instances.Clear();

			_separators.Clear();
			_separators.Add(null);

			_separatorsCanvasGroups.Clear();
			_separatorsCanvasGroups.Add(null);
		}

		protected virtual TView Instantiate(int index) {
			var obj = _itemFactory != null ? (TView)_itemFactory.Instantiate(_view, _container) : Object.Instantiate(_view, _container);
			obj.gameObject.name = $"{_view.name} #{index}";
			obj.SetSelectIndex(_selectIndex);
			foreach (var listener in _listeners.Values) listener.Dispatch(obj);
			return obj;
		}

		public void SetData(IEnumerable<TData> dataCollection) {
			Init();
			if (!IsValid) return;

			var i = 0;
			if (dataCollection != null) {
				foreach (var data in dataCollection) {
					var element = i < _instances.Count ? _instances[i] : AddElement(i);
					element.gameObject.SetActive(true);
					element.SetData(data, i);
					element.SetSelectIndex(_selectIndex);
					i++;
				}
			}

			var newLength = i;
			for (; i < _curLength; i++) _instances[i].gameObject.SetActive(false);

			UpdateSeparators(newLength);

			_curLength = newLength;
			if (_emptyView != null) _emptyView.SetActive(_curLength <= 0);
		}

		private void UpdateSeparators(int length) {
			if (!_separator) return;
			var max = Mathf.Max(length, _curLength);
			for (var j = 1; j < max; j++) {
				var isActive = j < length && !((!_showSeparatorBeforeSelected && _selectIndex >= 0 && _selectIndex == j) ||
					(!_showSeparatorAfterSelected && _selectIndex >= 0 && _selectIndex == j - 1));

				if (_switchOnlyCanvasGroupAlpha) {
					_separators[j].GetComponent<CanvasGroup>().alpha = isActive ? 1.0f : 0.0f;
					_separators[j].SetActive(j < length);
				}
				else
					_separators[j].SetActive(isActive);
			}
		}

		public virtual void SetSelect(int index = -1) {
			_selectIndex = index;
			for (var i = 0; i < _curLength; ++i) _instances[i].SetSelectIndex(_selectIndex);
			UpdateSeparators(_curLength);
		}

		public int SelectIndex => _selectIndex;

		private TView AddElement(int index) {
			if (_instances.Count == _separators.Count && _separator) {
				_separators.Add(Object.Instantiate(_separator, _container));
				if (_switchOnlyCanvasGroupAlpha) _separatorsCanvasGroups.Add(_separators.Last().GetComponent<CanvasGroup>());
			}

			var element = Instantiate(index);
			_instances.Add(element);
			return element;
		}

		public int IndexOf(TView comp) => _instances.IndexOf(comp);
		public int IndexOf(GameObject go) => _instances.FindIndex(x => x.gameObject == go);

		public void ScrollTo(RectTransform item, bool centered = false, float duration = 0.0f) =>
			_container.GetComponentInParent<ScrollRect>()?.ScrollToView(item, centered, duration);

		public void ScrollTo(int i, bool centered = false, float duration = 0.0f) =>
			_container.GetComponentInParent<ScrollRect>()?.ScrollToView(_instances.At(i).transform as RectTransform, centered, duration);

		public void ScrollTo(float p) {
			var sr = _container.GetComponentInParent<ScrollRect>();
			if (sr == null) return;

			if (sr.horizontal) sr.horizontalNormalizedPosition = p;

			if (sr.vertical) sr.verticalNormalizedPosition = p;
		}

		public override string ToString() {
			return $"{GetPrefab()} len:{_curLength}";
		}
	}

	[Serializable]
	public class UICountList<TView> : UISimpleList<TView, int> where TView : MonoBehaviour, IDataView<int> {
		public void SetCount(int active, int total) => SetData(Enumerable.Repeat(Mathf.Clamp(active, 0, total), total));
	}

	[Serializable]
	public class UIDoubleCountList<TView> : UISimpleList<TView, (int, int)> where TView : MonoBehaviour, IDataView<(int, int)> {
		public void SetCount((int, int) active, int total) => SetData(Enumerable.Repeat((Mathf.Clamp(active.Item1, 0, total), Mathf.Clamp(active.Item2, 0, total)), total));
	}

	public class ScreenListItemFactory : IListItemFactory {
		private readonly IScreenManager _screenManager;

		public ScreenListItemFactory(IScreenManager screenManager) {
			_screenManager = screenManager;
		}

		public MonoBehaviour Instantiate(MonoBehaviour prefab, Transform root) => _screenManager.InstantiateUIPrefab(prefab, root);
	}

}

public static class UISimpleListExtensions {
	private struct SimpleData<TArg> : IViewObservable<IDataInit<TArg>> {
		private TArg _arg;
		public string Id { get; }

		public SimpleData(TArg arg) : this() {
			_arg = arg;
			Id = TypeOf<TArg>.Name;
		}

		public void Dispatch(IDataInit<TArg> view) => view.Setup(_arg);
	}

	public static void Setup(this IListObserver list) => list.Register(null);
	public static void Setup<TArg>(this IListObserver list, TArg arg) => list.Register(new SimpleData<TArg>(arg));
	public static void AddListener(this IListObserver list, IViewObservable listener) => list.Register(listener);
	public static void RemoveListener(this IListObserver list, IViewObservable listener) => list.Unregister(listener);
	public static void DispatchListeners(this IListObserver list) => list.DispatchListeners();
	public static void SetItemFactory(this IUIListWithFactory list, IScreenManager screenManager) => list.SetItemFactory(new ScreenListItemFactory(screenManager));
}