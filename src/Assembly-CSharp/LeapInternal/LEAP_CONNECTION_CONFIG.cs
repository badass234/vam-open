using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONNECTION_CONFIG
	{
		public uint size;

		public uint flags;

		public IntPtr server_namespace;
	}
}
