using System.IO;

namespace IKVM.Reflection.Reader
{
	internal sealed class PEReader
	{
		private MSDOS_HEADER msdos = new MSDOS_HEADER();

		private IMAGE_NT_HEADERS headers = new IMAGE_NT_HEADERS();

		private SectionHeader[] sections;

		private bool mapped;

		internal IMAGE_FILE_HEADER FileHeader => headers.FileHeader;

		internal IMAGE_OPTIONAL_HEADER OptionalHeader => headers.OptionalHeader;

		internal void Read(BinaryReader br, bool mapped)
		{
			this.mapped = mapped;
			msdos.signature = br.ReadUInt16();
			br.BaseStream.Seek(58L, SeekOrigin.Current);
			msdos.peSignatureOffset = br.ReadUInt32();
			if (msdos.signature != 23117)
			{
				throw new BadImageFormatException();
			}
			br.BaseStream.Seek(msdos.peSignatureOffset, SeekOrigin.Begin);
			headers.Read(br);
			sections = new SectionHeader[headers.FileHeader.NumberOfSections];
			for (int i = 0; i < sections.Length; i++)
			{
				sections[i] = new SectionHeader();
				sections[i].Read(br);
			}
		}

		internal uint GetComDescriptorVirtualAddress()
		{
			return headers.OptionalHeader.DataDirectory[14].VirtualAddress;
		}

		internal void GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			rva = (int)headers.OptionalHeader.DataDirectory[index].VirtualAddress;
			length = (int)headers.OptionalHeader.DataDirectory[index].Size;
		}

		internal long RvaToFileOffset(uint rva)
		{
			if (mapped)
			{
				return rva;
			}
			for (int i = 0; i < sections.Length; i++)
			{
				if (rva >= sections[i].VirtualAddress && rva < sections[i].VirtualAddress + sections[i].VirtualSize)
				{
					return sections[i].PointerToRawData + rva - sections[i].VirtualAddress;
				}
			}
			throw new BadImageFormatException();
		}

		internal bool GetSectionInfo(int rva, out string name, out int characteristics, out int virtualAddress, out int virtualSize, out int pointerToRawData, out int sizeOfRawData)
		{
			for (int i = 0; i < sections.Length; i++)
			{
				if (rva >= sections[i].VirtualAddress && rva < sections[i].VirtualAddress + sections[i].VirtualSize)
				{
					name = sections[i].Name;
					characteristics = (int)sections[i].Characteristics;
					virtualAddress = (int)sections[i].VirtualAddress;
					virtualSize = (int)sections[i].VirtualSize;
					pointerToRawData = (int)sections[i].PointerToRawData;
					sizeOfRawData = (int)sections[i].SizeOfRawData;
					return true;
				}
			}
			name = null;
			characteristics = 0;
			virtualAddress = 0;
			virtualSize = 0;
			pointerToRawData = 0;
			sizeOfRawData = 0;
			return false;
		}
	}
}
