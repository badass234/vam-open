using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_MATRIX_3x3
	{
		public LEAP_VECTOR m1;

		public LEAP_VECTOR m2;

		public LEAP_VECTOR m3;
	}
}
