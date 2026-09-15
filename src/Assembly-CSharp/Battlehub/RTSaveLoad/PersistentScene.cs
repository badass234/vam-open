using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentScene
	{
		public PersistentDescriptor[] Descriptors;

		public PersistentData[] Data;

		public PersistentScene()
		{
			Descriptors = new PersistentDescriptor[0];
			Data = new PersistentData[0];
		}

		public static void InstantiateGameObjects(PersistentScene scene)
		{
			if (IdentifiersMap.Instance == null)
			{
				Debug.LogError("Create Runtime Resource Map");
				return;
			}
			DestroyGameObjects();
			if (scene.Data != null || scene.Descriptors != null)
			{
				if ((scene.Data == null && scene.Descriptors != null) || (scene.Data != null && scene.Descriptors == null))
				{
					throw new ArgumentException("data is corrupted", "scene");
				}
				if (scene.Descriptors.Length != 0)
				{
					bool includeDynamicResources = true;
					Dictionary<long, UnityEngine.Object> dictionary = IdentifiersMap.FindResources(includeDynamicResources);
					PersistentDescriptor.GetOrCreateGameObjects(scene.Descriptors, dictionary);
					PersistentData.RestoreDataAndResolveDependencies(scene.Data, dictionary);
				}
			}
		}

		private static void DestroyGameObjects()
		{
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				if (gameObject != null && DestroyGameObjects(gameObject.transform, false))
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
		}

		private static bool DestroyGameObjects(Transform t, bool forceDestroy)
		{
			bool result = true;
			if (!forceDestroy)
			{
				PersistentIgnore component = t.GetComponent<PersistentIgnore>();
				if ((bool)component)
				{
					if (component.IsRuntime)
					{
						return true;
					}
					return false;
				}
			}
			List<Transform> list = new List<Transform>();
			foreach (Transform item in t)
			{
				list.Add(item);
			}
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				if (DestroyGameObjects(transform, forceDestroy))
				{
					UnityEngine.Object.DestroyImmediate(transform.gameObject);
				}
				else
				{
					transform.SetParent(null, true);
				}
			}
			return result;
		}

		public static PersistentScene CreatePersistentScene(params Type[] ignoreTypes)
		{
			if (IdentifiersMap.Instance == null)
			{
				Debug.LogError("Create Runtime Resource Map");
				return null;
			}
			GameObject[] gameObjects = (from g in SceneManager.GetActiveScene().GetRootGameObjects()
				orderby g.transform.GetSiblingIndex()
				select g).ToArray();
			PersistentScene persistentScene = new PersistentScene();
			PersistentData.CreatePersistentDescriptorsAndData(gameObjects, out persistentScene.Descriptors, out persistentScene.Data);
			return persistentScene;
		}
	}
}
