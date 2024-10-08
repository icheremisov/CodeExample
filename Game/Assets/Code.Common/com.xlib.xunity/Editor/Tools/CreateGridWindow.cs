using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Unity.Tools {

	internal class CreateGridWindow : EditorWindow {

		private Vector2 _dimensions = new(5, 5);
		private float _heightY;
		private Object _prefab;
		private float _scale = 1;
		private float _size = 1;

		public void OnGUI() {
			_prefab = EditorGUILayout.ObjectField("Prefab", _prefab, TypeOf<GameObject>.Raw, false);
			_size = EditorGUILayout.FloatField("Size", _size);
			_scale = EditorGUILayout.FloatField("Scale multiplier", _scale);
			_heightY = EditorGUILayout.FloatField("Position height (Y)", _heightY);
			_dimensions = EditorGUILayout.Vector2Field("Dimensions", _dimensions);

			if (GUILayout.Button("Create")) CreateGrid();

			GUILayout.Space(20);

			if (GUILayout.Button("Layout Selection")) LinkSelectionToGrid();
		}

		public void OnSelectionChange() {
			Repaint();
		}

		[MenuItem("Tools/Create Grid", false, 1000)]
		public static void CreateGridMenu() {
			show();
		}

		public static void show() {
			GetWindow(typeof(CreateGridWindow), false, "Create grid");
		}

		private void CreateGrid() {
			if (_size <= 0.0001f || _dimensions.x < 1 || _dimensions.y < 1 || Selection.objects.Length != 1 || !_prefab) return;

			var parent = Selection.activeTransform;

			parent.DestroyAllChildren();

			var zeroOffset = _size * -0.5f * (_dimensions - Vector2Int.one);

			for (var y = 0; y < _dimensions.y; y++) {
				for (var x = 0; x < _dimensions.x; x++) {
					var pos = zeroOffset + new Vector2(x, y) * _size;

					var obj = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, parent);
					obj.transform.position = pos.ToX0Z(_heightY);
					obj.transform.SetLocalScale(_size * _scale);
					obj.name = $"{obj.name.Replace("(Clone)", string.Empty)} ({x:00}, {y:00})";
				}
			}
		}

		private void LinkSelectionToGrid() {
			if (_size <= 0.0001f || _dimensions.x < 1 || _dimensions.y < 1) return;

			var parent = Selection.transforms.First().parent;

			var zeroOffset = _size * -0.5f * (_dimensions - Vector2Int.one);

			var items = Selection.transforms.OrderBy(x => x.name).ToArray();
			var index = 0;

			for (var y = 0; y < _dimensions.y; y++) {
				for (var x = 0; x < _dimensions.x; x++) {
					var pos = zeroOffset + new Vector2(x, y) * _size;

					var obj = Selection.transforms[index];
					++index;
					obj.transform.position = pos.ToX0Z(_heightY);
					obj.transform.SetLocalScale(_size * _scale);

					if (index >= Selection.transforms.Length) break;
				}

				if (index >= Selection.transforms.Length) break;
			}
		}

	}

}