using System.Diagnostics.CodeAnalysis;
using XLib.Core.Runtime.Compression.Internal;

namespace XLib.Core.Runtime.Compression {

	/// <summary>
	///     compress data using fast LZF compression method
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class LZFCompressor : ICompressor {

		private readonly LZF _lzf = new();

		public byte[] Compress(byte[] bytes) => _lzf.CompressBytes(bytes);

		public byte[] Compress(byte[] bytes, int bytesCount) => _lzf.CompressBytes(bytes, bytesCount);

		public byte[] Decompress(byte[] bytes) => _lzf.DecompressBytes(bytes);

		public byte[] Decompress(byte[] bytes, int bytesCount) => _lzf.DecompressBytes(bytes, bytesCount);

		public int Compress(byte[] srcBuffer, int srcBytesCount, byte[] dstBuffer, uint dstBytesOffset) => _lzf.CompressBytes(srcBuffer, srcBytesCount, dstBuffer, dstBytesOffset);

		public int Decompress(byte[] srcBuffer, int srcBytesCount, uint srcBytesOffset, byte[] dstBuffer) =>
			_lzf.DecompressBytes(srcBuffer, srcBytesCount, srcBytesOffset, dstBuffer);

	}

}