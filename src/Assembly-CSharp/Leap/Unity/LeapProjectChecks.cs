using System;
using System.Collections.Generic;
using System.Reflection;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity
{
	public static class LeapProjectChecks
	{
		private struct ProjectCheck
		{
			public Func<bool> checkFunc;

			public LeapProjectCheckAttribute attribute;
		}

		private static List<ProjectCheck> _projectChecks;

		private const string IGNORED_KEYS_PREF = "LeapUnityWindow_IgnoredKeys";

		private static HashSet<string> _ignoredKeys => null;

		private static void ensureChecksLoaded()
		{
			if (_projectChecks != null)
			{
				return;
			}
			_projectChecks = new List<ProjectCheck>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Type item in assemblies.Query().SelectMany((Assembly a) => a.GetTypes()))
			{
				MethodInfo[] methods = item.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo method in methods)
				{
					object[] customAttributes = method.GetCustomAttributes(typeof(LeapProjectCheckAttribute), true);
					if (customAttributes.Length == 0)
					{
						continue;
					}
					LeapProjectCheckAttribute attribute = customAttributes[0] as LeapProjectCheckAttribute;
					_projectChecks.Add(new ProjectCheck
					{
						checkFunc = delegate
						{
							if (!method.IsStatic)
							{
								Debug.LogError("Invalid project check definition; project checks must be static methods.");
								return true;
							}
							return method.ReturnType != typeof(bool) || (bool)method.Invoke(null, null);
						},
						attribute = attribute
					});
				}
			}
			_projectChecks.Sort((ProjectCheck a, ProjectCheck b) => a.attribute.order.CompareTo(b.attribute.order));
		}

		public static void DrawProjectChecksGUI()
		{
		}

		public static bool CheckIgnoredKey(string editorPrefKey)
		{
			return false;
		}

		public static void SetIgnoredKey(string editorPrefKey, bool ignore)
		{
		}

		public static void ClearAllIgnoredKeys()
		{
		}

		private static HashSet<string> splitBySemicolonToSet(string ignoredKeys_semicolonDelimited)
		{
			HashSet<string> hashSet = new HashSet<string>();
			string[] array = ignoredKeys_semicolonDelimited.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in array)
			{
				hashSet.Add(item);
			}
			return hashSet;
		}

		private static string joinBySemicolon(HashSet<string> keys)
		{
			return string.Join(";", keys.Query().ToArray());
		}

		private static void uploadignoredKeyChangesToEditorPrefs()
		{
		}
	}
}
