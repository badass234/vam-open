using System.Collections.Generic;

namespace Leap.Unity
{
	public struct ReadonlyList<T>
	{
		private readonly List<T> _list;

		public bool isValid => _list != null;

		public int Count => _list.Count;

		public T this[int index] => _list[index];

		public ReadonlyList(List<T> list)
		{
			_list = list;
		}

		public List<T>.Enumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public static implicit operator ReadonlyList<T>(List<T> list)
		{
			return new ReadonlyList<T>(list);
		}

		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}
	}
}
