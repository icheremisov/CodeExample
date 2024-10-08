#if UNITY3D
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XLib.Assets.Cache;
using XLib.Assets.Contracts;
using XLib.Configs.Contracts;
using XLib.Configs.Core;

namespace XLib.Configs.Storage {

	public class AddressableStorageProvider : IDataStorageProvider {
		private readonly IAssetProvider _provider;
		private readonly string _configHash;

		// now ths called at start of UnityApplication.Load - fix double-loading
		private bool _localizationLoaded;

		public AddressableStorageProvider(IAssetProvider provider, string configHash) {
			_provider = provider;
			_configHash = configHash;
		}
		
		public async Task<IEnumerable<IGameItem>> LoadAll() =>
			(await _provider
				.LoadByLabelAsync<ConfigManifest>(new AssetLabel($"ConfigManifest"))).ToHashSet()
			.SelectMany(asset => asset.Configs);

		public Task<string> GetConfigHash() => Task.FromResult(_configHash);
	}

}

#endif