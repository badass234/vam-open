using System.IO;

namespace IKVM.Reflection.Reader
{
	internal sealed class IMAGE_FILE_HEADER
	{
		public const ushort IMAGE_FILE_MACHINE_I386 = 332;

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

		public ushort SizeOfOptionalHeader;

		public ushort Characteristics;

		internal void Read(BinaryReader br)
		{
			Machine = br.ReadUInt16();
			NumberOfSections = br.ReadUInt16();
			TimeDateStamp = br.ReadUInt32();
			PointerToSymbolTable = br.ReadUInt32();
			NumberOfSymbols = br.ReadUInt32();
			SizeOfOptionalHeader = br.ReadUInt16();
			Characteristics = br.ReadUInt16();
		}
	}
}
