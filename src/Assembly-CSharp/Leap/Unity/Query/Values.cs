using System;
using System.Collections.Generic;

namespace Leap.Unity.Query
{
	public static class Values
	{
		public static Query<T> Single<T>(T value)
		{
			T[] array = ArrayPool<T>.Spawn(1);
			array[0] = value;
			return new Query<T>(array, 1);
		}

		public static Query<T> Repeat<T>(T value, int times)
		{
			T[] array = ArrayPool<T>.Spawn(times);
			for (int i = 0; i < times; i++)
			{
				array[i] = value;
			}
			return new Query<T>(array, times);
		}

		public static Query<T> Empty<T>()
		{
			T[] array = ArrayPool<T>.Spawn(0);
			return new Query<T>(array, 0);
		}

		public static Query<int> Range(int from, int to, int step = 1, bool endIsExclusive = true)
		{
			if (step <= 0)
			{
				throw new ArgumentException("Step must be positive and non-zero.");
			}
			List<int> list = Pool<List<int>>.Spawn();
			try
			{
				int i = from;
				int num = Utils.Sign(to - from);
				if (num != 0)
				{
					for (; Utils.Sign(to - i) == num; i += step * num)
					{
						list.Add(i);
					}
				}
				if (!endIsExclusive && i == to)
				{
					list.Add(to);
				}
				return new Query<int>(list);
			}
			finally
			{
				list.Clear();
				Pool<List<int>>.Recycle(list);
			}
		}
	}
}
