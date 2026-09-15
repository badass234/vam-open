namespace Leap.Unity
{
	public class DelayBuffer<T>
	{
		private RingBuffer<T> _buffer;

		public RingBuffer<T> Buffer => _buffer;

		public int Count => _buffer.Count;

		public bool IsFull => _buffer.IsFull;

		public bool IsEmpty => _buffer.IsEmpty;

		public int Capacity => _buffer.Capacity;

		public DelayBuffer(int bufferSize)
		{
			_buffer = new RingBuffer<T>(bufferSize);
		}

		public void Clear()
		{
			_buffer.Clear();
		}

		public bool Add(T t, out T delayedT)
		{
			bool result;
			if (_buffer.IsFull)
			{
				result = true;
				delayedT = _buffer.GetOldest();
			}
			else
			{
				result = false;
				delayedT = default(T);
			}
			_buffer.Add(t);
			return result;
		}
	}
}
