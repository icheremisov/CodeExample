using System.Threading;
using System.Threading.Tasks;

namespace XLib.Core.Utils {

	/// <summary>
	///     <para>Defines a provider for progress updates.</para>
	/// </summary>
	/// <typeparam name="T">To be added.</typeparam>
	public interface IProgressAsync<in T> {

		/// <summary>To be added.</summary>
		/// <param name="value">To be added.</param>
		/// <param name="ct">token to cancel async operation</param>
		Task ReportAsync(T value, CancellationToken ct = default);

	}

}