using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONNECTION_MESSAGE
	{
		public uint size;

		public eLeapEventType type;

		public IntPtr eventStructPtr;
	}
}
