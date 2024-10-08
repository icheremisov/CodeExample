using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Core.CommonTypes;
using XLib.Core.RandGen;
using Random = UnityEngine.Random;

namespace XLib.Unity.Tools {

	internal class ArrangeItemWindow : EditorWindow {

		private readonly IRandom _random = new SystemRandom();

		public enum SortMode {

			X,
			Y,
			Z

		}

		private string _autoName = "obj";
		private Action<GameObject, float> _curAction = null;
		private float _deltaZ = 0.1f;

		private float _firstZ = 0;

		private float _fromZ = 0;
		private bool _invert = false;

		private bool _localSpace = true;

		private float _randomColorDelta = 0.5f;
		private RangeF _randomScale = new RangeF(0.5f, 1.5f);

		private Vector2 _scrollPos = new Vector2(0, 0);

		private Action<GameObject, float> _setX = null;
		private Action<GameObject, float> _setY = null;
		private Action<GameObject, float> _setZ = null;

		private SortMode _sortMode = SortMode.Z;
		private float _toZ = 10;

		public void OnGUI() {
			_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

			GUILayout.BeginVertical();

			GUILayout.Label("Random Color");
			_randomColorDelta = EditorGUILayout.Slider("Color variation", _randomColorDelta, 0.0f, 1.0f);
			if (GUILayout.Button("Randomize Selection")) {
				foreach (var obj in Selection.gameObjects) {
					RandomizeColor(obj, _randomColorDelta);
				}
			}

			GUILayout.Space(20);
			GUILayout.Label($"Random Scale: {_randomScale}");

			var min = _randomScale.min;
			var max = _randomScale.max;
			EditorGUILayout.MinMaxSlider(ref min, ref max, 0.0f, 10.0f);

			_randomScale.min = min;
			_randomScale.max = max;
			if (GUILayout.Button("Randomize Selection (XYZ)")) {
				foreach (var obj in Selection.gameObjects) {
					RandomizeScaleXYZ(obj.transform, _randomScale);
				}
			}

			if (GUILayout.Button("Randomize Selection (Y axis)")) {
				foreach (var obj in Selection.gameObjects) {
					RandomizeScaleY(obj.transform, _randomScale);
				}
			}

			GUILayout.Space(20);
			GUILayout.Label("Random Rotation");
			if (GUILayout.Button($"Randomize Selection (Axis {_sortMode})")) {
				foreach (var obj in Selection.gameObjects) {
					RandomizeRot(obj.transform, _sortMode);
				}
			}

			GUILayout.Space(20);

			_localSpace = EditorGUILayout.Toggle("Local space", _localSpace);
			_sortMode = (SortMode)EditorGUILayout.EnumPopup("Sort axis", _sortMode);
			GUILayout.Space(20);

			var canArrange = Selection.gameObjects.Length > 1;

			var objects = new GameObject[Selection.gameObjects.Length];

			Array.Copy(Selection.gameObjects, objects, Selection.gameObjects.Length);

			Array.Sort(objects, (a, b) => String.Compare(a.name, b.name));

			GUILayout.Label("Sort selection by name (in one parent)");

			var firstParent = objects.FirstOrDefault()?.transform?.parent;

			if (canArrange && objects.All(x => x.transform.parent == firstParent)) {
				GUILayout.Label("Selected objects:");
				if (GUILayout.Button("Sort by name:")) {
					foreach (var obj in objects) {
						obj.transform.parent = null;
					}

					foreach (var obj in objects) {
						obj.transform.parent = firstParent;
					}
				}

				if (GUILayout.Button("Sort by name (reverse):")) {
					foreach (var obj in objects) {
						obj.transform.parent = null;
					}

					foreach (var obj in objects.Reverse()) {
						obj.transform.parent = firstParent;
					}
				}

				foreach (var obj in objects) {
					GUILayout.Label(obj.name);
				}
			}

			GUILayout.Space(20);

			if (_curAction == null) {
				_setX = (GameObject obj, float z) => {
					if (_localSpace)
						obj.transform.localPosition = obj.transform.localPosition.To0YZ(z);
					else
						obj.transform.position = obj.transform.position.To0YZ(z);
				};
				_setY = (GameObject obj, float z) => {
					if (_localSpace)
						obj.transform.localPosition = obj.transform.localPosition.ToX0Z(z);
					else
						obj.transform.position = obj.transform.position.ToX0Z(z);
				};
				_setZ = (GameObject obj, float z) => {
					if (_localSpace)
						obj.transform.localPosition = obj.transform.localPosition.ToXY0(z);
					else
						obj.transform.position = obj.transform.position.ToXY0(z);
				};
			}

			switch (_sortMode) {
				case SortMode.X:
					_curAction = _setX;
					break;

				case SortMode.Y:
					_curAction = _setY;
					break;

				case SortMode.Z:
					_curAction = _setZ;
					break;
			}

			GUILayout.Label("Arrange " + _sortMode + " for selection (by name) - min/max");

			_fromZ = EditorGUILayout.FloatField("from " + _sortMode, _fromZ);
			_toZ = EditorGUILayout.FloatField("to " + _sortMode, _toZ);

			if (canArrange) {
				GUILayout.Label("Selected objects:");

				var c = objects.Length - 1;
				var dz = (_toZ - _fromZ) / c;
				var z = _fromZ;

				if (GUILayout.Button("Sort by from/to " + _sortMode)) {
					z = _fromZ;

					foreach (var obj in objects) {
						_curAction(obj, z);
						z += dz;
					}
				}

				z = _fromZ;
				foreach (var obj in objects) {
					GUILayout.Label(obj.name + (_localSpace ? " local." : "") + _sortMode + "=" + z);
					z += dz;
				}
			}

			GUILayout.Space(20);

			GUILayout.Label("Arrange " + _sortMode + " for selection (by name) - by delta");

			_firstZ = EditorGUILayout.FloatField("first " + _sortMode, _firstZ);
			_deltaZ = EditorGUILayout.FloatField("delta " + _sortMode, _deltaZ);

			if (canArrange) {
				GUILayout.Label("Selected objects:");

				var z = _firstZ;

				if (GUILayout.Button("Sort by delta" + _sortMode)) {
					z = _firstZ;

					foreach (var obj in objects) {
						_curAction(obj, z);
						z += _deltaZ;
					}
				}

				z = _firstZ;
				foreach (var obj in objects) {
					GUILayout.Label(obj.name + (_localSpace ? " local." : "") + _sortMode + "=" + z);
					z += _deltaZ;
				}
			}

			GUILayout.Space(20);

			GUILayout.Label("Autonames:");

			_autoName = EditorGUILayout.TextField("Prefix", _autoName);

			var canName = Selection.gameObjects.Length == 1 && _autoName.Length > 0;

			if (canName) {
				var obj = Selection.gameObjects[0];

				if (GUILayout.Button("Make first object")) {
					obj.name = _autoName + "01";
					var parent = obj.transform.parent;

					if (parent != null) {
						var c = parent.childCount;
						for (var i = 0; i < c; ++i) {
							var child = parent.GetChild(i);
							if (child.gameObject == obj && i < c - 1) {
								Selection.activeGameObject = parent.GetChild(i + 1).gameObject;
								break;
							}
						}
					}
				}

				if (GUILayout.Button("Make next object")) {
					var parent = obj.transform.parent;

					if (parent != null) {
						var nextNum = 1;
						var c = parent.childCount;

						for (var i = 0; i < c; ++i) {
							var child = parent.GetChild(i);
							if (!child.name.StartsWith(_autoName)) continue;

							var snum = child.name.Substring(_autoName.Length, child.name.Length - _autoName.Length);

							var n = 0;
							if (Int32.TryParse(snum, out n)) nextNum = Math.Max(n + 1, nextNum);
						}

						obj.name = _autoName + nextNum.ToString("D2");

						for (var i = 0; i < c; ++i) {
							var child = parent.GetChild(i);
							if (child.gameObject == obj && i < c - 1) {
								Selection.activeGameObject = parent.GetChild(i + 1).gameObject;
								break;
							}
						}
					}
					else {
						obj.name = _autoName + "01";
					}
				}
			}

			GUILayout.Space(20);

			GUILayout.Label("Order of children:");

			if (GUILayout.Button("Flip order of children")) {
				var parent = Selection.gameObjects[0].transform;
				var children = parent.AsEnumerable().OrderByDescending(x => x.GetSiblingIndex()).ToArray();

				for (var i = 0; i < children.Length; ++i) {
					children[i].SetSiblingIndex(i);
				}
			}

			if (GUILayout.Button("Sort A-Z")) {
				var parent = Selection.gameObjects[0].transform;
				var children = parent.AsEnumerable().OrderBy(x => x.name).ToArray();

				for (var i = 0; i < children.Length; ++i) {
					children[i].SetSiblingIndex(i);
				}
			}

			if (GUILayout.Button("Sort Z-A")) {
				var parent = Selection.gameObjects[0].transform;
				var children = parent.AsEnumerable().OrderByDescending(x => x.name).ToArray();

				for (var i = 0; i < children.Length; ++i) {
					children[i].SetSiblingIndex(i);
				}
			}

			GUILayout.Space(5);

			_invert = GUILayout.Toggle(_invert, "Invert order");

			if (GUILayout.Button($"Sort by axis: {_sortMode}")) {
				var parent = Selection.gameObjects[0].transform;

				Func<Transform, float> sort;

				if (_localSpace) {
					switch (_sortMode) {
						case SortMode.X:
							sort = x => x.localPosition.x;
							break;

						case SortMode.Y:
							sort = x => x.localPosition.y;
							break;

						case SortMode.Z:
							sort = x => x.localPosition.z;
							break;

						default: throw new ArgumentOutOfRangeException();
					}
				}
				else {
					switch (_sortMode) {
						case SortMode.X:
							sort = x => x.position.x;
							break;

						case SortMode.Y:
							sort = x => x.position.y;
							break;

						case SortMode.Z:
							sort = x => x.position.z;
							break;

						default: throw new ArgumentOutOfRangeException();
					}
				}

				var children = _invert ? parent.AsEnumerable().OrderByDescending(sort).ToArray() : parent.AsEnumerable().OrderBy(sort).ToArray();

				for (var i = 0; i < children.Length; ++i) {
					children[i].SetSiblingIndex(i);
				}
			}

			GUILayout.Space(20);

			GUILayout.Label("Flip selection:");
			if (GUILayout.Button("Axis X")) {
				foreach (var obj in Selection.gameObjects) {
					if (_localSpace) {
						var p = obj.transform.localPosition;
						obj.transform.localPosition = p.To0YZ(-p.x);
					}
					else {
						var p = obj.transform.position;
						obj.transform.position = p.To0YZ(-p.x);
					}
				}
			}

			if (GUILayout.Button("Axis Y")) {
				foreach (var obj in Selection.gameObjects) {
					if (_localSpace) {
						var p = obj.transform.localPosition;
						obj.transform.localPosition = p.ToX0Z(-p.y);
					}
					else {
						var p = obj.transform.position;
						obj.transform.position = p.ToX0Z(-p.y);
					}
				}
			}

			if (GUILayout.Button("Axis Z")) {
				foreach (var obj in Selection.gameObjects) {
					if (_localSpace) {
						var p = obj.transform.localPosition;
						obj.transform.localPosition = p.ToXY0(-p.z);
					}
					else {
						var p = obj.transform.position;
						obj.transform.position = p.ToXY0(-p.z);
					}
				}
			}

			GUILayout.EndVertical();

			GUILayout.EndScrollView();
		}

		public void OnSelectionChange() {
			this.Repaint();
		}

		[MenuItem("Tools/Arrange Objects", false, 1000)]
		public static void ArrangeObjects() {
			GetWindow(typeof(ArrangeItemWindow), false, "Arrange objects");
		}

		private void RandomizeColor(GameObject obj, float delta = 0.2f) {
			delta = Mathf.Clamp01(delta);

			foreach (var c in obj.GetComponents<Component>()) {
				var so = new SerializedObject(c);
				var colorProp = so.FindProperty("_color");

				if (colorProp == null) {
					continue;
				}

				var rand = 1.0f - Random.Range(0.0f, delta);
				colorProp.SetValue(new Color(rand, rand, rand, 1.0f));
				so.ApplyModifiedProperties();
				EditorUtility.SetDirty(c);
			}
		}

		private void RandomizeRot(Transform obj, SortMode axis) {
			var rand = Random.Range(0.0f, 360.0f);

			switch (axis) {
				case SortMode.X:
					obj.localRotation = Quaternion.AngleAxis(rand, Vector3.right);
					break;

				case SortMode.Y:
					obj.localRotation = Quaternion.AngleAxis(rand, Vector3.up);
					break;

				case SortMode.Z:
					obj.localRotation = Quaternion.AngleAxis(rand, Vector3.forward);
					break;

				default: throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
			}
		}

		private void RandomizeScaleXYZ(Transform obj, RangeF delta) {
			var rand = delta.GetRandom(_random);
			obj.SetLocalScale(rand);
		}

		private void RandomizeScaleY(Transform obj, RangeF delta) {
			var rand = delta.GetRandom(_random);
			obj.localScale = new Vector3(1.0f, rand, 1.0f);
		}

	}

}