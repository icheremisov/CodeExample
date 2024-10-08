using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using XLib.Assets.Cache;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Assets.Utils {

	public static class AddressableUtils {

		private static List<AddressableAssetGroup> _assetGroups = new();

		public static string AddToAddressables(Object asset, string assetGroupName, params AssetLabel[] labels) {
			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _)) throw new Exception($"Cannot get GUID for asset: '{asset.name}'");
			return AddToAddressables(guid, assetGroupName, labels);
		}

		public static string AddToAddressables(string guid, string assetGroupName, params AssetLabel[] labels)
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			var groups = EditorUtils.LoadAssets<AddressableAssetGroup>();

			AddressableAssetGroup group;

			if (assetGroupName.IsNullOrEmpty())
				group = settings.DefaultGroup;
			else {
				group = groups.FirstOrDefault(x => x.Name == assetGroupName);
				if (group == null) throw new Exception($"Addressable group not found: '{assetGroupName}'");
			}


			var entry = settings.CreateOrMoveEntry(guid, group);
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var name = Path.GetFileNameWithoutExtension(path);
			entry.SetAddress(name);
			entry.labels.Clear();

			foreach (var label in labels) entry.SetLabel(label.Label, true);

			Debug.Assert(!path.IsNullOrEmpty());
			AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant("aot support", "");

			return name;
		}

		public static void RemoveFromAddressables(Object asset) {
			if (!asset) return;

			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _)) throw new Exception($"Cannot get GUID for asset: '{asset.name}'");

			var settings = AddressableAssetSettingsDefaultObject.Settings;
			settings.RemoveAssetEntry(guid);
		}

		public static T GetMainAsset<T>(string address) where T : Object {
			var groups = EditorUtils.LoadAssets<AddressableAssetGroup>();

			AddressableAssetEntry entry = null;
			foreach (var assetGroup in groups) {
				entry = assetGroup.entries.FirstOrDefault(x => x.address == address);
				if (entry != null) break;
			}

			return entry?.MainAsset as T;
		}

		public static void CacheGroups() {
			_assetGroups.Clear();
			_assetGroups = EditorUtils.LoadAssets<AddressableAssetGroup>();
		}

		public static bool IsAssetCached(string address) {
			var result = false;

			if (_assetGroups.Count == 0) throw new Exception("Please cached AddressableAssetGroup");

			foreach (var assetGroup in _assetGroups) {
				var entry = assetGroup.entries.FirstOrDefault(x => x.address == address);
				if (entry != null) {
					result = true;
					break;
				}
			}

			return result;
		}

	}

}
