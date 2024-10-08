using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	[RequireComponent(typeof(Camera))]
	public class TransparencySortMode : MonoBehaviour {

		[SerializeField, Required, OnValueChanged(nameof(ApplyParameters))] 
		private UnityEngine.TransparencySortMode _sortMode = UnityEngine.TransparencySortMode.Default;
		
		[SerializeField, Required, OnValueChanged(nameof(ApplyParameters))]
		[ShowIf("@_sortMode == UnityEngine.TransparencySortMode.CustomAxis")]
		private Vector3 _sortAxis = Vector3.forward;
		
		private void OnEnable() {
			ApplyParameters();
		}

		private void ApplyParameters() {
			var cam = GetComponent<Camera>();
			if (!cam) return;

			var axis = _sortAxis.magnitude <= 0.001f ? Vector3.forward : _sortAxis; 
			
			cam.transparencySortAxis = axis;
			cam.transparencySortMode = _sortMode;
		}
	}

}