using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Configs.Utils;
using XLib.Core.Utils;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	public static class EditorConfigUtils {
		[MenuItem("Tools/Database/Update Config GUID", false, -101)]
		private static void UpdateConfigGuid() {
			Selection.objects.OfType<GameItemBase>().ForEach(ConfigUtils.UpdateItemId);
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Assets/Database/Create Game Config", false, -100)]
		private static void CreateConfigFromWindow() =>
			ClassTypeSelector.Show<GameItemBase>("Create Config", t => SaveAsset(Create(t), autosave: true, canRewrite: false), null, false,
				type => (type?.GetCustomAttribute<ItemCategoryAttribute>(true)?.Name ?? "Other", GetItemNameDescription(type)));

		private static string GetItemNameDescription(Type type) {
			if (type == null) return "<None>";

			var desc = type.GetCustomAttribute<ItemDescriptionAttribute>(true);
			var result = type.Name.Replace("Definition", string.Empty);
			if (desc != null) result += $"\n({desc.Description})";
			return result;
		}

		[MenuItem("Assets/Database/Create Scriptable Object", false, -100)]
		private static void CreateScriptableObjectFromWindow() =>
			ClassTypeSelector.Show<ScriptableObject>("Create Scriptable Object",
				t => SaveAsset(Create(t)), null, false, CategoryFromAssembly);

		private static (string category, string name) CategoryFromAssembly(Type type) {
			var category = type.Assembly.GetName().Name.Split('.', '-').First();
			if (category == "com") category = "Packages";

			return (category, ObjectNames.NicifyVariableName(type.Name));
		}

		[MenuItem("Assets/Database/Create Config From Script", false, -97)]
		private static void CreateFromScript() {
			SaveAsset(Create(((MonoScript)Selection.activeObject).GetClass()));
		}

		[MenuItem("Assets/Database/Create Config From Script", true, -97)]
		private static bool CreateFromScriptValidate() {
			var script = Selection.activeObject as MonoScript;
			return script != null &&
				script.GetClass().IsSubclassOf(typeof(ScriptableObject));
		}

		public static ScriptableObject Create(Type type) {
			if (!type.IsSubclassOf(typeof(ScriptableObject))) {
				Debug.LogError($"Can't create {type}. This is not a scriptable script.");
				return null;
			}

			var obj = ScriptableObject.CreateInstance(type);
			if (obj != null) return obj;

			Debug.LogError($"Can't create {type}");
			return null;
		}

		public static void SaveAsset(Object obj, string name = null, string path = null, bool autosave = false, bool canRewrite = true, bool select = true) {
			if (path == null) {
				path = Selection.activeObject ? AssetDatabase.GetAssetPath(Selection.activeObject) : "Assets";
				if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);
			}

			if (path == null) return;

			if (name.IsNullOrEmpty()) name = obj.name;
			if (name.IsNullOrEmpty()) name = obj.GetType().Name;
			if (!Path.HasExtension(name)) name += ".asset";

			path = Path.Combine(path, name);
			if (autosave) {
				if (!canRewrite) path = AssetDatabase.GenerateUniqueAssetPath(path);
				AssetDatabase.CreateAsset(obj, path);
				AssetDatabase.SaveAssets();
				if (select) Selection.activeObject = obj;
			}
			else {
				ProjectWindowUtil.CreateAsset(obj, path);
				return;
			}

			if (obj is not GameItemBase) return;

			var createCallback = TypeOf<GameItemBase>.Raw.GetMethod(GameItemBase.CreateMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (createCallback != null) createCallback.Invoke(obj, null);
		}

		internal static (string category, string name) ClassName(Type type) => ("Other", type?.Name ?? "None");

		[MenuItem("Assets/Database/Resave", false, -100)]
		public static void ResaveAssets() {
			try {
				AssetDatabase.StartAssetEditing();
				foreach (var o in Selection.objects.EnumerateAssetOfType<Object>(true)) {
					o.SetObjectDirty();
					AssetDatabase.SaveAssetIfDirty(o);
				}
			}
			finally {
				AssetDatabase.StopAssetEditing();
			}
		}

		public static T CreateAndSave<T>(string message, string defaultName, string path, bool autosave) {
			var fileName = EditorUtils.ShowInputDialog($"Create {TypeOf<T>.Name}", message, defaultName ?? string.Empty);
			if (fileName.IsNullOrEmpty()) return default;
			var asset = Create(TypeOf<T>.Raw);
			if (path == null) {
				path = Selection.activeObject ? AssetDatabase.GetAssetPath(Selection.activeObject) : "Assets";
				if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);
			}

			SaveAsset(asset, fileName, path, autosave);
			return asset is T t ? t : default;
		}

		private static void CopyAll(DirectoryInfo source, DirectoryInfo target, Dictionary<string, string> guidMaps, string filePrefix) {
			Directory.CreateDirectory(target.FullName);
			foreach (var fi in source.GetFiles()) {
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				CopyFile(fi, target, guidMaps, filePrefix);
			}

			foreach (var diSourceSubDir in source.GetDirectories()) CopyAll(diSourceSubDir, target.CreateSubdirectory(diSourceSubDir.Name), guidMaps, filePrefix);
		}

		public static void CopyFile(FileInfo source, DirectoryInfo dst, Dictionary<string, string> guidMaps, string filePrefix) {
			Directory.CreateDirectory(dst.FullName);
			var data = source.OpenText().ReadToEnd();

			foreach (var guidMap in guidMaps
						 .Where(guidMap => data.Contains(guidMap.Key)))
				data = data.ReplaceAll(guidMap.Key, guidMap.Value);

			var isDirMeta = source.FullName.EndsWith(".meta") && Directory.Exists(source.FullName[..^5]);

			if (!isDirMeta) {
				var name = Path.GetFileNameWithoutExtension(source.Name);
				data = data.ReplaceAll($"m_Name: {name}", $"m_Name: {filePrefix}{name}");
			}

			File.WriteAllText(Path.Combine(dst.FullName, isDirMeta ? source.Name : filePrefix + source.Name), data);
		}

		private static string GetGuidFromMetaFile(string file) {
			var lines = File.ReadLines(file).Take(2).ToArray();
			Debug.Assert(lines[0] == "fileFormatVersion: 2" || lines[1].StartsWith("guid: "), $"Unknown format \"{lines[0]}\" at file {file}");
			return lines[1].Substring(6, 32);
		}

		private static void SetLabel(string label, bool enable) {
			foreach (var file in Directory.EnumerateFiles(AssetDatabase.GetAssetPath(Selection.activeInstanceID), "*.asset", SearchOption.AllDirectories))
				SetLabelForFile(label, enable, file);
		}

		public static void SetLabelForFile(string label, bool enable, string path) {
			var asset = AssetDatabase.LoadMainAssetAtPath(path);
			var labels = AssetDatabase.GetLabels(asset).ToHashSet();
			var changed = false;
			if (enable)
				changed = labels.Add(label);
			else
				changed = labels.Remove(label);

			if (changed) AssetDatabase.SetLabels(asset, labels.ToArray());
		}
	}

}