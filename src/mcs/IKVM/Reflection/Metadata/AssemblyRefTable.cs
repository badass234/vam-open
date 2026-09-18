using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class AssemblyRefTable : Table<AssemblyRefTable.Record>
	{
		internal struct Record
		{
			internal ushort MajorVersion;

			internal ushort MinorVersion;

			internal ushort BuildNumber;

			internal ushort RevisionNumber;

			internal int Flags;

			internal int PublicKeyOrToken;

			internal int Name;

			internal int Culture;

			internal int HashValue;
		}

		internal const int Index = 35;

		internal int FindOrAddRecord(Record rec)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Name == rec.Name && records[i].MajorVersion == rec.MajorVersion && records[i].MinorVersion == rec.MinorVersion && records[i].BuildNumber == rec.BuildNumber && records[i].RevisionNumber == rec.RevisionNumber && records[i].Flags == rec.Flags && records[i].PublicKeyOrToken == rec.PublicKeyOrToken && records[i].Culture == rec.Culture)
				{
					return i + 1;
				}
			}
			return AddRecord(rec);
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].MajorVersion = mr.ReadUInt16();
				records[i].MinorVersion = mr.ReadUInt16();
				records[i].BuildNumber = mr.ReadUInt16();
				records[i].RevisionNumber = mr.ReadUInt16();
				records[i].Flags = mr.ReadInt32();
				records[i].PublicKeyOrToken = mr.ReadBlobIndex();
				records[i].Name = mr.ReadStringIndex();
				records[i].Culture = mr.ReadStringIndex();
				records[i].HashValue = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].MajorVersion);
				mw.Write(records[i].MinorVersion);
				mw.Write(records[i].BuildNumber);
				mw.Write(records[i].RevisionNumber);
				mw.Write(records[i].Flags);
				mw.WriteBlobIndex(records[i].PublicKeyOrToken);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteStringIndex(records[i].Culture);
				mw.WriteBlobIndex(records[i].HashValue);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(12).WriteBlobIndex().WriteStringIndex()
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}
	}
}
