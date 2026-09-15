using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_IMAGE_PROPERTIES
	{
		public eLeapImageType type;

		public eLeapImageFormat format;

		public uint bpp;

		public uint width;

		public uint height;

		public float x_scale;

		public float y_scale;

		public float x_offset;

		public float y_offset;
	}
}
