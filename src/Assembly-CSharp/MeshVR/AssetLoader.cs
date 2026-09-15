using System;
using System.Collections;
using System.Collections.Generic;
using MVR.FileManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshVR
{
	public class AssetLoader : MonoBehaviour
	{
		public delegate void LoadBundleFromFileCallback(AssetBundleFromFileRequest abffr);

		public class AssetBundleFromFileRequest
		{
			public string path;

			public LoadBundleFromFileCallback callback;

			public AssetBundle assetBundle;
		}

		public delegate void LoadSceneIntoTransformCallback(SceneLoadIntoTransformRequest slr);

		public class SceneLoadIntoTransformRequest
		{
			public string scenePath;

			public Transform transform;

			public LoadSceneIntoTransformCallback callback;

			public bool importLightmaps;

			public bool importLightProbes;

			public LightmapData[] lightmapData;

			public GlobalLightingManager.LightProbesHolder lightProbesHolder;

			public bool requestCancelled;
		}

		protected static AssetLoader singleton;

		protected Dictionary<string, int> assetBundleReferenceCounts;

		protected Dictionary<string, AssetBundle> pathToAssetBundle;

		protected List<AssetBundleFromFileRequest> assetBundleFromFileQueue;

		protected List<SceneLoadIntoTransformRequest> sceneLoadIntoTransformQueue;

		protected IEnumerator LoadBundleFileAsync(AssetBundleFromFileRequest abffr)
		{
			if (assetBundleReferenceCounts == null)
			{
				assetBundleReferenceCounts = new Dictionary<string, int>();
			}
			if (pathToAssetBundle == null)
			{
				pathToAssetBundle = new Dictionary<string, AssetBundle>();
			}
			string path = abffr.path;
			int cnt;
			if (assetBundleReferenceCounts.TryGetValue(path, out cnt))
			{
				cnt++;
				assetBundleReferenceCounts.Remove(path);
				assetBundleReferenceCounts.Add(path, cnt);
			}
			else
			{
				assetBundleReferenceCounts.Add(path, 1);
			}
			AssetBundle ab = null;
			if (!pathToAssetBundle.TryGetValue(path, out ab))
			{
				AssetBundleCreateRequest abcr = null;
				if (FileManager.IsFileInPackage(path))
				{
					VarFileEntry vfe = FileManager.GetVarFileEntry(path);
					if (vfe.Simulated)
					{
						string path2 = vfe.Package.Path + "\\" + vfe.InternalPath;
						abcr = AssetBundle.LoadFromFileAsync(path2);
					}
					else
					{
						byte[] assetbundleBytes = new byte[vfe.Size];
						yield return FileManager.ReadAllBytesCoroutine(vfe, assetbundleBytes);
						abcr = AssetBundle.LoadFromMemoryAsync(assetbundleBytes);
					}
				}
				else
				{
					abcr = AssetBundle.LoadFromFileAsync(path);
				}
				if (abcr != null)
				{
					yield return abcr;
					if (!abcr.assetBundle)
					{
						SuperController.LogError("Error during attempt to load assetbundle " + path + ". Not valid");
					}
					else
					{
						ab = abcr.assetBundle;
						pathToAssetBundle.Add(path, abcr.assetBundle);
					}
				}
			}
			abffr.assetBundle = ab;
			if (abffr.callback != null)
			{
				abffr.callback(abffr);
			}
		}

		public static void QueueLoadAssetBundleFromFile(AssetBundleFromFileRequest abffr)
		{
			if (singleton != null)
			{
				if (singleton.assetBundleFromFileQueue == null)
				{
					singleton.assetBundleFromFileQueue = new List<AssetBundleFromFileRequest>();
				}
				singleton.assetBundleFromFileQueue.Add(abffr);
			}
		}

		protected IEnumerator AssetBundleFromFileQueueProcessor()
		{
			if (assetBundleFromFileQueue == null)
			{
				assetBundleFromFileQueue = new List<AssetBundleFromFileRequest>();
			}
			while (true)
			{
				yield return null;
				if (assetBundleFromFileQueue.Count > 0)
				{
					AssetBundleFromFileRequest abffr = assetBundleFromFileQueue[0];
					assetBundleFromFileQueue.RemoveAt(0);
					yield return LoadBundleFileAsync(abffr);
				}
			}
		}

		public static void DoneWithAssetBundleFromFile(string path)
		{
			int value;
			if (!(singleton != null) || singleton.assetBundleReferenceCounts == null || !singleton.assetBundleReferenceCounts.TryGetValue(path, out value))
			{
				return;
			}
			value--;
			if (value <= 0)
			{
				AssetBundle value2;
				if (singleton.pathToAssetBundle.TryGetValue(path, out value2))
				{
					Debug.Log("Unloading unused asset bundle " + path);
					if (value2 != null)
					{
						value2.Unload(true);
					}
					singleton.pathToAssetBundle.Remove(path);
				}
				singleton.assetBundleReferenceCounts.Remove(path);
			}
			else
			{
				singleton.assetBundleReferenceCounts.Remove(path);
				singleton.assetBundleReferenceCounts.Add(path, value);
			}
		}

		protected IEnumerator LoadSceneIntoTransformAsync(SceneLoadIntoTransformRequest slr)
		{
			AsyncOperation async = null;
			try
			{
				async = SceneManager.LoadSceneAsync(slr.scenePath, LoadSceneMode.Additive);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Error during attempt to load scene: " + ex);
			}
			if (async == null)
			{
				yield break;
			}
			yield return async;
			Scene sc = SceneManager.GetSceneByPath(slr.scenePath);
			if (!sc.IsValid())
			{
				yield break;
			}
			if (slr.requestCancelled)
			{
				yield return SceneManager.UnloadSceneAsync(sc);
				yield break;
			}
			LightmapData[] newLightmapData = LightmapSettings.lightmaps;
			LightProbes lightProbes = LightmapSettings.lightProbes;
			if (GlobalLightingManager.singleton != null)
			{
				if (GlobalLightingManager.singleton.PushLightmapData(newLightmapData))
				{
					slr.lightmapData = newLightmapData;
					if (!slr.importLightmaps)
					{
						GlobalLightingManager.singleton.RemoveLightmapData(slr.lightmapData);
					}
				}
				else
				{
					slr.lightmapData = null;
				}
				slr.lightProbesHolder = GlobalLightingManager.singleton.PushLightProbes(lightProbes);
				if (slr.lightProbesHolder != null && !slr.importLightProbes)
				{
					GlobalLightingManager.singleton.RemoveLightProbesHolder(slr.lightProbesHolder);
				}
			}
			if (slr.transform != null)
			{
				GameObject[] rootGameObjects = sc.GetRootGameObjects();
				GameObject[] array = rootGameObjects;
				foreach (GameObject gameObject in array)
				{
					Vector3 localPosition = gameObject.transform.localPosition;
					Quaternion localRotation = gameObject.transform.localRotation;
					Vector3 localScale = gameObject.transform.localScale;
					SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
					gameObject.transform.SetParent(slr.transform);
					gameObject.transform.localPosition = localPosition;
					gameObject.transform.localRotation = localRotation;
					gameObject.transform.localScale = localScale;
				}
			}
			yield return SceneManager.UnloadSceneAsync(sc);
			if (slr.requestCancelled)
			{
				if (slr.lightmapData != null && slr.importLightmaps)
				{
					GlobalLightingManager.singleton.RemoveLightmapData(slr.lightmapData);
					slr.lightmapData = null;
				}
				if (slr.lightProbesHolder != null && slr.importLightProbes)
				{
					GlobalLightingManager.singleton.RemoveLightProbesHolder(slr.lightProbesHolder);
					slr.lightProbesHolder = null;
				}
			}
			else if (slr.callback != null)
			{
				slr.callback(slr);
			}
		}

		public static void QueueLoadSceneIntoTransform(SceneLoadIntoTransformRequest slr)
		{
			if (singleton != null)
			{
				if (singleton.sceneLoadIntoTransformQueue == null)
				{
					singleton.sceneLoadIntoTransformQueue = new List<SceneLoadIntoTransformRequest>();
				}
				singleton.sceneLoadIntoTransformQueue.Add(slr);
			}
		}

		protected IEnumerator SceneLoadIntoTransfromQueueProcessor()
		{
			if (sceneLoadIntoTransformQueue == null)
			{
				sceneLoadIntoTransformQueue = new List<SceneLoadIntoTransformRequest>();
			}
			while (true)
			{
				yield return null;
				if (sceneLoadIntoTransformQueue.Count > 0)
				{
					SceneLoadIntoTransformRequest slr = sceneLoadIntoTransformQueue[0];
					sceneLoadIntoTransformQueue.RemoveAt(0);
					yield return LoadSceneIntoTransformAsync(slr);
				}
			}
		}

		private void Awake()
		{
			singleton = this;
		}

		private void Start()
		{
			StartCoroutine(AssetBundleFromFileQueueProcessor());
			StartCoroutine(SceneLoadIntoTransfromQueueProcessor());
		}

		private void OnDestroy()
		{
			if (pathToAssetBundle == null)
			{
				return;
			}
			foreach (AssetBundle value in pathToAssetBundle.Values)
			{
				value.Unload(true);
			}
		}
	}
}
