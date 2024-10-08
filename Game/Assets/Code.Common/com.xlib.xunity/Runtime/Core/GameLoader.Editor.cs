#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using XLib.Core.Json.Net;
using XLib.Unity.Json.Net;
using UnityEditor;
using UnityEngine.SceneManagement;
using Zenject;

namespace XLib.Unity.Core {

	public static partial class GameLoader {
		private const string StorageKey = "Client.Core.GameLoader.Data";

		private static JsonSerializerSettings Settings { get; set; }

		private static LoaderData _data;

		[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
		private class LoaderData {
			public IEditorStartupParams Params { get; set; }
			public string[] LoadAdditiveScenes { get; set; }
			public GameLoadingMode Mode { get; set; }
		}

		private static void InitEditor() {
			Settings = new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Objects,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			};

			Settings.Converters.Add(new Vector2NetConverter());
			Settings.Converters.Add(new Vector3NetConverter());
			Settings.Converters.Add(new Vector4NetConverter());
			Settings.Converters.Add(new ColorNetConverter());
			Settings.Converters.Add(new QuaternionNetConverter());
			Settings.Converters.Add(new FlagsNetConverter());
			Settings.Converters.Add(new StringEnumConverter());
			Settings.Converters.Add(new RangeNetConverter());
			Settings.Converters.Add(new RangeFNetConverter());
		}

		private static async UniTask StartInEditor() {
			RestoreParams();

			var needChangeScene = Mode == GameLoadingMode.GameFromStart && SceneManager.GetActiveScene().name != InitializeSceneName;
			
			// if (Mode != GameLoadingMode.GameFromStart || needChangeScene) SceneContext.SuppressFatalErrors = true;

			if (needChangeScene) {
				await SceneManager.LoadSceneAsync(InitializeSceneName, LoadSceneMode.Single);
				await UniTask.DelayFrame(1);
			}

			if (_data?.LoadAdditiveScenes != null) {
				// SceneContext.SuppressFatalErrors = true;
				foreach (var additiveScene in _data.LoadAdditiveScenes) {
					await SceneManager.LoadSceneAsync(additiveScene, LoadSceneMode.Additive);
				}

				await UniTask.DelayFrame(1);
			}

			if (Mode == GameLoadingMode.GameFromStart) {
				await UniTask.DelayFrame(1);
				// SceneContext.SuppressFatalErrors = false;
			}

			Debug.Log($"{nameof(GameLoader)}: Mode={Mode}");
		}

		private static void BackupParams() {
			if (_data != null) {
				var obj = JsonConvert.SerializeObject(_data, Settings);
				EditorPrefs.SetString(StorageKey, obj);
			}
			else {
				EditorPrefs.DeleteKey(StorageKey);
			}
		}

		private static void RestoreParams() {
			_data = null;
			Params = null;

			try {
				var obj = EditorPrefs.GetString(StorageKey, string.Empty);
				_data = !obj.IsNullOrEmpty() ? JsonConvert.DeserializeObject<LoaderData>(obj, Settings) : null;
			}
			catch (Exception e) {
				Debug.LogError($"Cannot load IEditorStartupParams: {e.Message}");
			}

			EditorPrefs.DeleteKey(StorageKey);
			Params = _data?.Params;
			Mode = _data?.Mode ?? Mode;
		}

		public static void PlayGameFromStart(IEditorStartupParams additionalParams = null) {
			_data = new LoaderData() { Params = additionalParams, Mode = GameLoadingMode.GameFromStart };

			BackupParams();
			EditorApplication.EnterPlaymode();
		}

		public static void PlayGameFromCurrentScene(IEditorStartupParams additionalParams = null, string[] loadAdditiveScenes = null) {
			_data = new LoaderData() { Params = additionalParams, LoadAdditiveScenes = loadAdditiveScenes, Mode = GameLoadingMode.PlayFromScene };

			BackupParams();
			EditorApplication.EnterPlaymode();
		}
	}

}

#endif