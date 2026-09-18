using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ExportedTypeTable : Table<ExportedTypeTable.Record>
	{
		internal struct Record
		{
			internal int Flags;

			internal int TypeDefId;

			internal int TypeName;

			internal int TypeNamespace;

			internal int Implementation;
		}

		internal const int Index = 39;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].TypeDefId = mr.ReadInt32();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNamespace = mr.ReadStringIndex();
				records[i].Implementation = mr.ReadImplementation();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.Write(records[i].TypeDefId);
				mw.WriteStringIndex(records[i].TypeName);
				mw.WriteStringIndex(records[i].TypeNamespace);
				mw.WriteImplementation(records[i].Implementation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(8).WriteStringIndex().WriteStringIndex()
				.WriteImplementation()
				.Value;
		}

		internal int FindOrAddRecord(Record rec)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Implementation == rec.Implementation && records[i].TypeName == rec.TypeName && records[i].TypeNamespace == rec.TypeNamespace)
				{
					return i + 1;
				}
			}
			return AddRecord(rec);
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].Implementation);
			}
		}
	}
}
