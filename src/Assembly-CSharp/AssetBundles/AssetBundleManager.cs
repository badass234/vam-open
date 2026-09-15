using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleManager : MonoBehaviour
	{
		public enum LogMode
		{
			All,
			JustErrors
		}

		public enum LogType
		{
			Info,
			Warning,
			Error
		}

		private static LogMode m_LogMode = LogMode.All;

		private static string m_BaseDownloadingURL = string.Empty;

		private static string[] m_ActiveVariants = new string[0];

		private static AssetBundleManifest m_AssetBundleManifest = null;

		private static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, WWW> m_DownloadingWWWs = new Dictionary<string, WWW>();

		private static Dictionary<string, LoadedAssetBundle> m_TrackedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, AssetBundleCreateRequest> m_LoadingBundles = new Dictionary<string, AssetBundleCreateRequest>();

		private static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

		private static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();

		private static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

		protected List<string> keysToRemove;

		public static LogMode logMode
		{
			get
			{
				return m_LogMode;
			}
			set
			{
				m_LogMode = value;
			}
		}

		public static string BaseDownloadingURL
		{
			get
			{
				return m_BaseDownloadingURL;
			}
			set
			{
				m_BaseDownloadingURL = value;
			}
		}

		public static string[] ActiveVariants
		{
			get
			{
				return m_ActiveVariants;
			}
			set
			{
				m_ActiveVariants = value;
			}
		}

		public static AssetBundleManifest AssetBundleManifestObject
		{
			set
			{
				m_AssetBundleManifest = value;
			}
		}

		private static void Log(LogType logType, string text)
		{
			if (logType == LogType.Error)
			{
				Debug.LogError("[AssetBundleManager] " + text);
			}
			else if (m_LogMode == LogMode.All)
			{
				Debug.Log("[AssetBundleManager] " + text);
			}
		}

		/// <summary>
		/// Directory that holds the asset bundles. In a build it is the regular StreamingAssets; in
		/// the editor it is &lt;project&gt;\StreamingAssets (a junction to VaM_Data\StreamingAssets), so
		/// that 16 GB of bundles never reach the AssetDatabase.
		/// </summary>
		public static string GetStreamingAssetsDirectory()
		{
			if (Application.isEditor)
			{
				string projectRoot = Directory.GetParent(Application.dataPath).FullName;
				return Path.Combine(projectRoot, "StreamingAssets").Replace("\\", "/");
			}
			return Application.streamingAssetsPath;
		}

		public static void SetSourceAssetBundleDirectory(string relativePath)
		{
			BaseDownloadingURL = "file://" + relativePath;
		}

		public static void SetSourceAssetBundleURL(string absolutePath)
		{
			BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
		}

		public static void SetDevelopmentAssetBundleServer()
		{
			TextAsset textAsset = Resources.Load("AssetBundleServerURL") as TextAsset;
			string text = ((!(textAsset != null)) ? null : textAsset.text.Trim());
			if (text == null || text.Length == 0)
			{
				Debug.LogError("Development Server URL could not be found.");
			}
			else
			{
				SetSourceAssetBundleURL(text);
			}
		}

		public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
		{
			if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
			{
				return null;
			}
			LoadedAssetBundle value = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out value);
			if (value == null)
			{
				return null;
			}
			string[] value2 = null;
			if (!m_Dependencies.TryGetValue(assetBundleName, out value2))
			{
				return value;
			}
			string[] array = value2;
			foreach (string key in array)
			{
				if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
				{
					return value;
				}
				LoadedAssetBundle value3;
				m_LoadedAssetBundles.TryGetValue(key, out value3);
				if (value3 == null)
				{
					return null;
				}
			}
			return value;
		}

		public static AssetBundleLoadManifestOperation Initialize()
		{
			return Initialize(Utility.GetPlatformName());
		}

		public static AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
		{
			AssetBundleManager assetBundleManager = UnityEngine.Object.FindObjectOfType<AssetBundleManager>();
			if (assetBundleManager != null)
			{
				return null;
			}
			new GameObject("AssetBundleManager", typeof(AssetBundleManager));
			LoadAssetBundle(manifestAssetBundleName, true);
			AssetBundleLoadManifestOperation assetBundleLoadManifestOperation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
			m_InProgressOperations.Add(assetBundleLoadManifestOperation);
			return assetBundleLoadManifestOperation;
		}

		protected static void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
		{
			if (!isLoadingAssetBundleManifest && m_AssetBundleManifest == null)
			{
				Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
			}
			else if (!LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest) && !isLoadingAssetBundleManifest)
			{
				LoadDependencies(assetBundleName);
			}
		}

		protected static string RemapVariantName(string assetBundleName)
		{
			string[] allAssetBundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();
			string[] array = assetBundleName.Split('.');
			int num = int.MaxValue;
			int num2 = -1;
			for (int i = 0; i < allAssetBundlesWithVariant.Length; i++)
			{
				string[] array2 = allAssetBundlesWithVariant[i].Split('.');
				if (!(array2[0] != array[0]))
				{
					int num3 = Array.IndexOf(m_ActiveVariants, array2[1]);
					if (num3 == -1)
					{
						num3 = 2147483646;
					}
					if (num3 < num)
					{
						num = num3;
						num2 = i;
					}
				}
			}
			if (num == 2147483646)
			{
				Debug.LogWarning("Ambigious asset bundle variant chosen because there was no matching active variant: " + allAssetBundlesWithVariant[num2]);
			}
			if (num2 != -1)
			{
				return allAssetBundlesWithVariant[num2];
			}
			return assetBundleName;
		}

		protected static bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			LoadedAssetBundle value = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out value);
			if (value != null)
			{
				value.m_ReferencedCount++;
				return true;
			}
			LoadedAssetBundle value2;
			if (m_TrackedAssetBundles.TryGetValue(assetBundleName, out value2))
			{
				value2.m_ReferencedCount++;
				return true;
			}
			WWW wWW = null;
			string text = m_BaseDownloadingURL + assetBundleName;
			if (isLoadingAssetBundleManifest)
			{
				wWW = new WWW(text);
				m_DownloadingWWWs.Add(assetBundleName, wWW);
				value2 = new LoadedAssetBundle(null);
				m_TrackedAssetBundles.Add(assetBundleName, value2);
			}
			else if (text.StartsWith("file:"))
			{
				text = text.Replace("file://", string.Empty);
				AssetBundleCreateRequest value3 = AssetBundle.LoadFromFileAsync(text);
				m_LoadingBundles.Add(assetBundleName, value3);
				value2 = new LoadedAssetBundle(null);
				m_TrackedAssetBundles.Add(assetBundleName, value2);
			}
			else
			{
				wWW = WWW.LoadFromCacheOrDownload(text, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0u);
				m_DownloadingWWWs.Add(assetBundleName, wWW);
				value2 = new LoadedAssetBundle(null);
				m_TrackedAssetBundles.Add(assetBundleName, value2);
			}
			return false;
		}

		protected static void LoadDependencies(string assetBundleName)
		{
			if (m_AssetBundleManifest == null)
			{
				Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
			string[] allDependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
			if (allDependencies.Length != 0)
			{
				for (int i = 0; i < allDependencies.Length; i++)
				{
					allDependencies[i] = RemapVariantName(allDependencies[i]);
				}
				m_Dependencies.Add(assetBundleName, allDependencies);
				for (int j = 0; j < allDependencies.Length; j++)
				{
					LoadAssetBundleInternal(allDependencies[j], false);
				}
			}
		}

		public static void UnloadAssetBundle(string assetBundleName)
		{
			if (UnloadAssetBundleInternal(assetBundleName))
			{
				UnloadDependencies(assetBundleName);
			}
		}

		protected static void UnloadDependencies(string assetBundleName)
		{
			string[] value = null;
			if (m_Dependencies.TryGetValue(assetBundleName, out value))
			{
				string[] array = value;
				foreach (string assetBundleName2 in array)
				{
					UnloadAssetBundleInternal(assetBundleName2);
				}
				m_Dependencies.Remove(assetBundleName);
			}
		}

		protected static bool UnloadAssetBundleInternal(string assetBundleName)
		{
			LoadedAssetBundle value;
			if (m_TrackedAssetBundles.TryGetValue(assetBundleName, out value))
			{
				value.m_ReferencedCount--;
				if (value.m_ReferencedCount == 0 && value.m_AssetBundle != null)
				{
					value.m_AssetBundle.Unload(true);
					m_LoadedAssetBundles.Remove(assetBundleName);
					m_TrackedAssetBundles.Remove(assetBundleName);
					return true;
				}
				return false;
			}
			Debug.LogError("Tried to unload assetbundle " + assetBundleName + " which is not loaded");
			return false;
		}

		public static void ReportLoadedAssetBundles()
		{
			int num = 0;
			if (m_TrackedAssetBundles != null)
			{
				foreach (LoadedAssetBundle value in m_TrackedAssetBundles.Values)
				{
					SuperController.LogMessage("Loaded bundle " + value.m_AssetBundle.name + " has " + value.m_ReferencedCount + " references");
					num++;
				}
			}
			SuperController.LogMessage("Found " + num + " loaded bundles");
		}

		public static int GetNumberOfLoadedAssetBundles()
		{
			int num = 0;
			if (m_TrackedAssetBundles != null)
			{
				foreach (LoadedAssetBundle value in m_TrackedAssetBundles.Values)
				{
					num++;
				}
			}
			return num;
		}

		private void Update()
		{
			if (keysToRemove == null)
			{
				keysToRemove = new List<string>();
			}
			else
			{
				keysToRemove.Clear();
			}
			foreach (KeyValuePair<string, WWW> downloadingWWW in m_DownloadingWWWs)
			{
				WWW value = downloadingWWW.Value;
				if (value.error != null)
				{
					if (!m_DownloadingErrors.ContainsKey(downloadingWWW.Key))
					{
						m_DownloadingErrors.Add(downloadingWWW.Key, $"Failed downloading bundle {downloadingWWW.Key} from {value.url}: {value.error}");
					}
					keysToRemove.Add(downloadingWWW.Key);
				}
				else
				{
					if (!value.isDone)
					{
						continue;
					}
					AssetBundle assetBundle = value.assetBundle;
					if (assetBundle == null)
					{
						if (!m_DownloadingErrors.ContainsKey(downloadingWWW.Key))
						{
							m_DownloadingErrors.Add(downloadingWWW.Key, $"{downloadingWWW.Key} is not a valid asset bundle.");
						}
						keysToRemove.Add(downloadingWWW.Key);
						continue;
					}
					LoadedAssetBundle value2;
					if (m_TrackedAssetBundles.TryGetValue(downloadingWWW.Key, out value2))
					{
						if (value2.m_ReferencedCount == 0)
						{
							Debug.LogError("Unload assetbundle " + downloadingWWW.Key + " that was untracked while being loaded");
							assetBundle.Unload(true);
							m_TrackedAssetBundles.Remove(downloadingWWW.Key);
							m_LoadedAssetBundles.Remove(downloadingWWW.Key);
						}
						else if (!m_LoadedAssetBundles.ContainsKey(downloadingWWW.Key))
						{
							value2.m_AssetBundle = value.assetBundle;
							m_LoadedAssetBundles.Add(downloadingWWW.Key, value2);
						}
						else
						{
							Debug.LogError("Loaded assetbundles already contains " + downloadingWWW.Key);
						}
					}
					keysToRemove.Add(downloadingWWW.Key);
				}
			}
			foreach (string item in keysToRemove)
			{
				WWW wWW = m_DownloadingWWWs[item];
				m_DownloadingWWWs.Remove(item);
				wWW.Dispose();
			}
			keysToRemove.Clear();
			foreach (KeyValuePair<string, AssetBundleCreateRequest> loadingBundle in m_LoadingBundles)
			{
				AssetBundleCreateRequest value3 = loadingBundle.Value;
				if (!value3.isDone)
				{
					continue;
				}
				AssetBundle assetBundle2 = value3.assetBundle;
				if (assetBundle2 == null)
				{
					if (!m_DownloadingErrors.ContainsKey(loadingBundle.Key))
					{
						m_DownloadingErrors.Add(loadingBundle.Key, $"{loadingBundle.Key} is not a valid asset bundle.");
					}
					keysToRemove.Add(loadingBundle.Key);
					continue;
				}
				LoadedAssetBundle value4;
				if (m_TrackedAssetBundles.TryGetValue(loadingBundle.Key, out value4))
				{
					if (value4.m_ReferencedCount == 0)
					{
						Debug.LogError("Unload assetbundle " + loadingBundle.Key + " that was untracked while being loaded");
						assetBundle2.Unload(true);
						m_TrackedAssetBundles.Remove(loadingBundle.Key);
					}
					else if (!m_LoadedAssetBundles.ContainsKey(loadingBundle.Key))
					{
						value4.m_AssetBundle = value3.assetBundle;
						m_LoadedAssetBundles.Add(loadingBundle.Key, value4);
					}
					else
					{
						Debug.LogError("Loaded assetbundles already contains " + loadingBundle.Key);
					}
				}
				keysToRemove.Add(loadingBundle.Key);
			}
			foreach (string item2 in keysToRemove)
			{
				m_LoadingBundles.Remove(item2);
			}
			int num = 0;
			while (num < m_InProgressOperations.Count)
			{
				if (!m_InProgressOperations[num].Update())
				{
					m_InProgressOperations.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public static void RegisterAssetBundleAdditionalUse(string assetBundleName)
		{
			LoadedAssetBundle value;
			if (m_TrackedAssetBundles.TryGetValue(assetBundleName, out value))
			{
				value.m_ReferencedCount++;
			}
			else
			{
				Debug.LogError("Tried to register additional use of asset bundle " + assetBundleName + " which is not loaded");
			}
		}

		public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
			assetBundleName = RemapVariantName(assetBundleName);
			LoadAssetBundle(assetBundleName);
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);
			m_InProgressOperations.Add(assetBundleLoadAssetOperation);
			return assetBundleLoadAssetOperation;
		}

		public static AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
		{
			Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");
			AssetBundleLoadOperation assetBundleLoadOperation = null;
			assetBundleName = RemapVariantName(assetBundleName);
			LoadAssetBundle(assetBundleName);
			assetBundleLoadOperation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);
			m_InProgressOperations.Add(assetBundleLoadOperation);
			return assetBundleLoadOperation;
		}
	}
}
