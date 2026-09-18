namespace IKVM.Reflection.Writer
{
	internal sealed class IMAGE_FILE_HEADER
	{
		public const ushort IMAGE_FILE_MACHINE_I386 = 332;

		public const ushort IMAGE_FILE_MACHINE_ARM = 452;

		public const ushort IMAGE_FILE_MACHINE_IA64 = 512;

		public const ushort IMAGE_FILE_MACHINE_AMD64 = 34404;

		public const ushort IMAGE_FILE_32BIT_MACHINE = 256;

		public const ushort IMAGE_FILE_EXECUTABLE_IMAGE = 2;

		public const ushort IMAGE_FILE_LARGE_ADDRESS_AWARE = 32;

		public const ushort IMAGE_FILE_DLL = 8192;

		public ushort Machine;

		public ushort NumberOfSections;

		public uint TimeDateStamp;

		public uint PointerToSymbolTable;

		public uint NumberOfSymbols;

		public ushort SizeOfOptionalHeader = 224;

		public ushort Characteristics = 2;
	}
}
