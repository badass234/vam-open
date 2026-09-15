using System;
using System.Collections.Generic;
using System.IO;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class AddResourcesTest : MonoBehaviour
	{
		public KeyCode AddInstantiatedObjectKey = KeyCode.Keypad1;

		public KeyCode AddPrefabKey = KeyCode.Keypad2;

		public KeyCode AddTextureKey = KeyCode.Keypad3;

		public KeyCode AddWithDependenciesKey = KeyCode.Keypad4;

		public KeyCode AddAssetBundleKey = KeyCode.Keypad5;

		public KeyCode AddAssetBundleKey2 = KeyCode.Keypad6;

		public GameObject Prefab;

		public string ImagePath = "test.png";

		private IProjectManager m_projectManager;

		private void Start()
		{
			m_projectManager = Dependencies.ProjectManager;
		}

		private void Update()
		{
			if (InputController.GetKeyDown(AddAssetBundleKey2))
			{
				ProjectItem project = m_projectManager.Project;
				m_projectManager.AddBundledResources(project, "bundledemo", (UnityEngine.Object obj, string assetName) => true, delegate(ProjectItem[] addedItems)
				{
					for (int i = 0; i < addedItems.Length; i++)
					{
						Debug.Log(addedItems[i].ToString() + " added");
					}
				});
			}
			if (InputController.GetKeyDown(AddAssetBundleKey))
			{
				ProjectItem project2 = m_projectManager.Project;
				m_projectManager.AddBundledResource(project2, "bundledemo", "monkey", delegate(ProjectItem[] addedItems)
				{
					for (int j = 0; j < addedItems.Length; j++)
					{
						Debug.Log(addedItems[j].ToString() + " added");
					}
				});
			}
			if (InputController.GetKeyDown(AddWithDependenciesKey))
			{
				ProjectItem project3 = m_projectManager.Project;
				List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
				Material material = new Material(Shader.Find("Standard"));
				material.color = Color.yellow;
				Mesh mesh = RuntimeGraphics.CreateCubeMesh(Color.white, Vector3.zero, 1f);
				mesh.name = "TestMesh";
				GameObject gameObject = new GameObject();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				gameObject.name = "TestGO";
				meshRenderer.sharedMaterial = material;
				meshFilter.sharedMesh = mesh;
				objects.Add(gameObject);
				bool includingDependencies = true;
				Func<UnityEngine.Object, bool> filter = (UnityEngine.Object o) => !(o is Shader);
				m_projectManager.AddDynamicResources(project3, objects.ToArray(), includingDependencies, filter, delegate(ProjectItem[] addedItems)
				{
					for (int k = 0; k < addedItems.Length; k++)
					{
						Debug.Log(addedItems[k].ToString() + " added");
					}
					for (int l = 0; l < objects.Count; l++)
					{
						UnityEngine.Object.Destroy(objects[l]);
					}
				});
			}
			if (InputController.GetKeyDown(AddInstantiatedObjectKey))
			{
				ProjectItem project4 = m_projectManager.Project;
				List<UnityEngine.Object> objects2 = new List<UnityEngine.Object>();
				Material material2 = new Material(Shader.Find("Standard"));
				material2.color = Color.yellow;
				Mesh mesh2 = RuntimeGraphics.CreateCubeMesh(Color.white, Vector3.zero, 1f);
				mesh2.name = "TestMesh";
				GameObject gameObject2 = new GameObject();
				MeshRenderer meshRenderer2 = gameObject2.AddComponent<MeshRenderer>();
				MeshFilter meshFilter2 = gameObject2.AddComponent<MeshFilter>();
				gameObject2.name = "TestGO";
				meshRenderer2.sharedMaterial = material2;
				meshFilter2.sharedMesh = mesh2;
				objects2.Add(material2);
				objects2.Add(mesh2);
				objects2.Add(gameObject2);
				m_projectManager.AddDynamicResources(project4, objects2.ToArray(), delegate(ProjectItem[] addedItems)
				{
					for (int m = 0; m < addedItems.Length; m++)
					{
						Debug.Log(addedItems[m].ToString() + " added");
					}
					for (int n = 0; n < objects2.Count; n++)
					{
						UnityEngine.Object.Destroy(objects2[n]);
					}
				});
			}
			if (InputController.GetKeyDown(AddPrefabKey))
			{
				ProjectItem project5 = m_projectManager.Project;
				List<UnityEngine.Object> list = new List<UnityEngine.Object>();
				if (Prefab != null)
				{
					list.Add(Prefab);
				}
				bool includingDependencies2 = true;
				Func<UnityEngine.Object, bool> filter2 = (UnityEngine.Object o) => !(o is Shader);
				m_projectManager.AddDynamicResources(project5, list.ToArray(), includingDependencies2, filter2, delegate(ProjectItem[] addedItems)
				{
					for (int num = 0; num < addedItems.Length; num++)
					{
						Debug.Log(addedItems[num].ToString() + " added");
					}
				});
			}
			if (!InputController.GetKeyDown(AddTextureKey))
			{
				return;
			}
			ProjectItem project6 = m_projectManager.Project;
			List<UnityEngine.Object> list2 = new List<UnityEngine.Object>();
			string text = Application.streamingAssetsPath + "/" + ImagePath;
			Texture2D texture2D = LoadPNG(text);
			if (texture2D == null)
			{
				Debug.LogErrorFormat("File {0} not found", text);
				return;
			}
			texture2D.name = "TestTexture";
			list2.Add(texture2D);
			m_projectManager.AddDynamicResources(project6, list2.ToArray(), delegate(ProjectItem[] addedItems)
			{
				for (int num2 = 0; num2 < addedItems.Length; num2++)
				{
					Debug.Log(addedItems[num2].ToString() + " added");
				}
			});
		}

		public static Texture2D LoadPNG(string filePath)
		{
			Texture2D texture2D = null;
			if (File.Exists(filePath))
			{
				byte[] data = File.ReadAllBytes(filePath);
				texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, true);
				texture2D.LoadImage(data);
			}
			return texture2D;
		}
	}
}
