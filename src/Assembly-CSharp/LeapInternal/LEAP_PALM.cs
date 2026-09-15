using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_PALM
	{
		public LEAP_VECTOR position;

		public LEAP_VECTOR stabilized_position;

		public LEAP_VECTOR velocity;

		public LEAP_VECTOR normal;

		public float width;

		public LEAP_VECTOR direction;

		public LEAP_QUATERNION orientation;
	}
}
