using UnityEngine;

namespace Leap.Unity
{
	public class TransformHistory
	{
		public struct TransformData
		{
			public long time;

			public Vector3 position;

			public Quaternion rotation;

			public static TransformData Lerp(TransformData from, TransformData to, long time)
			{
				if (from.time == to.time)
				{
					return from;
				}
				float t = (float)((double)(time - from.time) / (double)(to.time - from.time));
				TransformData result = default(TransformData);
				result.time = time;
				result.position = Vector3.Lerp(from.position, to.position, t);
				result.rotation = Quaternion.Slerp(from.rotation, to.rotation, t);
				return result;
			}

			public static TransformData GetTransformAtTime(RingBuffer<TransformData> history, long desiredTime)
			{
				for (int num = history.Count - 1; num > 0; num--)
				{
					if (history.Get(num).time >= desiredTime && history.Get(num - 1).time < desiredTime)
					{
						return Lerp(history.Get(num - 1), history.Get(num), desiredTime);
					}
				}
				if (history.Count > 0)
				{
					return history.GetLatest();
				}
				TransformData result = default(TransformData);
				result.time = desiredTime;
				result.position = Vector3.zero;
				result.rotation = Quaternion.identity;
				return result;
			}
		}

		public RingBuffer<TransformData> history;

		public TransformHistory(int capacity = 32)
		{
			history = new RingBuffer<TransformData>(capacity);
		}

		public void UpdateDelay(Pose curPose, long timestamp)
		{
			TransformData transformData = default(TransformData);
			transformData.time = timestamp;
			transformData.position = curPose.position;
			transformData.rotation = curPose.rotation;
			TransformData t = transformData;
			history.Add(t);
		}

		public void SampleTransform(long timestamp, out Vector3 delayedPos, out Quaternion delayedRot)
		{
			TransformData transformAtTime = TransformData.GetTransformAtTime(history, timestamp);
			delayedPos = transformAtTime.position;
			delayedRot = transformAtTime.rotation;
		}
	}
}
