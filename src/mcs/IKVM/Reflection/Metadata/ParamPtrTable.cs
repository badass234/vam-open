using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ParamPtrTable : Table<int>
	{
		internal const int Index = 7;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadParam();
			}
		}
	}
}
