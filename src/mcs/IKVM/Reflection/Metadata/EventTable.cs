using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class EventTable : Table<EventTable.Record>
	{
		internal struct Record
		{
			internal short EventFlags;

			internal int Name;

			internal int EventType;
		}

		internal const int Index = 20;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].EventFlags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].EventType = mr.ReadTypeDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].EventFlags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteTypeDefOrRef(records[i].EventType);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteStringIndex().WriteTypeDefOrRef()
				.Value;
		}
	}
}
