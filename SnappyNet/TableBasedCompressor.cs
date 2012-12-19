using System;
using System.Linq;

namespace SnappyNet
{
	internal sealed class TableBasedCompressor
	{
		internal static Buffer Compress(byte[] inputBytes, int offset, int length, Buffer buffer)
		{
			if (buffer == null)
			{
				buffer = new Buffer(32 + length*6/5);
			}
			else
			{
				buffer.EnsureCapacity(32 + length * 6 / 5);
			}

			byte[] target = buffer.Data;
			int targetIndex = 0;
			int lasthit = offset;

			int l = length;
			while (l > 0)
			{
				if (l >= 128)
				{
					target[targetIndex++] = (byte) (0x80 | (l & 0x7f));
				}
				else
				{
					target[targetIndex++] = (byte) l;
				}
				l >>= 7;
			}
			int[] ilhm = Enumerable.Repeat(-1, length/5).ToArray();

			for (int i = offset; i + 4 < length && i < offset + 4; i++)
			{
				ilhm[ToInt(inputBytes, i)%ilhm.Length] = i;
			}

			for (int i = offset + 4; i < offset + length; i++)
			{
				Hit h = Search(inputBytes, i, length, ilhm);
				if (i + 4 < offset + length)
				{
					ilhm[ToInt(inputBytes, i)%ilhm.Length] = i;
				}
				if (h != null)
				{
					if (lasthit < i)
					{
						int len = i - lasthit - 1;
						if (len < 60)
						{
							target[targetIndex++] = (byte) (len << 2);
						}
						else if (len < 0x100)
						{
							target[targetIndex++] = 60 << 2;
							target[targetIndex++] = (byte) len;
						}
						else if (len < 0x10000)
						{
							target[targetIndex++] = 61 << 2;
							target[targetIndex++] = (byte) len;
							target[targetIndex++] = (byte) (len >> 8);
						}
						else if (len < 0x1000000)
						{
							target[targetIndex++] = 62 << 2;
							target[targetIndex++] = (byte) len;
							target[targetIndex++] = (byte) (len >> 8);
							target[targetIndex++] = (byte) (len >> 16);
						}
						else
						{
							target[targetIndex++] = 63 << 2;
							target[targetIndex++] = (byte) len;
							target[targetIndex++] = (byte) (len >> 8);
							target[targetIndex++] = (byte) (len >> 16);
							target[targetIndex++] = (byte) (len >> 24);
						}
						Array.Copy(inputBytes, lasthit, target, targetIndex, i + offset - lasthit);
						targetIndex += i - lasthit;
						lasthit = i;
					}
					if (h.Length <= 11 && h.Offset < 2048)
					{
						target[targetIndex] = 1;
						target[targetIndex] |= (byte) ((h.Length - 4) << 2);
						target[targetIndex++] |= (byte) ((h.Offset >> 3) & 0xe0);
						target[targetIndex++] = (byte) (h.Offset & 0xff);
					}
					else if (h.Offset < 65536)
					{
						target[targetIndex] = 2;
						target[targetIndex++] |= (byte) ((h.Length - 1) << 2);
						target[targetIndex++] = (byte) (h.Offset);
						target[targetIndex++] = (byte) (h.Offset >> 8);
					}
					else
					{
						target[targetIndex] = 3;
						target[targetIndex++] |= (byte) ((h.Length - 1) << 2);
						target[targetIndex++] = (byte) (h.Offset);
						target[targetIndex++] = (byte) (h.Offset >> 8);
						target[targetIndex++] = (byte) (h.Offset >> 16);
						target[targetIndex++] = (byte) (h.Offset >> 24);
					}
					for (; i < lasthit; i++)
					{
						if (i + 4 < inputBytes.Length)
						{
							ilhm[ToInt(inputBytes, i)%ilhm.Length] = i;
						}
					}
					lasthit = i + h.Length;
					while (i < lasthit - 1)
					{
						if (i + 4 < inputBytes.Length)
						{
							ilhm[ToInt(inputBytes, i)%ilhm.Length] = i;
						}
						i++;
					}
				}
				else
				{
					if (i + 4 < length)
					{
						ilhm[ToInt(inputBytes, i)%ilhm.Length] = i;
					}
				}
			}

			if (lasthit < offset + length)
			{
				int len = (offset + length) - lasthit - 1;
				if (len < 60)
				{
					target[targetIndex++] = (byte) (len << 2);
				}
				else if (len < 0x100)
				{
					target[targetIndex++] = 60 << 2;
					target[targetIndex++] = (byte) len;
				}
				else if (len < 0x10000)
				{
					target[targetIndex++] = 61 << 2;
					target[targetIndex++] = (byte) len;
					target[targetIndex++] = (byte) (len >> 8);
				}
				else if (len < 0x1000000)
				{
					target[targetIndex++] = 62 << 2;
					target[targetIndex++] = (byte) len;
					target[targetIndex++] = (byte) (len >> 8);
					target[targetIndex++] = (byte) (len >> 16);
				}
				else
				{
					target[targetIndex++] = 63 << 2;
					target[targetIndex++] = (byte) len;
					target[targetIndex++] = (byte) (len >> 8);
					target[targetIndex++] = (byte) (len >> 16);
					target[targetIndex++] = (byte) (len >> 24);
				}
				Array.ConstrainedCopy(inputBytes, lasthit, target, targetIndex, length + offset - lasthit);
				targetIndex += length - lasthit;
			}
			buffer.Length = targetIndex + offset;
			return buffer;
		}

		private static Hit Search(byte[] source, int index, int length, int[] ilhm)
		{
			if (index + 4 >= length)
			{
				return null;
			}

			if (index > 0 &&
				source[index] == source[index - 1] &&
				source[index] == source[index + 1] &&
				source[index] == source[index + 2] &&
				source[index] == source[index + 3])
			{
				int len = 0;
				for (int i = index; len < 64 && i < length && source[index] == source[i]; i++, len++)
				{
				}
				return new Hit(1, len);
			}

			int fp = ilhm[ToInt(source, index)%ilhm.Length];
			if (fp < 0)
			{
				return null;
			}
			int offset = index - fp;
			if (offset < 4)
			{
				return null;
			}
			int l = 0;
			for (int o = fp, io = index; io < length && source[o] == source[io] && o < index && l < 64; o++, io++)
			{
				l++;
			}
			return l >= 4 ? new Hit(offset, l) : null;
		}

		private static int ToInt(byte[] data, int offset)
		{
			return
				(((data[offset] & 0xff) << 24) |
				((data[offset + 1] & 0xff) << 16) |
				((data[offset + 2] & 0xff) << 8) |
				(data[offset + 3] & 0xff)) & 0x7fffffff;
		}
	}
}