using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : JSONStorable
{
	public enum MaskToShow
	{
		mask1,
		mask2,
		mask3,
		mask4
	}

	public Camera cameraToControl;

	public Camera cameraToControl2;

	public LayerMask mask1;

	public LayerMask mask2;

	public LayerMask mask3;

	public LayerMask mask4;

	public string mask1Name;

	public string mask2Name;

	public string mask3Name;

	public string mask4Name;

	public Transform cameraGroup;

	public GameObject otherObjectToControl;

	protected bool cameraOn;

	protected JSONStorableBool cameraOnJSON;

	public AudioListener audioListener;

	public bool useAudioListener;

	protected JSONStorableBool useAudioListenerJSON;

	public bool useAsMainCamera;

	protected JSONStorableBool useAsMainCameraJSON;

	protected JSONStorableStringChooser maskSelectionJSON;

	[SerializeField]
	protected MaskToShow _maskSelection;

	protected JSONStorableFloat cameraFOVJSON;

	[SerializeField]
	private float _cameraFOV = 40f;

	public GameObject HUDView;

	protected JSONStorableBool showHUDViewJSON;

	[SerializeField]
	protected bool _showHUDView;

	public MaskToShow maskSelection
	{
		get
		{
			return _maskSelection;
		}
		set
		{
			if (maskSelectionJSON != null)
			{
				maskSelectionJSON.val = value.ToString();
			}
			else if (_maskSelection != value)
			{
				SetMaskSelection(value.ToString());
			}
		}
	}

	public float cameraFOV
	{
		get
		{
			return _cameraFOV;
		}
		set
		{
			if (cameraFOVJSON != null)
			{
				cameraFOVJSON.val = value;
			}
			else if (_cameraFOV != value)
			{
				SyncCameraFOV(value);
			}
		}
	}

	public bool showHUDView
	{
		get
		{
			return _showHUDView;
		}
		set
		{
			if (showHUDViewJSON != null)
			{
				showHUDViewJSON.val = value;
			}
			else if (_showHUDView != value)
			{
				SyncShowHUDView(value);
			}
		}
	}

	protected void SyncCamera()
	{
		if (!(cameraToControl != null))
		{
			return;
		}
		switch (_maskSelection)
		{
		case MaskToShow.mask1:
			cameraToControl.cullingMask = mask1;
			if (cameraToControl2 != null)
			{
				cameraToControl2.cullingMask = mask1;
			}
			break;
		case MaskToShow.mask2:
			cameraToControl.cullingMask = mask2;
			if (cameraToControl2 != null)
			{
				cameraToControl2.cullingMask = mask2;
			}
			break;
		case MaskToShow.mask3:
			cameraToControl.cullingMask = mask3;
			if (cameraToControl2 != null)
			{
				cameraToControl2.cullingMask = mask3;
			}
			break;
		case MaskToShow.mask4:
			cameraToControl.cullingMask = mask4;
			if (cameraToControl2 != null)
			{
				cameraToControl2.cullingMask = mask4;
			}
			break;
		}
		cameraToControl.fieldOfView = _cameraFOV;
		if (cameraToControl2 != null)
		{
			cameraToControl2.fieldOfView = _cameraFOV;
		}
	}

	protected void SyncCameraOnAndUseAudioListener()
	{
		if (cameraGroup != null)
		{
			cameraGroup.gameObject.SetActive(cameraOn);
		}
		if (otherObjectToControl != null)
		{
			otherObjectToControl.SetActive(cameraOn);
		}
		if (audioListener != null && SuperController.singleton != null)
		{
			if (useAudioListener && cameraOn)
			{
				SuperController.singleton.PushAudioListener(audioListener);
			}
			else
			{
				SuperController.singleton.RemoveAudioListener(audioListener);
			}
		}
	}

	protected void SyncCameraOn(bool b)
	{
		cameraOn = b;
		SyncCameraOnAndUseAudioListener();
	}

	protected void SyncUseAudioListener(bool b)
	{
		useAudioListener = b;
		SyncCameraOnAndUseAudioListener();
	}

	protected void SyncUseAsMainCamera(bool b)
	{
		useAsMainCamera = b;
		if (cameraToControl != null)
		{
			if (useAsMainCamera)
			{
				cameraToControl.tag = "MainCamera";
			}
			else
			{
				cameraToControl.tag = "Untagged";
			}
		}
	}

	protected void SetMaskSelection(string maskSel)
	{
		try
		{
			MaskToShow maskToShow = (MaskToShow)Enum.Parse(typeof(MaskToShow), maskSel);
			_maskSelection = maskToShow;
			SyncCamera();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set mask type to " + maskSel + " which is not a valid mask type");
		}
	}

	protected void SyncCameraFOV(float f)
	{
		_cameraFOV = f;
		SyncCamera();
	}

	protected void SyncShowHUDView(bool b)
	{
		_showHUDView = b;
		if (HUDView != null)
		{
			HUDView.SetActive(_showHUDView);
		}
	}

	protected void Init()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (mask1Name != null && mask1Name != string.Empty)
		{
			list.Add("mask1");
			list2.Add(mask1Name);
		}
		if (mask2Name != null && mask2Name != string.Empty)
		{
			list.Add("mask2");
			list2.Add(mask2Name);
		}
		if (mask3Name != null && mask3Name != string.Empty)
		{
			list.Add("mask3");
			list2.Add(mask3Name);
		}
		if (mask4Name != null && mask4Name != string.Empty)
		{
			list.Add("mask4");
			list2.Add(mask4Name);
		}
		cameraOnJSON = new JSONStorableBool("cameraOn", cameraOn, SyncCameraOn);
		cameraOnJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterBool(cameraOnJSON);
		useAudioListenerJSON = new JSONStorableBool("useAudioListener", useAudioListener, SyncUseAudioListener);
		useAudioListenerJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterBool(useAudioListenerJSON);
		useAsMainCameraJSON = new JSONStorableBool("useAsMainCamera", useAsMainCamera, SyncUseAsMainCamera);
		useAsMainCameraJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterBool(useAsMainCameraJSON);
		SyncCameraOnAndUseAudioListener();
		maskSelectionJSON = new JSONStorableStringChooser("maskSelection", list, list2, _maskSelection.ToString(), null, SetMaskSelection);
		maskSelectionJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterStringChooser(maskSelectionJSON);
		cameraFOVJSON = new JSONStorableFloat("FOV", _cameraFOV, SyncCameraFOV, 10f, 100f);
		cameraFOVJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterFloat(cameraFOVJSON);
		SyncCamera();
		showHUDViewJSON = new JSONStorableBool("showHUDView", _showHUDView, SyncShowHUDView);
		showHUDViewJSON.storeType = JSONStorableParam.StoreType.Any;
		RegisterBool(showHUDViewJSON);
		SyncShowHUDView(_showHUDView);
	}

	public override void InitUI()
	{
		base.InitUI();
		if (UITransform != null)
		{
			CameraControlUI componentInChildren = UITransform.GetComponentInChildren<CameraControlUI>(true);
			if (componentInChildren != null)
			{
				cameraOnJSON.toggle = componentInChildren.cameraOnToggle;
				useAudioListenerJSON.toggle = componentInChildren.useAudioListenerToggle;
				useAsMainCameraJSON.toggle = componentInChildren.useAsMainCameraToggle;
				maskSelectionJSON.popup = componentInChildren.maskPopup;
				cameraFOVJSON.slider = componentInChildren.FOVSlider;
				showHUDViewJSON.toggle = componentInChildren.showHUDViewToggle;
			}
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (UITransformAlt != null)
		{
			CameraControlUI componentInChildren = UITransformAlt.GetComponentInChildren<CameraControlUI>(true);
			if (componentInChildren != null)
			{
				cameraOnJSON.toggleAlt = componentInChildren.cameraOnToggle;
				useAudioListenerJSON.toggleAlt = componentInChildren.useAudioListenerToggle;
				useAsMainCameraJSON.toggleAlt = componentInChildren.useAsMainCameraToggle;
				maskSelectionJSON.popupAlt = componentInChildren.maskPopup;
				cameraFOVJSON.sliderAlt = componentInChildren.FOVSlider;
				showHUDViewJSON.toggleAlt = componentInChildren.showHUDViewToggle;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}

	protected void OnDestroy()
	{
		if (useAudioListener && audioListener != null && SuperController.singleton != null)
		{
			SuperController.singleton.RemoveAudioListener(audioListener);
		}
	}
}
