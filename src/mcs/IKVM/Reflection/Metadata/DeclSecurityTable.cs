using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class DeclSecurityTable : SortedTable<DeclSecurityTable.Record>
	{
		internal struct Record : IRecord
		{
			internal short Action;

			internal int Parent;

			internal int PermissionSet;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EDeclSecurityTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EDeclSecurityTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EDeclSecurityTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Parent;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EDeclSecurityTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 14;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Action = mr.ReadInt16();
				records[i].Parent = mr.ReadHasDeclSecurity();
				records[i].PermissionSet = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Action);
				mw.WriteHasDeclSecurity(records[i].Parent);
				mw.WriteBlobIndex(records[i].PermissionSet);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteHasDeclSecurity().WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Parent;
				moduleBuilder.FixupPseudoToken(ref token);
				switch (token >> 24)
				{
				case 2:
					token = ((token & 0xFFFFFF) << 2) | 0;
					break;
				case 6:
					token = ((token & 0xFFFFFF) << 2) | 1;
					break;
				case 32:
					token = ((token & 0xFFFFFF) << 2) | 2;
					break;
				default:
					throw new InvalidOperationException();
				}
				records[i].Parent = token;
			}
			Sort();
		}
	}
}
