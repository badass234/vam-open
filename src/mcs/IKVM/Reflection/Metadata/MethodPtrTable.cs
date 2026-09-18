using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Metadata
{
	internal sealed class MethodPtrTable : Table<int>
	{
		internal const int Index = 5;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadMethodDef();
			}
		}
	}
}
