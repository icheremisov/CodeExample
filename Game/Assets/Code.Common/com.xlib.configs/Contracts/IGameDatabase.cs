using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XLib.Configs.Contracts {

	public interface IGameDatabase : IDisposable {
		Task LoadConfigs(IDataStorageProvider storageProvider);

		T Get<T>(ItemId id, bool throwOnNotFound = true) where T : class;

		IEnumerable<T> All<T>();

		IEnumerable<T> AllAsInterface<T>();

		T Once<T>(bool throwOnNotFound = true);
		int ShortVersion { get; }
		string ConfigHash { get; }
	}

}