using System.IO;

namespace IKVM.Reflection.Reader
{
	internal class SectionHeader
	{
		public const uint IMAGE_SCN_CNT_CODE = 32u;

		public const uint IMAGE_SCN_CNT_INITIALIZED_DATA = 64u;

		public const uint IMAGE_SCN_MEM_DISCARDABLE = 33554432u;

		public const uint IMAGE_SCN_MEM_EXECUTE = 536870912u;

		public const uint IMAGE_SCN_MEM_READ = 1073741824u;

		public const uint IMAGE_SCN_MEM_WRITE = 2147483648u;

		public string Name;

		public uint VirtualSize;

		public uint VirtualAddress;

		public uint SizeOfRawData;

		public uint PointerToRawData;

		public uint PointerToRelocations;

		public uint PointerToLinenumbers;

		public ushort NumberOfRelocations;

		public ushort NumberOfLinenumbers;

		public uint Characteristics;

		internal void Read(BinaryReader br)
		{
			char[] array = new char[8];
			int num = 8;
			for (int i = 0; i < 8; i++)
			{
				if ((array[i] = (char)br.ReadByte()) == '\0' && num == 8)
				{
					num = i;
				}
			}
			Name = new string(array, 0, num);
			VirtualSize = br.ReadUInt32();
			VirtualAddress = br.ReadUInt32();
			SizeOfRawData = br.ReadUInt32();
			PointerToRawData = br.ReadUInt32();
			PointerToRelocations = br.ReadUInt32();
			PointerToLinenumbers = br.ReadUInt32();
			NumberOfRelocations = br.ReadUInt16();
			NumberOfLinenumbers = br.ReadUInt16();
			Characteristics = br.ReadUInt32();
		}
	}
}
