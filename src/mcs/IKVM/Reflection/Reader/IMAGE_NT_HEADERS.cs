using System.IO;

namespace IKVM.Reflection.Reader
{
	internal sealed class IMAGE_NT_HEADERS
	{
		public const uint MAGIC_SIGNATURE = 17744u;

		public uint Signature;

		public IMAGE_FILE_HEADER FileHeader = new IMAGE_FILE_HEADER();

		public IMAGE_OPTIONAL_HEADER OptionalHeader = new IMAGE_OPTIONAL_HEADER();

		internal void Read(BinaryReader br)
		{
			Signature = br.ReadUInt32();
			if (Signature != 17744)
			{
				throw new BadImageFormatException();
			}
			FileHeader.Read(br);
			long position = br.BaseStream.Position;
			OptionalHeader.Read(br);
			if (br.BaseStream.Position > position + FileHeader.SizeOfOptionalHeader)
			{
				throw new BadImageFormatException();
			}
			br.BaseStream.Seek(position + FileHeader.SizeOfOptionalHeader, SeekOrigin.Begin);
		}
	}
}
