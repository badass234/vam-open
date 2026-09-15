using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad.PersistentObjects;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[ExecuteInEditMode]
	public class ProjectManager : RuntimeSceneManager, IProjectManager, ISceneManager
	{
		private class LoadedAssetBundle
		{
			public AssetBundle Bundle;

			public int Usages;
		}

		private IAssetBundleLoader m_bundleLoader;

		[SerializeField]
		private FolderTemplate m_projectTemplate;

		[NonSerialized]
		private ProjectRoot m_root;

		private bool m_isProjectLoaded;

		private Dictionary<long, UnityEngine.Object> m_loadedResources;

		private Dictionary<string, LoadedAssetBundle> m_loadedBundles = new Dictionary<string, LoadedAssetBundle>();

		private Dictionary<long, UnityEngine.Object> m_dynamicResources = new Dictionary<long, UnityEngine.Object>();

		[SerializeField]
		private Transform m_dynamicResourcesRoot;

		public ProjectItem Project
		{
			get
			{
				if (m_root == null)
				{
					return null;
				}
				return m_root.Item;
			}
		}

		public event EventHandler ProjectLoading;

		public event EventHandler<ProjectManagerEventArgs> ProjectLoaded;

		public event EventHandler<ProjectManagerEventArgs> BundledResourcesAdded;

		public event EventHandler<ProjectManagerEventArgs> DynamicResourcesAdded;

		protected override void AwakeOverride()
		{
			base.AwakeOverride();
			m_bundleLoader = Dependencies.BundleLoader;
			if (m_dynamicResourcesRoot == null)
			{
				m_dynamicResourcesRoot = base.transform;
			}
		}

		protected override void OnDestroyOverride()
		{
			base.OnDestroyOverride();
			if (m_dynamicResources != null)
			{
				DestroyDynamicResources();
			}
			if (m_loadedBundles != null)
			{
				UnloadAssetBundles();
			}
			m_loadedResources = null;
		}

		private void OnApplicationQuit()
		{
			m_loadedResources = null;
			m_dynamicResources = null;
			m_loadedBundles = null;
		}

		private void UnloadAssetBundles()
		{
			foreach (KeyValuePair<string, LoadedAssetBundle> loadedBundle in m_loadedBundles)
			{
				if (loadedBundle.Value.Bundle != null)
				{
					RuntimeShaderUtil.RemoveExtra(loadedBundle.Key);
					IdentifiersMap.Instance.Unregister(loadedBundle.Value.Bundle);
					loadedBundle.Value.Bundle.Unload(true);
				}
			}
		}

		private void DestroyDynamicResources()
		{
			foreach (KeyValuePair<long, UnityEngine.Object> dynamicResource in m_dynamicResources)
			{
				long key = dynamicResource.Key;
				IdentifiersMap.Instance.Unregister(key);
				m_loadedResources.Remove(key);
				UnityEngine.Object value = dynamicResource.Value;
				if ((bool)value)
				{
					UnityEngine.Object.Destroy(value);
				}
			}
			m_dynamicResources.Clear();
		}

		private void EnumerateBundles(ProjectItem item)
		{
			if (!string.IsNullOrEmpty(item.BundleName))
			{
				LoadedAssetBundle value;
				if (!m_loadedBundles.TryGetValue(item.BundleName, out value))
				{
					LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle();
					loadedAssetBundle.Usages = 1;
					value = loadedAssetBundle;
					m_loadedBundles.Add(item.BundleName, value);
				}
				else
				{
					value.Usages++;
				}
			}
			if (item.Children != null)
			{
				for (int i = 0; i < item.Children.Count; i++)
				{
					EnumerateBundles(item.Children[i]);
				}
			}
		}

		private void LoadBundles(Action callback)
		{
			int loading = m_loadedBundles.Count;
			if (loading == 0 && callback != null)
			{
				callback();
			}
			foreach (KeyValuePair<string, LoadedAssetBundle> kvp in m_loadedBundles)
			{
				m_bundleLoader.Load(kvp.Key, delegate(string bundleName, AssetBundle bundle)
				{
					if (bundle != null)
					{
						TextAsset[] textAssets = LoadAllTextAssets(bundle);
						RuntimeShaderUtil.AddExtra(base.name, textAssets);
						IdentifiersMap.Instance.Register(bundle);
						kvp.Value.Bundle = bundle;
					}
					loading--;
					if (loading == 0)
					{
						callback();
					}
				});
			}
		}

		private static TextAsset[] LoadAllTextAssets(AssetBundle bundle)
		{
			string[] allAssetNames = bundle.GetAllAssetNames();
			List<TextAsset> list = new List<TextAsset>();
			foreach (string text in allAssetNames)
			{
				if (text.EndsWith(".txt"))
				{
					list.Add(bundle.LoadAsset<TextAsset>(text));
				}
			}
			return list.ToArray();
		}

		public bool IsResource(UnityEngine.Object obj)
		{
			return !IdentifiersMap.IsNotMapped(obj);
		}

		public ID GetID(UnityEngine.Object obj)
		{
			return new ID(obj.GetMappedInstanceID());
		}

		public void LoadProject(string projectName, ProjectManagerCallback<ProjectItem> callback)
		{
			m_isProjectLoaded = false;
			if (this.ProjectLoading != null)
			{
				this.ProjectLoading(this, EventArgs.Empty);
			}
			UnloadAssetBundles();
			DestroyDynamicResources();
			IJob job = Dependencies.Job;
			job.Submit(delegate(Action doneCallback)
			{
				bool metaOnly = false;
				int[] exceptTypes = new int[1] { 2 };
				m_project.LoadProject(projectName, delegate(ProjectPayload<ProjectRoot> loadProjectCompleted)
				{
					m_root = loadProjectCompleted.Data;
					doneCallback();
				}, metaOnly, exceptTypes);
			}, delegate
			{
				if (m_root == null)
				{
					m_root = new ProjectRoot();
					m_root.Meta = new ProjectMeta();
					m_root.Data = new ProjectData();
					m_root.Item = ProjectItem.CreateFolder(projectName);
				}
				else if (m_root.Item == null)
				{
					m_root.Item = ProjectItem.CreateFolder(projectName);
				}
				else
				{
					m_root.Item.Name = projectName;
				}
				if (m_projectTemplate != null)
				{
					ProjectItem newTemplateFolder = ProjectTemplateToProjectItem(projectName, m_projectTemplate);
					if (m_root.Item.Children == null)
					{
						m_root.Item.Children = new List<ProjectItem>();
					}
					ProjectItem item = m_root.Item;
					ContinueLoadingProject(delegate
					{
						if (callback != null)
						{
							callback(m_root.Item);
						}
						if (this.ProjectLoaded != null)
						{
							this.ProjectLoaded(this, new ProjectManagerEventArgs(m_root.Item));
						}
					}, newTemplateFolder, item);
				}
			});
		}

		private void ContinueLoadingProject(ProjectManagerCallback callback, ProjectItem newTemplateFolder, ProjectItem existingTemplateFolder)
		{
			List<ProjectItem> list = new List<ProjectItem>();
			if (existingTemplateFolder != null)
			{
				MergeData(newTemplateFolder, existingTemplateFolder);
				Diff(newTemplateFolder, existingTemplateFolder, list);
				m_root.Item = newTemplateFolder;
			}
			else
			{
				m_root.Item = newTemplateFolder;
			}
			m_project.Delete(list.ToArray(), delegate
			{
				EnumerateBundles(m_root.Item);
				LoadBundles(delegate
				{
					m_project.Save(m_root.Item, false, delegate
					{
						CompleteProjectLoading(callback);
					});
				});
			});
		}

		private void CompleteProjectLoading(ProjectManagerCallback callback)
		{
			bool includeDynamicResources = false;
			Dictionary<long, UnityEngine.Object> allResources = IdentifiersMap.FindResources(includeDynamicResources);
			bool allowNulls = false;
			m_loadedResources = new Dictionary<long, UnityEngine.Object>();
			FindDependencies(m_root.Item, m_loadedResources, allResources, allowNulls);
			FindReferencedObjects(m_root.Item, m_loadedResources, allResources, allowNulls);
			m_project.UnloadData(m_root.Item);
			m_isProjectLoaded = true;
			callback();
		}

		public void IgnoreTypes(params Type[] types)
		{
			for (int i = 0; i < types.Length; i++)
			{
				PersistentDescriptor.IgnoreTypes.Add(types[i]);
			}
		}

		public override void SaveScene(ProjectItem scene, ProjectManagerCallback callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			if (!scene.IsScene)
			{
				throw new ArgumentException("is not a scene", "scene");
			}
			if (scene.Parent == null)
			{
				throw new ArgumentException("Scene does not have parent", "scene");
			}
			if (scene.Parent.Children.Where((ProjectItem c) => c.NameExt.ToLower() == scene.NameExt).Count() > 1)
			{
				throw new ArgumentException("Scene with same name exists", "scene");
			}
			base.SaveScene(scene, callback);
		}

		public override void LoadScene(ProjectItem scene, ProjectManagerCallback callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			if (!scene.IsScene)
			{
				throw new ArgumentException("is not a scene", "scene");
			}
			DestroyDynamicResources();
			RaiseSceneLoading(scene);
			bool isEnabled = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = null;
			RuntimeUndo.Enabled = isEnabled;
			bool includeDynamicResources = false;
			Dictionary<long, UnityEngine.Object> allObjects = IdentifiersMap.FindResources(includeDynamicResources);
			Dictionary<long, UnityEngine.Object> sceneDependencies = new Dictionary<long, UnityEngine.Object>();
			bool dynamicOnly = true;
			Dictionary<long, ProjectItem> idToProjectItem = GetIdToProjectItemMapping(m_root.Item, dynamicOnly);
			m_project.LoadData(new ProjectItem[1] { scene }, delegate(ProjectPayload<ProjectItem[]> loadDataCompleted)
			{
				scene = loadDataCompleted.Data[0];
				PersistentScene persistentScene = m_serializer.Deserialize<PersistentScene>(scene.Internal_Data.RawData);
				if (persistentScene.Data != null)
				{
					for (int i = 0; i < persistentScene.Data.Length; i++)
					{
						PersistentData persistentData = persistentScene.Data[i];
						bool allowNulls = true;
						persistentData.FindDependencies(sceneDependencies, allObjects, allowNulls);
					}
				}
				List<ProjectItem> list = new List<ProjectItem>();
				foreach (KeyValuePair<long, UnityEngine.Object> item in sceneDependencies)
				{
					long key = item.Key;
					UnityEngine.Object value = item.Value;
					ProjectItem value2;
					if (IdentifiersMap.IsDynamicResourceID(key) && idToProjectItem.TryGetValue(key, out value2))
					{
						list.Add(value2);
					}
				}
				GetOrCreateObjects(list.ToArray(), idToProjectItem, delegate
				{
					CompleteSceneLoading(scene, callback, isEnabled, persistentScene);
				});
			});
		}

		public override void CreateScene()
		{
			DestroyDynamicResources();
			base.CreateScene();
		}

		public void AddBundledResources(ProjectItem folder, string bundleName, Func<UnityEngine.Object, string, bool> filter, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddBundledResources(folder, bundleName, null, null, filter, callback);
		}

		public void AddBundledResource(ProjectItem folder, string bundleName, string assetName, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddBundledResources(folder, bundleName, new string[1] { assetName }, null, (UnityEngine.Object o, string n) => true, callback);
		}

		public void AddBundledResource<T>(ProjectItem folder, string bundleName, string assetName, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddBundledResources(folder, bundleName, new string[1] { assetName }, new Type[1] { typeof(T) }, (UnityEngine.Object o, string n) => true, callback);
		}

		public void AddBundledResource(ProjectItem folder, string bundleName, string assetName, Type assetType, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddBundledResources(folder, bundleName, new string[1] { assetName }, new Type[1] { assetType }, (UnityEngine.Object o, string n) => true, callback);
		}

		public void AddBundledResources(ProjectItem folder, string bundleName, string[] assetNames, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddBundledResources(folder, bundleName, assetNames, null, (UnityEngine.Object o, string n) => true, callback);
		}

		private void LoadAssetBundle(string bundleName, AssetBundleEventHandler callback)
		{
			if (m_loadedBundles.ContainsKey(bundleName))
			{
				callback(bundleName, m_loadedBundles[bundleName].Bundle);
			}
			else
			{
				m_bundleLoader.Load(bundleName, callback);
			}
		}

		public void AddBundledResources(ProjectItem folder, string bundleName, string[] assetNames, Type[] assetTypes, Func<UnityEngine.Object, string, bool> filter, ProjectManagerCallback<ProjectItem[]> callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			if (string.IsNullOrEmpty(bundleName))
			{
				throw new ArgumentException("bandle name is not specified", "bundleName");
			}
			if (assetNames != null && assetNames.Length > 0)
			{
				if (assetNames.Length != assetNames.Distinct().Count())
				{
					throw new ArgumentException("assetNames array contains duplicates", "assetNames");
				}
				if (assetTypes == null)
				{
					assetTypes = new Type[assetNames.Length];
				}
				if (assetNames.Length != assetTypes.Length)
				{
					throw new ArgumentException("asset types array should be of same size as the asset names array", "assetTypes");
				}
			}
			LoadAssetBundle(bundleName, delegate(string name, AssetBundle bundle)
			{
				if (bundle == null)
				{
					throw new ArgumentException("unable to load bundle" + name, "bundleName");
				}
				LoadedAssetBundle value;
				if (!m_loadedBundles.TryGetValue(name, out value))
				{
					value = new LoadedAssetBundle
					{
						Bundle = bundle,
						Usages = 0
					};
					m_loadedBundles.Add(name, value);
					TextAsset[] textAssets = LoadAllTextAssets(bundle);
					RuntimeShaderUtil.AddExtra(name, textAssets);
					IdentifiersMap.Instance.Register(bundle);
				}
				if (assetNames == null)
				{
					assetNames = bundle.GetAllAssetNames();
					assetTypes = new Type[assetNames.Length];
				}
				List<UnityEngine.Object> list = new List<UnityEngine.Object>();
				for (int i = 0; i < assetNames.Length; i++)
				{
					string text = assetNames[i];
					Type type = assetTypes[i];
					UnityEngine.Object @object = ((type == null) ? bundle.LoadAsset(text) : bundle.LoadAsset(text, type));
					if (@object == null)
					{
						throw new ArgumentException("unable to load asset " + text + " " + type);
					}
					if (filter(@object, text) && AddBundledResourceInternalFilter(@object, text))
					{
						if (@object is Material)
						{
							Material material = (Material)@object;
							if (material.shader != null && !IdentifiersMap.IsNotMapped(material.shader))
							{
							}
						}
						list.Add(@object);
					}
				}
				ProjectItem[] projectItems = ConvertObjectsToProjectItems(list.ToArray(), false, bundleName, assetNames, assetTypes);
				projectItems = projectItems.OrderBy((ProjectItem p) => p.NameExt).ToArray();
				foreach (ProjectItem projectItem in projectItems)
				{
					if (folder.Children != null)
					{
						ProjectItem projectItem2 = folder.Children.Where((ProjectItem p) => p.NameExt == projectItem.NameExt).FirstOrDefault();
						if (projectItem2 != null)
						{
							folder.RemoveChild(projectItem2);
						}
						else
						{
							value.Usages++;
						}
					}
					else
					{
						value.Usages++;
					}
					folder.AddChild(projectItem);
				}
				m_project.Save(projectItems, false, delegate
				{
					bool includeDynamicResources = false;
					Dictionary<long, UnityEngine.Object> allResources = IdentifiersMap.FindResources(includeDynamicResources);
					bool allowNulls = false;
					foreach (ProjectItem item in projectItems)
					{
						FindDependencies(item, m_loadedResources, allResources, allowNulls);
						FindReferencedObjects(item, m_loadedResources, allResources, allowNulls);
					}
					foreach (ProjectItem item2 in projectItems)
					{
						m_project.UnloadData(item2);
					}
					if (callback != null)
					{
						callback(projectItems);
					}
					if (this.BundledResourcesAdded != null)
					{
						this.BundledResourcesAdded(this, new ProjectManagerEventArgs(projectItems));
					}
				});
			});
		}

		private bool AddBundledResourceInternalFilter(UnityEngine.Object obj, string assetName)
		{
			if (obj == null)
			{
				return false;
			}
			if (assetName.Contains("resourcemap"))
			{
				return false;
			}
			if (assetName.StartsWith("rt_shader"))
			{
				return false;
			}
			if (obj is TextAsset)
			{
				return false;
			}
			if (obj is Shader)
			{
				if (!obj.HasMappedInstanceID())
				{
					Debug.LogWarningFormat("Shader {0} can't be added as bundled resource. Please consider adding it to main ResourceMap or bundle ResourceMap", obj.name);
				}
				return false;
			}
			if (!(obj is GameObject) && !(obj is Mesh) && !(obj is Material) && !(obj is Texture))
			{
				return false;
			}
			return PersistentData.CanCreate(obj);
		}

		public void AddDynamicResource(ProjectItem folder, UnityEngine.Object obj, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddDynamicResources(folder, new UnityEngine.Object[1] { obj }, false, (UnityEngine.Object o) => true, callback);
		}

		public void AddDynamicResources(ProjectItem folder, UnityEngine.Object[] objects, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddDynamicResources(folder, objects, false, (UnityEngine.Object o) => true, callback);
		}

		public void AddDynamicResource(ProjectItem folder, UnityEngine.Object obj, bool includingDependencies, Func<UnityEngine.Object, bool> filter, ProjectManagerCallback<ProjectItem[]> callback)
		{
			AddDynamicResources(folder, new UnityEngine.Object[1] { obj }, includingDependencies, filter, callback);
		}

		public void AddDynamicResources(ProjectItem folder, UnityEngine.Object[] objects, bool includingDependencies, Func<UnityEngine.Object, bool> filter, ProjectManagerCallback<ProjectItem[]> callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			if (objects == null || objects.Length == 0)
			{
				throw new ArgumentNullException("objects array is null or empty", "objects");
			}
			if (objects.Distinct().Count() != objects.Length)
			{
				throw new ArgumentException("same object included to objects array multiple times", "objects");
			}
			Dictionary<long, UnityEngine.Object> dictionary = new Dictionary<long, UnityEngine.Object>();
			if (includingDependencies)
			{
				HashSet<UnityEngine.Object> hashSet = new HashSet<UnityEngine.Object>();
				while (objects.Length > 0)
				{
					for (int i = 0; i < objects.Length; i++)
					{
						UnityEngine.Object @object = objects[i];
						if (AddDynamicResourceInternalFilter(@object) && filter(@object))
						{
							if (!hashSet.Contains(@object))
							{
								hashSet.Add(@object);
							}
						}
						else
						{
							objects[i] = null;
						}
					}
					Dictionary<long, UnityEngine.Object> dictionary2 = new Dictionary<long, UnityEngine.Object>();
					foreach (UnityEngine.Object object2 in objects)
					{
						if (object2 != null)
						{
							GetDependencies(object2, dictionary2);
						}
					}
					objects = dictionary2.Values.ToArray();
					foreach (KeyValuePair<long, UnityEngine.Object> item4 in dictionary2)
					{
						if (!dictionary.ContainsKey(item4.Key))
						{
							dictionary.Add(item4.Key, item4.Value);
						}
					}
				}
				objects = hashSet.ToArray();
			}
			else
			{
				for (int k = 0; k < objects.Length; k++)
				{
					UnityEngine.Object object3 = objects[k];
					if (AddDynamicResourceInternalFilter(object3) && filter(object3))
					{
						GetDependencies(object3, dictionary);
					}
					else
					{
						objects[k] = null;
					}
				}
				objects = objects.Where((UnityEngine.Object o) => o != null).ToArray();
			}
			foreach (UnityEngine.Object object4 in objects)
			{
				if (object4 is Component)
				{
					Component component = (Component)object4;
					long mappedInstanceID = component.gameObject.GetMappedInstanceID();
					if (!dictionary.ContainsKey(mappedInstanceID))
					{
						dictionary.Add(mappedInstanceID, component.gameObject);
					}
				}
			}
			Dictionary<long, UnityEngine.Object> dictionary3 = new Dictionary<long, UnityEngine.Object>();
			DuplicateAndRegister(objects, dictionary3);
			for (int m = 0; m < objects.Length; m++)
			{
				UnityEngine.Object object5 = objects[m];
				if (object5 is Component)
				{
					Component component2 = (Component)object5;
					objects[m] = component2.gameObject;
				}
			}
			foreach (UnityEngine.Object object6 in objects)
			{
				object6.name = ProjectItem.GetUniqueName(object6.name, object6, folder);
			}
			ProjectItem[] projectItems = ConvertObjectsToProjectItems(objects, false);
			foreach (UnityEngine.Object obj in objects)
			{
				GetReferredObjects(obj, dictionary);
			}
			foreach (KeyValuePair<long, UnityEngine.Object> item5 in dictionary3)
			{
				if (dictionary.ContainsKey(item5.Key))
				{
					dictionary[item5.Key] = item5.Value;
				}
			}
			foreach (ProjectItem projectItem in projectItems)
			{
				PersistentData.RestoreDataAndResolveDependencies(projectItem.Internal_Data.PersistentData, dictionary);
			}
			ProjectItem[] array = ConvertObjectsToProjectItems(objects, false);
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				array[num3].Internal_Data.RawData = projectItems[num3].Internal_Data.RawData;
			}
			projectItems = array;
			projectItems = projectItems.OrderBy((ProjectItem p) => p.NameExt).ToArray();
			foreach (ProjectItem item in projectItems)
			{
				folder.AddChild(item);
			}
			m_project.Save(projectItems, false, delegate
			{
				bool includeDynamicResources = false;
				Dictionary<long, UnityEngine.Object> allResources = IdentifiersMap.FindResources(includeDynamicResources);
				bool allowNulls = false;
				foreach (ProjectItem item2 in projectItems)
				{
					FindDependencies(item2, m_loadedResources, allResources, allowNulls);
					FindReferencedObjects(item2, m_loadedResources, allResources, allowNulls);
				}
				foreach (ProjectItem item3 in projectItems)
				{
					m_project.UnloadData(item3);
				}
				m_project.SaveProjectMeta(Project.Name, m_root.Meta, delegate
				{
					if (callback != null)
					{
						callback(projectItems);
					}
					if (this.DynamicResourcesAdded != null)
					{
						this.DynamicResourcesAdded(this, new ProjectManagerEventArgs(projectItems));
					}
				});
			});
		}

		private bool AddDynamicResourceInternalFilter(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is Texture2D)
			{
				Texture2D texture2D = (Texture2D)obj;
				bool flag = texture2D.IsReadable();
				if (!flag)
				{
					Debug.LogWarningFormat("Texture {0} can't be added as dynamic resource. Please consider adding it to main ResourceMap or bundle ResourceMap", texture2D.name);
				}
				return flag;
			}
			if (obj is Shader)
			{
				if (!obj.HasMappedInstanceID())
				{
					Debug.LogWarningFormat("Shader {0} can't be added as dynamic resource. Please consider adding it to main ResourceMap or bundle ResourceMap", obj.name);
				}
				return false;
			}
			return PersistentData.CanCreate(obj);
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

		private void GetReferredObjects(UnityEngine.Object obj, Dictionary<long, UnityEngine.Object> objects)
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
				GetReferredObjects(item.gameObject, objects);
			}
		}

		private void DuplicateAndRegister(UnityEngine.Object[] objects, Dictionary<long, UnityEngine.Object> objIdToDuplicate)
		{
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				bool active = false;
				bool flag = @object is Component;
				bool flag2 = @object is GameObject;
				GameObject gameObject = null;
				if (flag)
				{
					Component component = (Component)@object;
					gameObject = component.gameObject;
				}
				else if (flag2)
				{
					gameObject = (GameObject)@object;
				}
				if (gameObject != null)
				{
					active = gameObject.activeSelf;
					gameObject.SetActive(false);
				}
				UnityEngine.Object object2 = UnityEngine.Object.Instantiate(objects[i]);
				object2.name = @object.name;
				objects[i] = object2;
				GameObject gameObject2 = null;
				if (flag)
				{
					Component component2 = (Component)object2;
					gameObject2 = component2.gameObject;
					if (flag)
					{
						objIdToDuplicate.Add(gameObject.GetMappedInstanceID(), gameObject2);
					}
				}
				else if (flag2)
				{
					gameObject2 = (GameObject)object2;
				}
				objIdToDuplicate.Add(@object.GetMappedInstanceID(), object2);
				if (gameObject != null)
				{
					gameObject.SetActive(active);
					RegisterDynamicResource(gameObject2, delegate
					{
						if (m_root.Meta.Counter < 0)
						{
							throw new InvalidOperationException("identifiers exhausted");
						}
						m_root.Meta.Counter++;
						return m_root.Meta.Counter;
					});
					gameObject2.transform.SetParent(m_dynamicResourcesRoot, true);
					gameObject2.hideFlags = HideFlags.HideAndDontSave;
					gameObject2.SetActive(active);
					Component[] componentsInChildren = gameObject2.GetComponentsInChildren<Component>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						if (componentsInChildren[j] != null)
						{
							componentsInChildren[j].gameObject.hideFlags = HideFlags.HideAndDontSave;
							componentsInChildren[j].hideFlags = HideFlags.HideAndDontSave;
						}
					}
				}
				else if (IdentifiersMap.IsNotMapped(object2))
				{
					if (m_root.Meta.Counter < 0)
					{
						throw new InvalidOperationException("identifiers exhausted");
					}
					m_root.Meta.Counter++;
					IdentifiersMap.Instance.Register(object2, m_root.Meta.Counter);
					long mappedInstanceID = object2.GetMappedInstanceID();
					if (!m_loadedResources.ContainsKey(mappedInstanceID))
					{
						m_loadedResources.Add(mappedInstanceID, object2);
					}
				}
			}
		}

		public void CreateFolder(string name, ProjectItem parent, ProjectManagerCallback<ProjectItem> callback)
		{
			ProjectItem folder = ProjectItem.CreateFolder(name);
			parent.AddChild(folder);
			folder.Name = ProjectItem.GetUniqueName(name, folder, parent);
			m_project.Save(folder, true, delegate
			{
				if (callback != null)
				{
					callback(folder);
				}
			});
		}

		public void SaveObjects(ProjectItemObjectPair[] itemObjectPairs, ProjectManagerCallback callback)
		{
			if (itemObjectPairs == null || itemObjectPairs.Length == 0)
			{
				callback();
				return;
			}
			SaveObjectsToProjectItems(itemObjectPairs);
			ProjectItem[] projectItems = itemObjectPairs.Select((ProjectItemObjectPair i) => i.ProjectItem).ToArray();
			m_project.Save(projectItems, false, delegate
			{
				foreach (ProjectItem item in projectItems)
				{
					m_project.UnloadData(item);
				}
				if (callback != null)
				{
					callback();
				}
			});
		}

		public void GetOrCreateObjects(ProjectItem folder, ProjectManagerCallback<ProjectItemObjectPair[]> callback)
		{
			if (folder == null || folder.Children == null || folder.Children.Count == 0)
			{
				callback(new ProjectItemObjectPair[0]);
				return;
			}
			ProjectItem[] projectItems = folder.Children.ToArray();
			GetOrCreateObjectsFromProjectItems(projectItems, callback);
		}

		public void GetOrCreateObjects(ProjectItem[] projectItems, ProjectManagerCallback<ProjectItemObjectPair[]> callback)
		{
			if (projectItems == null || projectItems.Length == 0)
			{
				callback(new ProjectItemObjectPair[0]);
				return;
			}
			ProjectItem[] projectItems2 = projectItems.Where((ProjectItem item) => item.Children != null && item.Children.Count > 0).SelectMany((ProjectItem item) => item.Children).Union(projectItems.Where((ProjectItem item) => !item.IsFolder))
				.ToArray();
			GetOrCreateObjectsFromProjectItems(projectItems2, callback);
		}

		private void GetOrCreateObjectsFromProjectItems(ProjectItem[] projectItems, ProjectManagerCallback<ProjectItemObjectPair[]> callback)
		{
			ProjectItem[] array = (from p in projectItems
				where p.IsScene
				orderby p.Name
				select p).ToArray();
			ProjectItem[] array2 = (from p in projectItems
				where p.IsFolder
				orderby p.Name
				select p).ToArray();
			List<ProjectItemObjectPair> result = new List<ProjectItemObjectPair>();
			for (int i = 0; i < array2.Length; i++)
			{
				ProjectItemWrapper projectItemWrapper = ScriptableObject.CreateInstance<ProjectItemWrapper>();
				projectItemWrapper.ProjectItem = array2[i];
				result.Add(new ProjectItemObjectPair(projectItemWrapper.ProjectItem, projectItemWrapper));
			}
			for (int j = 0; j < array.Length; j++)
			{
				ProjectItemWrapper projectItemWrapper2 = ScriptableObject.CreateInstance<ProjectItemWrapper>();
				projectItemWrapper2.ProjectItem = array[j];
				result.Add(new ProjectItemObjectPair(projectItemWrapper2.ProjectItem, projectItemWrapper2));
			}
			projectItems = projectItems.Where((ProjectItem p) => !p.IsFolder && !p.IsScene).ToArray();
			bool dynamicOnly = true;
			Dictionary<long, ProjectItem> idToProjectItemMapping = GetIdToProjectItemMapping(m_root.Item, dynamicOnly);
			GetOrCreateObjects(projectItems, idToProjectItemMapping, delegate
			{
				projectItems = projectItems.OrderBy((ProjectItem item) => item.Name).ToArray();
				foreach (ProjectItem projectItem in projectItems)
				{
					UnityEngine.Object value;
					if (m_loadedResources.TryGetValue(projectItem.Internal_Meta.Descriptor.InstanceId, out value))
					{
						result.Add(new ProjectItemObjectPair(projectItem, value));
					}
				}
				if (callback != null)
				{
					callback(result.ToArray());
				}
			});
		}

		private void GetOrCreateObjects(ProjectItem[] projectItems, Dictionary<long, ProjectItem> idToProjectItem, ProjectManagerCallback callback)
		{
			if (projectItems == null || projectItems.Length == 0)
			{
				if (callback != null)
				{
					callback();
				}
				return;
			}
			Dictionary<long, ProjectItem> loadedProjectItemsDictionary = new Dictionary<long, ProjectItem>();
			LoadProjectItemsAndDependencies(projectItems, idToProjectItem, loadedProjectItemsDictionary, delegate
			{
				ProjectItem[] array = loadedProjectItemsDictionary.Values.ToArray();
				List<GameObject> list = new List<GameObject>();
				foreach (ProjectItem projectItem in array)
				{
					Dictionary<long, UnityEngine.Object> decomposition = null;
					if (projectItem.IsGameObject)
					{
						decomposition = new Dictionary<long, UnityEngine.Object>();
					}
					UnityEngine.Object orCreateObject = GetOrCreateObject(projectItem, m_loadedResources, decomposition);
					RegisterDynamicResource(InstanceID(projectItem), orCreateObject, decomposition);
					if (orCreateObject is GameObject && IsDynamicResource(projectItem))
					{
						list.Add((GameObject)orCreateObject);
					}
				}
				foreach (ProjectItem projectItem2 in array)
				{
					Dictionary<long, UnityEngine.Object> dictionary = new Dictionary<long, UnityEngine.Object>();
					FindDependencies(projectItem2, dictionary, m_loadedResources, false);
					FindReferencedObjects(projectItem2, dictionary, m_loadedResources, false);
					RestoreDataAndResolveDependencies(projectItem2, dictionary);
				}
				for (int k = 0; k < list.Count; k++)
				{
					GameObject gameObject = list[k];
					gameObject.transform.SetParent(m_dynamicResourcesRoot, true);
					gameObject.hideFlags = HideFlags.HideAndDontSave;
				}
				foreach (ProjectItem item in array)
				{
					m_project.UnloadData(item);
				}
				if (callback != null)
				{
					callback();
				}
			});
		}

		public void Duplicate(ProjectItem[] projectItems, ProjectManagerCallback<ProjectItem[]> callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			projectItems = ProjectItem.GetRootItems(projectItems);
			List<ProjectItem> allOriginalProjectItems = new List<ProjectItem>();
			for (int i = 0; i < projectItems.Length; i++)
			{
				allOriginalProjectItems.AddRange(projectItems[i].FlattenHierarchy(true));
			}
			m_project.LoadData(allOriginalProjectItems.ToArray(), delegate(ProjectPayload<ProjectItem[]> loadDataCompleted)
			{
				ProjectItem[] array = new ProjectItem[loadDataCompleted.Data.Length];
				for (int j = 0; j < loadDataCompleted.Data.Length; j++)
				{
					array[j] = loadDataCompleted.Data[j].Parent;
					loadDataCompleted.Data[j].Parent = null;
				}
				projectItems = m_serializer.DeepClone(loadDataCompleted.Data);
				for (int k = 0; k < projectItems.Length; k++)
				{
					loadDataCompleted.Data[k].Parent = array[k];
					projectItems[k].Parent = array[k];
				}
				for (int l = 0; l < allOriginalProjectItems.Count; l++)
				{
					m_project.UnloadData(allOriginalProjectItems[l]);
				}
				ProjectItem[] rootItems = ProjectItem.GetRootItems(projectItems);
				for (int m = 0; m < rootItems.Length; m++)
				{
					ProjectItem projectItem = rootItems[m];
					projectItem.IsExposedFromEditor = false;
					string uniqueName = ProjectItem.GetUniqueName(projectItem.Name, rootItems[m], rootItems[m].Parent, false);
					if (uniqueName != projectItem.NameExt)
					{
						projectItem.NameExt = uniqueName;
						if (!projectItem.IsFolder && !projectItem.IsScene)
						{
							projectItem.Rename(projectItem.Name);
						}
					}
				}
				foreach (ProjectItem projectItem2 in projectItems)
				{
					if (!projectItem2.IsFolder && !projectItem2.IsScene)
					{
						LoadedAssetBundle value;
						if (!string.IsNullOrEmpty(projectItem2.BundleName) && m_loadedBundles.TryGetValue(projectItem2.BundleName, out value))
						{
							value.Usages++;
						}
						PersistentDescriptor[] array2 = projectItem2.Internal_Meta.Descriptor.FlattenHierarchy();
						if (projectItem2.Internal_Data.PersistentData != null)
						{
							Dictionary<long, PersistentData> dictionary = projectItem2.Internal_Data.PersistentData.ToDictionary((PersistentData item) => item.InstanceId);
							HashSet<PersistentTransform> hashSet = new HashSet<PersistentTransform>();
							for (int num = 0; num < array2.Length; num++)
							{
								m_root.Meta.Counter++;
								PersistentDescriptor persistentDescriptor = array2[num];
								PersistentData value2;
								if (dictionary.TryGetValue(persistentDescriptor.InstanceId, out value2))
								{
									value2.InstanceId = IdentifiersMap.ToDynamicResourceID(m_root.Meta.Counter);
									if (value2 is PersistentTransform)
									{
										PersistentTransform item2 = (PersistentTransform)value2;
										if (!hashSet.Contains(item2))
										{
											hashSet.Add(item2);
										}
									}
								}
								persistentDescriptor.InstanceId = IdentifiersMap.ToDynamicResourceID(m_root.Meta.Counter);
							}
							foreach (PersistentTransform item4 in hashSet)
							{
								PersistentData value3;
								if (dictionary.TryGetValue(item4.parent, out value3))
								{
									item4.parent = value3.InstanceId;
								}
							}
						}
						else
						{
							m_root.Meta.Counter++;
							array2[0].InstanceId = IdentifiersMap.ToDynamicResourceID(m_root.Meta.Counter);
						}
					}
				}
				m_project.Save(projectItems.ToArray(), false, delegate
				{
					foreach (ProjectItem item3 in projectItems)
					{
						m_project.UnloadData(item3);
					}
					m_project.SaveProjectMeta(Project.Name, m_root.Meta, delegate
					{
						if (callback != null)
						{
							callback(projectItems);
						}
					});
				});
			});
		}

		public void Rename(ProjectItem projectItem, string newName, ProjectManagerCallback callback)
		{
			m_project.Rename(projectItem, newName, delegate
			{
				if (callback != null)
				{
					callback();
				}
			});
		}

		public void Move(ProjectItem[] projectItems, ProjectItem folder, ProjectManagerCallback callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			projectItems = ProjectItem.GetRootItems(projectItems);
			projectItems = projectItems.Where((ProjectItem item) => folder.Children == null || folder.Children.Contains(item) || !folder.Children.Any((ProjectItem c) => c.NameExt == item.NameExt)).ToArray();
			if (projectItems.Length == 0)
			{
				throw new InvalidOperationException("Can't move items");
			}
			m_project.Move(projectItems, folder, delegate
			{
				if (callback != null)
				{
					callback();
				}
			});
		}

		public void Delete(ProjectItem[] projectItems, ProjectManagerCallback callback)
		{
			if (!m_isProjectLoaded)
			{
				throw new InvalidOperationException("project is not loaded");
			}
			if (projectItems.Any((ProjectItem pi) => pi.IsResource && !IsDynamicResource(pi) && string.IsNullOrEmpty(pi.BundleName)))
			{
				throw new ArgumentException("Unable to remove non-dynamic and non-bundled projectItems", "projectItems");
			}
			Dictionary<long, ProjectItem> dictionary = new Dictionary<long, ProjectItem>();
			bool flag = false;
			foreach (ProjectItem projectItem in projectItems)
			{
				bool dynamicOnly = false;
				GetIdToProjectItemMapping(projectItem, dictionary, dynamicOnly);
				if (string.IsNullOrEmpty(projectItem.BundleName))
				{
					continue;
				}
				LoadedAssetBundle loadedAssetBundle = m_loadedBundles[projectItem.BundleName];
				loadedAssetBundle.Usages--;
				if (loadedAssetBundle.Usages <= 0)
				{
					m_loadedBundles.Remove(projectItem.BundleName);
					if (loadedAssetBundle.Bundle != null)
					{
						bool unloadAllLoadedObjects = true;
						RuntimeShaderUtil.RemoveExtra(projectItem.BundleName);
						IdentifiersMap.Instance.Unregister(loadedAssetBundle.Bundle);
						loadedAssetBundle.Bundle.Unload(unloadAllLoadedObjects);
						flag = true;
					}
				}
			}
			if (flag)
			{
				List<long> list = new List<long>();
				foreach (KeyValuePair<long, UnityEngine.Object> loadedResource in m_loadedResources)
				{
					if (loadedResource.Value == null)
					{
						list.Add(loadedResource.Key);
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					m_loadedResources.Remove(list[j]);
				}
			}
			foreach (long key in dictionary.Keys)
			{
				if (!IdentifiersMap.IsDynamicResourceID(key))
				{
					continue;
				}
				UnityEngine.Object value;
				if (m_loadedResources.TryGetValue(key, out value))
				{
					if (!(value is Component))
					{
						UnityEngine.Object.Destroy(value);
					}
					m_loadedResources.Remove(key);
				}
				m_dynamicResources.Remove(key);
			}
			m_project.Delete(projectItems, delegate
			{
				foreach (ProjectItem projectItem2 in projectItems)
				{
					m_project.UnloadData(projectItem2);
					if (projectItem2.Parent != null)
					{
						projectItem2.Parent.RemoveChild(projectItem2);
					}
				}
				if (callback != null)
				{
					callback();
				}
			});
		}

		private void LoadProjectItemsAndDependencies(ProjectItem[] projectItems, Dictionary<long, ProjectItem> idToProjectItem, Dictionary<long, ProjectItem> processedProjectItems, ProjectManagerCallback callback)
		{
			int[] exceptTypes = new int[1] { 2 };
			List<ProjectItem> loadProjectItems = new List<ProjectItem>();
			foreach (ProjectItem projectItem in projectItems)
			{
				if (!processedProjectItems.ContainsKey(InstanceID(projectItem)))
				{
					loadProjectItems.Add(projectItem);
				}
			}
			if (loadProjectItems.Count == 0)
			{
				if (callback != null)
				{
					callback();
				}
				return;
			}
			IJob job = Dependencies.Job;
			ProjectItem[] loadedProjectItems = new ProjectItem[0];
			job.Submit(delegate(Action doneCallback)
			{
				m_project.LoadData(loadProjectItems.ToArray(), delegate(ProjectPayload<ProjectItem[]> loadProjectItemsCompleted)
				{
					loadedProjectItems = loadProjectItemsCompleted.Data;
					doneCallback();
				}, exceptTypes);
			}, delegate
			{
				Dictionary<long, ProjectItem> dictionary = new Dictionary<long, ProjectItem>();
				foreach (ProjectItem projectItem2 in loadedProjectItems)
				{
					if (!processedProjectItems.ContainsKey(InstanceID(projectItem2)))
					{
						processedProjectItems.Add(InstanceID(projectItem2), projectItem2);
						FindDependencies(projectItem2, dictionary, idToProjectItem);
					}
				}
				if (dictionary.Count > 0)
				{
					LoadProjectItemsAndDependencies(dictionary.Values.ToArray(), idToProjectItem, processedProjectItems, callback);
				}
				else if (callback != null)
				{
					callback();
				}
			});
		}

		private void RegisterDynamicResource(long mappedInstanceId, UnityEngine.Object dynamicResource, Dictionary<long, UnityEngine.Object> decomposition)
		{
			if (!IdentifiersMap.IsDynamicResourceID(mappedInstanceId))
			{
				return;
			}
			IdentifiersMap.Instance.Register(dynamicResource, mappedInstanceId);
			if (!m_dynamicResources.ContainsKey(mappedInstanceId))
			{
				m_dynamicResources.Add(mappedInstanceId, dynamicResource);
				if (dynamicResource is GameObject)
				{
					GameObject gameObject = (GameObject)dynamicResource;
					gameObject.transform.SetParent(m_dynamicResourcesRoot, true);
					gameObject.hideFlags = HideFlags.HideAndDontSave;
				}
			}
			if (decomposition == null)
			{
				return;
			}
			foreach (KeyValuePair<long, UnityEngine.Object> item in decomposition)
			{
				long key = item.Key;
				UnityEngine.Object value = item.Value;
				IdentifiersMap.Instance.Register(value, key);
				if (!m_dynamicResources.ContainsKey(key))
				{
					m_dynamicResources.Add(key, value);
				}
			}
		}

		private void RegisterDynamicResource(GameObject obj, Func<int> id)
		{
			IdentifiersMap.Instance.Register(obj, id());
			long mappedInstanceID = obj.GetMappedInstanceID();
			if (!m_loadedResources.ContainsKey(mappedInstanceID))
			{
				m_loadedResources.Add(mappedInstanceID, obj);
			}
			Component[] components = obj.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component != null)
				{
					IdentifiersMap.Instance.Register(component, id());
					mappedInstanceID = component.GetMappedInstanceID();
					if (!m_loadedResources.ContainsKey(mappedInstanceID))
					{
						m_loadedResources.Add(mappedInstanceID, component);
					}
				}
			}
			foreach (Transform item in obj.transform)
			{
				RegisterDynamicResource(item.gameObject, id);
			}
		}

		private ProjectItem ProjectTemplateToProjectItem(string projectName, FolderTemplate projectTemplate)
		{
			ProjectItem projectItem = ProjectItem.CreateFolder(projectName);
			ProjectTemplateToProjectItem(projectTemplate, projectItem);
			projectItem.Name = projectName;
			return projectItem;
		}

		private static void ProjectTemplateToProjectItem(FolderTemplate template, ProjectItem folder)
		{
			folder.IsExposedFromEditor = true;
			folder.Name = template.name;
			foreach (Transform item in template.transform)
			{
				FolderTemplate component = item.GetComponent<FolderTemplate>();
				if (component != null)
				{
					ProjectItem projectItem = ProjectItem.CreateFolder(component.name);
					folder.AddChild(projectItem);
					ProjectTemplateToProjectItem(component, projectItem);
				}
			}
			UnityEngine.Object[] objects = template.Objects.Where((UnityEngine.Object obj) => obj != null).ToArray();
			ProjectItem[] array = ConvertObjectsToProjectItems(objects, true);
			for (int i = 0; i < array.Length; i++)
			{
				folder.AddChild(array[i]);
			}
		}

		private static ProjectItem[] ConvertObjectsToProjectItems(UnityEngine.Object[] objects, bool isExposedFromEditor, string bundleName = null, string[] assetNames = null, Type[] assetTypes = null)
		{
			if (objects == null)
			{
				return null;
			}
			if (objects.Length == 0)
			{
				return new ProjectItem[0];
			}
			List<ProjectItem> list = new List<ProjectItem>();
			List<ProjectItem> list2 = new List<ProjectItem>();
			List<ProjectItem> list3 = new List<ProjectItem>();
			List<GameObject> list4 = new List<GameObject>();
			List<UnityEngine.Object> list5 = new List<UnityEngine.Object>();
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				ProjectItem projectItem = new ProjectItem();
				if (@object is GameObject)
				{
					list4.Add((GameObject)@object);
					list2.Add(projectItem);
				}
				else
				{
					list5.Add(@object);
					list3.Add(projectItem);
				}
				projectItem.Internal_Meta = new ProjectItemMeta();
				if (!string.IsNullOrEmpty(bundleName))
				{
					projectItem.Internal_Meta.BundleDescriptor = new AssetBundleDescriptor
					{
						AssetName = ((assetNames.Length <= i) ? null : assetNames[i]),
						TypeName = ((assetNames.Length <= i || assetTypes[i] == null) ? null : assetTypes[i].AssemblyQualifiedName),
						BundleName = bundleName
					};
				}
				list.Add(projectItem);
			}
			if (list4.Count > 0)
			{
				PersistentDescriptor[] descriptors;
				PersistentData[][] data;
				PersistentData.CreatePersistentDescriptorsAndData(list4.ToArray(), out descriptors, out data);
				for (int j = 0; j < descriptors.Length; j++)
				{
					ProjectItem projectItem2 = list2[j];
					projectItem2.Internal_Meta.Descriptor = descriptors[j];
					projectItem2.Internal_Meta.Name = list4[j].name;
					projectItem2.Internal_Meta.TypeCode = ProjectItemTypes.GetProjectItemType(typeof(GameObject));
					projectItem2.Internal_Meta.IsExposedFromEditor = isExposedFromEditor;
					ProjectItemData projectItemData = new ProjectItemData();
					projectItemData.PersistentData = data[j];
					ProjectItemData internal_Data = projectItemData;
					projectItem2.Internal_Data = internal_Data;
				}
			}
			if (list5.Count > 0)
			{
				UnityEngine.Object[] array = list5.ToArray();
				PersistentDescriptor[] array2 = PersistentDescriptor.CreatePersistentDescriptors(array);
				PersistentData[] array3 = PersistentData.CreatePersistentData(array);
				for (int k = 0; k < array2.Length; k++)
				{
					UnityEngine.Object object2 = array[k];
					ProjectItem projectItem3 = list3[k];
					projectItem3.Internal_Meta.Descriptor = array2[k];
					projectItem3.Internal_Meta.Name = object2.name;
					projectItem3.Internal_Meta.TypeCode = ProjectItemTypes.GetProjectItemType(object2.GetType());
					projectItem3.Internal_Meta.IsExposedFromEditor = isExposedFromEditor;
					ProjectItemData internal_Data2 = CreateProjectItemData(array3[k], object2);
					projectItem3.Internal_Data = internal_Data2;
					TryReadRawData(projectItem3, object2);
				}
			}
			return list.ToArray();
		}

		private static ProjectItemData CreateProjectItemData(PersistentData oData, UnityEngine.Object obj)
		{
			ProjectItemData projectItemData = new ProjectItemData();
			projectItemData.PersistentData = new PersistentData[1] { oData };
			return projectItemData;
		}

		private static void SaveObjectsToProjectItems(ProjectItemObjectPair[] itemToObjectPairs)
		{
			if (itemToObjectPairs == null || itemToObjectPairs.Length == 0)
			{
				return;
			}
			List<ProjectItemObjectPair> list = new List<ProjectItemObjectPair>();
			List<ProjectItemObjectPair> list2 = new List<ProjectItemObjectPair>();
			foreach (ProjectItemObjectPair projectItemObjectPair in itemToObjectPairs)
			{
				if (projectItemObjectPair.Object is GameObject)
				{
					list.Add(projectItemObjectPair);
				}
				else
				{
					list2.Add(projectItemObjectPair);
				}
			}
			if (list.Count > 0)
			{
				PersistentDescriptor[] descriptors;
				PersistentData[][] data;
				PersistentData.CreatePersistentDescriptorsAndData(list.Select((ProjectItemObjectPair o) => (GameObject)o.Object).ToArray(), out descriptors, out data);
				for (int j = 0; j < descriptors.Length; j++)
				{
					ProjectItemMeta projectItemMeta = new ProjectItemMeta();
					projectItemMeta.Descriptor = descriptors[j];
					projectItemMeta.Name = list[j].Object.name;
					projectItemMeta.TypeCode = ProjectItemTypes.GetProjectItemType(typeof(GameObject));
					projectItemMeta.IsExposedFromEditor = list[j].ProjectItem.IsExposedFromEditor;
					projectItemMeta.BundleDescriptor = list[j].ProjectItem.Internal_Meta.BundleDescriptor;
					ProjectItemMeta internal_Meta = projectItemMeta;
					ProjectItemData projectItemData = new ProjectItemData();
					projectItemData.PersistentData = data[j];
					ProjectItemData internal_Data = projectItemData;
					ProjectItem projectItem = list[j].ProjectItem;
					projectItem.Internal_Meta = internal_Meta;
					projectItem.Internal_Data = internal_Data;
				}
			}
			if (list2.Count > 0)
			{
				UnityEngine.Object[] array = list2.Select((ProjectItemObjectPair o) => o.Object).ToArray();
				PersistentDescriptor[] array2 = PersistentDescriptor.CreatePersistentDescriptors(array);
				PersistentData[] array3 = PersistentData.CreatePersistentData(array);
				for (int k = 0; k < array2.Length; k++)
				{
					ProjectItemMeta projectItemMeta = new ProjectItemMeta();
					projectItemMeta.Descriptor = array2[k];
					projectItemMeta.Name = array[k].name;
					projectItemMeta.TypeCode = ProjectItemTypes.GetProjectItemType(array[k].GetType());
					projectItemMeta.IsExposedFromEditor = list2[k].ProjectItem.IsExposedFromEditor;
					projectItemMeta.BundleDescriptor = list2[k].ProjectItem.Internal_Meta.BundleDescriptor;
					ProjectItemMeta internal_Meta2 = projectItemMeta;
					ProjectItemData projectItemData = new ProjectItemData();
					projectItemData.PersistentData = new PersistentData[1] { array3[k] };
					ProjectItemData internal_Data2 = projectItemData;
					ProjectItem projectItem2 = list2[k].ProjectItem;
					projectItem2.Internal_Meta = internal_Meta2;
					projectItem2.Internal_Data = internal_Data2;
					TryReadRawData(projectItem2, array[k]);
				}
			}
		}

		private static long InstanceID(ProjectItem projectItem)
		{
			return projectItem.Internal_Meta.Descriptor.InstanceId;
		}

		private static bool IsDynamicResource(ProjectItem projectItem)
		{
			if (projectItem.Internal_Meta.Descriptor == null)
			{
				return false;
			}
			return IdentifiersMap.IsDynamicResourceID(projectItem.Internal_Meta.Descriptor.InstanceId);
		}

		private static UnityEngine.Object GetOrCreateObject(ProjectItem projectItem, Dictionary<long, UnityEngine.Object> allResources, Dictionary<long, UnityEngine.Object> decomposition = null)
		{
			if (projectItem.IsFolder)
			{
				throw new InvalidOperationException("Operation is invalid for Folder");
			}
			return PersistentDescriptor.GetOrCreateObject(projectItem.Internal_Meta.Descriptor, allResources, decomposition);
		}

		private static void RestoreDataAndResolveDependencies(ProjectItem projectItem, Dictionary<long, UnityEngine.Object> objects)
		{
			if (projectItem.Internal_Data != null && projectItem.Internal_Data.PersistentData != null)
			{
				PersistentData.RestoreDataAndResolveDependencies(projectItem.Internal_Data.PersistentData, objects);
			}
			TryLoadRawData(projectItem, objects);
		}

		private static void FindDependencies(ProjectItem item, Dictionary<long, ProjectItem> dependencies, Dictionary<long, ProjectItem> identifiersMapping)
		{
			bool allowNulls = false;
			if (item.Internal_Data != null)
			{
				PersistentData[] persistentData = item.Internal_Data.PersistentData;
				for (int i = 0; i < persistentData.Length; i++)
				{
					persistentData[i].FindDependencies(dependencies, identifiersMapping, allowNulls);
				}
			}
			if (item.Children != null)
			{
				for (int j = 0; j < item.Children.Count; j++)
				{
					ProjectItem item2 = item.Children[j];
					FindDependencies(item2, dependencies, identifiersMapping);
				}
			}
		}

		private static void FindDependencies(ProjectItem item, Dictionary<long, UnityEngine.Object> dependencies, Dictionary<long, UnityEngine.Object> allResources, bool allowNulls)
		{
			if (item.Internal_Data != null)
			{
				PersistentData[] persistentData = item.Internal_Data.PersistentData;
				for (int i = 0; i < persistentData.Length; i++)
				{
					persistentData[i].FindDependencies(dependencies, allResources, allowNulls);
				}
			}
			if (item.Children != null)
			{
				for (int j = 0; j < item.Children.Count; j++)
				{
					ProjectItem item2 = item.Children[j];
					FindDependencies(item2, dependencies, allResources, allowNulls);
				}
			}
		}

		private static void FindReferencedObjects(ProjectItem item, Dictionary<long, UnityEngine.Object> referencedObjects, Dictionary<long, UnityEngine.Object> allResources, bool allowNulls)
		{
			if (item.Internal_Meta.Descriptor != null)
			{
				item.Internal_Meta.Descriptor.FindReferencedObjects(referencedObjects, allResources, allowNulls);
			}
			if (item.Children != null)
			{
				for (int i = 0; i < item.Children.Count; i++)
				{
					ProjectItem item2 = item.Children[i];
					FindReferencedObjects(item2, referencedObjects, allResources, allowNulls);
				}
			}
		}

		private static Dictionary<long, ProjectItem> GetIdToProjectItemMapping(ProjectItem projectItem, bool dynamicOnly)
		{
			Dictionary<long, ProjectItem> dictionary = new Dictionary<long, ProjectItem>();
			GetIdToProjectItemMapping(projectItem, dictionary, dynamicOnly);
			return dictionary;
		}

		private static void GetIdToProjectItemMapping(ProjectItem item, Dictionary<long, ProjectItem> mapping, bool dynamicOnly)
		{
			if (item.Internal_Meta.Descriptor != null)
			{
				if ((item.Internal_Meta.Descriptor.Children == null || item.Internal_Meta.Descriptor.Children.Length == 0) && (item.Internal_Meta.Descriptor.Components == null || item.Internal_Meta.Descriptor.Components.Length == 0))
				{
					if ((!dynamicOnly || IsDynamicResource(item)) && !mapping.ContainsKey(InstanceID(item)))
					{
						mapping.Add(InstanceID(item), item);
					}
				}
				else
				{
					long[] instanceIds = item.Internal_Meta.Descriptor.GetInstanceIds();
					foreach (long num in instanceIds)
					{
						if ((!dynamicOnly || IdentifiersMap.IsDynamicResourceID(num)) && !mapping.ContainsKey(num))
						{
							mapping.Add(num, item);
						}
					}
				}
			}
			if (item.Children != null)
			{
				for (int j = 0; j < item.Children.Count; j++)
				{
					ProjectItem item2 = item.Children[j];
					GetIdToProjectItemMapping(item2, mapping, dynamicOnly);
				}
			}
		}

		private static void Diff(ProjectItem dst, ProjectItem src, List<ProjectItem> diff)
		{
			if (!dst.IsFolder)
			{
				return;
			}
			Dictionary<string, ProjectItem> dictionary = ((dst.Children == null) ? new Dictionary<string, ProjectItem>() : dst.Children.ToDictionary((ProjectItem child) => child.ToString()));
			if (src.Children == null)
			{
				return;
			}
			for (int i = 0; i < src.Children.Count; i++)
			{
				ProjectItem projectItem = src.Children[i];
				ProjectItem value;
				if (dictionary.TryGetValue(projectItem.ToString(), out value))
				{
					Diff(projectItem, value, diff);
				}
				else if (projectItem.IsExposedFromEditor)
				{
					diff.Add(projectItem);
				}
			}
		}

		private static void MergeData(ProjectItem dst, ProjectItem src)
		{
			if (!dst.IsFolder)
			{
				if (!src.IsExposedFromEditor)
				{
					return;
				}
				Dictionary<long, PersistentData> dictionary = ((dst.Internal_Data.PersistentData == null) ? new Dictionary<long, PersistentData>() : dst.Internal_Data.PersistentData.ToDictionary((PersistentData k) => k.InstanceId));
				Dictionary<long, PersistentData> dictionary2 = null;
				if (src.Internal_Data.PersistentData != null)
				{
					dictionary2 = src.Internal_Data.PersistentData.ToDictionary((PersistentData k) => k.InstanceId);
				}
				PersistentDescriptor descriptor = src.Internal_Meta.Descriptor;
				PersistentDescriptor descriptor2 = dst.Internal_Meta.Descriptor;
				if (descriptor2.InstanceId != descriptor.InstanceId)
				{
					return;
				}
				if (dictionary2 != null)
				{
					MergeRecursive(descriptor2, descriptor, dictionary, dictionary2);
					dst.Internal_Data.PersistentData = dictionary.Values.ToArray();
					if (src.Internal_Data.RawData != null)
					{
						dst.Internal_Data.RawData = src.Internal_Data.RawData;
					}
				}
				else
				{
					dst.Internal_Data.PersistentData = null;
					dst.Internal_Data.RawData = src.Internal_Data.RawData;
				}
				return;
			}
			Dictionary<string, ProjectItem> dictionary3 = ((dst.Children == null) ? new Dictionary<string, ProjectItem>() : dst.Children.ToDictionary((ProjectItem child) => child.ToString()));
			if (src.Children == null)
			{
				return;
			}
			for (int i = 0; i < src.Children.Count; i++)
			{
				ProjectItem projectItem = src.Children[i];
				ProjectItem value;
				if (dictionary3.TryGetValue(projectItem.ToString(), out value))
				{
					MergeData(value, projectItem);
				}
				else if (!projectItem.IsExposedFromEditor)
				{
					dictionary3.Add(projectItem.ToString(), projectItem);
					dst.AddChild(projectItem);
					i--;
				}
			}
		}

		private static void MergeRecursive(PersistentDescriptor descriptor, PersistentDescriptor otherDescriptor, Dictionary<long, PersistentData> data, Dictionary<long, PersistentData> otherData)
		{
			if (descriptor.InstanceId != otherDescriptor.InstanceId)
			{
				return;
			}
			data[descriptor.InstanceId] = otherData[descriptor.InstanceId];
			if (descriptor.Components != null && otherDescriptor.Components != null)
			{
				Dictionary<long, PersistentDescriptor> dictionary = otherDescriptor.Components.ToDictionary((PersistentDescriptor k) => k.InstanceId);
				for (int i = 0; i < descriptor.Components.Length; i++)
				{
					PersistentDescriptor persistentDescriptor = descriptor.Components[i];
					PersistentDescriptor value;
					if (dictionary.TryGetValue(persistentDescriptor.InstanceId, out value))
					{
						data[persistentDescriptor.InstanceId] = otherData[value.InstanceId];
					}
				}
			}
			if (descriptor.Children == null || otherDescriptor.Children == null)
			{
				return;
			}
			Dictionary<long, PersistentDescriptor> dictionary2 = otherDescriptor.Children.ToDictionary((PersistentDescriptor k) => k.InstanceId);
			for (int j = 0; j < descriptor.Children.Length; j++)
			{
				PersistentDescriptor persistentDescriptor2 = descriptor.Children[j];
				PersistentDescriptor value2;
				if (dictionary2.TryGetValue(persistentDescriptor2.InstanceId, out value2))
				{
					MergeRecursive(persistentDescriptor2, value2, data, otherData);
				}
			}
		}

		private static void TryLoadRawData(ProjectItem projectItem, Dictionary<long, UnityEngine.Object> objects)
		{
			if (!projectItem.IsExposedFromEditor && projectItem.Internal_Data != null && projectItem.Internal_Data.RawData != null)
			{
				PersistentData persistentData = projectItem.Internal_Data.PersistentData[0];
				UnityEngine.Object @object = objects.Get(persistentData.InstanceId);
				if (@object is Texture2D && string.IsNullOrEmpty(projectItem.BundleName))
				{
					Texture2D tex = (Texture2D)@object;
					tex.LoadImage(projectItem.Internal_Data.RawData);
				}
			}
		}

		private static void TryReadRawData(ProjectItem projectItem, UnityEngine.Object obj)
		{
			if (!projectItem.IsExposedFromEditor && obj is Texture2D && string.IsNullOrEmpty(projectItem.BundleName))
			{
				Texture2D tex = (Texture2D)obj;
				projectItem.Internal_Data.RawData = tex.EncodeToPNG();
			}
		}
	}
}
