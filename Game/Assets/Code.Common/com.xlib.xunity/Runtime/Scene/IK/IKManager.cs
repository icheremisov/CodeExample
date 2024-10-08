using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene.IK {

	public class IKManager : MonoBehaviour {

		[SerializeField, Required] private IKJoint _start;
		[SerializeField, Required] private IKJoint _end;

		[SerializeField, Required] private GameObject _target;

		[SerializeField, Required] private float _targetOffset;

		[SerializeField, Required] private float _threshold = 0.05f;

		[SerializeField, Required] public float _rate = 5.0f;

		[SerializeField, Required] public int _steps = 20;

		private Transform _endTr;
		private Transform _targetTr;

		private void Awake() {
			_endTr = _end.transform;
			_targetTr = _target.transform;
		}

		private void Update() {
			for (var i = 0; i < _steps; ++i) {
				if (Vector3.Distance(_endTr.position + _endTr.transform.up * _targetOffset, _targetTr.position) > _threshold) {
					var current = _start;

					while (current != null) {
						var slope = CalculateSlope(current);
						current.Rotate(-slope * _rate);
						current = current.Child;
					}
				}
			}
		}

		private float CalculateSlope(IKJoint ikJoint) {
			var deltaTheta = 0.01f;

			var distance1 = Vector3.Distance(_endTr.position + _endTr.transform.up * _targetOffset, _targetTr.position);

			ikJoint.Rotate(deltaTheta);

			var distance2 = Vector3.Distance(_endTr.position + _endTr.transform.up * _targetOffset, _targetTr.position);

			ikJoint.Rotate(-deltaTheta);

			return (distance2 - distance1) / deltaTheta;
		}

	}

}