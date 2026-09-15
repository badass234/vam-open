using UnityEngine;

namespace GPUTools.Common.Scripts.Utils
{
	public static class TransformUtils
	{
		public static Vector3[] TransformPoints(this Transform transform, Vector3[] points)
		{
			Vector3[] array = new Vector3[points.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = transform.TransformPoint(points[i]);
			}
			return array;
		}

		public static Vector3[] InverseTransformPoints(this Transform transform, Vector3[] points)
		{
			Vector3[] array = new Vector3[points.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = transform.InverseTransformPoint(points[i]);
			}
			return array;
		}

		public static void TransformPoints(this Transform transform, ref Vector3[] points)
		{
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = transform.TransformPoint(points[i]);
			}
		}

		public static Vector3[] TransformVectors(this Transform transform, Vector3[] vectors)
		{
			Vector3[] array = new Vector3[vectors.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = transform.TransformVector(vectors[i]);
			}
			return array;
		}

		public static void TransformVectors(this Transform transform, ref Vector3[] vectors)
		{
			for (int i = 0; i < vectors.Length; i++)
			{
				vectors[i] = transform.TransformVector(vectors[i]);
			}
		}

		public static Vector3[] TransformDirrections(this Transform transform, Vector3[] dirrections)
		{
			Vector3[] array = new Vector3[dirrections.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = transform.TransformDirection(dirrections[i]);
			}
			return array;
		}

		public static void TransformDirrections(this Transform transform, ref Vector3[] dirrections)
		{
			for (int i = 0; i < dirrections.Length; i++)
			{
				dirrections[i] = transform.TransformDirection(dirrections[i]);
			}
		}

		public static Vector3[] TransformPoints(this Matrix4x4 matrix, Vector3[] points)
		{
			Vector3[] array = new Vector3[points.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = matrix.MultiplyPoint3x4(points[i]);
			}
			return array;
		}

		public static Vector3[] InverseTransformPoints(this Matrix4x4 matrix, Vector3[] points)
		{
			Vector3[] array = new Vector3[points.Length];
			Matrix4x4 inverse = matrix.inverse;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = inverse.MultiplyPoint3x4(points[i]);
			}
			return array;
		}
	}
}
