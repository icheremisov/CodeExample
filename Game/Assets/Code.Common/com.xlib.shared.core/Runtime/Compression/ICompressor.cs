namespace XLib.Core.Runtime.Compression {

	/// <summary>
	///     compress and decompress data
	/// </summary>
	public interface ICompressor {

		byte[] Compress(byte[] bytes);
		byte[] Compress(byte[] bytes, int bytesCount);
		byte[] Decompress(byte[] bytes);
		byte[] Decompress(byte[] bytes, int bytesCount);
		int Compress(byte[] srcBuffer, int srcBytesCount, byte[] dstBuffer, uint dstBytesOffset);
		int Decompress(byte[] srcBuffer, int srcBytesCount, uint srcBytesOffset, byte[] dstBuffer);

	}

}