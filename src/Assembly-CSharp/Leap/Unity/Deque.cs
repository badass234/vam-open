using System;
using UnityEngine;

namespace Leap.Unity
{
	public class Deque<T>
	{
		private T[] _array;

		private uint _front;

		private uint _count;

		private uint _indexMask;

		public int Count => (int)_count;

		public T Front
		{
			get
			{
				checkForEmpty("get front");
				return _array[_front];
			}
			set
			{
				checkForEmpty("set front");
				_array[_front] = value;
			}
		}

		public T Back
		{
			get
			{
				checkForEmpty("get back");
				return _array[getBackIndex()];
			}
			set
			{
				checkForEmpty("set back");
				_array[getBackIndex()] = value;
			}
		}

		public T this[int index]
		{
			get
			{
				checkForValidIndex((uint)index);
				return _array[getIndex((uint)index)];
			}
			set
			{
				checkForValidIndex((uint)index);
				_array[getIndex((uint)index)] = value;
			}
		}

		public Deque(int minCapacity = 8)
		{
			if (minCapacity <= 0)
			{
				throw new ArgumentException("Capacity must be positive and nonzero.");
			}
			int num = Mathf.ClosestPowerOfTwo(minCapacity);
			if (num < minCapacity)
			{
				num *= 2;
			}
			_array = new T[num];
			recalculateIndexMask();
			_front = 0u;
			_count = 0u;
		}

		public void Clear()
		{
			if (_count != 0)
			{
				Array.Clear(_array, 0, _array.Length);
				_front = 0u;
				_count = 0u;
			}
		}

		public void PushBack(T t)
		{
			doubleCapacityIfFull();
			_count++;
			_array[getBackIndex()] = t;
		}

		public void PushFront(T t)
		{
			doubleCapacityIfFull();
			_count++;
			_front = (_front - 1) & _indexMask;
			_array[_front] = t;
		}

		public void PopBack()
		{
			checkForEmpty("pop back");
			_array[getBackIndex()] = default(T);
			_count--;
		}

		public void PopFront()
		{
			checkForEmpty("pop front");
			_array[_front] = default(T);
			_count--;
			_front = (_front + 1) & _indexMask;
		}

		public void PopBack(out T back)
		{
			checkForEmpty("pop back");
			uint backIndex = getBackIndex();
			back = _array[backIndex];
			_array[backIndex] = default(T);
			_count--;
		}

		public void PopFront(out T front)
		{
			checkForEmpty("pop front");
			front = _array[_front];
			_array[_front] = default(T);
			_front = (_front + 1) & _indexMask;
			_count--;
		}

		public string ToDebugString()
		{
			string text = "[";
			uint backIndex = getBackIndex();
			for (uint num = 0u; num < _array.Length; num++)
			{
				bool flag = _count == 0 || ((_count != 1) ? ((_front >= backIndex) ? (num < _front && num > backIndex) : (num < _front || num > backIndex)) : (num != _front));
				string empty = string.Empty;
				empty = ((num != _front) ? " " : "{");
				empty = ((!flag) ? (empty + _array[num].ToString()) : (empty + "."));
				empty = ((num != backIndex) ? (empty + " ") : (empty + "}"));
				text += empty;
			}
			return text + "]";
		}

		private uint getBackIndex()
		{
			return (_front + _count - 1) & _indexMask;
		}

		private uint getIndex(uint index)
		{
			return (_front + index) & _indexMask;
		}

		private void doubleCapacityIfFull()
		{
			if (_count >= _array.Length)
			{
				T[] array = new T[_array.Length * 2];
				uint backIndex = getBackIndex();
				if (_front <= backIndex)
				{
					Array.Copy(_array, _front, array, 0L, _count);
				}
				else
				{
					uint num = (uint)_array.Length - _front;
					Array.Copy(_array, _front, array, 0L, num);
					Array.Copy(_array, 0L, array, num, _count - num);
				}
				_front = 0u;
				_array = array;
				recalculateIndexMask();
			}
		}

		private void recalculateIndexMask()
		{
			_indexMask = (uint)(_array.Length - 1);
		}

		private void checkForValidIndex(uint index)
		{
			if (index >= _count)
			{
				throw new IndexOutOfRangeException("The index " + index + " was out of range for the RingBuffer with size " + _count + ".");
			}
		}

		private void checkForEmpty(string actionName)
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("Cannot " + actionName + " because the RingBuffer is empty.");
			}
		}
	}
}
