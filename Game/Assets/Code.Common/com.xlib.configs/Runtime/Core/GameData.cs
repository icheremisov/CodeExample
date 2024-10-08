#if UNITY3D

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XLib.Configs;
using XLib.Configs.Contracts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using XLib.Core.Utils;

// ReSharper disable once CheckNamespace
public static partial class GameData {
	private static IGameDatabase _instance;
	public static IGameDatabase Instance => _instance ?? GameDatabase.Instance ?? GetDefaultDatabase(true);
	public static IGameDatabase InstanceUnsafe => _instance ?? GameDatabase.Instance ?? GetDefaultDatabase(false);
	
	private static IGameDatabase GetDefaultDatabase(bool throwOnError) {
#if UNITY_EDITOR
		if (!UnityAppConstants.isPlaying) return GameDatabase_Editor.Instance;
#endif
		return throwOnError ? throw new Exception($"{nameof(GameData)} not initialized") : null;
	}

	public static void Reset() => _instance = null;

	public static T Get<T>(ItemId id, bool throwOnNotFound = true) where T : class => Instance.Get<T>(id, throwOnNotFound);
	public static IEnumerable<T> All<T>() => Instance.All<T>();
	public static IEnumerable<T> AllAsInterface<T>() => Instance.AllAsInterface<T>();

	public static T Once<T>(bool throwOnNotFound = true) =>
		Instance != null ? Instance.Once<T>(throwOnNotFound) : (throwOnNotFound ? throw new Exception("Database not loading") : default);

#if UNITY_EDITOR
	public static IEnumerable Dropdown<T>(bool withNull = false) where T : IGameItem {
		if (GameDatabase_Editor.Instance == null) return Enumerable.Empty<ValueDropdownItem>();
		var enumerable = GameDatabase_Editor.Instance.AllAsInterface<T>().Select(d => new ValueDropdownItem(d.FileName, d));
		return withNull ? enumerable.PrependWith(() => new ValueDropdownItem("<None>", null)) : enumerable;
	}
#endif
}

#endif