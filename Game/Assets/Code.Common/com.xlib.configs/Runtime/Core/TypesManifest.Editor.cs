#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Core.Reflection;
using XLib.Unity.Utils;

namespace XLib.Configs.Core {

	public partial class TypesManifest : IEditorAssetManifest {
		private List<ScriptInfo> GetScriptInfos() {
			var scripts = new List<ScriptInfo>();
			foreach (var script in EditorUtils.LoadAssets<MonoScript>(null, _sources)) {
				if (AssetDatabase.GetAssetPath(script).Contains("/Editor/")) continue;
				var cls = script.GetClass();
				if (cls != null && IsValidType(cls)) {
					scripts.Add(new ScriptInfo { guid = EditorUtils.GetGuid(script), type = cls.FullName });
				}
			}
			scripts.Sort((script1, script2) => string.Compare(script1.type, script2.type, StringComparison.Ordinal));
			return scripts;
		}
		
		[Button("Update", ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(-1)]
		void IEditorAssetManifest.EditorInitialize() {
			var scripts = GetScriptInfos();

			var dirty = false;
			if (_scripts.Count != scripts.Count || !_scripts.SequenceEqual(scripts)) {
				_scripts = scripts;
				dirty = true;
			}

			if (dirty) {
				this.SetObjectDirty();
				AssetDatabase.SaveAssetIfDirty(this);
			}
		}

		private static bool IsValidType(Type cls) => 
			cls.IsSubclassOf(typeof(ScriptableObject))
			&& !cls.IsAbstract && !cls.IsInterface
			&& !cls.HasAttribute<TypeManifestIgnoreAttribute>()
			&& !cls.IsSubclassOf(typeof(Editor))
			&& !cls.IsSubclassOf(typeof(EditorWindow));

		public bool NeedUpdateTypes() {
			var scripts = GetScriptInfos();
			return _scripts.Count != scripts.Count || !_scripts.SequenceEqual(scripts);
		}
	}

}
#endif