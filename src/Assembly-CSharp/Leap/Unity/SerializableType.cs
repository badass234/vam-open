using System;
using System.Reflection;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public struct SerializableType : ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private Type _type;

		[SerializeField]
		[HideInInspector]
		private string _fullName;

		private static Assembly[] _cachedAssemblies;

		private static Assembly[] _assemblies
		{
			get
			{
				if (_cachedAssemblies == null)
				{
					_cachedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				}
				return _cachedAssemblies;
			}
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(_fullName))
			{
				Assembly[] assemblies = _assemblies;
				foreach (Assembly assembly in assemblies)
				{
					_type = assembly.GetType(_fullName, false);
					if (_type != null)
					{
						break;
					}
				}
			}
			else
			{
				_type = null;
			}
		}

		public void OnBeforeSerialize()
		{
			if (_type != null)
			{
				_fullName = _type.FullName;
			}
		}

		public static implicit operator Type(SerializableType serializableType)
		{
			return serializableType._type;
		}

		public static implicit operator SerializableType(Type type)
		{
			SerializableType result = default(SerializableType);
			result._type = type;
			return result;
		}
	}
}
