using System;
using System.Collections.Generic;
using Leap.Unity.Query;

namespace Leap.Unity
{
	public struct Slice<T> : IIndexableStruct<T, Slice<T>>
	{
		private IList<T> _list;

		private int _beginIdx;

		private int _endIdx;

		private int _direction;

		public T this[int index]
		{
			get
			{
				if (index < 0 || index > Count - 1)
				{
					throw new IndexOutOfRangeException();
				}
				return _list[_beginIdx + index * _direction];
			}
			set
			{
				if (index < 0 || index > Count - 1)
				{
					throw new IndexOutOfRangeException();
				}
				_list[_beginIdx + index * _direction] = value;
			}
		}

		public int Count => (_endIdx - _beginIdx) * _direction;

		public Slice(IList<T> list, int beginIdx = 0, int endIdx = -1)
		{
			_list = list;
			_beginIdx = beginIdx;
			if (endIdx == -1)
			{
				endIdx = _list.Count;
			}
			_endIdx = endIdx;
			_direction = ((beginIdx <= endIdx) ? 1 : (-1));
		}

		public IndexableStructEnumerator<T, Slice<T>> GetEnumerator()
		{
			return new IndexableStructEnumerator<T, Slice<T>>(this);
		}

		public Query<T> Query()
		{
			T[] array = ArrayPool<T>.Spawn(Count);
			for (int i = 0; i < Count; i++)
			{
				array[i] = this[i];
			}
			return new Query<T>(array, Count);
		}
	}
}
