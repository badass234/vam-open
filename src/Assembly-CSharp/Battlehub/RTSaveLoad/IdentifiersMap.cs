using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class IdentifiersMap
	{
		public const string ResourceMapPrefabName = "ResourceMap";

		public const long T_NULL = 4294967296L;

		private const long T_RESOURCE = 8589934592L;

		private const long T_OBJECT = 17179869184L;

		private const long T_DYNAMIC_RESOURCE = 34359738368L;

		private Dictionary<int, int> m_idToDynamicID = new Dictionary<int, int>();

		private Dictionary<int, int> m_dynamicIDToID = new Dictionary<int, int>();

		private Dictionary<int, int> m_idToId = new Dictionary<int, int>();

		private Dictionary<Guid, int[]> m_loadedBundles = new Dictionary<Guid, int[]>();

		private static IdentifiersMap m_instance;

		public static bool IsInitialized => m_instance != null;

		public static IdentifiersMap Instance
		{
			get
			{
				if (m_instance == null)
				{
					IdentifiersMap identifiersMap = new IdentifiersMap();
					identifiersMap.Initialize();
				}
				return m_instance;
			}
			set
			{
				m_instance = value;
			}
		}

		public bool IsResource(UnityEngine.Object obj)
		{
			return m_idToId.ContainsKey(obj.GetInstanceID());
		}

		public bool IsDynamicResource(UnityEngine.Object obj)
		{
			return m_idToDynamicID.ContainsKey(obj.GetInstanceID());
		}

		public static long ToDynamicResourceID(int id)
		{
			return 0x800000000L | (long)(uint)id;
		}

		public static bool IsNotMapped(UnityEngine.Object obj)
		{
			long mappedInstanceID = obj.GetMappedInstanceID();
			return (mappedInstanceID & 0x400000000L) != 0 || (mappedInstanceID & 0x100000000L) != 0;
		}

		public static bool IsDynamicResourceID(long mappedInstanceID)
		{
			return (mappedInstanceID & 0x800000000L) != 0;
		}

		public long GetMappedInstanceID(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				return 4294967296L;
			}
			int instanceID = obj.GetInstanceID();
			return GetMappedInstanceID(instanceID);
		}

		public long GetMappedInstanceID(int instanceId)
		{
			int value;
			if (m_idToId.TryGetValue(instanceId, out value))
			{
				return 0x200000000L | (long)(uint)value;
			}
			if (m_idToDynamicID.TryGetValue(instanceId, out value))
			{
				return 0x800000000L | (long)(uint)value;
			}
			return 0x400000000L | (long)(uint)instanceId;
		}

		public long[] GetMappedInstanceID(UnityEngine.Object[] obj)
		{
			if (obj == null)
			{
				return null;
			}
			long[] array = new long[obj.Length];
			for (int i = 0; i < obj.Length; i++)
			{
				UnityEngine.Object obj2 = obj[i];
				array[i] = GetMappedInstanceID(obj2);
			}
			return array;
		}

		public void Register(UnityEngine.Object obj, int id)
		{
			if (!m_dynamicIDToID.ContainsKey(id))
			{
				int instanceID = obj.GetInstanceID();
				if (!m_idToDynamicID.ContainsKey(instanceID))
				{
					m_idToDynamicID.Add(instanceID, id);
					m_dynamicIDToID.Add(id, instanceID);
				}
			}
		}

		public void Register(UnityEngine.Object obj, long mappedInstanceId)
		{
			if (!IsDynamicResourceID(mappedInstanceId))
			{
				return;
			}
			int num = (int)mappedInstanceId;
			if (!m_dynamicIDToID.ContainsKey(num))
			{
				int instanceID = obj.GetInstanceID();
				if (!m_idToDynamicID.ContainsKey(instanceID))
				{
					m_idToDynamicID.Add(instanceID, num);
					m_dynamicIDToID.Add(num, instanceID);
				}
			}
		}

		public void Unregister(long mappedInstanceId)
		{
			if (IsDynamicResourceID(mappedInstanceId))
			{
				int key = (int)mappedInstanceId;
				int value;
				if (m_dynamicIDToID.TryGetValue(key, out value))
				{
					m_idToDynamicID.Remove(value);
				}
				m_dynamicIDToID.Remove(key);
			}
		}

		public void Register(AssetBundle bundle)
		{
			string[] allAssetNames = bundle.GetAllAssetNames();
			string text = allAssetNames.Where((string r) => r.Contains("resourcemap")).FirstOrDefault();
			if (text == null)
			{
				GenerateResourceMap(bundle, allAssetNames);
				return;
			}
			GameObject gameObject = bundle.LoadAsset<GameObject>(text);
			if (gameObject == null)
			{
				throw new ArgumentException($"Unable to register bundle. Bundle {bundle.name} does not contain BundleResourceMap", "bundle");
			}
			BundleResourceMap component = gameObject.GetComponent<BundleResourceMap>();
			if (component == null)
			{
				throw new ArgumentException($"Unable to register bundle. Bundle {bundle.name} does not contain BundleResourceMap", "bundle");
			}
			Guid key = new Guid(component.Guid);
			if (m_loadedBundles.ContainsKey(key))
			{
				throw new ArgumentException("bundle " + bundle.name + " already loaded", "bundle");
			}
			List<int> list = new List<int>();
			ResourceGroup[] componentsInChildren = gameObject.GetComponentsInChildren<ResourceGroup>(true);
			foreach (ResourceGroup group in componentsInChildren)
			{
				LoadMappings(group, false, list);
			}
			m_loadedBundles.Add(key, list.ToArray());
		}

		private static uint HashString(string str)
		{
			uint num = 2166136261u;
			for (int i = 0; i < str.Length; i++)
			{
				num ^= char.ToUpper(str[i]);
				num *= 16777619;
			}
			return num;
		}

		private static Guid BundleToGuid(AssetBundle bundle)
		{
			uint value = HashString(bundle.name);
			byte[] array = new byte[16];
			BitConverter.GetBytes(value).CopyTo(array, 0);
			return new Guid(array);
		}

		private static string GenerateUniqueObjectName(AssetBundle bundle, string asset, UnityEngine.Object loadedAsset, UnityEngine.Object obj)
		{
			if (loadedAsset == obj)
			{
				return $"{bundle.name}/{asset}";
			}
			string text = null;
			GameObject gameObject = null;
			if (obj is GameObject)
			{
				text = obj.name;
				gameObject = (GameObject)obj;
			}
			else if (obj is Component)
			{
				Component[] components = ((Component)obj).GetComponents(obj.GetType());
				for (int i = 0; i < components.Length; i++)
				{
					if (components[i] == obj)
					{
						text = $"{obj.name}.{obj.GetType().Name}:{i}";
						break;
					}
				}
				gameObject = ((Component)obj).gameObject;
			}
			else
			{
				text = $"{obj.name}.{obj.GetType().Name}";
			}
			if (gameObject != null && gameObject != loadedAsset)
			{
				Transform transform = gameObject.transform;
				while (gameObject != loadedAsset && transform != null)
				{
					text = $"{gameObject.name}/{text}";
					transform = transform.parent;
					gameObject = transform.gameObject;
				}
			}
			return $"{bundle.name}/{asset}/{text}";
		}

		private void GenerateResourceMap(AssetBundle bundle, string[] assets)
		{
			Guid key = BundleToGuid(bundle);
			if (m_loadedBundles.ContainsKey(key))
			{
				throw new ArgumentException("bundle " + bundle.name + " already loaded", "bundle");
			}
			List<int> list = new List<int>(assets.Length);
			foreach (string text in assets)
			{
				UnityEngine.Object @object = bundle.LoadAsset(text);
				Dictionary<long, UnityEngine.Object> dictionary = new Dictionary<long, UnityEngine.Object>();
				GetDependencies(@object, dictionary);
				GetReferencedObjects(@object, dictionary);
				foreach (UnityEngine.Object value2 in dictionary.Values)
				{
					int instanceID = value2.GetInstanceID();
					if (!m_idToId.ContainsKey(instanceID))
					{
						string str = GenerateUniqueObjectName(bundle, text, @object, value2);
						int value = (int)HashString(str);
						m_idToId.Add(instanceID, value);
						list.Add(instanceID);
					}
				}
			}
			m_loadedBundles.Add(key, list.ToArray());
		}

		private void GetDependencies(UnityEngine.Object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			PersistentData persistentData = PersistentData.Create(obj);
			if (persistentData == null)
			{
				return;
			}
			persistentData.GetDependencies(obj, objects);
			if (!(obj is GameObject))
			{
				return;
			}
			GameObject gameObject = (GameObject)obj;
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component != null)
				{
					persistentData = PersistentData.Create(component);
					persistentData.GetDependencies(component, objects);
				}
			}
			foreach (Transform item in gameObject.transform)
			{
				GetDependencies(item.gameObject, objects);
			}
		}

		private void GetReferencedObjects(UnityEngine.Object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			long mappedInstanceID = obj.GetMappedInstanceID();
			if (!objects.ContainsKey(mappedInstanceID))
			{
				objects.Add(mappedInstanceID, obj);
			}
			if (!(obj is GameObject))
			{
				return;
			}
			GameObject gameObject = (GameObject)obj;
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				mappedInstanceID = component.GetMappedInstanceID();
				if (!objects.ContainsKey(mappedInstanceID))
				{
					objects.Add(mappedInstanceID, component);
				}
			}
			foreach (Transform item in gameObject.transform)
			{
				GetReferencedObjects(item.gameObject, objects);
			}
		}

		public void Unregister(AssetBundle bundle)
		{
			string[] allAssetNames = bundle.GetAllAssetNames();
			string text = allAssetNames.Where((string r) => r.Contains("resourcemap")).FirstOrDefault();
			Guid key;
			if (text == null)
			{
				key = BundleToGuid(bundle);
			}
			else
			{
				GameObject gameObject = bundle.LoadAsset<GameObject>(text);
				if (gameObject == null)
				{
					throw new ArgumentException($"Unable to unregister bundle. Bundle {bundle.name} does not contain BundleResourceMap", "bundle");
				}
				BundleResourceMap component = gameObject.GetComponent<BundleResourceMap>();
				if (component == null)
				{
					throw new ArgumentException($"Unable to unregister bundle. Bundle {bundle.name} does not contain BundleResourceMap", "bundle");
				}
				key = new Guid(component.Guid);
			}
			if (m_loadedBundles.ContainsKey(key))
			{
				int[] array = m_loadedBundles[key];
				for (int i = 0; i < array.Length; i++)
				{
					m_idToId.Remove(array[i]);
				}
				m_loadedBundles.Remove(key);
			}
		}

		private void LoadMappings(ResourceGroup group, bool ignoreConflicts = false, List<int> ids = null)
		{
			if (!group.gameObject.IsPrefab())
			{
				PersistentIgnore component = group.GetComponent<PersistentIgnore>();
				if (component == null || component.IsRuntime)
				{
					return;
				}
			}
			ObjectToID[] mapping = group.Mapping;
			for (int i = 0; i < mapping.Length; i++)
			{
				ObjectToID objectToID = mapping[i];
				if (objectToID.Object == null)
				{
					continue;
				}
				int instanceID = objectToID.Object.GetInstanceID();
				if (m_idToId.ContainsKey(instanceID))
				{
					if (ignoreConflicts)
					{
						continue;
					}
					Debug.LogError("key " + instanceID + "already added. Group " + group.name + " guid " + group.Guid + " mapping " + i + " mapped object " + objectToID.Object);
				}
				m_idToId.Add(instanceID, objectToID.Id);
				ids?.Add(instanceID);
			}
		}

		private void Initialize()
		{
			BundleResourceMap resourceMap = Resources.Load<ResourceMap>("ResourceMap");
			Initialize(resourceMap);
		}

		private void Initialize(BundleResourceMap resourceMap)
		{
			if (resourceMap == null)
			{
				Debug.LogWarning("ResourceMap is null. Create Resource map using Tools->Runtime SaveLoad->Create Resource Map menu item");
				return;
			}
			m_idToId = new Dictionary<int, int>();
			m_instance = this;
			ResourceGroup[] source = Resources.FindObjectsOfTypeAll<ResourceGroup>();
			ResourceGroup[] array = source.Where((ResourceGroup rg) => !rg.gameObject.IsPrefab()).ToArray();
			ResourceGroup[] componentsInChildren = resourceMap.GetComponentsInChildren<ResourceGroup>();
			if (componentsInChildren.Length == 0)
			{
				Debug.LogWarning("No resource groups found. Create Resource map using Tools->Runtime SaveLoad->Create Resource Map menu item");
				return;
			}
			foreach (ResourceGroup group in componentsInChildren)
			{
				bool ignoreConflicts = true;
				LoadMappings(group, ignoreConflicts);
			}
			foreach (ResourceGroup group2 in array)
			{
				LoadMappings(group2);
			}
		}

		public static Dictionary<long, UnityEngine.Object> FindResources(bool includeDynamicResources)
		{
			Dictionary<long, UnityEngine.Object> dictionary = new Dictionary<long, UnityEngine.Object>();
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
			foreach (UnityEngine.Object @object in array)
			{
				if (!Instance.IsResource(@object) && (!includeDynamicResources || !Instance.IsDynamicResource(@object)))
				{
					continue;
				}
				long mappedInstanceID = Instance.GetMappedInstanceID(@object);
				if (mappedInstanceID != 4294967296L)
				{
					if (dictionary.ContainsKey(mappedInstanceID))
					{
						Debug.LogErrorFormat("Resource {0}  with instance id {1} already exists {2}", @object, mappedInstanceID, dictionary[mappedInstanceID]);
					}
					else
					{
						dictionary.Add(mappedInstanceID, @object);
					}
				}
			}
			return dictionary;
		}
	}
}
