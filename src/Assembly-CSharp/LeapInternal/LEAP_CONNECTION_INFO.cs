using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONNECTION_INFO
	{
		public uint size;

		public eLeapConnectionStatus status;
	}
}
