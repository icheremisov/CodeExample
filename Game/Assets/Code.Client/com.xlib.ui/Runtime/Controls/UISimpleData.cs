using UnityEngine;

namespace XLib.UI.Controls {

	public abstract class UISimpleData<T> : MonoBehaviour, IDataView<T>, IDataGet<T> {
		private T _data;
		private int _index;
		private int _lastSelectIndex = -1;

		public T Data { get => _data; }
		public int Index => _index;
		public bool IsSelect => _lastSelectIndex == Index;

		public void SetData(T data, int index) {
			_data = data;
			_index = index;
			SetData(_data);
		}

		protected abstract void SetData(T data);

		void IDataSelect.SetSelectIndex(int selectElementIndex) {
			if (_lastSelectIndex == selectElementIndex) return;
			_lastSelectIndex = selectElementIndex;
			OnSelect(selectElementIndex == Index, selectElementIndex);
		}

		protected virtual void OnSelect(bool select, int selectElementIndex) { }
		public T GetData() => Data;
		public GameObject GetGameObject() => gameObject;
	}

	public abstract class UISimpleData<T, TArgs> : UISimpleData<T>, IDataInit<TArgs> {
		protected TArgs Args { get; private set; }
		public virtual void Setup(TArgs args) => Args = args;
	}

}