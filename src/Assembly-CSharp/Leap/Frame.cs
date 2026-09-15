using System;
using System.Collections.Generic;

namespace Leap
{
	[Serializable]
	public class Frame : IEquatable<Frame>
	{
		[ThreadStatic]
		private static Queue<Hand> _handPool;

		public long Id;

		public long Timestamp;

		public float CurrentFramesPerSecond;

		public List<Hand> Hands;

		[Obsolete]
		public int SerializeLength
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		[Obsolete]
		public byte[] Serialize
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Frame()
		{
			Hands = new List<Hand>();
		}

		public Frame(long id, long timestamp, float fps, List<Hand> hands)
		{
			Id = id;
			Timestamp = timestamp;
			CurrentFramesPerSecond = fps;
			Hands = hands;
		}

		[Obsolete]
		public void Deserialize(byte[] arg)
		{
			throw new NotImplementedException();
		}

		public Hand Hand(int id)
		{
			int count = Hands.Count;
			while (count-- != 0)
			{
				if (Hands[count].Id == id)
				{
					return Hands[count];
				}
			}
			return null;
		}

		public bool Equals(Frame other)
		{
			return Id == other.Id && Timestamp == other.Timestamp;
		}

		public override string ToString()
		{
			return "Frame id: " + Id + " timestamp: " + Timestamp;
		}

		internal void ResizeHandList(int count)
		{
			if (_handPool == null)
			{
				_handPool = new Queue<Hand>();
			}
			while (Hands.Count < count)
			{
				Hand item = ((_handPool.Count <= 0) ? new Hand() : _handPool.Dequeue());
				Hands.Add(item);
			}
			while (Hands.Count > count)
			{
				Hand item2 = Hands[Hands.Count - 1];
				Hands.RemoveAt(Hands.Count - 1);
				_handPool.Enqueue(item2);
			}
		}
	}
}
