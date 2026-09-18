using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Metadata
{
	internal sealed class EventPtrTable : Table<int>
	{
		internal const int Index = 19;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadEvent();
			}
		}
	}
}
