using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ModuleRefTable : Table<int>
	{
		internal const int Index = 26;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteStringIndex(records[i]);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteStringIndex().Value;
		}

		internal int FindOrAddRecord(int str)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i] == str)
				{
					return i + 1;
				}
			}
			return AddRecord(str);
		}
	}
}
