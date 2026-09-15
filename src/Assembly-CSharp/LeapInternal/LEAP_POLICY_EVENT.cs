using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_POLICY_EVENT
	{
		public uint reserved;

		public uint current_policy;
	}
}
