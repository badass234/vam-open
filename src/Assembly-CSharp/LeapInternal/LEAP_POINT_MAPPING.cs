using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_POINT_MAPPING
	{
		public long frame_id;

		public long timestamp;

		public uint nPoints;

		public IntPtr points;

		public IntPtr ids;
	}
}
