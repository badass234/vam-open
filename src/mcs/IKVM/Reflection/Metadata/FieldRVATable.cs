using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class FieldRVATable : SortedTable<FieldRVATable.Record>
	{
		internal struct Record : IRecord
		{
			internal int RVA;

			internal int Field;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldRVATable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldRVATable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldRVATable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Field;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldRVATable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Field;
			}
		}

		internal const int Index = 29;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].RVA = mr.ReadInt32();
				records[i].Field = mr.ReadField();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].RVA);
				mw.WriteField(records[i].Field);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(4).WriteField().Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder, int sdataRVA, int cilRVA)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].RVA < 0)
				{
					records[i].RVA = (records[i].RVA & 0x7FFFFFFF) + cilRVA;
				}
				else
				{
					records[i].RVA += sdataRVA;
				}
				moduleBuilder.FixupPseudoToken(ref records[i].Field);
			}
			Sort();
		}
	}
}
