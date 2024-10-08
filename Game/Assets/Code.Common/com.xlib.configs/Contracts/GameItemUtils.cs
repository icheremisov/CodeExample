using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;
#endif
using XLib.Core.Utils;

namespace XLib.Configs.Contracts {

	public static class GameItemUtils {
		private const int OffsetZero = '0';
		private const int OffsetA = 'A' - 10;

		public static unsafe string ToKeyString(this ItemId id) {
			var block = stackalloc char[8];
			var idInt = id.AsInt();
			var ofs = 28;
			for (var i = 0; i < 8; i++, ofs -= 4) {
				var chr = (idInt >> ofs) & 0xF;
				block[i] = (char)((chr > 9 ? OffsetA : OffsetZero) + chr);
			}

			return new string(block, 0, 8);
		}

		public static unsafe string ToKeyString(this FileId id) {
			var block = stackalloc char[8];
			var idInt = id.AsInt();
			var ofs = 28;
			for (var i = 0; i < 8; i++, ofs -= 4) {
				var chr = (idInt >> ofs) & 0xF;
				block[i] = (char)((chr > 9 ? OffsetA : OffsetZero) + chr);
			}

			return new string(block, 0, 8);
		}

		public static ItemId ToItemId(this string key) {
			if (key.Length < 8) return ItemId.None;
			var val = 0;
			var ofs = 28;
			for (var i = 0; i < 8; i++, ofs -= 4) {
				var chr = key[i];
				var idx = chr - (chr >= 'A' ? OffsetA : OffsetZero);
				val |= (idx << ofs);
			}

			return val.ToEnum<ItemId>();
		}
		public static FileId ToFileId(this string key) {
			if (key.Length < 8) return FileId.None;
			var val = 0;
			var ofs = 28;
			for (var i = 0; i < 8; i++, ofs -= 4) {
				var chr = key[i];
				var idx = chr - (chr >= 'A' ? OffsetA : OffsetZero);
				val |= (idx << ofs);
			}

			return val.ToEnum<FileId>();
		}

		public static ItemId GuidToItemId(string guid) {
			if (guid.IsNullOrEmpty()) return ItemId.None;
			var g1 = uint.Parse(guid.Substring(0, 8), NumberStyles.HexNumber);
			var g2 = uint.Parse(guid.Substring(8, 8), NumberStyles.HexNumber);
			var g3 = uint.Parse(guid.Substring(16, 8), NumberStyles.HexNumber);
			var g4 = uint.Parse(guid.Substring(24, 8), NumberStyles.HexNumber);
			return Enums.ToEnum<ItemId, uint>(g1 ^ g2 ^ g3 ^ g4);
		}

#if UNITY_EDITOR
		public static ItemId GenerateId() => GuidToItemId(GUID.Generate().ToString());
#endif
		
	}
}