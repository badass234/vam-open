using System;

namespace Leap.Unity
{
	public class RingBuffer<T> : IIndexable<T>
	{
		private T[] arr;

		private int firstIdx;

		private int lastIdx = -1;

		public int Count
		{
			get
			{
				if (lastIdx == -1)
				{
					return 0;
				}
				int num = (lastIdx + 1) % arr.Length;
				if (num <= firstIdx)
				{
					num += arr.Length;
				}
				return num - firstIdx;
			}
		}

		public int Capacity => arr.Length;

		public bool IsFull => lastIdx != -1 && (lastIdx + 1 + arr.Length) % arr.Length == firstIdx;

		public bool IsEmpty => lastIdx == -1;

		public T this[int idx]
		{
			get
			{
				return Get(idx);
			}
			set
			{
				Set(idx, value);
			}
		}

		public RingBuffer(int bufferSize)
		{
			bufferSize = Math.Max(1, bufferSize);
			arr = new T[bufferSize];
		}

		public void Clear()
		{
			firstIdx = 0;
			lastIdx = -1;
		}

		public void Add(T t)
		{
			if (IsFull)
			{
				firstIdx++;
				firstIdx %= arr.Length;
			}
			lastIdx++;
			lastIdx %= arr.Length;
			arr[lastIdx] = t;
		}

		public T Get(int idx)
		{
			if (idx < 0 || idx > Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			return arr[(firstIdx + idx) % arr.Length];
		}

		public T GetLatest()
		{
			if (Count == 0)
			{
				throw new IndexOutOfRangeException("Can't get latest value in an empty RingBuffer.");
			}
			return Get(Count - 1);
		}

		public T GetOldest()
		{
			if (Count == 0)
			{
				throw new IndexOutOfRangeException("Can't get oldest value in an empty RingBuffer.");
			}
			return Get(0);
		}

		public void Set(int idx, T t)
		{
			if (idx < 0 || idx > Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			int num = (firstIdx + idx) % arr.Length;
			arr[num] = t;
		}

		public void SetLatest(T t)
		{
			if (Count == 0)
			{
				throw new IndexOutOfRangeException("Can't set latest value in an empty RingBuffer.");
			}
			Set(Count - 1, t);
		}
	}
}
