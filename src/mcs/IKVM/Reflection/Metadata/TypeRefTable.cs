using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class TypeRefTable : Table<TypeRefTable.Record>
	{
		internal struct Record
		{
			internal int ResolutionScope;

			internal int TypeName;

			internal int TypeNamespace;
		}

		internal const int Index = 1;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].ResolutionScope = mr.ReadResolutionScope();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNamespace = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteResolutionScope(records[i].ResolutionScope);
				mw.WriteStringIndex(records[i].TypeName);
				mw.WriteStringIndex(records[i].TypeNamespace);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.WriteResolutionScope().WriteStringIndex().WriteStringIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].ResolutionScope);
			}
		}
	}
}
