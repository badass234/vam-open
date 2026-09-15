using System;
using System.Reflection;
using UnityEngine;

namespace LeapInternal
{
	public static class Logger
	{
		public static void Log(object message)
		{
			Debug.Log(message);
		}

		public static void LogStruct(object thisObject, string title = "")
		{
			try
			{
				if (!thisObject.GetType().IsValueType)
				{
					Log(title + " ---- Trying to log non-struct with struct logger");
					return;
				}
				Log(title + " ---- " + thisObject.GetType().ToString());
				FieldInfo[] fields = thisObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					object value = fieldInfo.GetValue(thisObject);
					string text = ((value != null) ? value.ToString() : "null");
					Log(" -------- Name: " + fieldInfo.Name + ", Value = " + text);
				}
			}
			catch (Exception ex)
			{
				Log(ex.Message);
			}
		}
	}
}
