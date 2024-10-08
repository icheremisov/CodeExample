using System.Threading.Tasks;
using XLib.Configs.Contracts;

namespace Client.Core.Common.Contracts {

	public interface IGameDatabaseProvider {

		IGameDatabase Get();
	}


	public interface IClientGameDatabaseProvider : IGameDatabaseProvider {
		Task LoadGameDatabase();

	}

}