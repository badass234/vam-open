using System;
using System.Collections.Generic;

namespace Leap.Unity
{
	public static class ListAndArrayExtensions
	{
		public static T[] Fill<T>(this T[] array, T value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
			return array;
		}

		public static T[] Fill<T>(this T[] array, Func<T> constructor)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = constructor();
			}
			return array;
		}

		public static T[,] Fill<T>(this T[,] array, T value)
		{
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					array[i, j] = value;
				}
			}
			return array;
		}

		public static List<T> Fill<T>(this List<T> list, T value)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = value;
			}
			return list;
		}

		public static List<T> Fill<T>(this List<T> list, int count, T value)
		{
			list.Clear();
			for (int i = 0; i < count; i++)
			{
				list.Add(value);
			}
			return list;
		}

		public static List<T> FillEach<T>(this List<T> list, Func<T> generator)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = generator();
			}
			return list;
		}

		public static List<T> FillEach<T>(this List<T> list, Func<int, T> generator)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = generator(i);
			}
			return list;
		}

		public static List<T> FillEach<T>(this List<T> list, int count, Func<T> generator)
		{
			list.Clear();
			for (int i = 0; i < count; i++)
			{
				list.Add(generator());
			}
			return list;
		}

		public static List<T> FillEach<T>(this List<T> list, int count, Func<int, T> generator)
		{
			list.Clear();
			for (int i = 0; i < count; i++)
			{
				list.Add(generator(i));
			}
			return list;
		}

		public static List<T> Append<T>(this List<T> list, int count, T value)
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(value);
			}
			return list;
		}

		public static T RemoveLast<T>(this List<T> list)
		{
			T result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}

		public static bool RemoveUnordered<T>(this List<T> list, T element)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Equals(element))
				{
					list[i] = list.RemoveLast();
					return true;
				}
			}
			return false;
		}

		public static void RemoveAtUnordered<T>(this List<T> list, int index)
		{
			if (list.Count - 1 == index)
			{
				list.RemoveLast();
			}
			else
			{
				list[index] = list.RemoveLast();
			}
		}

		public static void InsertUnordered<T>(this List<T> list, int index, T element)
		{
			list.Add(list[index]);
			list[index] = element;
		}

		public static void RemoveAtMany<T>(this List<T> list, List<int> sortedIndexes)
		{
			if (sortedIndexes.Count == 0)
			{
				return;
			}
			if (sortedIndexes.Count == 1)
			{
				list.RemoveAt(sortedIndexes[0]);
				return;
			}
			int num = sortedIndexes[0];
			int num2 = num;
			int num3 = 0;
			while (true)
			{
				if (num2 == sortedIndexes[num3])
				{
					num2++;
					num3++;
					if (num3 == sortedIndexes.Count)
					{
						break;
					}
				}
				else
				{
					list[num++] = list[num2++];
				}
			}
			while (num2 < list.Count)
			{
				list[num++] = list[num2++];
			}
			list.RemoveRange(list.Count - num3, num3);
		}

		public static void InsertMany<T>(this List<T> list, List<int> sortedIndexes, List<T> elements)
		{
			if (sortedIndexes.Count == 0)
			{
				return;
			}
			if (sortedIndexes.Count == 1)
			{
				list.Insert(sortedIndexes[0], elements[0]);
				return;
			}
			int num = list.Count - 1;
			for (int i = 0; i < sortedIndexes.Count; i++)
			{
				list.Add(default(T));
			}
			int num2 = list.Count - 1;
			int num3 = sortedIndexes.Count - 1;
			while (true)
			{
				if (num2 == sortedIndexes[num3])
				{
					list[num2--] = elements[num3--];
					if (num3 == -1)
					{
						break;
					}
				}
				else
				{
					list[num2--] = list[num--];
				}
			}
		}
	}
}
