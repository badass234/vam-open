using System;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentArgumentCache
	{
		public bool m_BoolArgument;

		public float m_FloatArgument;

		public int m_IntArgument;

		public string m_StringArgument;

		public long m_ObjectArgument;

		public string m_ObjectArgumentAssemblyTypeName;

		private static bool m_isFieldInfoInitialized;

		private static FieldInfo m_boolArgumentFieldInfo;

		private static FieldInfo m_floatArgumentFieldInfo;

		private static FieldInfo m_intArgumentFieldInfo;

		private static FieldInfo m_stringArgumentFieldInfo;

		private static FieldInfo m_objectArgumentFieldInfo;

		private static FieldInfo m_objectArgumentAssemblyTypeNameFieldInfo;

		private static void Initialize(Type type)
		{
			if (!m_isFieldInfoInitialized)
			{
				m_boolArgumentFieldInfo = type.GetField("m_BoolArgument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_boolArgumentFieldInfo == null)
				{
					throw new NotSupportedException("m_BoolArgument FieldInfo not found.");
				}
				m_floatArgumentFieldInfo = type.GetField("m_FloatArgument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_floatArgumentFieldInfo == null)
				{
					throw new NotSupportedException("m_FloatArgument FieldInfo not found.");
				}
				m_intArgumentFieldInfo = type.GetField("m_IntArgument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_intArgumentFieldInfo == null)
				{
					throw new NotSupportedException("m_IntArgument FieldInfo not found.");
				}
				m_stringArgumentFieldInfo = type.GetField("m_StringArgument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_stringArgumentFieldInfo == null)
				{
					throw new NotSupportedException("m_StringArgument FieldInfo not found.");
				}
				m_objectArgumentFieldInfo = type.GetField("m_ObjectArgument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_objectArgumentFieldInfo == null)
				{
					throw new NotSupportedException("m_ObjectArgument FieldInfo not found.");
				}
				m_objectArgumentAssemblyTypeNameFieldInfo = type.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (m_objectArgumentAssemblyTypeNameFieldInfo == null)
				{
					throw new NotSupportedException("m_ObjectArgumentAssemblyTypeName FieldInfo not found.");
				}
				m_isFieldInfoInitialized = true;
			}
		}

		public void ReadFrom(object obj)
		{
			if (obj == null)
			{
				m_BoolArgument = false;
				m_FloatArgument = 0f;
				m_IntArgument = 0;
				m_StringArgument = null;
				m_ObjectArgument = 0L;
				m_ObjectArgumentAssemblyTypeName = null;
			}
			else
			{
				Initialize(obj.GetType());
				m_BoolArgument = (bool)m_boolArgumentFieldInfo.GetValue(obj);
				m_FloatArgument = (float)m_floatArgumentFieldInfo.GetValue(obj);
				m_IntArgument = (int)m_intArgumentFieldInfo.GetValue(obj);
				m_StringArgument = (string)m_stringArgumentFieldInfo.GetValue(obj);
				UnityEngine.Object obj2 = (UnityEngine.Object)m_objectArgumentFieldInfo.GetValue(obj);
				m_ObjectArgument = obj2.GetMappedInstanceID();
				m_ObjectArgumentAssemblyTypeName = (string)m_objectArgumentAssemblyTypeNameFieldInfo.GetValue(obj);
			}
		}

		public void GetDependencies(object obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			if (obj != null)
			{
				Initialize(obj.GetType());
				UnityEngine.Object obj2 = (UnityEngine.Object)m_objectArgumentFieldInfo.GetValue(obj);
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
			AddDependency(m_ObjectArgument, dependencies, objects, allowNulls);
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
				m_boolArgumentFieldInfo.SetValue(obj, m_BoolArgument);
				m_floatArgumentFieldInfo.SetValue(obj, m_FloatArgument);
				m_intArgumentFieldInfo.SetValue(obj, m_IntArgument);
				m_stringArgumentFieldInfo.SetValue(obj, m_StringArgument);
				m_objectArgumentFieldInfo.SetValue(obj, objects.Get(m_ObjectArgument));
				m_objectArgumentAssemblyTypeNameFieldInfo.SetValue(obj, m_ObjectArgumentAssemblyTypeName);
			}
		}
	}
}
