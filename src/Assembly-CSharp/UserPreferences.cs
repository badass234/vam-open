using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DynamicCSharp;
using MK.Glow;
using MVR.FileManagement;
using MVR.Hub;
using MeshVR;
using MeshVR.Hands;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using ZenFulcrum.EmbeddedBrowser;

[ExecuteInEditMode]
public class UserPreferences : MonoBehaviour
{
	private class QualityLevel
	{
		public float renderScale;

		public int msaaLevel;

		public int pixelLightCount;

		public ShaderLOD shaderLOD;

		public int smoothPasses;

		public bool mirrorReflections;

		public bool realtimeReflectionProbes;

		public bool closeObjectBlur;

		public bool softPhysics;

		public GlowEffectsLevel glowEffects;
	}

	private class QualityLevels
	{
		public static Dictionary<string, QualityLevel> levels = new Dictionary<string, QualityLevel>
		{
			{
				"UltraLow",
				new QualityLevel
				{
					renderScale = 1f,
					msaaLevel = 0,
					pixelLightCount = 0,
					shaderLOD = ShaderLOD.Low,
					smoothPasses = 1,
					mirrorReflections = false,
					realtimeReflectionProbes = false,
					closeObjectBlur = false,
					softPhysics = false,
					glowEffects = GlowEffectsLevel.Off
				}
			},
			{
				"Low",
				new QualityLevel
				{
					renderScale = 1f,
					msaaLevel = 2,
					pixelLightCount = 1,
					shaderLOD = ShaderLOD.Low,
					smoothPasses = 1,
					mirrorReflections = false,
					realtimeReflectionProbes = false,
					closeObjectBlur = false,
					softPhysics = false,
					glowEffects = GlowEffectsLevel.Off
				}
			},
			{
				"Mid",
				new QualityLevel
				{
					renderScale = 1f,
					msaaLevel = 4,
					pixelLightCount = 2,
					shaderLOD = ShaderLOD.High,
					smoothPasses = 1,
					mirrorReflections = false,
					realtimeReflectionProbes = false,
					closeObjectBlur = false,
					softPhysics = true,
					glowEffects = GlowEffectsLevel.Low
				}
			},
			{
				"High",
				new QualityLevel
				{
					renderScale = 1f,
					msaaLevel = 4,
					pixelLightCount = 2,
					shaderLOD = ShaderLOD.High,
					smoothPasses = 2,
					mirrorReflections = true,
					realtimeReflectionProbes = true,
					closeObjectBlur = false,
					softPhysics = true,
					glowEffects = GlowEffectsLevel.Low
				}
			},
			{
				"Ultra",
				new QualityLevel
				{
					renderScale = 1.5f,
					msaaLevel = 2,
					pixelLightCount = 3,
					shaderLOD = ShaderLOD.High,
					smoothPasses = 3,
					mirrorReflections = true,
					realtimeReflectionProbes = true,
					closeObjectBlur = true,
					softPhysics = true,
					glowEffects = GlowEffectsLevel.High
				}
			},
			{
				"Max",
				new QualityLevel
				{
					renderScale = 2f,
					msaaLevel = 2,
					pixelLightCount = 4,
					shaderLOD = ShaderLOD.High,
					smoothPasses = 4,
					mirrorReflections = true,
					realtimeReflectionProbes = true,
					closeObjectBlur = true,
					softPhysics = true,
					glowEffects = GlowEffectsLevel.High
				}
			}
		};
	}

	public enum ShaderLOD
	{
		Low = 250,
		Medium = 400,
		High = 600
	}

	public enum GlowEffectsLevel
	{
		Off,
		Low,
		High
	}

	public enum PhysicsRate
	{
		Auto,
		_45,
		_60,
		_72,
		_80,
		_90,
		_120,
		_144,
		_240,
		_288
	}

	public enum SortBy
	{
		AtoZ,
		ZtoA,
		NewToOld,
		OldToNew,
		NewToOldPackage,
		OldToNewPackage
	}

	public enum DirectoryOption
	{
		ShowFirst,
		ShowLast,
		Intermix,
		Hide
	}

	public static UserPreferences singleton;

	private bool _disableSave;

	private bool _disableToggles;

	public Toggle ultraLowQualityToggle;

	public Toggle lowQualityToggle;

	public Toggle midQualityToggle;

	public Toggle highQualityToggle;

	public Toggle ultraQualityToggle;

	public Toggle maxQualityToggle;

	public Toggle customQualityToggle;

	public bool loadPrefsFileOnStart = true;

	public LeapHandModelControl leapHandModelControl;

	public HandModelControl motionHandModelControl;

	public HandModelControl alternateMotionHandModelControl;

	public Slider renderScaleSlider;

	[SerializeField]
	private float _renderScale = 1f;

	public UIPopup msaaPopup;

	[SerializeField]
	private int _msaaLevel = 8;

	public GameObject[] firstTimeUserEnableGameObjects;

	public GameObject[] firstTimeUserDisableGameObjects;

	[SerializeField]
	private bool _firstTimeUser = true;

	[SerializeField]
	private string termsOfUsePath;

	public Button reviewTermsButton;

	public GameObject termsNotAcceptedGameObject;

	public Button termsAndSettingsAcceptedButton;

	public Toggle termsOfUseAcceptedToggle;

	[SerializeField]
	private bool _termsOfUseAccepted;

	public Toggle desktopVsyncToggle;

	[SerializeField]
	private bool _desktopVsync;

	public UIPopup smoothPassesPopup;

	[SerializeField]
	private int _smoothPasses = 3;

	public UIPopup pixelLightCountPopup;

	[SerializeField]
	private int _pixelLightCount = 4;

	public UIPopup shaderLODPopup;

	[SerializeField]
	private ShaderLOD _shaderLOD = ShaderLOD.High;

	[HideInInspector]
	public UIPopup screenResolutionPopup;

	[SerializeField]
	private string _screenResolution = string.Empty;

	private AlertUI _screenResolutionAlert;

	private int _pendingScreenResolutionWidth;

	private int _pendingScreenResolutionHeight;

	private int _syncedScreenResolutionWidth = -1;

	private int _syncedScreenResolutionHeight = -1;

	private bool _syncedScreenResolutionFullScreen;

	public Camera normalCamera;

	public Camera mirrorReflectionCamera1;

	public Camera mirrorReflectionCamera2;

	public Toggle mirrorReflectionsToggle;

	[SerializeField]
	private bool _mirrorReflections = true;

	public Toggle realtimeReflectionProbesToggle;

	[SerializeField]
	private bool _realtimeReflectionProbes = true;

	public Toggle mirrorToggle;

	[SerializeField]
	private bool _mirrorToDisplay;

	public GameObject exitButton;

	public Toggle hideExitButtonToggle;

	[SerializeField]
	private bool _hideExitButton;

	public Toggle showTargetsMenuOnlyToggle;

	[SerializeField]
	private bool _showTargetsMenuOnly = true;

	public Toggle alwaysShowPointersOnTouchToggle;

	[SerializeField]
	private bool _alwaysShowPointersOnTouch = true;

	public RectTransform enableWhenHideInactiveTargets;

	public RectTransform enableWhenShowInactiveTargets;

	public Toggle hideInactiveTargetsToggle;

	[SerializeField]
	private bool _hideInactiveTargets = true;

	public Toggle showControllersMenuOnlyToggle;

	[SerializeField]
	private bool _showControllersMenuOnly;

	public Slider targetAlphaSlider;

	[SerializeField]
	private float _targetAlpha = 1f;

	public Image crosshair;

	public Slider crosshairAlphaSlider;

	[SerializeField]
	private float _crosshairAlpha = 0.1f;

	public Toggle useMonitorViewOffsetWhenUIOpenToggle;

	[SerializeField]
	private bool _useMonitorViewOffsetWhenUIOpen = true;

	public SteamVR_RenderModel steamVRLeftControllerModel;

	public SteamVR_RenderModel steamVRRightControllerModel;

	public Toggle steamVRShowControllersToggle;

	[SerializeField]
	private bool _steamVRShowControllers;

	public SteamVR_Behaviour_Skeleton steamVRRightHandSkeleton;

	public SteamVR_Behaviour_Skeleton steamVRLeftHandSkeleton;

	public Toggle steamVRUseControllerHandPoseToggle;

	[SerializeField]
	private bool _steamVRUseControllerHandPose;

	public Transform steamVRLeftHandPointer;

	public Transform steamVRRightHandPointer;

	public Slider steamVRPointerAngleSlider;

	public float defaultSteamVRPointerAngle;

	[SerializeField]
	private float _steamVRPointerAngle;

	public HandInput steamVRLeftHandInput;

	public HandInput steamVRRightHandInput;

	public OVRHandInput ovrLeftHandInput;

	public OVRHandInput ovrRightHandInput;

	public Slider fingerInputFactorSlider;

	public float defaultFingerInputFactor = 0.25f;

	[SerializeField]
	private float _fingerInputFactor = 1f;

	public Slider thumbInputFactorSlider;

	public float defaultThumbInputFactor = 0.25f;

	[SerializeField]
	private float _thumbInputFactor = 1f;

	public Slider fingerSpreadOffsetSlider;

	public float defaultFingerSpreadOffset = -5f;

	[SerializeField]
	private float _fingerSpreadOffset = 1f;

	public Slider fingerBendOffsetSlider;

	public float defaultFingerBendOffset = 10f;

	[SerializeField]
	private float _fingerBendOffset = 1f;

	public Slider thumbSpreadOffsetSlider;

	public float defaultThumbSpreadOffset;

	[SerializeField]
	private float _thumbSpreadOffset = 1f;

	public Slider thumbBendOffsetSlider;

	public float defaultThumbBendOffset = 10f;

	[SerializeField]
	private float _thumbBendOffset = 1f;

	public Toggle oculusSwapGrabAndTriggerToggle;

	[SerializeField]
	private bool _oculusSwapGrabAndTrigger;

	public Toggle oculusDisableFreeMoveToggle;

	[SerializeField]
	private bool _oculusDisableFreeMove;

	public Slider pointLightShadowBlurSlider;

	[SerializeField]
	private float _pointLightShadowBlur = 0.5f;

	public Slider pointLightShadowBiasBaseSlider;

	[SerializeField]
	private float _pointLightShadowBiasBase = 0.01f;

	public Slider shadowFilterLevelSlider;

	[SerializeField]
	private float _shadowFilterLevel = 3f;

	public Toggle closeObjectBlurToggle;

	[SerializeField]
	private bool _closeObjectBlur = true;

	public Toggle softPhysicsToggle;

	[SerializeField]
	private bool _softPhysics = true;

	protected int glowObjectCount;

	protected List<MKGlow> dynamicGlow;

	public MKGlow[] glowObjects;

	private bool _pauseGlow;

	public UIPopup glowEffectsPopup;

	[SerializeField]
	private GlowEffectsLevel _glowEffects = GlowEffectsLevel.Low;

	public Text autoPhysicsRateText;

	public UIPopup physicsRatePopup;

	[SerializeField]
	private PhysicsRate _physicsRate;

	public UIPopup physicsUpdateCapPopup;

	[SerializeField]
	private int _physicsUpdateCap = 2;

	public Toggle physicsHighQualityToggle;

	[SerializeField]
	private bool _physicsHighQuality;

	public Transform headCollider;

	public Toggle useHeadColliderToggle;

	[SerializeField]
	private bool _useHeadCollider;

	public Toggle optimizeMemoryOnSceneLoadToggle;

	[SerializeField]
	private bool _optimizeMemoryOnSceneLoad = true;

	public Toggle optimizeMemoryOnPresetLoadToggle;

	[SerializeField]
	private bool _optimizeMemoryOnPresetLoad;

	public Toggle enableCachingToggle;

	[SerializeField]
	private bool _enableCaching = true;

	private string _cacheFolder;

	public Button browseCacheFolderButton;

	public Button resetCacheFolderButton;

	public Text cacheFolderText;

	public Toggle confirmLoadToggle;

	[SerializeField]
	private bool _confirmLoad;

	public ChildOrderFlip toolbarFlipper;

	public Toggle flipToolbarToggle;

	[SerializeField]
	private bool _flipToolbar;

	public Image panelForUIMaterial;

	public Shader overlayUIShader;

	public Shader defaultUIShader;

	public Toggle overlayUIToggle;

	[SerializeField]
	private bool _overlayUI = true;

	public Toggle enableWebBrowserToggle;

	public Toggle enableWebBrowserToggleAlt;

	[SerializeField]
	private bool _enableWebBrowser;

	protected HashSet<string> whitelistDomains;

	protected string[] whitelistDomainPaths = new string[2] { "whitelist_domains.json", "whitelist_domains_user.json" };

	public Toggle allowNonWhitelistDomainsToggle;

	public Toggle allowNonWhitelistDomainsToggleAlt;

	public Toggle allowNonWhitelistDomainsToggleAlt2;

	public Toggle allowNonWhitelistDomainsToggleAlt3;

	[SerializeField]
	private bool _allowNonWhitelistDomains;

	public Toggle enableWebBrowserProfileToggle;

	public Toggle enableWebBrowserProfileToggleAlt;

	[SerializeField]
	private bool _enableWebBrowserProfile = true;

	public Toggle enableWebMiscToggle;

	public Toggle enableWebMiscToggleAlt;

	[SerializeField]
	private bool _enableWebMisc;

	public Toggle enableHubToggle;

	public Toggle enableHubToggleAlt;

	[SerializeField]
	private bool _enableHub;

	public Toggle enableHubDownloaderToggle;

	public Toggle enableHubDownloaderToggleAlt;

	[SerializeField]
	private bool _enableHubDownloader;

	public Toggle enablePluginsToggle;

	public Toggle enablePluginsToggleAlt;

	[SerializeField]
	private bool _enablePlugins;

	[SerializeField]
	private Toggle allowPluginsNetworkAccessToggle;

	[SerializeField]
	private Toggle allowPluginsNetworkAccessToggleAlt;

	[SerializeField]
	private bool _allowPluginsNetworkAccess;

	public Toggle alwaysAllowPluginsDownloadedFromHubToggle;

	public Toggle alwaysAllowPluginsDownloadedFromHubToggleAlt;

	[SerializeField]
	private bool _alwaysAllowPluginsDownloadedFromHub;

	public Toggle hideDisabledWebMessagesToggle;

	[SerializeField]
	private bool _hideDisabledWebMessages;

	[SerializeField]
	private string _creatorName = "Anonymous";

	public InputField creatorNameInputField;

	private string _DAZExtraLibraryFolder = string.Empty;

	public Button browseDAZExtraLibraryFolderButton;

	public Button clearDAZExtraLibraryFolderButton;

	public Text DAZExtraLibraryFolderText;

	private string _DAZDefaultContentFolder = string.Empty;

	public Button browseDAZDefaultContentFolderButton;

	public Button clearDAZDefaultContentFolderButton;

	public Text DAZDefaultContentFolderText;

	protected SortBy _fileBrowserSortBy = SortBy.NewToOld;

	protected DirectoryOption _fileBrowserDirectoryOption;

	public bool shouldLoadPrefsFileOnStart
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.loadPrefsFileOnStart;
			}
			return loadPrefsFileOnStart;
		}
	}

	public float renderScale
	{
		get
		{
			return _renderScale;
		}
		set
		{
			if (_renderScale != value)
			{
				_renderScale = value;
				if (renderScaleSlider != null)
				{
					renderScaleSlider.value = value;
				}
				SyncRenderScale();
				SavePreferences();
			}
		}
	}

	public int msaaLevel
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overrideMsaaLevel)
			{
				return GlobalSceneOptions.singleton.msaaLevel;
			}
			return _msaaLevel;
		}
		set
		{
			if (_msaaLevel != value && (value == 0 || value == 2 || value == 4 || value == 8))
			{
				_msaaLevel = value;
				SyncMsaa();
				SyncMsaaPopup();
				SavePreferences();
			}
		}
	}

	public bool firstTimeUser
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.bypassFirstTimeUser)
			{
				return false;
			}
			return _firstTimeUser;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_firstTimeUser != value)
			{
				_firstTimeUser = value;
				SyncFirstTimeUser();
				SavePreferences();
			}
		}
	}

	public bool termsOfUseAccepted
	{
		get
		{
			return _termsOfUseAccepted;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_termsOfUseAccepted != value)
			{
				_termsOfUseAccepted = value;
				if (termsOfUseAcceptedToggle != null)
				{
					termsOfUseAcceptedToggle.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public bool desktopVsync
	{
		get
		{
			return _desktopVsync;
		}
		set
		{
			if (_desktopVsync != value)
			{
				_desktopVsync = value;
				if (desktopVsyncToggle != null)
				{
					desktopVsyncToggle.isOn = _desktopVsync;
				}
				SyncDesktopVsync();
				SavePreferences();
			}
		}
	}

	public int smoothPasses
	{
		get
		{
			return _smoothPasses;
		}
		set
		{
			if (_smoothPasses != value && value >= 0 && value <= 4)
			{
				_smoothPasses = value;
				SyncSmoothPassesPopup();
				SavePreferences();
			}
		}
	}

	public int pixelLightCount
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overridePixelLightCount)
			{
				return GlobalSceneOptions.singleton.pixelLightCount;
			}
			return _pixelLightCount;
		}
		set
		{
			if (_pixelLightCount != value)
			{
				_pixelLightCount = value;
				SyncPixelLightCount();
				if (pixelLightCountPopup != null)
				{
					pixelLightCountPopup.currentValue = pixelLightCount.ToString();
				}
				SavePreferences();
			}
		}
	}

	public ShaderLOD shaderLOD
	{
		get
		{
			return _shaderLOD;
		}
		set
		{
			if (_shaderLOD != value)
			{
				_shaderLOD = value;
				SetInternalShaderLOD();
				if (shaderLODPopup != null)
				{
					shaderLODPopup.currentValue = _shaderLOD.ToString();
				}
			}
		}
	}

	public string screenResolution
	{
		get
		{
			return _screenResolution;
		}
		set
		{
			SetScreenResolutionFromString(value);
		}
	}

	public bool mirrorReflections
	{
		get
		{
			return _mirrorReflections;
		}
		set
		{
			if (_mirrorReflections != value)
			{
				_mirrorReflections = value;
				if (mirrorReflectionsToggle != null)
				{
					mirrorReflectionsToggle.isOn = _mirrorReflections;
				}
				SyncMirrorReflections();
				SavePreferences();
			}
		}
	}

	public bool realtimeReflectionProbes
	{
		get
		{
			return _realtimeReflectionProbes;
		}
		set
		{
			if (_realtimeReflectionProbes != value)
			{
				_realtimeReflectionProbes = value;
				if (realtimeReflectionProbesToggle != null)
				{
					realtimeReflectionProbesToggle.isOn = _realtimeReflectionProbes;
				}
				SyncRealtimeReflectionProbes();
				SavePreferences();
			}
		}
	}

	public bool mirrorToDisplay
	{
		get
		{
			return _mirrorToDisplay;
		}
		set
		{
			if (_mirrorToDisplay != value)
			{
				_mirrorToDisplay = value;
				if (mirrorToggle != null)
				{
					mirrorToggle.isOn = _mirrorToDisplay;
				}
				SyncMirrorToDisplay();
				SavePreferences();
			}
		}
	}

	public bool hideExitButton
	{
		get
		{
			return _hideExitButton;
		}
		set
		{
			if (_hideExitButton != value)
			{
				_hideExitButton = value;
				if (hideExitButtonToggle != null)
				{
					hideExitButtonToggle.isOn = _hideExitButton;
				}
				SyncHideExitButton();
				SavePreferences();
			}
		}
	}

	public bool showTargetsMenuOnly
	{
		get
		{
			return _showTargetsMenuOnly;
		}
		set
		{
			if (_showTargetsMenuOnly != value)
			{
				_showTargetsMenuOnly = value;
				if (showTargetsMenuOnlyToggle != null)
				{
					showTargetsMenuOnlyToggle.isOn = _showTargetsMenuOnly;
				}
				SavePreferences();
			}
		}
	}

	public bool alwaysShowPointersOnTouch
	{
		get
		{
			return _alwaysShowPointersOnTouch;
		}
		set
		{
			if (_alwaysShowPointersOnTouch != value)
			{
				_alwaysShowPointersOnTouch = value;
				if (alwaysShowPointersOnTouchToggle != null)
				{
					alwaysShowPointersOnTouchToggle.isOn = _alwaysShowPointersOnTouch;
				}
				SavePreferences();
			}
		}
	}

	public bool hideInactiveTargets
	{
		get
		{
			return _hideInactiveTargets;
		}
		set
		{
			if (_hideInactiveTargets != value)
			{
				_hideInactiveTargets = value;
				if (hideInactiveTargetsToggle != null)
				{
					hideInactiveTargetsToggle.isOn = _hideInactiveTargets;
				}
				if (enableWhenHideInactiveTargets != null)
				{
					enableWhenHideInactiveTargets.gameObject.SetActive(_hideInactiveTargets);
				}
				if (enableWhenShowInactiveTargets != null)
				{
					enableWhenShowInactiveTargets.gameObject.SetActive(!_hideInactiveTargets);
				}
				if (SuperController.singleton != null)
				{
					SuperController.singleton.SyncVisibility();
				}
				SavePreferences();
			}
		}
	}

	public bool showControllersMenuOnly
	{
		get
		{
			return _showControllersMenuOnly;
		}
		set
		{
			if (_showControllersMenuOnly != value)
			{
				_showControllersMenuOnly = value;
				if (showControllersMenuOnlyToggle != null)
				{
					showControllersMenuOnlyToggle.isOn = _showControllersMenuOnly;
				}
				SavePreferences();
			}
		}
	}

	public float targetAlpha
	{
		get
		{
			return _targetAlpha;
		}
		set
		{
			if (_targetAlpha != value)
			{
				_targetAlpha = value;
				FreeControllerV3.targetAlpha = _targetAlpha;
				SelectTarget.useGlobalAlpha = true;
				SelectTarget.globalAlpha = _targetAlpha;
				if (targetAlphaSlider != null)
				{
					targetAlphaSlider.value = _targetAlpha;
				}
				SavePreferences();
			}
		}
	}

	public float crosshairAlpha
	{
		get
		{
			return _crosshairAlpha;
		}
		set
		{
			if (_crosshairAlpha != value)
			{
				_crosshairAlpha = value;
				SyncCrosshairAlpha();
				if (crosshairAlphaSlider != null)
				{
					crosshairAlphaSlider.value = _crosshairAlpha;
				}
				SavePreferences();
			}
		}
	}

	public bool useMonitorViewOffsetWhenUIOpen
	{
		get
		{
			return _useMonitorViewOffsetWhenUIOpen;
		}
		set
		{
			if (_useMonitorViewOffsetWhenUIOpen != value)
			{
				_useMonitorViewOffsetWhenUIOpen = value;
				if (useMonitorViewOffsetWhenUIOpenToggle != null)
				{
					useMonitorViewOffsetWhenUIOpenToggle.isOn = _useMonitorViewOffsetWhenUIOpen;
				}
				if (SuperController.singleton != null)
				{
					SuperController.singleton.SyncMonitorRigPosition();
				}
				SavePreferences();
			}
		}
	}

	public bool steamVRShowControllers
	{
		get
		{
			return _steamVRShowControllers;
		}
		set
		{
			if (_steamVRShowControllers != value)
			{
				_steamVRShowControllers = value;
				if (steamVRShowControllersToggle != null)
				{
					steamVRShowControllersToggle.isOn = _steamVRShowControllers;
				}
				SyncSteamVRControllerModels();
				SavePreferences();
			}
		}
	}

	public bool steamVRUseControllerHandPose
	{
		get
		{
			return _steamVRUseControllerHandPose;
		}
		set
		{
			if (_steamVRUseControllerHandPose != value)
			{
				_steamVRUseControllerHandPose = value;
				if (steamVRUseControllerHandPoseToggle != null)
				{
					steamVRUseControllerHandPoseToggle.isOn = _steamVRUseControllerHandPose;
				}
				SyncSteamVRUseControllerHandPose();
				SavePreferences();
			}
		}
	}

	public float steamVRPointerAngle
	{
		get
		{
			return _steamVRPointerAngle;
		}
		set
		{
			if (_steamVRPointerAngle != value)
			{
				_steamVRPointerAngle = value;
				if (steamVRPointerAngleSlider != null)
				{
					steamVRPointerAngleSlider.value = _steamVRPointerAngle;
				}
				SyncSteamVRPointerAngle();
				SavePreferences();
			}
		}
	}

	public float fingerInputFactor
	{
		get
		{
			return _fingerInputFactor;
		}
		set
		{
			if (_fingerInputFactor != value)
			{
				_fingerInputFactor = value;
				if (fingerInputFactorSlider != null)
				{
					fingerInputFactorSlider.value = _fingerInputFactor;
				}
				SyncFingerInputFactor();
				SavePreferences();
			}
		}
	}

	public float thumbInputFactor
	{
		get
		{
			return _thumbInputFactor;
		}
		set
		{
			if (_thumbInputFactor != value)
			{
				_thumbInputFactor = value;
				if (thumbInputFactorSlider != null)
				{
					thumbInputFactorSlider.value = _thumbInputFactor;
				}
				SyncThumbInputFactor();
				SavePreferences();
			}
		}
	}

	public float fingerSpreadOffset
	{
		get
		{
			return _fingerSpreadOffset;
		}
		set
		{
			if (_fingerSpreadOffset != value)
			{
				_fingerSpreadOffset = value;
				if (fingerSpreadOffsetSlider != null)
				{
					fingerSpreadOffsetSlider.value = _fingerSpreadOffset;
				}
				SyncFingerSpreadOffset();
				SavePreferences();
			}
		}
	}

	public float fingerBendOffset
	{
		get
		{
			return _fingerBendOffset;
		}
		set
		{
			if (_fingerBendOffset != value)
			{
				_fingerBendOffset = value;
				if (fingerBendOffsetSlider != null)
				{
					fingerBendOffsetSlider.value = _fingerBendOffset;
				}
				SyncFingerBendOffset();
				SavePreferences();
			}
		}
	}

	public float thumbSpreadOffset
	{
		get
		{
			return _thumbSpreadOffset;
		}
		set
		{
			if (_thumbSpreadOffset != value)
			{
				_thumbSpreadOffset = value;
				if (thumbSpreadOffsetSlider != null)
				{
					thumbSpreadOffsetSlider.value = _thumbSpreadOffset;
				}
				SyncThumbSpreadOffset();
				SavePreferences();
			}
		}
	}

	public float thumbBendOffset
	{
		get
		{
			return _thumbBendOffset;
		}
		set
		{
			if (_thumbBendOffset != value)
			{
				_thumbBendOffset = value;
				if (thumbBendOffsetSlider != null)
				{
					thumbBendOffsetSlider.value = _thumbBendOffset;
				}
				SyncThumbBendOffset();
				SavePreferences();
			}
		}
	}

	public bool oculusSwapGrabAndTrigger
	{
		get
		{
			return _oculusSwapGrabAndTrigger;
		}
		set
		{
			if (_oculusSwapGrabAndTrigger != value)
			{
				_oculusSwapGrabAndTrigger = value;
				if (oculusSwapGrabAndTriggerToggle != null)
				{
					oculusSwapGrabAndTriggerToggle.isOn = _oculusSwapGrabAndTrigger;
				}
				SavePreferences();
			}
		}
	}

	public bool oculusDisableFreeMove
	{
		get
		{
			return _oculusDisableFreeMove;
		}
		set
		{
			if (_oculusDisableFreeMove != value)
			{
				_oculusDisableFreeMove = value;
				if (oculusDisableFreeMoveToggle != null)
				{
					oculusDisableFreeMoveToggle.isOn = _oculusDisableFreeMove;
				}
				SavePreferences();
			}
		}
	}

	public float pointLightShadowBlur
	{
		get
		{
			return _pointLightShadowBlur;
		}
		set
		{
			if (_pointLightShadowBlur != value)
			{
				_pointLightShadowBlur = value;
				SyncShadowBlur();
				if (pointLightShadowBlurSlider != null)
				{
					pointLightShadowBlurSlider.value = _pointLightShadowBlur;
				}
				SavePreferences();
			}
		}
	}

	public float pointLightShadowBiasBase
	{
		get
		{
			return _pointLightShadowBiasBase;
		}
		set
		{
			if (_pointLightShadowBiasBase != value)
			{
				_pointLightShadowBiasBase = value;
				SyncShadowBiasBase();
				if (pointLightShadowBiasBaseSlider != null)
				{
					pointLightShadowBiasBaseSlider.value = _pointLightShadowBiasBase;
				}
				SavePreferences();
			}
		}
	}

	public float shadowFilterLevel
	{
		get
		{
			return _shadowFilterLevel;
		}
		set
		{
			if (_shadowFilterLevel != value)
			{
				_shadowFilterLevel = value;
				SyncShadowFilterLevel();
				if (shadowFilterLevelSlider != null)
				{
					shadowFilterLevelSlider.value = _shadowFilterLevel;
				}
			}
		}
	}

	public bool closeObjectBlur
	{
		get
		{
			return _closeObjectBlur;
		}
		set
		{
			if (_closeObjectBlur != value)
			{
				_closeObjectBlur = value;
				if (closeObjectBlurToggle != null)
				{
					closeObjectBlurToggle.isOn = _closeObjectBlur;
				}
				SyncCloseObjectBlur();
				SavePreferences();
			}
		}
	}

	public bool softPhysics
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overrideSoftPhysics)
			{
				return GlobalSceneOptions.singleton.softPhysics;
			}
			return _softPhysics;
		}
		set
		{
			if (_softPhysics != value)
			{
				_softPhysics = value;
				if (softPhysicsToggle != null)
				{
					softPhysicsToggle.isOn = _softPhysics;
				}
				SyncSoftPhysics();
				SavePreferences();
			}
		}
	}

	public bool pauseGlow
	{
		get
		{
			return _pauseGlow;
		}
		set
		{
			_pauseGlow = value;
			SyncGlow();
		}
	}

	public GlowEffectsLevel glowEffects
	{
		get
		{
			return _glowEffects;
		}
		set
		{
			if (_glowEffects != value)
			{
				_glowEffects = value;
				if (glowEffectsPopup != null)
				{
					glowEffectsPopup.currentValue = _glowEffects.ToString();
				}
				SyncGlow();
				SavePreferences();
			}
		}
	}

	public PhysicsRate physicsRate
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overridePhysicsRate)
			{
				return GlobalSceneOptions.singleton.physicsRate;
			}
			return _physicsRate;
		}
		set
		{
			if (_physicsRate != value)
			{
				_physicsRate = value;
				if (physicsRatePopup != null)
				{
					physicsRatePopup.currentValue = _physicsRate.ToString();
				}
				SyncPhysics();
				SavePreferences();
			}
		}
	}

	public int physicsUpdateCap
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overridePhysicsUpdateCap)
			{
				return GlobalSceneOptions.singleton.physicsUpdateCap;
			}
			return _physicsUpdateCap;
		}
		set
		{
			if (_physicsUpdateCap != value)
			{
				_physicsUpdateCap = value;
				if (physicsUpdateCapPopup != null)
				{
					physicsUpdateCapPopup.currentValue = _physicsUpdateCap.ToString();
				}
				SyncPhysics();
				SavePreferences();
			}
		}
	}

	public bool physicsHighQuality
	{
		get
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.overridePhysicsHighQuality)
			{
				return GlobalSceneOptions.singleton.physicsHighQuality;
			}
			return _physicsHighQuality;
		}
		set
		{
			if (_physicsHighQuality != value)
			{
				_physicsHighQuality = value;
				if (physicsHighQualityToggle != null)
				{
					physicsHighQualityToggle.isOn = _physicsHighQuality;
				}
				SyncPhysics();
				SavePreferences();
			}
		}
	}

	public bool useHeadCollider
	{
		get
		{
			return _useHeadCollider;
		}
		set
		{
			if (_useHeadCollider != value)
			{
				_useHeadCollider = value;
				if (useHeadColliderToggle != null)
				{
					useHeadColliderToggle.isOn = value;
				}
				SyncHeadCollider();
				SavePreferences();
			}
		}
	}

	public bool optimizeMemoryOnSceneLoad
	{
		get
		{
			return _optimizeMemoryOnSceneLoad;
		}
		set
		{
			if (_optimizeMemoryOnSceneLoad != value)
			{
				_optimizeMemoryOnSceneLoad = value;
				SavePreferences();
			}
		}
	}

	public bool optimizeMemoryOnPresetLoad
	{
		get
		{
			return _optimizeMemoryOnPresetLoad;
		}
		set
		{
			if (_optimizeMemoryOnPresetLoad != value)
			{
				_optimizeMemoryOnPresetLoad = value;
				SavePreferences();
			}
		}
	}

	public bool enableCaching
	{
		get
		{
			return _enableCaching;
		}
		set
		{
			if (_enableCaching != value)
			{
				_enableCaching = value;
				CacheManager.CachingEnabled = _enableCaching;
				FileManager.SyncJSONCache();
				if (enableCachingToggle != null)
				{
					enableCachingToggle.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public string cacheFolder
	{
		get
		{
			return _cacheFolder;
		}
		private set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_cacheFolder != value)
			{
				_cacheFolder = value;
				CacheManager.SetCacheDir(_cacheFolder);
				if (cacheFolderText != null)
				{
					cacheFolderText.text = _cacheFolder;
				}
				SavePreferences();
			}
		}
	}

	public bool confirmLoad
	{
		get
		{
			return _confirmLoad;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_confirmLoad != value)
			{
				_confirmLoad = value;
				if (confirmLoadToggle != null)
				{
					confirmLoadToggle.isOn = _confirmLoad;
				}
				SavePreferences();
			}
		}
	}

	public bool flipToolbar
	{
		get
		{
			return _flipToolbar;
		}
		set
		{
			if (_flipToolbar != value)
			{
				_flipToolbar = value;
				if (flipToolbarToggle != null)
				{
					flipToolbarToggle.isOn = _flipToolbar;
				}
				toolbarFlipper.flipped = _flipToolbar;
				SavePreferences();
			}
		}
	}

	public bool overlayUI
	{
		get
		{
			return _overlayUI;
		}
		set
		{
			if (_overlayUI != value)
			{
				_overlayUI = value;
				if (overlayUIToggle != null)
				{
					overlayUIToggle.isOn = _overlayUI;
				}
				SyncOverlayUI();
				SavePreferences();
			}
		}
	}

	public bool enableWebBrowser
	{
		get
		{
			return _enableWebBrowser;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enableWebBrowser != value)
			{
				_enableWebBrowser = value;
				if (enableWebBrowserToggle != null)
				{
					enableWebBrowserToggle.isOn = value;
				}
				if (enableWebBrowserToggleAlt != null)
				{
					enableWebBrowserToggleAlt.isOn = value;
				}
				SyncEnableWebBrowser();
				SavePreferences();
			}
		}
	}

	public bool allowNonWhitelistDomains
	{
		get
		{
			return _allowNonWhitelistDomains;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_allowNonWhitelistDomains != value)
			{
				_allowNonWhitelistDomains = value;
				if (allowNonWhitelistDomainsToggle != null)
				{
					allowNonWhitelistDomainsToggle.isOn = value;
				}
				if (allowNonWhitelistDomainsToggleAlt != null)
				{
					allowNonWhitelistDomainsToggleAlt.isOn = value;
				}
				if (allowNonWhitelistDomainsToggleAlt2 != null)
				{
					allowNonWhitelistDomainsToggleAlt2.isOn = value;
				}
				if (allowNonWhitelistDomainsToggleAlt3 != null)
				{
					allowNonWhitelistDomainsToggleAlt3.isOn = value;
				}
				SyncWhitelistDomains();
				SavePreferences();
			}
		}
	}

	public bool enableWebBrowserProfile
	{
		get
		{
			return _enableWebBrowserProfile;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enableWebBrowserProfile != value)
			{
				_enableWebBrowserProfile = value;
				if (enableWebBrowserProfileToggle != null)
				{
					enableWebBrowserProfileToggle.isOn = value;
				}
				if (enableWebBrowserProfileToggleAlt != null)
				{
					enableWebBrowserProfileToggleAlt.isOn = value;
				}
				SavePreferences();
				SyncEnableWebBrowserProfile();
			}
		}
	}

	public bool enableWebMisc
	{
		get
		{
			return _enableWebMisc;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enableWebMisc != value)
			{
				_enableWebMisc = value;
				if (enableWebMiscToggle != null)
				{
					enableWebMiscToggle.isOn = value;
				}
				if (enableWebMiscToggleAlt != null)
				{
					enableWebMiscToggleAlt.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public bool enableHub
	{
		get
		{
			return _enableHub;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enableHub != value)
			{
				_enableHub = value;
				if (enableHubToggle != null)
				{
					enableHubToggle.isOn = value;
				}
				if (enableHubToggleAlt != null)
				{
					enableHubToggleAlt.isOn = value;
				}
				SyncEnableHub();
				SavePreferences();
			}
		}
	}

	public bool enableHubDownloader
	{
		get
		{
			return _enableHubDownloader;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enableHubDownloader != value)
			{
				_enableHubDownloader = value;
				if (enableHubDownloaderToggle != null)
				{
					enableHubDownloaderToggle.isOn = value;
				}
				if (enableHubDownloaderToggleAlt != null)
				{
					enableHubDownloaderToggleAlt.isOn = value;
				}
				SyncEnableHubDownloader();
				SavePreferences();
			}
		}
	}

	public bool enablePlugins
	{
		get
		{
			return _enablePlugins;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_enablePlugins != value)
			{
				_enablePlugins = value;
				if (enablePluginsToggle != null)
				{
					enablePluginsToggle.isOn = value;
				}
				if (enablePluginsToggleAlt != null)
				{
					enablePluginsToggleAlt.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public bool allowPluginsNetworkAccess
	{
		get
		{
			return _allowPluginsNetworkAccess;
		}
		private set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_allowPluginsNetworkAccess != value)
			{
				_allowPluginsNetworkAccess = value;
				if (allowPluginsNetworkAccessToggle != null)
				{
					allowPluginsNetworkAccessToggle.isOn = value;
				}
				if (allowPluginsNetworkAccessToggleAlt != null)
				{
					allowPluginsNetworkAccessToggleAlt.isOn = value;
				}
				SyncAllowPluginsNetworkAccess();
				SavePreferences();
			}
		}
	}

	public bool alwaysAllowPluginsDownloadedFromHub
	{
		get
		{
			return _alwaysAllowPluginsDownloadedFromHub;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_alwaysAllowPluginsDownloadedFromHub != value)
			{
				_alwaysAllowPluginsDownloadedFromHub = value;
				if (alwaysAllowPluginsDownloadedFromHubToggle != null)
				{
					alwaysAllowPluginsDownloadedFromHubToggle.isOn = value;
				}
				if (alwaysAllowPluginsDownloadedFromHubToggleAlt != null)
				{
					alwaysAllowPluginsDownloadedFromHubToggleAlt.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public bool hideDisabledWebMessages
	{
		get
		{
			return _hideDisabledWebMessages;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_hideDisabledWebMessages != value)
			{
				_hideDisabledWebMessages = value;
				if (hideDisabledWebMessagesToggle != null)
				{
					hideDisabledWebMessagesToggle.isOn = value;
				}
				SavePreferences();
			}
		}
	}

	public string creatorName
	{
		get
		{
			return _creatorName;
		}
		set
		{
			FileManager.AssertNotCalledFromPlugin();
			if (_creatorName != value)
			{
				_creatorName = value;
				if (creatorNameInputField != null)
				{
					creatorNameInputField.text = _creatorName;
				}
				SavePreferences();
			}
		}
	}

	public string DAZExtraLibraryFolder
	{
		get
		{
			return _DAZExtraLibraryFolder;
		}
		set
		{
			if (_DAZExtraLibraryFolder != value)
			{
				_DAZExtraLibraryFolder = value;
				if (DAZExtraLibraryFolderText != null)
				{
					DAZExtraLibraryFolderText.text = _DAZExtraLibraryFolder;
				}
				SavePreferences();
			}
		}
	}

	public string DAZDefaultContentFolder
	{
		get
		{
			return _DAZDefaultContentFolder;
		}
		set
		{
			if (_DAZDefaultContentFolder != value)
			{
				_DAZDefaultContentFolder = value;
				if (DAZDefaultContentFolderText != null)
				{
					DAZDefaultContentFolderText.text = _DAZDefaultContentFolder;
				}
				SavePreferences();
			}
		}
	}

	public SortBy fileBrowserSortBy
	{
		get
		{
			return _fileBrowserSortBy;
		}
		set
		{
			if (_fileBrowserSortBy != value)
			{
				_fileBrowserSortBy = value;
				SavePreferences();
			}
		}
	}

	public DirectoryOption fileBrowserDirectoryOption
	{
		get
		{
			return _fileBrowserDirectoryOption;
		}
		set
		{
			if (_fileBrowserDirectoryOption != value)
			{
				_fileBrowserDirectoryOption = value;
				SavePreferences();
			}
		}
	}

	public void SetQuality(string qualityName)
	{
		QualityLevel value;
		if (QualityLevels.levels.TryGetValue(qualityName, out value))
		{
			_disableSave = true;
			renderScale = value.renderScale;
			msaaLevel = value.msaaLevel;
			pixelLightCount = value.pixelLightCount;
			shaderLOD = value.shaderLOD;
			smoothPasses = value.smoothPasses;
			mirrorReflections = value.mirrorReflections;
			realtimeReflectionProbes = value.realtimeReflectionProbes;
			closeObjectBlur = value.closeObjectBlur;
			softPhysics = value.softPhysics;
			glowEffects = value.glowEffects;
			_disableSave = false;
			SavePreferences();
		}
		else
		{
			UnityEngine.Debug.LogError("Could not find quality level " + qualityName);
		}
	}

	private void CheckQualityLevels()
	{
		bool flag = CheckQualityLevel("UltraLow");
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool isOn = false;
		if (!flag)
		{
			flag2 = CheckQualityLevel("Low");
			if (!flag2)
			{
				flag3 = CheckQualityLevel("Mid");
				if (!flag3)
				{
					flag4 = CheckQualityLevel("High");
					if (!flag4)
					{
						flag5 = CheckQualityLevel("Ultra");
						if (!flag5)
						{
							flag6 = CheckQualityLevel("Max");
							if (!flag6)
							{
								isOn = true;
							}
						}
					}
				}
			}
		}
		_disableToggles = true;
		if (ultraLowQualityToggle != null)
		{
			ultraLowQualityToggle.isOn = flag;
		}
		else if (flag)
		{
			isOn = true;
		}
		if (lowQualityToggle != null)
		{
			lowQualityToggle.isOn = flag2;
		}
		else if (flag2)
		{
			isOn = true;
		}
		if (midQualityToggle != null)
		{
			midQualityToggle.isOn = flag3;
		}
		else if (flag3)
		{
			isOn = true;
		}
		if (highQualityToggle != null)
		{
			highQualityToggle.isOn = flag4;
		}
		else if (flag4)
		{
			isOn = true;
		}
		if (ultraQualityToggle != null)
		{
			ultraQualityToggle.isOn = flag5;
		}
		else if (flag5)
		{
			isOn = true;
		}
		if (maxQualityToggle != null)
		{
			maxQualityToggle.isOn = flag6;
		}
		else if (flag6)
		{
			isOn = true;
		}
		if (customQualityToggle != null)
		{
			customQualityToggle.isOn = isOn;
		}
		_disableToggles = false;
	}

	private bool CheckQualityLevel(string qualityName)
	{
		bool result = false;
		QualityLevel value;
		if (QualityLevels.levels.TryGetValue(qualityName, out value))
		{
			result = true;
			if (_renderScale != value.renderScale || _msaaLevel != value.msaaLevel || _pixelLightCount != value.pixelLightCount || _shaderLOD != value.shaderLOD || _smoothPasses != value.smoothPasses || _mirrorReflections != value.mirrorReflections || _realtimeReflectionProbes != value.realtimeReflectionProbes || _closeObjectBlur != value.closeObjectBlur || _softPhysics != value.softPhysics || _glowEffects != value.glowEffects)
			{
				result = false;
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Could not find quality level " + qualityName);
		}
		return result;
	}

	public void SavePreferences()
	{
		if (_disableSave || !Application.isPlaying)
		{
			return;
		}
		JSONClass jSONClass = new JSONClass();
		jSONClass["firstTimeUser"].AsBool = _firstTimeUser;
		jSONClass["renderScale"].AsFloat = _renderScale;
		jSONClass["msaaLevel"].AsInt = _msaaLevel;
		jSONClass["desktopVsync"].AsBool = _desktopVsync;
		jSONClass["pixelLightCount"].AsInt = _pixelLightCount;
		jSONClass["shaderLOD"] = _shaderLOD.ToString();
		jSONClass["screenResolution"] = _screenResolution;
		jSONClass["smoothPasses"].AsInt = _smoothPasses;
		jSONClass["mirrorReflections"].AsBool = _mirrorReflections;
		jSONClass["realtimeReflectionProbes"].AsBool = _realtimeReflectionProbes;
		jSONClass["mirrorToDisplay"].AsBool = _mirrorToDisplay;
		jSONClass["hideExitButton"].AsBool = _hideExitButton;
		jSONClass["showTargetsMenuOnlyV2"].AsBool = _showTargetsMenuOnly;
		jSONClass["alwaysShowPointersOnTouch"].AsBool = _alwaysShowPointersOnTouch;
		jSONClass["hideInactiveTargets"].AsBool = _hideInactiveTargets;
		jSONClass["showControllersMenuOnly"].AsBool = _showControllersMenuOnly;
		jSONClass["targetAlpha"].AsFloat = _targetAlpha;
		jSONClass["crosshairAlpha"].AsFloat = _crosshairAlpha;
		jSONClass["useMonitorViewOffsetWhenUIOpen"].AsBool = _useMonitorViewOffsetWhenUIOpen;
		jSONClass["overlayUI"].AsBool = _overlayUI;
		jSONClass["oculusSwapGrabAndTrigger"].AsBool = _oculusSwapGrabAndTrigger;
		jSONClass["oculusDisableFreeMove"].AsBool = _oculusDisableFreeMove;
		jSONClass["steamVRShowControllers"].AsBool = _steamVRShowControllers;
		jSONClass["steamVRUseControllerHandPose"].AsBool = _steamVRUseControllerHandPose;
		jSONClass["steamVRPointerAngle"].AsFloat = _steamVRPointerAngle;
		jSONClass["fingerInputFactor"].AsFloat = _fingerInputFactor;
		jSONClass["thumbInputFactor"].AsFloat = _thumbInputFactor;
		jSONClass["fingerSpreadOffset"].AsFloat = _fingerSpreadOffset;
		jSONClass["fingerBendOffset"].AsFloat = _fingerBendOffset;
		jSONClass["thumbSpreadOffset"].AsFloat = _thumbSpreadOffset;
		jSONClass["thumbBendOffset"].AsFloat = _thumbBendOffset;
		jSONClass["physicsRate"] = physicsRate.ToString();
		jSONClass["physicsUpdateCap"] = physicsUpdateCap.ToString();
		jSONClass["physicsHighQuality"].AsBool = physicsHighQuality;
		jSONClass["softBodyPhysics"].AsBool = _softPhysics;
		jSONClass["glowEffects"] = _glowEffects.ToString();
		jSONClass["useHeadCollider"].AsBool = _useHeadCollider;
		jSONClass["optimizeMemoryOnSceneLoad"].AsBool = _optimizeMemoryOnSceneLoad;
		jSONClass["optimizeMemoryOnPresetLoad"].AsBool = _optimizeMemoryOnPresetLoad;
		jSONClass["enableCaching"].AsBool = _enableCaching;
		jSONClass["cacheFolder"] = _cacheFolder;
		jSONClass["confirmLoad"].AsBool = _confirmLoad;
		jSONClass["flipToolbar"].AsBool = _flipToolbar;
		jSONClass["enableWebBrowser"].AsBool = _enableWebBrowser;
		jSONClass["allowNonWhitelistDomains"].AsBool = _allowNonWhitelistDomains;
		jSONClass["enableWebBrowserProfile"].AsBool = _enableWebBrowserProfile;
		jSONClass["enableWebMisc"].AsBool = _enableWebMisc;
		jSONClass["enableHub"].AsBool = _enableHub;
		jSONClass["enableHubDownloader"].AsBool = _enableHubDownloader;
		jSONClass["enablePlugins"].AsBool = _enablePlugins;
		jSONClass["allowPluginsNetworkAccess"].AsBool = _allowPluginsNetworkAccess;
		jSONClass["alwaysAllowPluginsDownloadedFromHub"].AsBool = _alwaysAllowPluginsDownloadedFromHub;
		jSONClass["hideDisabledWebMessages"].AsBool = _hideDisabledWebMessages;
		if (SuperController.singleton != null)
		{
			if (!SuperController.singleton.disableTermsOfUse)
			{
				jSONClass["termsOfUseAccepted"].AsBool = _termsOfUseAccepted;
			}
			jSONClass["showHelpOverlay"].AsBool = SuperController.singleton.helpOverlayOn;
			jSONClass["lockHeightDuringNavigate"].AsBool = SuperController.singleton.lockHeightDuringNavigate;
			jSONClass["freeMoveFollowFloor"].AsBool = SuperController.singleton.freeMoveFollowFloor;
			jSONClass["disableAllNavigation"].AsBool = SuperController.singleton.disableAllNavigation;
			jSONClass["disableGrabNavigation"].AsBool = SuperController.singleton.disableGrabNavigation;
			jSONClass["disableTeleport"].AsBool = SuperController.singleton.disableTeleport;
			jSONClass["teleportAllowRotation"].AsBool = SuperController.singleton.teleportAllowRotation;
			jSONClass["disableTeleportDuringPossess"].AsBool = SuperController.singleton.disableTeleportDuringPossess;
			jSONClass["freeMoveMultiplier"].AsFloat = SuperController.singleton.freeMoveMultiplier;
			jSONClass["grabNavigationPositionMultiplier"].AsFloat = SuperController.singleton.grabNavigationPositionMultiplier;
			jSONClass["grabNavigationRotationMultiplier"].AsFloat = SuperController.singleton.grabNavigationRotationMultiplier;
			jSONClass["showNavigationHologrid"].AsBool = SuperController.singleton.showNavigationHologrid;
			jSONClass["hologridTransparency"].AsFloat = SuperController.singleton.hologridTransparency;
			jSONClass["oculusThumbstickFunction"] = SuperController.singleton.oculusThumbstickFunction.ToString();
			jSONClass["allowPossessSpringAdjustment"].AsBool = SuperController.singleton.allowPossessSpringAdjustment;
			jSONClass["possessPositionSpring"].AsFloat = SuperController.singleton.possessPositionSpring;
			jSONClass["possessRotationSpring"].AsFloat = SuperController.singleton.possessRotationSpring;
			jSONClass["loResScreenshotCameraFOV"].AsFloat = SuperController.singleton.loResScreenShotCameraFOV;
			jSONClass["hiResScreenshotCameraFOV"].AsFloat = SuperController.singleton.hiResScreenShotCameraFOV;
			jSONClass["allowGrabPlusTriggerHandToggle"].AsBool = SuperController.singleton.allowGrabPlusTriggerHandToggle;
			jSONClass["monitorUIScale"].AsFloat = SuperController.singleton.monitorUIScale;
			jSONClass["monitorUIYOffset"].AsFloat = SuperController.singleton.monitorUIYOffset;
			jSONClass["VRUISide"] = SuperController.singleton.UISide.ToString();
			jSONClass["motionControllerAlwaysUseAlternateHands"].AsBool = SuperController.singleton.alwaysUseAlternateHands;
			jSONClass["useLegacyWorldScaleChange"].AsBool = SuperController.singleton.useLegacyWorldScaleChange;
			jSONClass["onStartupSkipStartScreen"].AsBool = SuperController.singleton.onStartupSkipStartScreen;
			jSONClass["autoFreezeAnimationOnSwitchToEditMode"].AsBool = SuperController.singleton.autoFreezeAnimationOnSwitchToEditMode;
			jSONClass["worldUIVRAnchorDistance"].AsFloat = SuperController.singleton.worldUIVRAnchorDistance;
			jSONClass["worldUIVRAnchorHeight"].AsFloat = SuperController.singleton.worldUIVRAnchorHeight;
			jSONClass["useMonitorRigAudioListenerWhenActive"].AsBool = SuperController.singleton.useMonitorRigAudioListenerWhenActive;
			jSONClass["generateDepthTexture"].AsBool = SuperController.singleton.generateDepthTexture;
			jSONClass["leapMotionEnabled"].AsBool = SuperController.singleton.leapMotionEnabled;
			jSONClass["openMainHUDOnError"].AsBool = SuperController.singleton.openMainHUDOnError;
		}
		if (leapHandModelControl != null)
		{
			jSONClass["leapMotionAllowPinchGrab"].AsBool = leapHandModelControl.allowPinchGrab;
		}
		if (motionHandModelControl != null)
		{
			jSONClass["motionControllerLeftHandChoice"] = motionHandModelControl.leftHandChoice;
			jSONClass["motionControllerRightHandChoice"] = motionHandModelControl.rightHandChoice;
			jSONClass["motionControllerLinkHands"].AsBool = motionHandModelControl.linkHands;
			jSONClass["motionControllerUseCollision"].AsBool = motionHandModelControl.useCollision;
			jSONClass["motionControllerHandsPositionOffset"]["x"].AsFloat = motionHandModelControl.xPosition;
			jSONClass["motionControllerHandsPositionOffset"]["y"].AsFloat = motionHandModelControl.yPosition;
			jSONClass["motionControllerHandsPositionOffset"]["z"].AsFloat = motionHandModelControl.zPosition;
			jSONClass["motionControllerHandsRotationOffset"]["x"].AsFloat = motionHandModelControl.xRotation;
			jSONClass["motionControllerHandsRotationOffset"]["y"].AsFloat = motionHandModelControl.yRotation;
			jSONClass["motionControllerHandsRotationOffset"]["z"].AsFloat = motionHandModelControl.zRotation;
		}
		if (alternateMotionHandModelControl != null)
		{
			jSONClass["alternateMotionControllerLeftHandChoice"] = alternateMotionHandModelControl.leftHandChoice;
			jSONClass["alternateMotionControllerRightHandChoice"] = alternateMotionHandModelControl.rightHandChoice;
			jSONClass["alternateMotionControllerLinkHands"].AsBool = alternateMotionHandModelControl.linkHands;
		}
		jSONClass["creatorName"] = _creatorName;
		jSONClass["DAZExtraLibraryFolder"] = _DAZExtraLibraryFolder;
		jSONClass["DAZDefaultContentFolder"] = _DAZDefaultContentFolder;
		jSONClass["fileBrowserSortBy"] = _fileBrowserSortBy.ToString();
		jSONClass["fileBrowserDirectoryOption"] = _fileBrowserDirectoryOption.ToString();
		string value = jSONClass.ToString(string.Empty);
		StreamWriter streamWriter = new StreamWriter("prefs.json");
		streamWriter.Write(value);
		streamWriter.Close();
		CheckQualityLevels();
	}

	public void RestorePreferences()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		string path = "prefs.json";
		_disableSave = true;
		if (File.Exists(path))
		{
			try
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					string aJSON = streamReader.ReadToEnd();
					SimpleJSON.JSONNode jSONNode = JSON.Parse(aJSON);
					if (jSONNode["firstTimeUser"] != null)
					{
						firstTimeUser = jSONNode["firstTimeUser"].AsBool;
					}
					if (jSONNode["renderScale"] != null)
					{
						renderScale = jSONNode["renderScale"].AsFloat;
					}
					if (jSONNode["msaaLevel"] != null)
					{
						msaaLevel = jSONNode["msaaLevel"].AsInt;
					}
					if (jSONNode["desktopVsync"] != null)
					{
						desktopVsync = jSONNode["desktopVsync"].AsBool;
					}
					if (jSONNode["pixelLightCount"] != null)
					{
						pixelLightCount = jSONNode["pixelLightCount"].AsInt;
					}
					if (jSONNode["shaderLOD"] != null)
					{
						SetShaderLODFromString(jSONNode["shaderLOD"]);
					}
					if (jSONNode["screenResolution"] != null)
					{
						_screenResolution = jSONNode["screenResolution"];
					}
					if (jSONNode["smoothPasses"] != null)
					{
						smoothPasses = jSONNode["smoothPasses"].AsInt;
					}
					if (jSONNode["mirrorToDisplay"] != null)
					{
						mirrorToDisplay = jSONNode["mirrorToDisplay"].AsBool;
					}
					if (jSONNode["hideExitButton"] != null)
					{
						hideExitButton = jSONNode["hideExitButton"].AsBool;
					}
					if (jSONNode["mirrorReflections"] != null)
					{
						mirrorReflections = jSONNode["mirrorReflections"].AsBool;
					}
					if (jSONNode["realtimeReflectionProbes"] != null)
					{
						realtimeReflectionProbes = jSONNode["realtimeReflectionProbes"].AsBool;
					}
					if (jSONNode["showTargetsMenuOnlyV2"] != null)
					{
						showTargetsMenuOnly = jSONNode["showTargetsMenuOnlyV2"].AsBool;
					}
					if (jSONNode["alwaysShowPointersOnTouch"] != null)
					{
						alwaysShowPointersOnTouch = jSONNode["alwaysShowPointersOnTouch"].AsBool;
					}
					if (jSONNode["hideInactiveTargets"] != null)
					{
						hideInactiveTargets = jSONNode["hideInactiveTargets"].AsBool;
					}
					if (jSONNode["showControllersMenuOnly"] != null)
					{
						showControllersMenuOnly = jSONNode["showControllersMenuOnly"].AsBool;
					}
					if (jSONNode["overlayUI"] != null)
					{
						overlayUI = jSONNode["overlayUI"].AsBool;
					}
					if (jSONNode["targetAlpha"] != null)
					{
						targetAlpha = jSONNode["targetAlpha"].AsFloat;
					}
					if (jSONNode["crosshairAlpha"] != null)
					{
						crosshairAlpha = jSONNode["crosshairAlpha"].AsFloat;
					}
					if (jSONNode["useMonitorViewOffsetWhenUIOpen"] != null)
					{
						useMonitorViewOffsetWhenUIOpen = jSONNode["useMonitorViewOffsetWhenUIOpen"].AsBool;
					}
					if (jSONNode["oculusSwapGrabAndTrigger"] != null)
					{
						oculusSwapGrabAndTrigger = jSONNode["oculusSwapGrabAndTrigger"].AsBool;
					}
					if (jSONNode["oculusDisableFreeMove"] != null)
					{
						oculusDisableFreeMove = jSONNode["oculusDisableFreeMove"].AsBool;
					}
					if (jSONNode["steamVRShowControllers"] != null)
					{
						steamVRShowControllers = jSONNode["steamVRShowControllers"].AsBool;
					}
					if (jSONNode["steamVRUseControllerHandPose"] != null)
					{
						steamVRUseControllerHandPose = jSONNode["steamVRUseControllerHandPose"].AsBool;
					}
					if (jSONNode["steamVRPointerAngle"] != null)
					{
						steamVRPointerAngle = jSONNode["steamVRPointerAngle"].AsFloat;
					}
					if (jSONNode["fingerInputFactor"] != null)
					{
						fingerInputFactor = jSONNode["fingerInputFactor"].AsFloat;
					}
					if (jSONNode["thumbInputFactor"] != null)
					{
						thumbInputFactor = jSONNode["thumbInputFactor"].AsFloat;
					}
					if (jSONNode["fingerSpreadOffset"] != null)
					{
						fingerSpreadOffset = jSONNode["fingerSpreadOffset"].AsFloat;
					}
					if (jSONNode["fingerBendOffset"] != null)
					{
						fingerBendOffset = jSONNode["fingerBendOffset"].AsFloat;
					}
					if (jSONNode["thumbSpreadOffset"] != null)
					{
						thumbSpreadOffset = jSONNode["thumbSpreadOffset"].AsFloat;
					}
					if (jSONNode["thumbBendOffset"] != null)
					{
						thumbBendOffset = jSONNode["thumbBendOffset"].AsFloat;
					}
					if (jSONNode["shadowFilterLevel"] != null)
					{
						shadowFilterLevel = jSONNode["shadowFilterLevel"].AsFloat;
					}
					if (jSONNode["pointLightShadowBlur"] != null)
					{
						pointLightShadowBlur = jSONNode["pointLightShadowBlur"].AsFloat;
					}
					if (jSONNode["pointLightShadowBiasBase"] != null)
					{
						pointLightShadowBiasBase = jSONNode["pointLightShadowBiasBase"].AsFloat;
					}
					if (jSONNode["physicsRate"] != null)
					{
						SetPhysicsRateFromString(jSONNode["physicsRate"]);
					}
					if (jSONNode["physicsUpdateCap"] != null)
					{
						SetPhysicsUpdateCapFromString(jSONNode["physicsUpdateCap"]);
					}
					if (jSONNode["physicsHighQuality"] != null)
					{
						physicsHighQuality = jSONNode["physicsHighQuality"].AsBool;
					}
					if (jSONNode["softBodyPhysics"] != null)
					{
						softPhysics = jSONNode["softBodyPhysics"].AsBool;
					}
					if (jSONNode["glowEffects"] != null)
					{
						SetGlowEffectsFromString(jSONNode["glowEffects"]);
					}
					if (jSONNode["useHeadCollider"] != null)
					{
						useHeadCollider = jSONNode["useHeadCollider"].AsBool;
					}
					if (jSONNode["optimizeMemoryOnSceneLoad"] != null)
					{
						optimizeMemoryOnSceneLoad = jSONNode["optimizeMemoryOnSceneLoad"].AsBool;
					}
					if (jSONNode["optimizeMemoryOnPresetLoad"] != null)
					{
						optimizeMemoryOnPresetLoad = jSONNode["optimizeMemoryOnPresetLoad"].AsBool;
					}
					if (jSONNode["enableCaching"] != null)
					{
						enableCaching = jSONNode["enableCaching"].AsBool;
					}
					if (jSONNode["cacheFolder"] != null)
					{
						cacheFolder = jSONNode["cacheFolder"];
					}
					if (jSONNode["confirmLoad"] != null)
					{
						confirmLoad = jSONNode["confirmLoad"].AsBool;
					}
					if (jSONNode["flipToolbar"] != null)
					{
						flipToolbar = jSONNode["flipToolbar"].AsBool;
					}
					if (jSONNode["enableWebBrowser"] != null)
					{
						enableWebBrowser = jSONNode["enableWebBrowser"].AsBool;
					}
					if (jSONNode["allowNonWhitelistDomains"] != null)
					{
						allowNonWhitelistDomains = jSONNode["allowNonWhitelistDomains"].AsBool;
					}
					if (jSONNode["enableWebBrowserProfile"] != null)
					{
						enableWebBrowserProfile = jSONNode["enableWebBrowserProfile"].AsBool;
					}
					if (jSONNode["enableWebMisc"] != null)
					{
						enableWebMisc = jSONNode["enableWebMisc"].AsBool;
					}
					if (jSONNode["enableHub"] != null)
					{
						enableHub = jSONNode["enableHub"].AsBool;
					}
					if (jSONNode["enableHubDownloader"] != null)
					{
						enableHubDownloader = jSONNode["enableHubDownloader"].AsBool;
					}
					if (jSONNode["enablePlugins"] != null)
					{
						enablePlugins = jSONNode["enablePlugins"].AsBool;
					}
					if (jSONNode["allowPluginsNetworkAccess"] != null)
					{
						allowPluginsNetworkAccess = jSONNode["allowPluginsNetworkAccess"].AsBool;
					}
					if (jSONNode["alwaysAllowPluginsDownloadedFromHub"] != null)
					{
						alwaysAllowPluginsDownloadedFromHub = jSONNode["alwaysAllowPluginsDownloadedFromHub"].AsBool;
					}
					if (jSONNode["hideDisabledWebMessages"] != null)
					{
						hideDisabledWebMessages = jSONNode["hideDisabledWebMessages"].AsBool;
					}
					if (SuperController.singleton != null)
					{
						if (SuperController.singleton.termsOfUseDisabled)
						{
							termsOfUseAccepted = true;
						}
						else if (jSONNode["termsOfUseAccepted"] != null)
						{
							termsOfUseAccepted = jSONNode["termsOfUseAccepted"].AsBool;
						}
						if (jSONNode["showHelpOverlay"] != null)
						{
							SuperController.singleton.helpOverlayOn = jSONNode["showHelpOverlay"].AsBool;
						}
						if (jSONNode["lockHeightDuringNavigate"] != null)
						{
							SuperController.singleton.lockHeightDuringNavigate = jSONNode["lockHeightDuringNavigate"].AsBool;
						}
						if (jSONNode["freeMoveFollowFloor"] != null)
						{
							SuperController.singleton.freeMoveFollowFloor = jSONNode["freeMoveFollowFloor"].AsBool;
						}
						if (jSONNode["teleportAllowRotation"] != null)
						{
							SuperController.singleton.teleportAllowRotation = jSONNode["teleportAllowRotation"].AsBool;
						}
						if (jSONNode["disableAllNavigation"] != null)
						{
							SuperController.singleton.disableAllNavigation = jSONNode["disableAllNavigation"].AsBool;
						}
						if (jSONNode["disableGrabNavigation"] != null)
						{
							SuperController.singleton.disableGrabNavigation = jSONNode["disableGrabNavigation"].AsBool;
						}
						if (jSONNode["disableTeleport"] != null)
						{
							SuperController.singleton.disableTeleport = jSONNode["disableTeleport"].AsBool;
						}
						if (jSONNode["disableTeleportDuringPossess"] != null)
						{
							SuperController.singleton.disableTeleportDuringPossess = jSONNode["disableTeleportDuringPossess"].AsBool;
						}
						if (jSONNode["freeMoveMultiplier"] != null)
						{
							SuperController.singleton.freeMoveMultiplier = jSONNode["freeMoveMultiplier"].AsFloat;
						}
						if (jSONNode["grabNavigationPositionMultiplier"] != null)
						{
							SuperController.singleton.grabNavigationPositionMultiplier = jSONNode["grabNavigationPositionMultiplier"].AsFloat;
						}
						if (jSONNode["grabNavigationRotationMultiplier"] != null)
						{
							SuperController.singleton.grabNavigationRotationMultiplier = jSONNode["grabNavigationRotationMultiplier"].AsFloat;
						}
						if (jSONNode["showNavigationHologrid"] != null)
						{
							SuperController.singleton.showNavigationHologrid = jSONNode["showNavigationHologrid"].AsBool;
						}
						if (jSONNode["hologridTransparency"] != null)
						{
							SuperController.singleton.hologridTransparency = jSONNode["hologridTransparency"].AsFloat;
						}
						if (jSONNode["oculusThumbstickFunction"] != null)
						{
							SuperController.singleton.SetOculusThumbstickFunctionFromString(jSONNode["oculusThumbstickFunction"]);
						}
						if (jSONNode["allowPossessSpringAdjustment"] != null)
						{
							SuperController.singleton.allowPossessSpringAdjustment = jSONNode["allowPossessSpringAdjustment"].AsBool;
						}
						if (jSONNode["possessPositionSpring"] != null)
						{
							SuperController.singleton.possessPositionSpring = jSONNode["possessPositionSpring"].AsFloat;
						}
						if (jSONNode["possessRotationSpring"] != null)
						{
							SuperController.singleton.possessRotationSpring = jSONNode["possessRotationSpring"].AsFloat;
						}
						if (jSONNode["loResScreenshotCameraFOV"] != null)
						{
							SuperController.singleton.loResScreenShotCameraFOV = jSONNode["loResScreenshotCameraFOV"].AsFloat;
						}
						if (jSONNode["hiResScreenshotCameraFOV"] != null)
						{
							SuperController.singleton.hiResScreenShotCameraFOV = jSONNode["hiResScreenshotCameraFOV"].AsFloat;
						}
						if (jSONNode["allowGrabPlusTriggerHandToggle"] != null)
						{
							SuperController.singleton.allowGrabPlusTriggerHandToggle = jSONNode["allowGrabPlusTriggerHandToggle"].AsBool;
						}
						if (jSONNode["monitorUIScale"] != null)
						{
							SuperController.singleton.monitorUIScale = jSONNode["monitorUIScale"].AsFloat;
						}
						if (jSONNode["monitorUIYOffset"] != null)
						{
							SuperController.singleton.monitorUIYOffset = jSONNode["monitorUIYOffset"].AsFloat;
						}
						if (jSONNode["VRUISide"] != null)
						{
							SuperController.singleton.SetUISide(jSONNode["VRUISide"]);
						}
						if (jSONNode["motionControllerAlwaysUseAlternateHands"] != null)
						{
							SuperController.singleton.alwaysUseAlternateHands = jSONNode["motionControllerAlwaysUseAlternateHands"].AsBool;
						}
						if (jSONNode["useLegacyWorldScaleChange"] != null)
						{
							SuperController.singleton.useLegacyWorldScaleChange = jSONNode["useLegacyWorldScaleChange"].AsBool;
						}
						if (jSONNode["onStartupSkipStartScreen"] != null)
						{
							SuperController.singleton.onStartupSkipStartScreen = jSONNode["onStartupSkipStartScreen"].AsBool;
						}
						if (jSONNode["autoFreezeAnimationOnSwitchToEditMode"] != null)
						{
							SuperController.singleton.autoFreezeAnimationOnSwitchToEditMode = jSONNode["autoFreezeAnimationOnSwitchToEditMode"].AsBool;
						}
						if (jSONNode["worldUIVRAnchorDistance"] != null)
						{
							SuperController.singleton.worldUIVRAnchorDistance = jSONNode["worldUIVRAnchorDistance"].AsFloat;
						}
						if (jSONNode["worldUIVRAnchorHeight"] != null)
						{
							SuperController.singleton.worldUIVRAnchorHeight = jSONNode["worldUIVRAnchorHeight"].AsFloat;
						}
						if (jSONNode["useMonitorRigAudioListenerWhenActive"] != null)
						{
							SuperController.singleton.useMonitorRigAudioListenerWhenActive = jSONNode["useMonitorRigAudioListenerWhenActive"].AsBool;
						}
						if (jSONNode["generateDepthTexture"] != null)
						{
							SuperController.singleton.generateDepthTexture = jSONNode["generateDepthTexture"].AsBool;
						}
						if (jSONNode["leapMotionEnabled"] != null)
						{
							SuperController.singleton.leapMotionEnabled = jSONNode["leapMotionEnabled"].AsBool;
						}
						if (jSONNode["openMainHUDOnError"] != null)
						{
							SuperController.singleton.openMainHUDOnError = jSONNode["openMainHUDOnError"].AsBool;
						}
					}
					if (leapHandModelControl != null && jSONNode["leapMotionAllowPinchGrab"] != null)
					{
						leapHandModelControl.allowPinchGrab = jSONNode["leapMotionAllowPinchGrab"].AsBool;
					}
					if (motionHandModelControl != null)
					{
						if (jSONNode["motionControllerLeftHandChoice"] != null)
						{
							motionHandModelControl.leftHandChoice = jSONNode["motionControllerLeftHandChoice"];
						}
						if (jSONNode["motionControllerRightHandChoice"] != null)
						{
							motionHandModelControl.rightHandChoice = jSONNode["motionControllerRightHandChoice"];
						}
						if (jSONNode["motionControllerLinkHands"] != null)
						{
							motionHandModelControl.linkHands = jSONNode["motionControllerLinkHands"].AsBool;
						}
						if (jSONNode["motionControllerUseCollision"] != null)
						{
							motionHandModelControl.useCollision = jSONNode["motionControllerUseCollision"].AsBool;
						}
						if (jSONNode["motionControllerHandsPositionOffset"] != null)
						{
							motionHandModelControl.xPosition = jSONNode["motionControllerHandsPositionOffset"]["x"].AsFloat;
							motionHandModelControl.yPosition = jSONNode["motionControllerHandsPositionOffset"]["y"].AsFloat;
							motionHandModelControl.zPosition = jSONNode["motionControllerHandsPositionOffset"]["z"].AsFloat;
						}
						if (jSONNode["motionControllerHandsRotationOffset"] != null)
						{
							motionHandModelControl.xRotation = jSONNode["motionControllerHandsRotationOffset"]["x"].AsFloat;
							motionHandModelControl.yRotation = jSONNode["motionControllerHandsRotationOffset"]["y"].AsFloat;
							motionHandModelControl.zRotation = jSONNode["motionControllerHandsRotationOffset"]["z"].AsFloat;
						}
					}
					if (alternateMotionHandModelControl != null)
					{
						if (jSONNode["alternateMotionControllerLeftHandChoice"] != null)
						{
							alternateMotionHandModelControl.leftHandChoice = jSONNode["alternateMotionControllerLeftHandChoice"];
						}
						if (jSONNode["alternateMotionControllerRightHandChoice"] != null)
						{
							alternateMotionHandModelControl.rightHandChoice = jSONNode["alternateMotionControllerRightHandChoice"];
						}
						if (jSONNode["alternateMotionControllerLinkHands"] != null)
						{
							alternateMotionHandModelControl.linkHands = jSONNode["alternateMotionControllerLinkHands"].AsBool;
						}
					}
					if (jSONNode["creatorName"] != null)
					{
						creatorName = jSONNode["creatorName"];
					}
					if (jSONNode["DAZExtraLibraryFolder"] != null)
					{
						DAZExtraLibraryFolder = jSONNode["DAZExtraLibraryFolder"];
					}
					if (jSONNode["DAZDefaultContentFolder"] != null)
					{
						DAZDefaultContentFolder = jSONNode["DAZDefaultContentFolder"];
					}
					if (jSONNode["fileBrowserSortBy"] != null)
					{
						SetFileBrowserSortBy(jSONNode["fileBrowserSortBy"]);
					}
					if (jSONNode["fileBrowserDirectoryOption"] != null)
					{
						SetFileBrowserDirectoryOption(jSONNode["fileBrowserDirectoryOption"]);
					}
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during read of prefs file " + ex);
			}
		}
		_disableSave = false;
		CheckQualityLevels();
	}

	public void ResetPreferences()
	{
		SetQuality("High");
		_disableSave = true;
		firstTimeUser = true;
		desktopVsync = false;
		mirrorToDisplay = false;
		hideExitButton = false;
		showTargetsMenuOnly = false;
		alwaysShowPointersOnTouch = true;
		hideInactiveTargets = true;
		showControllersMenuOnly = false;
		targetAlpha = 1f;
		crosshairAlpha = 0.1f;
		useMonitorViewOffsetWhenUIOpen = true;
		oculusSwapGrabAndTrigger = false;
		oculusDisableFreeMove = false;
		steamVRShowControllers = false;
		steamVRUseControllerHandPose = false;
		steamVRPointerAngle = defaultSteamVRPointerAngle;
		fingerInputFactor = defaultFingerInputFactor;
		thumbInputFactor = defaultThumbInputFactor;
		fingerSpreadOffset = defaultFingerSpreadOffset;
		fingerBendOffset = defaultFingerBendOffset;
		thumbSpreadOffset = defaultThumbSpreadOffset;
		thumbBendOffset = defaultThumbBendOffset;
		shadowFilterLevel = 3f;
		pointLightShadowBlur = 0.5f;
		pointLightShadowBiasBase = 0.015f;
		physicsRate = PhysicsRate.Auto;
		physicsUpdateCap = 2;
		physicsHighQuality = false;
		overlayUI = true;
		useHeadCollider = false;
		optimizeMemoryOnSceneLoad = true;
		optimizeMemoryOnPresetLoad = false;
		enableCaching = true;
		confirmLoad = false;
		flipToolbar = false;
		enableWebBrowser = true;
		allowNonWhitelistDomains = false;
		enableWebBrowserProfile = true;
		enableWebMisc = true;
		enableHub = true;
		enableHubDownloader = true;
		enablePlugins = true;
		allowPluginsNetworkAccess = false;
		alwaysAllowPluginsDownloadedFromHub = false;
		hideDisabledWebMessages = true;
		if (SuperController.singleton != null)
		{
			if (SuperController.singleton.termsOfUseDisabled)
			{
				termsOfUseAccepted = true;
			}
			else
			{
				termsOfUseAccepted = false;
			}
			SuperController.singleton.helpOverlayOn = true;
			SuperController.singleton.lockHeightDuringNavigate = true;
			SuperController.singleton.freeMoveFollowFloor = true;
			SuperController.singleton.teleportAllowRotation = false;
			SuperController.singleton.disableAllNavigation = false;
			SuperController.singleton.disableGrabNavigation = false;
			SuperController.singleton.disableTeleport = false;
			SuperController.singleton.disableTeleportDuringPossess = true;
			SuperController.singleton.freeMoveMultiplier = 1f;
			SuperController.singleton.grabNavigationPositionMultiplier = 1f;
			SuperController.singleton.grabNavigationRotationMultiplier = 0.5f;
			SuperController.singleton.showNavigationHologrid = true;
			SuperController.singleton.hologridTransparency = 0.01f;
			SuperController.singleton.oculusThumbstickFunction = SuperController.ThumbstickFunction.GrabWorld;
			SuperController.singleton.allowPossessSpringAdjustment = true;
			SuperController.singleton.possessPositionSpring = 10000f;
			SuperController.singleton.possessRotationSpring = 1000f;
			SuperController.singleton.loResScreenShotCameraFOV = 40f;
			SuperController.singleton.hiResScreenShotCameraFOV = 40f;
			SuperController.singleton.allowGrabPlusTriggerHandToggle = true;
			SuperController.singleton.monitorUIScale = 1f;
			SuperController.singleton.monitorUIYOffset = 0f;
			SuperController.singleton.UISide = UISideAlign.Side.Right;
			SuperController.singleton.alwaysUseAlternateHands = false;
			SuperController.singleton.useLegacyWorldScaleChange = false;
			SuperController.singleton.onStartupSkipStartScreen = false;
			SuperController.singleton.autoFreezeAnimationOnSwitchToEditMode = false;
			SuperController.singleton.worldUIVRAnchorDistance = 2f;
			SuperController.singleton.worldUIVRAnchorHeight = 0.8f;
			SuperController.singleton.useMonitorRigAudioListenerWhenActive = true;
			SuperController.singleton.generateDepthTexture = false;
			SuperController.singleton.leapMotionEnabled = false;
			SuperController.singleton.openMainHUDOnError = true;
		}
		if (leapHandModelControl != null)
		{
			leapHandModelControl.allowPinchGrab = true;
		}
		if (motionHandModelControl != null)
		{
			motionHandModelControl.xPosition = 0f;
			motionHandModelControl.yPosition = 0f;
			motionHandModelControl.zPosition = 0f;
			motionHandModelControl.xRotation = 0f;
			motionHandModelControl.yRotation = 0f;
			motionHandModelControl.zRotation = 0f;
			motionHandModelControl.useCollision = false;
			motionHandModelControl.rightHandChoice = "SphereKinematic";
			motionHandModelControl.leftHandChoice = "SphereKinematic";
			motionHandModelControl.linkHands = true;
		}
		if (alternateMotionHandModelControl != null)
		{
			alternateMotionHandModelControl.useCollision = false;
			alternateMotionHandModelControl.rightHandChoice = "SphereKinematic";
			alternateMotionHandModelControl.leftHandChoice = "SphereKinematic";
			alternateMotionHandModelControl.linkHands = true;
		}
		_screenResolutionAlert = null;
		_pendingScreenResolutionWidth = 0;
		_pendingScreenResolutionHeight = 0;
		SyncScreenResolutionPopup();
		_disableSave = false;
		SavePreferences();
	}

	private void SyncRenderScale()
	{
		if (XRSettings.eyeTextureResolutionScale != _renderScale)
		{
			XRSettings.eyeTextureResolutionScale = _renderScale;
		}
	}

	private void SyncMsaa()
	{
		if (QualitySettings.antiAliasing != msaaLevel)
		{
			QualitySettings.antiAliasing = msaaLevel;
		}
	}

	private void SyncMsaaPopup()
	{
		if (msaaPopup != null)
		{
			switch (_msaaLevel)
			{
			case 0:
				msaaPopup.currentValue = "Off";
				break;
			case 2:
				msaaPopup.currentValue = "2X";
				break;
			case 4:
				msaaPopup.currentValue = "4X";
				break;
			case 8:
				msaaPopup.currentValue = "8X";
				break;
			}
		}
	}

	public void SetMsaaFromString(string levelString)
	{
		switch (levelString)
		{
		case "Off":
			msaaLevel = 0;
			break;
		case "2X":
			msaaLevel = 2;
			break;
		case "4X":
			msaaLevel = 4;
			break;
		case "8X":
			msaaLevel = 8;
			break;
		}
	}

	public void DisableFirstTimeUser()
	{
		firstTimeUser = false;
	}

	protected void SyncFirstTimeUser()
	{
		bool flag = firstTimeUser;
		if (flag && SuperController.singleton != null)
		{
			SuperController.singleton.CloseAllWorldUIPanels();
			SuperController.singleton.ActivateWorldUI();
		}
		if (firstTimeUserEnableGameObjects != null)
		{
			GameObject[] array = firstTimeUserEnableGameObjects;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(flag);
			}
		}
		if (firstTimeUserDisableGameObjects != null)
		{
			GameObject[] array2 = firstTimeUserDisableGameObjects;
			foreach (GameObject gameObject2 in array2)
			{
				gameObject2.SetActive(!flag);
			}
		}
	}

	public void ReviewTerms()
	{
		FileManager.AssertNotCalledFromPlugin();
		if (!string.IsNullOrEmpty(termsOfUsePath) && File.Exists(termsOfUsePath))
		{
			string fullPath = Path.GetFullPath(termsOfUsePath);
			Process.Start("file://" + fullPath);
		}
	}

	protected void TermsAndSettingsAcceptedPressed()
	{
		if ((bool)SuperController.singleton && termsNotAcceptedGameObject != null)
		{
			termsNotAcceptedGameObject.SetActive(!_termsOfUseAccepted);
		}
		if (_termsOfUseAccepted)
		{
			DisableFirstTimeUser();
		}
	}

	private void SyncDesktopVsync()
	{
		if ((SuperController.singleton == null || SuperController.singleton.IsMonitorOnly) && _desktopVsync)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	private void SyncSmoothPassesPopup()
	{
		if (smoothPassesPopup != null)
		{
			smoothPassesPopup.currentValue = _smoothPasses.ToString();
		}
	}

	public void SetSmoothPassesFromString(string levelString)
	{
		try
		{
			smoothPasses = int.Parse(levelString);
		}
		catch (FormatException)
		{
			UnityEngine.Debug.LogError("Attempted to set smooth passes to " + levelString + " which is not an int");
		}
	}

	private void SyncPixelLightCount()
	{
		if (QualitySettings.pixelLightCount != pixelLightCount)
		{
			QualitySettings.pixelLightCount = pixelLightCount;
		}
	}

	public void SetPixelLightCountFromString(string countString)
	{
		try
		{
			pixelLightCount = int.Parse(countString);
		}
		catch (FormatException)
		{
			UnityEngine.Debug.LogError("Attempted to set pixel light count to " + countString + " which is not an int");
		}
	}

	private void SetInternalShaderLOD()
	{
		Shader.globalMaximumLOD = (int)_shaderLOD;
	}

	public void SetShaderLODFromString(string lod)
	{
		try
		{
			shaderLOD = (ShaderLOD)Enum.Parse(typeof(ShaderLOD), lod);
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set shader lod " + lod + " which is not a valid lod string");
		}
	}

	private const int ScreenResolutionMaxEntries = 6;

	private const float ScreenResolutionAspectTolerance = 0.02f;

	private const float ScreenResolutionAlertSeconds = 10f;

	private static string GetScreenResolutionString(int width, int height)
	{
		return width + "x" + height;
	}

	private static string GetCurrentScreenResolutionString()
	{
		return GetScreenResolutionString(Screen.width, Screen.height);
	}

	private static bool IsVirtualRealityDisplayActive()
	{
		return XRSettings.enabled && XRSettings.isDeviceActive;
	}

	private static bool TryParseScreenResolution(string resolution, out int width, out int height)
	{
		width = 0;
		height = 0;
		if (resolution == null)
		{
			return false;
		}
		string text = resolution.Trim();
		int num = text.IndexOf(' ');
		if (num > 0)
		{
			text = text.Substring(0, num);
		}
		string[] array = text.Split('x', 'X');
		if (array.Length != 2)
		{
			return false;
		}
		int num2;
		int num3;
		if (!int.TryParse(array[0].Trim(), out num2) || !int.TryParse(array[1].Trim(), out num3))
		{
			return false;
		}
		if (num2 <= 0 || num3 <= 0)
		{
			return false;
		}
		width = num2;
		height = num3;
		return true;
	}

	private List<string> BuildScreenResolutionValues()
	{
		List<string> list = new List<string>();
		Resolution currentResolution = Screen.currentResolution;
		float num = ((currentResolution.height > 0) ? (((float)currentResolution.width) / (float)currentResolution.height) : (((float)Screen.width) / (float)Screen.height));
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions != null)
		{
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution resolution = resolutions[i];
				if (resolution.width < 640 || resolution.height < 480)
				{
					continue;
				}
				if (Mathf.Abs(((float)resolution.width) / (float)resolution.height - num) > ScreenResolutionAspectTolerance)
				{
					continue;
				}
				string item = GetScreenResolutionString(resolution.width, resolution.height);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		list.Sort(CompareScreenResolutionsByAreaDescending);
		if (list.Count > ScreenResolutionMaxEntries)
		{
			list.RemoveRange(ScreenResolutionMaxEntries, list.Count - ScreenResolutionMaxEntries);
		}
		string text = GetCurrentScreenResolutionString();
		if (!list.Contains(text))
		{
			list.Insert(0, text);
			if (list.Count > ScreenResolutionMaxEntries + 1)
			{
				list.RemoveRange(ScreenResolutionMaxEntries + 1, list.Count - ScreenResolutionMaxEntries - 1);
			}
		}
		return list;
	}

	private static int GetScreenResolutionArea(string resolution)
	{
		int num;
		int num2;
		if (!TryParseScreenResolution(resolution, out num, out num2))
		{
			return 0;
		}
		return num * num2;
	}

	private static int CompareScreenResolutionsByAreaDescending(string a, string b)
	{
		return GetScreenResolutionArea(b).CompareTo(GetScreenResolutionArea(a));
	}

	private static bool IsScreenResolutionSupported(int width, int height)
	{
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == width && resolutions[i].height == height)
			{
				return true;
			}
		}
		return false;
	}

	private void SyncScreenResolutionPopup()
	{
		SyncScreenResolutionPopup(null);
	}

	private void SyncScreenResolutionPopup(string resolutionOverride)
	{
		string text = ((resolutionOverride != null) ? resolutionOverride : GetCurrentScreenResolutionString());
		_screenResolution = text;
		if (screenResolutionPopup == null)
		{
			return;
		}
		List<string> list = BuildScreenResolutionValues();
		if (!list.Contains(text))
		{
			list.Insert(0, text);
		}
		screenResolutionPopup.SetPopupValues(list.ToArray(), null);
		screenResolutionPopup.currentValueNoCallback = text;
	}

	public void SetScreenResolutionFromString(string resolution)
	{
		ApplyScreenResolution(resolution, true);
	}

	private void ApplyScreenResolution(string resolution, bool prompt)
	{
		int num;
		int num2;
		if (!TryParseScreenResolution(resolution, out num, out num2))
		{
			SyncScreenResolutionPopup();
			return;
		}
		if (IsVirtualRealityDisplayActive())
		{
			SyncScreenResolutionPopup();
			return;
		}
		string text = GetScreenResolutionString(num, num2);
		if (num == Screen.width && num2 == Screen.height)
		{
			SyncScreenResolutionPopup(text);
			return;
		}
		if (prompt)
		{
			ShowScreenResolutionAlert(text);
		}
		Screen.SetResolution(num, num2, Screen.fullScreen);
		_syncedScreenResolutionWidth = num;
		_syncedScreenResolutionHeight = num2;
		_syncedScreenResolutionFullScreen = Screen.fullScreen;
		SyncScreenResolutionPopup(text);
		SavePreferences();
	}

	private void ShowScreenResolutionAlert(string resolution)
	{
		string text = "Screen resolution changed to " + resolution + ".\nKeep this resolution?";
		if (_screenResolutionAlert != null)
		{
			_screenResolutionAlert.SetText(text);
			_screenResolutionAlert.SetAutoClose(ScreenResolutionAlertSeconds, ScreenResolutionAlertRevertCallback);
			return;
		}
		if (SuperController.singleton == null)
		{
			return;
		}
		_pendingScreenResolutionWidth = Screen.width;
		_pendingScreenResolutionHeight = Screen.height;
		AlertUI alertUI = SuperController.singleton.Alert(text, ScreenResolutionAlertKeepCallback, ScreenResolutionAlertRevertCallback, ScreenResolutionAlertSeconds, ScreenResolutionAlertRevertCallback);
		if (alertUI == null)
		{
			_pendingScreenResolutionWidth = 0;
			_pendingScreenResolutionHeight = 0;
			return;
		}
		alertUI.SetOKButtonText("Keep");
		alertUI.SetCancelButtonText("Revert");
		alertUI.autoCloseCountdownFormat = "Reverting to the previous resolution in {0} seconds...";
		alertUI.SetText(text);
		_screenResolutionAlert = alertUI;
	}

	private void ScreenResolutionAlertKeepCallback()
	{
		_screenResolutionAlert = null;
		_pendingScreenResolutionWidth = 0;
		_pendingScreenResolutionHeight = 0;
		SyncScreenResolutionPopup();
	}

	private void ScreenResolutionAlertRevertCallback()
	{
		_screenResolutionAlert = null;
		int num = _pendingScreenResolutionWidth;
		int num2 = _pendingScreenResolutionHeight;
		_pendingScreenResolutionWidth = 0;
		_pendingScreenResolutionHeight = 0;
		if (num > 0 && num2 > 0)
		{
			ApplyScreenResolution(GetScreenResolutionString(num, num2), false);
		}
		else
		{
			SyncScreenResolutionPopup();
		}
	}

	private static Text FindScreenResolutionPopupLabel(UIPopup popup)
	{
		if (popup == null)
		{
			return null;
		}
		if (popup.labelText != null)
		{
			return popup.labelText;
		}
		Text[] componentsInChildren = popup.GetComponentsInChildren<Text>(true);
		if (componentsInChildren == null)
		{
			return null;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Text text = componentsInChildren[i];
			if (text == null)
			{
				continue;
			}
			if (popup.topButton != null && text.transform.IsChildOf(popup.topButton.transform))
			{
				continue;
			}
			if (popup.popupPanel != null && text.transform.IsChildOf(popup.popupPanel))
			{
				continue;
			}
			return text;
		}
		return null;
	}

	private void CreateScreenResolutionPopup()
	{
		if (screenResolutionPopup != null || physicsUpdateCapPopup == null)
		{
			return;
		}
		Transform transform = physicsUpdateCapPopup.transform.parent;
		if (transform == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(physicsUpdateCapPopup.gameObject, transform, false);
		if (gameObject == null)
		{
			return;
		}
		gameObject.name = "Screen Resolution Popup";
		UIPopup component = gameObject.GetComponent<UIPopup>();
		if (component == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		if (component2 != null)
		{
			component2.anchoredPosition = new Vector2(-42.5f, -815f);
		}
		Text text = FindScreenResolutionPopupLabel(component);
		if (text != null)
		{
			text.text = "Screen Resolution";
		}
		screenResolutionPopup = component;
	}

	private void InitScreenResolutionUI()
	{
		CreateScreenResolutionPopup();
		if (screenResolutionPopup == null)
		{
			return;
		}
		UIPopup uIPopup = screenResolutionPopup;
		uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetScreenResolutionFromString));
		screenResolutionPopup.gameObject.SetActive(!IsVirtualRealityDisplayActive());
		_syncedScreenResolutionWidth = Screen.width;
		_syncedScreenResolutionHeight = Screen.height;
		_syncedScreenResolutionFullScreen = Screen.fullScreen;
		int num;
		int num2;
		if (TryParseScreenResolution(_screenResolution, out num, out num2) && IsScreenResolutionSupported(num, num2))
		{
			ApplyScreenResolution(_screenResolution, false);
		}
		else
		{
			if (_screenResolution != null && _screenResolution.Length > 0)
			{
				SuperController.LogError("Screen resolution '" + _screenResolution + "' from the preferences file is not supported by this display, keeping " + GetCurrentScreenResolutionString());
			}
			SyncScreenResolutionPopup();
		}
	}

	private void SyncMirrorReflections()
	{
		MirrorReflection.globalEnabled = _mirrorReflections;
		if (normalCamera != null)
		{
			normalCamera.enabled = !_mirrorReflections;
		}
		if (mirrorReflectionCamera1 != null)
		{
			mirrorReflectionCamera1.enabled = _mirrorReflections;
		}
		if (mirrorReflectionCamera2 != null)
		{
			mirrorReflectionCamera2.enabled = _mirrorReflections;
		}
	}

	private void SyncRealtimeReflectionProbes()
	{
		QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbes;
	}

	private void SyncMirrorToDisplay()
	{
		if (XRSettings.showDeviceView != _mirrorToDisplay)
		{
			XRSettings.showDeviceView = _mirrorToDisplay;
		}
	}

	private void SyncHideExitButton()
	{
		if (exitButton != null)
		{
			exitButton.SetActive(!_hideExitButton);
		}
	}

	public void ShowInactiveTargets()
	{
		hideInactiveTargets = false;
	}

	public void HideInactiveTargets()
	{
		hideInactiveTargets = true;
	}

	private void SyncCrosshairAlpha()
	{
		if (crosshair != null)
		{
			Color color = crosshair.color;
			color.a = _crosshairAlpha;
			crosshair.color = color;
		}
	}

	private void SyncSteamVRControllerModels()
	{
		if (steamVRLeftControllerModel != null)
		{
			steamVRLeftControllerModel.enabled = _steamVRShowControllers;
			foreach (Transform item in steamVRLeftControllerModel.transform)
			{
				item.gameObject.SetActive(_steamVRShowControllers);
			}
		}
		if (!(steamVRRightControllerModel != null))
		{
			return;
		}
		steamVRRightControllerModel.enabled = _steamVRShowControllers;
		foreach (Transform item2 in steamVRRightControllerModel.transform)
		{
			item2.gameObject.SetActive(_steamVRShowControllers);
		}
	}

	private void SyncSteamVRUseControllerHandPose()
	{
		if (steamVRLeftHandSkeleton != null)
		{
			if (_steamVRUseControllerHandPose)
			{
				steamVRLeftHandSkeleton.rangeOfMotion = EVRSkeletalMotionRange.WithController;
			}
			else
			{
				steamVRLeftHandSkeleton.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
			}
		}
		if (steamVRRightHandSkeleton != null)
		{
			if (_steamVRUseControllerHandPose)
			{
				steamVRRightHandSkeleton.rangeOfMotion = EVRSkeletalMotionRange.WithController;
			}
			else
			{
				steamVRRightHandSkeleton.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
			}
		}
	}

	private void SyncSteamVRPointerAngle()
	{
		Vector3 localEulerAngles = default(Vector3);
		localEulerAngles.x = _steamVRPointerAngle;
		localEulerAngles.y = 0f;
		localEulerAngles.z = 0f;
		if (steamVRLeftHandPointer != null)
		{
			steamVRLeftHandPointer.localEulerAngles = localEulerAngles;
		}
		if (steamVRRightHandPointer != null)
		{
			steamVRRightHandPointer.localEulerAngles = localEulerAngles;
		}
	}

	private void SyncFingerInputFactor()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.fingerInputFactor = _fingerInputFactor;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.fingerInputFactor = _fingerInputFactor;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.fingerInputFactor = _fingerInputFactor;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.fingerInputFactor = _fingerInputFactor;
		}
	}

	private void SyncThumbInputFactor()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.thumbInputFactor = _thumbInputFactor;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.thumbInputFactor = _thumbInputFactor;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.thumbInputFactor = _thumbInputFactor;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.thumbInputFactor = _thumbInputFactor;
		}
	}

	private void SyncFingerSpreadOffset()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.fingerSpreadOffset = _fingerSpreadOffset;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.fingerSpreadOffset = _fingerSpreadOffset;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.fingerSpreadOffset = _fingerSpreadOffset;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.fingerSpreadOffset = _fingerSpreadOffset;
		}
	}

	private void SyncFingerBendOffset()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.fingerBendOffset = _fingerBendOffset;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.fingerBendOffset = _fingerBendOffset;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.fingerBendOffset = _fingerBendOffset;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.fingerBendOffset = _fingerBendOffset;
		}
	}

	private void SyncThumbSpreadOffset()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.thumbSpreadOffset = _thumbSpreadOffset;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.thumbSpreadOffset = _thumbSpreadOffset;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.thumbSpreadOffset = _thumbSpreadOffset;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.thumbSpreadOffset = _thumbSpreadOffset;
		}
	}

	private void SyncThumbBendOffset()
	{
		if (steamVRLeftHandInput != null)
		{
			steamVRLeftHandInput.thumbBendOffset = _thumbBendOffset;
		}
		if (steamVRRightHandInput != null)
		{
			steamVRRightHandInput.thumbBendOffset = _thumbBendOffset;
		}
		if (ovrLeftHandInput != null)
		{
			ovrLeftHandInput.thumbBendOffset = _thumbBendOffset;
		}
		if (ovrRightHandInput != null)
		{
			ovrRightHandInput.thumbBendOffset = _thumbBendOffset;
		}
	}

	private void SyncShadowBlur()
	{
		Shader.SetGlobalFloat("_ShadowPointKernel", _pointLightShadowBlur);
	}

	private void SyncShadowBiasBase()
	{
		Shader.SetGlobalFloat("_ShadowPointBiasBase", _pointLightShadowBiasBase);
	}

	private void SyncShadowFilterLevel()
	{
		int value = (int)_shadowFilterLevel;
		Shader.SetGlobalInt("_ShadowFilterLevel", value);
	}

	private void SyncCloseObjectBlur()
	{
	}

	private void SyncSoftPhysics()
	{
		DAZPhysicsMesh.globalEnable = softPhysics;
	}

	public void RegisterGlowObject()
	{
		glowObjectCount++;
		SyncGlow();
	}

	public void DeregisterGlowObject()
	{
		glowObjectCount--;
		SyncGlow();
	}

	public void RegisterGlowCamera(MKGlow mkg)
	{
		dynamicGlow.Add(mkg);
		SyncGlow();
	}

	public void DeregisterGlowCamera(MKGlow mkg)
	{
		dynamicGlow.Remove(mkg);
	}

	private void SyncGlow()
	{
		List<MKGlow> list = new List<MKGlow>();
		MKGlow[] array = glowObjects;
		foreach (MKGlow item in array)
		{
			list.Add(item);
		}
		foreach (MKGlow item2 in dynamicGlow)
		{
			list.Add(item2);
		}
		foreach (MKGlow item3 in list)
		{
			if (_pauseGlow)
			{
				item3.enabled = false;
				continue;
			}
			switch (_glowEffects)
			{
			case GlowEffectsLevel.Off:
				item3.enabled = false;
				break;
			case GlowEffectsLevel.Low:
				item3.enabled = glowObjectCount > 0;
				item3.Samples = 3f;
				break;
			case GlowEffectsLevel.High:
				item3.enabled = glowObjectCount > 0;
				item3.Samples = 2f;
				break;
			}
		}
	}

	public void SetGlowEffectsFromString(string ge)
	{
		try
		{
			glowEffects = (GlowEffectsLevel)Enum.Parse(typeof(GlowEffectsLevel), ge);
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set glow effects level " + ge + " which is not a valid glow effects string");
		}
	}

	private void SetPhysics45()
	{
		Time.fixedDeltaTime = 0.02222222f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 50 * num;
		}
	}

	private void SetPhysics60()
	{
		Time.fixedDeltaTime = 0.01666667f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 30 * num;
		}
	}

	private void SetPhysics72()
	{
		Time.fixedDeltaTime = 0.01388889f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 20 * num;
		}
	}

	private void SetPhysics80()
	{
		Time.fixedDeltaTime = 0.0125f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 17 * num;
		}
	}

	private void SetPhysics90()
	{
		Time.fixedDeltaTime = 1f / 90f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 14 * num;
		}
	}

	private void SetPhysics120()
	{
		Time.fixedDeltaTime = 0.008333333f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 10 * num;
		}
	}

	private void SetPhysics144()
	{
		Time.fixedDeltaTime = 1f / 144f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 8 * num;
		}
	}

	private void SetPhysics240()
	{
		Time.fixedDeltaTime = 0.004166667f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 7 * num;
		}
	}

	private void SetPhysics288()
	{
		Time.fixedDeltaTime = 0.0034722222f;
		int num = 1;
		if (_physicsHighQuality)
		{
			num = 5;
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.solverIterations = 7 * num;
		}
	}

	private void SyncPhysics()
	{
		if (autoPhysicsRateText != null)
		{
			autoPhysicsRateText.text = string.Empty;
		}
		switch (physicsRate)
		{
		case PhysicsRate.Auto:
		{
			if (!XRDevice.isPresent)
			{
				SetPhysics72();
				if (autoPhysicsRateText != null)
				{
					autoPhysicsRateText.text = "72 HZ";
				}
				break;
			}
			float num = XRDevice.refreshRate;
			if (num > 91f)
			{
				num *= 0.5f;
			}
			if (num != 0f)
			{
				if (num < 59f)
				{
					SetPhysics45();
					if (autoPhysicsRateText != null)
					{
						autoPhysicsRateText.text = "45 HZ";
					}
				}
				else if (num < 71f)
				{
					SetPhysics60();
					if (autoPhysicsRateText != null)
					{
						autoPhysicsRateText.text = "60 HZ";
					}
				}
				else if (num < 79f)
				{
					SetPhysics72();
					if (autoPhysicsRateText != null)
					{
						autoPhysicsRateText.text = "72 HZ";
					}
				}
				else if (num < 89f)
				{
					SetPhysics80();
					if (autoPhysicsRateText != null)
					{
						autoPhysicsRateText.text = "80 HZ";
					}
				}
				else
				{
					SetPhysics90();
					if (autoPhysicsRateText != null)
					{
						autoPhysicsRateText.text = "90 HZ";
					}
				}
			}
			else
			{
				SetPhysics90();
				if (autoPhysicsRateText != null)
				{
					autoPhysicsRateText.text = "90 HZ";
				}
			}
			break;
		}
		case PhysicsRate._45:
			SetPhysics45();
			break;
		case PhysicsRate._60:
			SetPhysics60();
			break;
		case PhysicsRate._72:
			SetPhysics72();
			break;
		case PhysicsRate._80:
			SetPhysics80();
			break;
		case PhysicsRate._90:
			SetPhysics90();
			break;
		case PhysicsRate._120:
			SetPhysics120();
			break;
		case PhysicsRate._144:
			SetPhysics144();
			break;
		case PhysicsRate._240:
			SetPhysics240();
			break;
		case PhysicsRate._288:
			SetPhysics288();
			break;
		}
		switch (physicsUpdateCap)
		{
		case 1:
			Time.maximumDeltaTime = Time.fixedDeltaTime;
			break;
		case 2:
			Time.maximumDeltaTime = Time.fixedDeltaTime * 2f;
			break;
		case 3:
			Time.maximumDeltaTime = Time.fixedDeltaTime * 3f;
			break;
		}
		if (Time.maximumDeltaTime > 0.05f)
		{
			Time.maximumDeltaTime = 0.05f;
		}
	}

	public void SetPhysicsRateFromString(string pr)
	{
		try
		{
			physicsRate = (PhysicsRate)Enum.Parse(typeof(PhysicsRate), pr);
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set physics rate " + pr + " which is not a valid physics rate string. Resetting to Auto");
			physicsRate = PhysicsRate.Auto;
		}
	}

	public void SetPhysicsUpdateCapFromString(string pr)
	{
		try
		{
			physicsUpdateCap = int.Parse(pr);
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set physics update cap " + pr + " which is not a valid physics update cap string");
		}
	}

	private void SyncHeadCollider()
	{
		if (headCollider != null)
		{
			headCollider.gameObject.SetActive(_useHeadCollider);
		}
	}

	private void SetCacheFolder(string folder)
	{
		if (folder != null && folder != string.Empty)
		{
			folder = folder.Replace("\\\\", "\\");
			cacheFolder = folder;
		}
	}

	public void BrowseCacheFolder()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetDirectoryPathDialog(SetCacheFolder, _cacheFolder);
		}
	}

	public void ResetCacheFolder()
	{
		CacheManager.ResetCacheDir();
		cacheFolder = CacheManager.GetCacheDir();
	}

	private void SyncOverlayUI()
	{
		if (panelForUIMaterial != null && overlayUIShader != null)
		{
			Material defaultMaterial = panelForUIMaterial.defaultMaterial;
			if (defaultUIShader == null)
			{
				defaultUIShader = defaultMaterial.shader;
			}
			if (_overlayUI)
			{
				defaultMaterial.shader = overlayUIShader;
			}
			else
			{
				defaultMaterial.shader = defaultUIShader;
			}
		}
	}

	protected void SyncEnableWebBrowser()
	{
		if (HubBrowse.singleton != null)
		{
			HubBrowse.singleton.WebBrowserEnabled = _enableWebBrowser;
		}
	}

	protected string ExtractTopDomain(string url)
	{
		string input = url;
		input = Regex.Replace(input, "^https://", string.Empty);
		input = Regex.Replace(input, "^http://", string.Empty);
		input = Regex.Replace(input, "/.*", string.Empty);
		Match match;
		if ((match = Regex.Match(input, "\\.([^\\.]+\\.[^\\.]+)")).Success)
		{
			string value = match.Groups[1].Value;
			input = value;
		}
		return input;
	}

	public bool CheckWhitelistDomain(string url)
	{
		if (url != null && !_allowNonWhitelistDomains)
		{
			if (url == "about:blank")
			{
				return true;
			}
			if (whitelistDomains != null)
			{
				string item = ExtractTopDomain(url);
				return whitelistDomains.Contains(item);
			}
			return false;
		}
		return true;
	}

	public void SyncWhitelistDomains()
	{
		whitelistDomains = new HashSet<string>();
		string[] array = whitelistDomainPaths;
		foreach (string path in array)
		{
			if (!File.Exists(path))
			{
				continue;
			}
			try
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					string aJSON = streamReader.ReadToEnd();
					SimpleJSON.JSONNode jSONNode = JSON.Parse(aJSON);
					JSONArray asArray = jSONNode["sites"].AsArray;
					if (!(asArray != null))
					{
						continue;
					}
					for (int j = 0; j < asArray.Count; j++)
					{
						string item = ExtractTopDomain(asArray[j]);
						if (!whitelistDomains.Contains(item))
						{
							whitelistDomains.Add(item);
						}
					}
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception while reading whitelist sites file " + ex);
			}
		}
	}

	protected void SyncEnableWebBrowserProfile()
	{
		if (_enableWebBrowserProfile)
		{
			try
			{
				Directory.CreateDirectory("BrowserProfile");
				BrowserNative.ProfilePath = Path.GetFullPath("BrowserProfile");
				return;
			}
			catch (Exception)
			{
				SuperController.LogError("VaM must be restarted for the web browser profile to be enabled because the web browser has already been activated.");
				return;
			}
		}
		try
		{
			BrowserNative.ProfilePath = null;
			if (Directory.Exists("BrowserProfile"))
			{
				Directory.Delete("BrowserProfile", true);
			}
		}
		catch (Exception)
		{
			SuperController.LogError("VaM must be restarted for the web browser profile to be disabled and the removal of BrowserProfile directory to take effect because the web browser has already been activated.");
		}
	}

	protected void SyncEnableHub()
	{
		if (HubBrowse.singleton != null)
		{
			HubBrowse.singleton.HubEnabled = _enableHub;
		}
	}

	protected void SyncEnableHubDownloader()
	{
		if (HubDownloader.singleton != null)
		{
			HubDownloader.singleton.HubDownloaderEnabled = _enableHubDownloader;
		}
	}

	private void SyncAllowPluginsNetworkAccess()
	{
		if (_allowPluginsNetworkAccess)
		{
			RuntimeRestrictions.RemoveRuntimeNamespaceRestriction("System.Net");
			RuntimeRestrictions.RemoveRuntimeNamespaceRestriction("UnityEngine.Network");
			RuntimeRestrictions.RemoveRuntimeNamespaceRestriction("UnityEngine.Networking");
		}
		else
		{
			RuntimeRestrictions.AddRuntimeNamespaceRestriction("System.Net");
			RuntimeRestrictions.AddRuntimeNamespaceRestriction("UnityEngine.Network");
			RuntimeRestrictions.AddRuntimeNamespaceRestriction("UnityEngine.Networking");
		}
	}

	protected void SetDAZExtraLibraryFolder(string folder)
	{
		folder = folder.Replace("\\\\", "\\");
		folder = folder.Replace("\\", "/");
		DAZExtraLibraryFolder = folder;
	}

	public void BrowseDAZExtraLibraryFolder()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetDirectoryPathDialog(SetDAZExtraLibraryFolder, _DAZExtraLibraryFolder);
		}
	}

	public void ClearDAZExtraLibraryFolder()
	{
		DAZExtraLibraryFolder = string.Empty;
	}

	protected void SetDAZDefaultContentFolder(string folder)
	{
		folder = folder.Replace("\\\\", "\\");
		folder = folder.Replace("\\", "/");
		DAZDefaultContentFolder = folder;
	}

	public void BrowseDAZDefaultContentFolder()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetDirectoryPathDialog(SetDAZDefaultContentFolder, _DAZDefaultContentFolder);
		}
	}

	public void ClearDAZDefaultContentFolder()
	{
		DAZDefaultContentFolder = string.Empty;
	}

	private void SyncShadows()
	{
		SyncShadowFilterLevel();
		SyncShadowBiasBase();
		SyncShadowBlur();
	}

	public void SetFileBrowserSortBy(string sortByString)
	{
		try
		{
			SortBy sortBy = (SortBy)Enum.Parse(typeof(SortBy), sortByString);
			fileBrowserSortBy = sortBy;
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set sort by to " + sortByString + " which is not a valid type");
		}
	}

	public void SetFileBrowserDirectoryOption(string dirOptionString)
	{
		try
		{
			DirectoryOption directoryOption = (DirectoryOption)Enum.Parse(typeof(DirectoryOption), dirOptionString);
			fileBrowserDirectoryOption = directoryOption;
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogError("Attempted to set directory option to " + dirOptionString + " which is not a valid type");
		}
	}

	private void InitUI()
	{
		if (reviewTermsButton != null)
		{
			reviewTermsButton.onClick.AddListener(ReviewTerms);
		}
		if (termsAndSettingsAcceptedButton != null)
		{
			termsAndSettingsAcceptedButton.onClick.AddListener(TermsAndSettingsAcceptedPressed);
		}
		SyncFirstTimeUser();
		if (termsOfUseAcceptedToggle != null)
		{
			termsOfUseAcceptedToggle.isOn = _termsOfUseAccepted;
			termsOfUseAcceptedToggle.onValueChanged.AddListener(delegate
			{
				termsOfUseAccepted = termsOfUseAcceptedToggle.isOn;
			});
		}
		if (renderScaleSlider != null)
		{
			renderScaleSlider.value = renderScale;
			renderScaleSlider.onValueChanged.AddListener(delegate
			{
				renderScale = renderScaleSlider.value;
			});
			SliderControl component = renderScaleSlider.GetComponent<SliderControl>();
			if (component != null)
			{
				component.defaultValue = 1f;
			}
			SyncRenderScale();
		}
		if (msaaPopup != null)
		{
			UIPopup uIPopup = msaaPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetMsaaFromString));
			SyncMsaaPopup();
			SyncMsaa();
		}
		if (smoothPassesPopup != null)
		{
			UIPopup uIPopup2 = smoothPassesPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetSmoothPassesFromString));
			SyncSmoothPassesPopup();
		}
		if (desktopVsyncToggle != null)
		{
			desktopVsyncToggle.isOn = _desktopVsync;
			desktopVsyncToggle.onValueChanged.AddListener(delegate
			{
				desktopVsync = desktopVsyncToggle.isOn;
			});
			SyncDesktopVsync();
		}
		if (pixelLightCountPopup != null)
		{
			pixelLightCountPopup.currentValue = pixelLightCount.ToString();
			UIPopup uIPopup3 = pixelLightCountPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetPixelLightCountFromString));
			SyncPixelLightCount();
		}
		if (shaderLODPopup != null)
		{
			shaderLODPopup.currentValue = _shaderLOD.ToString();
			UIPopup uIPopup4 = shaderLODPopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetShaderLODFromString));
			SetInternalShaderLOD();
		}
		if (mirrorReflectionsToggle != null)
		{
			mirrorReflectionsToggle.isOn = mirrorReflections;
			mirrorReflectionsToggle.onValueChanged.AddListener(delegate
			{
				mirrorReflections = mirrorReflectionsToggle.isOn;
			});
			SyncMirrorReflections();
		}
		if (realtimeReflectionProbesToggle != null)
		{
			realtimeReflectionProbesToggle.isOn = realtimeReflectionProbes;
			realtimeReflectionProbesToggle.onValueChanged.AddListener(delegate
			{
				realtimeReflectionProbes = realtimeReflectionProbesToggle.isOn;
			});
			SyncRealtimeReflectionProbes();
		}
		if (mirrorToggle != null)
		{
			mirrorToggle.isOn = mirrorToDisplay;
			mirrorToggle.onValueChanged.AddListener(delegate
			{
				mirrorToDisplay = mirrorToggle.isOn;
			});
			SyncMirrorToDisplay();
		}
		if (hideExitButtonToggle != null)
		{
			hideExitButtonToggle.isOn = _hideExitButton;
			hideExitButtonToggle.onValueChanged.AddListener(delegate
			{
				hideExitButton = hideExitButtonToggle.isOn;
			});
		}
		if (showTargetsMenuOnlyToggle != null)
		{
			showTargetsMenuOnlyToggle.isOn = _showTargetsMenuOnly;
			showTargetsMenuOnlyToggle.onValueChanged.AddListener(delegate
			{
				showTargetsMenuOnly = showTargetsMenuOnlyToggle.isOn;
			});
		}
		if (alwaysShowPointersOnTouchToggle != null)
		{
			alwaysShowPointersOnTouchToggle.isOn = _alwaysShowPointersOnTouch;
			alwaysShowPointersOnTouchToggle.onValueChanged.AddListener(delegate
			{
				alwaysShowPointersOnTouch = alwaysShowPointersOnTouchToggle.isOn;
			});
		}
		if (hideInactiveTargetsToggle != null)
		{
			hideInactiveTargetsToggle.isOn = _hideInactiveTargets;
			hideInactiveTargetsToggle.onValueChanged.AddListener(delegate
			{
				hideInactiveTargets = hideInactiveTargetsToggle.isOn;
			});
		}
		if (showControllersMenuOnlyToggle != null)
		{
			showControllersMenuOnlyToggle.isOn = _showControllersMenuOnly;
			showControllersMenuOnlyToggle.onValueChanged.AddListener(delegate
			{
				showControllersMenuOnly = showControllersMenuOnlyToggle.isOn;
			});
		}
		if (targetAlphaSlider != null)
		{
			targetAlphaSlider.value = targetAlpha;
			FreeControllerV3.targetAlpha = _targetAlpha;
			targetAlphaSlider.onValueChanged.AddListener(delegate
			{
				targetAlpha = targetAlphaSlider.value;
			});
			SliderControl component2 = targetAlphaSlider.GetComponent<SliderControl>();
			if (component2 != null)
			{
				component2.defaultValue = 1f;
			}
		}
		if (crosshairAlphaSlider != null)
		{
			crosshairAlphaSlider.value = crosshairAlpha;
			SyncCrosshairAlpha();
			crosshairAlphaSlider.onValueChanged.AddListener(delegate
			{
				crosshairAlpha = crosshairAlphaSlider.value;
			});
			SliderControl component3 = crosshairAlphaSlider.GetComponent<SliderControl>();
			if (component3 != null)
			{
				component3.defaultValue = 0.1f;
			}
		}
		if (useMonitorViewOffsetWhenUIOpenToggle != null)
		{
			useMonitorViewOffsetWhenUIOpenToggle.isOn = _useMonitorViewOffsetWhenUIOpen;
			useMonitorViewOffsetWhenUIOpenToggle.onValueChanged.AddListener(delegate
			{
				useMonitorViewOffsetWhenUIOpen = useMonitorViewOffsetWhenUIOpenToggle.isOn;
			});
		}
		if (steamVRShowControllersToggle != null)
		{
			steamVRShowControllersToggle.isOn = _steamVRShowControllers;
			steamVRShowControllersToggle.onValueChanged.AddListener(delegate
			{
				steamVRShowControllers = steamVRShowControllersToggle.isOn;
			});
		}
		if (steamVRUseControllerHandPoseToggle != null)
		{
			steamVRUseControllerHandPoseToggle.isOn = _steamVRUseControllerHandPose;
			steamVRUseControllerHandPoseToggle.onValueChanged.AddListener(delegate
			{
				steamVRUseControllerHandPose = steamVRUseControllerHandPoseToggle.isOn;
			});
		}
		if (steamVRPointerAngleSlider != null)
		{
			SliderControl component4 = steamVRPointerAngleSlider.GetComponent<SliderControl>();
			if (component4 != null)
			{
				component4.defaultValue = defaultSteamVRPointerAngle;
			}
			steamVRPointerAngleSlider.value = _steamVRPointerAngle;
			steamVRPointerAngleSlider.onValueChanged.AddListener(delegate
			{
				steamVRPointerAngle = steamVRPointerAngleSlider.value;
			});
			SyncSteamVRPointerAngle();
		}
		if (fingerInputFactorSlider != null)
		{
			SliderControl component5 = fingerInputFactorSlider.GetComponent<SliderControl>();
			if (component5 != null)
			{
				component5.defaultValue = defaultFingerInputFactor;
			}
			fingerInputFactorSlider.value = _fingerInputFactor;
			fingerInputFactorSlider.onValueChanged.AddListener(delegate
			{
				fingerInputFactor = fingerInputFactorSlider.value;
			});
			SyncFingerInputFactor();
		}
		if (thumbInputFactorSlider != null)
		{
			SliderControl component6 = thumbInputFactorSlider.GetComponent<SliderControl>();
			if (component6 != null)
			{
				component6.defaultValue = defaultThumbInputFactor;
			}
			thumbInputFactorSlider.value = _thumbInputFactor;
			thumbInputFactorSlider.onValueChanged.AddListener(delegate
			{
				thumbInputFactor = thumbInputFactorSlider.value;
			});
			SyncThumbInputFactor();
		}
		if (fingerSpreadOffsetSlider != null)
		{
			SliderControl component7 = fingerSpreadOffsetSlider.GetComponent<SliderControl>();
			if (component7 != null)
			{
				component7.defaultValue = defaultFingerSpreadOffset;
			}
			fingerSpreadOffsetSlider.value = _fingerSpreadOffset;
			fingerSpreadOffsetSlider.onValueChanged.AddListener(delegate
			{
				fingerSpreadOffset = fingerSpreadOffsetSlider.value;
			});
			SyncFingerSpreadOffset();
		}
		if (fingerBendOffsetSlider != null)
		{
			SliderControl component8 = fingerBendOffsetSlider.GetComponent<SliderControl>();
			if (component8 != null)
			{
				component8.defaultValue = defaultFingerBendOffset;
			}
			fingerBendOffsetSlider.value = _fingerBendOffset;
			fingerBendOffsetSlider.onValueChanged.AddListener(delegate
			{
				fingerBendOffset = fingerBendOffsetSlider.value;
			});
			SyncFingerBendOffset();
		}
		if (thumbSpreadOffsetSlider != null)
		{
			SliderControl component9 = thumbSpreadOffsetSlider.GetComponent<SliderControl>();
			if (component9 != null)
			{
				component9.defaultValue = defaultThumbSpreadOffset;
			}
			thumbSpreadOffsetSlider.value = _thumbSpreadOffset;
			thumbSpreadOffsetSlider.onValueChanged.AddListener(delegate
			{
				thumbSpreadOffset = thumbSpreadOffsetSlider.value;
			});
			SyncThumbSpreadOffset();
		}
		if (thumbBendOffsetSlider != null)
		{
			SliderControl component10 = thumbBendOffsetSlider.GetComponent<SliderControl>();
			if (component10 != null)
			{
				component10.defaultValue = defaultThumbBendOffset;
			}
			thumbBendOffsetSlider.value = _thumbBendOffset;
			thumbBendOffsetSlider.onValueChanged.AddListener(delegate
			{
				thumbBendOffset = thumbBendOffsetSlider.value;
			});
			SyncThumbBendOffset();
		}
		if (oculusSwapGrabAndTriggerToggle != null)
		{
			oculusSwapGrabAndTriggerToggle.isOn = _oculusSwapGrabAndTrigger;
			oculusSwapGrabAndTriggerToggle.onValueChanged.AddListener(delegate
			{
				oculusSwapGrabAndTrigger = oculusSwapGrabAndTriggerToggle.isOn;
			});
		}
		if (oculusDisableFreeMoveToggle != null)
		{
			oculusDisableFreeMoveToggle.isOn = _oculusDisableFreeMove;
			oculusDisableFreeMoveToggle.onValueChanged.AddListener(delegate
			{
				oculusDisableFreeMove = oculusDisableFreeMoveToggle.isOn;
			});
		}
		if (pointLightShadowBlurSlider != null)
		{
			pointLightShadowBlurSlider.value = pointLightShadowBlur;
			SyncShadowBlur();
			pointLightShadowBlurSlider.onValueChanged.AddListener(delegate
			{
				pointLightShadowBlur = pointLightShadowBlurSlider.value;
			});
			SliderControl component11 = pointLightShadowBlurSlider.GetComponent<SliderControl>();
			if (component11 != null)
			{
				component11.defaultValue = 0.5f;
			}
		}
		if (pointLightShadowBiasBaseSlider != null)
		{
			pointLightShadowBiasBaseSlider.value = pointLightShadowBiasBase;
			SyncShadowBiasBase();
			pointLightShadowBiasBaseSlider.onValueChanged.AddListener(delegate
			{
				pointLightShadowBiasBase = pointLightShadowBiasBaseSlider.value;
			});
			SliderControl component12 = pointLightShadowBiasBaseSlider.GetComponent<SliderControl>();
			if (component12 != null)
			{
				component12.defaultValue = 0.01f;
			}
		}
		if (shadowFilterLevelSlider != null)
		{
			shadowFilterLevelSlider.value = shadowFilterLevel;
			SyncShadowFilterLevel();
			shadowFilterLevelSlider.onValueChanged.AddListener(delegate
			{
				shadowFilterLevel = shadowFilterLevelSlider.value;
			});
			SliderControl component13 = shadowFilterLevelSlider.GetComponent<SliderControl>();
			if (component13 != null)
			{
				component13.defaultValue = 3f;
			}
		}
		if (closeObjectBlurToggle != null)
		{
			closeObjectBlurToggle.isOn = closeObjectBlur;
			closeObjectBlurToggle.onValueChanged.AddListener(delegate
			{
				closeObjectBlur = closeObjectBlurToggle.isOn;
			});
			SyncCloseObjectBlur();
		}
		if (physicsRatePopup != null)
		{
			physicsRatePopup.currentValue = physicsRate.ToString();
			UIPopup uIPopup5 = physicsRatePopup;
			uIPopup5.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup5.onValueChangeHandlers, new UIPopup.OnValueChange(SetPhysicsRateFromString));
		}
		if (physicsUpdateCapPopup != null)
		{
			physicsUpdateCapPopup.currentValue = physicsUpdateCap.ToString();
			UIPopup uIPopup6 = physicsUpdateCapPopup;
			uIPopup6.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup6.onValueChangeHandlers, new UIPopup.OnValueChange(SetPhysicsUpdateCapFromString));
		}
		if (physicsHighQualityToggle != null)
		{
			physicsHighQualityToggle.isOn = _physicsHighQuality;
			physicsHighQualityToggle.onValueChanged.AddListener(delegate
			{
				physicsHighQuality = physicsHighQualityToggle.isOn;
			});
		}
		SyncPhysics();
		if (softPhysicsToggle != null)
		{
			softPhysicsToggle.isOn = softPhysics;
			softPhysicsToggle.onValueChanged.AddListener(delegate
			{
				softPhysics = softPhysicsToggle.isOn;
			});
			SyncSoftPhysics();
		}
		if (glowEffectsPopup != null)
		{
			glowEffectsPopup.currentValue = _glowEffects.ToString();
			UIPopup uIPopup7 = glowEffectsPopup;
			uIPopup7.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup7.onValueChangeHandlers, new UIPopup.OnValueChange(SetGlowEffectsFromString));
			SyncGlow();
		}
		if (ultraLowQualityToggle != null)
		{
			ultraLowQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && ultraLowQualityToggle.isOn)
				{
					SetQuality("UltraLow");
				}
			});
		}
		if (lowQualityToggle != null)
		{
			lowQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && lowQualityToggle.isOn)
				{
					SetQuality("Low");
				}
			});
		}
		if (midQualityToggle != null)
		{
			midQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && midQualityToggle.isOn)
				{
					SetQuality("Mid");
				}
			});
		}
		if (highQualityToggle != null)
		{
			highQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && highQualityToggle.isOn)
				{
					SetQuality("High");
				}
			});
		}
		if (ultraQualityToggle != null)
		{
			ultraQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && ultraQualityToggle.isOn)
				{
					SetQuality("Ultra");
				}
			});
		}
		if (maxQualityToggle != null)
		{
			maxQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && maxQualityToggle.isOn)
				{
					SetQuality("Max");
				}
			});
		}
		if (customQualityToggle != null)
		{
			customQualityToggle.onValueChanged.AddListener(delegate
			{
				if (!_disableToggles && customQualityToggle.isOn)
				{
					CheckQualityLevels();
				}
			});
		}
		if (useHeadColliderToggle != null)
		{
			useHeadColliderToggle.isOn = _useHeadCollider;
			useHeadColliderToggle.onValueChanged.AddListener(delegate
			{
				useHeadCollider = useHeadColliderToggle.isOn;
			});
			SyncHeadCollider();
		}
		if (optimizeMemoryOnSceneLoadToggle != null)
		{
			optimizeMemoryOnSceneLoadToggle.isOn = _optimizeMemoryOnSceneLoad;
			optimizeMemoryOnSceneLoadToggle.onValueChanged.AddListener(delegate
			{
				optimizeMemoryOnSceneLoad = optimizeMemoryOnSceneLoadToggle.isOn;
			});
		}
		if (optimizeMemoryOnPresetLoadToggle != null)
		{
			optimizeMemoryOnPresetLoadToggle.isOn = _optimizeMemoryOnPresetLoad;
			optimizeMemoryOnPresetLoadToggle.onValueChanged.AddListener(delegate
			{
				optimizeMemoryOnPresetLoad = optimizeMemoryOnPresetLoadToggle.isOn;
			});
		}
		if (enableCachingToggle != null)
		{
			enableCachingToggle.isOn = _enableCaching;
			enableCachingToggle.onValueChanged.AddListener(delegate
			{
				enableCaching = enableCachingToggle.isOn;
			});
		}
		if (browseCacheFolderButton != null)
		{
			browseCacheFolderButton.onClick.AddListener(BrowseCacheFolder);
		}
		if (resetCacheFolderButton != null)
		{
			resetCacheFolderButton.onClick.AddListener(ResetCacheFolder);
		}
		if (cacheFolderText != null)
		{
			cacheFolderText.text = _cacheFolder;
		}
		if (confirmLoadToggle != null)
		{
			confirmLoadToggle.isOn = _confirmLoad;
			confirmLoadToggle.onValueChanged.AddListener(delegate
			{
				confirmLoad = confirmLoadToggle.isOn;
			});
		}
		if (flipToolbarToggle != null)
		{
			flipToolbarToggle.isOn = _flipToolbar;
			flipToolbarToggle.onValueChanged.AddListener(delegate
			{
				flipToolbar = flipToolbarToggle.isOn;
			});
		}
		if (enableWebBrowserToggle != null)
		{
			enableWebBrowserToggle.isOn = _enableWebBrowser;
			enableWebBrowserToggle.onValueChanged.AddListener(delegate
			{
				enableWebBrowser = enableWebBrowserToggle.isOn;
			});
			SyncEnableWebBrowser();
		}
		if (enableWebBrowserToggleAlt != null)
		{
			enableWebBrowserToggleAlt.isOn = _enableWebBrowser;
			enableWebBrowserToggleAlt.onValueChanged.AddListener(delegate
			{
				enableWebBrowser = enableWebBrowserToggleAlt.isOn;
			});
		}
		if (allowNonWhitelistDomainsToggle != null)
		{
			allowNonWhitelistDomainsToggle.isOn = _allowNonWhitelistDomains;
			allowNonWhitelistDomainsToggle.onValueChanged.AddListener(delegate
			{
				allowNonWhitelistDomains = allowNonWhitelistDomainsToggle.isOn;
			});
		}
		if (allowNonWhitelistDomainsToggleAlt != null)
		{
			allowNonWhitelistDomainsToggleAlt.isOn = _allowNonWhitelistDomains;
			allowNonWhitelistDomainsToggleAlt.onValueChanged.AddListener(delegate
			{
				allowNonWhitelistDomains = allowNonWhitelistDomainsToggleAlt.isOn;
			});
		}
		if (allowNonWhitelistDomainsToggleAlt2 != null)
		{
			allowNonWhitelistDomainsToggleAlt2.isOn = _allowNonWhitelistDomains;
			allowNonWhitelistDomainsToggleAlt2.onValueChanged.AddListener(delegate
			{
				allowNonWhitelistDomains = allowNonWhitelistDomainsToggleAlt2.isOn;
			});
		}
		if (allowNonWhitelistDomainsToggleAlt3 != null)
		{
			allowNonWhitelistDomainsToggleAlt3.isOn = _allowNonWhitelistDomains;
			allowNonWhitelistDomainsToggleAlt3.onValueChanged.AddListener(delegate
			{
				allowNonWhitelistDomains = allowNonWhitelistDomainsToggleAlt3.isOn;
			});
		}
		if (enableWebBrowserProfileToggle != null)
		{
			enableWebBrowserProfileToggle.isOn = _enableWebBrowserProfile;
			enableWebBrowserProfileToggle.onValueChanged.AddListener(delegate
			{
				enableWebBrowserProfile = enableWebBrowserProfileToggle.isOn;
			});
		}
		if (enableWebBrowserProfileToggleAlt != null)
		{
			enableWebBrowserProfileToggleAlt.isOn = _enableWebBrowserProfile;
			enableWebBrowserProfileToggleAlt.onValueChanged.AddListener(delegate
			{
				enableWebBrowserProfile = enableWebBrowserProfileToggleAlt.isOn;
			});
		}
		if (enableWebMiscToggle != null)
		{
			enableWebMiscToggle.isOn = _enableWebMisc;
			enableWebMiscToggle.onValueChanged.AddListener(delegate
			{
				enableWebMisc = enableWebMiscToggle.isOn;
			});
		}
		if (enableWebMiscToggleAlt != null)
		{
			enableWebMiscToggleAlt.isOn = _enableWebMisc;
			enableWebMiscToggleAlt.onValueChanged.AddListener(delegate
			{
				enableWebMisc = enableWebMiscToggleAlt.isOn;
			});
		}
		if (enableHubToggle != null)
		{
			enableHubToggle.isOn = _enableHub;
			enableHubToggle.onValueChanged.AddListener(delegate
			{
				enableHub = enableHubToggle.isOn;
			});
			SyncEnableHub();
		}
		if (enableHubToggleAlt != null)
		{
			enableHubToggleAlt.isOn = _enableHub;
			enableHubToggleAlt.onValueChanged.AddListener(delegate
			{
				enableHub = enableHubToggleAlt.isOn;
			});
		}
		if (enableHubDownloaderToggle != null)
		{
			enableHubDownloaderToggle.isOn = _enableHubDownloader;
			enableHubDownloaderToggle.onValueChanged.AddListener(delegate
			{
				enableHubDownloader = enableHubDownloaderToggle.isOn;
			});
			SyncEnableHubDownloader();
		}
		if (enableHubDownloaderToggleAlt != null)
		{
			enableHubDownloaderToggleAlt.isOn = _enableHubDownloader;
			enableHubDownloaderToggleAlt.onValueChanged.AddListener(delegate
			{
				enableHubDownloader = enableHubDownloaderToggleAlt.isOn;
			});
		}
		if (enablePluginsToggle != null)
		{
			enablePluginsToggle.isOn = _enablePlugins;
			enablePluginsToggle.onValueChanged.AddListener(delegate
			{
				enablePlugins = enablePluginsToggle.isOn;
			});
		}
		if (enablePluginsToggleAlt != null)
		{
			enablePluginsToggleAlt.isOn = _enablePlugins;
			enablePluginsToggleAlt.onValueChanged.AddListener(delegate
			{
				enablePlugins = enablePluginsToggleAlt.isOn;
			});
		}
		if (allowPluginsNetworkAccessToggle != null)
		{
			allowPluginsNetworkAccessToggle.isOn = _allowPluginsNetworkAccess;
			allowPluginsNetworkAccessToggle.onValueChanged.AddListener(delegate
			{
				allowPluginsNetworkAccess = allowPluginsNetworkAccessToggle.isOn;
			});
		}
		if (allowPluginsNetworkAccessToggleAlt != null)
		{
			allowPluginsNetworkAccessToggleAlt.isOn = _allowPluginsNetworkAccess;
			allowPluginsNetworkAccessToggleAlt.onValueChanged.AddListener(delegate
			{
				allowPluginsNetworkAccess = allowPluginsNetworkAccessToggleAlt.isOn;
			});
		}
		SyncAllowPluginsNetworkAccess();
		if (alwaysAllowPluginsDownloadedFromHubToggle != null)
		{
			alwaysAllowPluginsDownloadedFromHubToggle.isOn = _alwaysAllowPluginsDownloadedFromHub;
			alwaysAllowPluginsDownloadedFromHubToggle.onValueChanged.AddListener(delegate
			{
				alwaysAllowPluginsDownloadedFromHub = alwaysAllowPluginsDownloadedFromHubToggle.isOn;
			});
		}
		if (alwaysAllowPluginsDownloadedFromHubToggleAlt != null)
		{
			alwaysAllowPluginsDownloadedFromHubToggleAlt.isOn = _alwaysAllowPluginsDownloadedFromHub;
			alwaysAllowPluginsDownloadedFromHubToggleAlt.onValueChanged.AddListener(delegate
			{
				alwaysAllowPluginsDownloadedFromHub = alwaysAllowPluginsDownloadedFromHubToggleAlt.isOn;
			});
		}
		if (hideDisabledWebMessagesToggle != null)
		{
			hideDisabledWebMessagesToggle.isOn = _hideDisabledWebMessages;
			hideDisabledWebMessagesToggle.onValueChanged.AddListener(delegate
			{
				hideDisabledWebMessages = hideDisabledWebMessagesToggle.isOn;
			});
		}
		if (overlayUIToggle != null)
		{
			overlayUIToggle.isOn = _overlayUI;
			overlayUIToggle.onValueChanged.AddListener(delegate
			{
				overlayUI = overlayUIToggle.isOn;
			});
			SyncOverlayUI();
		}
		if (creatorNameInputField != null)
		{
			creatorNameInputField.text = _creatorName;
			creatorNameInputField.onEndEdit.AddListener(delegate(string s)
			{
				creatorName = s;
			});
		}
		if (browseDAZExtraLibraryFolderButton != null)
		{
			browseDAZExtraLibraryFolderButton.onClick.AddListener(BrowseDAZExtraLibraryFolder);
		}
		if (clearDAZExtraLibraryFolderButton != null)
		{
			clearDAZExtraLibraryFolderButton.onClick.AddListener(ClearDAZExtraLibraryFolder);
		}
		if (DAZExtraLibraryFolderText != null)
		{
			DAZExtraLibraryFolderText.text = _DAZExtraLibraryFolder;
		}
		if (browseDAZDefaultContentFolderButton != null)
		{
			browseDAZDefaultContentFolderButton.onClick.AddListener(BrowseDAZDefaultContentFolder);
		}
		if (clearDAZDefaultContentFolderButton != null)
		{
			clearDAZDefaultContentFolderButton.onClick.AddListener(ClearDAZDefaultContentFolder);
		}
		if (DAZDefaultContentFolderText != null)
		{
			DAZDefaultContentFolderText.text = _DAZDefaultContentFolder;
		}
		InitScreenResolutionUI();
	}

	private void Awake()
	{
		singleton = this;
		dynamicGlow = new List<MKGlow>();
	}

	private void Start()
	{
		_cacheFolder = CacheManager.GetCacheDir();
		CacheManager.CachingEnabled = _enableCaching;
		if (Application.isPlaying)
		{
			if (shouldLoadPrefsFileOnStart)
			{
				SyncEnableWebBrowserProfile();
				RestorePreferences();
			}
			FileManager.SyncJSONCache();
			InitUI();
			CheckQualityLevels();
			SyncWhitelistDomains();
		}
		else
		{
			SyncShadows();
		}
	}

	private void Update()
	{
		if (screenResolutionPopup == null)
		{
			return;
		}
		bool flag = !IsVirtualRealityDisplayActive();
		if (screenResolutionPopup.gameObject.activeSelf != flag)
		{
			screenResolutionPopup.gameObject.SetActive(flag);
		}
		if (Screen.width == _syncedScreenResolutionWidth && Screen.height == _syncedScreenResolutionHeight && Screen.fullScreen == _syncedScreenResolutionFullScreen)
		{
			return;
		}
		_syncedScreenResolutionWidth = Screen.width;
		_syncedScreenResolutionHeight = Screen.height;
		_syncedScreenResolutionFullScreen = Screen.fullScreen;
		SyncScreenResolutionPopup();
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			SyncShadows();
		}
	}
}
