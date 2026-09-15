using UnityEngine;

namespace GPUTools.Common.Scripts.Utils
{
	public class TriangleUtils
	{
		public static Rect FindBoundRect(Vector2[] points)
		{
			Rect rect = default(Rect);
			rect.min = points[0];
			rect.max = points[0];
			Rect result = rect;
			for (int i = 1; i < points.Length; i++)
			{
				if (points[i].x < result.min.x)
				{
					result.min = new Vector2(points[i].x, result.min.y);
				}
				if (points[i].y < result.min.y)
				{
					result.min = new Vector2(result.min.x, points[i].y);
				}
				if (points[i].x > result.max.x)
				{
					result.max = new Vector2(points[i].x, result.max.y);
				}
				if (points[i].y > result.max.y)
				{
					result.max = new Vector2(result.max.x, points[i].y);
				}
			}
			return result;
		}

		public static bool IsPointInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 barycentricInsideTriangle = GetBarycentricInsideTriangle(p, a, b, c);
			return barycentricInsideTriangle.x >= 0f && barycentricInsideTriangle.y >= 0f && barycentricInsideTriangle.x + barycentricInsideTriangle.y <= 1f;
		}

		public static bool IsPointInsideTriangle(Vector2 barycentric)
		{
			return barycentric.x >= 0f && barycentric.y >= 0f && barycentric.x + barycentric.y <= 1f;
		}

		public static Vector3 GetPointInsideTriangle(Vector3 a, Vector3 b, Vector3 c, Vector2 barycentric)
		{
			return a * (1f - (barycentric.x + barycentric.y)) + b * barycentric.y + c * barycentric.x;
		}

		public static Vector2 GetBarycentricInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 vector = c - a;
			Vector2 vector2 = b - a;
			Vector2 rhs = p - a;
			float num = Vector2.Dot(vector, vector);
			float num2 = Vector2.Dot(vector, vector2);
			float num3 = Vector2.Dot(vector, rhs);
			float num4 = Vector2.Dot(vector2, vector2);
			float num5 = Vector2.Dot(vector2, rhs);
			float num6 = 1f / (num * num4 - num2 * num2);
			float x = (num4 * num3 - num2 * num5) * num6;
			float y = (num * num5 - num2 * num3) * num6;
			return new Vector2(x, y);
		}
	}
}
