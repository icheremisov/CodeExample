using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	[ExecuteInEditMode]
	public class SyncTransform : MonoBehaviour {

		[SerializeField] private Transform _masterObject;
		public Transform MasterObject {
			get => _masterObject;
			set {
				_masterObject = value;
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

		private void UpdateView() {
			if (_masterObject == null) return;

			var tm = transform;

			if (_scale && _localScale) {
				var curScale = tm.localScale;
				var masterScale = _masterObject.localScale;
				tm.localScale = new Vector3(_syncXScale ? masterScale.x : curScale.x,
					_syncYScale ? masterScale.y : curScale.y,
					_syncZScale ? masterScale.z : curScale.z);
			}
			else if (_scale && !_localScale) {
				var curScale = tm.lossyScale;
				var masterScale = _masterObject.lossyScale;
				tm.localScale = tm.parent.InverseTransformVector(new Vector3(_syncXScale ? masterScale.x : curScale.x,
					_syncYScale ? masterScale.y : curScale.y,
					_syncZScale ? masterScale.z : curScale.z));
			}

			if (_rotation) {
				var curRot = tm.rotation.eulerAngles;
				var masterRot = _masterObject.rotation.eulerAngles;
				tm.rotation = Quaternion.Euler(_syncXRot ? masterRot.x : curRot.x, _syncYRot ? masterRot.y : curRot.y, _syncZRot ? masterRot.z : curRot.z);
			}

			if (_position) {
				var position = tm.position;
				var curPos = position;
				var masterPos = _masterObject.position;
				position = new Vector3(_syncXPos ? masterPos.x + _posGlobalOffset.x : curPos.x,
					_syncYPos ? masterPos.y + _posGlobalOffset.y : curPos.y,
					_syncZPos ? masterPos.z + _posGlobalOffset.z : curPos.z);

				position += tm.forward * _forwardOffset + tm.up * _upOffset + tm.right * _rightOffset;
				tm.position = position;
			}
		}

	}

}