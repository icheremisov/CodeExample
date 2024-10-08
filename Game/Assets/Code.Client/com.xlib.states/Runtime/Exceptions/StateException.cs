using System;

namespace XLib.States.Exceptions {

	public class StateException : Exception {

		public StateException() { }

		public StateException(string message) : base(message) { }

		public StateException(string message, Exception innerException) : base(message, innerException) { }

	}

}