using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_BONE
	{
		public LEAP_VECTOR prev_joint;

		public LEAP_VECTOR next_joint;

		public float width;

		public LEAP_QUATERNION rotation;
	}
}
