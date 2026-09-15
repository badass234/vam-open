using System;
using Leap.Unity.Query;

namespace Leap.Unity
{
	public struct ReadonlySlice<T> : IIndexableStruct<T, ReadonlySlice<T>>
	{
		private ReadonlyList<T> _list;

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
		}

		public int Count => (_endIdx - _beginIdx) * _direction;

		public ReadonlySlice(ReadonlyList<T> list, int beginIdx, int endIdx)
		{
			_list = list;
			_beginIdx = beginIdx;
			_endIdx = endIdx;
			_direction = ((beginIdx <= endIdx) ? 1 : (-1));
		}

		public IndexableStructEnumerator<T, ReadonlySlice<T>> GetEnumerator()
		{
			return new IndexableStructEnumerator<T, ReadonlySlice<T>>(this);
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
