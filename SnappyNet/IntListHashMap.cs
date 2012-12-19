using System;
using System.Collections.Generic;
using System.Linq;

namespace SnappyNet
{
	internal sealed class IntListHashMap
	{
		private readonly int[][] _content;
		private readonly int _buckets;

		public IntListHashMap(int buckets)
		{
			_buckets = buckets;
			_content = new int[buckets][];
		}

		internal void Put(int key, int value)
		{
			int bucket = key % _buckets;
			if (bucket < 0)
			{
				bucket = -bucket;
			}
			int[] data = _content[bucket];
			if (data == null)
			{
				data = new int[33];
				_content[bucket] = data;

			}
			int off = data[0] * 2 + 1;
			// eliminate duiplicates
			if (off == 1 || data[off - 2] != key || data[off - 1] != value)
			{
				if (off >= data.Length)
				{
					var ndata = new int[(data.Length - 1) * 2 + 1];
					Array.Copy(data, 0, ndata, 0, off);
					data = ndata;
					_content[bucket] = data;
				}
				data[off++] = key;
				data[off] = value;
				data[0]++;
			}
		}

		internal IEnumerable<int> GetReverse(int key)
		{
			int bucket = key % _buckets;
			if (bucket < 0)
			{
				bucket = -bucket;
			}
			int[] data = _content[bucket];

			if (data == null)
			{
				return Enumerable.Empty<int>();
			}
			return new IntEnumerable(data, key);
		}

		internal int GetFirstHit(int key, int maxValue)
		{
			int bucket = key % _buckets;
			if (bucket < 0)
			{
				bucket = -bucket;
			}
			int[] data = _content[bucket];

			if (data == null)
			{
				return -1;
			}
			int offset = data[0] * 2 - 1;
			while (offset > 0)
			{
				if (data[offset] == key && data[offset + 1] <= maxValue)
				{
					return data[offset + 1];
				}
				offset -= 2;
			}
			return -1;
		}
	}
}