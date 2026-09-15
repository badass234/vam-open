using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DynamicCSharp
{
	public sealed class ScriptAssembly
	{
		private ScriptDomain domain;

		private Assembly rawAssembly;

		public ScriptDomain Domain => domain;

		public ScriptType MainType
		{
			get
			{
				Type[] types = rawAssembly.GetTypes();
				if (types.Length == 0)
				{
					throw new InvalidProgramException("The assembly does not contain a 'MainType'");
				}
				return new ScriptType(this, types[0]);
			}
		}

		public Assembly RawAssembly => rawAssembly;

		internal ScriptAssembly(ScriptDomain domain, Assembly rawAssembly)
		{
			this.domain = domain;
			this.rawAssembly = rawAssembly;
		}

		public bool HasType(string name)
		{
			return FindType(name) != null;
		}

		public bool HasSubtypeOf(Type baseType)
		{
			return FindSubtypeOf(baseType) != null;
		}

		public bool HasSubtypeOf(Type baseType, string name)
		{
			return FindSubtypeOf(baseType, name) != null;
		}

		public bool HasSubtypeOf<T>()
		{
			return FindSubtypeOf<T>() != null;
		}

		public bool HasSubtypeOf<T>(string name)
		{
			return FindSubtypeOf<T>(name) != null;
		}

		public ScriptType FindType(string name)
		{
			Type type = rawAssembly.GetType(name, false, DynamicCSharp.Settings.caseSensitiveNames);
			if (type == null)
			{
				return null;
			}
			return new ScriptType(this, type);
		}

		public ScriptType FindSubtypeOf(Type baseType)
		{
			ScriptType[] array = FindAllTypes();
			foreach (ScriptType scriptType in array)
			{
				if (scriptType.IsSubtypeOf(baseType))
				{
					return scriptType;
				}
			}
			return null;
		}

		public ScriptType FindSubtypeOf(Type baseType, string name)
		{
			ScriptType scriptType = FindType(name);
			if (scriptType == null)
			{
				return null;
			}
			if (scriptType.IsSubtypeOf(baseType))
			{
				return scriptType;
			}
			return null;
		}

		public ScriptType FindSubtypeOf<T>()
		{
			return FindSubtypeOf(typeof(T));
		}

		public ScriptType FindSubtypeOf<T>(string name)
		{
			return FindSubtypeOf(typeof(T), name);
		}

		public ScriptType[] FindAllSubtypesOf(Type baseType)
		{
			List<ScriptType> list = new List<ScriptType>();
			Type[] types = rawAssembly.GetTypes();
			foreach (Type type in types)
			{
				if (DynamicCSharp.Settings.discoverNonPublicTypes || type.IsPublic)
				{
					ScriptType scriptType = new ScriptType(this, type);
					if (scriptType.IsSubtypeOf(baseType))
					{
						list.Add(scriptType);
					}
				}
			}
			return list.ToArray();
		}

		public ScriptType[] FindAllSubtypesOf<T>()
		{
			return FindAllSubtypesOf(typeof(T));
		}

		public ScriptType[] FindAllTypes()
		{
			List<ScriptType> list = new List<ScriptType>();
			Type[] types = rawAssembly.GetTypes();
			foreach (Type type in types)
			{
				if (DynamicCSharp.Settings.discoverNonPublicTypes || type.IsPublic)
				{
					ScriptType item = new ScriptType(this, type);
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public ScriptType[] FindAllUnityTypes()
		{
			return FindAllSubtypesOf<UnityEngine.Object>();
		}

		public ScriptType[] FindAllMonoBehaviourTypes()
		{
			return FindAllSubtypesOf<MonoBehaviour>();
		}

		public ScriptType[] FindAllScriptableObjectTypes()
		{
			return FindAllSubtypesOf<ScriptableObject>();
		}
	}
}
