using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class MemberRefTable : Table<MemberRefTable.Record>
	{
		internal struct Record
		{
			internal int Class;

			internal int Name;

			internal int Signature;
		}

		internal const int Index = 10;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Class = mr.ReadMemberRefParent();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteMemberRefParent(records[i].Class);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].Signature);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteMemberRefParent().WriteStringIndex().WriteBlobIndex()
				.Value;
		}

		internal int FindOrAddRecord(Record record)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Class == record.Class && records[i].Name == record.Name && records[i].Signature == record.Signature)
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
				moduleBuilder.FixupPseudoToken(ref records[i].Class);
			}
		}
	}
}
