using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_IMAGE_EVENT
	{
		public LEAP_FRAME_HEADER info;

		public LEAP_IMAGE leftImage;

		public LEAP_IMAGE rightImage;

		public IntPtr calib;
	}
}
