using System;

namespace XLib.States.Contracts {

	/// <summary>
	///     create all states
	/// </summary>
	public interface IStateFactory<out TState> {

		TState Create(Type stateType);

	}

}