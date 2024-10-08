using System;
using XLib.States.Contracts;

namespace XLib.States.Internal {

	internal static class StateHelpers {

		public static readonly Type PayloadedStateType = typeof(IPayloadedState<>);

	}

}