using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct LEAP_VARIANT_VALUE_TYPE
	{
		[FieldOffset(0)]
		public eLeapValueType type;

		[FieldOffset(4)]
		public int boolValue;

		[FieldOffset(4)]
		public int intValue;

		[FieldOffset(4)]
		public float floatValue;
	}
}
