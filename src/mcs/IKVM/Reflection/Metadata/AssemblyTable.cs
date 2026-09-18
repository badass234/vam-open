using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class AssemblyTable : Table<AssemblyTable.Record>
	{
		internal struct Record
		{
			internal int HashAlgId;

			internal ushort MajorVersion;

			internal ushort MinorVersion;

			internal ushort BuildNumber;

			internal ushort RevisionNumber;

			internal int Flags;

			internal int PublicKey;

			internal int Name;

			internal int Culture;
		}

		internal const int Index = 32;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].HashAlgId = mr.ReadInt32();
				records[i].MajorVersion = mr.ReadUInt16();
				records[i].MinorVersion = mr.ReadUInt16();
				records[i].BuildNumber = mr.ReadUInt16();
				records[i].RevisionNumber = mr.ReadUInt16();
				records[i].Flags = mr.ReadInt32();
				records[i].PublicKey = mr.ReadBlobIndex();
				records[i].Name = mr.ReadStringIndex();
				records[i].Culture = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].HashAlgId);
				mw.Write(records[i].MajorVersion);
				mw.Write(records[i].MinorVersion);
				mw.Write(records[i].BuildNumber);
				mw.Write(records[i].RevisionNumber);
				mw.Write(records[i].Flags);
				mw.WriteBlobIndex(records[i].PublicKey);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteStringIndex(records[i].Culture);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(16).WriteBlobIndex().WriteStringIndex()
				.WriteStringIndex()
				.Value;
		}
	}
}
