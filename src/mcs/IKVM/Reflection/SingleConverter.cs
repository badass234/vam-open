using System.Runtime.InteropServices;

namespace IKVM.Reflection
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct SingleConverter
	{
		[FieldOffset(0)]
		private int i;

		[FieldOffset(0)]
		private float f;

		internal static int SingleToInt32Bits(float v)
		{
			SingleConverter singleConverter = new SingleConverter
			{
				f = v
			};
			return singleConverter.i;
		}

		internal static float Int32BitsToSingle(int v)
		{
			SingleConverter singleConverter = new SingleConverter
			{
				i = v
			};
			return singleConverter.f;
		}
	}
}
