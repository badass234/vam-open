using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ParamTable : Table<ParamTable.Record>
	{
		internal struct Record
		{
			internal short Flags;

			internal short Sequence;

			internal int Name;
		}

		internal const int Index = 8;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Sequence = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteParamTable(mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(4).WriteStringIndex().Value;
		}
	}
}
