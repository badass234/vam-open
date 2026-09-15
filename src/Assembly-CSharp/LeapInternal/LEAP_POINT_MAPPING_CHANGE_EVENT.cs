using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_POINT_MAPPING_CHANGE_EVENT
	{
		public long frame_id;

		public long timestamp;

		public uint nPoints;
	}
}
