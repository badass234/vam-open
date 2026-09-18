using System;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class InterfaceImplTable : SortedTable<InterfaceImplTable.Record>
	{
		internal struct Record : IRecord
		{
			internal int Class;

			internal int Interface;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EInterfaceImplTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EInterfaceImplTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EInterfaceImplTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Class;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EInterfaceImplTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Class;
			}
		}

		internal const int Index = 9;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Class = mr.ReadTypeDef();
				records[i].Interface = mr.ReadTypeDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Class);
				mw.WriteEncodedTypeDefOrRef(records[i].Interface);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteTypeDef().WriteTypeDefOrRef().Value;
		}

		internal void Fixup()
		{
			for (int i = 0; i < rowCount; i++)
			{
				int num = records[i].Interface;
				switch (num >> 24)
				{
				case 2:
					num = ((num & 0xFFFFFF) << 2) | 0;
					break;
				case 1:
					num = ((num & 0xFFFFFF) << 2) | 1;
					break;
				case 27:
					num = ((num & 0xFFFFFF) << 2) | 2;
					break;
				default:
					throw new InvalidOperationException();
				case 0:
					break;
				}
				records[i].Interface = num;
			}
			Sort();
		}
	}
}
