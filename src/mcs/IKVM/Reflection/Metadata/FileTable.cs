using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class FileTable : Table<FileTable.Record>
	{
		internal struct Record
		{
			internal int Flags;

			internal int Name;

			internal int HashValue;
		}

		internal const int Index = 38;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].Name = mr.ReadStringIndex();
				records[i].HashValue = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].HashValue);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(4).WriteStringIndex().WriteBlobIndex()
				.Value;
		}
	}
}
