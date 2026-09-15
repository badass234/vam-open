using System;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPersistentCall
	{
		public PersistentArgumentCache m_Arguments;

		public UnityEventCallState m_CallState;

		public string m_MethodName;

		public PersistentListenerMode m_Mode;

		public long m_Target;

		public string TypeName;

		private static bool m_isFieldInfoInitialized;

		private static FieldInfo m_argumentsFieldInfo;

		private static FieldInfo m_callStatFieldInfo;

		private static FieldInfo m_methodNameFieldInfo;

		private static FieldInfo m_modeFieldInfo;

		private static FieldInfo m_targetFieldInfo;

		private static void Initialize(Type type)
		{
			if (!m_isFieldInfoInitialized)
			{
				m_argumentsFieldInfo = type.GetField("m_Arguments", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_argumentsFieldInfo == null)
				{
					throw new NotSupportedException("m_Arguments FieldInfo not found.");
				}
				m_callStatFieldInfo = type.GetField("m_CallState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_callStatFieldInfo == null)
				{
					throw new NotSupportedException("m_CallState FieldInfo not found.");
				}
				m_methodNameFieldInfo = type.GetField("m_MethodName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_methodNameFieldInfo == null)
				{
					throw new NotSupportedException("m_MethodName FieldInfo not found.");
				}
				m_modeFieldInfo = type.GetField("m_Mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_modeFieldInfo == null)
				{
					throw new NotSupportedException("m_Mode FieldInfo not found.");
				}
				m_targetFieldInfo = type.GetField("m_Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_targetFieldInfo == null)
				{
					throw new NotSupportedException("m_Target FieldInfo not found.");
				}
				m_isFieldInfoInitialized = true;
			}
		}

		public void ReadFrom(object obj)
		{
			if (obj == null)
			{
				m_Arguments = null;
				m_CallState = UnityEventCallState.Off;
				m_MethodName = null;
				m_Mode = PersistentListenerMode.EventDefined;
				m_Target = 0L;
			}
			else
			{
				Initialize(obj.GetType());
				m_Arguments = new PersistentArgumentCache();
				m_Arguments.ReadFrom(m_argumentsFieldInfo.GetValue(obj));
				m_CallState = (UnityEventCallState)m_callStatFieldInfo.GetValue(obj);
				m_MethodName = (string)m_methodNameFieldInfo.GetValue(obj);
				m_Mode = (PersistentListenerMode)m_modeFieldInfo.GetValue(obj);
				UnityEngine.Object obj2 = (UnityEngine.Object)m_targetFieldInfo.GetValue(obj);
				m_Target = obj2.GetMappedInstanceID();
			}
		}

		public void GetDependencies(object obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			if (obj != null)
			{
				Initialize(obj.GetType());
				PersistentArgumentCache persistentArgumentCache = new PersistentArgumentCache();
				persistentArgumentCache.GetDependencies(m_argumentsFieldInfo.GetValue(obj), dependencies);
				UnityEngine.Object obj2 = (UnityEngine.Object)m_targetFieldInfo.GetValue(obj);
				AddDependency(obj2, dependencies);
			}
		}

		protected void AddDependency(UnityEngine.Object obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			if (!(obj == null))
			{
				long mappedInstanceID = obj.GetMappedInstanceID();
				if (!dependencies.ContainsKey(mappedInstanceID))
				{
					dependencies.Add(mappedInstanceID, obj);
				}
			}
		}

		public void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			if (m_Arguments != null)
			{
				m_Arguments.FindDependencies(dependencies, objects, allowNulls);
			}
			AddDependency(m_Target, dependencies, objects, allowNulls);
		}

		protected void AddDependency<T>(long id, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			T val = objects.Get(id);
			if ((val != null || allowNulls) && !dependencies.ContainsKey(id))
			{
				dependencies.Add(id, val);
			}
		}

		public void WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			if (obj != null)
			{
				Initialize(obj.GetType());
				TypeName = obj.GetType().AssemblyQualifiedName;
				if (m_Arguments != null)
				{
					object obj2 = Activator.CreateInstance(m_argumentsFieldInfo.FieldType);
					m_Arguments.WriteTo(obj2, objects);
					m_argumentsFieldInfo.SetValue(obj, obj2);
				}
				m_callStatFieldInfo.SetValue(obj, m_CallState);
				m_methodNameFieldInfo.SetValue(obj, m_MethodName);
				m_modeFieldInfo.SetValue(obj, m_Mode);
				m_targetFieldInfo.SetValue(obj, objects.Get(m_Target));
			}
		}
	}
}
