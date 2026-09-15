namespace GPUTools.Common.Scripts.Tools
{
	public class FixedList<T>
	{
		public int Size { get; private set; }

		public int Count { get; private set; }

		public T[] Data { get; private set; }

		public T this[int i]
		{
			get
			{
				return Data[i];
			}
			set
			{
				Data[i] = value;
			}
		}

		public FixedList(int size)
		{
			Data = new T[size];
			Size = size;
			Count = 0;
		}

		public void Add(T item)
		{
			Data[Count] = item;
			Count++;
		}

		public void Reset()
		{
			Count = 0;
		}

		public bool Contains(T item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (Data[i].Equals(item))
				{
					return true;
				}
			}
			return false;
		}
	}
}
