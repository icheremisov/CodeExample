using System.Collections.Generic;
using System.Threading.Tasks;

namespace XLib.Configs.Contracts {

	public interface IDataStorageProvider {
		Task<IEnumerable<IGameItem>> LoadAll();
		Task<string> GetConfigHash();

	}

}