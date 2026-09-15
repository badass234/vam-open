using System.Collections;
using AssetBundles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JSONStorableDynamic : MonoBehaviour
{
	public delegate void OnLoaded();

	public Atom containingAtom;

	public Transform prefab;

	public string sceneName;

	public string assetBundleName;

	public string assetName;

	[SerializeField]
	protected Transform instance;

	protected bool isRegistered;

	public bool nameInstance;

	public string instanceName;

	public OnLoaded onPreloadedHandlers;

	public OnLoaded onLoadedHandlers;

	public bool neverUnloadOnDisable;

	public bool unloadOnDisable;

	protected JSONStorable[] jss;

	protected Canvas[] cvs;

	protected PhysicsSimulator[] pss;

	protected PhysicsSimulatorJSONStorable[] psjs;

	protected ScaleChangeReceiver[] scrs;

	protected ScaleChangeReceiverJSONStorable[] scrjss;

	protected RenderSuspend[] rss;

	public bool needsPostLoadJSONRestore;

	protected AsyncFlag loadFlag;

	protected bool didStartLoadFromBundleAsync;

	protected bool didRegisterBundle;

	protected AssetBundleLoadAssetOperation loadRequest;

	protected AsyncOperation async;

	public bool ready { get; protected set; }

	protected virtual void InitInstance()
	{
		instance.parent = base.transform;
		instance.localPosition = Vector3.zero;
		instance.localRotation = Quaternion.identity;
		instance.localScale = Vector3.one;
		UIConnectorMaster[] componentsInChildren = instance.GetComponentsInChildren<UIConnectorMaster>(true);
		UIConnectorMaster[] array = componentsInChildren;
		foreach (UIConnectorMaster uIConnectorMaster in array)
		{
			uIConnectorMaster.containingAtom = containingAtom;
		}
	}

	protected void RegisterDynamicObjs()
	{
		if (!(containingAtom != null) || !(instance != null))
		{
			return;
		}
		isRegistered = true;
		if (jss == null)
		{
			jss = instance.GetComponentsInChildren<JSONStorable>(true);
		}
		JSONStorable[] array = jss;
		foreach (JSONStorable jSONStorable in array)
		{
			if (!jSONStorable.exclude)
			{
				containingAtom.RegisterAdditionalStorable(jSONStorable);
			}
		}
		if (cvs == null)
		{
			cvs = instance.GetComponentsInChildren<Canvas>();
		}
		Canvas[] array2 = cvs;
		foreach (Canvas c in array2)
		{
			containingAtom.AddCanvas(c);
		}
		if (pss == null)
		{
			pss = instance.GetComponentsInChildren<PhysicsSimulator>(true);
		}
		PhysicsSimulator[] array3 = pss;
		foreach (PhysicsSimulator ps in array3)
		{
			containingAtom.RegisterDynamicPhysicsSimulator(ps);
		}
		if (psjs == null)
		{
			psjs = instance.GetComponentsInChildren<PhysicsSimulatorJSONStorable>(true);
		}
		PhysicsSimulatorJSONStorable[] array4 = psjs;
		foreach (PhysicsSimulatorJSONStorable ps2 in array4)
		{
			containingAtom.RegisterDynamicPhysicsSimulatorJSONStorable(ps2);
		}
		if (scrs == null)
		{
			scrs = instance.GetComponentsInChildren<ScaleChangeReceiver>(true);
		}
		ScaleChangeReceiver[] array5 = scrs;
		foreach (ScaleChangeReceiver scr in array5)
		{
			containingAtom.RegisterDynamicScaleChangeReceiver(scr);
		}
		if (scrjss == null)
		{
			scrjss = instance.GetComponentsInChildren<ScaleChangeReceiverJSONStorable>(true);
		}
		ScaleChangeReceiverJSONStorable[] array6 = scrjss;
		foreach (ScaleChangeReceiverJSONStorable scr2 in array6)
		{
			containingAtom.RegisterDynamicScaleChangeReceiverJSONStorable(scr2);
		}
		if (rss == null)
		{
			rss = instance.GetComponentsInChildren<RenderSuspend>(true);
		}
		RenderSuspend[] array7 = rss;
		foreach (RenderSuspend rs in array7)
		{
			containingAtom.RegisterDynamicRenderSuspend(rs);
		}
	}

	protected void UnregisterDynamicObjs()
	{
		if (!(containingAtom != null) || !(instance != null))
		{
			return;
		}
		isRegistered = false;
		if (jss != null)
		{
			JSONStorable[] array = jss;
			foreach (JSONStorable jSONStorable in array)
			{
				if (!jSONStorable.exclude)
				{
					containingAtom.UnregisterAdditionalStorable(jSONStorable);
				}
			}
		}
		if (cvs != null)
		{
			Canvas[] array2 = cvs;
			foreach (Canvas c in array2)
			{
				containingAtom.RemoveCanvas(c);
			}
		}
		if (pss != null)
		{
			PhysicsSimulator[] array3 = pss;
			foreach (PhysicsSimulator ps in array3)
			{
				containingAtom.DeregisterDynamicPhysicsSimulator(ps);
			}
		}
		if (psjs != null)
		{
			PhysicsSimulatorJSONStorable[] array4 = psjs;
			foreach (PhysicsSimulatorJSONStorable ps2 in array4)
			{
				containingAtom.DeregisterDynamicPhysicsSimulatorJSONStorable(ps2);
			}
		}
		if (scrs != null)
		{
			ScaleChangeReceiver[] array5 = scrs;
			foreach (ScaleChangeReceiver scr in array5)
			{
				containingAtom.DeregisterDynamicScaleChangeReceiver(scr);
			}
		}
		if (scrjss != null)
		{
			ScaleChangeReceiverJSONStorable[] array6 = scrjss;
			foreach (ScaleChangeReceiverJSONStorable scr2 in array6)
			{
				containingAtom.DeregisterDynamicScaleChangeReceiverJSONStorable(scr2);
			}
		}
		if (rss != null)
		{
			RenderSuspend[] array7 = rss;
			foreach (RenderSuspend rs in array7)
			{
				containingAtom.DeregisterDynamicRenderSuspend(rs);
			}
		}
	}

	public void ResetUnregisteredInstance()
	{
		if (instance != null && !isRegistered)
		{
			JSONStorable[] componentsInChildren = instance.GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array = componentsInChildren;
			foreach (JSONStorable jSONStorable in array)
			{
				jSONStorable.RestoreAllFromDefaults();
			}
		}
	}

	public void PostLoadJSONRestore()
	{
		if (containingAtom != null && instance != null && needsPostLoadJSONRestore)
		{
			JSONStorable[] componentsInChildren = instance.GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array = componentsInChildren;
			foreach (JSONStorable js in array)
			{
				containingAtom.RestoreFromLast(js);
			}
			needsPostLoadJSONRestore = false;
		}
	}

	protected void ClearLoadFlag()
	{
		if (loadFlag != null)
		{
			loadFlag.Raise();
			loadFlag = null;
		}
	}

	protected IEnumerator LoadFromBundleAsync()
	{
		yield return SuperController.AssetManagerReady();
		loadFlag = new AsyncFlag("Load " + assetName);
		SuperController.singleton.SetLoadingIconFlag(loadFlag);
		yield return null;
		float startTime = Time.realtimeSinceStartup;
		GameObject go = null;
		if (SuperController.singleton != null)
		{
			go = SuperController.singleton.GetCachedPrefab(assetBundleName, assetName);
			if (go != null)
			{
				didRegisterBundle = true;
			}
		}
		if (go == null)
		{
			if (loadRequest == null)
			{
				loadRequest = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
				if (loadRequest == null)
				{
					Debug.LogError("Failed to load asset " + assetName);
					yield break;
				}
			}
			didStartLoadFromBundleAsync = true;
			yield return StartCoroutine(loadRequest);
			go = loadRequest.GetAsset<GameObject>();
			loadRequest = null;
			didStartLoadFromBundleAsync = false;
			if (go != null)
			{
				if (SuperController.singleton != null)
				{
					SuperController.singleton.RegisterPrefab(assetBundleName, assetName, go);
					didRegisterBundle = true;
				}
			}
			else
			{
				Debug.LogError("Asset " + assetName + " is missing game object");
			}
		}
		if (go != null)
		{
			startTime = Time.realtimeSinceStartup;
			instance = Object.Instantiate(go.transform);
			if (nameInstance)
			{
				instance.name = instanceName;
			}
			else
			{
				instance.name = instance.name.Replace("(Clone)", string.Empty);
			}
			instance.gameObject.SetActive(true);
			InitInstance();
			OnPreloadComplete();
			RegisterDynamicObjs();
		}
		OnLoadComplete();
		ClearLoadFlag();
	}

	protected void LoadFromPrefab()
	{
		instance = Object.Instantiate(prefab);
		if (nameInstance)
		{
			instance.name = instanceName;
		}
		else
		{
			instance.name = instance.name.Replace("(Clone)", string.Empty);
		}
		instance.gameObject.SetActive(true);
		InitInstance();
		OnPreloadComplete();
		RegisterDynamicObjs();
		OnLoadComplete();
	}

	protected IEnumerator LoadFromPrefabAsync()
	{
		loadFlag = new AsyncFlag("Load " + prefab.name);
		SuperController.singleton.SetLoadingIconFlag(loadFlag);
		yield return null;
		yield return null;
		LoadFromPrefab();
		ClearLoadFlag();
	}

	private IEnumerator LoadAsync()
	{
		yield return null;
		Scene sc = SceneManager.GetSceneByName(sceneName);
		if (!sc.IsValid() || !sc.isLoaded)
		{
			async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			yield return async;
			sc = SceneManager.GetSceneByName(sceneName);
		}
		if (sc.IsValid())
		{
			GameObject[] rootGameObjects = sc.GetRootGameObjects();
			if (rootGameObjects.Length > 0)
			{
				GameObject gameObject = rootGameObjects[0];
				instance = Object.Instantiate(gameObject.transform);
				if (nameInstance)
				{
					instance.name = instanceName;
				}
				else
				{
					instance.name = instance.name.Replace("(Clone)", string.Empty);
				}
				instance.gameObject.SetActive(true);
				gameObject.SetActive(false);
				InitInstance();
				OnPreloadComplete();
				RegisterDynamicObjs();
				OnLoadComplete();
			}
		}
		else
		{
			Debug.LogError("Could not open scene " + sceneName);
		}
	}

	protected virtual void Connect()
	{
	}

	protected virtual void OnPreloadComplete()
	{
		if (onPreloadedHandlers != null)
		{
			onPreloadedHandlers();
			onPreloadedHandlers = null;
		}
	}

	protected virtual void OnLoadComplete(bool skipJSONRestore = false)
	{
		Connect();
		ready = true;
		if (onLoadedHandlers != null)
		{
			onLoadedHandlers();
			onLoadedHandlers = null;
		}
		PostLoadJSONRestore();
	}

	protected virtual void OnEnable()
	{
		if (instance == null)
		{
			if (assetBundleName != null && assetBundleName != string.Empty)
			{
				if (assetName == null || assetName == string.Empty)
				{
					assetName = base.transform.name;
				}
				if (Application.isPlaying)
				{
					StartCoroutine(LoadFromBundleAsync());
				}
			}
			else if (sceneName != null && sceneName != string.Empty)
			{
				if (Application.isPlaying)
				{
					StartCoroutine(LoadAsync());
				}
			}
			else if (prefab != null)
			{
				if (Application.isPlaying)
				{
					StartCoroutine(LoadFromPrefabAsync());
				}
				else
				{
					LoadFromPrefab();
				}
			}
			else
			{
				OnPreloadComplete();
				OnLoadComplete(true);
			}
		}
		else
		{
			OnPreloadComplete();
			RegisterDynamicObjs();
			OnLoadComplete();
		}
	}

	protected void UnloadInstance()
	{
		if (instance != null)
		{
			Debug.Log("Unload " + containingAtom.name + " " + instance.name);
			Object.Destroy(instance.gameObject);
			instance = null;
			jss = null;
			cvs = null;
			pss = null;
			psjs = null;
			scrs = null;
			scrjss = null;
			rss = null;
			ready = false;
			if (didRegisterBundle)
			{
				didRegisterBundle = false;
				SuperController.singleton.UnregisterPrefab(assetBundleName, assetName);
			}
			else if (didStartLoadFromBundleAsync)
			{
				didStartLoadFromBundleAsync = false;
				loadRequest = null;
				AssetBundleManager.UnloadAssetBundle(assetBundleName);
			}
		}
	}

	public void UnloadIfInactive()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			UnloadInstance();
		}
	}

	public void UnloadIfNotEnabled()
	{
		if (!base.enabled)
		{
			UnloadInstance();
		}
	}

	protected virtual void OnDisable()
	{
		ClearLoadFlag();
		if (instance != null)
		{
			UnregisterDynamicObjs();
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(instance.gameObject);
				instance = null;
				ready = false;
			}
			else if (!neverUnloadOnDisable && unloadOnDisable && containingAtom.on && containingAtom.gameObject.activeSelf)
			{
				UnloadInstance();
			}
			if (onPreloadedHandlers != null)
			{
				onPreloadedHandlers = null;
			}
			if (onLoadedHandlers != null)
			{
				onLoadedHandlers = null;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		ClearLoadFlag();
		StopAllCoroutines();
		if (didRegisterBundle)
		{
			SuperController.singleton.UnregisterPrefab(assetBundleName, assetName);
		}
		else if (didStartLoadFromBundleAsync)
		{
			AssetBundleManager.UnloadAssetBundle(assetBundleName);
		}
	}
}
