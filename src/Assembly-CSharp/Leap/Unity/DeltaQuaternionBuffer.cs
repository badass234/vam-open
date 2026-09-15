using UnityEngine;

namespace Leap.Unity
{
	public class DeltaQuaternionBuffer : DeltaBuffer<Quaternion, Vector3>
	{
		public DeltaQuaternionBuffer(int bufferSize)
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
				ValueTimePair valueTimePair = _buffer.Get(i);
				ValueTimePair valueTimePair2 = _buffer.Get(i + 1);
				Quaternion value = valueTimePair.value;
				float time = valueTimePair.time;
				Quaternion value2 = valueTimePair2.value;
				float time2 = valueTimePair2.time;
				Vector3 vector = value2.From(value).ToAngleAxisVector();
				float num = time2.From(time);
				zero += vector / num;
			}
			return zero / (base.Count - 1);
		}
	}
}
