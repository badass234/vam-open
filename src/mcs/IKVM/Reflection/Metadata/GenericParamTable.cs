using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class GenericParamTable : SortedTable<GenericParamTable.Record>, IComparer<GenericParamTable.Record>
	{
		internal struct Record : IRecord
		{
			internal short Number;

			internal short Flags;

			internal int Owner;

			internal int Name;

			internal int unsortedIndex;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EGenericParamTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EGenericParamTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EGenericParamTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Owner;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EGenericParamTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Owner;
			}
		}

		internal const int Index = 42;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Number = mr.ReadInt16();
				records[i].Flags = mr.ReadInt16();
				records[i].Owner = mr.ReadTypeOrMethodDef();
				records[i].Name = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Number);
				mw.Write(records[i].Flags);
				mw.WriteTypeOrMethodDef(records[i].Owner);
				mw.WriteStringIndex(records[i].Name);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(4).WriteTypeOrMethodDef().WriteStringIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Owner;
				moduleBuilder.FixupPseudoToken(ref token);
				switch (token >> 24)
				{
				case 2:
					records[i].Owner = ((token & 0xFFFFFF) << 1) | 0;
					break;
				case 6:
					records[i].Owner = ((token & 0xFFFFFF) << 1) | 1;
					break;
				default:
					throw new InvalidOperationException();
				}
				records[i].unsortedIndex = i;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			if (x.Owner == y.Owner)
			{
				if (x.Number != y.Number)
				{
					if (x.Number <= y.Number)
					{
						return -1;
					}
					return 1;
				}
				return 0;
			}
			if (x.Owner <= y.Owner)
			{
				return -1;
			}
			return 1;
		}

		internal void PatchAttribute(int token, GenericParameterAttributes genericParameterAttributes)
		{
			records[(token & 0xFFFFFF) - 1].Flags = (short)genericParameterAttributes;
		}

		internal int[] GetIndexFixup()
		{
			int[] array = new int[rowCount];
			for (int i = 0; i < rowCount; i++)
			{
				array[records[i].unsortedIndex] = i;
			}
			return array;
		}

		internal int FindFirstByOwner(int token)
		{
			Enumerator enumerator = Filter(token).GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			return -1;
		}
	}
}
