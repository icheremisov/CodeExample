using System;

namespace XLib.Configs.Storage {

	public class ConfigParsingException : Exception
	{
		public string Path = "";
		public ConfigParsingException(string message, Exception previous = null) : base(message, previous) {}

		public override string Message => base.Message + " Faulty object located at " + Path;

	}

}