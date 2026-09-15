using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_TRACKING_EVENT
	{
		public LEAP_FRAME_HEADER info;

		public long tracking_id;

		public uint nHands;

		public IntPtr pHands;

		public float framerate;
	}
}
