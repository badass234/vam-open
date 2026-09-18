using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class MethodSpecTable : Table<MethodSpecTable.Record>
	{
		internal struct Record
		{
			internal int Method;

			internal int Instantiation;
		}

		internal const int Index = 43;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Method = mr.ReadMethodDefOrRef();
				records[i].Instantiation = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteMethodDefOrRef(records[i].Method);
				mw.WriteBlobIndex(records[i].Instantiation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteMethodDefOrRef().WriteBlobIndex().Value;
		}

		internal int FindOrAddRecord(Record record)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Method == record.Method && records[i].Instantiation == record.Instantiation)
				{
					return i + 1;
				}
			}
			return AddRecord(record);
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].Method);
			}
		}
	}
}
