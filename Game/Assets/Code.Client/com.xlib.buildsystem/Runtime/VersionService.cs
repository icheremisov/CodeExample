using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XLib.BuildSystem {

	public static class VersionService {

		public const string VersionFileName = "version";

		private static VersionStorage _storage;

		public static string ShortVersionString {
			get {
				LoadVersion(false);
				return _storage.versionString;
			}
		}
		public static string ConfigHash {
			get {
				LoadVersion(false);
				return _storage.configHash;
			}
		}

		public static int VersionCode {
			get {
				LoadVersion(false);
				return _storage.versionCode;
			}
			set {
				LoadVersion(false);
				_storage.versionCode = value;
			}
		}

		public static string FullVersionString {
			get {
				LoadVersion(false);
				var env = EnvironmentName;
				return env.IsNullOrEmpty() ? $"{_storage.versionString} ({VersionCode}) {_storage.description}" :
					$"{_storage.versionString} ({VersionCode}) {_storage.description}\n[{env}]";
			}
		}
		
		public static string VersionString {
			get {
				LoadVersion(false);
				var env = EnvironmentName;
				return $"{_storage.versionString} ({VersionCode}) {_storage.description}";
			}
		}

		public static string BundleTarget {
			get {
				LoadVersion(false);
				if (Application.isEditor) {
					var platform =
#if UNITY_ANDROID
						"android";
#elif UNITY_IOS
						"ios";
#else
						"win";
#endif
					var target = "internal";
					
#pragma warning disable CS0162 // Unreachable code detected
					if (GameFeature.Staging) target = "staging";
					if (GameFeature.Production) target = "public";
					if (GameFeature.Demo) target = "demo";
#pragma warning restore CS0162 // Unreachable code detected
					
					return $"{platform}-{target}";
				}
				return _storage.bundleTarget;
			}
		}

#if DEVELOPMENT_BUILD		
		public static string EnvironmentName {
			get {
				LoadVersion(false);
				return _storage.env;
			}
		}
#else
		public static string EnvironmentName => string.Empty;
#endif

		private static void LoadVersion(bool force) {
			if (_storage != null && !force) return;

			try {
				var resText = Resources.Load<TextAsset>(VersionFileName);

				_storage = JsonConvert.DeserializeObject<VersionStorage>(resText.text);
				Debug.Log($"Version loaded: {FullVersionString}");
			}
			catch (Exception e) {
				Debug.LogError($"Error loading version from {VersionFileName}: {e.Message}");
				_storage = new VersionStorage { versionString = "n/a", versionCode = 0 };
			}
		}

		public static void Initialize(bool force = false) {
			LoadVersion(force);
		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public class VersionStorage {

			public int versionCode;
			public string versionString;
			public string description;
			public string env;
			public string bundleTarget;
			public string configHash;

		}

#if UNITY_EDITOR
		private static string VersionFilePath => $"Assets/Resources/{VersionFileName}.json";

		public static void SaveInEditor(VersionStorage storage) {
			_storage = storage;
			
			Directory.CreateDirectory(Path.GetDirectoryName(VersionFilePath));
			File.WriteAllText(VersionFilePath, JsonConvert.SerializeObject(storage), Encoding.UTF8);
			Debug.Log($"Version saved: {FullVersionString}");
			Debug.Log($"File saved: {VersionFilePath}");

			AssetDatabase.ImportAsset(VersionFilePath);
		}

		public static VersionStorage LoadInEditor(bool autoCreate) {
			try {
				return JsonConvert.DeserializeObject<VersionStorage>(File.ReadAllText(VersionFilePath, Encoding.UTF8));
			}
			catch (Exception e) {
				var err = $"Error loading version from {VersionFilePath} - create new file: {e.Message}";
				if (!autoCreate) throw new Exception(err);

				Debug.LogWarning(err);
				var storage = new VersionStorage { versionString = "1.0.0", versionCode = 1 };
				SaveInEditor(storage);
				AssetDatabase.Refresh();

				return storage;
			}
		}

#endif

	}

}