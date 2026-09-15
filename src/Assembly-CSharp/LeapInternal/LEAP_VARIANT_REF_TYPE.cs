using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_VARIANT_REF_TYPE
	{
		public eLeapValueType type;

		public string stringValue;
	}
}
