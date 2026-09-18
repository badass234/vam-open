namespace IKVM.Reflection.Writer
{
	internal sealed class IMAGE_NT_HEADERS
	{
		public uint Signature = 17744u;

		public IMAGE_FILE_HEADER FileHeader = new IMAGE_FILE_HEADER();

		public IMAGE_OPTIONAL_HEADER OptionalHeader = new IMAGE_OPTIONAL_HEADER();
	}
}
