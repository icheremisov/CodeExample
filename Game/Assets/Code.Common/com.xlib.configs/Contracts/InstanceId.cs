namespace XLib.Configs.Contracts {

	public enum InstanceId {
		None = 0
	}

	public static class InstanceIdExtension {
		public static bool IsNone(this InstanceId id) => id == InstanceId.None;
	}

}