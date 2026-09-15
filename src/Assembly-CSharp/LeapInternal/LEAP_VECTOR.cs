using System.Runtime.InteropServices;
using Leap;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_VECTOR
	{
		public float x;

		public float y;

		public float z;

		public LEAP_VECTOR(Vector leap)
		{
			x = leap.x;
			y = leap.y;
			z = leap.z;
		}

		public Vector ToLeapVector()
		{
			return new Vector(x, y, z);
		}
	}
}
