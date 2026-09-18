namespace IKVM.Reflection.Reader
{
	internal sealed class MSDOS_HEADER
	{
		internal const ushort MAGIC_MZ = 23117;

		internal ushort signature;

		internal uint peSignatureOffset;
	}
}
