using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_LOG_EVENT
	{
		public eLeapLogSeverity severity;

		public long timestamp;

		public IntPtr message;
	}
}
