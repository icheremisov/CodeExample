namespace Client.Core.Notifications.Types {

	public struct NotifyParams {
		public NotifyParams(string header, string desc) {
			Header = header;
			Desc = desc;
		}
		public string Header { get; set; }
		public string Desc { get; set; }
	}

}