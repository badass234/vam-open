using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class CustomAttributeTable : SortedTable<CustomAttributeTable.Record>
	{
		internal struct Record : IRecord
		{
			internal int Parent;

			internal int Type;

			internal int Value;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002ECustomAttributeTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002ECustomAttributeTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002ECustomAttributeTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return EncodeHasCustomAttribute(Parent);
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002ECustomAttributeTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 12;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadHasCustomAttribute();
				records[i].Type = mr.ReadCustomAttributeType();
				records[i].Value = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteHasCustomAttribute(records[i].Parent);
				mw.WriteCustomAttributeType(records[i].Type);
				mw.WriteBlobIndex(records[i].Value);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteHasCustomAttribute().WriteCustomAttributeType().WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			int[] indexFixup = moduleBuilder.GenericParam.GetIndexFixup();
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].Type);
				moduleBuilder.FixupPseudoToken(ref records[i].Parent);
				if (records[i].Parent >> 24 == 42)
				{
					records[i].Parent = (42 << 24) + indexFixup[(records[i].Parent & 0xFFFFFF) - 1] + 1;
				}
			}
			Sort();
		}

		internal static int EncodeHasCustomAttribute(int token)
		{
			switch (token >> 24)
			{
			case 6:
				return ((token & 0xFFFFFF) << 5) | 0;
			case 4:
				return ((token & 0xFFFFFF) << 5) | 1;
			case 1:
				return ((token & 0xFFFFFF) << 5) | 2;
			case 2:
				return ((token & 0xFFFFFF) << 5) | 3;
			case 8:
				return ((token & 0xFFFFFF) << 5) | 4;
			case 9:
				return ((token & 0xFFFFFF) << 5) | 5;
			case 10:
				return ((token & 0xFFFFFF) << 5) | 6;
			case 0:
				return ((token & 0xFFFFFF) << 5) | 7;
			case 23:
				return ((token & 0xFFFFFF) << 5) | 9;
			case 20:
				return ((token & 0xFFFFFF) << 5) | 0xA;
			case 17:
				return ((token & 0xFFFFFF) << 5) | 0xB;
			case 26:
				return ((token & 0xFFFFFF) << 5) | 0xC;
			case 27:
				return ((token & 0xFFFFFF) << 5) | 0xD;
			case 32:
				return ((token & 0xFFFFFF) << 5) | 0xE;
			case 35:
				return ((token & 0xFFFFFF) << 5) | 0xF;
			case 38:
				return ((token & 0xFFFFFF) << 5) | 0x10;
			case 39:
				return ((token & 0xFFFFFF) << 5) | 0x11;
			case 40:
				return ((token & 0xFFFFFF) << 5) | 0x12;
			case 42:
				return ((token & 0xFFFFFF) << 5) | 0x13;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
