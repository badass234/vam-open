using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_TIP
	{
		public LEAP_VECTOR position;

		public float radius;
	}
}
