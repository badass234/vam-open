using System.IO;

namespace IKVM.Reflection.Reader
{
	internal sealed class IMAGE_OPTIONAL_HEADER
	{
		public const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 267;

		public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 523;

		public const ushort IMAGE_SUBSYSTEM_WINDOWS_GUI = 2;

		public const ushort IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;

		public ushort Magic;

		public byte MajorLinkerVersion;

		public byte MinorLinkerVersion;

		public uint SizeOfCode;

		public uint SizeOfInitializedData;

		public uint SizeOfUninitializedData;

		public uint AddressOfEntryPoint;

		public uint BaseOfCode;

		public uint BaseOfData;

		public ulong ImageBase;

		public uint SectionAlignment;

		public uint FileAlignment;

		public ushort MajorOperatingSystemVersion;

		public ushort MinorOperatingSystemVersion;

		public ushort MajorImageVersion;

		public ushort MinorImageVersion;

		public ushort MajorSubsystemVersion;

		public ushort MinorSubsystemVersion;

		public uint Win32VersionValue;

		public uint SizeOfImage;

		public uint SizeOfHeaders;

		public uint CheckSum;

		public ushort Subsystem;

		public ushort DllCharacteristics;

		public ulong SizeOfStackReserve;

		public ulong SizeOfStackCommit;

		public ulong SizeOfHeapReserve;

		public ulong SizeOfHeapCommit;

		public uint LoaderFlags;

		public uint NumberOfRvaAndSizes;

		public IMAGE_DATA_DIRECTORY[] DataDirectory;

		internal void Read(BinaryReader br)
		{
			Magic = br.ReadUInt16();
			if (Magic != 267 && Magic != 523)
			{
				throw new BadImageFormatException();
			}
			MajorLinkerVersion = br.ReadByte();
			MinorLinkerVersion = br.ReadByte();
			SizeOfCode = br.ReadUInt32();
			SizeOfInitializedData = br.ReadUInt32();
			SizeOfUninitializedData = br.ReadUInt32();
			AddressOfEntryPoint = br.ReadUInt32();
			BaseOfCode = br.ReadUInt32();
			if (Magic == 267)
			{
				BaseOfData = br.ReadUInt32();
				ImageBase = br.ReadUInt32();
			}
			else
			{
				ImageBase = br.ReadUInt64();
			}
			SectionAlignment = br.ReadUInt32();
			FileAlignment = br.ReadUInt32();
			MajorOperatingSystemVersion = br.ReadUInt16();
			MinorOperatingSystemVersion = br.ReadUInt16();
			MajorImageVersion = br.ReadUInt16();
			MinorImageVersion = br.ReadUInt16();
			MajorSubsystemVersion = br.ReadUInt16();
			MinorSubsystemVersion = br.ReadUInt16();
			Win32VersionValue = br.ReadUInt32();
			SizeOfImage = br.ReadUInt32();
			SizeOfHeaders = br.ReadUInt32();
			CheckSum = br.ReadUInt32();
			Subsystem = br.ReadUInt16();
			DllCharacteristics = br.ReadUInt16();
			if (Magic == 267)
			{
				SizeOfStackReserve = br.ReadUInt32();
				SizeOfStackCommit = br.ReadUInt32();
				SizeOfHeapReserve = br.ReadUInt32();
				SizeOfHeapCommit = br.ReadUInt32();
			}
			else
			{
				SizeOfStackReserve = br.ReadUInt64();
				SizeOfStackCommit = br.ReadUInt64();
				SizeOfHeapReserve = br.ReadUInt64();
				SizeOfHeapCommit = br.ReadUInt64();
			}
			LoaderFlags = br.ReadUInt32();
			NumberOfRvaAndSizes = br.ReadUInt32();
			DataDirectory = new IMAGE_DATA_DIRECTORY[NumberOfRvaAndSizes];
			for (uint num = 0u; num < NumberOfRvaAndSizes; num++)
			{
				DataDirectory[num] = default(IMAGE_DATA_DIRECTORY);
				DataDirectory[num].Read(br);
			}
		}
	}
}
