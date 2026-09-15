using System;
using System.Collections;
using AssetBundles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MeshVR
{
	public class GlobalSceneOptions : MonoBehaviour
	{
		[Serializable]
		public class AssetBundleAssetNames
		{
			public string assetBundleName;

			public string assetName;

			public bool setPosition;

			public Vector3 position;

			public bool setRotation;

			public Vector3 rotationEuler;
		}

		protected bool isLoading;

		public Transform atomContainer;

		public Text versionText;

		public bool processCommandlineForBenchmarks;

		public string freeKey;

		public string teaserKey;

		public string entertainerKey;

		public string creatorKey;

		public string steamKey;

		public string nsteamKey;

		public string retailKey;

		public string keyFilePath;

		public string legacySteamKeyFilePath;

		public bool disableAdvancedSceneEdit;

		public bool disableSaveSceneButton;

		public bool disableLoadSceneButton;

		public bool disableCustomUI;

		public bool disableBrowse;

		public bool disablePackages;

		public bool disablePromotional;

		public bool disableKeyInformation;

		public bool disableHub;

		public bool disableTermsOfUse;

		public bool disableUI;

		public bool alwaysEnablePointers;

		public bool disableNavigation;

		public bool disableVR;

		public float startingMonitorCameraFOV = 40f;

		public bool enableStartScene;

		public JSONEmbed startJSONEmbedScene;

		public bool assetManagerSimulateInEditor = true;

		public bool disableLeap;

		public bool loadPrefsFileOnStart = true;

		public bool bypassFirstTimeUser = true;

		public bool overridePhysicsRate;

		public UserPreferences.PhysicsRate physicsRate = UserPreferences.PhysicsRate._90;

		public bool overridePhysicsUpdateCap;

		public int physicsUpdateCap = 2;

		public bool overridePhysicsHighQuality;

		public bool physicsHighQuality;

		public bool overrideSoftPhysics;

		public bool softPhysics = true;

		public bool overrideMsaaLevel;

		public int msaaLevel = 4;

		public bool overridePixelLightCount;

		public int pixelLightCount = 2;

		public bool enablePerfMonOnStart;

		public AssetBundleAssetNames[] assetsToLoadOnStart;

		public Transform[] transformsToActivateAfterAssetLoad;

		public static GlobalSceneOptions singleton { get; private set; }

		public static bool IsLoading
		{
			get
			{
				if (singleton != null)
				{
					return singleton.isLoading;
				}
				return false;
			}
		}

		private IEnumerator LoadAssets()
		{
			AssetBundleManager.SetSourceAssetBundleDirectory(AssetBundleManager.GetStreamingAssetsDirectory() + "/");
			AssetBundleLoadManifestOperation request = AssetBundleManager.Initialize();
			if (request != null)
			{
				yield return StartCoroutine(request);
			}
			Debug.Log("Asset Manager Ready");
			AssetBundleAssetNames[] array = assetsToLoadOnStart;
			foreach (AssetBundleAssetNames assetToLoad in array)
			{
				AssetBundleLoadAssetOperation arequest = AssetBundleManager.LoadAssetAsync(assetToLoad.assetBundleName, assetToLoad.assetName, typeof(GameObject));
				if (arequest == null)
				{
					Debug.LogError("Failed to load asset " + assetToLoad.assetBundleName + " " + assetToLoad.assetName);
					continue;
				}
				yield return StartCoroutine(arequest);
				GameObject go = arequest.GetAsset<GameObject>();
				if (!(go != null))
				{
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(go);
				if (atomContainer != null)
				{
					gameObject.transform.SetParent(atomContainer, true);
					if (assetToLoad.setPosition)
					{
						gameObject.transform.position = assetToLoad.position;
					}
					if (assetToLoad.setRotation)
					{
						gameObject.transform.eulerAngles = assetToLoad.rotationEuler;
					}
					gameObject.name = assetToLoad.assetName;
				}
			}
			if (transformsToActivateAfterAssetLoad != null)
			{
				Transform[] array2 = transformsToActivateAfterAssetLoad;
				foreach (Transform transform in array2)
				{
					transform.gameObject.SetActive(true);
				}
			}
			isLoading = false;
		}

		private void Awake()
		{
			singleton = this;
		}

		private void Start()
		{
			if (processCommandlineForBenchmarks)
			{
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				string text = string.Empty;
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i] == "-benchmark")
					{
						text = commandLineArgs[i + 1];
					}
				}
				if (text != string.Empty)
				{
					SceneManager.LoadScene(text, LoadSceneMode.Single);
				}
			}
			if (assetsToLoadOnStart != null && assetsToLoadOnStart.Length > 0)
			{
				isLoading = true;
				StartCoroutine(LoadAssets());
			}
		}
	}
}
