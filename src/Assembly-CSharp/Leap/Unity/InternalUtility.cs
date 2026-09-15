using System;
using UnityEngine;

namespace Leap.Unity
{
	public static class InternalUtility
	{
		public static T AddComponent<T>(GameObject obj) where T : Component
		{
			return obj.AddComponent<T>();
		}

		public static Component AddComponent(GameObject obj, Type type)
		{
			return obj.AddComponent(type);
		}

		public static void Destroy(UnityEngine.Object obj)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}
}
