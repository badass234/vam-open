namespace LeapInternal
{
	public class CircularObjectBuffer<T> where T : new()
	{
		private T[] array;

		private T emptyT;

		private int current;

		private object locker = new object();

		public int Count { get; private set; }

		public int Capacity { get; private set; }

		public bool IsEmpty { get; private set; }

		public CircularObjectBuffer(int capacity)
		{
			Capacity = capacity;
			array = new T[Capacity];
			emptyT = new T();
			current = 0;
			Count = 0;
			IsEmpty = true;
		}

		public virtual void Put(ref T item)
		{
			lock (locker)
			{
				if (!IsEmpty)
				{
					current++;
					if (current >= Capacity)
					{
						current = 0;
					}
				}
				if (Count < Capacity)
				{
					Count++;
				}
				lock (array)
				{
					array[current] = item;
				}
				IsEmpty = false;
			}
		}

		public void Get(out T t, int index = 0)
		{
			lock (locker)
			{
				if (IsEmpty || index > Count - 1 || index < 0)
				{
					t = emptyT;
					return;
				}
				int num = current - index;
				if (num < 0)
				{
					num += Capacity;
				}
				t = array[num];
			}
		}

		public void Resize(int newCapacity)
		{
			lock (locker)
			{
				if (newCapacity > Capacity)
				{
					T[] array = new T[newCapacity];
					int num = 0;
					for (int num2 = Count - 1; num2 >= 0; num2--)
					{
						T t;
						Get(out t, num2);
						array[num++] = t;
					}
					this.array = array;
					Capacity = newCapacity;
				}
			}
		}
	}
}
