using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class AssertHelper
{
	[Conditional("UNITY_EDITOR")]
	public static void AssertRuntimeOnly(string message = null)
	{
		message = message ?? "Assert failed because game was not in Play Mode.";
	}

	[Conditional("UNITY_EDITOR")]
	public static void AssertEditorOnly(string message = null)
	{
		message = message ?? "Assert failed because game was in Play Mode.";
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Implies(bool condition, bool result, string message = "")
	{
		if (!condition)
		{
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Implies(bool condition, Func<bool> result, string message = "")
	{
		if (!condition)
		{
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Implies(string conditionName, bool condition, string resultName, bool result)
	{
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Implies(string conditionName, bool condition, string resultName, Func<bool> result)
	{
		if (!condition)
		{
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Contains<T>(T value, IEnumerable<T> collection, string message = "")
	{
		if (collection.Contains(value))
		{
			return;
		}
		string text = string.Concat("The value ", value, " was not found in the collection [");
		bool flag = true;
		foreach (T item in collection)
		{
			if (!flag)
			{
				text += ", ";
				flag = false;
			}
			text += item.ToString();
		}
		text = text + "]\n" + message;
	}
}
