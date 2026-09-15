using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_IMAGE
	{
		public LEAP_IMAGE_PROPERTIES properties;

		public ulong matrix_version;

		public IntPtr distortionMatrix;

		public IntPtr data;

		public uint offset;
	}
}
