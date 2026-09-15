using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DEVICE_EVENT
	{
		public uint flags;

		public LEAP_DEVICE_REF device;

		public eLeapDeviceStatus status;
	}
}
