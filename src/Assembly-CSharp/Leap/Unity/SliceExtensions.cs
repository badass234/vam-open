using System.Collections.Generic;

namespace Leap.Unity
{
	public static class SliceExtensions
	{
		public static Slice<T> Slice<T>(this IList<T> list, int beginIdx = -1, int endIdx = -1)
		{
			if (beginIdx == -1 && endIdx == -1)
			{
				return new Slice<T>(list, 0, list.Count);
			}
			if (beginIdx == -1 && endIdx != -1)
			{
				return new Slice<T>(list, 0, endIdx);
			}
			if (endIdx == -1 && beginIdx != -1)
			{
				return new Slice<T>(list, beginIdx, list.Count);
			}
			return new Slice<T>(list, beginIdx, endIdx);
		}

		public static Slice<T> FromIndex<T>(this IList<T> list, int fromIdx)
		{
			return list.Slice(fromIdx);
		}
	}
}
