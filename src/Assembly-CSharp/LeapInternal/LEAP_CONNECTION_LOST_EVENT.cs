using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONNECTION_LOST_EVENT
	{
		public uint flags;
	}
}
