using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct SingleConverter
	{
		[FieldOffset(0)]
		private int i;

		[FieldOffset(0)]
		private float f;

		public static int SingleToInt32Bits(float v)
		{
			SingleConverter singleConverter = new SingleConverter
			{
				f = v
			};
			return singleConverter.i;
		}
	}
}
