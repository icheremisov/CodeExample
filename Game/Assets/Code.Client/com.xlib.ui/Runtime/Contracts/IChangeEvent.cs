using System;

namespace XLib.UI.Contracts {

	public interface IChangeEvent {

		event Action OnChange;

	}

}