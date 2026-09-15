using System;
using System.Collections;
using System.Text.RegularExpressions;
using OldMoatGames;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using mset;

public class SkyshopLightController : JSONStorable
{
	public static SkyshopLightController singleton;

	protected string[] customParamNames = new string[1] { "skyName" };

	public SkyManager skyManager;

	public Transform autoSkiesContainer;

	public Sky[] skies;

	public Sky customSky;

	public Transform skyContainer;

	public Transform skyContainer2;

	public UIPopup skyPopup;

	public Transform overlaySkyContainer;

	public Sky overlaySky;

	protected bool _overlaySkyActive;

	protected Sky _startingSky;

	protected Sky _currentSky;

	protected string _skyName;

	public float minMasterIntensity;

	public float maxMasterIntensity = 10f;

	public float minDiffuseIntensity;

	public float maxDiffuseIntensity = 10f;

	public float minSpecularIntensity;

	public float maxSpecularIntensity = 10f;

	public float minCamExposure;

	public float maxCamExposure = 10f;

	protected JSONStorableFloat masterIntensityJSON;

	protected JSONStorableFloat diffuseIntensityJSON;

	protected JSONStorableFloat specularIntensityJSON;

	protected JSONStorableFloat unityAmbientIntensityJSON;

	protected JSONStorableColor unityAmbientColorJSON;

	protected JSONStorableFloat camExposureJSON;

	[SerializeField]
	protected bool _showSkyBox;

	protected JSONStorableBool showSkyboxJSON;

	protected JSONStorableFloat skyboxIntensityJSON;

	protected JSONStorableAngle skyboxYAngleJSON;

	public int flashFrames = 45;

	public float flashIntensity = 10f;

	protected float flashDecerement = 1f;

	protected bool notActiveOnSync;

	public InputFieldAction urlInputFieldAction;

	public InputField urlInputField;

	public InputFieldAction urlInputFieldActionAlt;

	public InputField urlInputFieldAlt;

	[SerializeField]
	protected string _url;

	public Button loadButton;

	public Button loadButtonAlt;

	public Button copyToClipboardButton;

	public Button copyToClipboardButtonAlt;

	public Button copyFromClipboardButton;

	public Button copyFromClipboardButtonAlt;

	public Button fileBrowseButton;

	public Button fileBrowseButtonAlt;

	public bool overlaySkyActive
	{
		get
		{
			return _overlaySkyActive;
		}
		set
		{
			if (_overlaySkyActive != value)
			{
				_overlaySkyActive = value;
				if (overlaySkyContainer != null)
				{
					overlaySkyContainer.gameObject.SetActive(_overlaySkyActive);
				}
				SyncGlobalSky();
			}
		}
	}

	public string skyName
	{
		get
		{
			return _skyName;
		}
		set
		{
			if (!(_skyName != value))
			{
				return;
			}
			Sky[] array = skies;
			foreach (Sky sky in array)
			{
				if (sky.name == value)
				{
					_skyName = value;
					_currentSky = sky;
					SyncCurrentSky();
					SyncGlobalSky();
					if (skyPopup != null)
					{
						skyPopup.currentValueNoCallback = _skyName;
					}
					break;
				}
			}
		}
	}

	public string url
	{
		get
		{
			return _url;
		}
		set
		{
			if (_url != value)
			{
				_url = value;
				if (urlInputField != null)
				{
					urlInputField.text = _url;
				}
				if (urlInputFieldAlt != null)
				{
					urlInputFieldAlt.text = _url;
				}
				StartSyncImage();
			}
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includeAppearance || forceStore) && (_currentSky != _startingSky || forceStore))
		{
			jSON["skyName"] = _skyName;
			needsStore = true;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.appearanceLocked && restoreAppearance && !IsCustomPhysicalParamLocked("skyName"))
		{
			if (jc["skyName"] != null)
			{
				SetSky(jc["skyName"]);
			}
			else if (setMissingToDefault)
			{
				skyName = _startingSky.name;
			}
		}
	}

	protected void SyncCurrentSky()
	{
		if (_currentSky != null)
		{
			_currentSky.MasterIntensity = masterIntensityJSON.val;
			_currentSky.DiffIntensity = diffuseIntensityJSON.val;
			_currentSky.SpecIntensity = specularIntensityJSON.val;
			_currentSky.DiffIntensityLM = unityAmbientIntensityJSON.val;
			_currentSky.CamExposure = camExposureJSON.val;
			_currentSky.SkyIntensity = skyboxIntensityJSON.val;
		}
	}

	protected void SyncGlobalSky()
	{
		if (skyManager != null)
		{
			if (overlaySkyActive && overlaySky != null)
			{
				skyManager.GlobalSky = overlaySky;
			}
			else
			{
				skyManager.GlobalSky = _currentSky;
			}
		}
	}

	protected void SetSky(string sname)
	{
		skyName = sname;
	}

	protected void SyncMasterIntensity(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.MasterIntensity = f;
			_currentSky.Apply();
		}
	}

	protected void SyncDiffuseIntensity(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.DiffIntensity = f;
			_currentSky.Apply();
		}
	}

	protected void SyncSpecularIntensity(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.SpecIntensity = f;
			_currentSky.Apply();
		}
	}

	protected void SyncUnityAmbientIntensity(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.DiffIntensityLM = f;
			_currentSky.Apply();
		}
	}

	protected void SyncCamExposure(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.CamExposure = f;
			_currentSky.Apply();
		}
	}

	protected void SyncUnityAmbientColor(float h, float s, float v)
	{
		Color ambientLight = HSVColorPicker.HSVToRGB(h, s, v);
		RenderSettings.ambientLight = ambientLight;
	}

	protected void SyncShowSkybox(bool b)
	{
		_showSkyBox = b;
		if (skyContainer != null)
		{
			skyContainer.gameObject.SetActive(b);
		}
	}

	protected void SyncSkyboxIntensity(float f)
	{
		if (_currentSky != null)
		{
			_currentSky.SkyIntensity = f;
			_currentSky.Apply();
		}
	}

	protected void SyncSkyboxYAngle(float f)
	{
		if (skyContainer != null)
		{
			Vector3 localEulerAngles = skyContainer.localEulerAngles;
			localEulerAngles.y = f;
			skyContainer.localEulerAngles = localEulerAngles;
		}
		if (skyContainer2 != null)
		{
			Vector3 localEulerAngles2 = skyContainer2.localEulerAngles;
			localEulerAngles2.y = f;
			skyContainer2.localEulerAngles = localEulerAngles2;
		}
	}

	private IEnumerator FlashIter()
	{
		if (skyManager != null && skyManager.GlobalSky != null && camExposureJSON != null)
		{
			float currentFlashIntensity = flashIntensity;
			float flashDecerement = (currentFlashIntensity - camExposureJSON.val) / (float)flashFrames;
			for (int i = 0; i < flashFrames; i++)
			{
				skyManager.GlobalSky.CamExposure = currentFlashIntensity;
				skyManager.GlobalSky.Apply();
				currentFlashIntensity -= flashDecerement;
				yield return null;
			}
			skyManager.GlobalSky.CamExposure = camExposureJSON.val;
			skyManager.GlobalSky.Apply();
		}
	}

	public void Flash()
	{
		StartCoroutine(FlashIter());
	}

	private IEnumerator SyncImage()
	{
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, false)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		string urltoload = _url;
		if (!Regex.IsMatch(urltoload, "^http") && !Regex.IsMatch(urltoload, "^file"))
		{
			urltoload = ((!urltoload.Contains(":/")) ? ("file:///.\\" + urltoload) : ("file:///" + urltoload));
		}
		WWW www = new WWW(urltoload);
		yield return www;
		if (www.error == null || www.error == string.Empty)
		{
			www.LoadImageIntoTexture(tex);
		}
		else
		{
			SuperController.LogError("Could not load image at " + urltoload + " Error: " + www.error);
		}
	}

	public void StartSyncImage()
	{
		if (UserPreferences.singleton == null || UserPreferences.singleton.enableWebMisc || !Regex.IsMatch(url, "^http"))
		{
			if (base.gameObject.activeInHierarchy)
			{
				AnimatedGifPlayer component = GetComponent<AnimatedGifPlayer>();
				if (_url.EndsWith(".gif"))
				{
					if (component != null)
					{
						component.enabled = true;
						component.FileName = url;
						component.Init();
					}
				}
				else if (_url.EndsWith(".jpg") || _url.EndsWith(".jpeg") || _url.EndsWith(".png"))
				{
					if (component != null)
					{
						component.enabled = false;
					}
					StartCoroutine(SyncImage());
				}
				else
				{
					Debug.LogError("Unknown image type for url " + _url);
				}
			}
			else
			{
				notActiveOnSync = true;
			}
		}
		else if (UserPreferences.singleton == null || !UserPreferences.singleton.hideDisabledWebMessages)
		{
			SuperController.LogError("Attempted to load http URL image when web load option is disabled. To enable, see User Preferences -> Web Security tab");
			SuperController.singleton.ShowMainHUDAuto();
			SuperController.singleton.SetActiveUI("MainMenu");
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}
	}

	public void CopyURLToClipboard()
	{
		GUIUtility.systemCopyBuffer = _url;
	}

	public void CopyURLFromClipboard()
	{
		url = GUIUtility.systemCopyBuffer;
	}

	public void SetFilePath(string path)
	{
		if (path != null && path != string.Empty)
		{
			path = SuperController.singleton.NormalizeMediaPath(path);
			url = path;
		}
	}

	public void FileBrowse()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetMediaPathDialog(SetFilePath, "jpg|jpeg|png|gif");
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		SkyshopLightControllerUI componentInChildren = UITransform.GetComponentInChildren<SkyshopLightControllerUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		if (masterIntensityJSON != null)
		{
			masterIntensityJSON.slider = componentInChildren.masterIntensitySlider;
		}
		if (diffuseIntensityJSON != null)
		{
			diffuseIntensityJSON.slider = componentInChildren.diffuseIntensitySlider;
		}
		if (specularIntensityJSON != null)
		{
			specularIntensityJSON.slider = componentInChildren.specularIntensitySlider;
		}
		if (unityAmbientIntensityJSON != null)
		{
			unityAmbientIntensityJSON.slider = componentInChildren.unityAmbientIntensitySlider;
		}
		if (unityAmbientColorJSON != null)
		{
			unityAmbientColorJSON.colorPicker = componentInChildren.unityAmbientColorPicker;
		}
		if (camExposureJSON != null)
		{
			camExposureJSON.slider = componentInChildren.camExposureSlider;
		}
		if (showSkyboxJSON != null)
		{
			showSkyboxJSON.toggle = componentInChildren.showSkyboxToggle;
		}
		if (skyboxIntensityJSON != null)
		{
			skyboxIntensityJSON.slider = componentInChildren.skyboxIntensitySlider;
		}
		if (skyboxYAngleJSON != null)
		{
			skyboxYAngleJSON.slider = componentInChildren.skyboxYAngleSlider;
		}
		skyPopup = componentInChildren.skyPopup;
		if (skyPopup != null)
		{
			skyPopup.numPopupValues = skies.Length;
			int num = 0;
			Sky[] array = skies;
			foreach (Sky sky in array)
			{
				skyPopup.setPopupValue(num, sky.name);
				num++;
			}
			skyPopup.currentValue = _skyName;
			UIPopup uIPopup = skyPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetSky));
		}
	}

	protected void Init()
	{
		singleton = this;
		if (skyManager != null && skyManager.GlobalSky != null)
		{
			if (autoSkiesContainer != null)
			{
				skies = autoSkiesContainer.GetComponentsInChildren<Sky>();
			}
			_startingSky = skyManager.GlobalSky;
			_currentSky = _startingSky;
			_skyName = _currentSky.name;
			masterIntensityJSON = new JSONStorableFloat("masterIntensity", skyManager.GlobalSky.MasterIntensity, SyncMasterIntensity, minMasterIntensity, maxMasterIntensity);
			RegisterFloat(masterIntensityJSON);
			diffuseIntensityJSON = new JSONStorableFloat("diffuseIntensity", skyManager.GlobalSky.DiffIntensity, SyncDiffuseIntensity, minDiffuseIntensity, maxDiffuseIntensity);
			RegisterFloat(diffuseIntensityJSON);
			specularIntensityJSON = new JSONStorableFloat("specularIntensity", skyManager.GlobalSky.SpecIntensity, SyncSpecularIntensity, minSpecularIntensity, maxSpecularIntensity);
			RegisterFloat(specularIntensityJSON);
			unityAmbientIntensityJSON = new JSONStorableFloat("unityAmbientIntensity", skyManager.GlobalSky.DiffIntensityLM, SyncUnityAmbientIntensity, 0f, 1f);
			RegisterFloat(unityAmbientIntensityJSON);
			Color ambientLight = RenderSettings.ambientLight;
			unityAmbientColorJSON = new JSONStorableColor("unityAmbientColor", HSVColorPicker.RGBToHSV(ambientLight.r, ambientLight.g, ambientLight.b), SyncUnityAmbientColor);
			RegisterColor(unityAmbientColorJSON);
			camExposureJSON = new JSONStorableFloat("camExposure", skyManager.GlobalSky.CamExposure, SyncCamExposure, minCamExposure, maxCamExposure);
			RegisterFloat(camExposureJSON);
			showSkyboxJSON = new JSONStorableBool("showSkybox", _showSkyBox, SyncShowSkybox);
			RegisterBool(showSkyboxJSON);
			skyboxIntensityJSON = new JSONStorableFloat("skyboxIntensity", skyManager.GlobalSky.SkyIntensity, SyncSkyboxIntensity, minMasterIntensity, maxMasterIntensity);
			RegisterFloat(skyboxIntensityJSON);
			float v = 0f;
			if (skyContainer != null)
			{
				v = skyContainer.localEulerAngles.y;
			}
			skyboxYAngleJSON = new JSONStorableAngle("skyboxYAngle", v, SyncSkyboxYAngle);
			RegisterFloat(skyboxYAngleJSON);
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
		}
	}
}
