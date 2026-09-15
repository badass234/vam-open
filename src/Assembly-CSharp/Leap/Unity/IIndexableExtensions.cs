using Leap.Unity.Query;

namespace Leap.Unity
{
	public static class IIndexableExtensions
	{
		public static IndexableEnumerator<T> GetEnumerator<T>(this IIndexable<T> indexable)
		{
			return new IndexableEnumerator<T>(indexable);
		}

		public static Query<T> Query<T>(this IIndexable<T> indexable)
		{
			T[] array = ArrayPool<T>.Spawn(indexable.Count);
			for (int i = 0; i < indexable.Count; i++)
			{
				array[i] = indexable[i];
			}
			return new Query<T>(array, indexable.Count);
		}
	}
}
