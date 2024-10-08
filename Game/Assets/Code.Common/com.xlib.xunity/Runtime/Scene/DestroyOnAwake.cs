using UnityEngine;

namespace XLib.Unity.Scene {

	public class DestroyOnAwake : MonoBehaviour {

		public void Awake() {
			if (enabled) Destroy(gameObject);
		}

#if UNITY_EDITOR
		// ReSharper disable once Unity.RedundantEventFunction
		public void Start() {
			// dont remove this!
		}
#endif

	}

}