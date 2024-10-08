namespace XLib.Assets.Cache {

	public readonly struct AssetLabel {

		public string Label { get; }

		public AssetLabel(string label) {
			Label = label;
		}

		public override string ToString() => Label ?? string.Empty;

	}

}