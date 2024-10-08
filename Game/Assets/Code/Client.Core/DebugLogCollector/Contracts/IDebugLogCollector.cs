namespace Client.Core.DebugLogCollector.Contracts {

	public interface IDebugLogCollector {
		int TotalErrors { get; }
		public string GetLog();
		string GetErrorLog();
		string FirstError { get; }
		void ClearLog();
	}

}