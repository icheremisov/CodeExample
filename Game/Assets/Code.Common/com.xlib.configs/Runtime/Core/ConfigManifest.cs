using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;

namespace XLib.Configs.Core {

	[ValidateAssetName("configs")]
	public partial class ConfigManifest : AssetManifest {
		[SerializeField, Title("Config database"), Space, HideInInspector] protected List<GameItemCore> _configs;
		[ShowInInspector] private int Count => _configs.Count;
		[ShowInInspector, ListDrawerSettings, Searchable]	private List<string> Items => _configs.Select(core => core != null ? core.ToString() : null).ToList();

		public static string ConfigsName => nameof(_configs);
		public IEnumerable<IGameItem> Configs => _configs;
	}

}