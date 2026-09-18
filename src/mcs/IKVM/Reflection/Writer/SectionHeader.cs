namespace IKVM.Reflection.Writer
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
	}
}
