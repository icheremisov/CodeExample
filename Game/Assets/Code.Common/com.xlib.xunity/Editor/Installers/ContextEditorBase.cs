using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Zenject;

namespace XLib.Unity.Installers {

	public abstract class ContextEditorBase : OdinEditor {
		private bool _foldout;

		private static bool _needUpdate;
		private static Context _cachedTarget;

		[DidReloadScripts]
		public static void ScriptsReloaded() {
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;

			_cachedTarget = null;
			_needUpdate = false;
		}

		public override void OnInspectorGUI() {
			if (NeedUpdateInstallers()) {
				EditorGUILayout.HelpBox($"Update Installers Required!", MessageType.Error);

				GUILayout.Space(10);
			}

			if (GUILayout.Button("Update Installers")) {
				UpdateInstallers();
			}

			GUILayout.Space(10);

			_foldout = EditorGUILayout.Foldout(_foldout, "Additional Properties", true);

			if (_foldout) {
				base.OnInspectorGUI();
			}
		}

		private bool NeedUpdateInstallers() {
			if (_cachedTarget != (Context)target) {
				_cachedTarget = (Context)target;
				_needUpdate = ContextEditorHelpers.NeedUpdateInstallers(_cachedTarget);
			}

			return _needUpdate;
		}

		private void UpdateInstallers() {
			ContextEditorHelpers.BindAll((Context)target);

			_cachedTarget = null;
			_needUpdate = false;
		}
	}

}