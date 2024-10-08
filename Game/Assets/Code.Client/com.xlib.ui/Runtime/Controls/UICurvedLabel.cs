// using BezierSolution;
// using Sirenix.OdinInspector;
// using TMPro;
// using UnityEngine;
//
// namespace XLib.UI.Controls {
//
// 	/// <summary>
// 	///     align text to spline
// 	/// </summary>
// 	[ExecuteInEditMode, RequireComponent(typeof(TMP_Text))]
// 	public class UICurvedLabel : MonoBehaviour {
//
// 		[SerializeField] private BezierSpline _spline;
//
// 		private TMP_Text _text;
// 		private Transform _transform;
//
// 		private void Awake() {
// 			_transform = transform;
// 		}
//
// #if UNITY_EDITOR
// 		private void LateUpdate() {
// 			if (!Application.isPlaying) ForceUpdate();
// 		}
// #endif
//
// 		private void OnEnable() {
// 			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
// 			if (!_text) _text = this.GetExistingComponent<TMP_Text>();
//
// 			ForceUpdate();
// 		}
//
// 		private void OnDisable() {
// 			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
// 		}
//
// 		private void ON_TEXT_CHANGED(Object obj) {
// 			if (obj == _text) UpdateTextPosition();
// 		}
//
// 		private void UpdateTextPosition() {
// 			if (!_spline) return;
//
// 			if (!_text) _text = this.GetExistingComponent<TMP_Text>();
//
// 			var textInfo = _text.textInfo;
// 			var characterCount = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);
//
// 			if (characterCount == 0) return;
//
// #if UNITY_EDITOR
// 			if (!Application.isPlaying) _spline.Refresh();
// #endif
//
// 			for (var i = 0; i < characterCount; i++) {
// 				if (!textInfo.characterInfo[i].isVisible) continue;
//
// 				var vertexIndex = textInfo.characterInfo[i].vertexIndex;
//
// 				// Get the index of the mesh used by this character.
// 				var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
//
// 				var vertices = textInfo.meshInfo[materialIndex].vertices;
//
// 				// Compute the baseline mid point for each character
// 				Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
// 				//float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);
//
// 				// Apply offset to adjust our pivot point.
// 				vertices[vertexIndex + 0] += -offsetToMidBaseline;
// 				vertices[vertexIndex + 1] += -offsetToMidBaseline;
// 				vertices[vertexIndex + 2] += -offsetToMidBaseline;
// 				vertices[vertexIndex + 3] += -offsetToMidBaseline;
//
// 				// Compute the angle of rotation for each character based on the animation curve
// 				var x0 = offsetToMidBaseline.x;
// 				var x1 = x0 + 10;
// 				var y0 = CalcLocalY(x0);
// 				var y1 = CalcLocalY(x1);
//
// 				var horizontal = new Vector3(1, 0, 0);
//
// 				var tangent = new Vector3(x1, y1) - new Vector3(x0, y0);
//
// 				var dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
// 				var cross = Vector3.Cross(horizontal, tangent);
// 				var angle = cross.z > 0 ? dot : 360 - dot;
//
// 				var matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);
//
// 				vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
// 				vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
// 				vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
// 				vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
//
// 				vertices[vertexIndex + 0] += offsetToMidBaseline;
// 				vertices[vertexIndex + 1] += offsetToMidBaseline;
// 				vertices[vertexIndex + 2] += offsetToMidBaseline;
// 				vertices[vertexIndex + 3] += offsetToMidBaseline;
// 			}
//
// 			// Upload the mesh with the revised information
// 			_text.UpdateVertexData();
// 		}
//
// 		private float CalcLocalY(float localX) {
// 			var p = _transform.TransformPoint(new Vector3(localX, 0, 0));
// 			p = _spline.FindNearestPointTo(p);
// 			p = _transform.InverseTransformPoint(p);
// 			return p.y;
// 		}
//
// 		[Button]
// 		public void ForceUpdate() {
// 			if (_text == null) return;
//
// 			_text.havePropertiesChanged = true;
// 			_text.ForceMeshUpdate();
// 		}
//
// 	}
//
// }