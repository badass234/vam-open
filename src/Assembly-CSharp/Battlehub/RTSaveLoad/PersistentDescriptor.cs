using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentDescriptor
	{
		public static readonly HashSet<Type> IgnoreTypes = new HashSet<Type>(new Type[0]);

		public static readonly Dictionary<Type, HashSet<Type>> m_dependencies = new Dictionary<Type, HashSet<Type>> { 
		{
			typeof(ParticleSystemRenderer),
			new HashSet<Type> { typeof(ParticleSystem) }
		} };

		private static Shader m_standard;

		public long InstanceId;

		public string TypeName;

		[ProtoIgnore]
		public PersistentDescriptor Parent;

		public PersistentDescriptor[] Children;

		public PersistentDescriptor[] Components;

		public PersistentDescriptor()
		{
			Children = new PersistentDescriptor[0];
			Components = new PersistentDescriptor[0];
		}

		public PersistentDescriptor(UnityEngine.Object obj)
		{
			InstanceId = obj.GetMappedInstanceID();
			TypeName = obj.GetType().AssemblyQualifiedName;
			Children = new PersistentDescriptor[0];
			Components = new PersistentDescriptor[0];
		}

		public override string ToString()
		{
			string text = string.Empty;
			PersistentDescriptor persistentDescriptor = this;
			if (persistentDescriptor.Parent == null)
			{
				text += "/";
			}
			else
			{
				while (persistentDescriptor.Parent != null)
				{
					text = text + "/" + persistentDescriptor.Parent.InstanceId;
					persistentDescriptor = persistentDescriptor.Parent;
				}
			}
			return $"Descriptor InstanceId = {InstanceId}, Type = {TypeName}, Path = {text}, Children = {((Children != null) ? Children.Length : 0)} Components = {((Components != null) ? Components.Length : 0)}";
		}

		public long[] GetInstanceIds()
		{
			if ((Children == null || Children.Length == 0) && (Components == null || Components.Length == 0))
			{
				return new long[1] { InstanceId };
			}
			List<long> list = new List<long>();
			GetInstanceIds(this, list);
			return list.ToArray();
		}

		private void GetInstanceIds(PersistentDescriptor descriptor, List<long> instanceIds)
		{
			instanceIds.Add(descriptor.InstanceId);
			if (descriptor.Components != null)
			{
				for (int i = 0; i < descriptor.Components.Length; i++)
				{
					GetInstanceIds(descriptor.Components[i], instanceIds);
				}
			}
			if (descriptor.Children != null)
			{
				for (int j = 0; j < descriptor.Children.Length; j++)
				{
					GetInstanceIds(descriptor.Children[j], instanceIds);
				}
			}
		}

		public void FindReferencedObjects(Dictionary<long, UnityEngine.Object> referredObjects, Dictionary<long, UnityEngine.Object> allObjects, bool allowNulls)
		{
			FindReferencedObjects(this, referredObjects, allObjects, allowNulls);
		}

		private void FindReferencedObjects(PersistentDescriptor descriptor, Dictionary<long, UnityEngine.Object> referencedObjects, Dictionary<long, UnityEngine.Object> allObjects, bool allowNulls)
		{
			UnityEngine.Object value;
			if (allObjects.TryGetValue(descriptor.InstanceId, out value))
			{
				if (!referencedObjects.ContainsKey(descriptor.InstanceId))
				{
					referencedObjects.Add(descriptor.InstanceId, value);
				}
			}
			else if (allowNulls && !referencedObjects.ContainsKey(descriptor.InstanceId))
			{
				referencedObjects.Add(descriptor.InstanceId, null);
			}
			if (descriptor.Components != null)
			{
				for (int i = 0; i < descriptor.Components.Length; i++)
				{
					PersistentDescriptor descriptor2 = descriptor.Components[i];
					FindReferencedObjects(descriptor2, referencedObjects, allObjects, allowNulls);
				}
			}
			if (descriptor.Children != null)
			{
				for (int j = 0; j < descriptor.Children.Length; j++)
				{
					PersistentDescriptor descriptor3 = descriptor.Children[j];
					FindReferencedObjects(descriptor3, referencedObjects, allObjects, allowNulls);
				}
			}
		}

		public PersistentDescriptor[] FlattenHierarchy()
		{
			List<PersistentDescriptor> list = new List<PersistentDescriptor>();
			FlattenHierarchy(this, list);
			return list.ToArray();
		}

		private void FlattenHierarchy(PersistentDescriptor descriptor, List<PersistentDescriptor> descriptors)
		{
			descriptors.Add(descriptor);
			if (descriptor.Components != null)
			{
				for (int i = 0; i < descriptor.Components.Length; i++)
				{
					descriptors.Add(descriptor.Components[i]);
				}
			}
			if (descriptor.Children != null)
			{
				for (int j = 0; j < descriptor.Children.Length; j++)
				{
					FlattenHierarchy(descriptor.Children[j], descriptors);
				}
			}
		}

		public static UnityEngine.Object GetOrCreateObject(PersistentDescriptor descriptor, Dictionary<long, UnityEngine.Object> dependencies, Dictionary<long, UnityEngine.Object> decomposition = null)
		{
			Type type = Type.GetType(descriptor.TypeName);
			if (type == null)
			{
				Debug.LogError("Unable to find System.Type for " + descriptor.TypeName);
				return null;
			}
			if (type == typeof(GameObject))
			{
				GameObject[] orCreateGameObjects = GetOrCreateGameObjects(new PersistentDescriptor[1] { descriptor }, dependencies, decomposition);
				return orCreateGameObjects[0];
			}
			UnityEngine.Object value;
			if (!dependencies.TryGetValue(descriptor.InstanceId, out value))
			{
				value = CreateInstance(type);
			}
			if (value == null)
			{
				Debug.LogError("Unable to instantiate object of type " + type.FullName);
				return null;
			}
			if (!dependencies.ContainsKey(descriptor.InstanceId))
			{
				dependencies.Add(descriptor.InstanceId, value);
			}
			decomposition?.Add(descriptor.InstanceId, value);
			return value;
		}

		private static UnityEngine.Object CreateInstance(Type type)
		{
			if (type == typeof(Material))
			{
				if (m_standard == null)
				{
					m_standard = Shader.Find("Standard");
				}
				return new Material(m_standard);
			}
			if (type == typeof(Texture2D))
			{
				return new Texture2D(1, 1, TextureFormat.ARGB32, true);
			}
			return (UnityEngine.Object)Activator.CreateInstance(type);
		}

		public static GameObject[] GetOrCreateGameObjects(PersistentDescriptor[] descriptors, Dictionary<long, UnityEngine.Object> dependencies, Dictionary<long, UnityEngine.Object> decomposition = null)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (PersistentDescriptor descriptor in descriptors)
			{
				CreateGameObjectWithComponents(descriptor, list, dependencies, decomposition);
			}
			return list.ToArray();
		}

		public static PersistentDescriptor[] CreatePersistentDescriptors(UnityEngine.Object[] objects)
		{
			List<PersistentDescriptor> list = new List<PersistentDescriptor>();
			foreach (UnityEngine.Object @object in objects)
			{
				if (!(@object == null))
				{
					PersistentDescriptor item = new PersistentDescriptor(@object);
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		private static void CreateGameObjectWithComponents(PersistentDescriptor descriptor, List<GameObject> createdGameObjects, Dictionary<long, UnityEngine.Object> objects, Dictionary<long, UnityEngine.Object> decomposition = null)
		{
			GameObject gameObject;
			if (objects.ContainsKey(descriptor.InstanceId))
			{
				UnityEngine.Object @object = objects[descriptor.InstanceId];
				if (@object != null && !(@object is GameObject))
				{
					Debug.LogError(string.Concat("Invalid Type ", @object.name, " ", @object.GetType(), " ", @object.GetInstanceID(), " ", descriptor.TypeName));
				}
				gameObject = (GameObject)@object;
			}
			else
			{
				gameObject = new GameObject();
				objects.Add(descriptor.InstanceId, gameObject);
			}
			if (decomposition != null && !decomposition.ContainsKey(descriptor.InstanceId))
			{
				decomposition.Add(descriptor.InstanceId, gameObject);
			}
			createdGameObjects.Add(gameObject);
			gameObject.SetActive(false);
			if (descriptor.Parent != null)
			{
				if (!objects.ContainsKey(descriptor.Parent.InstanceId))
				{
					throw new ArgumentException(string.Format("objects dictionary is supposed to have object with instance id {0} at this stage. Descriptor {1}", descriptor.Parent.InstanceId, descriptor, "descriptor"));
				}
				GameObject gameObject2 = objects[descriptor.Parent.InstanceId] as GameObject;
				if (gameObject2 == null)
				{
					throw new ArgumentException(string.Format("object with instance id {0} should have GameObject type. Descriptor {1}", descriptor.Parent.InstanceId, descriptor, "descriptor"));
				}
				gameObject.transform.SetParent(gameObject2.transform, false);
			}
			if (descriptor.Components != null)
			{
				HashSet<Type> requirements = new HashSet<Type>();
				for (int i = 0; i < descriptor.Components.Length; i++)
				{
					PersistentDescriptor persistentDescriptor = descriptor.Components[i];
					Type type = Type.GetType(persistentDescriptor.TypeName);
					if (type == null)
					{
						Debug.LogWarningFormat("Unknown type {0} associated with component Descriptor {1}", persistentDescriptor.TypeName, persistentDescriptor.ToString());
						continue;
					}
					if (!type.IsSubclassOf(typeof(Component)))
					{
						Debug.LogErrorFormat("{0} is not subclass of {1}", type.FullName, typeof(Component).FullName);
						continue;
					}
					UnityEngine.Object object2;
					if (objects.ContainsKey(persistentDescriptor.InstanceId))
					{
						object2 = objects[persistentDescriptor.InstanceId];
						if (object2 != null && !(object2 is Component))
						{
							Debug.LogError(string.Concat("Invalid Type. Component ", object2.name, " ", object2.GetType(), " ", object2.GetInstanceID(), " ", descriptor.TypeName, " ", persistentDescriptor.TypeName));
						}
					}
					else
					{
						object2 = AddComponent(objects, gameObject, requirements, persistentDescriptor, type);
					}
					if (decomposition != null && !decomposition.ContainsKey(persistentDescriptor.InstanceId))
					{
						decomposition.Add(persistentDescriptor.InstanceId, object2);
					}
				}
			}
			if (descriptor.Children != null)
			{
				for (int j = 0; j < descriptor.Children.Length; j++)
				{
					PersistentDescriptor descriptor2 = descriptor.Children[j];
					CreateGameObjectWithComponents(descriptor2, createdGameObjects, objects, decomposition);
				}
			}
		}

		private static UnityEngine.Object AddComponent(Dictionary<long, UnityEngine.Object> objects, GameObject go, HashSet<Type> requirements, PersistentDescriptor componentDescriptor, Type componentType)
		{
			Component component;
			if (requirements.Contains(componentType) || componentType.IsSubclassOf(typeof(Transform)) || componentType == typeof(Transform) || componentType.IsDefined(typeof(DisallowMultipleComponent), true) || (m_dependencies.ContainsKey(componentType) && m_dependencies[componentType].Any((Type d) => go.GetComponent(d) != null)))
			{
				component = go.GetComponent(componentType);
				if (component == null)
				{
					component = go.AddComponent(componentType);
				}
			}
			else
			{
				component = go.AddComponent(componentType);
				if (component == null)
				{
					component = go.GetComponent(componentType);
				}
			}
			if (component == null)
			{
				Debug.LogErrorFormat("Unable to add or get component of type {0}", componentType);
			}
			else
			{
				object[] customAttributes = component.GetType().GetCustomAttributes(typeof(RequireComponent), true);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					RequireComponent requireComponent = customAttributes[i] as RequireComponent;
					if (requireComponent != null)
					{
						if (requireComponent.m_Type0 != null && !requirements.Contains(requireComponent.m_Type0))
						{
							requirements.Add(requireComponent.m_Type0);
						}
						if (requireComponent.m_Type1 != null && !requirements.Contains(requireComponent.m_Type1))
						{
							requirements.Add(requireComponent.m_Type1);
						}
						if (requireComponent.m_Type2 != null && !requirements.Contains(requireComponent.m_Type2))
						{
							requirements.Add(requireComponent.m_Type2);
						}
					}
				}
				objects.Add(componentDescriptor.InstanceId, component);
			}
			return component;
		}

		public static PersistentDescriptor CreateDescriptor(GameObject go, PersistentDescriptor parentDescriptor = null)
		{
			PersistentIgnore component = go.GetComponent<PersistentIgnore>();
			if (component != null)
			{
				return null;
			}
			PersistentDescriptor persistentDescriptor = new PersistentDescriptor(go);
			persistentDescriptor.Parent = parentDescriptor;
			Component[] array;
			if (component == null)
			{
				array = (from c in go.GetComponents<Component>()
					where c != null && !IgnoreTypes.Contains(c.GetType())
					select c).ToArray();
			}
			else
			{
				array = go.GetComponents<Transform>();
				Array.Resize(ref array, array.Length + 1);
				array[array.Length - 1] = component;
			}
			if (array.Length > 0)
			{
				persistentDescriptor.Components = new PersistentDescriptor[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					Component obj = array[i];
					PersistentDescriptor persistentDescriptor2 = new PersistentDescriptor(obj);
					persistentDescriptor2.Parent = persistentDescriptor;
					persistentDescriptor.Components[i] = persistentDescriptor2;
				}
			}
			Transform transform = go.transform;
			if (transform.childCount > 0)
			{
				List<PersistentDescriptor> list = new List<PersistentDescriptor>();
				foreach (Transform item in transform)
				{
					if (component == null)
					{
						PersistentDescriptor persistentDescriptor3 = CreateDescriptor(item.gameObject, persistentDescriptor);
						if (persistentDescriptor3 != null)
						{
							list.Add(persistentDescriptor3);
						}
					}
				}
				persistentDescriptor.Children = list.ToArray();
			}
			return persistentDescriptor;
		}

		[ProtoAfterDeserialization]
		public void OnDeserialized()
		{
			if (Components != null)
			{
				for (int i = 0; i < Components.Length; i++)
				{
					Components[i].Parent = this;
				}
			}
			if (Children != null)
			{
				for (int j = 0; j < Children.Length; j++)
				{
					Children[j].Parent = this;
				}
			}
		}
	}
}
