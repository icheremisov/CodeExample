#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using XLib.Core.Reflection;
using XLib.Core.Utils;
using Object = UnityEngine.Object;

namespace XLib.Unity.Utils {

	public static partial class EditorUtils {
		[Flags]
		public enum SetupRenderersFlags {
			Default = 0,
			CastShadows = 1 << 0,
			ReceiveShadows = 1 << 1
		}

		/// <summary>
		///     get project name from folder
		/// </summary>
		public static string ProjectName {
			get {
				var path = Application.dataPath.Split(Path.PathSeparator);
				var index = path.LastIndexOf(x => !x.IsNullOrEmpty() && !x.Equals("Assets", StringComparison.InvariantCultureIgnoreCase));

				return index < 0 ? "UnnamedApp" : path[index];
			}
		}

		public static IEnumerable<T> EnumerateAssetOfType<T>(this IEnumerable<object> objects, bool withFolders = false, bool withSubAssets = false) where T : Object {
			foreach (var o in objects) {
				switch (o) {
					case DefaultAsset defaultAsset: {
						if (!withFolders && defaultAsset is T t) yield return t;
						var path = AssetDatabase.GetAssetPath(defaultAsset);
						if (!AssetDatabase.IsValidFolder(path)) continue;
						foreach (var guid in AssetDatabase.FindAssets("", new [] {path})) {
							var file = AssetDatabase.GUIDToAssetPath(guid);
							if (withSubAssets && !file.EndsWith(".unity", StringComparison.InvariantCultureIgnoreCase)) {
								foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(file)) {
									if (asset is T a) yield return a;
								}
							}
							else {
								var type = AssetDatabase.GetMainAssetTypeAtPath(file);
								if (!TypeOf<T>.IsAssignableFrom(type)) continue;
								var obj = AssetDatabase.LoadAssetAtPath<T>(file);
								if (obj != null)
									yield return obj;
							}
						}
						break;
					}

					default: {
						if (withSubAssets) {
							foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(o as Object))) {
								if (asset is T t) yield return t;
							}
						}
						else {
							if (o is T t) yield return t;
						}
						break;
					}
				}
			}
		}
		
		/// <summary>
		///     load assets of specific type to list
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>list with items</returns>
		public static List<T> LoadAssets<T>(string name = null, params string[] searchInFolders) where T : Object {
			name ??= string.Empty;
			var result = AssetDatabase.FindAssets(InjectFilter<T>($"t:{typeof(T).Name} {name}"), searchInFolders)
				.Select(AssetDatabase.GUIDToAssetPath)
				.SelectMany(AssetDatabase.LoadAllAssetsAtPath)
				.OfType<T>();

			if (!name.IsNullOrEmpty()) result = result.Where(x => x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			return result.ToList();
		}

		public static event Func<string, Type, string> OnFilter;
		public static ILockable FilterLockable = new LockerCounter();

		public static string InjectFilter<T>(string filter) => FilterLockable.IsLocked ? filter : (OnFilter?.Invoke(filter, TypeOf<T>.Raw) ?? filter);
		public static string InjectFilter(string filter, Type type) => FilterLockable.IsLocked ? filter : (OnFilter?.Invoke(filter, type) ?? filter);

		/// <summary>
		///     load assets of specific type to list
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <param name="folder">asset folder</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>list with items</returns>
		public static List<T> LoadAssets<T>(string name, string folder) where T : Object {
			var result = AssetDatabase.FindAssets(InjectFilter<T>($"t:{typeof(T).Name} {name}"), folder.IsNullOrEmpty() ? null : new[] { folder })
				.Select(AssetDatabase.GUIDToAssetPath)
				.SelectMany(AssetDatabase.LoadAllAssetsAtPath)
				.OfType<T>();

			if (!name.IsNullOrEmpty()) result = result.Where(x => x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			return result.Where(o => o != null).ToList();
		}

		/// <summary>
		///     load assets of specific type to dictionary
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>list with items</returns>
		public static Dictionary<string, T> LoadAssetsToDictionary<T>(string name = null, params string[] searchInFolders) where T : Object {
			name ??= string.Empty;
			var result = AssetDatabase.FindAssets(InjectFilter<T>($"t:{typeof(T).Name} {name}"), searchInFolders)
				.Select(AssetDatabase.GUIDToAssetPath)
				.SelectMany(AssetDatabase.LoadAllAssetsAtPath)
				.OfType<T>();

			if (!name.IsNullOrEmpty()) result = result.Where(x => x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			return result.Where(o => o != null).ToDictionary(x => x.name, StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>
		///     load assets of specific type and select first (random)
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>single item or null</returns>
		public static T LoadFirstAsset<T>(string name = "") where T : Object => LoadAssets<T>(name).FirstOrDefault();

		/// <summary>
		///     load assets of specific type and check for unique
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>single item or null if not found</returns>
		public static T LoadSingleAsset<T>(string name = "") where T : Object {
			var result = LoadAssets<T>(name);
			if (result.Count > 1) throw new Exception($"Expected only 1 asset (type={TypeOf<T>.Name} name='{name}') but found {result.Count} assets of same type");

			return result.FirstOrDefault();
		}

		/// <summary>
		///     load assets of specific type and check for unique and existance
		/// </summary>
		/// <param name="name">if not empty, exact name for match</param>
		/// <typeparam name="T">Unity object</typeparam>
		/// <returns>single item</returns>
		public static T LoadExistingAsset<T>(string name = "") where T : Object {
			var result = LoadAssets<T>(name);
			if (result.IsNullOrEmpty()) throw new Exception($"Asset (type={TypeOf<T>.Name} name='{name}') not found!");

			if (!name.IsNullOrEmpty()) result = result.Where(x => x != null && x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
			if (result.Count > 1) throw new Exception($"Expected only 1 asset (type={TypeOf<T>.Name} name='{name}') but found {result.Count} assets");
			if (result[0] == null) throw new Exception($"Asset (type={TypeOf<T>.Name} name='{name}') found but it's script is missing!");

			return result.FirstOrDefault();
		}

		/// <summary>
		///     get list of all assets at path. return full paths
		/// </summary>
		public static string[] GetAssetPaths<T>(string name = "") =>
			AssetDatabase.FindAssets(InjectFilter<T>($"t:{typeof(T).Name} {name}")).SelectToArray(AssetDatabase.GUIDToAssetPath);

		/// <summary>
		///     get list of all assets at path. return full paths
		/// </summary>
		public static string[] GetAssetPaths(Type type, string name = "") =>
			AssetDatabase.FindAssets(InjectFilter($"t:{type.Name} {name}", type)).SelectToArray(AssetDatabase.GUIDToAssetPath);

		/// <summary>
		///     get list of all assets at path. return assets names
		/// </summary>
		public static IEnumerable<string> EnumerateAssets(Type type, string folder, bool namesOnly = true) {
			var assets = AssetDatabase.FindAssets(InjectFilter($"t:{type.Name}", type), folder != null ? new[] { folder } : null);
			if (namesOnly)
				return assets.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x))).OrderBy(x => x).ToArray();
			else
				return assets.Select(AssetDatabase.GUIDToAssetPath).OrderBy(x => x).ToArray();
		}

		/// <summary>
		///     get list of all assets at path. return assets names
		/// </summary>
		public static IEnumerable<string> EnumerateAssets<T>(string folder, bool namesOnly = true) where T : Object => EnumerateAssets(TypeOf<T>.Raw, folder, namesOnly);

		/// <summary>
		///     find all component's 'fields with [ViewReference] and try assign reference to component from Scene
		/// </summary>
		public static void FindReferences(MonoBehaviour component, bool assignValues, UnityEngine.SceneManagement.Scene? scene) {
			Debug.Log($"FindReferences: {component.GetFullPath()}/{component.GetType().Name}");

			var t = component.GetType();

			foreach (var fieldInfo in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						 .Where(x => x.HasAttribute<ViewReferenceAttribute>())) {
				var pattern = fieldInfo.GetAttribute<ViewReferenceAttribute>().SearchMask;
				pattern = pattern.IsNullOrEmpty() ? "*" : $"*{pattern}*";

				var fieldType = fieldInfo.FieldType;

				try {
					if (fieldType.IsGenericList()) {
						var objectType = fieldType.Get1stGenericArgument();

						var objects = GameObjectExtensions.FindObjectsOfTypeAll(objectType, scene);
						objects = objects.Where(x => x.name.IsMatch(pattern)).ToArray();
						if (objects.Length == 0) {
							if (fieldInfo.HasAttribute<RequiredAttribute>()) throw new Exception($"Cannot find any object of type {objectType} '{pattern}'");
							continue;
						}

						if (assignValues) {
							var list = (IList)Activator.CreateInstance(fieldType);

							foreach (var obj in objects) list.Add(obj);

							fieldInfo.SetValue(component, list);
						}
					}
					else if (fieldType.IsArray) {
						var objectType = fieldType.GetElementType();

						var objects = GameObjectExtensions.FindObjectsOfTypeAll(objectType, scene);
						objects = objects.Where(x => x.name.IsMatch(pattern)).ToArray();

						if (objects.Length == 0) {
							if (fieldInfo.HasAttribute<RequiredAttribute>()) throw new Exception($"Cannot find any object of type {objectType} '{pattern}'");
							continue;
						}

						if (assignValues) {
							var list = Array.CreateInstance(objectType, objects.Length);

							for (var index = 0; index < objects.Length; index++) {
								var obj = objects[index];
								list.SetValue(obj, index);
							}

							fieldInfo.SetValue(component, list);
						}
					}
					else if (!assignValues || (fieldInfo.GetValue(component) as Object) == null) {
						if (!TypeOf<Object>.IsAssignableFrom(fieldType)) {
							throw new Exception($"type {fieldType}: Attribute {nameof(ViewReferenceAttribute)} can be only used for types, derived from Unity.Object!");
						}

						var objects = GameObjectExtensions.FindObjectsOfTypeAll(fieldType, scene);
						objects = objects.Where(x => x.name.IsMatch(pattern)).ToArray();

						if (objects.Length == 0) {
							if (fieldInfo.HasAttribute<RequiredAttribute>()) throw new Exception($"Cannot find any object of type {fieldType} '{pattern}'");
							continue;
						}

						var objectToSet = objects.Length == 1 ? objects[0] : null;

						if (objectToSet == null) {
							var name = fieldInfo.Name;
							if (name.StartsWith("_")) name = name[1..];
							var autoDetectItems = objects.Where(x => x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
							if (autoDetectItems.Length == 1) objectToSet = autoDetectItems[0];
						}

						if (objectToSet == null) {
							var name = ObjectNames.NicifyVariableName(fieldInfo.Name);
							var autoDetectItems = objects.Where(x => x.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
							if (autoDetectItems.Length == 1) objectToSet = autoDetectItems[0];
						}

						if (objectToSet == null) {
							throw new Exception(
								$"Expected only 1 object of type '{fieldType}', but found ({objects.Length}):\n{objects.Select(x => x.GetFullPath()).JoinToString("\n")}");
						}

						if (assignValues) fieldInfo.SetValue(component, objectToSet);
					}
				}
				catch (Exception e) {
					Debug.LogError($"{fieldInfo.Name} ({fieldType}): {e}");
				}
			}

			if (assignValues) EditorUtility.SetDirty(component);
		}

		public static T CreateScriptableObject<T>(string path, string name) where T : ScriptableObject {
			var asset = ScriptableObject.CreateInstance<T>();

			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			AssetDatabase.CreateAsset(asset, Path.Combine(path, $"{name}.asset"));
			return asset;
		}

		public static TextAsset CreateTextAsset(string path, string nameWithExt, string text) {
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			var filePath = Path.Combine(path, nameWithExt);
			File.WriteAllText(filePath, text, new UTF8Encoding(false));

			AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
			return AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
		}

		public static void ReplaceTextAsset(TextAsset asset, string newText) {
			var path = AssetDatabase.GetAssetPath(asset);
			File.WriteAllText(path, newText, new UTF8Encoding(false));
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}

		public static void DeleteAsset(Object asset) {
			if (!asset) return;

			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _)) throw new Exception($"Cannot get GUID for asset: '{asset.name}'");

			var path = AssetDatabase.GUIDToAssetPath(guid);
			AssetDatabase.DeleteAsset(path);
		}

		public static Texture2D SavePngToAssets(byte[] pngBytes, string fn, TextureImporterType importerTextureType,
			TextureImporterAlphaSource textureImporterAlphaSource, bool importerAlphaIsTransparency,
			bool enableMipMaps, TextureWrapMode wrapMode,
			int maxTextureSize, TextureImporterCompression textureImporterCompression,
			bool crunchedCompression) {
			Directory.CreateDirectory(Path.GetDirectoryName(fn) ?? throw new InvalidOperationException());
			File.WriteAllBytes(fn, pngBytes);

			return ImportTextureAsset(fn, importerTextureType,
				textureImporterAlphaSource, importerAlphaIsTransparency,
				enableMipMaps, wrapMode,
				maxTextureSize, textureImporterCompression,
				crunchedCompression);
		}

		public static Texture2D ImportTextureAsset(string fileName, TextureImporterType importerTextureType,
			TextureImporterAlphaSource textureImporterAlphaSource, bool importerAlphaIsTransparency,
			bool enableMipMaps, TextureWrapMode wrapMode,
			int maxTextureSize, TextureImporterCompression textureImporterCompression,
			bool crunchedCompression) {
			AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUncompressedImport);

			var importer = (TextureImporter)AssetImporter.GetAtPath(fileName);
			importer.wrapMode = wrapMode;
			importer.textureType = importerTextureType;
			importer.alphaSource = textureImporterAlphaSource;
			importer.alphaIsTransparency = importerAlphaIsTransparency;
			importer.mipmapEnabled = enableMipMaps;
			importer.maxTextureSize = maxTextureSize;
			importer.textureCompression = textureImporterCompression;
			importer.crunchedCompression = crunchedCompression;

			AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);

			return AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);
		}

		public static void SetupAllRenderers(this GameObject obj, SetupRenderersFlags preset) {
			obj.transform.SetupAllRenderers(preset);
		}

		public static void SetupAllRenderers(this Transform obj, SetupRenderersFlags preset) {
			foreach (var r in obj.GetComponentsInChildren<Renderer>(true)) {
				r.shadowCastingMode = preset.Has(SetupRenderersFlags.CastShadows) ? ShadowCastingMode.On : ShadowCastingMode.Off;
				r.receiveShadows = preset.Has(SetupRenderersFlags.ReceiveShadows);
				r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
				r.lightProbeUsage = LightProbeUsage.Off;
				r.reflectionProbeUsage = ReflectionProbeUsage.Off;
				r.rayTracingMode = RayTracingMode.Off;
				EditorUtility.SetDirty(r.gameObject);
			}
		}

		public static void SetObjectDirty(this Object obj) {
			if (EditorApplication.isPlaying) return;

			EditorUtility.SetDirty(obj);
			if (obj is GameObject go)
				EditorSceneManager.MarkSceneDirty(go.scene);
			else if (obj is Component comp) EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
		}

		public static void MarkNotSaveable(this GameObject obj) {
			foreach (var child in obj.GetComponentsInChildren<Transform>()) {
				child.gameObject.hideFlags = HideFlags.DontSaveInBuild;
				child.gameObject.SetObjectDirty();
			}

			var handle = PrefabUtility.GetPrefabInstanceHandle(obj);
			if (handle != null) handle.hideFlags = HideFlags.DontSaveInBuild;
		}

		public static string GetGuid(Object obj) {
			if (obj == null) return null;

			if (AssetDatabase.IsMainAsset(obj)) {
				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long _);
				return guid;
			}

			return null;
		}

		public static void ChangeAssetType(Object obj, Type newType) {
			var properties = new SerializedObject(obj);
			var script = properties.FindProperty("m_Script");
			script.objectReferenceValue = GetMonoScriptByType(newType);
			properties.ApplyModifiedProperties();
		}

		public static void ChangeComponentTypeScript<T>(Component obj) where T : MonoBehaviour {
			if (PrefabUtility.IsPartOfVariantPrefab(obj.gameObject)) {
				EditorUtility.DisplayDialog("Error", "Нельзя сменить тип компонента у варианта префаба", "ok");
				return;
			}

			var tempGo = new GameObject("TEMP");
			var monoScript = MonoScript.FromMonoBehaviour(tempGo.AddComponent<T>());
			Object.DestroyImmediate(tempGo);

			var so = new SerializedObject(obj);
			var scriptProperty = so.FindProperty("m_Script");
			so.Update();
			scriptProperty.objectReferenceValue = monoScript;
			so.ApplyModifiedProperties();
		}

		public static Object GetRootAsset(this Object owner) {
			if (owner is not GameObject go && (owner is not Component comp || (go = comp.gameObject) == null))
				return !AssetDatabase.IsMainAsset(owner) ? AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(owner)) : owner;

			if (PrefabUtility.IsPartOfAnyPrefab(owner)) return AssetDatabase.LoadAssetAtPath<Object>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(owner));

			if (!string.IsNullOrEmpty(go.scene.path)) return AssetDatabase.LoadAssetAtPath<Object>(go.scene.path);

			var cur = go.transform;
			while (cur.parent != null) cur = cur.parent;
			return cur;
		}

		public class DisplayProgressUsing : IDisposable {
			private string _header;
			private bool _cancelable = false;
			private float _min = 0f, _max = 0f;
			private float _progress = 0;

			public DisplayProgressUsing(string header, bool cancelable = false) {
				_header = header;
				_cancelable = cancelable;
			}

			public void SetHeader(string header) => _header = header;

			public void Progress(string text, float progress, int stepNo, int stepCount) {
				if (progress > _max) {
					_min = _progress;
					_max = progress;
				}

				Progress($"{text} {stepNo}/{stepCount}", _min + (_max - _min) * stepNo / stepCount);
			}

			public void Progress(string text, float progress) {
				_progress = Mathf.Clamp01(progress);
				Log(text);

				if (!_cancelable) {
					EditorUtility.DisplayProgressBar(_header, text, _progress);
					return;
				}

				if (EditorUtility.DisplayCancelableProgressBar(_header, text, _progress)) {
					EditorUtility.ClearProgressBar();
					throw new TaskCanceledException();
				}
			}

			public void Dispose() => EditorUtility.ClearProgressBar();

			public void Log(string message) => Debug.Log(message);
			public void Error(string message) => Debug.LogError(message);
		}

		public static DisplayProgressUsing DisplayProgress(string header, bool cancelable = true) => new(header, cancelable);

		private static readonly Dictionary<Type, MonoScript> _monoScripts = new Dictionary<Type, MonoScript>();

		public static MonoScript GetMonoScriptByType(Type type) {
			if (_monoScripts.TryGetValue(type, out var script)) return script;

			var path = AssetDatabase.FindAssets($"t:MonoScript {type.Name}").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(s => Path.GetFileNameWithoutExtension(s) == type.Name);
			if (!path.IsNullOrEmpty()) {
				script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				_monoScripts[type] = script;
				return script;
			}
			Debug.LogError($"Cannot find script for type: {type.Name}");
			
			if (type.IsSubclassOf(typeof(ScriptableObject))) {
					var dummy = ScriptableObject.CreateInstance(type);
					script = MonoScript.FromScriptableObject(dummy);
					Object.DestroyImmediate(dummy);
			}
			else {
				var dummy = EditorUtility.CreateGameObjectWithHideFlags("temp", HideFlags.HideAndDontSave, type);
				var comp = dummy.GetComponent(type) as MonoBehaviour;
				script = MonoScript.FromMonoBehaviour(comp);
				Object.DestroyImmediate(dummy);
			}

			_monoScripts[type] = script;
			return script;
		}

		public static string ShowInputDialog(string title, string description, string input, string ok = "Ok", string cancel = "Cancel") =>
			EditorInputDialog.Show(title, description, input, ok, cancel);

		/// <summary>
		/// try to get active path in Project Window 
		/// </summary>
		public static bool TryGetActiveFolderPath(out string path) {
			var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
			var args = new object[] { null };
			var found = (bool)tryGetActiveFolderPath.Invoke(null, args);
			path = (string)args[0];

			if (!found && Selection.activeObject != null) {
				var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (Directory.Exists(assetPath)) {
					path = assetPath;
					return true;
				}

				path = Path.GetDirectoryName(assetPath);
				return true;
			}

			return found;
		}

		public static string GetSerializedTypeString(Type type) {
			var script = GetMonoScriptByType(type);
			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out var guid, out long fileId)) return null;
			return "  m_Script: {fileID: " + fileId + ", guid: " + guid + ", type: 3}";
		}

		public static void ChangeAllObjectsType<TFrom, TTo>(params string[] folders) where TFrom : Object where TTo : Object =>
			ChangeAllObjectsType(TypeOf<TFrom>.Raw, TypeOf<TTo>.Raw, folders);

		public static void ChangeAllObjectTypeInScene<TFrom, TTo>(UnityEngine.SceneManagement.Scene scene) where TFrom : Object where TTo : Object {
			foreach (var component in scene.FindComponents<TFrom>()) {
				if (component.GetType() == typeof(TFrom)) ChangeAssetType(component, typeof(TTo));
			}
		}

		public static void ChangeAllObjectTypeInGameObject<TFrom, TTo>(IEnumerable<GameObject> gameObjects) where TFrom : Object where TTo : Object {
			foreach (var gameObject in gameObjects) {
				var component = gameObject.GetComponent<TFrom>();
				if (component != null) ChangeAssetType(component, typeof(TTo));
			}
		}

		public static void ChangeAllObjectsType(Type fromType, Type toType, params string[] folders) {
			var search = GetSerializedTypeString(fromType);
			var replace = GetSerializedTypeString(toType);
			if (search == null || replace == null) throw new InvalidOperationException("Something wrong");
			var count = 0;

			foreach (var candidateGuid in AssetDatabase.FindAssets("t:scene t:prefab t:ScriptableObject", folders)) {
				var path = AssetDatabase.GUIDToAssetPath(candidateGuid);
				var lines = File.ReadAllLines(path);
				var changed = false;
				for (var i = 0; i < lines.Length; i++) {
					if (lines[i] == search) {
						lines[i] = replace;
						changed = true;
						++count;
					}
				}

				if (changed) {
					File.WriteAllLines(path, lines);
				}
			}

			Debug.Log($"{count} objects of type {fromType} replaced with {toType}");
		}

		/// <summary>
		///    Create directory in Assets
		/// </summary>
		/// <param name="path"> should start as "Assets/..."</param>
		public static void TryCreateAssetFolder(string path) {
			if (AssetDatabase.IsValidFolder(path)) return;
			var dirFullPath = $"{Path.Combine(Application.dataPath.Replace("Assets", ""), path)}/";
			Directory.CreateDirectory(dirFullPath);
			AssetDatabase.Refresh();
		}
	}

}
#else
namespace XLib.Unity.Utils
{
	public static class EditorUtils {
			public static void SetObjectDirty(this UnityEngine.Object obj) {}
	}
}
#endif