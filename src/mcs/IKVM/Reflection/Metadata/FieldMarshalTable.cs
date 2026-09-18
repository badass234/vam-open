using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class FieldMarshalTable : SortedTable<FieldMarshalTable.Record>
	{
		internal struct Record : IRecord
		{
			internal int Parent;

			internal int NativeType;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldMarshalTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldMarshalTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldMarshalTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return EncodeHasFieldMarshal(Parent);
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EFieldMarshalTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 13;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadHasFieldMarshal();
				records[i].NativeType = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteHasFieldMarshal(records[i].Parent);
				mw.WriteBlobIndex(records[i].NativeType);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteHasFieldMarshal().WriteBlobIndex().Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				records[i].Parent = moduleBuilder.ResolvePseudoToken(records[i].Parent);
			}
			Sort();
		}

		internal static int EncodeHasFieldMarshal(int token)
		{
			switch (token >> 24)
			{
			case 4:
				return ((token & 0xFFFFFF) << 1) | 0;
			case 8:
				return ((token & 0xFFFFFF) << 1) | 1;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
