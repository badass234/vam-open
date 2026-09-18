using System.IO;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	internal sealed class IMAGE_OPTIONAL_HEADER
	{
		public const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 267;

		public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 523;

		public const ushort IMAGE_SUBSYSTEM_WINDOWS_GUI = 2;

		public const ushort IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;

		public ushort Magic = 267;

		public byte MajorLinkerVersion = 8;

		public byte MinorLinkerVersion;

		public uint SizeOfCode;

		public uint SizeOfInitializedData;

		public uint SizeOfUninitializedData;

		public uint AddressOfEntryPoint;

		public uint BaseOfCode;

		public uint BaseOfData;

		public ulong ImageBase;

		public uint SectionAlignment = 8192u;

		public uint FileAlignment;

		public ushort MajorOperatingSystemVersion = 4;

		public ushort MinorOperatingSystemVersion;

		public ushort MajorImageVersion;

		public ushort MinorImageVersion;

		public ushort MajorSubsystemVersion = 4;

		public ushort MinorSubsystemVersion;

		public uint Win32VersionValue;

		public uint SizeOfImage;

		public uint SizeOfHeaders;

		public uint CheckSum;

		public ushort Subsystem;

		public ushort DllCharacteristics;

		public ulong SizeOfStackReserve;

		public ulong SizeOfStackCommit = 4096uL;

		public ulong SizeOfHeapReserve = 1048576uL;

		public ulong SizeOfHeapCommit = 4096uL;

		public uint LoaderFlags;

		public uint NumberOfRvaAndSizes = 16u;

		public IMAGE_DATA_DIRECTORY[] DataDirectory = new IMAGE_DATA_DIRECTORY[16];

		internal void Write(BinaryWriter bw)
		{
			bw.Write(Magic);
			bw.Write(MajorLinkerVersion);
			bw.Write(MinorLinkerVersion);
			bw.Write(SizeOfCode);
			bw.Write(SizeOfInitializedData);
			bw.Write(SizeOfUninitializedData);
			bw.Write(AddressOfEntryPoint);
			bw.Write(BaseOfCode);
			if (Magic == 267)
			{
				bw.Write(BaseOfData);
				bw.Write((uint)ImageBase);
			}
			else
			{
				bw.Write(ImageBase);
			}
			bw.Write(SectionAlignment);
			bw.Write(FileAlignment);
			bw.Write(MajorOperatingSystemVersion);
			bw.Write(MinorOperatingSystemVersion);
			bw.Write(MajorImageVersion);
			bw.Write(MinorImageVersion);
			bw.Write(MajorSubsystemVersion);
			bw.Write(MinorSubsystemVersion);
			bw.Write(Win32VersionValue);
			bw.Write(SizeOfImage);
			bw.Write(SizeOfHeaders);
			bw.Write(CheckSum);
			bw.Write(Subsystem);
			bw.Write(DllCharacteristics);
			if (Magic == 267)
			{
				bw.Write((uint)SizeOfStackReserve);
				bw.Write((uint)SizeOfStackCommit);
				bw.Write((uint)SizeOfHeapReserve);
				bw.Write((uint)SizeOfHeapCommit);
			}
			else
			{
				bw.Write(SizeOfStackReserve);
				bw.Write(SizeOfStackCommit);
				bw.Write(SizeOfHeapReserve);
				bw.Write(SizeOfHeapCommit);
			}
			bw.Write(LoaderFlags);
			bw.Write(NumberOfRvaAndSizes);
			for (int i = 0; i < DataDirectory.Length; i++)
			{
				bw.Write(DataDirectory[i].VirtualAddress);
				bw.Write(DataDirectory[i].Size);
			}
		}
	}
}
