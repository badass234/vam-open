using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentUnityEventBase
	{
		private static FieldInfo m_persistentCallGroupInfo;

		private static FieldInfo m_callsInfo;

		private static Type m_callType;

		public PersistentPersistentCall[] m_calls;

		static PersistentUnityEventBase()
		{
			m_persistentCallGroupInfo = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if (m_persistentCallGroupInfo == null)
			{
				throw new NotSupportedException("m_PersistentCalls FieldInfo not found.");
			}
			Type fieldType = m_persistentCallGroupInfo.FieldType;
			m_callsInfo = fieldType.GetField("m_Calls", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (m_callsInfo == null)
			{
				throw new NotSupportedException("m_Calls FieldInfo not found. ");
			}
			Type fieldType2 = m_callsInfo.FieldType;
			if (!fieldType2.IsGenericType() || fieldType2.GetGenericTypeDefinition() != typeof(List<>))
			{
				throw new NotSupportedException("m_callsInfo.FieldType is not a generic List<>");
			}
			m_callType = fieldType2.GetGenericArguments()[0];
		}

		public void ReadFrom(UnityEventBase obj)
		{
			if (obj == null)
			{
				return;
			}
			object value = m_persistentCallGroupInfo.GetValue(obj);
			if (value == null)
			{
				return;
			}
			object value2 = m_callsInfo.GetValue(value);
			if (value2 != null)
			{
				IList list = (IList)value2;
				m_calls = new PersistentPersistentCall[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					object obj2 = list[i];
					PersistentPersistentCall persistentPersistentCall = new PersistentPersistentCall();
					persistentPersistentCall.ReadFrom(obj2);
					m_calls[i] = persistentPersistentCall;
				}
			}
		}

		public void GetDependencies(UnityEventBase obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			if (obj == null)
			{
				return;
			}
			object value = m_persistentCallGroupInfo.GetValue(obj);
			if (value == null)
			{
				return;
			}
			object value2 = m_callsInfo.GetValue(value);
			if (value2 != null)
			{
				IList list = (IList)value2;
				for (int i = 0; i < list.Count; i++)
				{
					object obj2 = list[i];
					PersistentPersistentCall persistentPersistentCall = new PersistentPersistentCall();
					persistentPersistentCall.GetDependencies(obj2, dependencies);
				}
			}
		}

		public void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			if (m_calls != null)
			{
				for (int i = 0; i < m_calls.Length; i++)
				{
					m_calls[i]?.FindDependencies(dependencies, objects, allowNulls);
				}
			}
		}

		public void WriteTo(UnityEventBase obj, Dictionary<long, UnityEngine.Object> objects)
		{
			if (obj == null || m_calls == null)
			{
				return;
			}
			object obj2 = Activator.CreateInstance(m_persistentCallGroupInfo.FieldType);
			object obj3 = Activator.CreateInstance(m_callsInfo.FieldType);
			IList list = (IList)obj3;
			for (int i = 0; i < m_calls.Length; i++)
			{
				PersistentPersistentCall persistentPersistentCall = m_calls[i];
				if (persistentPersistentCall != null)
				{
					object obj4 = Activator.CreateInstance(m_callType);
					persistentPersistentCall.WriteTo(obj4, objects);
					list.Add(obj4);
				}
				else
				{
					list.Add(null);
				}
			}
			m_callsInfo.SetValue(obj2, obj3);
			m_persistentCallGroupInfo.SetValue(obj, obj2);
		}
	}
}
