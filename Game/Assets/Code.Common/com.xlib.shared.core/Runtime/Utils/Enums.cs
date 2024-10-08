using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using XLib.Core.Reflection;

namespace XLib.Core.Utils {

	public class EnumComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : unmanaged, Enum {
		public static EnumComparer<TEnum> Default = new();
		private EnumComparer() { }
		public bool Equals(TEnum x, TEnum y) => Enums.ToIntegral<TEnum, int>(x) == Enums.ToIntegral<TEnum, int>(y);
		public int GetHashCode(TEnum obj) => Enums.ToIntegral<TEnum, int>(obj);
	}

	[SuppressMessage("ReSharper", "UnusedType.Global"), SuppressMessage("ReSharper", "CheckNamespace")]
	public static class Enums {
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AsInt<TEnum>(this TEnum value) where TEnum : unmanaged, Enum => ToIntegral<TEnum, int>(value);

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long AsLong<TEnum>(this TEnum value) where TEnum : unmanaged, Enum => ToIntegral<TEnum, long>(value);

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TEnum ToEnum<TEnum>(this int value) where TEnum : unmanaged, Enum => ToEnum<TEnum, int>(value);

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TEnum ToEnum<TEnum>(this long value) where TEnum : unmanaged, Enum => ToEnum<TEnum, long>(value);

		/// <summary>
		///     convert enum to integer value
		/// </summary>
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe TValue ToIntegral<TEnum, TValue>(TEnum value)
			where TEnum : unmanaged, Enum
			where TValue : unmanaged {
			if (sizeof(TValue) > sizeof(TEnum)) {
				TValue o = default;
				*(TEnum*)&o = value;
				return o;
			}

			return *(TValue*)&value;
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe TEnum ToEnum<TEnum, TValue>(TValue value)
			where TEnum : unmanaged, Enum
			where TValue : unmanaged {
			if (sizeof(TEnum) > sizeof(TValue)) {
				TEnum o = default;
				*(TValue*)&o = value;
				return o;
			}

			return *(TEnum*)&value;
		}

		/// <summary>
		///     return count items of an enum
		/// </summary>
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Count<T>() where T : unmanaged, Enum => EnumCount<T>.Count;

		/// <summary>
		///     return all items of an enum
		/// </summary>
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Values<T>() where T : unmanaged, Enum {
			if (!TypeOf<T>.Raw.IsEnum) throw new ArgumentException($"T must be an enumerated type but found {TypeOf<T>.Raw.FullName}");

			return (T[])Enum.GetValues(TypeOf<T>.Raw);
		}

		/// <summary>
		///     return all items of an enum
		/// </summary>
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Array Values(Type type) {
			if (!type.IsEnum) throw new ArgumentException($"Type must be an enumerated type but found {type.FullName}");

			return Enum.GetValues(type);
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ToEnum<T>(string str, bool ignoreCase = true) where T : unmanaged, Enum {
			T result;
			try {
				result = (T)Enum.Parse(TypeOf<T>.Raw, str, ignoreCase);
				if (!Enum.IsDefined(TypeOf<T>.Raw, result)) throw new Exception();
			}
			catch (Exception) {
				throw new ArgumentException($"Unknown enum ({TypeOf<T>.Raw.Name}) value: '{str}'");
			}

			return result;
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryToEnum<T>(string str, out T result, bool ignoreCase = true) where T : unmanaged, Enum {
			try {
				if (!Enum.TryParse(TypeOf<T>.Raw, str, ignoreCase, out var resultObj)) {
					result = default;
					return false;
				}

				result = (T)resultObj;

				if (Enum.IsDefined(TypeOf<T>.Raw, result)) return true;
			}
			catch {
				// ignored
			}

			result = default;
			return false;
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object ToEnum(Type t, string str, bool ignoreCase = true) {
			try {
				return Enum.Parse(t, str, ignoreCase);
			}
			catch (Exception) {
				throw new ArgumentException($"Unknown enum ({t.Name}) value: \'{str}\'");
			}
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryToEnum(Type t, string str, out object result, bool ignoreCase = true) {
			try {
				return Enum.TryParse(t, str, ignoreCase, out result);
			}
			catch {
				result = default;
				return false;
			}
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryToEnum<T, TValue>(TValue value, out T result) where T : unmanaged, Enum
			where TValue : unmanaged {
			if (!Enum.IsDefined(TypeOf<T>.Raw, value)) {
				result = default;
				return false;
			}

			result = ToEnum<T, TValue>(value);

			return true;
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDefined<T>(T value) where T : unmanaged, Enum => Enum.IsDefined(TypeOf<T>.Raw, value);

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFlagEnum(Type type) {
			if (!type.IsEnum) throw new ArgumentException($"T must be an enumerated type but found {type.FullName}");

			return type.HasAttribute<FlagsAttribute>();
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFlagEnum<TEnum>() where TEnum : unmanaged, Enum => TypeOf<TEnum>.Raw.HasAttribute<FlagsAttribute>();

		public static List<TEnum> UnpackFlagsOffsetToValue<TEnumFlag, TEnum>(this TEnumFlag types) where TEnumFlag : unmanaged, Enum where TEnum : unmanaged, Enum {
			if (!IsFlagEnum<TEnumFlag>()) throw new ArgumentException($"{TypeOf<TEnumFlag>.Name} must be an enumerated type with attribute [Flags]");

			var counts = (int)MathEx.NumberOfSetBits(ToIntegral<TEnumFlag, uint>(types));
			var result = new List<TEnum>(counts);
			var i = 0;
			foreach (var value in Values<TEnum>()) {
				var v = (1 << value.AsInt()).ToEnum<TEnumFlag>();
				if (!types.Has(v)) continue;
				result.Add(value);
				++i;
			}

			return result;
		}

		public static TEnumFlag PackEnumListToFlags<TEnum, TEnumFlag>(this IReadOnlyCollection<TEnum> types) where TEnumFlag : unmanaged, Enum where TEnum : unmanaged, Enum {
			if (!IsFlagEnum<TEnumFlag>()) throw new ArgumentException($"{TypeOf<TEnumFlag>.Name} must be an enumerated type with attribute [Flags]");
			var result = ToIntegral<TEnumFlag, int>(default);
			foreach (var type in types) result |= ToIntegral<TEnumFlag, int>((1 << type.AsInt()).ToEnum<TEnumFlag>());
			return result.ToEnum<TEnumFlag>();
		}

		public static T GetAttribute<T>(this Enum value) where T : Attribute {
			var type = value.GetType();
			var memberInfo = type.GetMember(value.ToString());
			return memberInfo.Length > 0 ? memberInfo[0].GetAttribute<T>() : null;
		}

		public static bool HasAttribute<T>(this Enum value) where T : Attribute {
			var type = value.GetType();
			var memberInfo = type.GetMember(value.ToString());
			return memberInfo.Length > 0 && memberInfo[0].HasAttribute<T>();
		}

		private static class EnumCount<T> where T : unmanaged, Enum {
			// ReSharper disable once StaticMemberInGenericType
			private static int? _count;

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false)]
			public static int Count {
				get {
					if (!_count.HasValue) _count = Enum.GetNames(TypeOf<T>.Raw).Length;

					return _count.Value;
				}
			}
		}
	}

}