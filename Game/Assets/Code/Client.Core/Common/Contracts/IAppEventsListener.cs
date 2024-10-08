using System;

namespace Client.Core.Common.Contracts {

	/// <summary>
	///     subscribe to Application-wide events from non-unity classes
	/// </summary>
	public interface IAppEventsListener {

		event Action<bool> ApplicationPause;
		event Action ApplicationQuit;

	}

}