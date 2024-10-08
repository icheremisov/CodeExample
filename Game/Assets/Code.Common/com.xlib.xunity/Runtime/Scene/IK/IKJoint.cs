using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene.IK {

	public class IKJoint : MonoBehaviour {

		[SerializeField, Required] private IKJoint _child;

		private Vector3 _offset;

		public IKJoint Child => _child;

		public void Rotate(float angle) {
			transform.Rotate(Vector3.up * angle);
		}

	}

}