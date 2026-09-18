using System.IO;

namespace IKVM.Reflection.Reader
{
	internal struct IMAGE_DATA_DIRECTORY
	{
		public uint VirtualAddress;

		public uint Size;

		internal void Read(BinaryReader br)
		{
			VirtualAddress = br.ReadUInt32();
			Size = br.ReadUInt32();
		}
	}
}
