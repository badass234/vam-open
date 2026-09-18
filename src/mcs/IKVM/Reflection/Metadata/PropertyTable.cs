using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class PropertyTable : Table<PropertyTable.Record>
	{
		internal struct Record
		{
			internal short Flags;

			internal int Name;

			internal int Type;
		}

		internal const int Index = 23;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Type = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].Type);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteStringIndex().WriteBlobIndex()
				.Value;
		}
	}
}
