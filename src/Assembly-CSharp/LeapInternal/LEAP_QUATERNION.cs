using System.Runtime.InteropServices;
using Leap;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_QUATERNION
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public LEAP_QUATERNION(LeapQuaternion q)
		{
			x = q.x;
			y = q.y;
			z = q.z;
			w = q.w;
		}

		public LeapQuaternion ToLeapQuaternion()
		{
			return new LeapQuaternion(x, y, z, w);
		}
	}
}
