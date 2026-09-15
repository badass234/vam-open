using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	public static class ObiUtils
	{
		public static void DrawArrowGizmo(float bodyLenght, float bodyWidth, float headLenght, float headWidth)
		{
			float num = bodyLenght * 0.5f;
			float num2 = bodyWidth * 0.5f;
			Gizmos.DrawLine(new Vector3(num2, 0f, 0f - num), new Vector3(num2, 0f, num));
			Gizmos.DrawLine(new Vector3(0f - num2, 0f, 0f - num), new Vector3(0f - num2, 0f, num));
			Gizmos.DrawLine(new Vector3(0f - num2, 0f, 0f - num), new Vector3(num2, 0f, 0f - num));
			Gizmos.DrawLine(new Vector3(num2, 0f, num), new Vector3(headWidth, 0f, num));
			Gizmos.DrawLine(new Vector3(0f - num2, 0f, num), new Vector3(0f - headWidth, 0f, num));
			Gizmos.DrawLine(new Vector3(0f, 0f, num + headLenght), new Vector3(headWidth, 0f, num));
			Gizmos.DrawLine(new Vector3(0f, 0f, num + headLenght), new Vector3(0f - headWidth, 0f, num));
		}

		public static void ArrayFill<T>(T[] arrayToFill, T[] fillValue)
		{
			if (fillValue.Length > arrayToFill.Length)
			{
				return;
			}
			Array.Copy(fillValue, arrayToFill, fillValue.Length);
			int num = arrayToFill.Length / 2;
			for (int num2 = fillValue.Length; num2 < arrayToFill.Length; num2 *= 2)
			{
				int length = num2;
				if (num2 > num)
				{
					length = arrayToFill.Length - num2;
				}
				Array.Copy(arrayToFill, 0, arrayToFill, num2, length);
			}
		}

		public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
		{
			if (indexA != indexB && indexB > -1 && indexB < list.Count && indexA > -1 && indexA < list.Count)
			{
				T value = list[indexA];
				list[indexA] = list[indexB];
				list[indexB] = value;
			}
			return list;
		}

		public static void AddRange<T>(ref T[] array, T[] other)
		{
			if (array != null && other != null)
			{
				int index = array.Length;
				Array.Resize(ref array, array.Length + other.Length);
				other.CopyTo(array, index);
			}
		}

		public static void RemoveRange<T>(ref T[] array, int index, int count)
		{
			if (array != null)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException("Index and/or count are < 0.");
				}
				if (index + count > array.Length)
				{
					throw new ArgumentException("Index and count do not denote a valid range of elements.");
				}
				for (int i = index; i < array.Length - count; i++)
				{
					array.SetValue(array.GetValue(i + count), i);
				}
				Array.Resize(ref array, array.Length - count);
			}
		}

		public static Bounds Transform(this Bounds b, Matrix4x4 m)
		{
			Vector4 vector = m.GetColumn(0) * b.min.x;
			Vector4 vector2 = m.GetColumn(0) * b.max.x;
			Vector4 vector3 = m.GetColumn(1) * b.min.y;
			Vector4 vector4 = m.GetColumn(1) * b.max.y;
			Vector4 vector5 = m.GetColumn(2) * b.min.z;
			Vector4 vector6 = m.GetColumn(2) * b.max.z;
			Bounds result = default(Bounds);
			Vector3 vector7 = m.GetColumn(3);
			result.SetMinMax(Vector3.Min(vector, vector2) + Vector3.Min(vector3, vector4) + Vector3.Min(vector5, vector6) + vector7, Vector3.Max(vector, vector2) + Vector3.Max(vector3, vector4) + Vector3.Max(vector5, vector6) + vector7);
			return result;
		}

		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		public static float Mod(float a, float b)
		{
			return a - b * Mathf.Floor(a / b);
		}

		public static float TriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return Mathf.Sqrt(Vector3.Cross(p2 - p1, p3 - p1).sqrMagnitude) / 2f;
		}
	}
}
