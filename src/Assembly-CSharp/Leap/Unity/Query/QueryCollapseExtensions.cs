using System;
using System.Collections.Generic;

namespace Leap.Unity.Query
{
	public static class QueryCollapseExtensions
	{
		private static class FoldDelegate<T> where T : IComparable<T>
		{
			public static readonly Func<T, T, T> max = (T a, T b) => (a.CompareTo(b) <= 0) ? b : a;

			public static readonly Func<T, T, T> min = (T a, T b) => (a.CompareTo(b) >= 0) ? b : a;
		}

		public static bool All<T>(this Query<T> query, Func<T, bool> predicate)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					if (!predicate(querySlice[i]))
					{
						return false;
					}
				}
				return true;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static bool AllEqual<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count <= 1)
				{
					return true;
				}
				EqualityComparer<T> @default = EqualityComparer<T>.Default;
				T x = querySlice[0];
				for (int i = 1; i < querySlice.Count; i++)
				{
					if (!@default.Equals(x, querySlice[i]))
					{
						return false;
					}
				}
				return true;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static bool Any<T>(this Query<T> query)
		{
			return query.Count() > 0;
		}

		public static bool Any<T>(this Query<T> query, Func<T, bool> predicate)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					if (predicate(querySlice[i]))
					{
						return true;
					}
				}
				return false;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static float Average(this Query<float> query)
		{
			Query<float>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				float num = 0f;
				for (int i = 0; i < querySlice.Count; i++)
				{
					num += querySlice[i];
				}
				return num / (float)querySlice.Count;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static double Average(this Query<double> query)
		{
			Query<double>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				double num = 0.0;
				for (int i = 0; i < querySlice.Count; i++)
				{
					num += querySlice[i];
				}
				return num / (double)querySlice.Count;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static bool Contains<T>(this Query<T> query, T item)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int i = 0; i < count; i++)
			{
				if (@default.Equals(item, array[i]))
				{
					ArrayPool<T>.Recycle(array);
					return true;
				}
			}
			ArrayPool<T>.Recycle(array);
			return false;
		}

		public static int Count<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				return querySlice.Count;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static int Count<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).Count();
		}

		public static int CountUnique<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			HashSet<T> hashSet = Pool<HashSet<T>>.Spawn();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					hashSet.Add(querySlice[i]);
				}
				return hashSet.Count;
			}
			finally
			{
				querySlice.Dispose();
				hashSet.Clear();
				Pool<HashSet<T>>.Recycle(hashSet);
			}
		}

		public static int CountUnique<T, K>(this Query<T> query, Func<T, K> selector)
		{
			return query.Select(selector).CountUnique();
		}

		public static T ElementAt<T>(this Query<T> query, int index)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (index < 0 || index >= querySlice.Count)
				{
					throw new IndexOutOfRangeException("The index " + index + " was out of range.  Query only has length of " + querySlice.Count);
				}
				return querySlice[index];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T ElementAtOrDefault<T>(this Query<T> query, int index)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (index < 0 || index >= querySlice.Count)
				{
					return default(T);
				}
				return querySlice[index];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T First<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					throw new InvalidOperationException("The source Query was empty.");
				}
				return querySlice[0];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T First<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).First();
		}

		public static T FirstOrDefault<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					return default(T);
				}
				return querySlice[0];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T FirstOrDefault<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).FirstOrDefault();
		}

		public static Maybe<T> FirstOrNone<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					return Maybe.None;
				}
				return Maybe.Some(querySlice[0]);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Maybe<T> FirstOrNone<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).FirstOrNone();
		}

		public static T Fold<T>(this Query<T> query, Func<T, T, T> foldFunc)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					throw new InvalidOperationException("The source Query was empty.");
				}
				T val = querySlice[0];
				for (int i = 1; i < querySlice.Count; i++)
				{
					val = foldFunc(val, querySlice[i]);
				}
				return val;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static int IndexOf<T>(this Query<T> query, T t)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				EqualityComparer<T> @default = EqualityComparer<T>.Default;
				for (int i = 0; i < querySlice.Count; i++)
				{
					if (@default.Equals(t, querySlice[i]))
					{
						return i;
					}
				}
				return -1;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static int IndexOf<T>(this Query<T> query, Func<T, bool> predicate)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					if (predicate(querySlice[i]))
					{
						return i;
					}
				}
				return -1;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T Last<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					throw new InvalidOperationException("The source Query was empty.");
				}
				return querySlice[querySlice.Count - 1];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T Last<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).Last();
		}

		public static T LastOrDefault<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					return default(T);
				}
				return querySlice[querySlice.Count - 1];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T LastOrDefault<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).LastOrDefault();
		}

		public static Maybe<T> LastOrNone<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					return Maybe.None;
				}
				return Maybe.Some(querySlice[querySlice.Count - 1]);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Maybe<T> LastOrNone<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).LastOrNone();
		}

		public static T Max<T>(this Query<T> query) where T : IComparable<T>
		{
			return query.Fold(FoldDelegate<T>.max);
		}

		public static K Max<T, K>(this Query<T> query, Func<T, K> selector) where K : IComparable<K>
		{
			return query.Select(selector).Max();
		}

		public static T Min<T>(this Query<T> query) where T : IComparable<T>
		{
			return query.Fold(FoldDelegate<T>.min);
		}

		public static K Min<T, K>(this Query<T> query, Func<T, K> selector) where K : IComparable<K>
		{
			return query.Select(selector).Min();
		}

		public static T Single<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count != 1)
				{
					throw new InvalidOperationException("The Query had a count of " + querySlice.Count + " instead of a count of 1.");
				}
				return querySlice[0];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T Single<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).Single();
		}

		public static T SingleOrDefault<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count != 1)
				{
					return default(T);
				}
				return querySlice[0];
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T SingleOrDefault<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).SingleOrDefault();
		}

		public static Maybe<T> SingleOrNone<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count != 1)
				{
					return Maybe.None;
				}
				return Maybe.Some(querySlice[0]);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Maybe<T> SingleOrNone<T>(this Query<T> query, Func<T, bool> predicate)
		{
			return query.Where(predicate).SingleOrNone();
		}

		public static int Sum(this Query<int> query)
		{
			return query.Fold((int a, int b) => a + b);
		}

		public static float Sum(this Query<float> query)
		{
			return query.Fold((float a, float b) => a + b);
		}

		public static double Sum(this Query<double> query)
		{
			return query.Fold((double a, double b) => a + b);
		}

		public static T UniformOrDefault<T>(this Query<T> query)
		{
			return query.UniformOrNone().valueOrDefault;
		}

		public static Maybe<T> UniformOrNone<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				if (querySlice.Count == 0)
				{
					return Maybe.None;
				}
				Query<T>.QuerySlice querySlice2 = querySlice;
				T val = querySlice2[0];
				EqualityComparer<T> @default = EqualityComparer<T>.Default;
				for (int i = 1; i < querySlice.Count; i++)
				{
					if (!@default.Equals(val, querySlice[i]))
					{
						return Maybe.None;
					}
				}
				return Maybe.Some(val);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static T[] ToArray<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				T[] array = new T[querySlice.Count];
				Array.Copy(querySlice.BackingArray, array, querySlice.Count);
				return array;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static void FillArray<T>(this Query<T> query, T[] array, int offset = 0)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				Array.Copy(querySlice.BackingArray, 0, array, offset, querySlice.Count);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static List<T> ToList<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				List<T> list = new List<T>(querySlice.Count);
				for (int i = 0; i < querySlice.Count; i++)
				{
					list.Add(querySlice[i]);
				}
				return list;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static void FillList<T>(this Query<T> query, List<T> list)
		{
			list.Clear();
			query.AppendList(list);
		}

		public static void AppendList<T>(this Query<T> query, List<T> list)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					list.Add(querySlice[i]);
				}
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static HashSet<T> ToHashSet<T>(this Query<T> query)
		{
			HashSet<T> hashSet = new HashSet<T>();
			query.AppendHashSet(hashSet);
			return hashSet;
		}

		public static void FillHashSet<T>(this Query<T> query, HashSet<T> set)
		{
			set.Clear();
			query.AppendHashSet(set);
		}

		public static void AppendHashSet<T>(this Query<T> query, HashSet<T> set)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				for (int i = 0; i < querySlice.Count; i++)
				{
					set.Add(querySlice[i]);
				}
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Dictionary<K, V> ToDictionary<T, K, V>(this Query<T> query, Func<T, K> keySelector, Func<T, V> valueSelector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				Dictionary<K, V> dictionary = new Dictionary<K, V>();
				for (int i = 0; i < querySlice.Count; i++)
				{
					dictionary[keySelector(querySlice[i])] = valueSelector(querySlice[i]);
				}
				return dictionary;
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Dictionary<T, V> ToDictionary<T, V>(this Query<T> query, Func<T, V> valueSelector)
		{
			return query.ToDictionary((T t) => t, valueSelector);
		}
	}
}
