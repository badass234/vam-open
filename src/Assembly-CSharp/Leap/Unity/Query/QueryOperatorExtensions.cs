using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Query
{
	public static class QueryOperatorExtensions
	{
		public struct PrevPair<T>
		{
			public T value;

			public T prev;

			public bool hasPrev;
		}

		public struct IndexedValue<T>
		{
			public int index;

			public T value;
		}

		private class FunctorComparer<T, K> : IComparer<T> where K : IComparable<K>
		{
			[ThreadStatic]
			private static FunctorComparer<T, K> _single;

			private Func<T, K> _functor;

			private int _sign;

			private FunctorComparer()
			{
			}

			public static FunctorComparer<T, K> Ascending(Func<T, K> functor)
			{
				return single(functor, 1);
			}

			public static FunctorComparer<T, K> Descending(Func<T, K> functor)
			{
				return single(functor, -1);
			}

			private static FunctorComparer<T, K> single(Func<T, K> functor, int sign)
			{
				if (_single == null)
				{
					_single = new FunctorComparer<T, K>();
				}
				_single._functor = functor;
				_single._sign = sign;
				return _single;
			}

			public void Clear()
			{
				_functor = null;
			}

			public int Compare(T x, T y)
			{
				return _sign * _functor(x).CompareTo(_functor(y));
			}
		}

		public static Query<T> Concat<T>(this Query<T> query, ICollection<T> collection)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				T[] array = ArrayPool<T>.Spawn(querySlice.Count + collection.Count);
				Array.Copy(querySlice.BackingArray, array, querySlice.Count);
				collection.CopyTo(array, querySlice.Count);
				return new Query<T>(array, querySlice.Count + collection.Count);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<T> Concat<T>(this Query<T> query, Query<T> other)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				Query<T>.QuerySlice querySlice2 = other.Deconstruct();
				try
				{
					T[] array = ArrayPool<T>.Spawn(querySlice.Count + querySlice2.Count);
					Array.Copy(querySlice.BackingArray, array, querySlice.Count);
					Array.Copy(querySlice2.BackingArray, 0, array, querySlice.Count, querySlice2.Count);
					return new Query<T>(array, querySlice.Count + querySlice2.Count);
				}
				finally
				{
					((IDisposable)querySlice2/*cast due to .constrained prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<T> Distinct<T>(this Query<T> query)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			HashSet<T> hashSet = Pool<HashSet<T>>.Spawn();
			for (int i = 0; i < count; i++)
			{
				hashSet.Add(array[i]);
			}
			Array.Clear(array, 0, array.Length);
			count = 0;
			foreach (T item in hashSet)
			{
				array[count++] = item;
			}
			hashSet.Clear();
			Pool<HashSet<T>>.Recycle(hashSet);
			return new Query<T>(array, count);
		}

		public static Query<T> OfType<T>(this Query<T> query, Type type)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				T[] array = ArrayPool<T>.Spawn(querySlice.Count);
				int count = 0;
				for (int i = 0; i < querySlice.Count; i++)
				{
					if (querySlice[i] != null && type.IsAssignableFrom(querySlice[i].GetType()))
					{
						array[count++] = querySlice[i];
					}
				}
				return new Query<T>(array, count);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<T> OrderBy<T, K>(this Query<T> query, Func<T, K> selector) where K : IComparable<K>
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			FunctorComparer<T, K> functorComparer = FunctorComparer<T, K>.Ascending(selector);
			Array.Sort(array, 0, count, functorComparer);
			functorComparer.Clear();
			return new Query<T>(array, count);
		}

		public static Query<T> OrderByDescending<T, K>(this Query<T> query, Func<T, K> selector) where K : IComparable<K>
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			FunctorComparer<T, K> functorComparer = FunctorComparer<T, K>.Descending(selector);
			Array.Sort(array, 0, count, functorComparer);
			functorComparer.Clear();
			return new Query<T>(array, count);
		}

		public static Query<T> Repeat<T>(this Query<T> query, int times)
		{
			if (times < 0)
			{
				throw new ArgumentException("The repetition count must be non-negative.");
			}
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				T[] array = ArrayPool<T>.Spawn(querySlice.Count * times);
				for (int i = 0; i < times; i++)
				{
					Array.Copy(querySlice.BackingArray, 0, array, i * querySlice.Count, querySlice.Count);
				}
				return new Query<T>(array, querySlice.Count * times);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<T> Reverse<T>(this Query<T> query)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			array.Reverse(0, count);
			return new Query<T>(array, count);
		}

		public static Query<K> Select<T, K>(this Query<T> query, Func<T, K> selector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				K[] array = ArrayPool<K>.Spawn(querySlice.Count);
				for (int i = 0; i < querySlice.Count; i++)
				{
					array[i] = selector(querySlice[i]);
				}
				return new Query<K>(array, querySlice.Count);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<K> SelectMany<T, K>(this Query<T> query, Func<T, ICollection<K>> selector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				int num = 0;
				for (int i = 0; i < querySlice.Count; i++)
				{
					num += selector(querySlice[i]).Count;
				}
				K[] array = ArrayPool<K>.Spawn(num);
				int num2 = 0;
				for (int j = 0; j < querySlice.Count; j++)
				{
					ICollection<K> collection = selector(querySlice[j]);
					collection.CopyTo(array, num2);
					num2 += collection.Count;
				}
				return new Query<K>(array, num);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<K> SelectMany<T, K>(this Query<T> query, Func<T, Query<K>> selector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				Query<K>.QuerySlice[] array = ArrayPool<Query<K>.QuerySlice>.Spawn(querySlice.Count);
				int num = 0;
				for (int i = 0; i < querySlice.Count; i++)
				{
					array[i] = selector(querySlice[i]).Deconstruct();
					num += array[i].Count;
				}
				K[] array2 = ArrayPool<K>.Spawn(num);
				int num2 = 0;
				for (int j = 0; j < querySlice.Count; j++)
				{
					Array.Copy(array[j].BackingArray, 0, array2, num2, array[j].Count);
					num2 += array[j].Count;
					array[j].Dispose();
				}
				ArrayPool<Query<K>.QuerySlice>.Recycle(array);
				return new Query<K>(array2, num);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<T> Skip<T>(this Query<T> query, int toSkip)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			int num = Mathf.Max(count - toSkip, 0);
			toSkip = count - num;
			Array.Copy(array, toSkip, array, 0, num);
			Array.Clear(array, num, array.Length - num);
			return new Query<T>(array, num);
		}

		public static Query<T> SkipWhile<T>(this Query<T> query, Func<T, bool> predicate)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			int i;
			for (i = 0; i < count && predicate(array[i]); i++)
			{
			}
			int num = count - i;
			Array.Copy(array, i, array, 0, num);
			Array.Clear(array, num, array.Length - num);
			return new Query<T>(array, num);
		}

		public static Query<T> Sort<T>(this Query<T> query) where T : IComparable<T>
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			Array.Sort(array, 0, count);
			return new Query<T>(array, count);
		}

		public static Query<T> SortDescending<T>(this Query<T> query) where T : IComparable<T>
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			Array.Sort(array, 0, count);
			array.Reverse(0, count);
			return new Query<T>(array, count);
		}

		public static Query<T> Take<T>(this Query<T> query, int toTake)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			count = Mathf.Min(count, toTake);
			Array.Clear(array, count, array.Length - count);
			return new Query<T>(array, count);
		}

		public static Query<T> TakeWhile<T>(this Query<T> query, Func<T, bool> predicate)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			int i;
			for (i = 0; i < count && predicate(array[i]); i++)
			{
			}
			Array.Clear(array, i, array.Length - i);
			return new Query<T>(array, i);
		}

		public static Query<T> Where<T>(this Query<T> query, Func<T, bool> predicate)
		{
			T[] array;
			int count;
			query.Deconstruct(out array, out count);
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (predicate(array[i]))
				{
					array[num++] = array[i];
				}
			}
			Array.Clear(array, num, array.Length - num);
			return new Query<T>(array, num);
		}

		public static Query<T> ValidUnityObjs<T>(this Query<T> query) where T : UnityEngine.Object
		{
			return query.Where((T t) => t != null);
		}

		public static Query<IndexedValue<T>> WithIndices<T>(this Query<T> query)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				IndexedValue<T>[] array = ArrayPool<IndexedValue<T>>.Spawn(querySlice.Count);
				for (int i = 0; i < querySlice.Count; i++)
				{
					array[i] = new IndexedValue<T>
					{
						index = i,
						value = querySlice[i]
					};
				}
				return new Query<IndexedValue<T>>(array, querySlice.Count);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<PrevPair<T>> WithPrevious<T>(this Query<T> query, int offset = 1, bool includeStart = false)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				int num = ((!includeStart) ? Mathf.Max(0, querySlice.Count - offset) : querySlice.Count);
				PrevPair<T>[] array = ArrayPool<PrevPair<T>>.Spawn(num);
				int num2 = 0;
				if (includeStart)
				{
					for (int i = 0; i < Mathf.Min(querySlice.Count, offset); i++)
					{
						array[num2++] = new PrevPair<T>
						{
							value = querySlice[i],
							prev = default(T),
							hasPrev = false
						};
					}
				}
				for (int j = offset; j < querySlice.Count; j++)
				{
					array[num2++] = new PrevPair<T>
					{
						value = querySlice[j],
						prev = querySlice[j - offset],
						hasPrev = true
					};
				}
				return new Query<PrevPair<T>>(array, num);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<V> Zip<T, K, V>(this Query<T> query, ICollection<K> collection, Func<T, K, V> selector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				int num = Mathf.Min(querySlice.Count, collection.Count);
				V[] array = ArrayPool<V>.Spawn(num);
				K[] array2 = ArrayPool<K>.Spawn(collection.Count);
				collection.CopyTo(array2, 0);
				for (int i = 0; i < num; i++)
				{
					array[i] = selector(querySlice[i], array2[i]);
				}
				ArrayPool<K>.Recycle(array2);
				return new Query<V>(array, num);
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}

		public static Query<V> Zip<T, K, V>(this Query<T> query, Query<K> otherQuery, Func<T, K, V> selector)
		{
			Query<T>.QuerySlice querySlice = query.Deconstruct();
			try
			{
				Query<K>.QuerySlice querySlice2 = otherQuery.Deconstruct();
				try
				{
					int num = Mathf.Min(querySlice.Count, querySlice2.Count);
					V[] array = ArrayPool<V>.Spawn(num);
					for (int i = 0; i < num; i++)
					{
						array[i] = selector(querySlice[i], querySlice2[i]);
					}
					return new Query<V>(array, num);
				}
				finally
				{
					((IDisposable)querySlice2/*cast due to .constrained prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)querySlice/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}
}
