using System;

namespace XLib.Core.Parsers.Base {

	/// <remarks>Based on http://www.csharp411.com/convert-binary-to-base64-string/</remarks>
	public class BaseEncoder {

		private const char PaddingChar = '=';

		public readonly char[] CharacterSet;
		private readonly byte[] Map;
		public readonly bool PaddingEnabled;

		public BaseEncoder(char[] characterSet, bool paddingEnabled) {
			PaddingEnabled = paddingEnabled;
			CharacterSet = characterSet;
			Map = Create(CharacterSet);
		}

		public string ToBase(byte[] data) => ToBase(new ArraySegment<byte>(data));

		public string ToBase(ArraySegment<byte> data) {
			int length;
			if (data == null || 0 == (length = data.Count)) return string.Empty;

			unsafe {
				fixed (byte* _d = data.Array)
				fixed (char* _cs = CharacterSet) {
					var d = _d;
					d += data.Offset;

					var padding = length % 3;
					if (padding > 0) padding = 3 - padding;
					var blocks = (length - 1) / 3 + 1;

					var l = blocks * 4;

					var _s = new char[l];

					fixed (char* _sp = _s) {
						var sp = _sp;
						byte b1, b2, b3;

						for (var i = 1; i < blocks; i++) {
							b1 = *d++;
							b2 = *d++;
							b3 = *d++;

							*sp++ = _cs[(b1 & 0xFC) >> 2];
							*sp++ = _cs[((b2 & 0xF0) >> 4) | ((b1 & 0x03) << 4)];
							*sp++ = _cs[((b3 & 0xC0) >> 6) | ((b2 & 0x0F) << 2)];
							*sp++ = _cs[b3 & 0x3F];
						}

						var pad2 = padding == 2;
						var pad1 = padding > 0;

						b1 = *d++;
						b2 = pad2 ? (byte)0 : *d++;
						b3 = pad1 ? (byte)0 : *d++;

						*sp++ = _cs[(b1 & 0xFC) >> 2];
						*sp++ = _cs[((b2 & 0xF0) >> 4) | ((b1 & 0x03) << 4)];
						*sp++ = pad2 ? '=' : _cs[((b3 & 0xC0) >> 6) | ((b2 & 0x0F) << 2)];
						*sp++ = pad1 ? '=' : _cs[b3 & 0x3F];

						if (!PaddingEnabled) {
							if (pad2) l--;
							if (pad1) l--;
						}
					}

					return new string(_s, 0, l);
				}
			}
		}

		public byte[] FromBase(string data, int offset = 0) {
			var length = data == null ? 0 : data.Length - offset;

			if (length <= 0) return new byte[0];

			unsafe {
				fixed (char* _p = data.ToCharArray()) {
					var p2 = _p + offset;

					var blocks = (length - 1) / 4 + 1;
					var bytes = blocks * 3;

					var padding = blocks * 4 - length;

					if (length > 2 && p2[length - 2] == PaddingChar)
						padding = 2;
					else if (length > 1 && p2[length - 1] == PaddingChar) padding = 1;

					var _data = new byte[bytes - padding];

					byte temp1, temp2;
					byte* dp;

					fixed (byte* _d = _data) {
						dp = _d;

						for (var i = 1; i < blocks; i++) {
							temp1 = Map[*p2++];
							temp2 = Map[*p2++];

							*dp++ = (byte)((temp1 << 2) | ((temp2 & 0x30) >> 4));
							temp1 = Map[*p2++];
							*dp++ = (byte)(((temp1 & 0x3C) >> 2) | ((temp2 & 0x0F) << 4));
							temp2 = Map[*p2++];
							*dp++ = (byte)(((temp1 & 0x03) << 6) | temp2);
						}

						temp1 = Map[*p2++];
						temp2 = Map[*p2++];

						*dp++ = (byte)((temp1 << 2) | ((temp2 & 0x30) >> 4));

						temp1 = Map[*p2++];

						if (padding != 2) *dp++ = (byte)(((temp1 & 0x3C) >> 2) | ((temp2 & 0x0F) << 4));

						temp2 = Map[*p2++];
						if (padding == 0) *dp++ = (byte)(((temp1 & 0x03) << 6) | temp2);
					}

					return _data;
				}
			}
		}

		private static byte[] Create(char[] characterSet) {
			var x = new byte[123];

			for (byte i = 0; i < characterSet.Length; i++) x[(byte)characterSet[i]] = i;

			return x;
		}

	}

}