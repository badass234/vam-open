using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_FRAME_HEADER
	{
		public IntPtr reserved;

		public long frame_id;

		public long timestamp;
	}
}
