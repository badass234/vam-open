using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ClassLayoutTable : SortedTable<ClassLayoutTable.Record>
	{
		internal struct Record : IRecord
		{
			internal short PackingSize;

			internal int ClassSize;

			internal int Parent;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EClassLayoutTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EClassLayoutTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EClassLayoutTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Parent;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EClassLayoutTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 15;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].PackingSize = mr.ReadInt16();
				records[i].ClassSize = mr.ReadInt32();
				records[i].Parent = mr.ReadTypeDef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			Sort();
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].PackingSize);
				mw.Write(records[i].ClassSize);
				mw.WriteTypeDef(records[i].Parent);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(6).WriteTypeDef().Value;
		}
	}
}
