using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class StandAloneSigTable : Table<int>
	{
		internal const int Index = 17;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteBlobIndex(records[i]);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteBlobIndex().Value;
		}

		internal int FindOrAddRecord(int blob)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i] == blob)
				{
					return i + 1;
				}
			}
			return AddRecord(blob);
		}
	}
}
