using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DROPPED_FRAME_EVENT
	{
		public long frame_id;

		public eLeapDroppedFrameType reason;
	}
}
