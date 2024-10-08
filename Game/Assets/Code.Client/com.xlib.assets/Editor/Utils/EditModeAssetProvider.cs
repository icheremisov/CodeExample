using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLib.Assets.Cache;
using XLib.Assets.Contracts;
using XLib.Assets.Types;
using XLib.Core.Utils;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Assets.Utils {

	/// <summary>
	///     provide access to resources in edit-mode
	/// </summary>
	// public class EditModeAssetProvider : IAssetProvider {
	//
	// 	private readonly List<AddressableAssetGroup> _groups = EditorUtils.LoadAssets<AddressableAssetGroup>();
	// 	public UniTask InitializeCatalogAsync(CancellationToken ct) =>  UniTask.CompletedTask;
	//
	// 	public void Dispose() { }
	//
	// 	public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
	//
	// 	public UniTask<T> LoadByKeyAsync<T>(string key, string category = AddressableCategory.Default) where T : class => UniTask.FromResult(Load<T>(key));
	// 	public UniTask<T> LoadAsync<T>(AssetReference reference, string category = AddressableCategory.Default) where T : class => UniTask.FromResult(Load<T>(reference));
	//
	// 	public UniTask<IEnumerable<T>> LoadByLabelAsync<T>(AssetLabel label, string category = AddressableCategory.Default) where T : class {
	// 		var result = _groups
	// 			.SelectMany(x => x.entries)
	// 			.Where(x => x.labels.Contains(label.ToString()))
	// 			.Select(LoadEntry<T>);
	//
	// 		return UniTask.FromResult(result);
	// 	}
	//
	// 	public UniTask<IEnumerable<T>> LoadAsync<T>(IEnumerable<string> keys, string category = AddressableCategory.Default) where T : class {
	// 		var result = keys.Select(Load<T>);
	//
	// 		return UniTask.FromResult(result);
	// 	}
	//
	// 	public UniTask PreloadAllAssets() => UniTask.CompletedTask;
	//
	// 	public UniTask PreloadAsync(string[] keys) => UniTask.CompletedTask;
	//
	// 	public Sprite GetSprite(string key) => throw new NotSupportedException();
	//
	// 	public void Unload(string key) { }
	//
	// 	public void UnloadCategory(string key) { }
	//
	// 	public bool IsKeyValid<T>(string key) where T : class {
	// 		var t = TypeOf<T>.Raw;
	//
	// 		if (t == TypeOf<Sprite>.Raw) throw new NotSupportedException();
	//
	// 		return _groups.Any(assetGroup => assetGroup.entries.Any(entry => entry.address.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
	// 	}
	//
	// 	private T Load<T>(string key) where T : class {
	// 		if (!TypeOf<Object>.Raw.IsAssignableFrom(TypeOf<T>.Raw))
	// 			throw new Exception($"Error loading address='{key}' in editor mode: asset type ('{TypeOf<T>.Name}') must be derived from Unity.Object!");
	//
	// 		foreach (var assetGroup in _groups) {
	// 			foreach (var entry in assetGroup.entries) {
	// 				if (entry.address.Equals(key, StringComparison.InvariantCultureIgnoreCase)) return LoadEntry<T>(entry);
	// 			}
	// 		}
	//
	// 		throw new Exception($"Cannot find '{TypeOf<T>.Name}' with address='{key}'");
	// 	}
	// 	
	// 	private static T Load<T>(AssetReference reference) where T : class {
	// 		if (!TypeOf<Object>.Raw.IsAssignableFrom(TypeOf<T>.Raw))
	// 			throw new Exception($"Error loading address='{reference}' in editor mode: asset type ('{TypeOf<T>.Name}') must be derived from Unity.Object!");
	//
	// 		var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(reference.AssetGUID));
	//
	// 		if (asset == null) throw new Exception($"Cannot load '{TypeOf<T>.Name}' with GUID='{reference.AssetGUID}': loaded null asset");
	// 		
	// 		return asset as T ??
	// 			throw new Exception(
	// 				$"Cannot cast asset to type '{TypeOf<T>.Name}' with GUID='{reference.AssetGUID}' (real type is {asset.GetType().FullName})");
	// 	}
	//
	// 	private static T LoadEntry<T>(AddressableAssetEntry entry) where T : class {
	// 		var asset = AssetDatabase.LoadAssetAtPath<Object>(entry.AssetPath);
	// 		if (asset == null) throw new Exception($"Cannot load '{TypeOf<T>.Name}' with address='{entry.address}' path='{entry.AssetPath}': loaded null asset");
	//
	// 		return asset as T ??
	// 			throw new Exception(
	// 				$"Cannot cast asset to type '{TypeOf<T>.Name}' with address='{entry.address}' path='{entry.AssetPath}' (real type is {asset.GetType().FullName})");
	// 	}
	//
	// }

}