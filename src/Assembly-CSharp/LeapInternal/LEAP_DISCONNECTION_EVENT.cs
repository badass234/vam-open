using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_DISCONNECTION_EVENT
	{
		public uint reserved;
	}
}
