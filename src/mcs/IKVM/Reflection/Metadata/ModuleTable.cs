using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ModuleTable : Table<ModuleTable.Record>
	{
		internal struct Record
		{
			internal short Generation;

			internal int Name;

			internal int Mvid;

			internal int EncId;

			internal int EncBaseId;
		}

		internal const int Index = 0;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Generation = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Mvid = mr.ReadGuidIndex();
				records[i].EncId = mr.ReadGuidIndex();
				records[i].EncBaseId = mr.ReadGuidIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Generation);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteGuidIndex(records[i].Mvid);
				mw.WriteGuidIndex(records[i].EncId);
				mw.WriteGuidIndex(records[i].EncBaseId);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteStringIndex().WriteGuidIndex()
				.WriteGuidIndex()
				.WriteGuidIndex()
				.Value;
		}

		internal void Add(short generation, int name, int mvid, int encid, int encbaseid)
		{
			AddRecord(new Record
			{
				Generation = generation,
				Name = name,
				Mvid = mvid,
				EncId = encid,
				EncBaseId = encbaseid
			});
		}
	}
}
