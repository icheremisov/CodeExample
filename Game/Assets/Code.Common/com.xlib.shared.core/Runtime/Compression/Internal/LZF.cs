//
// http://forum.unity3d.com/threads/lzf-compression-and-decompression-for-unity.152579/
//

/*
 * Improved version to C# LibLZF Port:
 * Copyright (c) 2010 Roman Atachiants <kelindar@gmail.com>
 *
 * Original CLZF Port:
 * Copyright (c) 2005 Oren J. Maurice <oymaurice@hazorea.org.il>
 *
 * Original LibLZF Library  Algorithm:
 * Copyright (c) 2000-2008 Marc Alexander Lehmann <schmorp@schmorp.de>
 *
 * Redistribution and use in source and binary forms, with or without modifica-
 * tion, are permitted provided that the following conditions are met:
 *
 *   1.  Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *
 *   2.  Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 *   3.  The name of the author may not be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MER-
 * CHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO
 * EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPE-
 * CIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTH-
 * ERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Alternatively, the contents of this file may be used under the terms of
 * the GNU General Public License version 2 (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of the above. If you wish to
 * allow the use of your version of this file only under the terms of the
 * GPL and not to allow others to use your version of this file under the
 * BSD license, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the GPL. If
 * you do not delete the provisions above, a recipient may use your version
 * of this file under either the BSD or the GPL.
 */

/* Benchmark with Alice29 Canterbury Corpus
		---------------------------------------
		(Compression) Original CLZF C#
		Raw = 152089, Compressed = 101092
		 8292,4743 ms.
		---------------------------------------
		(Compression) My LZF C#
		Raw = 152089, Compressed = 101092
		 33,0019 ms.
		---------------------------------------
		(Compression) Zlib using SharpZipLib
		Raw = 152089, Compressed = 54388
		 8389,4799 ms.
		---------------------------------------
		(Compression) QuickLZ C#
		Raw = 152089, Compressed = 83494
		 80,0046 ms.
		---------------------------------------
		(Decompression) Original CLZF C#
		Decompressed = 152089
		 16,0009 ms.
		---------------------------------------
		(Decompression) My LZF C#
		Decompressed = 152089
		 15,0009 ms.
		---------------------------------------
		(Decompression) Zlib using SharpZipLib
		Decompressed = 152089
		 3577,2046 ms.
		---------------------------------------
		(Decompression) QuickLZ C#
		Decompressed = 152089
		 21,0012 ms.
	*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace XLib.Core.Runtime.Compression.Internal {

	/// <summary>
	///     Improved C# LZF Compressor, a very small data compression library. The compression algorithm is extremely fast.
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	internal class LZF {

		public const int MAX_UNCOMPRESSED_DATA_SIZE = 1024 /*kb*/ * 1024 /*mb*/ * 10;
		private const uint MIN_BYTES_REQUIRED_TO_COMPRESS_DATA = 40;
		private const uint HLOG = 14;
		private const uint HSIZE = 1 << 14;
		private const uint MAX_LIT = 1 << 5;
		private const uint MAX_OFF = 1 << 13;
		private const uint MAX_REF = (1 << 8) + (1 << 3);

		/// <summary>
		///     Hashtable and temp buffer, that can be allocated only once
		/// </summary>
		private readonly long[] HashTable = new long[HSIZE];

		private readonly byte[] tempBuffer = new byte[MAX_UNCOMPRESSED_DATA_SIZE + LZFPayload.PAYLOAD_DATA_SIZE];

		private bool debugEmulateCompressionFail;

		// Intended for use by unit tests
		public void DebugSetCompressionFailEmulation(bool emulate) {
			debugEmulateCompressionFail = emulate;
		}

		public byte[] CompressBytes(byte[] input) => CompressBytes(input, input.Length);

		public int CompressBytes(byte[] input, byte[] outputBuffer) => CompressBytes(input, input.Length, outputBuffer);

		public byte[] CompressBytes(byte[] input, int inputBytes) {
			var compressedDataSize = CompressBytes(input, inputBytes, tempBuffer);

			var result = new byte[compressedDataSize];
			Array.Copy(tempBuffer, 0, result, 0, compressedDataSize);

			return result;
		}

		public int CompressBytes(byte[] input, int inputBytes, byte[] outputBuffer) => CompressBytes(input, inputBytes, outputBuffer, 0);

		public int CompressBytes(byte[] input, int inputBytes, byte[] outputBuffer, uint outputBytesOffset) {
			if (inputBytes <= 0 || inputBytes >= MAX_UNCOMPRESSED_DATA_SIZE) {
				Console.WriteLine("LZF.CompressBytes() - invalid input bytes count: " + inputBytes);
				return 0;
			}

			var payload = new LZFPayload(inputBytes, true);
			var offset = payload.Apply(outputBuffer, outputBytesOffset);

			var compressedDataBytesCount = 0;
			if (inputBytes > MIN_BYTES_REQUIRED_TO_COMPRESS_DATA && !debugEmulateCompressionFail)
				compressedDataBytesCount = LZFCompress(input, inputBytes, (uint)offset, outputBuffer);

			if (compressedDataBytesCount == 0) // Copying uncompressed data
			{
				payload = new LZFPayload(inputBytes, false);
				offset = payload.Apply(outputBuffer, outputBytesOffset);

				Array.Copy(input, 0, outputBuffer, offset, inputBytes);
				return inputBytes + offset;
			}

			return compressedDataBytesCount;
		}

		public byte[] DecompressBytes(byte[] input) => DecompressBytes(input, input.Length);

		public int DecompressBytes(byte[] input, byte[] outputBuffer) => DecompressBytes(input, input.Length, outputBuffer);

		public byte[] DecompressBytes(byte[] input, int inputBytes) {
			var decompressedDataSize = DecompressBytes(input, inputBytes, tempBuffer);

			var result = new byte[decompressedDataSize];
			Array.Copy(tempBuffer, 0, result, 0, decompressedDataSize);

			return result;
		}

		public int DecompressBytes(byte[] input, int inputBytes, byte[] outputBuffer) => DecompressBytes(input, inputBytes, 0, outputBuffer);

		public int DecompressBytes(byte[] input, int inputBytes, uint inputBytesOffset, byte[] outputBuffer) {
			uint offset = 0;
			var payload = new LZFPayload(input, inputBytesOffset, out offset);

			var decompressedBytesCount = (int)payload.UncompressedDataSize;

			if (payload.IsDataCompressed)
				decompressedBytesCount = LZFDecompress(input, inputBytes, offset, outputBuffer);
			else {
				try {
					Array.Copy(input, offset, outputBuffer, 0, decompressedBytesCount);
				}
				catch (ArgumentException) {
					Console.WriteLine("LZF.DecompressBytes() - failed to copy decompressed data to buffer! Input buffer size: " + input.Length + ", bytes: " + inputBytes +
						", Output buffer size: " + outputBuffer.Length + ", decompressed bytes count: " + decompressedBytesCount);
					return 0;
				}
			}

			if (decompressedBytesCount != payload.UncompressedDataSize) {
				Console.WriteLine("LZF.DecompressBytes() - decompressed data bytes count " + decompressedBytesCount + " doesn't match ucompressed data bytes count " +
					payload.UncompressedDataSize);
				return 0;
			}

			return decompressedBytesCount;
		}

		public string CompressString(string input) => ActOnString(input, CompressBytes);

		public string DecompressString(string input) => ActOnString(input, DecompressBytes);

		private string ActOnString(string input, Func<byte[], byte[]> act) {
			var bytes = Encoding.Unicode.GetBytes(input);
			var result = act(bytes);
			return Encoding.Unicode.GetString(result);
		}

		public void CompressFile(string path) {
			ActOnFile(path, CompressBytes);
		}

		public void DecompressFile(string path) {
			ActOnFile(path, DecompressBytes);
		}

		private void ActOnFile(string path, Func<byte[], byte[]> act) {
			var input = File.ReadAllBytes(path);
			var output = act(input);
			File.WriteAllBytes(path, output);
		}

		public void CompressFiles(string outputPath, FileInfo[] inputFiles) {
			var inputData = new List<OutputFile>();
			for (var i = 0; i < inputFiles.Length; ++i) {
				var filename = inputFiles[i].Name;
				var bytes = File.ReadAllBytes(inputFiles[i].FullName);
				inputData.Add(new OutputFile(filename, bytes));
			}

			using (var stream = new FileStream(outputPath, FileMode.OpenOrCreate)) {
				using (var writer = new BinaryWriter(stream)) {
					writer.Write(inputData.Count);
					for (var i = 0; i < inputData.Count; ++i) {
						var compressedBytes = CompressBytes(inputData[i].bytes);
						writer.Write(inputData[i].filename);
						writer.Write(BitConverter.GetBytes(compressedBytes.Length));
						writer.Write(compressedBytes);
					}

					writer.Close();
				}
			}
		}

		public void CompressFiles(string outputPath, string[] inputPaths) {
			var inputFiles = new FileInfo[inputPaths.Length];
			for (var i = 0; i < inputPaths.Length; ++i) inputFiles[i] = new FileInfo(inputPaths[i]);
			CompressFiles(outputPath, inputFiles);
		}

		public List<OutputFile> DecompressFiles(string inputPath) {
			var inputBytes = File.ReadAllBytes(inputPath);
			return DecompressFiles(inputBytes);
		}

		public List<OutputFile> DecompressFiles(byte[] inputBytes) {
			var res = new List<OutputFile>();
			using (var stream = new MemoryStream(inputBytes)) {
				using (var reader = new BinaryReader(stream)) {
					var arraySize = reader.ReadInt32();
					for (var i = 0; i < arraySize; ++i) {
						var filename = reader.ReadString();
						var bytesCount = reader.ReadInt32();
						var bytes = reader.ReadBytes(bytesCount);
						res.Add(new OutputFile(filename, DecompressBytes(bytes)));
					}

					reader.Close();
				}
			}

			return res;
		}

		/// <summary>
		///     Compresses the data using LibLZF algorithm
		/// </summary>
		/// <returns>The size of the compressed archive in the output buffer</returns>
		private int LZFCompress(byte[] input, int inputBytes, uint dstBytesOffset, byte[] buffer) {
			var output = buffer;
			var inputLength = inputBytes;
			var outputLength = output.Length;

			Array.Clear(HashTable, 0, (int)HSIZE);

			long hslot;
			uint iidx = 0;
			var oidx = dstBytesOffset;
			long reference;

			var hval = (uint)((input[iidx] << 8) | input[iidx + 1]); // FRST(in_data, iidx);
			long off;
			var lit = 0;

			for (;;) {
				if (iidx < inputLength - 2) {
					hval = (hval << 8) | input[iidx + 2];
					hslot = ((hval ^ (hval << 5)) >> (int)(3 * 8 - HLOG - hval * 5)) & (HSIZE - 1);
					reference = HashTable[hslot];
					HashTable[hslot] = iidx;

					if ((off = iidx - reference - 1) < MAX_OFF
						&& iidx + 4 < inputLength
						&& reference > 0
						&& input[reference + 0] == input[iidx + 0]
						&& input[reference + 1] == input[iidx + 1]
						&& input[reference + 2] == input[iidx + 2]
					   ) {
						/* match found at *reference++ */
						uint len = 2;
						var maxlen = (uint)inputLength - iidx - len;
						maxlen = maxlen > MAX_REF ? MAX_REF : maxlen;

						if (oidx + lit + 1 + 3 >= outputLength) return 0;

						do
							len++;
						while (len < maxlen && input[reference + len] == input[iidx + len]);

						if (lit != 0) {
							output[oidx++] = (byte)(lit - 1);
							lit = -lit;
							do
								output[oidx++] = input[iidx + lit];
							while (++lit != 0);
						}

						len -= 2;
						iidx++;

						if (len < 7)
							output[oidx++] = (byte)((off >> 8) + (len << 5));
						else {
							output[oidx++] = (byte)((off >> 8) + (7 << 5));
							output[oidx++] = (byte)(len - 7);
						}

						output[oidx++] = (byte)off;

						iidx += len - 1;
						hval = (uint)((input[iidx] << 8) | input[iidx + 1]);

						hval = (hval << 8) | input[iidx + 2];
						HashTable[((hval ^ (hval << 5)) >> (int)(3 * 8 - HLOG - hval * 5)) & (HSIZE - 1)] = iidx;
						iidx++;

						hval = (hval << 8) | input[iidx + 2];
						HashTable[((hval ^ (hval << 5)) >> (int)(3 * 8 - HLOG - hval * 5)) & (HSIZE - 1)] = iidx;
						iidx++;
						continue;
					}
				}
				else if (iidx == inputLength) break;

				/* one more literal byte we must copy */
				lit++;
				iidx++;

				if (lit == MAX_LIT) {
					if (oidx + 1 + MAX_LIT >= outputLength) return 0;

					output[oidx++] = (byte)(MAX_LIT - 1);
					lit = -lit;
					do
						output[oidx++] = input[iidx + lit];
					while (++lit != 0);
				}
			}

			if (lit != 0) {
				if (oidx + lit + 1 >= outputLength) return 0;

				output[oidx++] = (byte)(lit - 1);
				lit = -lit;
				do
					output[oidx++] = input[iidx + lit];
				while (++lit != 0);
			}

			return (int)oidx;
		}

		/// <summary>
		///     Decompresses the data using LibLZF algorithm
		/// </summary>
		/// <returns>Returns decompressed size</returns>
		private int LZFDecompress(byte[] input, int inputBytes, uint srcOffset, byte[] buffer) {
			var output = buffer;
			var outputLength = output.Length;

			var iidx = srcOffset;
			uint oidx = 0;

			do {
				uint ctrl = input[iidx++];

				if (ctrl < 1 << 5) /* literal run */ {
					ctrl++;

					if (oidx + ctrl > outputLength) {
						//SET_ERRNO (E2BIG);
						return 0;
					}

					do
						output[oidx++] = input[iidx++];
					while (--ctrl != 0);
				}
				else /* back reference */ {
					var len = ctrl >> 5;

					var reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);

					if (len == 7) len += input[iidx++];

					reference -= input[iidx++];

					if (oidx + len + 2 > outputLength) {
						//SET_ERRNO (E2BIG);
						return 0;
					}

					if (reference < 0) {
						//SET_ERRNO (EINVAL);
						return 0;
					}

					output[oidx++] = output[reference++];
					output[oidx++] = output[reference++];

					do
						output[oidx++] = output[reference++];
					while (--len != 0);
				}
			} while (iidx < inputBytes);

			return (int)oidx;
		}

		public struct OutputFile {

			public string filename;
			public byte[] bytes;

			public OutputFile(string filename, byte[] bytes) {
				this.filename = filename;
				this.bytes = bytes;
			}

		}

	}

}