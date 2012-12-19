using System;

namespace SnappyNet
{
	public static class SnappyDecompressor
	{
		public static Buffer Decompress(byte[] inputBytes)
		{
			return Decompress(inputBytes, 0, inputBytes.Length, null);
		}

		public static Buffer Decompress(byte[] inputBytes, Buffer buffer)
		{
			return Decompress(inputBytes, 0, inputBytes.Length, buffer);
		}

		public static Buffer Decompress(byte[] inputBytes, int offset, int length)
		{
			return Decompress(inputBytes, offset, length, null);
		}

		public static Buffer Decompress(Buffer inputBytes)
		{
			return Decompress(inputBytes.Data, 0, inputBytes.Length);
		}

		public static Buffer Decompress(Buffer inputBytes, Buffer buffer)
		{
			return Decompress(inputBytes.Data, 0, inputBytes.Length, buffer);
		}

		public static Buffer Decompress(byte[] inputBytes, int offset, int length, Buffer buffer)
		{
			int sourceIndex = offset, targetIndex = 0;
			int targetLength = 0;
			if (length == 0) {
				if (buffer != null) {
					buffer.Length = 0;
					return buffer;
				}
				return new Buffer(0);
			}
			int i = 0;
			do
			{
				targetLength += (inputBytes[sourceIndex] & 0x7f) << (i++ * 7);
			} while ((inputBytes[sourceIndex++] & 0x80) == 0x80);

			if (buffer == null)
			{
				buffer = new Buffer(targetLength);
			}
			else
			{
				buffer.EnsureCapacity(targetLength);
			}

			buffer.Length = targetLength;
			byte[] outBuffer = buffer.Data;

			while (sourceIndex < offset + length)
			{

				if (targetIndex >= targetLength)
				{
					throw new FormatViolationException("Superfluous input data encountered on offset " + sourceIndex, sourceIndex);
				}

				int l;
				int o;
				int c;
				switch (inputBytes[sourceIndex] & 3)
				{
					case 0:
						l = (inputBytes[sourceIndex++] >> 2) & 0x3f;
						switch (l)
						{
							case 60:
								l = inputBytes[sourceIndex++] & 0xff;
								l++;
								break;
							case 61:
								l = inputBytes[sourceIndex++] & 0xff;
								l |= (inputBytes[sourceIndex++] & 0xff) << 8;
								l++;
								break;
							case 62:
								l = inputBytes[sourceIndex++] & 0xff;
								l |= (inputBytes[sourceIndex++] & 0xff) << 8;
								l |= (inputBytes[sourceIndex++] & 0xff) << 16;
								l++;
								break;
							case 63:
								l = inputBytes[sourceIndex++] & 0xff;
								l |= (inputBytes[sourceIndex++] & 0xff) << 8;
								l |= (inputBytes[sourceIndex++] & 0xff) << 16;
								l |= (inputBytes[sourceIndex++] & 0xff) << 24;
								l++;
								break;
							default:
								l++;
								break;
						}
						Array.ConstrainedCopy(inputBytes, sourceIndex, outBuffer, targetIndex, l);
						sourceIndex += l;
						targetIndex += l;
						break;
					case 1:
						l = 4 + ((inputBytes[sourceIndex] >> 2) & 7);
						o = (inputBytes[sourceIndex++] & 0xe0) << 3;
						o |= inputBytes[sourceIndex++] & 0xff;
						if (l < o)
						{
							Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, l);
							targetIndex += l;
						}
						else
						{
							if (o == 1)
							{
								Fill(outBuffer, targetIndex, targetIndex + l, outBuffer[targetIndex - 1]);
								targetIndex += l;
							}
							else
							{
								while (l > 0)
								{
									c = l > o ? o : l;
									Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, c);
									targetIndex += c;
									l -= c;
								}
							}
						}
						break;
					case 2:
						l = ((inputBytes[sourceIndex++] >> 2) & 0x3f) + 1;
						o = inputBytes[sourceIndex++] & 0xff;
						o |= (inputBytes[sourceIndex++] & 0xff) << 8;
						if (l < o)
						{
							Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, l);
							targetIndex += l;
						}
						else
						{
							while (l > 0)
							{
								c = l > o ? o : l;
								Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, c);
								targetIndex += c;
								l -= c;
							}
						}
						break;
					case 3:
						l = ((inputBytes[sourceIndex++] >> 2) & 0x3f) + 1;
						o = inputBytes[sourceIndex++] & 0xff;
						o |= (inputBytes[sourceIndex++] & 0xff) << 8;
						o |= (inputBytes[sourceIndex++] & 0xff) << 16;
						o |= (inputBytes[sourceIndex++] & 0xff) << 24;
						if (l < o)
						{
							Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, l);
							targetIndex += l;
						}
						else
						{
							if (o == 1)
							{
								Fill(outBuffer, targetIndex, targetIndex + l, outBuffer[targetIndex - 1]);
								targetIndex += l;
							}
							else
							{
								while (l > 0)
								{
									c = l > o ? o : l;
									Array.ConstrainedCopy(outBuffer, targetIndex - o, outBuffer, targetIndex, c);
									targetIndex += c;
									l -= c;
								}
							}
						}
						break;
				}
			}

			return buffer;
		}

		public static void Fill<T>(T[] array, int start, int end, T value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (start < 0 || start >= end)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (end >= array.Length)
			{
				throw new ArgumentOutOfRangeException("end");
			}
			for (int i = start; i < end; i++)
			{
				array[i] = value;
			}
		}
	}
}
