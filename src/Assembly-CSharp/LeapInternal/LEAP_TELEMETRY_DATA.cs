using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_TELEMETRY_DATA
	{
		public uint threadId;

		public ulong startTime;

		public ulong endTime;

		public uint zoneDepth;

		public string fileName;

		public uint lineNumber;

		public string zoneName;
	}
}
