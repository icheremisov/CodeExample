using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace XLib.Unity.Scene {

	[ExecuteInEditMode]
	public class SyncTransformRef : MonoBehaviour {

		[SerializeField] private Transform _slaveObject;
		public Transform SlaveObject {
			get => _slaveObject;
			set {
				_slaveObject = value;
				UpdateView();
			}
		}

		[SerializeField, Required] private Vector3 _posGlobalOffset;

		[SerializeField, Required] private float _rightOffset;
		[SerializeField, Required] private float _upOffset;
		[SerializeField, Required] private float _forwardOffset;

		[SerializeField] private bool _position = true;
		[ShowIf(nameof(_position)), SerializeField] private bool _syncXPos = true;
		[ShowIf(nameof(_position)), SerializeField] private bool _syncYPos = true;
		[ShowIf(nameof(_position)), SerializeField] private bool _syncZPos = true;

		[SerializeField] private bool _rotation = true;
		[ShowIf(nameof(_rotation)), SerializeField] private bool _syncXRot;
		[ShowIf(nameof(_rotation)), SerializeField] private bool _syncYRot;
		[ShowIf(nameof(_rotation)), SerializeField] private bool _syncZRot;

		[SerializeField] private bool _scale;
		[ShowIf(nameof(_scale)), SerializeField] private bool _localScale = true;
		[ShowIf(nameof(_scale)), SerializeField] private bool _syncXScale = true;
		[ShowIf(nameof(_scale)), SerializeField] private bool _syncYScale = true;
		[ShowIf(nameof(_scale)), SerializeField] private bool _syncZScale = true;

		private void LateUpdate() {
			UpdateView();
		}

		private void OnEnable() {
			UpdateView();
		}

		public void ForceUpdate() {
			UpdateView();
		}

		private void UpdateView() {
			if (_slaveObject == null) return;

			var tm = transform;

			if (_scale && _localScale) {
				var curScale = tm.localScale;
				var otherScale = _slaveObject.localScale;
				_slaveObject.localScale = new Vector3(_syncXScale ? curScale.x : otherScale.x,
					_syncYScale ? curScale.y : otherScale.y,
					_syncZScale ? curScale.z : otherScale.z);
			}
			else if (_scale && !_localScale) {
				var curScale = tm.lossyScale;
				var otherScale = _slaveObject.lossyScale;
				_slaveObject.localScale = _slaveObject.parent.InverseTransformVector(new Vector3(_syncXScale ? curScale.x : otherScale.x,
					_syncYScale ? curScale.y : otherScale.y,
					_syncZScale ? curScale.z : otherScale.z));
			}
			
			if (_rotation) {
				var curRot = tm.rotation.eulerAngles;
				var otherRot = _slaveObject.rotation.eulerAngles;
				_slaveObject.rotation = Quaternion.Euler(_syncXRot ? curRot.x : otherRot.x, _syncYRot ? curRot.y : otherRot.y, _syncZRot ? curRot.z : otherRot.z);
			}

			if (_position) {
				var position = tm.position;
				var curPos = position;
				var otherPos = _slaveObject.position;
				position = new Vector3(_syncXPos ? curPos.x + _posGlobalOffset.x : otherPos.x,
					_syncYPos ? curPos.y + _posGlobalOffset.y : otherPos.y,
					_syncZPos ? curPos.z + _posGlobalOffset.z : otherPos.z);

				var otherTm = _slaveObject.transform;
				position += otherTm.forward * _forwardOffset + otherTm.up * _upOffset + otherTm.right * _rightOffset;
				otherTm.position = position;
			}
		}

	}

}