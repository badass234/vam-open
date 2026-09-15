using System;
using UnityEngine;

namespace Leap.Unity
{
	public class MinHeap<T> where T : IMinHeapNode, IComparable<T>
	{
		private T[] _array = new T[4];

		private int _count;

		public int Count => _count;

		public void Clear()
		{
			Array.Clear(_array, 0, _count);
			_count = 0;
		}

		public void Insert(T element)
		{
			if (_array.Length == _count)
			{
				T[] array = new T[_array.Length * 2];
				Array.Copy(_array, array, _array.Length);
				_array = array;
			}
			element.heapIndex = _count;
			_count++;
			bubbleUp(element);
		}

		public void Remove(T element)
		{
			removeAt(element.heapIndex);
		}

		public T PeekMin()
		{
			if (_count == 0)
			{
				throw new Exception("Cannot peek when there are zero elements!");
			}
			return _array[0];
		}

		public T RemoveMin()
		{
			if (_count == 0)
			{
				throw new Exception("Cannot Remove Min when there are zero elements!");
			}
			return removeAt(0);
		}

		private T removeAt(int index)
		{
			T result = _array[index];
			_count--;
			if (_count == 0)
			{
				return result;
			}
			T val = _array[_count];
			val.heapIndex = index;
			int parentIndex = getParentIndex(index);
			if (isValidIndex(parentIndex) && _array[parentIndex].CompareTo(val) > 0)
			{
				bubbleUp(val);
			}
			else
			{
				bubbleDown(val);
			}
			return result;
		}

		private void bubbleUp(T element)
		{
			while (element.heapIndex != 0)
			{
				int parentIndex = getParentIndex(element.heapIndex);
				T val = _array[parentIndex];
				if (val.CompareTo(element) <= 0)
				{
					break;
				}
				val.heapIndex = element.heapIndex;
				_array[element.heapIndex] = val;
				element.heapIndex = parentIndex;
			}
			_array[element.heapIndex] = element;
		}

		public bool Validate()
		{
			return validateHeapInternal("Validation ");
		}

		private void bubbleDown(T element)
		{
			int num = element.heapIndex;
			while (true)
			{
				int childLeftIndex = getChildLeftIndex(num);
				int childRightIndex = getChildRightIndex(num);
				T val = element;
				int num2 = num;
				if (!isValidIndex(childLeftIndex))
				{
					break;
				}
				T val2 = _array[childLeftIndex];
				if (val2.CompareTo(val) < 0)
				{
					val = val2;
					num2 = childLeftIndex;
				}
				if (isValidIndex(childRightIndex))
				{
					T val3 = _array[childRightIndex];
					if (val3.CompareTo(val) < 0)
					{
						val = val3;
						num2 = childRightIndex;
					}
				}
				if (num2 == num)
				{
					break;
				}
				val.heapIndex = num;
				_array[num] = val;
				num = num2;
			}
			element.heapIndex = num;
			_array[num] = element;
		}

		private bool validateHeapInternal(string operation)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_array[i].heapIndex != i)
				{
					Debug.LogError("Element " + i + " had an index of " + _array[i].heapIndex + " instead, after " + operation);
					return false;
				}
				if (i != 0)
				{
					T val = _array[getParentIndex(i)];
					if (val.CompareTo(_array[i]) > 0)
					{
						Debug.LogError("Element " + i + " had an incorrect order after " + operation);
						return false;
					}
				}
			}
			return true;
		}

		private static int getChildLeftIndex(int index)
		{
			return index * 2 + 1;
		}

		private static int getChildRightIndex(int index)
		{
			return index * 2 + 2;
		}

		private static int getParentIndex(int index)
		{
			return (index - 1) / 2;
		}

		private bool isValidIndex(int index)
		{
			return index < _count && index >= 0;
		}
	}
}
