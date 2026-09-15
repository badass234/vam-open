using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	public class ObiParticleCache : ScriptableObject
	{
		public class UncompressedFrame
		{
			public List<int> indices = new List<int>();

			public List<Vector3> positions = new List<Vector3>();
		}

		[Serializable]
		public class Frame
		{
			public float time;

			public List<Vector3> positions;

			public List<int> indices;

			public Frame()
			{
				time = 0f;
				positions = new List<Vector3>();
				indices = new List<int>();
			}

			public void Clear()
			{
				time = 0f;
				positions.Clear();
				indices.Clear();
			}

			public int SizeInBytes()
			{
				return 4 + positions.Count * 4 * 3 + indices.Count * 4;
			}

			public static void Lerp(Frame a, Frame b, ref Frame result, float mu)
			{
				result.Clear();
				result.time = Mathf.Lerp(a.time, b.time, mu);
				int num = 0;
				int num2 = 0;
				int count = a.indices.Count;
				int count2 = b.indices.Count;
				float num3 = 1f - mu;
				Vector3 zero = Vector3.zero;
				while (num < count && num2 < count2)
				{
					int num4 = a.indices[num];
					int num5 = b.indices[num2];
					if (num4 > num5)
					{
						result.indices.Add(num5);
						result.positions.Add(b.positions[num2]);
						num2++;
						continue;
					}
					if (num4 < num5)
					{
						result.indices.Add(num4);
						result.positions.Add(a.positions[num]);
						num++;
						continue;
					}
					result.indices.Add(num4);
					Vector3 vector = a.positions[num];
					Vector3 vector2 = b.positions[num2];
					zero.Set(vector.x * num3 + vector2.x * mu, vector.y * num3 + vector2.y * mu, vector.z * num3 + vector2.z * mu);
					result.positions.Add(zero);
					num++;
					num2++;
				}
			}
		}

		public float referenceIntervalSeconds = 0.5f;

		public bool localSpace = true;

		[SerializeField]
		private float duration;

		[SerializeField]
		private List<Frame> frames;

		[SerializeField]
		private List<int> references;

		public float Duration => duration;

		public int FrameCount => frames.Count;

		public void OnEnable()
		{
			if (frames == null)
			{
				frames = new List<Frame>();
			}
			if (references == null)
			{
				references = new List<int> { 0 };
			}
		}

		public int SizeInBytes()
		{
			int num = 0;
			foreach (Frame frame in frames)
			{
				num += frame.SizeInBytes();
			}
			return num + references.Count * 4;
		}

		public void Clear()
		{
			duration = 0f;
			frames.Clear();
			references.Clear();
			references.Add(0);
		}

		private int GetBaseFrame(float time)
		{
			int num = Mathf.FloorToInt(time / referenceIntervalSeconds);
			if (num >= 0 && num < references.Count)
			{
				return references[num];
			}
			return int.MaxValue;
		}

		public void AddFrame(Frame frame)
		{
			int num = Mathf.FloorToInt(frame.time / referenceIntervalSeconds);
			if (num >= references.Count)
			{
				for (float num2 = frame.time - (float)Mathf.Max(0, references.Count - 1) * referenceIntervalSeconds; num2 >= referenceIntervalSeconds; num2 -= referenceIntervalSeconds)
				{
					references.Add(frames.Count);
				}
			}
			if (frame.time >= duration)
			{
				frames.Add(frame);
				duration = frame.time;
				return;
			}
			int num3 = references[num];
			for (int i = num3; i < frames.Count; i++)
			{
				if (frames[i].time > frame.time)
				{
					frames[i] = frame;
					break;
				}
			}
		}

		public void GetFrame(float time, bool interpolate, ref Frame result)
		{
			time = Mathf.Clamp(time, 0f, duration);
			int baseFrame = GetBaseFrame(time);
			for (int i = baseFrame; i < frames.Count; i++)
			{
				if (!(frames[i].time > time))
				{
					continue;
				}
				if (interpolate)
				{
					int num = Mathf.Max(0, i - 1);
					float mu = 0f;
					if (i != num)
					{
						mu = (time - frames[num].time) / (frames[i].time - frames[num].time);
					}
					Frame.Lerp(frames[num], frames[i], ref result, mu);
				}
				else
				{
					result = frames[i];
				}
				break;
			}
		}
	}
}
