using UnityEngine;

namespace Leap.Unity
{
	public abstract class DeltaBuffer<SampleType, DerivativeType> : IIndexable<SampleType>
	{
		protected struct ValueTimePair
		{
			public SampleType value;

			public float time;
		}

		protected RingBuffer<ValueTimePair> _buffer;

		public int Count => _buffer.Count;

		public bool IsFull => _buffer.IsFull;

		public bool IsEmpty => _buffer.IsEmpty;

		public int Capacity => _buffer.Capacity;

		public SampleType this[int idx] => _buffer[idx].value;

		public DeltaBuffer(int bufferSize)
		{
			_buffer = new RingBuffer<ValueTimePair>(bufferSize);
		}

		public void Clear()
		{
			_buffer.Clear();
		}

		public void Add(SampleType sample, float sampleTime)
		{
			if (!IsEmpty && sampleTime == GetLatestTime())
			{
				SetLatest(sample, sampleTime);
				return;
			}
			_buffer.Add(new ValueTimePair
			{
				value = sample,
				time = sampleTime
			});
		}

		public SampleType Get(int idx)
		{
			return _buffer.Get(idx).value;
		}

		public SampleType GetLatest()
		{
			return Get(Count - 1);
		}

		public void Set(int idx, SampleType sample, float sampleTime)
		{
			_buffer.Set(idx, new ValueTimePair
			{
				value = sample,
				time = sampleTime
			});
		}

		public void SetLatest(SampleType sample, float sampleTime)
		{
			if (Count == 0)
			{
				Set(0, sample, sampleTime);
			}
			else
			{
				Set(Count - 1, sample, sampleTime);
			}
		}

		public float GetTime(int idx)
		{
			return _buffer.Get(idx).time;
		}

		public float GetLatestTime()
		{
			return _buffer.Get(Count - 1).time;
		}

		public abstract DerivativeType Delta();

		public IndexableEnumerator<SampleType> GetEnumerator()
		{
			return new IndexableEnumerator<SampleType>(this);
		}
	}
	public class DeltaBuffer : DeltaBuffer<Vector3, Vector3>
	{
		public DeltaBuffer(int bufferSize)
			: base(bufferSize)
		{
		}

		public override Vector3 Delta()
		{
			if (base.Count <= 1)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < base.Count - 1; i++)
			{
				zero += (Get(i + 1) - Get(i)) / (GetTime(i + 1) - GetTime(i));
			}
			return zero / (base.Count - 1);
		}
	}
}
