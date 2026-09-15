namespace Leap.Unity
{
	public static class ReadonlySliceExtensions
	{
		public static ReadonlySlice<T> ReadonlySlice<T>(this ReadonlyList<T> list, int beginIdx = -1, int endIdx = -1)
		{
			if (beginIdx == -1 && endIdx == -1)
			{
				return new ReadonlySlice<T>(list, 0, list.Count);
			}
			if (beginIdx == -1 && endIdx != -1)
			{
				return new ReadonlySlice<T>(list, 0, endIdx);
			}
			if (endIdx == -1 && beginIdx != -1)
			{
				return new ReadonlySlice<T>(list, beginIdx, list.Count);
			}
			return new ReadonlySlice<T>(list, beginIdx, endIdx);
		}

		public static ReadonlySlice<T> FromIndex<T>(this ReadonlyList<T> list, int fromIdx)
		{
			return list.ReadonlySlice(fromIdx);
		}
	}
}
