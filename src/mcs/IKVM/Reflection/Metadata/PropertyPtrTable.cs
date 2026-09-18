using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Metadata
{
	internal sealed class PropertyPtrTable : Table<int>
	{
		internal const int Index = 22;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadProperty();
			}
		}
	}
}
