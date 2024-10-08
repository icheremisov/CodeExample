using System;

namespace XLib.Core.AsyncEx.Disposables {

	/// <summary>
	///     A singleton disposable that does nothing when disposed.
	/// </summary>
	public sealed class NoopDisposable : IDisposable {

		private NoopDisposable() { }

		/// <summary>
		///     Gets the instance of <see cref="NoopDisposable" />.
		/// </summary>
		public static NoopDisposable Instance { get; } = new();

		/// <summary>
		///     Does nothing.
		/// </summary>
		public void Dispose() { }

	}

}