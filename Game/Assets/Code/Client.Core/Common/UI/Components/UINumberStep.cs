using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Controls;

namespace Client.Core.Common.UI.Components {

	public struct StepData {
		public int Value;
		public int MinValue;
		public int MaxValue;
		public int Step;
		public Action<int> OnChange;
	}
	public class UINumberStep : UISimpleData<StepData> {
		[SerializeField, ChildGameObjectsOnly] private Button _prev;
		[SerializeField, ChildGameObjectsOnly] private Button _next;
		
		[SerializeField, ChildGameObjectsOnly] private TMP_Text _value;

		private void Awake() {
			if(_prev != null) _prev.onClick.AddListener(OnPrev);
			if(_next != null) _next.onClick.AddListener(OnNext);
		}

		private void OnNext() => ChangeValue(Mathf.Min(Data.Value + Data.Step, Data.MaxValue));
		private void OnPrev() => ChangeValue(Mathf.Max(Data.Value - Data.Step, Data.MinValue));

		private void ChangeValue(int value) {
			var data = Data;
			data.Value = value;
			base.SetData(data, Index);
			data.OnChange?.Invoke(value);
		}

		protected override void SetData(StepData data) => _value.text = data.Value.ToString();
	}

}