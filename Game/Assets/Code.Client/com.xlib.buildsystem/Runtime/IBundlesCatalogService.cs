using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace XLib.BuildSystem {

	[Serializable]
	public class BundleVersionInfo {
		public string config_hash; // config hash
		public string version; // bundle version
		public int rev; // pipeline id
	}

	[Serializable]
	public class BundleCatalogInfo {
		public BundleVersionInfo[] catalog;
	}

	public interface IBundlesCatalogService {
		Task DownloadCatalog(CancellationToken ct);
		IEnumerable<BundleVersionInfo> GetBundles();
	}

}