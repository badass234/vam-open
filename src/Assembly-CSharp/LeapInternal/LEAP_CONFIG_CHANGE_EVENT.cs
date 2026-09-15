using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONFIG_CHANGE_EVENT
	{
		public uint requestId;

		public bool status;
	}
}
