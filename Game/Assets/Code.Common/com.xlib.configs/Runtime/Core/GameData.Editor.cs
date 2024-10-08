#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using XLib.Configs;
using XLib.Configs.Contracts;

[InitializeOnLoad]
[SuppressMessage("ReSharper", "CheckNamespace")]
public static partial class GameData {

	static GameData() {
		_instance = !Application.isPlaying ? GameDatabase_Editor.Instance : null;
	}

	public static void SetInstance(IGameDatabase gameDatabase) {
		_instance = gameDatabase;
	}
}
#endif