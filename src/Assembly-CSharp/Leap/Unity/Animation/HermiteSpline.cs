using System;
using UnityEngine;

namespace Leap.Unity.Animation
{
	[Serializable]
	public struct HermiteSpline
	{
		public float t0;

		public float t1;

		public float pos0;

		public float pos1;

		public float vel0;

		public float vel1;

		public HermiteSpline(float pos0, float pos1)
		{
			t0 = 0f;
			t1 = 1f;
			vel0 = 0f;
			vel1 = 0f;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline(float pos0, float pos1, float vel0, float vel1)
		{
			t0 = 0f;
			t1 = 1f;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline(float pos0, float pos1, float vel0, float vel1, float length)
		{
			t0 = 0f;
			t1 = length;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline(float t0, float t1, float pos0, float pos1, float vel0, float vel1)
		{
			this.t0 = t0;
			this.t1 = t1;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public float PositionAt(float t)
		{
			float num = Mathf.Clamp01((t - t0) / (t1 - t0));
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = (2f * num3 - 3f * num2 + 1f) * pos0;
			float num5 = (num3 - 2f * num2 + num) * (t1 - t0) * vel0;
			float num6 = (-2f * num3 + 3f * num2) * pos1;
			float num7 = (num3 - num2) * (t1 - t0) * vel1;
			return num4 + num5 + num6 + num7;
		}

		public float VelocityAt(float t)
		{
			float num = t1 - t0;
			float num2 = 1f / num;
			float num3 = Mathf.Clamp01((t - t0) * num2);
			float num4 = num2;
			float num5 = num3 * num3;
			float num6 = 2f * num3 * num4;
			float num7 = num6 * num3 + num4 * num5;
			float num8 = (num7 * 2f - num6 * 3f) * pos0;
			float num9 = (num7 - 2f * num6 + num4) * num * vel0;
			float num10 = (num6 * 3f - 2f * num7) * pos1;
			float num11 = (num7 - num6) * num * vel1;
			return num8 + num10 + num9 + num11;
		}

		public void PositionAndVelAt(float t, out float position, out float velocity)
		{
			float num = t1 - t0;
			float num2 = 1f / num;
			float num3 = Mathf.Clamp01((t - t0) * num2);
			float num4 = num2;
			float num5 = num3 * num3;
			float num6 = 2f * num3 * num4;
			float num7 = num5 * num3;
			float num8 = num6 * num3 + num4 * num5;
			float num9 = (2f * num7 - 3f * num5 + 1f) * pos0;
			float num10 = (num8 * 2f - num6 * 3f) * pos0;
			float num11 = (num7 - 2f * num5 + num3) * num * vel0;
			float num12 = (num8 - 2f * num6 + num4) * num * vel0;
			float num13 = (3f * num5 - 2f * num7) * pos1;
			float num14 = (num6 * 3f - 2f * num8) * pos1;
			float num15 = (num7 - num5) * num * vel1;
			float num16 = (num8 - num6) * num * vel1;
			position = num9 + num13 + num11 + num15;
			velocity = num10 + num14 + num12 + num16;
		}
	}
}
