using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLib.UI.Controls;

namespace Client.Core.Common.UI.Components {

	[Serializable]
	public class UITabs<TData> : UISimpleList<UITabView, object> {
		private Action<TData> _onChange;
		private Action<TData> _onBeforeChange;

		public void Setup(Func<TData, (string name, Sprite image)> callback, Action<TData> onChange) {
			this.Setup(new UITabView.Params { Callback = (i, o) => callback((TData)o), OnClick = SetSelect });
			_onChange = onChange;
		}

		public void SetupOnClick(Func<TData, (string name, Sprite image)> callback, Action<TData> onChange) {
			this.Setup(new UITabView.Params { Callback = (i, o) => callback((TData)o), OnClick = OnClick });
			_onBeforeChange = onChange;
		}

		public void SetData(IEnumerable<TData> dataCollection) => SetData(dataCollection.OfType<object>());

		public void SetSelect(TData data) => SetSelect(FindIndex(view => view.Data.Equals(data)));

		public override void SetSelect(int index = -1) {
			base.SetSelect(index);
			_onChange?.Invoke((TData)GetElement(index)?.Data);
		}

		public void OnClick(int index = -1) {
			_onBeforeChange?.Invoke((TData)GetElement(index)?.Data);
		}
	}

	public class UITabView : UISimpleData<object, UITabView.Params>, IPointerClickHandler {
		[SerializeField, ChildGameObjectsOnly] private TMP_Text _name;
		[SerializeField, ChildGameObjectsOnly] private TMP_Text[] _names;
		[SerializeField, ChildGameObjectsOnly] private Image _image;
		[SerializeField, ChildGameObjectsOnly] private GameObject[] _selectable;
		[SerializeField, ChildGameObjectsOnly] private GameObject[] _unselectable;

		public class Params {
			public Func<int, object, (string name, Sprite image)> Callback { get; set; }
			public Action<int> OnClick { get; set; }
		}

		public void SetTextAlignment(TextAlignmentOptions alignmentOptions) => _name.alignment = alignmentOptions;
		public void SetTextColor(Color textColor) => _name.color = textColor;
		public void SetTextAlpha(float a) => _name.alpha = a;

		protected override void SetData(object data) {
			(var nm, var image) = Args.Callback(Index, data);
			if (_name != null) _name.text = nm;
			_names?.ForEach(x => x.text = nm);
			if (_image == null) return;
			if (image != null) {
				_image.sprite = image;
				_image.enabled = true;
			}
			else
				_image.enabled = false;
		}

		protected override void OnSelect(bool select, int selectElementIndex) {
			foreach (var o in _selectable) {
				if (o != null) o.SetActive(select);
			}

			foreach (var o in _unselectable) {
				if (o != null) o.SetActive(!select);
			}
		}

		public void OnPointerClick(PointerEventData eventData) => Args?.OnClick(Index);
	}

}