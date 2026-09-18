using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class PropertyMapTable : SortedTable<PropertyMapTable.Record>
	{
		internal struct Record : IRecord
		{
			internal int Parent;

			internal int PropertyList;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EPropertyMapTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EPropertyMapTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EPropertyMapTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Parent;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EPropertyMapTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 21;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadTypeDef();
				records[i].PropertyList = mr.ReadProperty();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Parent);
				mw.WriteProperty(records[i].PropertyList);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteTypeDef().WriteProperty().Value;
		}
	}
}
