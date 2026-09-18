using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class FieldTable : Table<FieldTable.Record>
	{
		internal struct Record
		{
			internal short Flags;

			internal int Name;

			internal int Signature;
		}

		internal const int Index = 4;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteFieldTable(mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteStringIndex().WriteBlobIndex()
				.Value;
		}
	}
}
