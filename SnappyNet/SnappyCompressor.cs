using System;

namespace SnappyNet
{
	public static class SnappyCompressor
	{
		private const int DefaultEffort = 1;

		public static Buffer Compress(byte[] inputBytes)
		{
			return Compress(inputBytes, 0, inputBytes.Length, null);
		}
	
		public static Buffer Compress(byte[] inputBytes, Buffer buffer)
		{
			return Compress(inputBytes, 0, inputBytes.Length, buffer);
		}

		public static Buffer Compress(byte[] inputBytes, int offset, int length)
		{
			return Compress(inputBytes, offset, length, null);
		}

		public static Buffer Compress(Buffer inputBytes)
		{
			return Compress(inputBytes.Data, 0, inputBytes.Length, null);
		}

		public static Buffer Compress(Buffer inputBytes, Buffer buffer)
		{
			return Compress(inputBytes.Data, 0, inputBytes.Length, buffer);
		}

		public static Buffer Compress(byte[] inputBytes, int offset, int length, Buffer buffer)
		{
			return Compress(inputBytes, offset, length, buffer, DefaultEffort);
		}

		public static Buffer Compress(byte[] inputBytes, int offset, int length, Buffer buffer, int effort)
		{
			if (effort < 1 || effort > 100)
			{
				throw new ArgumentOutOfRangeException("effort", "Compression effort must be an integer from 0 (fastest, less compression) to 100 (slowest, highest compression)");
			}
			if (effort < 30)
			{
				return TableBasedCompressor.Compress(inputBytes, offset, length, buffer);
			}
			if (effort < 70)
			{
				return MapBasedCompressor.Compress(inputBytes, offset, length, buffer, true);
			}
			return MapBasedCompressor.Compress(inputBytes, offset, length, buffer, false);
		}
	}
}
