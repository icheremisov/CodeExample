using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XLib.UI.Animation {

	public abstract class UIAnimationSavableValues : MonoBehaviour {
		private class Values {
			public Vector3 StartPosition;
			public Quaternion StartRotation;
			public Vector3 StartScale;
			public float StartAlpha;
		}

		private readonly Dictionary<GameObject, Values> _savedValues = new();

		protected void SaveStartValues(GameObject go) {
			if (go == null || _savedValues.ContainsKey(go)) return;

			var values = new Values();
			
			var tr = go.transform;
			values.StartPosition = tr.localPosition;
			values.StartRotation = tr.localRotation;
			values.StartScale = tr.localScale;

			var canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup != null) values.StartAlpha = canvasGroup.alpha;
			
			_savedValues.Add(go, values);
		}

		protected void SaveStartValues(IEnumerable<UIAnimationPart> parts) =>
			parts.Select(part => part.OverrideTarget ? part.Target : gameObject).Distinct().ForEach(SaveStartValues);

		protected void ApplyStartValues(GameObject go) {
			if (go == null || !_savedValues.TryGetValue(go, out var values)) return;
			
			var tr = go.transform;
			tr.localPosition = values.StartPosition;
			tr.localRotation = values.StartRotation;
			tr.localScale = values.StartScale;
			
			var canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup != null) canvasGroup.alpha = values.StartAlpha;
		}

		protected void ApplyStartValues(IEnumerable<UIAnimationPart> parts) =>
			parts.Select(part => part.OverrideTarget ? part.Target : gameObject).Distinct().ForEach(ApplyStartValues);
		
		public abstract void SaveStartValues();
		public abstract void ApplyStartValues();
	}

}