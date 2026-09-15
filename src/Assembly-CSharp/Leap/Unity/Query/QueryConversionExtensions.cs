using System.Collections.Generic;

namespace Leap.Unity.Query
{
	public static class QueryConversionExtensions
	{
		public static Query<T> Query<T>(this ICollection<T> collection)
		{
			return new Query<T>(collection);
		}

		public static Query<T> Query<T>(this IEnumerable<T> enumerable)
		{
			List<T> list = Pool<List<T>>.Spawn();
			try
			{
				list.AddRange(enumerable);
				return new Query<T>(list);
			}
			finally
			{
				list.Clear();
				Pool<List<T>>.Recycle(list);
			}
		}

		public static Query<T> Query<T>(this IEnumerator<T> enumerator)
		{
			List<T> list = Pool<List<T>>.Spawn();
			try
			{
				while (enumerator.MoveNext())
				{
					list.Add(enumerator.Current);
				}
				return new Query<T>(list);
			}
			finally
			{
				list.Clear();
				Pool<List<T>>.Recycle(list);
			}
		}

		public static Query<T> Query<T>(this T[,] array)
		{
			T[] array2 = ArrayPool<T>.Spawn(array.GetLength(0) * array.GetLength(1));
			int num = 0;
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					array2[num++] = array[i, j];
				}
			}
			return new Query<T>(array2, array.GetLength(0) * array.GetLength(1));
		}
	}
}
