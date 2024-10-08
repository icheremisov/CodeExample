namespace XLib.Assets.Types {

	public static class AddressableCategory {

		/// <summary>
		///     default behaviour
		/// </summary>
		public const string Default = Persistent;

		/// <summary>
		///     resources loaded once and never unloaded
		/// </summary>
		public const string Persistent = "Persistent";

		/// <summary>
		///     resources unloaded when level changed
		/// </summary>
		public const string Level = "Level";

	}

}