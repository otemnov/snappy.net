using System.Collections;
using System.Collections.Generic;

namespace SnappyNet
{
	internal sealed class IntEnumerable : IEnumerable<int>
	{
		private readonly int[] _data;
		private readonly int _key;

		internal IntEnumerable(int[] data, int key)
		{
			_data = data;
			_key = key;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<int> GetEnumerator()
		{
			var offset = _data[0] * 2 + 1;
			while (offset > 2)
			{
				offset -= 2;
				if (_data[offset] == _key)
				{
					yield return _data[offset + 1];
				}
			}
		}
	}
}
