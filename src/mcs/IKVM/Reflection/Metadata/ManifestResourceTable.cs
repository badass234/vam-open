using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ManifestResourceTable : Table<ManifestResourceTable.Record>
	{
		internal struct Record
		{
			internal int Offset;

			internal int Flags;

			internal int Name;

			internal int Implementation;
		}

		internal const int Index = 40;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Offset = mr.ReadInt32();
				records[i].Flags = mr.ReadInt32();
				records[i].Name = mr.ReadStringIndex();
				records[i].Implementation = mr.ReadImplementation();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Offset);
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteImplementation(records[i].Implementation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(8).WriteStringIndex().WriteImplementation()
				.Value;
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
