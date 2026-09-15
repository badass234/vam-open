using System;
using UnityEngine;

namespace Leap.Unity
{
	public class ProduceConsumeBuffer<T>
	{
		private T[] _buffer;

		private uint _bufferMask;

		private uint _head;

		private uint _tail;

		public int Capacity => _buffer.Length;

		public int Count
		{
			get
			{
				int num = (int)_tail;
				int head = (int)_head;
				if (num < head)
				{
					num += Capacity;
				}
				return num - head;
			}
		}

		public ProduceConsumeBuffer(int minCapacity)
		{
			if (minCapacity <= 0)
			{
				throw new ArgumentOutOfRangeException("The capacity of the ProduceConsumeBuffer must be positive and non-zero.");
			}
			int num = Mathf.ClosestPowerOfTwo(minCapacity);
			int num2 = ((num == minCapacity) ? minCapacity : ((num >= minCapacity) ? num : (num * 2)));
			_buffer = new T[num2];
			_bufferMask = (uint)(num2 - 1);
			_head = 0u;
			_tail = 0u;
		}

		public bool TryEnqueue(ref T t)
		{
			uint num = (_tail + 1) & _bufferMask;
			if (num == _head)
			{
				return false;
			}
			_buffer[_tail] = t;
			_tail = num;
			return true;
		}

		public bool TryEnqueue(T t)
		{
			return TryEnqueue(ref t);
		}

		public bool TryPeek(out T t)
		{
			if (Count == 0)
			{
				t = default(T);
				return false;
			}
			t = _buffer[_head];
			return true;
		}

		public bool TryDequeue(out T t)
		{
			if (_tail == _head)
			{
				t = default(T);
				return false;
			}
			t = _buffer[_head];
			_head = (_head + 1) & _bufferMask;
			return true;
		}

		public bool TryDequeue()
		{
			if (_tail == _head)
			{
				return false;
			}
			_head = (_head + 1) & _bufferMask;
			return true;
		}

		public bool TryDequeueAll(out T mostRecent)
		{
			if (!TryDequeue(out mostRecent))
			{
				return false;
			}
			T t;
			while (TryDequeue(out t))
			{
				mostRecent = t;
			}
			return true;
		}
	}
}
