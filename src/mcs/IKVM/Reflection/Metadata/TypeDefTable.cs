using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class TypeDefTable : Table<TypeDefTable.Record>
	{
		internal struct Record
		{
			internal int Flags;

			internal int TypeName;

			internal int TypeNamespace;

			internal int Extends;

			internal int FieldList;

			internal int MethodList;
		}

		internal const int Index = 2;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNamespace = mr.ReadStringIndex();
				records[i].Extends = mr.ReadTypeDefOrRef();
				records[i].FieldList = mr.ReadField();
				records[i].MethodList = mr.ReadMethodDef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteTypeDefTable(mw);
		}

		internal int AllocToken()
		{
			return 33554432 + AddVirtualRecord();
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(4).WriteStringIndex().WriteStringIndex()
				.WriteTypeDefOrRef()
				.WriteField()
				.WriteMethodDef()
				.Value;
		}
	}
}
