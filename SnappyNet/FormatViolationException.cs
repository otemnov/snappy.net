using System;
using System.Runtime.Serialization;

namespace SnappyNet
{
	[Serializable]
	public sealed class FormatViolationException : Exception
	{
		public int Offset { get; private set; }

		public FormatViolationException(String message)
			: base(message)
		{
		}

		public FormatViolationException(String message, int offset) : base(message)
		{
			Offset = offset;
		}

		public FormatViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Offset = info.GetInt32("Offset");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Offset", Offset);
		}
	}
}
