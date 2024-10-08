using UnityEngine;

namespace XLib.Unity.Scene {

	public class DontDestroyObject : MonoBehaviour {

		public void Awake() {
			if (transform.parent == null) DontDestroyOnLoad(gameObject);
		}

	}

}