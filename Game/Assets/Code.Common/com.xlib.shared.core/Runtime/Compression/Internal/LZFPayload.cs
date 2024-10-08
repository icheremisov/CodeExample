using System;
using System.Diagnostics.CodeAnalysis;

namespace XLib.Core.Runtime.Compression.Internal {

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	internal struct LZFPayload {

		public static readonly uint PAYLOAD_DATA_SIZE = sizeof(uint) + sizeof(byte);

		public uint UncompressedDataSize;
		public bool IsDataCompressed;

		public LZFPayload(int uncompressedDataSize, bool isDataCompressed) {
			if (uncompressedDataSize < 0 || uncompressedDataSize >= LZF.MAX_UNCOMPRESSED_DATA_SIZE) throw new Exception("Invalid uncompressed data size: " + uncompressedDataSize);

			UncompressedDataSize = (uint)uncompressedDataSize;
			IsDataCompressed = isDataCompressed;
		}

		public LZFPayload(byte[] compressedData, uint bytesOffset, out uint offset) {
			offset = bytesOffset;

			UncompressedDataSize = (uint)((compressedData[offset] << 24) | (compressedData[offset + 1] << 16) | (compressedData[offset + 2] << 8) | compressedData[offset + 3]);
			offset += 4;

			IsDataCompressed = compressedData[offset] == 1;
			offset++;
		}

		public int Apply(byte[] buffer, uint bytesOffset) {
			if (buffer.Length < PAYLOAD_DATA_SIZE) throw new Exception("Invalid buffer size " + buffer.Length);

			var offset = (int)bytesOffset;
			buffer[offset] = (byte)(UncompressedDataSize >> 24);
			offset++;

			buffer[offset] = (byte)(UncompressedDataSize >> 16);
			offset++;

			buffer[offset] = (byte)(UncompressedDataSize >> 8);
			offset++;

			buffer[offset] = (byte)UncompressedDataSize;
			offset++;

			buffer[offset] = (byte)(IsDataCompressed ? 1 : 0);
			offset++;

			return offset;
		}

	}

}