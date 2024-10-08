using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

/// <summary>
///     convert types
/// </summary>
// ReSharper disable once CheckNamespace
public static class ConvertUtils {
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TDest reinterpret_cast<TSource, TDest>(TSource source) {
		// TODO make research to avoid boxing for value types
		return (TDest)(object)source;
		
	}	
	
	public static long ToLong(long s0, long s1, long s2, long s3) => ((long)(short)s0 << 0x30) + ((long)(short)s1 << 0x20) + ((long)(short)s2 << 0x10) + (short)s3;

	public static long ToLong(int s0, int s1, int s2, int s3) => ((long)(short)s0 << 0x30) + ((long)(short)s1 << 0x20) + ((long)(short)s2 << 0x10) + (short)s3;

	public static long ToLong(short s0, short s1, short s2, short s3) => ((long)s0 << 0x30) + ((long)s1 << 0x20) + ((long)s2 << 0x10) + s3;

	public static short[] ToShortArray(long src) {
		return new[] {
			(short)((src >> 0x30) & 0xFFFF),
			(short)((src >> 0x20) & 0xFFFF),
			(short)((src >> 0x10) & 0xFFFF),
			(short)(src & 0xFFFF)
		};
	}

	public static unsafe Color32 ToColor32(uint argb) => *(Color32*)&argb;
	public static unsafe uint ToARGB(this Color32 c) => *(uint*)&c;
	
	public static Color ToColor(uint argb) => ToColor32(argb);
	public static uint ToARGB(this Color c) => ToARGB((Color32)c);

	public static bool TryParseBoolean(this string v, out bool result) {
		v = v?.Trim().ToLowerInvariant();

		if (!v.IsNullOrEmpty()) {
			switch (v) {
				case "true" or "1" or "yes" or "y" or "+" or "x":
					result = true;
					return true;

				case "false" or "0" or "no" or "n" or "-":
					result = false;
					return true;
			}
		}

		result = false;
		return false;
	}
	
}