using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_HAND
	{
		public uint id;

		public uint flags;

		public eLeapHandType type;

		public float confidence;

		public ulong visible_time;

		public float pinch_distance;

		public float grab_angle;

		public float pinch_strength;

		public float grab_strength;

		public LEAP_PALM palm;

		public LEAP_DIGIT thumb;

		public LEAP_DIGIT index;

		public LEAP_DIGIT middle;

		public LEAP_DIGIT ring;

		public LEAP_DIGIT pinky;

		public LEAP_BONE arm;
	}
}
