using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class MethodDefTable : Table<MethodDefTable.Record>
	{
		internal struct Record
		{
			internal int RVA;

			internal short ImplFlags;

			internal short Flags;

			internal int Name;

			internal int Signature;

			internal int ParamList;
		}

		internal const int Index = 6;

		private int baseRVA;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].RVA = mr.ReadInt32();
				records[i].ImplFlags = mr.ReadInt16();
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
				records[i].ParamList = mr.ReadParam();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteMethodDefTable(baseRVA, mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(8).WriteStringIndex().WriteBlobIndex()
				.WriteParam()
				.Value;
		}

		internal void Fixup(TextSection code)
		{
			baseRVA = (int)code.MethodBodiesRVA;
		}
	}
}
