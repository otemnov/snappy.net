using System;

namespace SnappyNet
{
	public sealed class Buffer
	{
		private byte[] _data;
		public byte[] Data { get { return _data; } }

		private int _length;
		public int Length
		{
			get
			{
				return _length;
			}
			set
			{
				if (_data == null)
				{
					throw new ApplicationException("Internal buffer not initialized");
				}
				if (value > _data.Length)
				{
					throw new ArgumentException("Internal buffer length (" + _data.Length + ") is less than length argument (" + value + ")", "length");
				}
				_length = value;
			}
		}

		public Buffer()
		{
		}
		
		public Buffer(int capacity)
		{
			_data = new byte[capacity];
		}

		public void EnsureCapacity(int capacity)
		{
			if (_data == null)
			{
				_data = new byte[capacity];
			}
			else if (_data.Length < capacity)
			{
				var nb = new byte[capacity];
				Array.Copy(_data, 0, nb, 0, _data.Length);
				_data = nb;
			}
		}

		public byte[] ToByteArray()
		{
			var res = new byte[_length];
			Array.Copy(_data, 0, res, 0, _length);
			return res;
		}
	}
}