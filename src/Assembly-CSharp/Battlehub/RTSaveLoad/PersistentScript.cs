using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlehub.RTSaveLoad.PersistentObjects;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentScript : PersistentObject
	{
		public PersistentData baseObjectData;

		public Dictionary<string, DataContract> fields = new Dictionary<string, DataContract>();

		public string TypeName;

		public PersistentScript()
		{
		}

		public PersistentScript(PersistentData baseObjData)
		{
			baseObjectData = baseObjData;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (baseObjectData != null)
			{
				baseObjectData.ReadFrom(obj);
			}
			Type type = obj.GetType();
			if (!type.IsScript())
			{
				throw new ArgumentException($"obj of type {type} is not subclass of {typeof(MonoBehaviour)}", "obj");
			}
			TypeName = type.AssemblyQualifiedName;
			do
			{
				FieldInfo[] serializableFields = type.GetSerializableFields();
				foreach (FieldInfo fieldInfo in serializableFields)
				{
					if (fields.ContainsKey(fieldInfo.Name))
					{
						Debug.LogErrorFormat("Fields with same names are not supported. Field name {0}", fieldInfo.Name);
						continue;
					}
					bool isArray = fieldInfo.FieldType.IsArray;
					Type type2 = fieldInfo.FieldType;
					if (isArray)
					{
						type2 = type2.GetElementType();
					}
					if (type2.IsSubclassOf(typeof(UnityEngine.Object)) || type2 == typeof(UnityEngine.Object))
					{
						if (isArray)
						{
							UnityEngine.Object[] array = (UnityEngine.Object[])fieldInfo.GetValue(obj);
							if (array != null)
							{
								long[] array2 = new long[array.Length];
								for (int j = 0; j < array2.Length; j++)
								{
									array2[j] = array[j].GetMappedInstanceID();
								}
								fields.Add(fieldInfo.Name, new DataContract(PrimitiveContract.Create(array2)));
							}
							else
							{
								fields.Add(fieldInfo.Name, new DataContract(PrimitiveContract.Create(new long[0])));
							}
						}
						else
						{
							UnityEngine.Object obj2 = (UnityEngine.Object)fieldInfo.GetValue(obj);
							fields.Add(fieldInfo.Name, new DataContract(PrimitiveContract.Create(obj2.GetMappedInstanceID())));
						}
					}
					else if (type2.IsSubclassOf(typeof(UnityEventBase)))
					{
						UnityEventBase unityEventBase = (UnityEventBase)fieldInfo.GetValue(obj);
						if (unityEventBase != null)
						{
							PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
							persistentUnityEventBase.ReadFrom(unityEventBase);
							fields.Add(fieldInfo.Name, new DataContract(persistentUnityEventBase));
						}
					}
					else if (!fieldInfo.FieldType.IsEnum())
					{
						object value = fieldInfo.GetValue(obj);
						if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
						{
							IEnumerable enumerable = (IEnumerable)value;
							foreach (object item in enumerable)
							{
								if (item is IRTSerializable)
								{
									IRTSerializable iRTSerializable = (IRTSerializable)item;
									iRTSerializable.Serialize();
								}
							}
						}
						else if (value is IRTSerializable)
						{
							IRTSerializable iRTSerializable2 = (IRTSerializable)value;
							iRTSerializable2.Serialize();
						}
						if (fieldInfo.FieldType.IsPrimitive() || fieldInfo.FieldType.IsArray())
						{
							PrimitiveContract primitiveContract = PrimitiveContract.Create(fieldInfo.FieldType);
							primitiveContract.ValueBase = value;
							fields.Add(fieldInfo.Name, new DataContract(primitiveContract));
						}
						else
						{
							fields.Add(fieldInfo.Name, new DataContract
							{
								Data = value
							});
						}
					}
					else
					{
						PrimitiveContract primitiveContract2 = PrimitiveContract.Create(typeof(uint));
						primitiveContract2.ValueBase = (uint)Convert.ChangeType(fieldInfo.GetValue(obj), typeof(uint));
						fields.Add(fieldInfo.Name, new DataContract(primitiveContract2));
					}
				}
				type = type.BaseType();
			}
			while (type.IsScript());
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			base.ReadFrom(obj);
			if (baseObjectData != null)
			{
				baseObjectData.GetDependencies(obj, dependencies);
			}
			Type type = obj.GetType();
			if (!type.IsScript())
			{
				throw new ArgumentException($"obj of type {type} is not subclass of {typeof(MonoBehaviour)}", "obj");
			}
			do
			{
				FieldInfo[] serializableFields = type.GetSerializableFields();
				foreach (FieldInfo fieldInfo in serializableFields)
				{
					bool isArray = fieldInfo.FieldType.IsArray;
					Type type2 = fieldInfo.FieldType;
					if (isArray)
					{
						type2 = type2.GetElementType();
					}
					if (type2.IsSubclassOf(typeof(UnityEngine.Object)) || type2 == typeof(UnityEngine.Object))
					{
						if (isArray)
						{
							UnityEngine.Object[] array = (UnityEngine.Object[])fieldInfo.GetValue(obj);
							if (array != null)
							{
								AddDependencies(array, dependencies);
							}
						}
						else
						{
							UnityEngine.Object obj2 = (UnityEngine.Object)fieldInfo.GetValue(obj);
							AddDependency(obj2, dependencies);
						}
					}
					else if (type2.IsSubclassOf(typeof(UnityEventBase)))
					{
						UnityEventBase unityEventBase = (UnityEventBase)fieldInfo.GetValue(obj);
						if (unityEventBase != null)
						{
							PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
							persistentUnityEventBase.GetDependencies(unityEventBase, dependencies);
						}
					}
					else
					{
						if (fieldInfo.FieldType.IsEnum())
						{
							continue;
						}
						object value = fieldInfo.GetValue(obj);
						if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
						{
							IEnumerable enumerable = (IEnumerable)value;
							foreach (object item in enumerable)
							{
								if (item is IRTSerializable)
								{
									IRTSerializable iRTSerializable = (IRTSerializable)item;
									iRTSerializable.GetDependencies(dependencies);
								}
							}
						}
						else if (value is IRTSerializable)
						{
							IRTSerializable iRTSerializable2 = (IRTSerializable)value;
							iRTSerializable2.GetDependencies(dependencies);
						}
					}
				}
				type = type.BaseType();
			}
			while (type.IsScript());
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			if (baseObjectData != null)
			{
				baseObjectData.FindDependencies(dependencies, objects, allowNulls);
			}
			Type type = Type.GetType(TypeName);
			if (type == null)
			{
				Debug.LogWarning(TypeName + " is not found");
				return;
			}
			if (!type.IsScript())
			{
				throw new ArgumentException($"obj of type {type} is not subclass of {typeof(MonoBehaviour)}", "obj");
			}
			do
			{
				FieldInfo[] serializableFields = type.GetSerializableFields();
				foreach (FieldInfo fieldInfo in serializableFields)
				{
					string oldName = fieldInfo.Name;
					if (!fields.ContainsKey(fieldInfo.Name))
					{
						FormerlySerializedAsAttribute formerlySerializedAsAttribute = fieldInfo.GetCustomAttributes(typeof(FormerlySerializedAsAttribute), false).FirstOrDefault() as FormerlySerializedAsAttribute;
						if (formerlySerializedAsAttribute == null)
						{
							continue;
						}
						oldName = formerlySerializedAsAttribute.oldName;
						if (!fields.ContainsKey(oldName))
						{
							continue;
						}
					}
					bool isArray = fieldInfo.FieldType.IsArray;
					Type type2 = fieldInfo.FieldType;
					if (isArray)
					{
						type2 = type2.GetElementType();
					}
					DataContract dataContract = fields[oldName];
					if (type2.IsSubclassOf(typeof(UnityEngine.Object)) || type2 == typeof(UnityEngine.Object))
					{
						if (isArray)
						{
							if (dataContract.AsPrimitive.ValueBase is long[])
							{
								long[] ids = (long[])dataContract.AsPrimitive.ValueBase;
								AddDependencies(ids, dependencies, objects, allowNulls);
							}
						}
						else if (dataContract.AsPrimitive.ValueBase is long)
						{
							long id = (long)dataContract.AsPrimitive.ValueBase;
							AddDependency(id, dependencies, objects, allowNulls);
						}
					}
					else if (dataContract.Data != null)
					{
						if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
						{
							PersistentUnityEventBase asUnityEvent = dataContract.AsUnityEvent;
							asUnityEvent.FindDependencies(dependencies, objects, allowNulls);
						}
					}
					else
					{
						if (fieldInfo.FieldType.IsEnum())
						{
							continue;
						}
						object obj;
						if (fieldInfo.FieldType.IsPrimitive() || fieldInfo.FieldType.IsArray())
						{
							PrimitiveContract asPrimitive = dataContract.AsPrimitive;
							if (asPrimitive == null || (asPrimitive.ValueBase == null && fieldInfo.FieldType.IsValueType()) || (asPrimitive.ValueBase != null && !fieldInfo.FieldType.IsAssignableFrom(asPrimitive.ValueBase.GetType())))
							{
								continue;
							}
							obj = asPrimitive.ValueBase;
						}
						else
						{
							obj = dataContract.Data;
						}
						if (obj is IEnumerable)
						{
							IEnumerable enumerable = (IEnumerable)obj;
							foreach (object item in enumerable)
							{
								if (item is IRTSerializable)
								{
									IRTSerializable iRTSerializable = (IRTSerializable)item;
									iRTSerializable.FindDependencies(dependencies, objects, allowNulls);
								}
							}
						}
						else if (obj is IRTSerializable)
						{
							IRTSerializable iRTSerializable2 = (IRTSerializable)obj;
							iRTSerializable2.FindDependencies(dependencies, objects, allowNulls);
						}
					}
				}
				type = type.BaseType();
			}
			while (type.IsScript());
		}

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (baseObjectData != null)
			{
				PersistentObject asPersistentObject = baseObjectData.AsPersistentObject;
				if (asPersistentObject != null)
				{
					asPersistentObject.name = name;
				}
				baseObjectData.WriteTo(obj, objects);
			}
			Type type = obj.GetType();
			if (!type.IsScript())
			{
				throw new ArgumentException($"obj of type {type} is not subclass of {typeof(MonoBehaviour)}", "obj");
			}
			do
			{
				FieldInfo[] serializableFields = type.GetSerializableFields();
				foreach (FieldInfo fieldInfo in serializableFields)
				{
					string oldName = fieldInfo.Name;
					if (!fields.ContainsKey(fieldInfo.Name))
					{
						FormerlySerializedAsAttribute formerlySerializedAsAttribute = fieldInfo.GetCustomAttributes(typeof(FormerlySerializedAsAttribute), false).FirstOrDefault() as FormerlySerializedAsAttribute;
						if (formerlySerializedAsAttribute == null)
						{
							continue;
						}
						oldName = formerlySerializedAsAttribute.oldName;
						if (!fields.ContainsKey(oldName))
						{
							continue;
						}
					}
					bool isArray = fieldInfo.FieldType.IsArray;
					Type type2 = fieldInfo.FieldType;
					if (isArray)
					{
						type2 = type2.GetElementType();
					}
					DataContract dataContract = fields[oldName];
					if (type2.IsSubclassOf(typeof(UnityEngine.Object)) || type2 == typeof(UnityEngine.Object))
					{
						if (isArray)
						{
							if (dataContract.AsPrimitive != null && dataContract.AsPrimitive.ValueBase is long[])
							{
								long[] array = (long[])dataContract.AsPrimitive.ValueBase;
								Array array2 = Array.CreateInstance(type2, array.Length);
								for (int j = 0; j < array.Length; j++)
								{
									object value = objects.Get(array[j]);
									array2.SetValue(value, j);
								}
								fieldInfo.SetValue(obj, array2);
							}
						}
						else
						{
							if (dataContract.AsPrimitive == null || !(dataContract.AsPrimitive.ValueBase is long))
							{
								continue;
							}
							long num = (long)dataContract.AsPrimitive.ValueBase;
							if (objects.ContainsKey(num))
							{
								object value2 = objects[num];
								try
								{
									fieldInfo.SetValue(obj, value2);
								}
								catch
								{
									Debug.LogError(num);
									throw;
								}
							}
						}
					}
					else if (dataContract == null)
					{
						if (!fieldInfo.FieldType.IsValueType())
						{
							fieldInfo.SetValue(obj, dataContract);
						}
					}
					else if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
					{
						if (dataContract.AsUnityEvent != null)
						{
							PersistentUnityEventBase asUnityEvent = dataContract.AsUnityEvent;
							UnityEventBase unityEventBase = (UnityEventBase)Activator.CreateInstance(fieldInfo.FieldType);
							asUnityEvent.WriteTo(unityEventBase, objects);
							fieldInfo.SetValue(obj, unityEventBase);
						}
					}
					else if (!fieldInfo.FieldType.IsEnum())
					{
						object obj3;
						if (fieldInfo.FieldType.IsPrimitive() || fieldInfo.FieldType.IsArray())
						{
							PrimitiveContract asPrimitive = dataContract.AsPrimitive;
							if (asPrimitive == null || (asPrimitive.ValueBase == null && fieldInfo.FieldType.IsValueType()) || (asPrimitive.ValueBase != null && !fieldInfo.FieldType.IsAssignableFrom(asPrimitive.ValueBase.GetType())))
							{
								continue;
							}
							obj3 = asPrimitive.ValueBase;
						}
						else
						{
							obj3 = dataContract.Data;
						}
						fieldInfo.SetValue(obj, obj3);
						if (obj3 is IEnumerable)
						{
							IEnumerable enumerable = (IEnumerable)obj3;
							foreach (object item in enumerable)
							{
								if (item is IRTSerializable)
								{
									IRTSerializable iRTSerializable = (IRTSerializable)item;
									iRTSerializable.Deserialize(objects);
								}
							}
						}
						else if (obj3 is IRTSerializable)
						{
							IRTSerializable iRTSerializable2 = (IRTSerializable)obj3;
							iRTSerializable2.Deserialize(objects);
						}
					}
					else
					{
						PrimitiveContract asPrimitive2 = dataContract.AsPrimitive;
						if (asPrimitive2 != null && (asPrimitive2.ValueBase != null || !fieldInfo.FieldType.IsValueType()) && (asPrimitive2.ValueBase == null || asPrimitive2.ValueBase.GetType() == typeof(uint)))
						{
							object value3 = Enum.ToObject(fieldInfo.FieldType, asPrimitive2.ValueBase);
							fieldInfo.SetValue(obj, value3);
						}
					}
				}
				type = type.BaseType();
			}
			while (type.IsScript());
			return obj;
		}
	}
}
