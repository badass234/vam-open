using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Debug
{
	public class LoggerUtil
	{
		public static void LogArray<T>(T[] list, int max)
		{
			for (int i = 0; i < Math.Min(list.Length, max); i++)
			{
				UnityEngine.Debug.Log(list[i]);
			}
		}

		public static void LogList<T>(List<T> list, int max)
		{
			for (int i = 0; i < Math.Min(list.Count, max); i++)
			{
				UnityEngine.Debug.Log(list[i]);
			}
		}
	}
}
