namespace Client.Core.Notifications.Types {

	public enum NotifyOptions {
		None = 0,
		
		/// <summary>
		/// hide any UI dialogs
		/// </summary>
		Silent = 1 << 0,
		
		/// <summary>
		/// always show all reward notifications
		/// </summary>
		Force = 1 << 1,
		
		Default = None,
	}

}