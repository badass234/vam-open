using System.Collections.Generic;
using Leap.Unity.Query;

namespace Leap.Unity
{
	public struct ReadonlyHashSet<T>
	{
		private readonly HashSet<T> _set;

		public int Count => _set.Count;

		public ReadonlyHashSet(HashSet<T> set)
		{
			_set = set;
		}

		public HashSet<T>.Enumerator GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		public bool Contains(T obj)
		{
			return _set.Contains(obj);
		}

		public Query<T> Query()
		{
			return _set.Query();
		}

		public static implicit operator ReadonlyHashSet<T>(HashSet<T> set)
		{
			return new ReadonlyHashSet<T>(set);
		}

		public static implicit operator ReadonlyHashSet<T>(SerializableHashSet<T> set)
		{
			return (HashSet<T>)set;
		}
	}
}
