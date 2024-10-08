#if !UNITY3D

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Utils;
using XLib.Core.Json;
using XLib.Core.Reflection;
using XLib.Localization.Public;

namespace XLib.Configs.Storage {

	public class YamlLocalStorageProvider : IDataStorageProvider {

		private readonly string _assetsDir;
		private readonly IUnityLogger _logger;
		private string _configHash;
		private readonly Assembly[] _assemblies;

		public YamlLocalStorageProvider(string assetsPath, IUnityLogger logger) {
			_assetsDir = assetsPath;
			_logger = logger;
			_assemblies = AssemblyUtils.GetAssembliesReferencingAn(typeof(AssemblyUtils).Assembly);

		}

		public Task LoadLocalization() => Task.CompletedTask;

		public Task<IEnumerable<IGameItem>> LoadAll() {
			try {
				var allAssets = ConfigUtils.GetAllAssets(_assetsDir);
				_configHash = ConfigUtils.CalculateHash(allAssets);

				var storage = new YamlStorage(_logger);
				var serializer = new YamlSerializer(storage, _logger);
				var typeProvider = new ManifestTypeProvider(_assemblies);
				storage.PhaseLoading(allAssets, _assetsDir);
				typeProvider.PhaseExtractTypes(storage);
				storage.PhaseConstruction(typeProvider);
				
				return Task.FromResult(PhaseSerializing(storage, serializer));
			}
			finally {
				GC.Collect();
			}
		}

		private IEnumerable<IGameItem> PhaseSerializing(YamlStorage storage, YamlSerializer serializer) {
			Debug.Log("Building config structures: Third pass (deserialize and link)");
			foreach (var entry in storage.Entries.Values) {
				var result = serializer.DeserializeEntry(entry);
				if (result is IGameItem dbItem) yield return dbItem;
			}
		}

		public Task<string> GetConfigHash() => Task.FromResult(_configHash);

	}

}
#endif