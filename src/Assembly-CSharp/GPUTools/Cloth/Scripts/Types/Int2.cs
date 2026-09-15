using System;

namespace GPUTools.Cloth.Scripts.Types
{
	[Serializable]
	public struct Int2
	{
		public int X;

		public int Y;

		public Int2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static int SizeOf()
		{
			return 8;
		}
	}
}
