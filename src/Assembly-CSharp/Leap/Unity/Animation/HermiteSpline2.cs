using System;
using UnityEngine;

namespace Leap.Unity.Animation
{
	[Serializable]
	public struct HermiteSpline2
	{
		public float t0;

		public float t1;

		public Vector2 pos0;

		public Vector2 pos1;

		public Vector2 vel0;

		public Vector2 vel1;

		public HermiteSpline2(Vector2 pos0, Vector2 pos1)
		{
			t0 = 0f;
			t1 = 1f;
			vel0 = default(Vector2);
			vel1 = default(Vector2);
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline2(Vector2 pos0, Vector2 pos1, Vector2 vel0, Vector2 vel1)
		{
			t0 = 0f;
			t1 = 1f;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline2(Vector2 pos0, Vector2 pos1, Vector2 vel0, Vector2 vel1, float length)
		{
			t0 = 0f;
			t1 = length;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public HermiteSpline2(float t0, float t1, Vector2 pos0, Vector2 pos1, Vector2 vel0, Vector2 vel1)
		{
			this.t0 = t0;
			this.t1 = t1;
			this.vel0 = vel0;
			this.vel1 = vel1;
			this.pos0 = pos0;
			this.pos1 = pos1;
		}

		public Vector2 PositionAt(float t)
		{
			float num = Mathf.Clamp01((t - t0) / (t1 - t0));
			float num2 = num * num;
			float num3 = num2 * num;
			Vector2 vector = (2f * num3 - 3f * num2 + 1f) * pos0;
			Vector2 vector2 = (num3 - 2f * num2 + num) * (t1 - t0) * vel0;
			Vector2 vector3 = (-2f * num3 + 3f * num2) * pos1;
			Vector2 vector4 = (num3 - num2) * (t1 - t0) * vel1;
			return vector + vector2 + vector3 + vector4;
		}

		public Vector2 VelocityAt(float t)
		{
			float num = t1 - t0;
			float num2 = 1f / num;
			float num3 = Mathf.Clamp01((t - t0) * num2);
			float num4 = num2;
			float num5 = num3 * num3;
			float num6 = 2f * num3 * num4;
			float num7 = num6 * num3 + num4 * num5;
			Vector2 vector = (num7 * 2f - num6 * 3f) * pos0;
			Vector2 vector2 = (num7 - 2f * num6 + num4) * num * vel0;
			Vector2 vector3 = (num6 * 3f - 2f * num7) * pos1;
			Vector2 vector4 = (num7 - num6) * num * vel1;
			return vector + vector3 + vector2 + vector4;
		}

		public void PositionAndVelAt(float t, out Vector2 position, out Vector2 velocity)
		{
			float num = t1 - t0;
			float num2 = 1f / num;
			float num3 = Mathf.Clamp01((t - t0) * num2);
			float num4 = num2;
			float num5 = num3 * num3;
			float num6 = 2f * num3 * num4;
			float num7 = num5 * num3;
			float num8 = num6 * num3 + num4 * num5;
			Vector2 vector = (2f * num7 - 3f * num5 + 1f) * pos0;
			Vector2 vector2 = (num8 * 2f - num6 * 3f) * pos0;
			Vector2 vector3 = (num7 - 2f * num5 + num3) * num * vel0;
			Vector2 vector4 = (num8 - 2f * num6 + num4) * num * vel0;
			Vector2 vector5 = (3f * num5 - 2f * num7) * pos1;
			Vector2 vector6 = (num6 * 3f - 2f * num8) * pos1;
			Vector2 vector7 = (num7 - num5) * num * vel1;
			Vector2 vector8 = (num8 - num6) * num * vel1;
			position = vector + vector5 + vector3 + vector7;
			velocity = vector2 + vector6 + vector4 + vector8;
		}
	}
}
