using System.IO;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal struct RvaSize
	{
		internal uint VirtualAddress;

		internal uint Size;

		internal void Read(BinaryReader br)
		{
			VirtualAddress = br.ReadUInt32();
			Size = br.ReadUInt32();
		}

		internal void Write(MetadataWriter mw)
		{
			mw.Write(VirtualAddress);
			mw.Write(Size);
		}
	}
}
