using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using XLib.Core.Utils;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

public static class EnumExtensions {
	//checks if the value contains the provided type
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Has<T>(this T type, T value) where T : unmanaged, Enum {
		var t = Enums.ToIntegral<T, long>(type);
		var v = Enums.ToIntegral<T, long>(value);

		return (t & v) == v;
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasAny<T>(this T type, T value) where T : unmanaged, Enum {
		var t = Enums.ToIntegral<T, long>(type);
		var v = Enums.ToIntegral<T, long>(value);
		return (t & v) != 0;
	}


	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T With<T>(this T type, T value) where T : unmanaged, Enum {
		var t = Enums.ToIntegral<T, long>(type);
		var v = Enums.ToIntegral<T, long>(value);

		return Enums.ToEnum<T, long>(t | v);
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Remove<T>(this T type, T value) where T : unmanaged, Enum {
		var t = Enums.ToIntegral<T, long>(type);
		var v = Enums.ToIntegral<T, long>(value);

		return Enums.ToEnum<T, long>(t & ~v);
	}

	//add or remove value based on bool input
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T With<T>(this T type, T value, bool isOn) where T : unmanaged, Enum => isOn ? type.With(value) : type.Remove(value);

	public static T GetAttributeOfType<T>(this Enum value) where T : Attribute =>
		(T)value.GetType()
			.GetMember(value.ToString())
			.FirstOrDefault()
			?.GetCustomAttribute(typeof(T), false);
}