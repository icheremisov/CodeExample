using System.Threading.Tasks;

namespace XLib.Core.AsyncEx {

	public interface IAsyncOperation<T> {

		Task<T> GetResult();

	}

}