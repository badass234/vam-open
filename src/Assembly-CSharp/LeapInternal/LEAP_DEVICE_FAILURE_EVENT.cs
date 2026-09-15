using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DEVICE_FAILURE_EVENT
	{
		public eLeapDeviceStatus status;

		public IntPtr hDevice;
	}
}
