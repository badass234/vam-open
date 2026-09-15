using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONFIG_RESPONSE_EVENT
	{
		public uint requestId;

		public LEAP_VARIANT_VALUE_TYPE value;
	}
}
