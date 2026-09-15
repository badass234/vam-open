using UnityEngine;

namespace GPUTools.Common.Scripts.PL.Tools
{
	public static class ComputeUtils
	{
		public static ComputeBuffer ToComputeBuffer<T>(this T[] array, int stride, ComputeBufferType type = ComputeBufferType.Default)
		{
			ComputeBuffer computeBuffer = new ComputeBuffer(array.Length, stride, type);
			computeBuffer.SetData(array);
			return computeBuffer;
		}

		public static T[] ToArray<T>(this ComputeBuffer buffer)
		{
			T[] array = new T[buffer.count];
			buffer.GetData(array);
			return array;
		}

		public static void LogBuffer<T>(ComputeBuffer buffer)
		{
			T[] array = new T[buffer.count];
			buffer.GetData(array);
			for (int i = 0; i < array.Length; i++)
			{
				Debug.Log($"i:{i} val:{array[i]}");
			}
		}

		public static void LogLargeBuffer<T>(ComputeBuffer buffer)
		{
			T[] array = new T[buffer.count];
			buffer.GetData(array);
			string text = string.Empty;
			for (int i = 1; i <= array.Length; i++)
			{
				text = text + "|" + array[i - 1];
				if (i % 12 == 0)
				{
					Debug.Log($"from i:{i} values:{text}");
					text = string.Empty;
				}
			}
		}
	}
}
