using System;

namespace XLib.Core.Parsers.Exceptions {

	public class RowValueException : Exception {

		public RowValueException() { }

		public RowValueException(string message) : base(message) { }
		public override string StackTrace => "";

	}

}