using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_HEAD_POSE_EVENT
	{
		public long timestamp;

		public LEAP_VECTOR head_position;

		public LEAP_QUATERNION head_orientation;
	}
}
