using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_CONFIG_RESPONSE_EVENT_WITH_REF_TYPE
	{
		public uint requestId;

		public LEAP_VARIANT_REF_TYPE value;
	}
}
