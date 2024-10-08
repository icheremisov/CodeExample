namespace XLib.Unity.LocalStorage {

	public partial class LocalProfileStorage {

		public static void LoadProfile(string profileId, string overrideKey = null) => S.Load(profileId, overrideKey);
		public static void CloseProfile() => S.Close();
		public static void DeleteAllKeys(string overrideKey = null) => S.Clear(overrideKey);
	}

}