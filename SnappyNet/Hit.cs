namespace SnappyNet
{
	internal sealed class Hit
	{
		public int Length { get; set; }
		public int Offset { get; set; }

		internal Hit(int offset, int length)
		{
			Offset = offset;
			Length = length;
		}
	}
}