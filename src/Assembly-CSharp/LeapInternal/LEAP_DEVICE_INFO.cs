using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DEVICE_INFO
	{
		public uint size;

		public eLeapDeviceStatus status;

		public eLeapDeviceCaps caps;

		public eLeapDeviceType type;

		public uint baseline;

		public uint serial_length;

		public IntPtr serial;

		public float h_fov;

		public float v_fov;

		public uint range;
	}
}
