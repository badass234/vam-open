using System.IO;
using System.Text;

namespace IKVM.Reflection.Writer
{
	internal sealed class PEWriter
	{
		private readonly BinaryWriter bw;

		private readonly IMAGE_NT_HEADERS hdr = new IMAGE_NT_HEADERS();

		public IMAGE_NT_HEADERS Headers => hdr;

		public uint HeaderSize => (uint)(152 + hdr.FileHeader.SizeOfOptionalHeader + hdr.FileHeader.NumberOfSections * 40);

		internal bool Is32Bit => (Headers.FileHeader.Characteristics & 0x100) != 0;

		internal uint Thumb
		{
			get
			{
				if (Headers.FileHeader.Machine != 452)
				{
					return 0u;
				}
				return 1u;
			}
		}

		internal PEWriter(Stream stream)
		{
			bw = new BinaryWriter(stream);
			WriteMSDOSHeader();
		}

		private void WriteMSDOSHeader()
		{
			bw.Write(new byte[128]
			{
				77, 90, 144, 0, 3, 0, 0, 0, 4, 0,
				0, 0, 255, 255, 0, 0, 184, 0, 0, 0,
				0, 0, 0, 0, 64, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				128, 0, 0, 0, 14, 31, 186, 14, 0, 180,
				9, 205, 33, 184, 1, 76, 205, 33, 84, 104,
				105, 115, 32, 112, 114, 111, 103, 114, 97, 109,
				32, 99, 97, 110, 110, 111, 116, 32, 98, 101,
				32, 114, 117, 110, 32, 105, 110, 32, 68, 79,
				83, 32, 109, 111, 100, 101, 46, 13, 13, 10,
				36, 0, 0, 0, 0, 0, 0, 0
			});
		}

		internal void WritePEHeaders()
		{
			bw.Write(hdr.Signature);
			bw.Write(hdr.FileHeader.Machine);
			bw.Write(hdr.FileHeader.NumberOfSections);
			bw.Write(hdr.FileHeader.TimeDateStamp);
			bw.Write(hdr.FileHeader.PointerToSymbolTable);
			bw.Write(hdr.FileHeader.NumberOfSymbols);
			bw.Write(hdr.FileHeader.SizeOfOptionalHeader);
			bw.Write(hdr.FileHeader.Characteristics);
			hdr.OptionalHeader.Write(bw);
		}

		internal void WriteSectionHeader(SectionHeader sectionHeader)
		{
			byte[] array = new byte[8];
			Encoding.UTF8.GetBytes(sectionHeader.Name, 0, sectionHeader.Name.Length, array, 0);
			bw.Write(array);
			bw.Write(sectionHeader.VirtualSize);
			bw.Write(sectionHeader.VirtualAddress);
			bw.Write(sectionHeader.SizeOfRawData);
			bw.Write(sectionHeader.PointerToRawData);
			bw.Write(sectionHeader.PointerToRelocations);
			bw.Write(sectionHeader.PointerToLinenumbers);
			bw.Write(sectionHeader.NumberOfRelocations);
			bw.Write(sectionHeader.NumberOfLinenumbers);
			bw.Write(sectionHeader.Characteristics);
		}

		internal uint ToFileAlignment(uint p)
		{
			return (p + (Headers.OptionalHeader.FileAlignment - 1)) & ~(Headers.OptionalHeader.FileAlignment - 1);
		}

		internal uint ToSectionAlignment(uint p)
		{
			return (p + (Headers.OptionalHeader.SectionAlignment - 1)) & ~(Headers.OptionalHeader.SectionAlignment - 1);
		}
	}
}
