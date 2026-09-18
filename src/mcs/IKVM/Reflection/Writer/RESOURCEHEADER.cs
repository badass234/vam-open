using System.Text;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	internal struct RESOURCEHEADER
	{
		internal int DataSize;

		internal int HeaderSize;

		internal OrdinalOrName TYPE;

		internal OrdinalOrName NAME;

		internal int DataVersion;

		internal ushort MemoryFlags;

		internal ushort LanguageId;

		internal int Version;

		internal int Characteristics;

		internal RESOURCEHEADER(ByteReader br)
		{
			DataSize = br.ReadInt32();
			HeaderSize = br.ReadInt32();
			TYPE = ReadOrdinalOrName(br);
			NAME = ReadOrdinalOrName(br);
			br.Align(4);
			DataVersion = br.ReadInt32();
			MemoryFlags = br.ReadUInt16();
			LanguageId = br.ReadUInt16();
			Version = br.ReadInt32();
			Characteristics = br.ReadInt32();
		}

		private static OrdinalOrName ReadOrdinalOrName(ByteReader br)
		{
			char c = br.ReadChar();
			if (c == '\uffff')
			{
				return new OrdinalOrName(br.ReadUInt16());
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (c != 0)
			{
				stringBuilder.Append(c);
				c = br.ReadChar();
			}
			return new OrdinalOrName(stringBuilder.ToString());
		}
	}
}
