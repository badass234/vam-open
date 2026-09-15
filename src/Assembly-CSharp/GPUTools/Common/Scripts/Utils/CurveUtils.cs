using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Utils
{
	public class CurveUtils
	{
		public static Vector3 GetSplinePoint(List<Vector3> points, float t)
		{
			int b = points.Count - 1;
			int num = (int)(t * (float)points.Count);
			float num2 = 1f / (float)points.Count;
			float t2 = t % num2 * (float)points.Count;
			int index = Mathf.Max(0, num - 1);
			int index2 = Mathf.Min(num, b);
			int index3 = Mathf.Min(num + 1, b);
			Vector3 vector = points[index];
			Vector3 vector2 = points[index2];
			Vector3 vector3 = points[index3];
			Vector3 p = (vector + vector2) * 0.5f;
			Vector3 p2 = (vector2 + vector3) * 0.5f;
			return GetBezierPoint(p, vector2, p2, t2);
		}

		public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
		{
			float num = 1f - t;
			return num * num * p0 + 2f * num * t * p1 + t * t * p2;
		}
	}
}
