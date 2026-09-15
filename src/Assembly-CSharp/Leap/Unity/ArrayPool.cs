using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public static class ArrayPool<T>
	{
		private static Dictionary<int, Stack<T[]>> _bins;

		static ArrayPool()
		{
			_bins = new Dictionary<int, Stack<T[]>>();
			_bins[0] = new Stack<T[]>();
			for (int i = 0; i < 32; i++)
			{
				_bins[1 << i] = new Stack<T[]>();
			}
		}

		public static T[] Spawn(int minLength)
		{
			int num = Mathf.NextPowerOfTwo(minLength);
			Stack<T[]> stack = _bins[num];
			if (stack.Count > 0)
			{
				return stack.Pop();
			}
			return new T[num];
		}

		public static void Recycle(T[] array)
		{
			Array.Clear(array, 0, array.Length);
			int key = Mathf.NextPowerOfTwo(array.Length + 1) / 2;
			_bins[key].Push(array);
		}
	}
}
