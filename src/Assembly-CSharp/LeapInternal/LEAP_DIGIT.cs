using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DIGIT
	{
		public int finger_id;

		public LEAP_BONE metacarpal;

		public LEAP_BONE proximal;

		public LEAP_BONE intermediate;

		public LEAP_BONE distal;

		public int is_extended;
	}
}
