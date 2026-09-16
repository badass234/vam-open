using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AssetBundles;
using Battlehub.RTCommon;
using DynamicCSharp;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Leap.Unity;
using MHLab.PATCH.Settings;
using MHLab.PATCH.Utilities;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using MVR.Hub;
using MeshVR;
using MeshVR.Hands;
using Mono.CSharp;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using uFileBrowser;

public class SuperController : MonoBehaviour
{
	public enum KeyType
	{
		Invalid,
		Free,
		Teaser,
		Entertainer,
		NSteam,
		Steam,
		Retail,
		Creator
	}

	public delegate void ScreenShotCallback(string imgPath);

	public enum GameMode
	{
		Edit,
		Play
	}

	public enum DisplayUIChoice
	{
		Auto,
		Normal,
		World
	}

	public enum AlignRotationOffset
	{
		None,
		Right,
		Left
	}

	public enum ActiveUI
	{
		None,
		MainMenu,
		MainMenuOnly,
		SelectedOptions,
		MultiButtonPanel,
		EmbeddedScenePanel,
		OnlineBrowser,
		PackageBuilder,
		PackageManager,
		PackageDownloader,
		Custom
	}

	public enum SelectMode
	{
		Off,
		FilteredTargets,
		Targets,
		Controller,
		ForceReceiver,
		ForceProducer,
		Rigidbody,
		Atom,
		Possess,
		TwoStagePossess,
		PossessAndAlign,
		Unpossess,
		AnimationRecord,
		ArmedForRecord,
		Teleport,
		FreeMove,
		FreeMoveMouse,
		SaveScreenshot,
		Screenshot,
		Custom,
		CustomWithTargetControl,
		CustomWithVRTargetControl
	}

	public delegate void SelectControllerCallback(FreeControllerV3 fc);

	public delegate void SelectForceProducerCallback(ForceProducerV2 fp);

	public delegate void SelectForceReceiverCallback(ForceReceiver fr);

	public delegate void SelectRigidbodyCallback(Rigidbody rb);

	public delegate void SelectAtomCallback(Atom a);

	public delegate void OnForceReceiverNamesChanged(string[] receiverNames);

	public delegate void OnForceProducerNamesChanged(string[] producerNames);

	public delegate void OnRhythmControllerNamesChanged(string[] controllerNames);

	public delegate void OnAudioSourceControlNamesChanged(string[] controlNames);

	public delegate void OnFreeControllerNamesChanged(string[] controllerNames);

	public delegate void OnRigidbodyNamesChanged(string[] rigidbodyNames);

	[Serializable]
	public class AtomAsset
	{
		public string assetBundleName;

		public string assetName;

		public string category;
	}

	public delegate void OnAtomUIDsChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithForceReceiversChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithForceProducersChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithFreeControllersChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithRigidbodiesChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDRename(string oldName, string newName);

	public delegate void OnAtomParentChanged(Atom atom, Atom newParent);

	public delegate void OnAtomSubSceneChanged(Atom atom, SubScene newSubScene);

	public delegate void OnAtomAdded(Atom atom);

	public delegate void OnAtomRemoved(Atom atom);

	public delegate void OnGameModeChanged(GameMode gameMode);

	public delegate void OnSceneLoaded();

	public delegate void OnSubSceneLoaded(SubScene subScene);

	public delegate void OnBeforeSceneSave();

	public delegate void OnSceneSaved();

	public enum ThumbstickFunction
	{
		GrabWorld,
		SwapAxis,
		Both
	}

	private static SuperController _singleton;

	public string editorMimicVersion = "1.20.0.3";

	public KeyType editorMimicHighestKey;

	public RectTransform dynamicButtonPrefab;

	public RectTransform dynamicTogglePrefab;

	public RectTransform dynamicSliderPrefab;

	[Tooltip("Add Atom Tab, Animation Tab, Audio Tab")]
	public bool disableAdvancedSceneEdit;

	public bool disableSaveSceneButton;

	public bool disableLoadSceneButton;

	public bool disablePackages;

	public bool disableCustomUI;

	public bool disableBrowse;

	public bool disablePromotional;

	public bool disableKeyInformation;

	public bool disableHub;

	public bool disableTermsOfUse;

	public string demoScenesDir = "Saves/scene/MeshedVR/DemoScenes";

	public string[] demoPackagePrefixes;

	public Transform[] loadSceneButtons;

	public Transform[] loadSceneDisabledButtons;

	public Transform[] onlineBrowseSceneButtons;

	public Transform[] onlineBrowseSceneDisabledButtons;

	public Transform[] saveSceneButtons;

	public Transform[] saveSceneDisabledButtons;

	public Transform[] advancedSceneEditButtons;

	public Transform[] advancedSceneEditDisabledButtons;

	public Transform[] advancedSceneEditOnlyEditModeTransforms;

	public Transform[] advancedSceneEditDisabledEditModeTransforms;

	public Transform[] keyInformationTransforms;

	public Transform[] promotionalTransforms;

	public Transform[] hubDisabledTransforms;

	public Transform[] hubDisabledEnableTransforms;

	public Transform[] termsOfUseTransforms;

	[SerializeField]
	protected string _freeKey;

	[SerializeField]
	protected string _teaserKey;

	[SerializeField]
	protected string _entertainerKey;

	[SerializeField]
	protected string _creatorKey;

	[SerializeField]
	protected string _steamKey;

	[SerializeField]
	protected string _nsteamKey;

	[SerializeField]
	protected string _retailKey;

	[SerializeField]
	protected string _keyFilePath;

	[SerializeField]
	protected string _legacySteamKeyFilePath;

	public InputField keyInputField;

	public InputFieldAction keyInputFieldAction;

	public Text keyEntryStatus;

	public Transform freeKeyTransform;

	public Transform teaserKeyTransform;

	public Transform entertainerKeyTransform;

	public Transform creatorKeyTransform;

	public Transform keySyncingIndicator;

	public string savesDir = "Saves\\";

	public string savesDirEditor = "Saves\\";

	protected string lastLoadDir = string.Empty;

	protected string loadedName;

	protected bool _isLoading;

	public PackageBuilder packageBuilder;

	public Transform packageBuilderUI;

	public PackageBuilder packageManager;

	public Transform packageManagerUI;

	public HubDownloader packageDownloader;

	public Transform loadConfirmPanel;

	public Button loadConfirmButton;

	public Text loadConfirmPathText;

	public Transform overwriteConfirmPanel;

	public Button overwriteConfirmButton;

	public Text overwriteConfirmPathText;

	protected List<Atom> _saveQueue;

	public bool packageMode;

	protected ZipOutputStream zos;

	protected Dictionary<string, bool> alreadyPackaged;

	protected HashSet<string> referencedVarPackages;

	public JSONNode loadJson;

	private AsyncFlag loadFlag;

	protected bool loadSceneWorldDialogActivatedFromWorld;

	protected bool loadTemplateWorldDialogActivatedFromWorld;

	protected bool hubOpenedFromWorld;

	public bool enableStartScene = true;

	public JSONEmbed startJSONEmbedScene;

	public JSONEmbed newJSONEmbedScene;

	public bool disableUI;

	public bool alwaysEnablePointers;

	public bool disableNavigation;

	public bool disableVR;

	private bool onStartScene;

	public string startSceneName = "scene/default.json";

	public string startSceneAltName = "scene/MeshedVR/default.json";

	public string newSceneName = "scene/default.json";

	public string newSceneAltName = "scene/MeshedVR/default.json";

	public string[] editorSceneList;

	public PresetManagerControl sessionPresetManagerControl;

	public Transform atomContainer;

	public Transform atomPoolContainer;

	protected Dictionary<string, List<Atom>> typeToAtomPool;

	protected string lastMediaDir = string.Empty;

	protected string lastScenePathDir = string.Empty;

	protected string lastBrowseDir = string.Empty;

	protected string[] obsoleteDirectories = new string[27]
	{
		"Saves/scene/MeshedVR/1.0Source", "Saves/scene/MeshedVR/1.1Source", "Saves/scene/MeshedVR/1.2Source", "Saves/scene/MeshedVR/1.3Source", "Saves/scene/MeshedVR/1.4Source", "Saves/scene/MeshedVR/1.5Source", "Saves/scene/MeshedVR/1.6Source", "Saves/scene/MeshedVR/1.7Source", "Saves/scene/MeshedVR/1.8Source", "Saves/scene/MeshedVR/1.9Source",
		"Saves/scene/MeshedVR/1.10Source", "Saves/scene/MeshedVR/1.11Source", "Saves/scene/MeshedVR/1.12Source", "Saves/scene/MeshedVR/1.13Source", "Saves/scene/MeshedVR/1.15Source", "Saves/scene/MeshedVR/1.16Source", "Saves/scene/MeshedVR/Bonus", "Saves/scene/MeshedVR/Tests", "Saves/Person/appearance/MeshedVR", "Saves/Person/appearance/NutkinChan Looks Mega Pack",
		"Saves/Person/full/MeshedVR", "Saves/Person/pose/MeshedVR", "Saves/Person/pose/NutkinChan Pose Mega Pack", "Custom/Atom/Person/Appearance/MVR", "Custom/Atom/Person/Clothing/MVR", "Custom/Atom/Person/Hair/MVR", "Custom/Atom/Person/Morphs/MVR"
	};

	public Transform obsoletePathsPanel;

	public Text obsoletePathsText;

	private List<string> directoriesToRemove;

	protected Dictionary<string, string> pathMigrationMappings;

	private List<string> legacyDirectories;

	public Transform migratePathsPanel;

	public Text oldPathsText;

	public Text newPathsText;

	protected Dictionary<string, string> filesToMigrateMap;

	public Camera screenshotCamera;

	public Transform screenshotPreview;

	public Camera hiResScreenshotCamera;

	public Transform hiResScreenshotPreview;

	public Slider loResScreenShotCameraFOVSlider;

	[SerializeField]
	private float _loResScreenShotCameraFOV = 40f;

	public Slider hiResScreenShotCameraFOVSlider;

	[SerializeField]
	private float _hiResScreenShotCameraFOV = 40f;

	protected string savingName;

	protected ScreenShotCallback screenShotCallback;

	[SerializeField]
	private GameMode _gameMode;

	public Toggle editModeToggle;

	public Toggle playModeToggle;

	public Transform[] editModeOnlyTransforms;

	public Transform[] playModeOnlyTransforms;

	public Transform errorLogPanel;

	protected string errorLog;

	public InputField allErrorsInputField;

	public InputField allErrorsInputField2;

	public Text allErrorsCountText;

	public Text allErrorsCountText2;

	public Transform errorSplashTransform;

	protected float errorSplashTimeRemaining;

	protected float errorSplashTime = 5f;

	public Transform msgLogPanel;

	protected string msgLog;

	public InputField allMessagesInputField;

	public InputField allMessagesInputField2;

	public Text allMessagesCountText;

	public Text allMessagesCountText2;

	public Transform msgSplashTransform;

	protected float msgSplashTimeRemaining;

	protected float msgSplashTime = 5f;

	public Transform normalAlertRoot;

	public Transform worldAlertRoot;

	public GameObject okAlertPrefab;

	public GameObject okAndCancelAlertPrefab;

	protected int _errorCount;

	protected bool errorLogDirty;

	protected int maxLength = 10000;

	public Toggle openMainHUDOnErrorToggle;

	[SerializeField]
	private bool _openMainHUDOnError = true;

	protected int _msgCount;

	protected bool msgLogDirty;

	protected bool hasPendingErrorSplash;

	public float maxAngularVelocity = 20f;

	public float maxDepenetrationVelocity = 1f;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected bool _resetSimulation;

	public Transform waitTransform;

	public Text[] waitReasonTexts;

	protected int pauseFrames;

	protected bool hideWaitTransform;

	protected bool hiddenReset;

	protected bool readyToResumeSimulation;

	protected List<AsyncFlag> removeSimulationFlags;

	protected AsyncFlag resetSimulationTimerFlag;

	protected List<AsyncFlag> waitResumeAutoSimulationFlags;

	protected bool _autoSimulation = true;

	protected List<AsyncFlag> removeAutoSimulationFlags;

	protected AsyncFlag pauseAutoSimulationFlag;

	protected bool _pauseAutoSimulation;

	public Toggle pauseAutoSimulationToggle;

	protected List<AsyncFlag> waitResumeRenderFlags;

	protected bool _render = true;

	protected List<AsyncFlag> removeRenderFlags;

	protected AsyncFlag pauseRenderFlag;

	protected bool _pauseRender;

	public Toggle pauseRenderToggle;

	protected List<AsyncFlag> removeHoldFlags;

	protected List<AsyncFlag> holdLoadCompleteFlags;

	public Transform loadingIcon;

	protected List<AsyncFlag> removeLoadFlags;

	protected List<AsyncFlag> loadingIconFlags;

	public Toggle autoFreezeAnimationOnSwitchToEditModeToggle;

	protected bool _autoFreezeAnimationOnSwitchToEditMode;

	public Toggle freezeAnimationToggle;

	public Toggle freezeAnimationToggleAlt;

	private bool _freezeAnimation;

	public Transform worldUI;

	public Transform topWorldUI;

	public HUDAnchor worldUIAnchor;

	public Transform mainHUD;

	public Transform mainMenuUI;

	public UITabSelector mainMenuTabSelector;

	public UITabSelector userPrefsTabSelector;

	public Transform loadingUI;

	public Transform loadingUIAlt;

	public Transform loadingGeometry;

	public Slider loadingProgressSlider;

	public Slider loadingProgressSliderAlt;

	public Text loadingTextStatus;

	public Text loadingTextStatusAlt;

	public Transform sceneControlUI;

	public Transform sceneControlUIAlt;

	public Transform wizardWorldUI;

	public FileBrowser fileBrowserUI;

	public FileBrowser fileBrowserWorldUI;

	public FileBrowser templatesFileBrowserWorldUI;

	public FileBrowser mediaFileBrowserUI;

	public FileBrowser directoryBrowserUI;

	public MultiButtonPanel multiButtonPanel;

	public VRWebBrowser onlineBrowser;

	public Transform onlineBrowserUI;

	public HubBrowse hubBrowser;

	public Transform embeddedSceneUI;

	public float targetAlpha;

	public Material rayLineMaterialRight;

	public Material rayLineMaterialLeft;

	private bool drawRayLineLeft;

	private bool drawRayLineRight;

	private LineDrawer rayLineDrawerRight;

	private LineDrawer rayLineDrawerLeft;

	public LineRenderer rayLineRight;

	public LineRenderer rayLineLeft;

	public float rayLineWidth = 0.004f;

	public Toggle quickSelectAlignToggle;

	public Toggle quickSelectMoveToggle;

	public Toggle quickSelectOpenUIToggle;

	public Toggle selectAtomOnAddToggle;

	public Toggle focusAtomOnAddToggle;

	public UIPopup selectAtomPopup;

	public UIPopup selectControllerPopup;

	public Material twoStageLineMaterial;

	private LineDrawer rightTwoStageLineDrawer;

	private LineDrawer leftTwoStageLineDrawer;

	private LineDrawer headTwoStageLineDrawer;

	private LineDrawer leapRightTwoStageLineDrawer;

	private LineDrawer leapLeftTwoStageLineDrawer;

	private LineDrawer tracker1TwoStageLineDrawer;

	private LineDrawer tracker2TwoStageLineDrawer;

	private LineDrawer tracker3TwoStageLineDrawer;

	private LineDrawer tracker4TwoStageLineDrawer;

	private LineDrawer tracker5TwoStageLineDrawer;

	private LineDrawer tracker6TwoStageLineDrawer;

	private LineDrawer tracker7TwoStageLineDrawer;

	private LineDrawer tracker8TwoStageLineDrawer;

	public string version = "UNOFFICIAL";

	public Text versionText;

	public int oldestMajorVersion = 20;

	protected string resolvedVersion;

	public string[] specificMilestoneVersionDefines;

	protected List<string> resolvedVersionDefines;

	protected bool foundVersion;

	protected string lastCycleSelectAtomType;

	protected string lastCycleSelectAtomUid;

	public UIPopup alignRotationOffsetPopup;

	protected AlignRotationOffset _alignRotationOffset = AlignRotationOffset.Left;

	public Toggle worldUIShowOverlaySkyToggle;

	protected bool _worldUIShowOverlaySky = true;

	protected AsyncFlag worldUIActiveFlag;

	protected float _worldUIMonitorAnchorDistance = 2.5f;

	protected float _worldUIMonitorAnchorHeight = 0.8f;

	public Slider worldUIVRAnchorDistanceSlider;

	protected float _worldUIVRAnchorDistance = 2f;

	public Slider worldUIVRAnchorHeightSlider;

	protected float _worldUIVRAnchorHeight = 0.8f;

	private ActiveUI _lastActiveUI;

	private Transform customUI;

	public Transform alternateCustomUI;

	private ActiveUI _activeUI = ActiveUI.SelectedOptions;

	public Toggle helpToggle;

	public Toggle helpToggleAlt;

	public Transform helpOverlayOVR;

	public Transform helpOverlayVive;

	private bool _helpOverlayOnAux = true;

	[SerializeField]
	private bool _helpOverlayOn = true;

	protected string tempHelpText;

	public Text helpHUDText;

	protected string _helpText;

	protected Color _helpColor;

	public Transform leftHand;

	public Transform leftHandAlternate;

	public Transform rightHand;

	public Transform rightHandAlternate;

	public HandModelControl commonHandModelControl;

	public HandModelControl alternateControllerHandModelControl;

	private HandControl leftHandControl;

	private HandControl rightHandControl;

	public Transform handsContainer;

	public OVRHandInput ovrHandInputLeft;

	public OVRHandInput ovrHandInputRight;

	public SteamVRHandInput steamVRHandInputLeft;

	public SteamVRHandInput steamVRHandInputRight;

	public Toggle alwaysUseAlternateHandsToggle;

	[SerializeField]
	protected bool _alwaysUseAlternateHands;

	public Transform mouseGrab;

	public Camera leftControllerCamera;

	public Camera rightControllerCamera;

	private FreeControllerV3 rightGrabbedController;

	private bool rightGrabbedControllerIsRemote;

	private FreeControllerV3 leftGrabbedController;

	private bool leftGrabbedControllerIsRemote;

	private FreeControllerV3 rightFullGrabbedController;

	private bool rightFullGrabbedControllerIsRemote;

	private FreeControllerV3 leftFullGrabbedController;

	private bool leftFullGrabbedControllerIsRemote;

	public Transform worldScaleTransform;

	public Slider worldScaleSlider;

	public Slider worldScaleSliderAlt;

	private float _worldScale = 1f;

	public Slider controllerScaleSlider;

	private float _controllerScale = 1f;

	public Toggle useLegacyWorldScaleChangeToggle;

	[SerializeField]
	protected bool _useLegacyWorldScaleChange;

	public LayerMask targetColliderMask;

	public Transform selectPrefab;

	public Mesh selectPrefabStandardMesh;

	public float selectPrefabStandardScale = 0.5f;

	public LayerMask selectColliderMask;

	private FreeControllerV3 selectedController;

	public FCPositionHandle selectedControllerPositionHandle;

	public FCRotationHandle selectedControllerRotationHandle;

	public Text selectedControllerNameDisplay;

	private SelectMode selectMode;

	public Transform selectionHUDTransform;

	private SelectionHUD selectionHUD;

	public SelectionHUD mouseSelectionHUD;

	private List<FreeControllerV3> highlightedControllersLook;

	private List<SelectTarget> highlightedSelectTargetsLook;

	private List<FreeControllerV3> highlightedControllersMouse;

	private List<SelectTarget> highlightedSelectTargetsMouse;

	public Transform rightSelectionHUDTransform;

	public Transform leftSelectionHUDTransform;

	private SelectionHUD rightSelectionHUD;

	private SelectionHUD leftSelectionHUD;

	private List<FreeControllerV3> highlightedControllersLeft;

	private List<FreeControllerV3> highlightedControllersRight;

	private List<SelectTarget> highlightedSelectTargetsLeft;

	private List<SelectTarget> highlightedSelectTargetsRight;

	private HashSet<FreeControllerV3> onlyShowControllers;

	protected bool cursorLockedLastFrame;

	private List<Transform> selectionInstances;

	private SelectControllerCallback selectControllerCallback;

	private SelectForceProducerCallback selectForceProducerCallback;

	private SelectForceReceiverCallback selectForceReceiverCallback;

	private SelectRigidbodyCallback selectRigidbodyCallback;

	private SelectAtomCallback selectAtomCallback;

	public SteamVR_Action_Boolean menuAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Menu");

	public SteamVR_Action_Boolean UIInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("UIInteract");

	public SteamVR_Action_Boolean targetShowAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TargetShow");

	public SteamVR_Action_Boolean cycleEngageAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("CycleEngage");

	public SteamVR_Action_Vector2 cycleUsingXAxisAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("CycleUsingXAxis");

	public SteamVR_Action_Vector2 cycleUsingYAxisAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("CycleUsingYAxis");

	public SteamVR_Action_Boolean selectAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Select");

	public SteamVR_Action_Vector2 pushPullAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("PushPullNode");

	public SteamVR_Action_Boolean teleportShowAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TeleportShow");

	public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");

	public SteamVR_Action_Boolean grabNavigateAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabNavigate");

	public SteamVR_Action_Vector2 freeMoveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("FreeMove");

	public SteamVR_Action_Vector2 freeModeMoveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("FreeModeMove");

	public SteamVR_Action_Boolean swapFreeMoveAxis = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SwapFreeMoveAxis");

	public SteamVR_Action_Boolean grabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Grab");

	public SteamVR_Action_Boolean holdGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("HoldGrab");

	public SteamVR_Action_Single grabValAction = SteamVR_Input.GetAction<SteamVR_Action_Single>("GrabVal");

	public SteamVR_Action_Boolean remoteGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RemoteGrab");

	public SteamVR_Action_Boolean remoteHoldGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RemoteHoldGrab");

	public SteamVR_Action_Boolean toggleHandAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ToggleHand");

	public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

	public bool useLookSelect;

	public Camera lookCamera;

	public string buttonToggleTargets = "ButtonBack";

	private bool targetsOnWithButton;

	public string buttonSelect = "ButtonA";

	public string buttonUnselect = "ButtonB";

	public string buttonToggleRotateMode = "ButtonY";

	public string buttonCycleSelection = "ButtonX";

	public JoystickControl.Axis navigationForwardAxis = JoystickControl.Axis.LeftStickY;

	public bool invertNavigationForwardAxis;

	public JoystickControl.Axis navigationSideAxis = JoystickControl.Axis.LeftStickX;

	public bool invertNavigationSideAxis;

	public JoystickControl.Axis navigationTurnAxis = JoystickControl.Axis.RightStickX;

	public bool invertNavigationTurnAxis;

	public JoystickControl.Axis navigationUpAxis = JoystickControl.Axis.RightStickY;

	public bool invertNavigationUpAxis;

	public bool invertAxis1 = true;

	public bool invertAxis2 = true;

	public bool invertAxis3 = true;

	private bool _swapAxis;

	private bool leftGUIInteract;

	private bool rightGUIInteract;

	private bool _setLeftSelect;

	private bool _setRightSelect;

	protected bool remoteHoldGrabDisabled;

	public LayerMask lookAtTriggerMask;

	private LookAtTrigger currentLookAtTrigger;

	private bool _pointerModeLeft;

	private bool _pointerModeRight;

	protected Dictionary<FreeControllerV3, bool> wasHitFC;

	protected List<FreeControllerV3> overlappingFcs;

	protected List<FreeControllerV3> alreadyDisplayed;

	protected Collider[] overlappingControls;

	protected bool _allowGrabPlusTriggerHandToggle = true;

	public Toggle allowGrabPlusTriggerHandToggleToggle;

	private float cycleClick = 0.25f;

	private bool _leftCycleOn;

	private float _leftCycleXPosition;

	private float _leftCycleYPosition;

	private int leftCycleX;

	private int leftCycleY;

	private bool _rightCycleOn;

	private float _rightCycleXPosition;

	private float _rightCycleYPosition;

	private int rightCycleX;

	private int rightCycleY;

	private bool isLeftOverlap;

	private bool isRightOverlap;

	public GameObject mouseCrosshair;

	public GameObject freeMouseMoveModeIndicator;

	public RectTransform mouseCrosshairPointer;

	private FreeControllerV3 potentialGrabbedControllerMouse;

	private FreeControllerV3 grabbedControllerMouse;

	private float grabbedControllerMouseDistance;

	private Vector3 mouseDownLastWorldPosition;

	private bool dragActivated;

	private bool mouseClickUsed;

	private bool eligibleForMouseSelect;

	private Vector3 currentMousePosition;

	private Vector3 lastMousePosition;

	private Vector3 mouseChange;

	private Vector3 mouseChangeScaled;

	private float mouseAxisX;

	private float mouseAxisY;

	private bool useMouseRDPMode = true;

	public UIPopup UISidePopup;

	[SerializeField]
	protected UISideAlign.Side _UISide = UISideAlign.Side.Right;

	public Toggle onStartupSkipStartScreenToggle;

	protected bool _onStartupSkipStartScreen;

	public Transform mainHUDAttachPoint;

	public Transform mainHUDPivot;

	public float mainHUDPivotXRotationVR = -30f;

	public float mainHUDPivotXRotationMonitor;

	private Vector3 mainHUDAttachPointStartingPosition;

	private Quaternion mainHUDAttachPointStartingRotation;

	public bool showMainHUDOnStart;

	private bool _mainHUDVisible;

	private bool _mainHUDAnchoredOnMonitor;

	private float _monitorRigRightOffsetWhenUIOpen = -0.1f;

	private bool GUIhit;

	private bool GUIhitLeft;

	private bool GUIhitRight;

	private bool GUIhitMouse;

	public bool overrideCanvasSortingLayer;

	public string overrideCanvasSortingLayerName;

	protected Dictionary<SelectTarget, bool> wasHitST;

	protected RaycastHit[] raycastHits;

	private FreeControllerV3 rightPossessedController;

	private FreeControllerV3 rightStartPossessedController;

	private FreeControllerV3 leftPossessedController;

	private FreeControllerV3 leftStartPossessedController;

	private FreeControllerV3 headPossessedController;

	private FreeControllerV3 headStartPossessedController;

	private FreeControllerV3 tracker1PossessedController;

	private FreeControllerV3 tracker1StartPossessedController;

	private FreeControllerV3 tracker2PossessedController;

	private FreeControllerV3 tracker2StartPossessedController;

	private FreeControllerV3 tracker3PossessedController;

	private FreeControllerV3 tracker3StartPossessedController;

	private FreeControllerV3 tracker4PossessedController;

	private FreeControllerV3 tracker4StartPossessedController;

	private FreeControllerV3 tracker5PossessedController;

	private FreeControllerV3 tracker5StartPossessedController;

	private FreeControllerV3 tracker6PossessedController;

	private FreeControllerV3 tracker6StartPossessedController;

	private FreeControllerV3 tracker7PossessedController;

	private FreeControllerV3 tracker7StartPossessedController;

	private FreeControllerV3 tracker8PossessedController;

	private FreeControllerV3 tracker8StartPossessedController;

	private HandControl leftPossessHandControl;

	private HandControl leapLeftPossessHandControl;

	private HandControl rightPossessHandControl;

	private HandControl leapRightPossessHandControl;

	public Transform headPossessedActivateTransform;

	public Text headPossessedText;

	public Toggle allowHeadPossessMousePanAndZoomToggle;

	private bool _allowHeadPossessMousePanAndZoom;

	public Toggle allowPossessSpringAdjustmentToggle;

	[SerializeField]
	private bool _allowPossessSpringAdjustment = true;

	public Slider possessPositionSpringSlider;

	[SerializeField]
	private float _possessPositionSpring = 10000f;

	public Slider possessRotationSpringSlider;

	[SerializeField]
	private float _possessRotationSpring = 1000f;

	public MotionAnimationMaster motionAnimationMaster;

	protected MotionAnimationMaster currentAnimationMaster;

	public bool assetManagerSimulateInEditor = true;

	private bool _assetManagerReady;

	protected Dictionary<string, GameObject> assetBundleAssetNameToPrefab;

	protected Dictionary<string, int> assetBundleAssetNameRefCounts;

	private Dictionary<string, bool> uids;

	private Dictionary<string, Atom> atoms;

	private List<Atom> atomsList;

	private HashSet<Atom> startingAtoms;

	private bool _pauseSyncAtomLists;

	private List<string> atomUIDs;

	private List<string> atomUIDsWithForceReceivers;

	private List<string> atomUIDsWithForceProducers;

	private List<string> atomUIDsWithRhythmControllers;

	private List<string> atomUIDsWithAudioSourceControls;

	private List<string> atomUIDsWithFreeControllers;

	private List<string> atomUIDsWithRigidbodies;

	private List<string> sortedAtomUIDs;

	private List<string> sortedAtomUIDsWithForceReceivers;

	private List<string> sortedAtomUIDsWithForceProducers;

	private List<string> sortedAtomUIDsWithRhythmControllers;

	private List<string> sortedAtomUIDsWithAudioSourceControls;

	private List<string> sortedAtomUIDsWithFreeControllers;

	private List<string> sortedAtomUIDsWithRigidbodies;

	private List<string> hiddenAtomUIDs;

	private List<string> hiddenAtomUIDsWithFreeControllers;

	private List<string> visibleAtomUIDs;

	private List<string> visibleAtomUIDsWithFreeControllers;

	public RectTransform isolatedSceneEditControlPanel;

	public Text isolatedSubSceneLabel;

	protected SubScene _isolatedSubScene;

	protected HashSet<Atom> atomsInSubSceneHash;

	protected HashSet<Atom> nestedAtomsInSubSceneHash;

	public Toggle disableRenderForAtomsNotInIsolatedSubSceneToggle;

	protected bool _disableRenderForAtomsNotInIsolatedSubScene;

	public Toggle freezePhysicsForAtomsNotInIsolatedSubSceneToggle;

	protected bool _freezePhysicsForAtomsNotInIsolatedSubScene;

	public Toggle disableCollisionForAtomsNotInIsolatedSubSceneToggle;

	protected bool _disableCollisionForAtomsNotInIsolatedSubScene;

	public Button endIsolateEditSubSceneButton;

	public Button quickSaveIsolatedSubSceneButton;

	public Button quickReloadIsolatedSubSceneButton;

	public Button selectIsolatedSubSceneButton;

	public RectTransform isolatedAtomEditControlPanel;

	public Text isolatedAtomLabel;

	protected Atom _isolatedAtom;

	public Toggle disableRenderForAtomsNotInIsolatedAtomToggle;

	protected bool _disableRenderForAtomsNotInIsolatedAtom;

	public Toggle freezePhysicsForAtomsNotInIsolatedAtomToggle;

	protected bool _freezePhysicsForAtomsNotInIsolatedAtom;

	public Toggle disableCollisionForAtomsNotInIsolatedAtomToggle;

	protected bool _disableCollisionForAtomsNotInIsolatedAtom;

	public Button endIsolateEditAtomButton;

	public Button selectIsolatedAtomButton;

	public bool sortAtomUIDs = true;

	public Toggle showHiddenAtomsToggle;

	public Toggle showHiddenAtomsToggleAlt;

	protected bool _showHiddenAtoms;

	private List<FreeControllerV3> allControllers;

	private List<AnimationPattern> allAnimationPatterns;

	private List<AnimationStep> allAnimationSteps;

	private List<Animator> allAnimators;

	private List<Canvas> allCanvases;

	private Dictionary<string, ForceReceiver> frMap;

	private Dictionary<string, ForceProducerV2> fpMap;

	private Dictionary<string, FreeControllerV3> fcMap;

	private Dictionary<string, RhythmController> rcMap;

	private Dictionary<string, AudioSourceControl> ascMap;

	private Dictionary<string, Rigidbody> rbMap;

	private Dictionary<string, GrabPoint> gpMap;

	private Dictionary<string, MotionAnimationControl> macMap;

	private Dictionary<string, PlayerNavCollider> pncMap;

	private int maxUID = 1000;

	public OnForceReceiverNamesChanged onForceReceiverNamesChangedHandlers;

	private string[] _forceReceiverNames;

	public OnForceProducerNamesChanged onForceProducerNamesChangedHandlers;

	private string[] _forceProducerNames;

	public OnRhythmControllerNamesChanged onRhythmControllerNamesChangedHandlers;

	private string[] _rhythmControllerNames;

	public OnAudioSourceControlNamesChanged onAudioSourceControlNamesChangedHandlers;

	private string[] _audioSourceControlNames;

	public OnFreeControllerNamesChanged onFreeControllerNamesChangedHandlers;

	private string[] _freeControllerNames;

	public OnRigidbodyNamesChanged onRigidbodyNamesChangedHandlers;

	private string[] _rigidbodyNames;

	public string atomAssetsFile;

	public AtomAsset[] atomAssets;

	public AtomAsset[] indirectAtomAssets;

	public Atom[] atomPrefabs;

	public Atom[] indirectAtomPrefabs;

	protected Dictionary<string, Atom> atomPrefabByType;

	protected Dictionary<string, AtomAsset> atomAssetByType;

	protected List<string> atomTypes;

	protected List<string> atomCategories;

	protected Dictionary<string, List<string>> atomCategoryToAtomTypes;

	public string atomCategory;

	public UIPopup atomCategoryPopup;

	public UIPopup atomPrefabPopup;

	public OnAtomUIDsChanged onAtomUIDsChangedHandlers;

	public OnAtomUIDsWithForceReceiversChanged onAtomUIDsWithForceReceiversChangedHandlers;

	public OnAtomUIDsWithForceProducersChanged onAtomUIDsWithForceProducersChangedHandlers;

	public OnAtomUIDsWithFreeControllersChanged onAtomUIDsWithFreeControllersChangedHandlers;

	public OnAtomUIDsWithRigidbodiesChanged onAtomUIDsWithRigidbodiesChangedHandlers;

	public OnAtomUIDRename onAtomUIDRenameHandlers;

	public OnAtomParentChanged onAtomParentChangedHandlers;

	public OnAtomSubSceneChanged onAtomSubSceneChangedHandlers;

	public OnAtomAdded onAtomAddedHandlers;

	public OnAtomRemoved onAtomRemovedHandlers;

	public OnGameModeChanged onGameModeChangedHandlers;

	public OnSceneLoaded onSceneLoadedHandlers;

	public OnSubSceneLoaded onSubSceneLoadedHandlers;

	public OnSceneLoaded onBeforeSceneSaveHandlers;

	public OnSceneLoaded onSceneSavedHandlers;

	protected Atom lastAddedAtom;

	public Transform navigationPlayArea;

	public Transform regularPlayArea;

	public Transform navigationRig;

	public Transform navigationRigParent;

	public Transform navigationPlayer;

	public Transform navigationCamera;

	public Transform navigationHologrid;

	protected bool navigationHologridVisible;

	protected float navigationHologridShowTime;

	protected float navigationHologridTransparencyMultiplier = 1f;

	public Toggle showNavigationHologridToggle;

	[SerializeField]
	private bool _showNavigationHologrid = true;

	public Slider hologridTransparencySlider;

	[SerializeField]
	private float _hologridTransparency = 0.01f;

	protected Vector3 sceneLoadMonitorCameraLocalRotation;

	protected Vector3 sceneLoadPosition;

	protected Quaternion sceneLoadRotation;

	protected float sceneLoadPlayerHeightAdjust;

	public Toggle useSceneLoadPositionToggle;

	protected bool _useSceneLoadPosition;

	public CubicBezierCurve navigationCurve;

	public float navigationDistance = 100f;

	public bool useLookForNavigation = true;

	public LayerMask navigationColliderMask;

	public Toggle lockHeightDuringNavigateToggle;

	public Toggle lockHeightDuringNavigateToggleAlt;

	[SerializeField]
	private bool _lockHeightDuringNavigate = true;

	public Toggle disableAllNavigationToggle;

	[SerializeField]
	private bool _disableAllNavigation;

	public Toggle freeMoveFollowFloorToggle;

	public Toggle freeMoveFollowFloorToggleAlt;

	[SerializeField]
	private bool _freeMoveFollowFloor = true;

	public Toggle teleportAllowRotationToggle;

	[SerializeField]
	private bool _teleportAllowRotation;

	public Toggle disableTeleportToggle;

	[SerializeField]
	private bool _disableTeleport;

	public Toggle disableTeleportDuringPossessToggle;

	[SerializeField]
	private bool _disableTeleportDuringPossess = true;

	public Slider freeMoveMultiplierSlider;

	[SerializeField]
	private float _freeMoveMultiplier = 1f;

	public Toggle disableGrabNavigationToggle;

	[SerializeField]
	private bool _disableGrabNavigation;

	public Slider grabNavigationPositionMultiplierSlider;

	[SerializeField]
	private float _grabNavigationPositionMultiplier = 1f;

	public Slider grabNavigationRotationMultiplierSlider;

	[SerializeField]
	private float _grabNavigationRotationMultiplier = 0.5f;

	private float _grabNavigationRotationResistance = 0.1f;

	private Vector3 startNavigatePosition;

	private Vector3 startGrabNavigatePositionRight;

	private Vector3 startGrabNavigatePositionLeft;

	private bool isGrabNavigatingRight;

	private bool isGrabNavigatingLeft;

	private Quaternion startNavigateRotation;

	private Quaternion startGrabNavigateRotationRight;

	private Quaternion startGrabNavigateRotationLeft;

	private bool startedTeleport;

	private PlayerNavCollider teleportPlayerNavCollider;

	private PlayerNavCollider playerNavCollider;

	private GameObject playerNavTrackerGO;

	public Transform heightAdjustTransform;

	public Slider playerHeightAdjustSlider;

	public Slider playerHeightAdjustSliderAlt;

	private float _playerHeightAdjust;

	private Ray castRay;

	private MeshRenderer regularPlayAreaMR;

	private MeshRenderer navigationPlayAreaMR;

	private MeshRenderer navigationPlayerMR;

	private MeshRenderer navigationCameraMR;

	private bool isTeleporting;

	private bool didStartLeftNavigate;

	private bool didStartRightNavigate;

	public float focusDistance = 1.5f;

	public bool disableInternalKeyBindings;

	public bool disableInternalNavigationKeyBindings;

	private int _solverIterations = 15;

	private bool _useInterpolation = true;

	public bool disableLeap;

	public Transform LeapRig;

	public Toggle leapMotionEnabledToggle;

	protected bool _leapMotionEnabled;

	public LeapXRServiceProvider[] LeapServiceProviders;

	public Transform leapHandLeft;

	public Transform leapHandRight;

	public Transform leapHandMountLeft;

	public Transform leapHandMountRight;

	public LeapHandModelControl leapHandModelControl;

	private FreeControllerV3 leapLeftPossessedController;

	private FreeControllerV3 leapLeftStartPossessedController;

	private FreeControllerV3 leapRightPossessedController;

	private FreeControllerV3 leapRightStartPossessedController;

	protected bool _leapHandLeftConnected;

	protected bool _leapHandRightConnected;

	public Transform viveTracker1;

	public SteamVR_RenderModel viveTracker1Model;

	public Transform viveTracker2;

	public SteamVR_RenderModel viveTracker2Model;

	public Transform viveTracker3;

	public SteamVR_RenderModel viveTracker3Model;

	public Transform viveTracker4;

	public SteamVR_RenderModel viveTracker4Model;

	public Transform viveTracker5;

	public SteamVR_RenderModel viveTracker5Model;

	public Transform viveTracker6;

	public SteamVR_RenderModel viveTracker6Model;

	public Transform viveTracker7;

	public SteamVR_RenderModel viveTracker7Model;

	public Transform viveTracker8;

	public SteamVR_RenderModel viveTracker8Model;

	protected bool _hideTrackers;

	protected bool _tracker1Visible = true;

	protected bool _tracker2Visible = true;

	protected bool _tracker3Visible = true;

	protected bool _tracker4Visible = true;

	protected bool _tracker5Visible = true;

	protected bool _tracker6Visible = true;

	protected bool _tracker7Visible = true;

	protected bool _tracker8Visible = true;

	public bool isOVR;

	public bool isOpenVR;

	public CameraTarget centerCameraTarget;

	public GameObject[] centerCameraTargetDisableWhenMonitor;

	public Toggle generateDepthTextureToggle;

	protected bool _generateDepthTexture;

	public Toggle useMonitorRigAudioListenerWhenActiveToggle;

	protected bool _useMonitorRigAudioListenerWhenActive = true;

	protected AudioListener monitorRigAudioListener;

	protected AudioListener ovrRigAudioListener;

	protected AudioListener openVRRigAudioListener;

	protected LinkedList<AudioListener> additionalAudioListeners;

	protected AudioListener overrideAudioListener;

	protected AudioListener currentAudioListener;

	private bool MonitorRigActive;

	private bool isMonitorOnly;

	public Transform MonitorRig;

	public Camera MonitorCenterCamera;

	public Vector3 MonitorCenterCameraOffset = Vector3.zero;

	public Transform MonitorUI;

	public Transform MonitorUIAnchor;

	public Transform MonitorUIAttachPoint;

	public Transform MonitorModeAuxUI;

	public Button MonitorModeButton;

	protected bool _toggleMonitorSaveMainHUDVisible;

	protected bool _monitorUIVisible = true;

	public Slider monitorUIScaleSlider;

	public float fixedMonitorUIScale = 1f;

	private float _monitorUIScale = 1f;

	public Slider monitorUIYOffsetSlider;

	private float _monitorUIYOffset;

	public Slider monitorCameraFOVSlider;

	[SerializeField]
	private float _monitorCameraFOV = 40f;

	private Transform saveCenterEyeAttachPoint;

	public Transform OVRRig;

	public Camera OVRCenterCamera;

	public Transform touchObjectLeft;

	protected MeshRenderer[] touchObjectLeftMeshRenderers;

	public Transform touchHandMountLeft;

	public Transform touchCenterHandLeft;

	public Transform touchObjectRight;

	protected MeshRenderer[] touchObjectRightMeshRenderers;

	public Transform touchHandMountRight;

	public Transform touchCenterHandRight;

	public UIPopup oculusThumbstickFunctionPopup;

	[SerializeField]
	protected ThumbstickFunction _oculusThumbstickFunction;

	public Transform ViveRig;

	public Camera ViveCenterCamera;

	public Transform viveObjectLeft;

	public Transform viveHandMountLeft;

	public Transform viveCenterHandLeft;

	public Transform viveObjectRight;

	public Transform viveHandMountRight;

	public Transform viveCenterHandRight;

	protected MD5CryptoServiceProvider md5;

	[SerializeField]
	protected Transform bootstrapPluginContainer;

	protected ScriptDomain bootstrapPluginDomain;

	protected Dictionary<string, List<MVRScriptController>> bootstrapPluginScriptControllers;

	public RectTransform vamXPanel;

	public Button vamXButton;

	public Text vamXButtonText;

	public Button vamXTutorialButton;

	protected int vamXVersion;

	public GameObject[] vamXEnabledGameObjects;

	public GameObject[] vamXEnabledAndAdvancedSceneEditGameObjects;

	public GameObject[] vamXDisabledGameObjects;

	public GameObject[] vamXDisabledAndAdvancedSceneEditGameObjects;

	public GameObject[] vamXDisabledAndAdvancedSceneEditDisabledGameObjects;

	protected bool _vamXWasInstalled;

	protected bool _vamXInstalled;

	protected string lastLoadedvamXBootstrapPath;

	protected string vamXBootstrapPluginPath = "vamX.1.latest:/vamXBootstrap.cs";

	protected string vamXTutorialScene = "vamX.1.latest:/Saves/scene/vamX Tutorials.json";

	protected string vamXMainScene = "vamX.1.latest:/Saves/scene/vamX.json";

	public static SuperController singleton => _singleton;

	protected bool advancedSceneEditDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableAdvancedSceneEdit;
			}
			return disableAdvancedSceneEdit;
		}
	}

	protected bool saveSceneButtonDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableSaveSceneButton;
			}
			return disableSaveSceneButton;
		}
	}

	protected bool loadSceneButtonDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableLoadSceneButton;
			}
			return disableLoadSceneButton;
		}
	}

	protected bool customUIDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableCustomUI;
			}
			return disableCustomUI;
		}
	}

	protected bool browseDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableBrowse;
			}
			return disableBrowse;
		}
	}

	protected bool packagesDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disablePackages;
			}
			return disablePackages;
		}
	}

	public bool promotionalDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disablePromotional;
			}
			return disablePromotional;
		}
	}

	public bool keyInformationDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableKeyInformation;
			}
			return disableKeyInformation;
		}
	}

	public bool hubDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableHub;
			}
			return disableHub;
		}
	}

	public bool termsOfUseDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableTermsOfUse;
			}
			return disableTermsOfUse;
		}
	}

	public string freeKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.freeKey;
			}
			return _freeKey;
		}
	}

	public string teaserKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.teaserKey;
			}
			return _teaserKey;
		}
	}

	public string entertainerKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.entertainerKey;
			}
			return _entertainerKey;
		}
	}

	public string creatorKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.creatorKey;
			}
			return _creatorKey;
		}
	}

	public string steamKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.steamKey;
			}
			return _steamKey;
		}
	}

	public string nsteamKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.nsteamKey;
			}
			return _nsteamKey;
		}
	}

	public string retailKey
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.retailKey;
			}
			return _retailKey;
		}
	}

	public string keyFilePath
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.keyFilePath;
			}
			return _keyFilePath;
		}
	}

	public string legacySteamKeyFilePath
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.legacySteamKeyFilePath;
			}
			return _legacySteamKeyFilePath;
		}
	}

	public string currentSaveDir
	{
		get
		{
			return FileManager.CurrentSaveDir;
		}
		set
		{
			FileManager.SetSaveDir(value);
		}
	}

	public string currentLoadDir
	{
		get
		{
			return FileManager.CurrentLoadDir;
		}
		set
		{
			FileManager.SetLoadDir(value, true);
		}
	}

	public string LoadedSceneName => loadedName;

	public bool isLoading => _isLoading;

	public string savesDirResolved
	{
		get
		{
			if (Application.isEditor)
			{
				return savesDirEditor;
			}
			return savesDir;
		}
	}

	public bool HubOpen
	{
		get
		{
			if (hubBrowser != null)
			{
				return hubBrowser.IsShowing && worldUIActivated;
			}
			return false;
		}
	}

	protected bool startSceneEnabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.enableStartScene;
			}
			return enableStartScene;
		}
	}

	protected JSONEmbed embeddedJSONScene
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.startJSONEmbedScene;
			}
			return startJSONEmbedScene;
		}
	}

	public bool UIDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableUI;
			}
			return disableUI;
		}
	}

	protected bool pointersAlwaysEnabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.alwaysEnablePointers;
			}
			return alwaysEnablePointers;
		}
	}

	public bool navigationDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableNavigation;
			}
			return disableNavigation;
		}
	}

	protected bool VRDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableVR;
			}
			return disableVR;
		}
	}

	protected Transform atomContainerTransform
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.atomContainer;
			}
			return atomContainer;
		}
	}

	public float loResScreenShotCameraFOV
	{
		get
		{
			return _loResScreenShotCameraFOV;
		}
		set
		{
			if (_loResScreenShotCameraFOV != value)
			{
				_loResScreenShotCameraFOV = value;
				if (screenshotCamera != null)
				{
					screenshotCamera.fieldOfView = _loResScreenShotCameraFOV;
				}
				if (loResScreenShotCameraFOVSlider != null)
				{
					loResScreenShotCameraFOVSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float hiResScreenShotCameraFOV
	{
		get
		{
			return _hiResScreenShotCameraFOV;
		}
		set
		{
			if (_hiResScreenShotCameraFOV != value)
			{
				_hiResScreenShotCameraFOV = value;
				if (hiResScreenshotCamera != null)
				{
					hiResScreenshotCamera.fieldOfView = _hiResScreenShotCameraFOV;
				}
				if (hiResScreenShotCameraFOVSlider != null)
				{
					hiResScreenShotCameraFOVSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public GameMode gameMode
	{
		get
		{
			return _gameMode;
		}
		set
		{
			if (_gameMode != value)
			{
				_gameMode = value;
				SyncGameMode();
				if (_autoFreezeAnimationOnSwitchToEditMode)
				{
					SetFreezeAnimation(_gameMode == GameMode.Edit);
				}
				if (onGameModeChangedHandlers != null)
				{
					onGameModeChangedHandlers(_gameMode);
				}
			}
		}
	}

	public int errorCount
	{
		get
		{
			return _errorCount;
		}
		set
		{
			if (_errorCount != value)
			{
				_errorCount = value;
				if (allErrorsCountText != null)
				{
					allErrorsCountText.text = _errorCount.ToString();
				}
				if (allErrorsCountText2 != null)
				{
					allErrorsCountText2.text = _errorCount.ToString();
				}
			}
		}
	}

	public bool openMainHUDOnError
	{
		get
		{
			return _openMainHUDOnError;
		}
		set
		{
			if (_openMainHUDOnError != value)
			{
				_openMainHUDOnError = value;
				if (openMainHUDOnErrorToggle != null)
				{
					openMainHUDOnErrorToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public int msgCount
	{
		get
		{
			return _msgCount;
		}
		set
		{
			if (_msgCount != value)
			{
				_msgCount = value;
				if (allMessagesCountText != null)
				{
					allMessagesCountText.text = _msgCount.ToString();
				}
				if (allMessagesCountText2 != null)
				{
					allMessagesCountText2.text = _msgCount.ToString();
				}
			}
		}
	}

	protected bool resetSimulation
	{
		get
		{
			return _resetSimulation;
		}
		set
		{
			if (_resetSimulation != value)
			{
				_resetSimulation = value;
			}
		}
	}

	public bool autoSimulation
	{
		get
		{
			return _autoSimulation;
		}
		protected set
		{
			if (_autoSimulation == value)
			{
				return;
			}
			_autoSimulation = value;
			Physics.autoSimulation = value;
			SyncActiveHands();
			if (!_autoSimulation)
			{
				return;
			}
			foreach (Atom atoms in atomsList)
			{
				atoms.ResetRigidbodies();
			}
		}
	}

	public bool pauseAutoSimulation
	{
		get
		{
			return _pauseAutoSimulation;
		}
		set
		{
			if (_pauseAutoSimulation != value)
			{
				if (pauseAutoSimulationFlag != null)
				{
					pauseAutoSimulationFlag.Raise();
				}
				_pauseAutoSimulation = value;
				if (pauseAutoSimulationToggle != null)
				{
					pauseAutoSimulationToggle.isOn = _pauseAutoSimulation;
				}
				if (_pauseAutoSimulation)
				{
					pauseAutoSimulationFlag = new AsyncFlag("Global Pause Auto Simulation");
					PauseAutoSimulation(pauseAutoSimulationFlag);
				}
			}
		}
	}

	public bool render
	{
		get
		{
			return _render;
		}
		protected set
		{
			if (_render == value)
			{
				return;
			}
			_render = value;
			if (atoms == null)
			{
				return;
			}
			foreach (Atom atoms in atomsList)
			{
				atoms.globalDisableRender = !_render;
			}
		}
	}

	public bool pauseRender
	{
		get
		{
			return _pauseRender;
		}
		set
		{
			if (_pauseRender != value)
			{
				if (pauseRenderFlag != null)
				{
					pauseRenderFlag.Raise();
				}
				_pauseRender = value;
				if (pauseRenderToggle != null)
				{
					pauseRenderToggle.isOn = _pauseRender;
				}
				if (_pauseRender)
				{
					pauseRenderFlag = new AsyncFlag("Global Pause Render");
					PauseRender(pauseRenderFlag);
				}
			}
		}
	}

	public bool autoFreezeAnimationOnSwitchToEditMode
	{
		get
		{
			return _autoFreezeAnimationOnSwitchToEditMode;
		}
		set
		{
			if (_autoFreezeAnimationOnSwitchToEditMode != value)
			{
				_autoFreezeAnimationOnSwitchToEditMode = value;
				if (autoFreezeAnimationOnSwitchToEditModeToggle != null)
				{
					autoFreezeAnimationOnSwitchToEditModeToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool freezeAnimation => _freezeAnimation || _isLoading || _resetSimulation || !_autoSimulation;

	public AlignRotationOffset alignRotationOffset
	{
		get
		{
			return _alignRotationOffset;
		}
		set
		{
			if (_alignRotationOffset != value)
			{
				_alignRotationOffset = value;
				if (alignRotationOffsetPopup != null)
				{
					alignRotationOffsetPopup.currentValue = _alignRotationOffset.ToString();
				}
			}
		}
	}

	public bool worldUIShowOverlaySky
	{
		get
		{
			return _worldUIShowOverlaySky;
		}
		set
		{
			if (_worldUIShowOverlaySky != value)
			{
				_worldUIShowOverlaySky = value;
				if (worldUIShowOverlaySkyToggle != null)
				{
					worldUIShowOverlaySkyToggle.isOn = _worldUIShowOverlaySky;
				}
				SyncWorldUIOverlaySky();
			}
		}
	}

	public bool worldUIActivated { get; private set; }

	public float worldUIVRAnchorDistance
	{
		get
		{
			return _worldUIVRAnchorDistance;
		}
		set
		{
			if (_worldUIVRAnchorDistance != value)
			{
				_worldUIVRAnchorDistance = value;
				if (worldUIVRAnchorDistanceSlider != null)
				{
					worldUIVRAnchorDistanceSlider.value = _worldUIVRAnchorDistance;
				}
				SyncWorldUIAnchor();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float worldUIVRAnchorHeight
	{
		get
		{
			return _worldUIVRAnchorHeight;
		}
		set
		{
			if (_worldUIVRAnchorHeight != value)
			{
				_worldUIVRAnchorHeight = value;
				if (worldUIVRAnchorHeightSlider != null)
				{
					worldUIVRAnchorHeightSlider.value = _worldUIVRAnchorHeight;
				}
				SyncWorldUIAnchor();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public ActiveUI lastActiveUI => _lastActiveUI;

	public ActiveUI activeUI
	{
		get
		{
			return _activeUI;
		}
		set
		{
			if (_activeUI != value)
			{
				_lastActiveUI = _activeUI;
				_activeUI = value;
			}
			ClearAllUI();
			SyncActiveUI();
		}
	}

	public bool helpOverlayOn
	{
		get
		{
			return _helpOverlayOn;
		}
		set
		{
			if (_helpOverlayOn != value)
			{
				_helpOverlayOn = value;
				SyncHelpOverlay();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public string helpText
	{
		get
		{
			return _helpText;
		}
		set
		{
			if (_helpText != value)
			{
				_helpText = value;
				SyncHelpText();
			}
		}
	}

	public Color helpColor
	{
		get
		{
			return _helpColor;
		}
		set
		{
			if (_helpColor != value)
			{
				_helpColor = value;
				SyncHelpText();
			}
		}
	}

	public bool alwaysUseAlternateHands
	{
		get
		{
			return _alwaysUseAlternateHands;
		}
		set
		{
			if (_alwaysUseAlternateHands != value)
			{
				_alwaysUseAlternateHands = value;
				if (alwaysUseAlternateHandsToggle != null)
				{
					alwaysUseAlternateHandsToggle.isOn = _alwaysUseAlternateHands;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				SyncActiveHands();
			}
		}
	}

	public FreeControllerV3 RightGrabbedController => rightGrabbedController;

	public FreeControllerV3 LeftGrabbedController => leftGrabbedController;

	public FreeControllerV3 RightFullGrabbedController => rightFullGrabbedController;

	public FreeControllerV3 LeftFullGrabbedController => leftFullGrabbedController;

	public float worldScale
	{
		get
		{
			return _worldScale;
		}
		set
		{
			if (_worldScale == value)
			{
				return;
			}
			_worldScale = value;
			if (worldScaleSlider != null)
			{
				worldScaleSlider.value = _worldScale;
			}
			if (worldScaleSliderAlt != null)
			{
				worldScaleSliderAlt.value = _worldScale;
			}
			SplashNavigationHologrid(1f);
			if (rayLineLeft != null)
			{
				rayLineLeft.startWidth = rayLineWidth * _worldScale;
				rayLineLeft.endWidth = rayLineLeft.startWidth;
			}
			if (rayLineRight != null)
			{
				rayLineRight.startWidth = rayLineWidth * _worldScale;
				rayLineRight.endWidth = rayLineRight.startWidth;
			}
			Vector3 vector = Vector3.zero;
			if (centerCameraTarget != null)
			{
				vector = centerCameraTarget.transform.position;
			}
			Vector3 localScale = new Vector3(_worldScale, _worldScale, _worldScale);
			if ((bool)worldScaleTransform)
			{
				worldScaleTransform.localScale = localScale;
			}
			else
			{
				base.transform.localScale = localScale;
			}
			if (centerCameraTarget != null && navigationRig != null)
			{
				if (_useLegacyWorldScaleChange)
				{
					Vector3 position = centerCameraTarget.transform.position;
					Vector3 vector2 = vector - position;
					Vector3 vector3 = navigationRig.position + vector2;
					Vector3 up = navigationRig.up;
					float num = Vector3.Dot(vector3 - navigationRig.position, up);
					vector3 += up * (0f - num);
					navigationRig.position = vector3;
				}
				else
				{
					playerHeightAdjust = 0f;
					Vector3 position2 = centerCameraTarget.transform.position;
					Vector3 vector4 = vector - position2;
					Vector3 up2 = navigationRig.up;
					float num2 = Vector3.Dot(vector4, up2);
					vector4 += up2 * (0f - num2);
					navigationRig.position += vector4;
					playerHeightAdjust = num2;
				}
			}
			if (LookInputModule.singleton != null)
			{
				LookInputModule.singleton.worldScale = _worldScale;
			}
			ScaleChangeReceiver[] componentsInChildren = GetComponentsInChildren<ScaleChangeReceiver>(true);
			ScaleChangeReceiver[] array = componentsInChildren;
			foreach (ScaleChangeReceiver scaleChangeReceiver in array)
			{
				scaleChangeReceiver.ScaleChanged(_worldScale);
			}
			SyncPlayerHeightAdjust();
		}
	}

	public float controllerScale
	{
		get
		{
			return _controllerScale;
		}
		set
		{
			if (_controllerScale != value)
			{
				_controllerScale = value;
				if (controllerScaleSlider != null)
				{
					controllerScaleSlider.value = _controllerScale;
				}
			}
		}
	}

	public bool useLegacyWorldScaleChange
	{
		get
		{
			return _useLegacyWorldScaleChange;
		}
		set
		{
			if (_useLegacyWorldScaleChange != value)
			{
				_useLegacyWorldScaleChange = value;
				if (useLegacyWorldScaleChangeToggle != null)
				{
					useLegacyWorldScaleChangeToggle.isOn = _useLegacyWorldScaleChange;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public SelectMode currentSelectMode => selectMode;

	private Transform motionControllerLeft
	{
		get
		{
			if (isOVR)
			{
				return touchObjectLeft;
			}
			if (isOpenVR)
			{
				return viveObjectLeft;
			}
			return null;
		}
	}

	private Transform handMountLeft
	{
		get
		{
			if (isOVR)
			{
				return touchHandMountLeft;
			}
			if (isOpenVR)
			{
				return viveHandMountLeft;
			}
			return null;
		}
	}

	private Transform centerHandLeft
	{
		get
		{
			if (isOVR)
			{
				return touchCenterHandLeft;
			}
			if (isOpenVR)
			{
				return viveCenterHandLeft;
			}
			return null;
		}
	}

	private Transform motionControllerRight
	{
		get
		{
			if (isOVR)
			{
				return touchObjectRight;
			}
			if (isOpenVR)
			{
				return viveObjectRight;
			}
			return null;
		}
	}

	private Transform handMountRight
	{
		get
		{
			if (isOVR)
			{
				return touchHandMountRight;
			}
			if (isOpenVR)
			{
				return viveHandMountRight;
			}
			return null;
		}
	}

	private Transform centerHandRight
	{
		get
		{
			if (isOVR)
			{
				return touchCenterHandRight;
			}
			if (isOpenVR)
			{
				return viveCenterHandRight;
			}
			return null;
		}
	}

	private Transform motionControllerHead
	{
		get
		{
			if (centerCameraTarget != null)
			{
				return centerCameraTarget.transform;
			}
			return null;
		}
	}

	public bool allowGrabPlusTriggerHandToggle
	{
		get
		{
			return _allowGrabPlusTriggerHandToggle;
		}
		set
		{
			if (_allowGrabPlusTriggerHandToggle != value)
			{
				_allowGrabPlusTriggerHandToggle = value;
				if (allowGrabPlusTriggerHandToggleToggle != null)
				{
					allowGrabPlusTriggerHandToggleToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public UISideAlign.Side UISide
	{
		get
		{
			return _UISide;
		}
		set
		{
			if (_UISide != value)
			{
				_UISide = value;
				if (UISidePopup != null)
				{
					UISidePopup.currentValueNoCallback = _UISide.ToString();
				}
				SyncUISide();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool onStartupSkipStartScreen
	{
		get
		{
			return _onStartupSkipStartScreen;
		}
		set
		{
			if (_onStartupSkipStartScreen != value)
			{
				_onStartupSkipStartScreen = value;
				if (onStartupSkipStartScreenToggle != null)
				{
					onStartupSkipStartScreenToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool MainHUDVisible => _mainHUDVisible;

	public bool MainHUDAnchoredOnMonitor => _mainHUDAnchoredOnMonitor;

	public bool allowHeadPossessMousePanAndZoom
	{
		get
		{
			return _allowHeadPossessMousePanAndZoom;
		}
		set
		{
			if (_allowHeadPossessMousePanAndZoom != value)
			{
				_allowHeadPossessMousePanAndZoom = value;
				if (allowHeadPossessMousePanAndZoomToggle != null)
				{
					allowHeadPossessMousePanAndZoomToggle.isOn = _allowHeadPossessMousePanAndZoom;
				}
			}
		}
	}

	public bool allowPossessSpringAdjustment
	{
		get
		{
			return _allowPossessSpringAdjustment;
		}
		set
		{
			if (_allowPossessSpringAdjustment != value)
			{
				_allowPossessSpringAdjustment = value;
				if (allowPossessSpringAdjustmentToggle != null)
				{
					allowPossessSpringAdjustmentToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float possessPositionSpring
	{
		get
		{
			return _possessPositionSpring;
		}
		set
		{
			if (_possessPositionSpring != value)
			{
				_possessPositionSpring = value;
				if (possessPositionSpringSlider != null)
				{
					possessPositionSpringSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float possessRotationSpring
	{
		get
		{
			return _possessRotationSpring;
		}
		set
		{
			if (_possessRotationSpring != value)
			{
				_possessRotationSpring = value;
				if (possessRotationSpringSlider != null)
				{
					possessRotationSpringSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool assetManagerReady => _assetManagerReady;

	public SubScene isolatedSubScene
	{
		get
		{
			return _isolatedSubScene;
		}
		protected set
		{
			if (!(_isolatedSubScene != value))
			{
				return;
			}
			_isolatedSubScene = value;
			if (_isolatedSubScene != null)
			{
				if (quickReloadIsolatedSubSceneButton != null)
				{
					quickReloadIsolatedSubSceneButton.interactable = _isolatedSubScene.CheckExistance();
				}
				if (quickSaveIsolatedSubSceneButton != null)
				{
					quickSaveIsolatedSubSceneButton.interactable = _isolatedSubScene.CheckReadyForStore();
				}
			}
			if (_isolatedSubScene != null && isolatedSubSceneLabel != null)
			{
				isolatedSubSceneLabel.text = _isolatedSubScene.containingAtom.uidWithoutSubScenePath;
			}
			if (isolatedSceneEditControlPanel != null)
			{
				isolatedSceneEditControlPanel.gameObject.SetActive(_isolatedSubScene != null);
			}
			SyncIsolatedSubScene();
		}
	}

	public bool disableRenderForAtomsNotInIsolatedSubScene
	{
		get
		{
			return _disableRenderForAtomsNotInIsolatedSubScene;
		}
		set
		{
			if (_disableRenderForAtomsNotInIsolatedSubScene != value)
			{
				_disableRenderForAtomsNotInIsolatedSubScene = value;
				if (disableRenderForAtomsNotInIsolatedSubSceneToggle != null)
				{
					disableRenderForAtomsNotInIsolatedSubSceneToggle.isOn = value;
				}
				SyncIsolatedSubScene();
			}
		}
	}

	public bool freezePhysicsForAtomsNotInIsolatedSubScene
	{
		get
		{
			return _freezePhysicsForAtomsNotInIsolatedSubScene;
		}
		set
		{
			if (_freezePhysicsForAtomsNotInIsolatedSubScene != value)
			{
				_freezePhysicsForAtomsNotInIsolatedSubScene = value;
				if (freezePhysicsForAtomsNotInIsolatedSubSceneToggle != null)
				{
					freezePhysicsForAtomsNotInIsolatedSubSceneToggle.isOn = _freezePhysicsForAtomsNotInIsolatedSubScene;
				}
				if (!_freezePhysicsForAtomsNotInIsolatedSubScene && _disableCollisionForAtomsNotInIsolatedSubScene)
				{
					disableCollisionForAtomsNotInIsolatedSubScene = false;
				}
				else
				{
					SyncIsolatedSubScene();
				}
			}
		}
	}

	public bool disableCollisionForAtomsNotInIsolatedSubScene
	{
		get
		{
			return _disableCollisionForAtomsNotInIsolatedSubScene;
		}
		set
		{
			if (_disableCollisionForAtomsNotInIsolatedSubScene != value)
			{
				_disableCollisionForAtomsNotInIsolatedSubScene = value;
				if (disableCollisionForAtomsNotInIsolatedSubSceneToggle != null)
				{
					disableCollisionForAtomsNotInIsolatedSubSceneToggle.isOn = _disableCollisionForAtomsNotInIsolatedSubScene;
				}
				if (_disableCollisionForAtomsNotInIsolatedSubScene && !_freezePhysicsForAtomsNotInIsolatedSubScene)
				{
					freezePhysicsForAtomsNotInIsolatedSubScene = true;
				}
				else
				{
					SyncIsolatedSubScene();
				}
			}
		}
	}

	public Atom isolatedAtom
	{
		get
		{
			return _isolatedAtom;
		}
		protected set
		{
			if (_isolatedAtom != value)
			{
				_isolatedAtom = value;
				if (_isolatedAtom != null && isolatedAtomLabel != null)
				{
					isolatedAtomLabel.text = _isolatedAtom.uid;
				}
				if (isolatedAtomEditControlPanel != null)
				{
					isolatedAtomEditControlPanel.gameObject.SetActive(_isolatedAtom != null);
				}
				SyncIsolatedAtom();
			}
		}
	}

	public bool disableRenderForAtomsNotInIsolatedAtom
	{
		get
		{
			return _disableRenderForAtomsNotInIsolatedAtom;
		}
		set
		{
			if (_disableRenderForAtomsNotInIsolatedAtom != value)
			{
				_disableRenderForAtomsNotInIsolatedAtom = value;
				if (disableRenderForAtomsNotInIsolatedAtomToggle != null)
				{
					disableRenderForAtomsNotInIsolatedAtomToggle.isOn = value;
				}
				SyncIsolatedAtom();
			}
		}
	}

	public bool freezePhysicsForAtomsNotInIsolatedAtom
	{
		get
		{
			return _freezePhysicsForAtomsNotInIsolatedAtom;
		}
		set
		{
			if (_freezePhysicsForAtomsNotInIsolatedAtom != value)
			{
				_freezePhysicsForAtomsNotInIsolatedAtom = value;
				if (freezePhysicsForAtomsNotInIsolatedAtomToggle != null)
				{
					freezePhysicsForAtomsNotInIsolatedAtomToggle.isOn = _freezePhysicsForAtomsNotInIsolatedAtom;
				}
				if (!_freezePhysicsForAtomsNotInIsolatedAtom && _disableCollisionForAtomsNotInIsolatedAtom)
				{
					disableCollisionForAtomsNotInIsolatedAtom = false;
				}
				else
				{
					SyncIsolatedAtom();
				}
			}
		}
	}

	public bool disableCollisionForAtomsNotInIsolatedAtom
	{
		get
		{
			return _disableCollisionForAtomsNotInIsolatedAtom;
		}
		set
		{
			if (_disableCollisionForAtomsNotInIsolatedAtom != value)
			{
				_disableCollisionForAtomsNotInIsolatedAtom = value;
				if (disableCollisionForAtomsNotInIsolatedAtomToggle != null)
				{
					disableCollisionForAtomsNotInIsolatedAtomToggle.isOn = _disableCollisionForAtomsNotInIsolatedAtom;
				}
				if (_disableCollisionForAtomsNotInIsolatedAtom && !_freezePhysicsForAtomsNotInIsolatedAtom)
				{
					freezePhysicsForAtomsNotInIsolatedAtom = true;
				}
				else
				{
					SyncIsolatedAtom();
				}
			}
		}
	}

	public bool showHiddenAtoms
	{
		get
		{
			return _showHiddenAtoms;
		}
		set
		{
			if (_showHiddenAtoms != value)
			{
				_showHiddenAtoms = value;
				if (showHiddenAtomsToggle != null)
				{
					showHiddenAtomsToggle.isOn = value;
				}
				if (showHiddenAtomsToggleAlt != null)
				{
					showHiddenAtomsToggleAlt.isOn = value;
				}
				SyncSelectAtomPopup();
				SyncVisibility();
			}
		}
	}

	public string[] forceReceiverNames => _forceReceiverNames;

	public string[] forceProducerNames => _forceProducerNames;

	public string[] rhythmControllerNames => _rhythmControllerNames;

	public string[] audioSourceControlNames => _audioSourceControlNames;

	public string[] freeControllerNames => _freeControllerNames;

	public string[] rigidbodyNames => _rigidbodyNames;

	public bool showNavigationHologrid
	{
		get
		{
			return _showNavigationHologrid;
		}
		set
		{
			if (_showNavigationHologrid != value)
			{
				_showNavigationHologrid = value;
				if (showNavigationHologridToggle != null)
				{
					showNavigationHologridToggle.isOn = _showNavigationHologrid;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float hologridTransparency
	{
		get
		{
			return _hologridTransparency;
		}
		set
		{
			if (_hologridTransparency != value)
			{
				_hologridTransparency = value;
				SyncHologridTransparency();
				if (hologridTransparencySlider != null)
				{
					hologridTransparencySlider.value = _hologridTransparency;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool useSceneLoadPosition
	{
		get
		{
			return _useSceneLoadPosition;
		}
		set
		{
			if (_useSceneLoadPosition != value)
			{
				_useSceneLoadPosition = value;
				if (useSceneLoadPositionToggle != null)
				{
					useSceneLoadPositionToggle.isOn = _useSceneLoadPosition;
				}
			}
		}
	}

	public bool lockHeightDuringNavigate
	{
		get
		{
			return _lockHeightDuringNavigate;
		}
		set
		{
			if (_lockHeightDuringNavigate != value)
			{
				_lockHeightDuringNavigate = value;
				SyncLockHeightDuringNavigate();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableAllNavigation
	{
		get
		{
			return _disableAllNavigation;
		}
		set
		{
			if (_disableAllNavigation != value)
			{
				_disableAllNavigation = value;
				SyncDisableAllNavigation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool freeMoveFollowFloor
	{
		get
		{
			return _freeMoveFollowFloor;
		}
		set
		{
			if (_freeMoveFollowFloor != value)
			{
				_freeMoveFollowFloor = value;
				SyncFreeMoveFollowFloor();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool teleportAllowRotation
	{
		get
		{
			return _teleportAllowRotation;
		}
		set
		{
			if (_teleportAllowRotation != value)
			{
				_teleportAllowRotation = value;
				SyncTeleportAllowRotation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableTeleport
	{
		get
		{
			return _disableTeleport;
		}
		set
		{
			if (_disableTeleport != value)
			{
				_disableTeleport = value;
				SyncDisableTeleport();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableTeleportDuringPossess
	{
		get
		{
			return _disableTeleportDuringPossess;
		}
		set
		{
			if (_disableTeleportDuringPossess != value)
			{
				_disableTeleportDuringPossess = value;
				SyncDisableTeleportDuringPossess();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float freeMoveMultiplier
	{
		get
		{
			return _freeMoveMultiplier;
		}
		set
		{
			if (_freeMoveMultiplier != value)
			{
				_freeMoveMultiplier = value;
				SyncFreeMoveMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableGrabNavigation
	{
		get
		{
			return _disableGrabNavigation;
		}
		set
		{
			if (_disableGrabNavigation != value)
			{
				_disableGrabNavigation = value;
				SyncDisableGrabNavigation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float grabNavigationPositionMultiplier
	{
		get
		{
			return _grabNavigationPositionMultiplier;
		}
		set
		{
			if (_grabNavigationPositionMultiplier != value)
			{
				_grabNavigationPositionMultiplier = value;
				SyncGrabNavigationPositionMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float grabNavigationRotationMultiplier
	{
		get
		{
			return _grabNavigationRotationMultiplier;
		}
		set
		{
			if (_grabNavigationRotationMultiplier != value)
			{
				_grabNavigationRotationMultiplier = value;
				SyncGrabNavigationRotationMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float playerHeightAdjust
	{
		get
		{
			return _playerHeightAdjust;
		}
		set
		{
			if (_playerHeightAdjust == value)
			{
				return;
			}
			float adj = value - _playerHeightAdjust;
			HUDAnchor.AdjustAnchorHeights(adj);
			_playerHeightAdjust = value;
			SyncPlayerHeightAdjust();
			if (playerHeightAdjustSlider != null)
			{
				while (_playerHeightAdjust < playerHeightAdjustSlider.minValue)
				{
					playerHeightAdjustSlider.minValue *= 2f;
				}
				while (_playerHeightAdjust > playerHeightAdjustSlider.maxValue)
				{
					playerHeightAdjustSlider.maxValue *= 2f;
				}
				playerHeightAdjustSlider.value = _playerHeightAdjust;
			}
			if (playerHeightAdjustSliderAlt != null)
			{
				while (_playerHeightAdjust < playerHeightAdjustSliderAlt.minValue)
				{
					playerHeightAdjustSliderAlt.minValue *= 2f;
				}
				while (_playerHeightAdjust > playerHeightAdjustSliderAlt.maxValue)
				{
					playerHeightAdjustSliderAlt.maxValue *= 2f;
				}
				playerHeightAdjustSliderAlt.value = _playerHeightAdjust;
			}
		}
	}

	public int solverIterations
	{
		get
		{
			return _solverIterations;
		}
		set
		{
			if (_solverIterations == value)
			{
				return;
			}
			_solverIterations = value;
			foreach (Atom atoms in atomsList)
			{
				Rigidbody[] rigidbodies = atoms.rigidbodies;
				foreach (Rigidbody rigidbody in rigidbodies)
				{
					rigidbody.solverIterations = _solverIterations;
				}
				PhysicsSimulator[] physicsSimulators = atoms.physicsSimulators;
				foreach (PhysicsSimulator physicsSimulator in physicsSimulators)
				{
					physicsSimulator.solverIterations = _solverIterations;
				}
				PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable = atoms.physicsSimulatorsStorable;
				foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in physicsSimulatorsStorable)
				{
					physicsSimulatorJSONStorable.solverIterations = _solverIterations;
				}
			}
		}
	}

	private bool useInterpolation
	{
		get
		{
			return _useInterpolation;
		}
		set
		{
			if (_useInterpolation == value)
			{
				return;
			}
			_useInterpolation = value;
			foreach (Atom atoms in atomsList)
			{
				atoms.useRigidbodyInterpolation = _useInterpolation;
			}
		}
	}

	protected bool leapDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableLeap;
			}
			return disableLeap;
		}
	}

	public bool leapMotionEnabled
	{
		get
		{
			return _leapMotionEnabled;
		}
		set
		{
			if (_leapMotionEnabled != value)
			{
				_leapMotionEnabled = value;
				if (leapMotionEnabledToggle != null)
				{
					leapMotionEnabledToggle.isOn = value;
				}
				SyncLeapEnabled();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool hideTrackers
	{
		get
		{
			return _hideTrackers;
		}
		set
		{
			if (_hideTrackers != value)
			{
				_hideTrackers = value;
				SyncTrackerVisibility();
			}
		}
	}

	protected bool tracker1Visible
	{
		get
		{
			return _tracker1Visible;
		}
		set
		{
			if (_tracker1Visible != value)
			{
				_tracker1Visible = value;
				SyncTracker1Visibility();
			}
		}
	}

	protected bool tracker2Visible
	{
		get
		{
			return _tracker2Visible;
		}
		set
		{
			if (_tracker2Visible != value)
			{
				_tracker2Visible = value;
				SyncTracker2Visibility();
			}
		}
	}

	protected bool tracker3Visible
	{
		get
		{
			return _tracker3Visible;
		}
		set
		{
			if (_tracker3Visible != value)
			{
				_tracker3Visible = value;
				SyncTracker3Visibility();
			}
		}
	}

	protected bool tracker4Visible
	{
		get
		{
			return _tracker4Visible;
		}
		set
		{
			if (_tracker4Visible != value)
			{
				_tracker4Visible = value;
				SyncTracker4Visibility();
			}
		}
	}

	protected bool tracker5Visible
	{
		get
		{
			return _tracker5Visible;
		}
		set
		{
			if (_tracker5Visible != value)
			{
				_tracker5Visible = value;
				SyncTracker5Visibility();
			}
		}
	}

	protected bool tracker6Visible
	{
		get
		{
			return _tracker6Visible;
		}
		set
		{
			if (_tracker6Visible != value)
			{
				_tracker6Visible = value;
				SyncTracker6Visibility();
			}
		}
	}

	protected bool tracker7Visible
	{
		get
		{
			return _tracker7Visible;
		}
		set
		{
			if (_tracker7Visible != value)
			{
				_tracker7Visible = value;
				SyncTracker7Visibility();
			}
		}
	}

	protected bool tracker8Visible
	{
		get
		{
			return _tracker8Visible;
		}
		set
		{
			if (_tracker8Visible != value)
			{
				_tracker8Visible = value;
				SyncTracker8Visibility();
			}
		}
	}

	public bool generateDepthTexture
	{
		get
		{
			return _generateDepthTexture;
		}
		set
		{
			if (_generateDepthTexture != value)
			{
				_generateDepthTexture = value;
				if (generateDepthTextureToggle != null)
				{
					generateDepthTextureToggle.isOn = value;
				}
				SyncGenerateDepthTexture();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool useMonitorRigAudioListenerWhenActive
	{
		get
		{
			return _useMonitorRigAudioListenerWhenActive;
		}
		set
		{
			if (_useMonitorRigAudioListenerWhenActive != value)
			{
				_useMonitorRigAudioListenerWhenActive = value;
				if (useMonitorRigAudioListenerWhenActiveToggle != null)
				{
					useMonitorRigAudioListenerWhenActiveToggle.isOn = value;
				}
				SyncAudioListener();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public AudioListener CurrentAudioListener => currentAudioListener;

	public bool IsMonitorRigActive => MonitorRigActive;

	public bool IsMonitorOnly => isMonitorOnly;

	public float monitorUIScale
	{
		get
		{
			return _monitorUIScale;
		}
		set
		{
			if (_monitorUIScale != value)
			{
				_monitorUIScale = value;
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				if (monitorUIScaleSlider != null)
				{
					monitorUIScaleSlider.value = _monitorUIScale;
				}
			}
		}
	}

	public float monitorUIYOffset
	{
		get
		{
			return _monitorUIYOffset;
		}
		set
		{
			if (_monitorUIYOffset != value)
			{
				_monitorUIYOffset = value;
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				if (monitorUIYOffsetSlider != null)
				{
					monitorUIYOffsetSlider.value = _monitorUIYOffset;
				}
			}
		}
	}

	public float startingMonitorCameraFOV
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.startingMonitorCameraFOV;
			}
			return _monitorCameraFOV;
		}
	}

	public float monitorCameraFOV
	{
		get
		{
			if (worldUIActivated)
			{
				return 40f;
			}
			return _monitorCameraFOV;
		}
		set
		{
			if (_monitorCameraFOV != value)
			{
				_monitorCameraFOV = value;
				if (monitorCameraFOVSlider != null)
				{
					monitorCameraFOVSlider.value = _monitorCameraFOV;
				}
				SyncMonitorCameraFOV();
			}
		}
	}

	public ThumbstickFunction oculusThumbstickFunction
	{
		get
		{
			return _oculusThumbstickFunction;
		}
		set
		{
			if (_oculusThumbstickFunction != value)
			{
				_oculusThumbstickFunction = value;
				if (oculusThumbstickFunctionPopup != null)
				{
					oculusThumbstickFunctionPopup.currentValue = _oculusThumbstickFunction.ToString();
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool vamXIntalled => _vamXInstalled;

	protected RectTransform CreateDynamicUIElement(RectTransform parent, RectTransform prefab)
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate(prefab);
		rectTransform.SetParent(parent, false);
		return rectTransform;
	}

	public UIDynamicButton CreateDynamicButton(RectTransform parent)
	{
		UIDynamicButton uIDynamicButton = null;
		if (dynamicButtonPrefab != null)
		{
			RectTransform rectTransform = CreateDynamicUIElement(parent, dynamicButtonPrefab);
			uIDynamicButton = rectTransform.GetComponentInChildren<UIDynamicButton>(true);
			if (uIDynamicButton == null)
			{
				UnityEngine.Object.Destroy(rectTransform);
			}
		}
		return uIDynamicButton;
	}

	public UIDynamicToggle CreateDynamicToggle(RectTransform parent)
	{
		UIDynamicToggle uIDynamicToggle = null;
		if (dynamicTogglePrefab != null)
		{
			RectTransform rectTransform = CreateDynamicUIElement(parent, dynamicTogglePrefab);
			uIDynamicToggle = rectTransform.GetComponentInChildren<UIDynamicToggle>(true);
			if (uIDynamicToggle == null)
			{
				UnityEngine.Object.Destroy(rectTransform);
			}
		}
		return uIDynamicToggle;
	}

	public UIDynamicSlider CreateDynamicSlider(RectTransform parent)
	{
		UIDynamicSlider uIDynamicSlider = null;
		if (dynamicSliderPrefab != null)
		{
			RectTransform rectTransform = CreateDynamicUIElement(parent, dynamicSliderPrefab);
			uIDynamicSlider = rectTransform.GetComponentInChildren<UIDynamicSlider>(true);
			if (uIDynamicSlider == null)
			{
				UnityEngine.Object.Destroy(rectTransform);
			}
		}
		return uIDynamicSlider;
	}

	protected void SyncAdvancedSceneEditModeTransforms()
	{
		if (advancedSceneEditOnlyEditModeTransforms != null)
		{
			Transform[] array = advancedSceneEditOnlyEditModeTransforms;
			foreach (Transform transform in array)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(!advancedSceneEditDisabled && gameMode == GameMode.Edit);
				}
			}
		}
		if (advancedSceneEditDisabledEditModeTransforms == null)
		{
			return;
		}
		Transform[] array2 = advancedSceneEditDisabledEditModeTransforms;
		foreach (Transform transform2 in array2)
		{
			if (transform2 != null)
			{
				transform2.gameObject.SetActive(advancedSceneEditDisabled && gameMode == GameMode.Edit);
			}
		}
	}

	public void SyncUIToUnlockLevel()
	{
		if (loadSceneButtons != null)
		{
			Transform[] array = loadSceneButtons;
			foreach (Transform transform in array)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(!loadSceneButtonDisabled);
				}
			}
		}
		if (loadSceneDisabledButtons != null)
		{
			Transform[] array2 = loadSceneDisabledButtons;
			foreach (Transform transform2 in array2)
			{
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(loadSceneButtonDisabled);
				}
			}
		}
		if (onlineBrowseSceneButtons != null)
		{
			Transform[] array3 = onlineBrowseSceneButtons;
			foreach (Transform transform3 in array3)
			{
				if (transform3 != null)
				{
					transform3.gameObject.SetActive(!browseDisabled);
				}
			}
		}
		if (onlineBrowseSceneDisabledButtons != null)
		{
			Transform[] array4 = onlineBrowseSceneDisabledButtons;
			foreach (Transform transform4 in array4)
			{
				if (transform4 != null)
				{
					transform4.gameObject.SetActive(browseDisabled);
				}
			}
		}
		if (saveSceneButtons != null)
		{
			Transform[] array5 = saveSceneButtons;
			foreach (Transform transform5 in array5)
			{
				if (transform5 != null)
				{
					transform5.gameObject.SetActive(!saveSceneButtonDisabled);
				}
			}
		}
		if (saveSceneDisabledButtons != null)
		{
			Transform[] array6 = saveSceneDisabledButtons;
			foreach (Transform transform6 in array6)
			{
				if (transform6 != null)
				{
					transform6.gameObject.SetActive(saveSceneButtonDisabled);
				}
			}
		}
		if (advancedSceneEditButtons != null)
		{
			Transform[] array7 = advancedSceneEditButtons;
			foreach (Transform transform7 in array7)
			{
				if (transform7 != null)
				{
					transform7.gameObject.SetActive(!advancedSceneEditDisabled);
				}
			}
		}
		if (advancedSceneEditDisabledButtons != null)
		{
			Transform[] array8 = advancedSceneEditDisabledButtons;
			foreach (Transform transform8 in array8)
			{
				if (transform8 != null)
				{
					transform8.gameObject.SetActive(advancedSceneEditDisabled);
				}
			}
		}
		if (promotionalTransforms != null)
		{
			Transform[] array9 = promotionalTransforms;
			foreach (Transform transform9 in array9)
			{
				if (transform9 != null)
				{
					transform9.gameObject.SetActive(!promotionalDisabled);
				}
			}
		}
		if (keyInformationTransforms != null)
		{
			Transform[] array10 = keyInformationTransforms;
			foreach (Transform transform10 in array10)
			{
				if (transform10 != null)
				{
					transform10.gameObject.SetActive(!keyInformationDisabled);
				}
			}
		}
		if (hubDisabledTransforms != null && hubDisabled)
		{
			Transform[] array11 = hubDisabledTransforms;
			foreach (Transform transform11 in array11)
			{
				if (transform11 != null)
				{
					transform11.gameObject.SetActive(false);
				}
			}
			Transform[] array12 = hubDisabledEnableTransforms;
			foreach (Transform transform12 in array12)
			{
				if (transform12 != null)
				{
					transform12.gameObject.SetActive(true);
				}
			}
		}
		if (termsOfUseTransforms != null)
		{
			Transform[] array13 = termsOfUseTransforms;
			foreach (Transform transform13 in array13)
			{
				if (transform13 != null)
				{
					transform13.gameObject.SetActive(!termsOfUseDisabled);
				}
			}
		}
		SyncAdvancedSceneEditModeTransforms();
		SyncVamX();
	}

	protected static string GetSha256Hash(SHA256 shaHash, string input, int length)
	{
		byte[] array = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length && i < length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	protected KeyType IsValidKey(string key)
	{
		KeyType keyType;
		switch (key[0])
		{
		case 'F':
		case 'f':
			keyType = KeyType.Free;
			break;
		case 'T':
		case 't':
			keyType = KeyType.Teaser;
			break;
		case 'E':
		case 'e':
			keyType = KeyType.Entertainer;
			break;
		case 'N':
		case 'n':
			keyType = KeyType.NSteam;
			break;
		case 'S':
		case 's':
			keyType = KeyType.Steam;
			break;
		case 'R':
		case 'r':
			keyType = KeyType.Retail;
			break;
		case 'C':
		case 'c':
			keyType = KeyType.Creator;
			break;
		default:
			return KeyType.Invalid;
		}
		SHA256 shaHash = SHA256.Create();
		string sha256Hash = GetSha256Hash(shaHash, keyType.ToString() + key.ToUpper(), 3);
		string text = null;
		string text2 = null;
		switch (keyType)
		{
		case KeyType.Free:
			text = "F";
			text2 = freeKey;
			break;
		case KeyType.Teaser:
			text = "T";
			text2 = teaserKey;
			break;
		case KeyType.Entertainer:
			text = "E";
			text2 = entertainerKey;
			break;
		case KeyType.NSteam:
			text = "N";
			text2 = nsteamKey;
			break;
		case KeyType.Steam:
			text = "S";
			text2 = steamKey;
			break;
		case KeyType.Retail:
			text = "R";
			text2 = retailKey;
			break;
		case KeyType.Creator:
			text = "C";
			text2 = creatorKey;
			break;
		}
		if (text + sha256Hash == text2)
		{
			return keyType;
		}
		return KeyType.Invalid;
	}

	protected IEnumerator SyncToKeyFilePackageRefresh()
	{
		if (keySyncingIndicator != null)
		{
			keySyncingIndicator.gameObject.SetActive(true);
		}
		yield return null;
		FileManager.Refresh();
		if (keySyncingIndicator != null)
		{
			keySyncingIndicator.gameObject.SetActive(false);
		}
	}

	protected void SyncToKeyFile(bool userInvoked = false)
	{
		KeyType keyType = KeyType.Free;
		if (keyFilePath != null && keyFilePath != string.Empty)
		{
			if (FileManager.FileExists(keyFilePath))
			{
				try
				{
					string aJSON = FileManager.ReadAllText(keyFilePath, true);
					JSONNode jSONNode = JSON.Parse(aJSON);
					if (jSONNode != null)
					{
						JSONClass asObject = jSONNode.AsObject;
						if (asObject != null)
						{
							foreach (string key in asObject.Keys)
							{
								switch (IsValidKey(key))
								{
								case KeyType.Creator:
									if (keyType < KeyType.Creator)
									{
										keyType = KeyType.Creator;
									}
									break;
								case KeyType.Retail:
									if (keyType < KeyType.Retail)
									{
										keyType = KeyType.Retail;
									}
									break;
								case KeyType.Steam:
									if (keyType < KeyType.Steam)
									{
										keyType = KeyType.Steam;
									}
									break;
								case KeyType.NSteam:
									if (keyType < KeyType.NSteam)
									{
										keyType = KeyType.NSteam;
									}
									break;
								case KeyType.Entertainer:
									if (keyType < KeyType.Entertainer)
									{
										keyType = KeyType.Entertainer;
									}
									break;
								case KeyType.Teaser:
									if (keyType < KeyType.Teaser)
									{
										keyType = KeyType.Teaser;
									}
									break;
								}
							}
						}
						else
						{
							UnityEngine.Debug.LogError("Invalid key file");
							if (keyEntryStatus != null)
							{
								keyEntryStatus.text = "Invalid key file";
							}
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Invalid key file");
						if (keyEntryStatus != null)
						{
							keyEntryStatus.text = "Invalid key file";
						}
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Exception while syncing key file " + ex.Message);
					if (keyEntryStatus != null)
					{
						keyEntryStatus.text = "Exception while syncing key file " + ex.Message;
					}
				}
			}
		}
		else if (Application.isEditor)
		{
			keyType = editorMimicHighestKey;
		}
		if (legacySteamKeyFilePath != null && legacySteamKeyFilePath != string.Empty && FileManager.FileExists(legacySteamKeyFilePath))
		{
			try
			{
				string aJSON2 = FileManager.ReadAllText(legacySteamKeyFilePath, true);
				JSONNode jSONNode2 = JSON.Parse(aJSON2);
				if (jSONNode2 != null)
				{
					JSONClass asObject2 = jSONNode2.AsObject;
					if (asObject2 != null)
					{
						foreach (string key2 in asObject2.Keys)
						{
							KeyType keyType2 = IsValidKey(key2);
							if (keyType2 == KeyType.Steam && keyType < KeyType.Steam)
							{
								keyType = KeyType.Steam;
							}
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Invalid legacy Steam key file");
						if (keyEntryStatus != null)
						{
							keyEntryStatus.text = "Invalid legacy Steam key file";
						}
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Invalid legacy Steam key file");
					if (keyEntryStatus != null)
					{
						keyEntryStatus.text = "Invalid legacy Steam key file";
					}
				}
			}
			catch (Exception ex2)
			{
				UnityEngine.Debug.LogError("Exception while syncing legacy Steam key file " + ex2.Message);
				if (keyEntryStatus != null)
				{
					keyEntryStatus.text = "Exception while syncing legacy Steam key file " + ex2.Message;
				}
			}
		}
		switch (keyType)
		{
		case KeyType.Free:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = true;
				GlobalSceneOptions.singleton.disableSaveSceneButton = true;
				GlobalSceneOptions.singleton.disableLoadSceneButton = true;
				GlobalSceneOptions.singleton.disableCustomUI = true;
				GlobalSceneOptions.singleton.disableBrowse = true;
				GlobalSceneOptions.singleton.disablePackages = true;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = false;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = true;
				disableSaveSceneButton = true;
				disableLoadSceneButton = true;
				disableCustomUI = true;
				disableBrowse = true;
				disablePackages = true;
				disablePromotional = false;
				disableKeyInformation = false;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		case KeyType.Teaser:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = true;
				GlobalSceneOptions.singleton.disableSaveSceneButton = true;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = false;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = true;
				disableSaveSceneButton = true;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = false;
				disableKeyInformation = false;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		case KeyType.Entertainer:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = true;
				GlobalSceneOptions.singleton.disableSaveSceneButton = false;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = false;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = true;
				disableSaveSceneButton = false;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = false;
				disableKeyInformation = false;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		case KeyType.NSteam:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = false;
				GlobalSceneOptions.singleton.disableSaveSceneButton = false;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = true;
				GlobalSceneOptions.singleton.disableKeyInformation = true;
				GlobalSceneOptions.singleton.disableHub = true;
				GlobalSceneOptions.singleton.disableTermsOfUse = true;
			}
			else
			{
				disableAdvancedSceneEdit = false;
				disableSaveSceneButton = false;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = true;
				disableKeyInformation = true;
				disableHub = true;
				disableTermsOfUse = true;
			}
			break;
		case KeyType.Steam:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = false;
				GlobalSceneOptions.singleton.disableSaveSceneButton = false;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = true;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = false;
				disableSaveSceneButton = false;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = false;
				disableKeyInformation = true;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		case KeyType.Retail:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = false;
				GlobalSceneOptions.singleton.disableSaveSceneButton = false;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = true;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = false;
				disableSaveSceneButton = false;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = false;
				disableKeyInformation = true;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		case KeyType.Creator:
			if (GlobalSceneOptions.singleton != null)
			{
				GlobalSceneOptions.singleton.disableAdvancedSceneEdit = false;
				GlobalSceneOptions.singleton.disableSaveSceneButton = false;
				GlobalSceneOptions.singleton.disableLoadSceneButton = false;
				GlobalSceneOptions.singleton.disableCustomUI = false;
				GlobalSceneOptions.singleton.disableBrowse = false;
				GlobalSceneOptions.singleton.disablePackages = false;
				GlobalSceneOptions.singleton.disablePromotional = false;
				GlobalSceneOptions.singleton.disableKeyInformation = false;
				GlobalSceneOptions.singleton.disableHub = false;
				GlobalSceneOptions.singleton.disableTermsOfUse = false;
			}
			else
			{
				disableAdvancedSceneEdit = false;
				disableSaveSceneButton = false;
				disableLoadSceneButton = false;
				disableCustomUI = false;
				disableBrowse = false;
				disablePackages = false;
				disablePromotional = false;
				disableKeyInformation = false;
				disableHub = false;
				disableTermsOfUse = false;
			}
			break;
		}
		if (freeKeyTransform != null)
		{
			freeKeyTransform.gameObject.SetActive(keyType == KeyType.Free);
		}
		if (teaserKeyTransform != null)
		{
			teaserKeyTransform.gameObject.SetActive(keyType == KeyType.Teaser);
		}
		if (entertainerKeyTransform != null)
		{
			entertainerKeyTransform.gameObject.SetActive(keyType == KeyType.Entertainer);
		}
		if (creatorKeyTransform != null)
		{
			creatorKeyTransform.gameObject.SetActive(keyType == KeyType.Creator);
		}
		SyncUIToUnlockLevel();
		FileManager.packagesEnabled = !packagesDisabled;
		if (userInvoked)
		{
			StartCoroutine(SyncToKeyFilePackageRefresh());
		}
		else
		{
			FileManager.Refresh();
		}
	}

	public void AddKey()
	{
		if (keyFilePath != null && keyFilePath != string.Empty && keyInputField != null)
		{
			JSONClass jSONClass;
			if (FileManager.FileExists(keyFilePath, true))
			{
				string aJSON = FileManager.ReadAllText(keyFilePath, true);
				JSONNode jSONNode = JSON.Parse(aJSON);
				if (jSONNode == null)
				{
					jSONClass = new JSONClass();
				}
				else
				{
					jSONClass = jSONNode.AsObject;
					if (jSONClass == null)
					{
						jSONClass = new JSONClass();
					}
				}
			}
			else
			{
				jSONClass = new JSONClass();
			}
			string text = keyInputField.text;
			if (IsValidKey(text) != 0)
			{
				if (keyEntryStatus != null)
				{
					keyEntryStatus.text = "Key accepted";
				}
				jSONClass[text].AsBool = true;
				string text2 = jSONClass.ToString(string.Empty);
				try
				{
					FileManager.CreateDirectory(Path.GetDirectoryName(keyFilePath));
					FileManager.WriteAllText(keyFilePath, text2);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Exception while storing key " + ex.Message);
					if (keyEntryStatus != null)
					{
						keyEntryStatus.text = "Exception while storing key " + ex.Message;
					}
					return;
				}
				SyncToKeyFile(true);
			}
			else if (keyEntryStatus != null)
			{
				keyEntryStatus.text = "Invalid key";
			}
		}
		else if (keyEntryStatus != null)
		{
			keyEntryStatus.text = "Keys not needed";
		}
	}

	public void OutputEncryptedKey(string keyval)
	{
		KeyType keyType;
		string text;
		switch (keyval[0])
		{
		default:
			return;
		case 'F':
		case 'f':
			keyType = KeyType.Free;
			text = "F";
			break;
		case 'T':
		case 't':
			keyType = KeyType.Teaser;
			text = "T";
			break;
		case 'E':
		case 'e':
			keyType = KeyType.Entertainer;
			text = "E";
			break;
		case 'C':
		case 'c':
			keyType = KeyType.Creator;
			text = "C";
			break;
		case 'N':
		case 'n':
			keyType = KeyType.NSteam;
			text = "N";
			break;
		case 'S':
		case 's':
			keyType = KeyType.Steam;
			text = "S";
			break;
		case 'R':
		case 'r':
			keyType = KeyType.Retail;
			text = "R";
			break;
		}
		SHA256 shaHash = SHA256.Create();
		string sha256Hash = GetSha256Hash(shaHash, keyType.ToString() + keyval.ToUpper(), 3);
		string text2 = text + sha256Hash;
		UnityEngine.Debug.Log("Encrypted value for key " + keyval + " is " + text2);
	}

	public void GetScenePathDialog(FileBrowserCallback callback)
	{
		LoadDialog(lastScenePathDir, false);
		fileBrowserUI.SetTitle("Select File");
		fileBrowserUI.Show(callback);
	}

	protected void GetMediaPathDialogInternal(string filter = "", string suggestedFolder = null, bool fullComputerBrowse = true, bool showDirs = true, bool showKeepOpt = false, string fileRemovePrefix = null, bool hideExtension = false, List<ShortCut> shortCuts = null, bool browseVarFilesAsDirectories = true, bool showInstallFolderInDirectoryList = false)
	{
		string text = savesDirResolved + "scene";
		if (suggestedFolder != null && suggestedFolder != string.Empty)
		{
			if (FileManager.DirectoryExists(suggestedFolder))
			{
				text = suggestedFolder;
			}
			else
			{
				text = ".";
				fullComputerBrowse = true;
			}
		}
		else if (lastMediaDir != string.Empty && FileManager.DirectoryExists(lastMediaDir))
		{
			text = lastMediaDir;
		}
		List<ShortCut> list = shortCuts;
		if (fullComputerBrowse)
		{
			VarDirectoryEntry varDirectoryEntry = FileManager.GetVarDirectoryEntry(text);
			text = ((varDirectoryEntry == null) ? Path.GetFullPath(text) : varDirectoryEntry.Path);
			if (list == null)
			{
				list = new List<ShortCut>();
				ShortCut shortCut = new ShortCut();
				shortCut.package = string.Empty;
				shortCut.displayName = "Default";
				shortCut.path = text;
				list.Add(shortCut);
				ShortCut shortCut2 = new ShortCut();
				shortCut2.package = string.Empty;
				shortCut2.displayName = "Addon Packages";
				shortCut2.path = "AddonPackages";
				list.Add(shortCut2);
			}
		}
		mediaFileBrowserUI.fileRemovePrefix = fileRemovePrefix;
		mediaFileBrowserUI.hideExtension = hideExtension;
		mediaFileBrowserUI.keepOpen = false;
		mediaFileBrowserUI.fileFormat = filter;
		mediaFileBrowserUI.defaultPath = text;
		mediaFileBrowserUI.showDirs = showDirs;
		mediaFileBrowserUI.SetShortCuts(list);
		mediaFileBrowserUI.browseVarFilesAsDirectories = browseVarFilesAsDirectories;
		mediaFileBrowserUI.showInstallFolderInDirectoryList = showInstallFolderInDirectoryList;
		mediaFileBrowserUI.SetTextEntry(false);
	}

	public void GetMediaPathDialog(FileBrowserCallback callback, string filter = "", string suggestedFolder = null, bool fullComputerBrowse = true, bool showDirs = true, bool showKeepOpt = false, string fileRemovePrefix = null, bool hideExtenstion = false, List<ShortCut> shortCuts = null, bool browseVarFilesAsDirectories = true, bool showInstallFolderInDirectoryList = false)
	{
		if (!browseDisabled)
		{
			GetMediaPathDialogInternal(filter, suggestedFolder, fullComputerBrowse, showDirs, showKeepOpt, fileRemovePrefix, hideExtenstion, shortCuts, browseVarFilesAsDirectories, showInstallFolderInDirectoryList);
			mediaFileBrowserUI.Show(callback);
		}
		else
		{
			LogMessage("Please back this project on Patreon at https://www.patreon.com/meshedvr to unlock this feature!");
			callback?.Invoke(string.Empty);
		}
	}

	public void GetMediaPathDialog(FileBrowserFullCallback callback, string filter = "", string suggestedFolder = null, bool fullComputerBrowse = true, bool showDirs = true, bool showKeepOpt = false, string fileRemovePrefix = null, bool hideExtenstion = false, List<ShortCut> shortCuts = null, bool browseVarFilesAsDirectories = true, bool showInstallFolderInDirectoryList = false)
	{
		if (!browseDisabled)
		{
			GetMediaPathDialogInternal(filter, suggestedFolder, fullComputerBrowse, showDirs, showKeepOpt, fileRemovePrefix, hideExtenstion, shortCuts, browseVarFilesAsDirectories, showInstallFolderInDirectoryList);
			mediaFileBrowserUI.Show(callback);
		}
		else
		{
			LogMessage("Please back this project on Patreon at https://www.patreon.com/meshedvr to unlock this feature!");
			callback?.Invoke(string.Empty, true);
		}
	}

	protected void TestDirectoryCallback(string dir)
	{
		UnityEngine.Debug.Log("Selected dir " + dir);
	}

	public void TestDirectoryPathBrowse()
	{
		GetDirectoryPathDialog(TestDirectoryCallback);
	}

	public void GetDirectoryPathDialog(FileBrowserCallback callback, string suggestedFolder = null, List<ShortCut> shortCuts = null, bool fullComputerBrowse = true)
	{
		if (!browseDisabled)
		{
			string text = savesDirResolved + "scene";
			if (suggestedFolder != null && suggestedFolder != string.Empty && FileManager.DirectoryExists(suggestedFolder))
			{
				text = suggestedFolder;
			}
			else if (lastBrowseDir != string.Empty && FileManager.DirectoryExists(lastBrowseDir))
			{
				text = lastBrowseDir;
			}
			if (fullComputerBrowse)
			{
				text = Path.GetFullPath(text);
			}
			directoryBrowserUI.fileFormat = string.Empty;
			directoryBrowserUI.defaultPath = text;
			directoryBrowserUI.SetShortCuts(shortCuts);
			directoryBrowserUI.SetTextEntry(true);
			directoryBrowserUI.Show(callback);
		}
		else
		{
			LogMessage("Please back this project on Patreon at https://www.patreon.com/meshedvr to unlock this feature!");
			callback?.Invoke(string.Empty);
		}
	}

	protected void SetSavesDirFromCommandline()
	{
		if (Application.isEditor)
		{
			return;
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-savesdir" && i + 1 < commandLineArgs.Length)
			{
				savesDir = commandLineArgs[i + 1];
				if (savesDir[savesDir.Length - 1] != '\\')
				{
					savesDir += "\\";
				}
			}
		}
	}

	public void StartScene()
	{
		if (embeddedJSONScene != null)
		{
			LoadFromJSONEmbed(embeddedJSONScene);
			return;
		}
		if (File.Exists(savesDirResolved + startSceneName))
		{
			Load(savesDirResolved + startSceneName);
		}
		else
		{
			Load(savesDirResolved + startSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void StartSceneForEdit()
	{
		if (embeddedJSONScene != null)
		{
			LoadFromJSONEmbed(embeddedJSONScene);
			return;
		}
		if (File.Exists(savesDirResolved + startSceneName))
		{
			LoadForEdit(savesDirResolved + startSceneName);
		}
		else
		{
			LoadForEdit(savesDirResolved + startSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void NewScene()
	{
		if (newJSONEmbedScene != null)
		{
			LoadFromJSONEmbed(newJSONEmbedScene, false, true);
			return;
		}
		if (File.Exists(savesDirResolved + newSceneName))
		{
			LoadForEdit(savesDirResolved + newSceneName);
		}
		else
		{
			LoadForEdit(savesDirResolved + newSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void NewScenePlayMode()
	{
		if (newJSONEmbedScene != null)
		{
			LoadFromJSONEmbed(newJSONEmbedScene);
			return;
		}
		if (File.Exists(savesDirResolved + newSceneName))
		{
			Load(savesDirResolved + newSceneName);
		}
		else
		{
			Load(savesDirResolved + newSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void ClearScene()
	{
		if (Application.isPlaying)
		{
			if (!_isLoading)
			{
				onStartScene = false;
				gameMode = GameMode.Edit;
				loadedName = string.Empty;
				_isLoading = true;
				StartCoroutine(LoadCo(true));
			}
			else
			{
				UnityEngine.Debug.LogWarning("Already loading file " + loadedName + ". Can't clear until complete");
			}
		}
	}

	public void SaveSceneDialog()
	{
		SaveSceneDialog(SaveFromDialog);
	}

	public void SaveSceneLegacyPackageDialog()
	{
		SaveSceneDialog(SaveLegacyPackageFromDialog);
	}

	public void SaveSceneNewAddonPackageDialog()
	{
		SaveSceneDialog(SaveAndAddToNewPackageFromDialog);
	}

	public void SaveSceneCurrentAddonPackageDialog()
	{
		SaveSceneDialog(SaveAndAddToCurrentPackageFromDialog);
	}

	public void OpenPackageBuilder()
	{
		activeUI = ActiveUI.PackageBuilder;
	}

	public void OpenPackageManager()
	{
		ShowMainHUDAuto();
		activeUI = ActiveUI.PackageManager;
	}

	public void OpenPackageDownloader()
	{
		ShowMainHUDAuto();
		activeUI = ActiveUI.PackageDownloader;
	}

	public void OpenPackageInManager(string packageUid)
	{
		packageUid = Regex.Replace(packageUid, ":.*", string.Empty);
		OpenPackageManager();
		if (packageManager != null)
		{
			packageManager.LoadMetaFromPackageUid(packageUid);
		}
	}

	public void RescanPackages()
	{
		FileManager.Refresh();
	}

	protected void ClearFileBrowsersCurrentPath()
	{
		if (fileBrowserUI != null)
		{
			fileBrowserUI.ClearCurrentPath();
		}
		if (fileBrowserWorldUI != null)
		{
			fileBrowserWorldUI.ClearCurrentPath();
		}
	}

	protected void OnPackageRefresh()
	{
		ClearFileBrowsersCurrentPath();
		SyncVamX();
	}

	public void OpenLinkInBrowser(string url)
	{
		if (onlineBrowser != null)
		{
			activeUI = ActiveUI.OnlineBrowser;
			onlineBrowser.url = url;
		}
	}

	public void SaveSceneDialog(FileBrowserCallback callback)
	{
		try
		{
			string text = savesDirResolved + "scene";
			fileBrowserUI.SetShortCuts(null);
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadDir, false);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text = suggestedBrowserDirectoryFromDirectoryPath;
			}
			if (!FileManager.DirectoryExists(text, true))
			{
				FileManager.CreateDirectory(text);
			}
			fileBrowserUI.defaultPath = text;
			activeUI = ActiveUI.None;
			fileBrowserUI.SetTitle("Select Save File");
			fileBrowserUI.SetTextEntry(true);
			fileBrowserUI.fileFormat = "json|vac|zip";
			fileBrowserUI.Show(callback);
			if (fileBrowserUI.fileEntryField != null)
			{
				string text2 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds/*cast due to .constrained prefix*/).ToString();
				fileBrowserUI.fileEntryField.text = text2;
				fileBrowserUI.ActivateFileNameField();
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during open of save scene dialog: " + ex);
		}
	}

	public void SaveConfirm(string option)
	{
		if (_lastActiveUI == ActiveUI.MultiButtonPanel)
		{
			activeUI = ActiveUI.None;
		}
		else
		{
			activeUI = _lastActiveUI;
		}
		multiButtonPanel.gameObject.SetActive(false);
		multiButtonPanel.buttonCallback = null;
		if (option == "Save New")
		{
			int num = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			loadedName = savesDirResolved + "scene\\" + num + ".json";
			SaveInternal(loadedName);
		}
		else if (loadedName != null && loadedName != string.Empty && option == "Overwrite Current")
		{
			SaveInternal(loadedName, null, true, true, null, true);
		}
	}

	public void SaveAddDependency(Atom saveAtom)
	{
		if (_saveQueue != null)
		{
			_saveQueue.Add(saveAtom);
		}
	}

	public string NormalizeSavePath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty && path != "/" && path != "NULL")
		{
			result = FileManager.NormalizeSavePath(path);
			if (packageMode)
			{
				string fileName = Path.GetFileName(path);
				result = AddFileToPackage(path, fileName);
			}
		}
		return result;
	}

	protected void SaveFromDialog(string saveName)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (!saveName.EndsWith(".json"))
		{
			saveName += ".json";
		}
		if (FileManager.FileExists(saveName) && overwriteConfirmButton != null && overwriteConfirmPanel != null)
		{
			overwriteConfirmButton.onClick.RemoveAllListeners();
			overwriteConfirmButton.onClick.AddListener(delegate
			{
				overwriteConfirmPanel.gameObject.SetActive(false);
				SaveInternal(saveName, null, true, true, null, true);
			});
			overwriteConfirmPanel.gameObject.SetActive(true);
			if (overwriteConfirmPathText != null)
			{
				overwriteConfirmPathText.text = saveName;
			}
		}
		else
		{
			SaveInternal(saveName);
		}
	}

	protected void SaveLegacyPackageFromDialog(string saveName)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (!saveName.EndsWith(".vac"))
		{
			saveName += ".vac";
		}
		if (FileManager.FileExists(saveName) && overwriteConfirmButton != null && overwriteConfirmPanel != null)
		{
			overwriteConfirmButton.onClick.RemoveAllListeners();
			overwriteConfirmButton.onClick.AddListener(delegate
			{
				overwriteConfirmPanel.gameObject.SetActive(false);
				SavePackage(saveName, true);
			});
			overwriteConfirmPanel.gameObject.SetActive(true);
			if (overwriteConfirmPathText != null)
			{
				overwriteConfirmPathText.text = saveName;
			}
		}
		else
		{
			SavePackage(saveName);
		}
	}

	protected void SaveAndAddToCurrentPackageFromDialog(string saveName)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (!saveName.EndsWith(".json"))
		{
			saveName += ".json";
		}
		if (FileManager.FileExists(saveName) && overwriteConfirmButton != null && overwriteConfirmPanel != null)
		{
			overwriteConfirmButton.onClick.RemoveAllListeners();
			overwriteConfirmButton.onClick.AddListener(delegate
			{
				overwriteConfirmPanel.gameObject.SetActive(false);
				SaveAndAddToCurrentPackage(saveName, true);
			});
			overwriteConfirmPanel.gameObject.SetActive(true);
			if (overwriteConfirmPathText != null)
			{
				overwriteConfirmPathText.text = saveName;
			}
		}
		else
		{
			SaveAndAddToCurrentPackage(saveName);
		}
	}

	protected void SaveAndAddToNewPackageFromDialog(string saveName)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (!saveName.EndsWith(".json"))
		{
			saveName += ".json";
		}
		if (FileManager.FileExists(saveName) && overwriteConfirmButton != null && overwriteConfirmPanel != null)
		{
			overwriteConfirmButton.onClick.RemoveAllListeners();
			overwriteConfirmButton.onClick.AddListener(delegate
			{
				overwriteConfirmPanel.gameObject.SetActive(false);
				SaveAndAddToNewPackage(saveName, true);
			});
			overwriteConfirmPanel.gameObject.SetActive(true);
			if (overwriteConfirmPathText != null)
			{
				overwriteConfirmPathText.text = saveName;
			}
		}
		else
		{
			SaveAndAddToNewPackage(saveName);
		}
	}

	public void Save(string saveName)
	{
		if (saveName != string.Empty)
		{
			Save(saveName, null, true, true, null, false);
		}
	}

	protected void SaveAndAddToCurrentPackage(string saveName, bool isOverwrite = false)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName != string.Empty)
		{
			SaveInternal(saveName, null, true, true, delegate
			{
				packageBuilder.AddContentItem(saveName);
				OpenPackageBuilder();
			}, isOverwrite);
		}
	}

	protected void SaveAndAddToNewPackage(string saveName, bool isOverwrite = false)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (saveName != string.Empty)
		{
			SaveInternal(saveName, null, true, true, delegate
			{
				packageBuilder.ClearAll();
				packageBuilder.PackageName = Path.GetFileNameWithoutExtension(saveName);
				packageBuilder.AddContentItem(saveName);
				OpenPackageBuilder();
			}, isOverwrite);
		}
	}

	public JSONClass GetSaveJSON(Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true)
	{
		JSONClass jSONClass = new JSONClass();
		if (_saveQueue == null)
		{
			_saveQueue = new List<Atom>();
		}
		else
		{
			_saveQueue.Clear();
		}
		if (specificAtom == null)
		{
			if (headPossessedController != null)
			{
				jSONClass["headPossessedController"] = headPossessedController.containingAtom.uid + ":" + headPossessedController.name;
			}
			if (playerNavCollider != null)
			{
				jSONClass["playerNavCollider"] = playerNavCollider.containingAtom.uid + ":" + playerNavCollider.name;
			}
			if (worldScaleSlider != null)
			{
				SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
				if (component == null || component.defaultValue != worldScale)
				{
					jSONClass["worldScale"].AsFloat = worldScale;
				}
			}
			if (playerHeightAdjustSlider != null)
			{
				SliderControl component2 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component2 == null || component2.defaultValue != _playerHeightAdjust)
				{
					jSONClass["playerHeightAdjust"].AsFloat = _playerHeightAdjust;
				}
			}
			if (MonitorCenterCamera != null)
			{
				Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
				jSONClass["monitorCameraRotation"]["x"].AsFloat = localEulerAngles.x;
				jSONClass["monitorCameraRotation"]["y"].AsFloat = localEulerAngles.y;
				jSONClass["monitorCameraRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			if (useSceneLoadPositionToggle != null)
			{
				jSONClass["useSceneLoadPosition"].AsBool = _useSceneLoadPosition;
			}
			if (useSceneLoadPosition)
			{
				MoveToSceneLoadPosition();
			}
			JSONArray jSONArray = (JSONArray)(jSONClass["atoms"] = new JSONArray());
			foreach (Atom atoms in atomsList)
			{
				atoms.Store(jSONArray);
			}
		}
		else
		{
			JSONArray jSONArray2 = (JSONArray)(jSONClass["atoms"] = new JSONArray());
			specificAtom.Store(jSONArray2, includePhysical, includeAppearance);
			if (includePhysical)
			{
				foreach (Atom item in _saveQueue)
				{
					item.Store(jSONArray2, includePhysical, includeAppearance);
				}
			}
		}
		return jSONClass;
	}

	public void Save(string saveName = "Saves\\scene\\savefile.json", Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true, ScreenShotCallback callback = null, bool isOverwrite = false)
	{
		SaveInternal(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite, true);
	}

	public void SaveFromAtom(string saveName = "Saves\\scene\\savefile.json", Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true, ScreenShotCallback callback = null, bool isOverwrite = false)
	{
		FileManager.AssertNotCalledFromPlugin();
		SaveInternal(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite);
	}

	private void SaveInternalFinish(string saveName, Atom specificAtom, bool includePhysical, bool includeAppearance, ScreenShotCallback callback, bool isOverwrite)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (onBeforeSceneSaveHandlers != null)
		{
			onBeforeSceneSaveHandlers();
		}
		loadedName = saveName;
		int num = saveName.LastIndexOf('\\');
		if (num >= 0)
		{
			string path = saveName.Substring(0, num);
			FileManager.CreateDirectory(path);
		}
		lastLoadDir = FileManager.GetDirectoryName(saveName, true);
		if (!isOverwrite)
		{
			ClearFileBrowsersCurrentPath();
		}
		FileManager.SetSaveDirFromFilePath(saveName);
		FileManager.SetLoadDirFromFilePath(saveName);
		JSONClass saveJSON = GetSaveJSON(specificAtom, includePhysical, includeAppearance);
		SaveJSONInternal(saveJSON, saveName);
		if (onSceneSavedHandlers != null)
		{
			onSceneSavedHandlers();
		}
		DoSaveScreenshot(saveName, callback);
	}

	private void SaveInternal(string saveName = "Saves\\scene\\savefile.json", Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true, ScreenShotCallback callback = null, bool isOverwrite = false, bool fromPlugin = false)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		try
		{
			if (!saveName.EndsWith(".json"))
			{
				saveName += ".json";
			}
			if (fromPlugin)
			{
				if (!FileManager.IsSecurePluginWritePath(saveName))
				{
					throw new Exception("Attempted to save scene at non-secure path " + saveName);
				}
			}
			else if (!FileManager.IsSecureWritePath(saveName))
			{
				throw new Exception("Attempted to save scene at non-secure path " + saveName);
			}
			UnityEngine.Debug.Log("Save " + saveName);
			packageMode = false;
			if (fromPlugin)
			{
				if (File.Exists(saveName))
				{
					if (!FileManager.IsPluginWritePathThatNeedsConfirm(saveName))
					{
						SaveInternalFinish(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite);
						return;
					}
					FileManager.ConfirmPluginActionWithUser("save scene to file " + saveName, delegate
					{
						try
						{
							SaveInternalFinish(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite);
						}
						catch (Exception ex)
						{
							LogError("Exception during Save: " + ex);
						}
					}, null);
				}
				else
				{
					SaveInternalFinish(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite);
				}
			}
			else
			{
				SaveInternalFinish(saveName, specificAtom, includePhysical, includeAppearance, callback, isOverwrite);
			}
		}
		catch (Exception ex2)
		{
			LogError("Exception during Save: " + ex2);
		}
	}

	public void SaveJSON(JSONClass jc, string saveName)
	{
		SaveJSON(jc, saveName, null, null, null);
	}

	public void SaveJSON(JSONClass jc, string saveName, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
	{
		if (!FileManager.IsSecurePluginWritePath(saveName))
		{
			Exception ex = new Exception("Attempted to save json file at non-secure path " + saveName);
			if (exceptionCallback != null)
			{
				exceptionCallback(ex);
				return;
			}
			throw ex;
		}
		if (File.Exists(saveName))
		{
			if (!FileManager.IsPluginWritePathThatNeedsConfirm(saveName))
			{
				try
				{
					SaveJSONInternal(jc, saveName);
				}
				catch (Exception e)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(e);
					}
					return;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
				return;
			}
			FileManager.ConfirmPluginActionWithUser("save json to file " + saveName, delegate
			{
				try
				{
					SaveJSONInternal(jc, saveName);
				}
				catch (Exception ex2)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(ex2);
						return;
					}
					throw ex2;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
			}, denyCallback);
			return;
		}
		try
		{
			SaveJSONInternal(jc, saveName);
		}
		catch (Exception e2)
		{
			if (exceptionCallback != null)
			{
				exceptionCallback(e2);
			}
			return;
		}
		if (confirmCallback != null)
		{
			confirmCallback();
		}
	}

	private void SaveJSONInternal(JSONClass jc, string saveName)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		try
		{
			StringBuilder stringBuilder = new StringBuilder(100000);
			jc.ToString(string.Empty, stringBuilder);
			string value = stringBuilder.ToString();
			using (StreamWriter streamWriter = FileManager.OpenStreamWriter(saveName))
			{
				streamWriter.Write(value);
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during SaveJSON: " + ex);
		}
	}

	public JSONNode LoadJSON(string saveName)
	{
		JSONNode result = null;
		try
		{
			FileEntry fileEntry = FileManager.GetFileEntry(saveName, true);
			if (fileEntry != null)
			{
				using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(fileEntry))
				{
					string aJSON = fileEntryStreamReader.ReadToEnd();
					result = JSON.Parse(aJSON);
				}
			}
			else
			{
				LogError("LoadJSON: File " + saveName + " not found");
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during LoadJSON: " + ex);
		}
		return result;
	}

	public void AddVarPackageRefToVacPackage(string packageUid)
	{
		referencedVarPackages.Add(packageUid);
	}

	public string AddFileToPackage(string path, string packagepath)
	{
		if (zos != null)
		{
			VarFileEntry varFileEntry = FileManager.GetVarFileEntry(path);
			if (varFileEntry == null)
			{
				if (!alreadyPackaged.ContainsKey(packagepath))
				{
					alreadyPackaged.Add(packagepath, true);
					byte[] buffer = new byte[4096];
					ZipEntry entry = new ZipEntry(packagepath);
					zos.PutNextEntry(entry);
					using (FileEntryStream fileEntryStream = FileManager.OpenStream(path))
					{
						StreamUtils.Copy(fileEntryStream.Stream, zos, buffer);
					}
					zos.CloseEntry();
				}
				return packagepath;
			}
			referencedVarPackages.Add(varFileEntry.Package.Uid);
		}
		return path;
	}

	private void SavePackage(string saveName = "Saves\\scene\\savefile.vac", bool isOverwrite = false)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		try
		{
			if (!(saveName != string.Empty))
			{
				return;
			}
			alreadyPackaged = new Dictionary<string, bool>();
			referencedVarPackages = new HashSet<string>();
			string fileName = Path.GetFileName(saveName);
			fileName = fileName.Replace(".vac", string.Empty);
			if (!saveName.EndsWith(".vac"))
			{
				saveName += ".vac";
			}
			packageMode = true;
			UnityEngine.Debug.Log("Save Package " + saveName);
			byte[] buffer = new byte[4096];
			using (zos = new ZipOutputStream(File.Create(saveName)))
			{
				zos.SetLevel(5);
				JSONClass saveJSON = GetSaveJSON();
				ZipEntry entry = new ZipEntry(fileName + ".json");
				zos.PutNextEntry(entry);
				StringBuilder stringBuilder = new StringBuilder(100000);
				saveJSON.ToString(string.Empty, stringBuilder);
				string s = stringBuilder.ToString();
				using (MemoryStream source = new MemoryStream(Encoding.Default.GetBytes(s)))
				{
					StreamUtils.Copy(source, zos, buffer);
				}
				zos.CloseEntry();
				ZipEntry entry2 = new ZipEntry("meta.json");
				zos.PutNextEntry(entry2);
				JSONClass jSONClass = new JSONClass();
				JSONClass jSONClass2 = new JSONClass();
				HashSet<string> visited = new HashSet<string>();
				HashSet<VarPackage> allReferencedPackages = new HashSet<VarPackage>();
				HashSet<string> allReferencedPackageUids = new HashSet<string>();
				foreach (string referencedVarPackage in referencedVarPackages)
				{
					VarPackage package = FileManager.GetPackage(referencedVarPackage);
					PackageBuilder.GetPackageDependenciesRecursive(package, referencedVarPackage, visited, allReferencedPackages, allReferencedPackageUids, jSONClass2);
					LogMessage("INFO: VAC references VAR package " + referencedVarPackage);
				}
				jSONClass["programVersion"] = GetVersion();
				jSONClass["dependencies"] = jSONClass2;
				s = jSONClass.ToString(string.Empty);
				using (MemoryStream source2 = new MemoryStream(Encoding.Default.GetBytes(s)))
				{
					StreamUtils.Copy(source2, zos, buffer);
				}
				zos.CloseEntry();
			}
			if (!isOverwrite)
			{
				ClearFileBrowsersCurrentPath();
			}
			packageMode = false;
			DoSaveScreenshot(saveName);
		}
		catch (Exception ex)
		{
			LogError("Exception during SavePackage: " + ex);
		}
	}

	protected IEnumerator LoadCo(bool clearOnly = false, bool loadMerge = false)
	{
		hideWaitTransform = loadMerge;
		if (loadFlag != null)
		{
			loadFlag.Raise();
		}
		loadFlag = new AsyncFlag("Scene Load");
		ResetSimulation(loadFlag);
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.pauseGlow = true;
		}
		DeactivateWorldUI();
		if (loadingUI != null)
		{
			if (!loadMerge)
			{
				if (fileBrowserUI == null || fileBrowserUI.IsHidden() || !fileBrowserUI.keepOpen)
				{
					HideMainHUD();
				}
				HUDAnchor.SetAnchorsToReference();
				loadingUI.gameObject.SetActive(true);
				if (loadingUIAlt != null && !_mainHUDAnchoredOnMonitor)
				{
					loadingUIAlt.gameObject.SetActive(true);
				}
			}
			if (loadingGeometry != null)
			{
				loadingGeometry.gameObject.SetActive(true);
			}
		}
		yield return null;
		ResetMonitorCenterCamera();
		if (!loadMerge)
		{
			JSONClass jc = new JSONClass();
			ClearSelection();
			ClearAllGrabbedControllers();
			ClearPossess();
			DisconnectNavRigFromPlayerNavCollider();
			if (worldScaleSlider != null)
			{
				SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
				if (component != null)
				{
					worldScale = component.defaultValue;
				}
			}
			if (playerHeightAdjustSlider != null)
			{
				SliderControl component2 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component2 != null)
				{
					playerHeightAdjust = component2.defaultValue;
				}
			}
			Atom[] atms = new Atom[atoms.Count];
			atomsList.CopyTo(atms, 0);
			foreach (Atom atom in atms)
			{
				if (atom != null)
				{
					atom.PreRestore();
				}
			}
			RemoveNonStartingAtoms();
			yield return null;
			foreach (Atom atoms in atomsList)
			{
				atoms.ClearParentAtom();
			}
			foreach (Atom atoms2 in atomsList)
			{
				atoms2.RestoreTransform(jc);
			}
			foreach (Atom atoms3 in atomsList)
			{
				atoms3.RestoreParentAtom(jc);
			}
			FileManager.PushLoadDir(string.Empty);
			foreach (Atom atoms4 in atomsList)
			{
				atoms4.Restore(jc, true, true, true, null, true);
			}
			FileManager.PopLoadDir();
			foreach (Atom atoms5 in atomsList)
			{
				atoms5.LateRestore(jc);
			}
			foreach (Atom atoms6 in atomsList)
			{
				atoms6.PostRestore();
			}
			if (UserPreferences.singleton != null && UserPreferences.singleton.optimizeMemoryOnSceneLoad && MemoryOptimizer.singleton != null)
			{
				yield return StartCoroutine(MemoryOptimizer.singleton.OptimizeMemoryUsage());
			}
			else
			{
				yield return Resources.UnloadUnusedAssets();
				GC.Collect();
			}
		}
		if (!clearOnly)
		{
			JSONArray jatoms = loadJson["atoms"].AsArray;
			if (loadJson["worldScale"] != null)
			{
				worldScale = loadJson["worldScale"].AsFloat;
			}
			else if (worldScaleSlider != null)
			{
				SliderControl component3 = worldScaleSlider.GetComponent<SliderControl>();
				if (component3 != null)
				{
					worldScale = component3.defaultValue;
				}
			}
			if (loadJson["environmentHeight"] != null)
			{
				playerHeightAdjust = loadJson["environmentHeight"].AsFloat;
			}
			else if (loadJson["playerHeightAdjust"] != null)
			{
				playerHeightAdjust = loadJson["playerHeightAdjust"].AsFloat;
			}
			else if (playerHeightAdjustSlider != null)
			{
				SliderControl component4 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component4 != null)
				{
					playerHeightAdjust = component4.defaultValue;
				}
			}
			if (loadJson["monitorCameraRotation"] != null)
			{
				Vector3 localEulerAngles = default(Vector3);
				localEulerAngles.x = 0f;
				localEulerAngles.y = 0f;
				localEulerAngles.z = 0f;
				if (loadJson["monitorCameraRotation"]["x"] != null)
				{
					localEulerAngles.x = loadJson["monitorCameraRotation"]["x"].AsFloat;
				}
				if (loadJson["monitorCameraRotation"]["y"] != null)
				{
					localEulerAngles.y = loadJson["monitorCameraRotation"]["y"].AsFloat;
				}
				if (loadJson["monitorCameraRotation"]["z"] != null)
				{
					localEulerAngles.z = loadJson["monitorCameraRotation"]["z"].AsFloat;
				}
				if (MonitorCenterCamera != null)
				{
					MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
				}
			}
			if (loadJson["useSceneLoadPosition"] != null)
			{
				useSceneLoadPosition = loadJson["useSceneLoadPosition"].AsBool;
			}
			if (loadingProgressSlider != null)
			{
				loadingProgressSlider.minValue = 0f;
				loadingProgressSlider.maxValue = (float)jatoms.Count * 2f + 2f;
				loadingProgressSlider.value = 0f;
			}
			if (loadingProgressSliderAlt != null)
			{
				loadingProgressSliderAlt.minValue = 0f;
				loadingProgressSliderAlt.maxValue = (float)jatoms.Count * 2f + 2f;
				loadingProgressSliderAlt.value = 0f;
			}
			UpdateLoadingStatus("Pre-Restore");
			IEnumerator enumerator7 = jatoms.GetEnumerator();
			try
			{
				while (enumerator7.MoveNext())
				{
					JSONClass jSONClass = (JSONClass)enumerator7.Current;
					string uid = jSONClass["id"];
					Atom atomByUid = GetAtomByUid(uid);
					if (atomByUid != null)
					{
						atomByUid.PreRestore();
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = (disposable = enumerator7 as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			IncrementLoadingSlider();
			Physics.autoSimulation = false;
			IEnumerator enumerator8 = jatoms.GetEnumerator();
			try
			{
				while (enumerator8.MoveNext())
				{
					JSONClass jatom = (JSONClass)enumerator8.Current;
					string auid = jatom["id"];
					string type = jatom["type"];
					UpdateLoadingStatus("Loading Atom " + auid);
					Atom a = GetAtomByUid(auid);
					if (a == null)
					{
						yield return StartCoroutine(AddAtomByType(type, auid));
						a = GetAtomByUid(auid);
						if (a != null)
						{
							a.ResetSimulation(loadFlag);
						}
					}
					else if (a.type != type)
					{
						Error("Atom " + a.name + " already exists, but uses different type " + a.type + " compared to requested " + type);
					}
					if (a != null)
					{
						a.SetOn(true);
					}
					IncrementLoadingSlider();
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable3 = (disposable = enumerator8 as IDisposable);
				if (disposable != null)
				{
					disposable3.Dispose();
				}
			}
			UpdateLoadingStatus("Restoring atom contents. Note large save files could take a while...");
			yield return null;
			Physics.Simulate(0.01f);
			yield return null;
			IEnumerator enumerator9 = jatoms.GetEnumerator();
			try
			{
				while (enumerator9.MoveNext())
				{
					JSONClass jSONClass2 = (JSONClass)enumerator9.Current;
					string text = jSONClass2["id"];
					string text2 = jSONClass2["type"];
					Atom atomByUid2 = GetAtomByUid(text);
					if (atomByUid2 != null)
					{
						atomByUid2.RestoreTransform(jSONClass2);
					}
					else
					{
						Error("Failed to find atom " + text + " of type " + text2);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable4 = (disposable = enumerator9 as IDisposable);
				if (disposable != null)
				{
					disposable4.Dispose();
				}
			}
			IEnumerator enumerator10 = jatoms.GetEnumerator();
			try
			{
				while (enumerator10.MoveNext())
				{
					JSONClass jSONClass3 = (JSONClass)enumerator10.Current;
					string uid2 = jSONClass3["id"];
					Atom atomByUid3 = GetAtomByUid(uid2);
					if (atomByUid3 != null)
					{
						atomByUid3.RestoreParentAtom(jSONClass3);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable5 = (disposable = enumerator10 as IDisposable);
				if (disposable != null)
				{
					disposable5.Dispose();
				}
			}
			IEnumerator enumerator11 = jatoms.GetEnumerator();
			try
			{
				while (enumerator11.MoveNext())
				{
					JSONClass jSONClass4 = (JSONClass)enumerator11.Current;
					string text3 = jSONClass4["id"];
					Atom atomByUid4 = GetAtomByUid(text3);
					if (atomByUid4 != null)
					{
						UpdateLoadingStatus("Restoring atom " + text3);
						atomByUid4.Restore(jSONClass4);
					}
					else
					{
						Error("Could not find atom by uid " + text3);
					}
					IncrementLoadingSlider();
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable6 = (disposable = enumerator11 as IDisposable);
				if (disposable != null)
				{
					disposable6.Dispose();
				}
			}
			UpdateLoadingStatus("Post-Restore");
			IEnumerator enumerator12 = jatoms.GetEnumerator();
			try
			{
				while (enumerator12.MoveNext())
				{
					JSONClass jSONClass5 = (JSONClass)enumerator12.Current;
					string text4 = jSONClass5["id"];
					Atom atomByUid5 = GetAtomByUid(text4);
					if (atomByUid5 != null)
					{
						atomByUid5.LateRestore(jSONClass5);
					}
					else
					{
						Error("Could not find atom by uid " + text4);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable7 = (disposable = enumerator12 as IDisposable);
				if (disposable != null)
				{
					disposable7.Dispose();
				}
			}
			foreach (Atom atoms7 in atomsList)
			{
				atoms7.PostRestore();
			}
			while (CheckHoldLoad())
			{
				UpdateLoadingStatus("Waiting for async load from " + holdLoadCompleteFlags[0].Name);
				yield return null;
			}
			Physics.autoSimulation = true;
			SetSceneLoadPosition();
			IncrementLoadingSlider();
			yield return null;
			if (loadJson["headPossessedController"] != null)
			{
				FreeControllerV3 freeControllerV = FreeControllerNameToFreeController(loadJson["headPossessedController"]);
				if (freeControllerV != null)
				{
					HeadPossess(freeControllerV, true);
				}
			}
			if (loadJson["playerNavCollider"] != null)
			{
				string text5 = loadJson["playerNavCollider"];
				PlayerNavCollider value;
				if (pncMap.TryGetValue(text5, out value))
				{
					playerNavCollider = value;
					ConnectNavRigToPlayerNavCollider();
				}
				else
				{
					Error("Could not find playerNavCollider " + text5);
				}
			}
		}
		if (loadMerge)
		{
			foreach (Atom atoms8 in atomsList)
			{
				atoms8.Validate();
			}
		}
		for (int j = 0; j < 20; j++)
		{
			yield return null;
		}
		loadFlag.Raise();
		for (int k = 0; k < 5; k++)
		{
			yield return null;
		}
		_isLoading = false;
		SyncSortedAtomUIDs();
		SyncSortedAtomUIDsWithForceProducers();
		SyncSortedAtomUIDsWithForceReceivers();
		SyncSortedAtomUIDsWithFreeControllers();
		SyncSortedAtomUIDsWithRhythmControllers();
		SyncSortedAtomUIDsWithAudioSourceControls();
		SyncSortedAtomUIDsWithRigidbodies();
		SyncHiddenAtoms();
		SyncSelectAtomPopup();
		if (loadingUI != null)
		{
			for (int l = 0; l < 10; l++)
			{
				yield return null;
			}
			loadingUI.gameObject.SetActive(false);
			if (loadingUIAlt != null)
			{
				loadingUIAlt.gameObject.SetActive(false);
			}
			if (loadingGeometry != null)
			{
				loadingGeometry.gameObject.SetActive(false);
			}
		}
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.pauseGlow = false;
		}
		if (UIDisabled && !loadMerge)
		{
			HideMainHUD();
		}
		if (!loadMerge && mainHUDAttachPoint != null)
		{
			mainHUDAttachPoint.localPosition = mainHUDAttachPointStartingPosition;
			mainHUDAttachPoint.localRotation = mainHUDAttachPointStartingRotation;
		}
		SyncVisibility();
		hideWaitTransform = false;
		// Last stop of a load: everything the scene brought with it is on a renderer by now, so the
		// materials that no init hook ever sees can be moved onto this project's shaders here.
		MeshVR.VamShaderProvider.UseProjectShadersEverywhere();
		if (onSceneLoadedHandlers != null)
		{
			onSceneLoadedHandlers();
		}
	}

	private string ExtractZipFile(string archiveFilenameIn)
	{
		FileManager.AssertNotCalledFromPlugin();
		ZipFile zipFile = null;
		string result = null;
		bool flag = false;
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(archiveFilenameIn, true))
			{
				zipFile = new ZipFile(fileEntryStream.Stream);
				string directoryName = FileManager.GetDirectoryName(archiveFilenameIn);
				string fileName = Path.GetFileName(archiveFilenameIn);
				fileName = fileName.Replace(".zip", string.Empty);
				fileName = fileName.Replace(".vac", string.Empty);
				directoryName = directoryName + "/" + fileName;
				foreach (ZipEntry item in zipFile)
				{
					if (!item.IsFile)
					{
						continue;
					}
					if (item.Name.Contains("../") || item.Name.Contains("..\\"))
					{
						throw new Exception("Potentially malicious zip file " + archiveFilenameIn + " that contains .. paths detected. Aborting extraction");
					}
					string text = item.Name;
					byte[] buffer = new byte[4096];
					Stream inputStream = zipFile.GetInputStream(item);
					string text2 = Path.Combine(directoryName, text);
					string fileName2 = Path.GetFileName(text);
					if (text.EndsWith(".var"))
					{
						text2 = "AddonPackages/" + fileName2;
						string packageUidOrPath = fileName2.Replace(".var", string.Empty);
						if (File.Exists(text2) || FileManager.GetPackage(packageUidOrPath) != null)
						{
							continue;
						}
						flag = true;
					}
					else
					{
						string directoryName2 = Path.GetDirectoryName(text2);
						if (directoryName2.Length > 0)
						{
							FileManager.CreateDirectory(directoryName2);
						}
						if (fileName2 != "meta.json" && (text2.EndsWith(".vac") || text2.EndsWith(".json")))
						{
							result = text2;
						}
					}
					using (FileStream destination = File.Create(text2))
					{
						StreamUtils.Copy(inputStream, destination, buffer);
					}
				}
			}
			if (flag)
			{
				FileManager.Refresh();
			}
		}
		catch (Exception ex)
		{
			Error("Exception during zip file extract of " + archiveFilenameIn + ": " + ex);
		}
		finally
		{
			if (zipFile != null)
			{
				zipFile.IsStreamOwner = true;
				zipFile.Close();
			}
		}
		return result;
	}

	protected void LoadInternal(string saveName = "savefile", bool loadMerge = false, bool editMode = false)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!_isLoading)
		{
			try
			{
				if (saveName != null && saveName != string.Empty && FileManager.FileExists(saveName))
				{
					if (onStartScene)
					{
						lastLoadDir = FileManager.GetDirectoryName(savesDirResolved + startSceneName, true);
					}
					else
					{
						lastLoadDir = FileManager.GetDirectoryName(saveName, true);
					}
					onStartScene = false;
					if (editMode)
					{
						gameMode = GameMode.Edit;
					}
					else
					{
						gameMode = GameMode.Play;
					}
					UnityEngine.Debug.Log("Load " + saveName);
					if (!loadMerge)
					{
						ClearSelection();
						ClearPossess();
						ClearAllGrabbedControllers();
						DisconnectNavRigFromPlayerNavCollider();
						_isLoading = true;
						Atom[] array = new Atom[atoms.Count];
						atomsList.CopyTo(array, 0);
						foreach (Atom atom in array)
						{
							if (atom != null)
							{
								atom.PreRestore();
							}
						}
						RemoveNonStartingAtoms();
						_isLoading = false;
					}
					if (saveName.EndsWith(".zip"))
					{
						string text = ExtractZipFile(saveName);
						if (text != null)
						{
							saveName = text;
						}
					}
					if (saveName.EndsWith(".vac"))
					{
						string text2 = ExtractZipFile(saveName);
						ClearFileBrowsersCurrentPath();
						if (text2 != null && text2.EndsWith(".json"))
						{
							saveName = text2;
						}
					}
					if (saveName.EndsWith(".json") && FileManager.FileExists(saveName))
					{
						using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName, true))
						{
							string aJSON = fileEntryStreamReader.ReadToEnd();
							loadJson = JSON.Parse(aJSON);
						}
						loadedName = saveName;
						FileManager.SetLoadDirFromFilePath(saveName);
						_isLoading = true;
						StartCoroutine(LoadCo(false, loadMerge));
					}
					else
					{
						Error("json file " + saveName + " is missing");
					}
				}
				else
				{
					onStartScene = false;
				}
				return;
			}
			catch (Exception ex)
			{
				_isLoading = false;
				Error("Exception during load " + ex);
				return;
			}
		}
		UnityEngine.Debug.LogWarning("Already loading file " + loadedName + ". Can't load another until complete");
	}

	protected void LoadFromSceneWorldDialog(string saveName)
	{
		if (saveName != null && saveName != string.Empty)
		{
			LoadInternal(saveName);
		}
		else if (!loadSceneWorldDialogActivatedFromWorld)
		{
			DeactivateWorldUI();
		}
		loadSceneWorldDialogActivatedFromWorld = false;
	}

	protected void LoadFromTemplateWorldDialog(string saveName)
	{
		if (saveName != null && saveName != string.Empty)
		{
			LoadInternal(saveName, false, true);
		}
		else if (!loadTemplateWorldDialogActivatedFromWorld)
		{
			DeactivateWorldUI();
		}
		loadTemplateWorldDialogActivatedFromWorld = false;
	}

	protected void LoadFromSceneDialog(string saveName)
	{
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (UserPreferences.singleton != null && UserPreferences.singleton.confirmLoad && loadConfirmButton != null && loadConfirmPanel != null)
		{
			loadConfirmButton.onClick.RemoveAllListeners();
			loadConfirmButton.onClick.AddListener(delegate
			{
				loadConfirmPanel.gameObject.SetActive(false);
				Load(saveName);
			});
			loadConfirmPanel.gameObject.SetActive(true);
			if (loadConfirmPathText != null)
			{
				loadConfirmPathText.text = saveName;
			}
		}
		else
		{
			Load(saveName);
		}
	}

	protected void LoadForEditFromSceneDialog(string saveName)
	{
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (UserPreferences.singleton != null && UserPreferences.singleton.confirmLoad && loadConfirmButton != null && loadConfirmPanel != null)
		{
			loadConfirmButton.onClick.RemoveAllListeners();
			loadConfirmButton.onClick.AddListener(delegate
			{
				loadConfirmPanel.gameObject.SetActive(false);
				LoadForEdit(saveName);
			});
			loadConfirmPanel.gameObject.SetActive(true);
			if (loadConfirmPathText != null)
			{
				loadConfirmPathText.text = saveName;
			}
		}
		else
		{
			LoadForEdit(saveName);
		}
	}

	protected void LoadMergeFromSceneDialog(string saveName)
	{
		if (saveName == null || !(saveName != string.Empty))
		{
			return;
		}
		if (UserPreferences.singleton != null && UserPreferences.singleton.confirmLoad && loadConfirmButton != null && loadConfirmPanel != null)
		{
			loadConfirmButton.onClick.RemoveAllListeners();
			loadConfirmButton.onClick.AddListener(delegate
			{
				loadConfirmPanel.gameObject.SetActive(false);
				LoadMerge(saveName);
			});
			loadConfirmPanel.gameObject.SetActive(true);
			if (loadConfirmPathText != null)
			{
				loadConfirmPathText.text = saveName;
			}
		}
		else
		{
			LoadMerge(saveName);
		}
	}

	public void Load(string saveName = "savefile")
	{
		LoadInternal(saveName);
	}

	public void LoadRestoreUI(string saveName = "savefile")
	{
		LoadInternal(saveName);
		ShowMainHUDAuto();
	}

	public void LoadForEdit(string saveName = "savefile")
	{
		LoadInternal(saveName, false, true);
		ShowMainHUDAuto();
	}

	public void LoadMerge(string saveName = "savefile")
	{
		LoadInternal(saveName, true);
	}

	public void LoadFromJSONEmbed(JSONEmbed je, bool loadMerge = false, bool editMode = false)
	{
		if (Application.isPlaying)
		{
			onStartScene = false;
			if (editMode)
			{
				gameMode = GameMode.Edit;
			}
			else
			{
				gameMode = GameMode.Play;
			}
			loadJson = JSON.Parse(je.jsonStore);
			loadedName = je.name;
			FileManager.SetLoadDir(string.Empty);
			_isLoading = true;
			StartCoroutine(LoadCo(false, loadMerge));
		}
	}

	protected void IncrementLoadingSlider()
	{
		if (loadingProgressSlider != null)
		{
			loadingProgressSlider.value += 1f;
		}
		if (loadingProgressSliderAlt != null)
		{
			loadingProgressSliderAlt.value += 1f;
		}
	}

	protected void UpdateLoadingStatus(string txt)
	{
		if (loadingTextStatus != null)
		{
			loadingTextStatus.text = txt;
		}
		if (loadingTextStatusAlt != null)
		{
			loadingTextStatusAlt.text = txt;
		}
	}

	protected void RemoveNonStartingAtoms()
	{
		if (startingAtoms == null)
		{
			return;
		}
		HashSet<Atom> hashSet = new HashSet<Atom>();
		foreach (Atom startingAtom in startingAtoms)
		{
			if (startingAtom != null)
			{
				hashSet.Add(startingAtom);
			}
		}
		startingAtoms = hashSet;
		List<Atom> list = new List<Atom>();
		foreach (Atom atoms in atomsList)
		{
			if (!startingAtoms.Contains(atoms))
			{
				list.Add(atoms);
			}
		}
		foreach (Atom item in list)
		{
			RemoveAtom(item, false);
		}
		atomsList = startingAtoms.ToList();
	}

	protected void LoadDialog(string lastPath, bool resetFileFormat = true)
	{
		string text = savesDirResolved + "scene";
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, true, false, true, true);
		if (FileManager.DirectoryExists("Saves/Downloads"))
		{
			ShortCut shortCut = new ShortCut();
			shortCut.displayName = "Saves\\Downloads";
			shortCut.path = "Saves\\Downloads";
			shortCutsForDirectory.Insert(1, shortCut);
		}
		fileBrowserUI.SetShortCuts(shortCutsForDirectory, true);
		string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastPath);
		if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
		{
			text = suggestedBrowserDirectoryFromDirectoryPath;
		}
		if (resetFileFormat)
		{
			fileBrowserUI.fileFormat = "json|vac|zip";
		}
		fileBrowserUI.defaultPath = text;
		activeUI = ActiveUI.None;
		fileBrowserUI.SetTextEntry(false);
		fileBrowserUI.keepOpen = false;
	}

	public void LoadMergeSceneDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Scene For Merge");
		fileBrowserUI.Show(LoadMergeFromSceneDialog);
	}

	public void LoadSceneForEditDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Scene For Edit");
		fileBrowserUI.Show(LoadForEditFromSceneDialog);
	}

	public void LoadSceneDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Scene To Load");
		fileBrowserUI.Show(LoadFromSceneDialog);
	}

	public void LoadSceneWorldDialog(bool fromWorld = true)
	{
		if (fileBrowserWorldUI != null)
		{
			loadSceneWorldDialogActivatedFromWorld = worldUIActivated;
			CloseHub();
			ActivateWorldUI();
			fileBrowserWorldUI.SetTextEntry(false);
			fileBrowserWorldUI.keepOpen = false;
			fileBrowserWorldUI.hideExtension = true;
			fileBrowserWorldUI.SetTitle("Select Scene To Load");
			if (loadSceneButtonDisabled)
			{
				string text = demoScenesDir;
				fileBrowserWorldUI.defaultPath = text;
				fileBrowserWorldUI.GotoDirectory(text, null, true);
			}
			else
			{
				string text2 = "Saves/scene";
				fileBrowserWorldUI.defaultPath = text2;
				fileBrowserWorldUI.GotoDirectory(text2, null, true, true);
			}
			fileBrowserWorldUI.Show(LoadFromSceneWorldDialog, false);
		}
	}

	public void LoadSceneWorldDialogWithPath(string path = "Saves/scene")
	{
		if (fileBrowserWorldUI != null && !loadSceneButtonDisabled)
		{
			if (FileManager.IsSecureReadPath(path))
			{
				loadSceneWorldDialogActivatedFromWorld = worldUIActivated;
				CloseHub();
				ActivateWorldUI();
				fileBrowserWorldUI.SetTextEntry(false);
				fileBrowserWorldUI.keepOpen = false;
				fileBrowserWorldUI.hideExtension = true;
				fileBrowserWorldUI.SetTitle("Select Scene To Load");
				fileBrowserWorldUI.defaultPath = path;
				fileBrowserWorldUI.GotoDirectory(path, null, true, true);
				fileBrowserWorldUI.Show(LoadFromSceneWorldDialog, false);
			}
			else
			{
				Error("Attempted to use LoadSceneWorldDialogWithPath on a path that is not inside game directory");
			}
		}
	}

	public void LoadTemplateWorldDialog(bool fromWorld = true)
	{
		if (templatesFileBrowserWorldUI != null && !advancedSceneEditDisabled)
		{
			loadTemplateWorldDialogActivatedFromWorld = worldUIActivated;
			CloseHub();
			ActivateWorldUI();
			templatesFileBrowserWorldUI.defaultPath = "Saves/scene";
			templatesFileBrowserWorldUI.SetTextEntry(false);
			templatesFileBrowserWorldUI.keepOpen = false;
			templatesFileBrowserWorldUI.hideExtension = true;
			templatesFileBrowserWorldUI.SetTitle("Select Template Scene To Load");
			templatesFileBrowserWorldUI.GotoDirectory("Saves/scene", null, true, true);
			templatesFileBrowserWorldUI.Show(LoadFromTemplateWorldDialog, false);
		}
	}

	public void CloseTemplateWorldDialog()
	{
	}

	public void OpenWizard()
	{
		if (wizardWorldUI != null)
		{
			ActivateWorldUI();
			wizardWorldUI.gameObject.SetActive(true);
		}
	}

	public void OpenHub()
	{
		if (hubBrowser != null)
		{
			hubOpenedFromWorld = false;
			hubBrowser.Show();
		}
	}

	public void OpenHubFromWorldUI()
	{
		if (hubBrowser != null)
		{
			hubOpenedFromWorld = true;
			hubBrowser.Show();
		}
	}

	public void CloseHub()
	{
		if (hubBrowser != null)
		{
			hubBrowser.Hide();
		}
		if (!hubOpenedFromWorld)
		{
			DeactivateWorldUI();
		}
		else
		{
			hubOpenedFromWorld = false;
		}
	}

	public void HardReset()
	{
		UnregisterAllPrefabsFromAtoms();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	protected void InitAtomPool()
	{
		typeToAtomPool = new Dictionary<string, List<Atom>>();
		if (atomPoolContainer == null)
		{
			GameObject gameObject = new GameObject("AtomPool");
			atomPoolContainer = gameObject.transform;
		}
		if (!(atomPoolContainer != null))
		{
			return;
		}
		Atom[] componentsInChildren = atomPoolContainer.GetComponentsInChildren<Atom>(true);
		foreach (Atom atom in componentsInChildren)
		{
			atom.inPool = true;
			List<Atom> value;
			if (!typeToAtomPool.TryGetValue(atom.type, out value))
			{
				value = new List<Atom>();
				typeToAtomPool.Add(atom.type, value);
			}
			value.Add(atom);
		}
	}

	protected Atom GetAtomOfTypeFromPool(string atomType)
	{
		List<Atom> value;
		if (typeToAtomPool.TryGetValue(atomType, out value))
		{
			foreach (Atom item in value)
			{
				if (item.inPool)
				{
					item.inPool = false;
					item.gameObject.SetActive(true);
					return item;
				}
			}
		}
		return null;
	}

	protected void PutAtomBackInPool(Atom a)
	{
		a.transform.SetParent(atomPoolContainer);
		a.PrepareToPutBackInPool();
		a.gameObject.SetActive(false);
		a.inPool = true;
		List<Atom> value;
		if (!typeToAtomPool.TryGetValue(a.type, out value))
		{
			value = new List<Atom>();
			typeToAtomPool.Add(a.type, value);
		}
		bool flag = false;
		foreach (Atom item in value)
		{
			if (item == a)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			value.Add(a);
		}
	}

	public void ClearAtomPool()
	{
		foreach (KeyValuePair<string, List<Atom>> item in typeToAtomPool)
		{
			List<Atom> list = new List<Atom>();
			foreach (Atom item2 in item.Value)
			{
				if (item2.inPool)
				{
					list.Add(item2);
				}
			}
			foreach (Atom item3 in list)
			{
				item.Value.Remove(item3);
				UnityEngine.Object.Destroy(item3.gameObject);
				AtomAsset value;
				if (item3.loadedFromBundle && item3.type != null && item3.type != string.Empty && atomAssetByType.TryGetValue(item3.type, out value))
				{
					UnregisterPrefab(value.assetBundleName, value.assetName);
				}
			}
		}
	}

	protected static void OpenURLDirect(string url)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		Uri result;
		if (Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
		{
			Application.OpenURL(url);
		}
		else
		{
			LogError("OpenURL only supports http/https URLs");
		}
	}

	public static void OpenURL(string url)
	{
		string pluginHashDisplayName = FileManager.GetPluginHashDisplayName();
		Uri result;
		if (Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
		{
			if (pluginHashDisplayName != null)
			{
				FileManager.ConfirmPluginActionWithUser("open url " + url, delegate
				{
					OpenURLDirect(url);
				}, null);
			}
			else
			{
				OpenURLDirect(url);
			}
		}
		else if (pluginHashDisplayName != null)
		{
			LogError("Plugin " + pluginHashDisplayName + " tried to use OpenURL in a way that is restricted. URL: " + url);
		}
		else
		{
			LogError("OpenURL only supports http/https URLs");
		}
	}

	public void OpenFolderInExplorer(string path)
	{
		if (MonitorRigActive && path != null && FileManager.DirectoryExists(path, true))
		{
			string fullPath = Path.GetFullPath(path);
			Process.Start("explorer", fullPath);
		}
	}

	public string NormalizePath(string path)
	{
		return FileManager.NormalizePath(path);
	}

	public string NormalizeMediaPath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastMediaDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeScenePath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastScenePathDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeDirectoryPath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastBrowseDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeLoadPath(string path)
	{
		string text = path;
		if (path != string.Empty && path != "NULL")
		{
			text = FileManager.NormalizeLoadPath(path);
			if (pathMigrationMappings != null && !text.StartsWith("http") && !File.Exists(text))
			{
				foreach (KeyValuePair<string, string> pathMigrationMapping in pathMigrationMappings)
				{
					string key = pathMigrationMapping.Key;
					string pattern = "^" + key;
					if (Regex.IsMatch(text, pattern))
					{
						string value = pathMigrationMapping.Value;
						string text2 = Regex.Replace(text, pattern, value);
						text = text2;
					}
				}
			}
		}
		return text;
	}

	public void PushOverrideLoadDirFromFilePath(string path)
	{
		FileManager.PushLoadDirFromFilePath(path, true);
	}

	public void PopOverrideLoadDir()
	{
		FileManager.PopLoadDir();
	}

	public void SetLoadDirFromFilePath(string path)
	{
		FileManager.SetLoadDirFromFilePath(path, true);
	}

	public void SetSaveDirFromFilePath(string path)
	{
		FileManager.SetSaveDirFromFilePath(path);
	}

	public void SetNullSaveDir()
	{
		FileManager.SetNullSaveDir();
	}

	private bool BuildObsoleteDirectoriesList()
	{
		directoriesToRemove = new List<string>();
		bool flag = false;
		if (obsoletePathsText != null)
		{
			obsoletePathsText.text = string.Empty;
		}
		try
		{
			string[] array = obsoleteDirectories;
			foreach (string text in array)
			{
				if (FileManager.DirectoryExists(text))
				{
					directoriesToRemove.Add(text);
					flag = true;
					if (obsoletePathsText != null)
					{
						Text text2 = obsoletePathsText;
						text2.text = text2.text + "Found obsolete directory " + text + "\n";
					}
				}
			}
			if (obsoletePathsPanel != null)
			{
				obsoletePathsPanel.gameObject.SetActive(flag);
			}
		}
		catch (Exception ex)
		{
			Error("Exception during obsolete directory cleanup " + ex);
		}
		return flag;
	}

	public void CancelRemoveObsoleteFiles()
	{
		if (obsoletePathsPanel != null)
		{
			obsoletePathsPanel.gameObject.SetActive(false);
		}
		if (startSceneEnabled && (!showMainHUDOnStart || UIDisabled || _onStartupSkipStartScreen))
		{
			StartCoroutine(DelayStart());
		}
	}

	public void RemoveObsoleteDirectories()
	{
		try
		{
			foreach (string item in directoriesToRemove)
			{
				if (FileManager.DirectoryExists(item))
				{
					FileManager.DeleteDirectory(item, true);
				}
			}
		}
		catch (Exception ex)
		{
			Error("Exception during obsolete directory removal " + ex);
		}
		CancelRemoveObsoleteFiles();
	}

	protected void BuildMigrationMappings()
	{
		legacyDirectories = new List<string>();
		legacyDirectories.Add("Textures/");
		legacyDirectories.Add("Saves/Scripts/");
		legacyDirectories.Add("Saves/Assets/");
		legacyDirectories.Add("Import/morphs/");
		legacyDirectories.Add("Import/");
		pathMigrationMappings = new Dictionary<string, string>();
		pathMigrationMappings.Add("Textures/", "Custom/Atom/Person/Textures/");
		pathMigrationMappings.Add("Saves/Scripts/", "Custom/Scripts/");
		pathMigrationMappings.Add("Import/morphs/", "Custom/Atom/Person/Morphs/");
		pathMigrationMappings.Add("Saves/Assets/", "Custom/Assets/");
	}

	protected bool BuildFilesToMigrateMap()
	{
		filesToMigrateMap = new Dictionary<string, string>();
		bool flag = false;
		try
		{
			bool flag2 = false;
			StringBuilder stringBuilder = new StringBuilder(25000);
			StringBuilder stringBuilder2 = new StringBuilder(25000);
			StreamWriter streamWriter = new StreamWriter("migrate.log");
			streamWriter.WriteLine("Report:");
			foreach (KeyValuePair<string, string> pathMigrationMapping in pathMigrationMappings)
			{
				string key = pathMigrationMapping.Key;
				string value = pathMigrationMapping.Value;
				if (!FileManager.DirectoryExists(key))
				{
					continue;
				}
				string[] files = Directory.GetFiles(key, "*", SearchOption.AllDirectories);
				string pattern = "^" + key;
				string[] array = files;
				foreach (string text in array)
				{
					flag = true;
					string text2 = text;
					string text3 = Regex.Replace(text2, pattern, value);
					string value2 = text2;
					string value3 = text3;
					if (text2.Length > text3.Length)
					{
						value3 = text3.PadRight(text2.Length);
					}
					else if (text3.Length > text2.Length)
					{
						value2 = text2.PadRight(text3.Length);
					}
					streamWriter.WriteLine(text2 + " -> " + text3);
					if (stringBuilder.Length < 16000 && stringBuilder2.Length < 16000)
					{
						stringBuilder.AppendLine(value2);
						stringBuilder2.AppendLine(value3);
					}
					else
					{
						flag2 = true;
					}
					filesToMigrateMap.Add(text, text3);
				}
			}
			if (flag2)
			{
				stringBuilder.AppendLine("Truncated...too long to display. See migrate.log");
			}
			if (!flag)
			{
				streamWriter.WriteLine("No files found that need migrating");
			}
			streamWriter.Close();
			if (oldPathsText != null)
			{
				oldPathsText.text = stringBuilder.ToString();
			}
			if (newPathsText != null)
			{
				newPathsText.text = stringBuilder2.ToString();
			}
			if (flag)
			{
				HideMainHUD();
			}
			if (migratePathsPanel != null)
			{
				migratePathsPanel.gameObject.SetActive(flag);
			}
		}
		catch (Exception ex)
		{
			Error("Exception during search for migration file " + ex);
		}
		return flag;
	}

	public void CancelMigrateFiles()
	{
		if (migratePathsPanel != null)
		{
			migratePathsPanel.gameObject.SetActive(false);
		}
		if (startSceneEnabled && (!showMainHUDOnStart || UIDisabled || _onStartupSkipStartScreen))
		{
			StartCoroutine(DelayStart());
		}
	}

	protected void RemoveLegacyDirectories(StreamWriter log)
	{
		try
		{
			foreach (string legacyDirectory in legacyDirectories)
			{
				if (FileManager.DirectoryExists(legacyDirectory))
				{
					string[] files = Directory.GetFiles(legacyDirectory, "*", SearchOption.AllDirectories);
					if (files.Length == 0)
					{
						log?.WriteLine("Deleting directory " + legacyDirectory);
						UnityEngine.Debug.Log("Delete directory " + legacyDirectory);
						Directory.Delete(legacyDirectory, true);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Error("Exception during migrate directory removal " + ex);
		}
	}

	public void MigrateFilesInMigrateMap()
	{
		if (filesToMigrateMap != null)
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter("migrate.log", true);
				foreach (KeyValuePair<string, string> item in filesToMigrateMap)
				{
					string key = item.Key;
					string value = item.Value;
					streamWriter.WriteLine("Migrate file " + key + " to " + value + " ...");
					UnityEngine.Debug.Log("Migrate file " + key + " to " + value);
					string directoryName = Path.GetDirectoryName(value);
					try
					{
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						File.SetAttributes(key, FileAttributes.Normal);
						if (!File.Exists(value))
						{
							File.Move(key, value);
							streamWriter.WriteLine("  ...File moved");
						}
						else
						{
							streamWriter.WriteLine("  ...File already exists in new location. Just removing old file");
							File.Delete(key);
						}
					}
					catch (Exception ex)
					{
						Error("Exception during migrate of " + key + " :" + ex);
					}
				}
				RemoveLegacyDirectories(streamWriter);
				streamWriter.Close();
			}
			catch (Exception ex2)
			{
				Error("Exception during migrate copy " + ex2);
			}
		}
		CancelMigrateFiles();
	}

	private void ProcessSaveScreenshot(bool force = false)
	{
		if (GetRightSelect() || GetLeftSelect() || GetMouseSelect() || force)
		{
			if (screenshotCamera != null)
			{
				RenderTexture targetTexture = screenshotCamera.targetTexture;
				if (targetTexture != null)
				{
					try
					{
						Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.ARGB32, false);
						RenderTexture.active = targetTexture;
						texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
						texture2D.Apply();
						byte[] bytes = texture2D.EncodeToJPG();
						string text = savingName.Replace(".json", ".jpg");
						text = text.Replace(".vac", ".jpg");
						FileManager.WriteAllBytes(text, bytes);
						if (fileBrowserUI != null)
						{
							fileBrowserUI.ClearCacheImage(text);
						}
						if (screenShotCallback != null)
						{
							screenShotCallback(text);
							screenShotCallback = null;
						}
						UnityEngine.Object.Destroy(texture2D);
					}
					catch (Exception ex)
					{
						LogError("Exception during screenshot processing: " + ex.Message);
					}
				}
				screenshotCamera.enabled = false;
			}
			SelectModeOff();
			ShowMainHUDAuto();
		}
		if (GetCancel())
		{
			SelectModeOff();
			ShowMainHUDAuto();
		}
	}

	private void ProcessHiResScreenshot()
	{
		try
		{
			if ((GetRightSelect() || GetLeftSelect() || GetMouseSelect()) && hiResScreenshotCamera != null)
			{
				RenderTexture targetTexture = hiResScreenshotCamera.targetTexture;
				if (targetTexture != null)
				{
					Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.ARGB32, false);
					RenderTexture.active = targetTexture;
					texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
					texture2D.Apply();
					byte[] bytes = texture2D.EncodeToJPG(100);
					int num = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
					string text = savesDirResolved + "screenshots\\" + num + ".jpg";
					int num2 = text.LastIndexOf('\\');
					if (num2 >= 0)
					{
						string path = text.Substring(0, num2);
						FileManager.CreateDirectory(path);
					}
					FileManager.WriteAllBytes(text, bytes);
					if (SkyshopLightController.singleton != null)
					{
						SkyshopLightController.singleton.Flash();
					}
					UnityEngine.Object.Destroy(texture2D);
				}
			}
			if (GetCancel())
			{
				SelectModeOff();
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during process of screenshot: " + ex.Message);
		}
	}

	public void DoSaveScreenshot(string saveName, ScreenShotCallback callback = null)
	{
		if (screenshotCamera != null)
		{
			savingName = saveName;
			screenShotCallback = callback;
			if (screenshotPreview != null)
			{
				screenshotPreview.gameObject.SetActive(true);
			}
			ResetSelectionInstances();
			ClearSelectionHUDs();
			HideMainHUD();
			selectMode = SelectMode.SaveScreenshot;
			helpText = "Aim head and press select to take screenshot for save.";
			SyncVisibility();
			screenshotCamera.enabled = true;
			if (loResScreenShotCameraFOVSlider != null)
			{
				screenshotCamera.fieldOfView = loResScreenShotCameraFOVSlider.value;
			}
			else
			{
				screenshotCamera.fieldOfView = 40f;
			}
		}
	}

	public string[] GetFilesAtPath(string path, string pattern = null)
	{
		string[] result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.GetFiles(path, pattern, true);
		}
		else
		{
			Error("Attempted to use GetFilesAtPath on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public string[] GetDirectoriesAtPath(string path, string pattern = null)
	{
		string[] result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.GetDirectories(path, pattern, true);
		}
		else
		{
			Error("Attempted to use GetDirectoriesAtPath on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public string ReadFileIntoString(string path)
	{
		string result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.ReadAllText(path, true);
		}
		else
		{
			Error("Attempted to use ReadFileIntoString on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public void SaveStringIntoFile(string path, string contents)
	{
		if (!FileManager.IsSecurePluginWritePath(path))
		{
			Error("Attempted to use SaveStringIntoFile on a path that is not inside game directory Saves or Custom area. That is not allowed");
		}
		if (File.Exists(path))
		{
			if (!FileManager.IsPluginWritePathThatNeedsConfirm(path))
			{
				try
				{
					FileManager.WriteAllText(path, contents);
					return;
				}
				catch (Exception ex)
				{
					Error("Exception while saving string into file " + ex);
					return;
				}
			}
			FileManager.ConfirmPluginActionWithUser("save string into file " + path, delegate
			{
				try
				{
					FileManager.WriteAllText(path, contents);
				}
				catch (Exception ex2)
				{
					Error("Exception while saving string into file " + ex2);
				}
			}, null);
			return;
		}
		try
		{
			FileManager.WriteAllText(path, contents);
		}
		catch (Exception ex3)
		{
			Error("Exception while saving string into file " + ex3);
		}
	}

	private void SyncGameMode()
	{
		if (_gameMode == GameMode.Edit)
		{
			if (editModeToggle != null)
			{
				editModeToggle.isOn = true;
			}
			if (playModeToggle != null)
			{
				playModeToggle.isOn = false;
			}
			if (_activeUI == ActiveUI.SelectedOptions)
			{
				activeUI = ActiveUI.SelectedOptions;
			}
			if (editModeOnlyTransforms != null)
			{
				Transform[] array = editModeOnlyTransforms;
				foreach (Transform transform in array)
				{
					if (transform != null)
					{
						transform.gameObject.SetActive(true);
					}
				}
			}
			if (playModeOnlyTransforms != null)
			{
				Transform[] array2 = playModeOnlyTransforms;
				foreach (Transform transform2 in array2)
				{
					if (transform2 != null)
					{
						transform2.gameObject.SetActive(false);
					}
				}
			}
		}
		else
		{
			if (editModeToggle != null)
			{
				editModeToggle.isOn = false;
			}
			if (playModeToggle != null)
			{
				playModeToggle.isOn = true;
			}
			if (_activeUI == ActiveUI.SelectedOptions)
			{
				activeUI = ActiveUI.SelectedOptions;
			}
			if (editModeOnlyTransforms != null)
			{
				Transform[] array3 = editModeOnlyTransforms;
				foreach (Transform transform3 in array3)
				{
					if (transform3 != null)
					{
						transform3.gameObject.SetActive(false);
					}
				}
			}
			if (playModeOnlyTransforms != null)
			{
				Transform[] array4 = playModeOnlyTransforms;
				foreach (Transform transform4 in array4)
				{
					if (transform4 != null)
					{
						transform4.gameObject.SetActive(true);
					}
				}
			}
		}
		SyncAdvancedSceneEditModeTransforms();
		SyncVisibility();
	}

	public void PurgeImageCache()
	{
		if (ImageLoaderThreaded.singleton != null)
		{
			ImageLoaderThreaded.singleton.PurgeAllTextures();
		}
	}

	public void UnloadUnusedResources()
	{
		Resources.UnloadUnusedAssets();
	}

	public void GarbageCollect()
	{
		LogMessage("Memory usage before garbage collect " + GC.GetTotalMemory(false));
		GC.Collect();
		LogMessage("Memory usage  after garbage collect " + GC.GetTotalMemory(true));
	}

	public void ReportLoadedAssetBundles()
	{
		AssetBundleManager.ReportLoadedAssetBundles();
	}

	public void Quit()
	{
		Application.Quit();
	}

	public static void LogError(string err)
	{
		if (_singleton != null)
		{
			_singleton.Error(err);
		}
		else
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public static void LogError(string err, bool logToFile)
	{
		if (_singleton != null)
		{
			_singleton.Error(err, logToFile);
		}
		else
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public static void LogError(string err, bool logToFile, bool splash)
	{
		if (_singleton != null)
		{
			_singleton.Error(err, logToFile, splash);
		}
		else
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public static void LogMessage(string msg)
	{
		if (_singleton != null)
		{
			_singleton.Message(msg);
		}
		else
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	public static void LogMessage(string msg, bool logToFile)
	{
		if (_singleton != null)
		{
			_singleton.Message(msg, logToFile);
		}
		else
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	public static void LogMessage(string msg, bool logToFile, bool splash)
	{
		if (_singleton != null)
		{
			_singleton.Message(msg, logToFile, splash);
		}
		else
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	public static void AlertUser(string alert, UnityAction okCallback, DisplayUIChoice displayUIChoice = DisplayUIChoice.Auto)
	{
		if (_singleton != null)
		{
			_singleton.Alert(alert, okCallback, displayUIChoice);
		}
	}

	public static void AlertUser(string alert, UnityAction okCallback, UnityAction cancelCallback, DisplayUIChoice displayUIChoice = DisplayUIChoice.Auto)
	{
		if (_singleton != null)
		{
			_singleton.Alert(alert, okCallback, cancelCallback, displayUIChoice);
		}
	}

	public void OpenErrorLogPanel()
	{
		if (!_mainHUDVisible)
		{
			ShowMainHUDAuto();
		}
		if (errorLogPanel != null)
		{
			errorLogPanel.gameObject.SetActive(true);
		}
	}

	public void CloseErrorLogPanel()
	{
		if (errorLogPanel != null)
		{
			errorLogPanel.gameObject.SetActive(false);
		}
	}

	public void ClearErrors()
	{
		errorCount = 0;
		errorLog = string.Empty;
		errorLogDirty = true;
		CloseErrorSplash();
	}

	protected void SyncAllErrorsInputFields()
	{
		if (errorLogDirty)
		{
			if (allErrorsInputField != null)
			{
				allErrorsInputField.text = errorLog;
			}
			if (allErrorsInputField2 != null)
			{
				allErrorsInputField2.text = errorLog;
			}
			errorLogDirty = false;
		}
	}

	public void Error(string err, bool logToFile = true, bool splash = true)
	{
		errorCount++;
		if (splash)
		{
			if (!worldUIActivated && !_mainHUDVisible && _openMainHUDOnError)
			{
				ShowMainHUDAuto();
			}
			errorSplashTimeRemaining = errorSplashTime;
		}
		if (errorLog != null && errorLog.Length > maxLength)
		{
			errorLog = errorLog.Substring(0, maxLength / 2);
			errorLog += "\n\n<Truncated>\n";
		}
		errorLog = errorLog + "\n!> " + err;
		errorLogDirty = true;
		if (logToFile)
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public void OpenMessageLogPanel()
	{
		if (!_mainHUDVisible)
		{
			ShowMainHUDAuto();
		}
		if (msgLogPanel != null)
		{
			msgLogPanel.gameObject.SetActive(true);
		}
	}

	public void CloseMessageLogPanel()
	{
		if (msgLogPanel != null)
		{
			msgLogPanel.gameObject.SetActive(false);
		}
	}

	protected void SyncAllMessagesInputFields()
	{
		if (msgLogDirty)
		{
			if (allMessagesInputField != null)
			{
				allMessagesInputField.text = msgLog;
			}
			if (allMessagesInputField2 != null)
			{
				allMessagesInputField2.text = msgLog;
			}
			msgLogDirty = false;
		}
	}

	public void ClearMessages()
	{
		msgCount = 0;
		msgLog = string.Empty;
		msgLogDirty = true;
		CloseMessageSplash();
	}

	public void Message(string msg, bool logToFile = true, bool splash = true)
	{
		msgCount++;
		if (splash)
		{
			msgSplashTimeRemaining = msgSplashTime;
		}
		if (msgLog != null && msgLog.Length > maxLength)
		{
			msgLog = msgLog.Substring(0, maxLength / 2);
			msgLog += "\n\n<Truncated>\n";
		}
		msgLog = msgLog + "\n" + msg;
		msgLogDirty = true;
		if (logToFile)
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	public void CloseMessageSplash()
	{
		if (msgSplashTimeRemaining > 0f)
		{
			msgSplashTimeRemaining = 0.001f;
		}
	}

	public void CloseErrorSplash()
	{
		if (errorSplashTimeRemaining > 0f)
		{
			errorSplashTimeRemaining = 0.001f;
		}
	}

	protected void CheckMessageAndErrorQueue()
	{
		if (!worldUIActivated)
		{
			if (hasPendingErrorSplash)
			{
				if (!_mainHUDVisible)
				{
					ShowMainHUDAuto();
				}
				hasPendingErrorSplash = false;
			}
			if (errorSplashTimeRemaining > 0f)
			{
				errorSplashTimeRemaining -= Time.unscaledDeltaTime;
				if (errorSplashTimeRemaining <= 0f)
				{
					if (errorSplashTransform != null)
					{
						errorSplashTransform.gameObject.SetActive(false);
					}
				}
				else if (errorSplashTransform != null)
				{
					errorSplashTransform.gameObject.SetActive(true);
				}
			}
			else
			{
				if (!(msgSplashTimeRemaining > 0f))
				{
					return;
				}
				msgSplashTimeRemaining -= Time.unscaledDeltaTime;
				if (msgSplashTimeRemaining <= 0f)
				{
					if (msgSplashTransform != null)
					{
						msgSplashTransform.gameObject.SetActive(false);
					}
				}
				else if (msgSplashTransform != null)
				{
					msgSplashTransform.gameObject.SetActive(true);
				}
			}
		}
		else if (errorSplashTimeRemaining > 0f)
		{
			hasPendingErrorSplash = true;
		}
	}

	protected Transform SyncToDisplayChoice(DisplayUIChoice displayUIChoice)
	{
		Transform result = null;
		switch (displayUIChoice)
		{
		case DisplayUIChoice.Auto:
			if (worldUIActivated && !_mainHUDVisible)
			{
				result = worldAlertRoot;
				break;
			}
			result = normalAlertRoot;
			if (!_mainHUDVisible)
			{
				ShowMainHUDAuto();
			}
			break;
		case DisplayUIChoice.Normal:
			result = normalAlertRoot;
			DeactivateWorldUI();
			if (!_mainHUDVisible)
			{
				ShowMainHUDAuto();
			}
			break;
		case DisplayUIChoice.World:
			result = worldAlertRoot;
			ActivateWorldUI();
			break;
		}
		return result;
	}

	public void Alert(string alertMessage, UnityAction okAlertCallback, UnityAction cancelAlertCallback, DisplayUIChoice displayUIChoice = DisplayUIChoice.Auto)
	{
		if (!(okAndCancelAlertPrefab != null))
		{
			return;
		}
		Transform parent = SyncToDisplayChoice(displayUIChoice);
		GameObject gameObject = UnityEngine.Object.Instantiate(okAndCancelAlertPrefab, parent, false);
		if (gameObject != null)
		{
			AlertUI component = gameObject.GetComponent<AlertUI>();
			if (component != null)
			{
				component.SetText(alertMessage);
				component.SetOKButton(okAlertCallback);
				component.SetCancelButton(cancelAlertCallback);
			}
		}
	}

	public void Alert(string alertMessage, UnityAction okAlertCallback, DisplayUIChoice displayUIChoice = DisplayUIChoice.Auto)
	{
		if (!(okAlertPrefab != null))
		{
			return;
		}
		Transform parent = SyncToDisplayChoice(displayUIChoice);
		GameObject gameObject = UnityEngine.Object.Instantiate(okAlertPrefab, parent, false);
		if (gameObject != null)
		{
			AlertUI component = gameObject.GetComponent<AlertUI>();
			if (component != null)
			{
				component.SetText(alertMessage);
				component.SetOKButton(okAlertCallback);
			}
		}
	}

	public bool IsSimulationResetting()
	{
		return _resetSimulation;
	}

	protected void CheckResumeSimulation()
	{
		if (pauseFrames >= 0)
		{
			pauseFrames--;
			if (pauseFrames < 0 && resetSimulationTimerFlag != null)
			{
				resetSimulationTimerFlag.Raise();
			}
		}
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			if (removeSimulationFlags == null)
			{
				removeSimulationFlags = new List<AsyncFlag>();
			}
			else
			{
				removeSimulationFlags.Clear();
			}
			Text[] array = waitReasonTexts;
			foreach (Text text in array)
			{
				text.text = string.Empty;
			}
			int num = 0;
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				if (num < waitReasonTexts.Length)
				{
					waitReasonTexts[num].text = waitResumeSimulationFlag.Name;
					num++;
				}
				if (waitResumeSimulationFlag.Raised)
				{
					removeSimulationFlags.Add(waitResumeSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag removeSimulationFlag in removeSimulationFlags)
			{
				waitResumeSimulationFlags.Remove(removeSimulationFlag);
			}
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			readyToResumeSimulation = false;
			if (waitTransform != null && !hideWaitTransform && !hiddenReset)
			{
				waitTransform.gameObject.SetActive(true);
			}
			resetSimulation = true;
		}
		else if (flag)
		{
			readyToResumeSimulation = true;
		}
		else if (readyToResumeSimulation)
		{
			if (waitTransform != null)
			{
				waitTransform.gameObject.SetActive(false);
			}
			readyToResumeSimulation = false;
			resetSimulation = false;
			hiddenReset = false;
		}
	}

	public void PauseSimulation(AsyncFlag af, bool hidden = false)
	{
		ResetSimulation(af, hidden);
	}

	public void ResetSimulation(AsyncFlag af, bool hidden = false)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		if (hidden)
		{
			if (!resetSimulation)
			{
				hiddenReset = true;
			}
		}
		else
		{
			hiddenReset = false;
		}
		if (waitResumeSimulationFlags.Contains(af))
		{
			return;
		}
		resetSimulation = true;
		waitResumeSimulationFlags.Add(af);
		if (atoms == null)
		{
			return;
		}
		foreach (Atom atoms in atomsList)
		{
			atoms.ResetSimulation(af);
		}
	}

	public void PauseSimulation(int numFrames, string pauseName)
	{
		ResetSimulation(numFrames, pauseName);
	}

	public void PauseSimulation(int numFrames, string pauseName, bool hidden)
	{
		ResetSimulation(numFrames, pauseName, hidden);
	}

	public void ResetSimulation(int numFrames, string pauseName, bool hidden = false)
	{
		if (pauseFrames < numFrames)
		{
			pauseFrames = numFrames;
			if (waitResumeSimulationFlags == null)
			{
				waitResumeSimulationFlags = new List<AsyncFlag>();
			}
			if (resetSimulationTimerFlag == null)
			{
				resetSimulationTimerFlag = new AsyncFlag(pauseName);
			}
			else
			{
				resetSimulationTimerFlag.Name = pauseName;
			}
			if (!waitResumeSimulationFlags.Contains(resetSimulationTimerFlag))
			{
				resetSimulationTimerFlag.Lower();
				ResetSimulation(resetSimulationTimerFlag, hidden);
			}
		}
	}

	protected void CheckResumeAutoSimulation()
	{
		if (waitResumeAutoSimulationFlags == null)
		{
			waitResumeAutoSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeAutoSimulationFlags.Count > 0)
		{
			if (removeAutoSimulationFlags == null)
			{
				removeAutoSimulationFlags = new List<AsyncFlag>();
			}
			else
			{
				removeAutoSimulationFlags.Clear();
			}
			foreach (AsyncFlag waitResumeAutoSimulationFlag in waitResumeAutoSimulationFlags)
			{
				if (waitResumeAutoSimulationFlag.Raised)
				{
					removeAutoSimulationFlags.Add(waitResumeAutoSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag removeAutoSimulationFlag in removeAutoSimulationFlags)
			{
				waitResumeAutoSimulationFlags.Remove(removeAutoSimulationFlag);
			}
		}
		if (waitResumeAutoSimulationFlags.Count > 0)
		{
			autoSimulation = false;
		}
		else if (flag)
		{
			autoSimulation = true;
		}
	}

	public void PauseAutoSimulation(AsyncFlag af)
	{
		if (waitResumeAutoSimulationFlags == null)
		{
			waitResumeAutoSimulationFlags = new List<AsyncFlag>();
		}
		if (!waitResumeAutoSimulationFlags.Contains(af))
		{
			autoSimulation = false;
			waitResumeAutoSimulationFlags.Add(af);
		}
	}

	protected void SetPauseAutoSimulation(bool b)
	{
		pauseAutoSimulation = b;
	}

	protected void CheckResumeRender()
	{
		if (waitResumeRenderFlags == null)
		{
			waitResumeRenderFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeRenderFlags.Count > 0)
		{
			if (removeRenderFlags == null)
			{
				removeRenderFlags = new List<AsyncFlag>();
			}
			else
			{
				removeRenderFlags.Clear();
			}
			foreach (AsyncFlag waitResumeRenderFlag in waitResumeRenderFlags)
			{
				if (waitResumeRenderFlag.Raised)
				{
					removeRenderFlags.Add(waitResumeRenderFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag removeRenderFlag in removeRenderFlags)
			{
				waitResumeRenderFlags.Remove(removeRenderFlag);
			}
		}
		if (waitResumeRenderFlags.Count > 0)
		{
			render = false;
		}
		else if (flag)
		{
			render = true;
		}
	}

	public void PauseRender(AsyncFlag af)
	{
		if (waitResumeRenderFlags == null)
		{
			waitResumeRenderFlags = new List<AsyncFlag>();
		}
		if (!waitResumeRenderFlags.Contains(af))
		{
			render = false;
			waitResumeRenderFlags.Add(af);
		}
	}

	protected void SetPauseRender(bool b)
	{
		pauseRender = b;
	}

	protected bool CheckHoldLoad()
	{
		if (holdLoadCompleteFlags == null)
		{
			holdLoadCompleteFlags = new List<AsyncFlag>();
		}
		if (holdLoadCompleteFlags.Count > 0)
		{
			if (removeHoldFlags == null)
			{
				removeHoldFlags = new List<AsyncFlag>();
			}
			else
			{
				removeHoldFlags.Clear();
			}
			foreach (AsyncFlag holdLoadCompleteFlag in holdLoadCompleteFlags)
			{
				if (holdLoadCompleteFlag.Raised)
				{
					removeHoldFlags.Add(holdLoadCompleteFlag);
				}
			}
			foreach (AsyncFlag removeHoldFlag in removeHoldFlags)
			{
				holdLoadCompleteFlags.Remove(removeHoldFlag);
			}
		}
		if (holdLoadCompleteFlags.Count > 0)
		{
			return true;
		}
		return false;
	}

	public void HoldLoadComplete(AsyncFlag af)
	{
		if (holdLoadCompleteFlags == null)
		{
			holdLoadCompleteFlags = new List<AsyncFlag>();
		}
		holdLoadCompleteFlags.Add(af);
	}

	public void SetLoadFlag()
	{
		if (loadFlag != null)
		{
			loadFlag.Raise();
		}
	}

	protected bool CheckLoadingIcon()
	{
		if (loadingIconFlags == null)
		{
			loadingIconFlags = new List<AsyncFlag>();
		}
		if (loadingIconFlags.Count > 0)
		{
			if (removeLoadFlags == null)
			{
				removeLoadFlags = new List<AsyncFlag>();
			}
			else
			{
				removeLoadFlags.Clear();
			}
			foreach (AsyncFlag loadingIconFlag in loadingIconFlags)
			{
				if (loadingIconFlag.Raised)
				{
					removeLoadFlags.Add(loadingIconFlag);
				}
			}
			foreach (AsyncFlag removeLoadFlag in removeLoadFlags)
			{
				loadingIconFlags.Remove(removeLoadFlag);
			}
		}
		if (loadingIconFlags.Count > 0)
		{
			if (loadingIcon != null)
			{
				loadingIcon.gameObject.SetActive(true);
			}
			return true;
		}
		if (loadingIcon != null)
		{
			loadingIcon.gameObject.SetActive(false);
		}
		return false;
	}

	public void SetLoadingIconFlag(AsyncFlag af)
	{
		if (loadingIconFlags == null)
		{
			loadingIconFlags = new List<AsyncFlag>();
		}
		loadingIconFlags.Add(af);
	}

	protected void SyncAutoFreezeAnimationOnSwitchToEditMode(bool b)
	{
		autoFreezeAnimationOnSwitchToEditMode = b;
	}

	protected void SyncFreezeAnimation()
	{
		if (allAnimators == null)
		{
			return;
		}
		foreach (Animator allAnimator in allAnimators)
		{
			allAnimator.enabled = !_freezeAnimation;
		}
	}

	public void SetFreezeAnimation(bool freeze)
	{
		if (_freezeAnimation != freeze)
		{
			_freezeAnimation = freeze;
			SyncFreezeAnimation();
			if (freezeAnimationToggle != null)
			{
				freezeAnimationToggle.isOn = _freezeAnimation;
			}
			if (freezeAnimationToggleAlt != null)
			{
				freezeAnimationToggleAlt.isOn = _freezeAnimation;
			}
		}
	}

	public void SyncVersionText()
	{
		string text = "VaM: ";
		text = ((!foundVersion) ? (text + version) : (text + resolvedVersion));
		if (vamXIntalled)
		{
			text = text + " vamX: 1." + vamXVersion;
		}
		if (versionText != null)
		{
			versionText.text = text;
		}
		if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.versionText != null)
		{
			GlobalSceneOptions.singleton.versionText.text = text;
		}
	}

	protected void SyncVersion()
	{
		SettingsManager.APP_PATH = Path.GetFullPath(".");
		SettingsManager.RegeneratePaths();
		string vERSION_FILE_LOCAL_PATH = SettingsManager.VERSION_FILE_LOCAL_PATH;
		foundVersion = false;
		if (File.Exists(vERSION_FILE_LOCAL_PATH))
		{
			string cipherText = File.ReadAllText(SettingsManager.PATCH_VERSION_PATH).Replace("\n", string.Empty).Replace("\r", string.Empty);
			resolvedVersion = MHLab.PATCH.Utilities.Rijndael.Decrypt(cipherText, SettingsManager.PATCH_VERSION_ENCRYPTION_PASSWORD);
			foundVersion = true;
		}
		else if (Application.isEditor)
		{
			resolvedVersion = editorMimicVersion;
			foundVersion = true;
		}
		else
		{
			resolvedVersion = editorMimicVersion;
		}
		resolvedVersionDefines = null;
		string[] array = resolvedVersion.Split('.');
		if (array.Length == 4)
		{
			resolvedVersionDefines = new List<string>();
			string text = array[0] + "_" + array[1];
			string item = "VAM_" + text;
			resolvedVersionDefines.Add(item);
			item = "VAM_" + text + "_" + array[2];
			resolvedVersionDefines.Add(item);
			item = "VAM_" + text + "_" + array[2] + "_" + array[3];
			resolvedVersionDefines.Add(item);
			int result;
			if (int.TryParse(array[1], out result))
			{
				for (int i = oldestMajorVersion; i <= result; i++)
				{
					item = "VAM_GT_" + array[0] + "_" + i;
					resolvedVersionDefines.Add(item);
				}
			}
			if (specificMilestoneVersionDefines != null)
			{
				string[] array2 = specificMilestoneVersionDefines;
				foreach (string text2 in array2)
				{
					item = "VAM_GT_" + text2;
					resolvedVersionDefines.Add(item);
				}
			}
		}
		SyncVersionText();
	}

	public string GetVersion()
	{
		if (resolvedVersion != null)
		{
			return resolvedVersion;
		}
		return version;
	}

	public IEnumerable<string> GetResolvedVersionDefines()
	{
		return resolvedVersionDefines;
	}

	private void SyncSelectAtomPopup()
	{
		if (!(selectAtomPopup != null) || _isLoading || _pauseSyncAtomLists)
		{
			return;
		}
		string text = string.Empty;
		if (selectedController != null && selectedController.containingAtom != null)
		{
			text = selectedController.containingAtom.uid;
		}
		bool flag = false;
		List<string> list = GetAtomUIDsWithFreeControllers();
		selectAtomPopup.currentValueNoCallback = "None";
		selectAtomPopup.numPopupValues = list.Count + 1;
		selectAtomPopup.setPopupValue(0, "None");
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == text)
			{
				flag = true;
				selectAtomPopup.currentValueNoCallback = text;
			}
			selectAtomPopup.setPopupValue(i + 1, list[i]);
		}
		if (!flag)
		{
			selectAtomPopup.currentValue = "None";
			SyncControllerPopup("None");
		}
	}

	public void SelectLastAddedAtom()
	{
		if (visibleAtomUIDsWithFreeControllers != null && visibleAtomUIDsWithFreeControllers.Count > 0 && selectAtomPopup != null)
		{
			selectAtomPopup.currentValue = visibleAtomUIDsWithFreeControllers[visibleAtomUIDsWithFreeControllers.Count - 1];
		}
	}

	public void CycleSelectAtomOfType(string type)
	{
		if (visibleAtomUIDsWithFreeControllers == null)
		{
			return;
		}
		List<string> list = visibleAtomUIDsWithFreeControllers;
		string text = null;
		if (lastCycleSelectAtomType != null && lastCycleSelectAtomType == type)
		{
			List<string> list2 = new List<string>();
			int num = 0;
			int num2 = -1;
			foreach (string item in list)
			{
				Atom atomByUid = GetAtomByUid(item);
				if (atomByUid.type == type)
				{
					list2.Add(item);
					if (lastCycleSelectAtomUid == item)
					{
						num2 = num;
					}
					num++;
				}
			}
			if (num2 != -1)
			{
				text = ((num2 != list2.Count - 1) ? list2[num2 + 1] : list2[0]);
			}
			else if (list2.Count > 0)
			{
				text = list2[0];
			}
		}
		else
		{
			foreach (string item2 in list)
			{
				Atom atomByUid2 = GetAtomByUid(item2);
				if (atomByUid2.type == type)
				{
					text = item2;
					break;
				}
			}
		}
		if (text != null)
		{
			lastCycleSelectAtomUid = text;
			lastCycleSelectAtomType = type;
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValue = text;
			}
			List<string> freeControllerNamesInAtom = GetFreeControllerNamesInAtom(text);
			if (freeControllerNamesInAtom != null && freeControllerNamesInAtom.Count > 1)
			{
				selectControllerPopup.currentValue = freeControllerNamesInAtom[0];
			}
			activeUI = ActiveUI.SelectedOptions;
		}
	}

	private void SyncControllerPopup(string nv)
	{
		string currentValue = selectAtomPopup.currentValue;
		if (currentValue == "None")
		{
			selectControllerPopup.currentValueNoCallback = "None";
			selectControllerPopup.numPopupValues = 1;
			selectControllerPopup.setPopupValue(0, "None");
			return;
		}
		string empty = string.Empty;
		if (selectedController != null && selectedController.containingAtom != null)
		{
			empty = selectedController.containingAtom.uid;
			if (empty != currentValue)
			{
				selectControllerPopup.currentValueNoCallback = "None";
			}
		}
		else
		{
			selectControllerPopup.currentValueNoCallback = "None";
		}
		List<string> freeControllerNamesInAtom = GetFreeControllerNamesInAtom(currentValue);
		if (freeControllerNamesInAtom == null || freeControllerNamesInAtom.Count == 0)
		{
			selectControllerPopup.numPopupValues = 1;
			selectControllerPopup.setPopupValue(0, "None");
			return;
		}
		selectControllerPopup.numPopupValues = freeControllerNamesInAtom.Count + 1;
		selectControllerPopup.setPopupValue(0, "None");
		for (int i = 0; i < freeControllerNamesInAtom.Count; i++)
		{
			selectControllerPopup.setPopupValue(i + 1, freeControllerNamesInAtom[i]);
		}
		if (freeControllerNamesInAtom.Count == 1)
		{
			selectControllerPopup.currentValue = freeControllerNamesInAtom[0];
		}
	}

	private void SyncUIToSelectedController()
	{
		if (selectedController == null)
		{
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = string.Empty;
			}
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValue = "None";
			}
			if (selectControllerPopup != null)
			{
				selectControllerPopup.currentValueNoCallback = "None";
			}
			return;
		}
		Atom containingAtom = selectedController.containingAtom;
		if (containingAtom != null)
		{
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = selectedController.containingAtom.uid + ":" + selectedController.name;
			}
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValue = containingAtom.uid;
			}
			if (selectControllerPopup != null)
			{
				selectControllerPopup.currentValueNoCallback = selectedController.name;
			}
		}
		else if (selectedControllerNameDisplay != null)
		{
			selectedControllerNameDisplay.text = selectedController.name;
		}
	}

	public void SetAlignRotationOffset(string type)
	{
		try
		{
			alignRotationOffset = (AlignRotationOffset)System.Enum.Parse(typeof(AlignRotationOffset), type);
		}
		catch (ArgumentException)
		{
			Error("Attempted to align rotation offset type to " + type + " which is not a valid type");
		}
	}

	public void AlignRigFacingController(FreeControllerV3 controller, bool rotationOnly = false)
	{
		if (!(controller != null))
		{
			return;
		}
		SetMonitorRigPositionZero();
		Transform focusPoint = controller.focusPoint;
		if (focusPoint == null)
		{
			focusPoint = controller.transform;
		}
		Vector3 up = navigationRig.up;
		Vector3 toDirection = Vector3.ProjectOnPlane(focusPoint.position - motionControllerHead.position, up);
		Vector3 fromDirection = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
		navigationRig.rotation = quaternion * navigationRig.rotation;
		if (!rotationOnly)
		{
			toDirection = Vector3.ProjectOnPlane(focusPoint.position - motionControllerHead.position, up);
			float magnitude = toDirection.magnitude;
			float num = ((!MonitorRigActive) ? (1.5f * _worldScale) : (3f * _worldScale));
			if (Mathf.Approximately(magnitude, 0f))
			{
				navigationRig.position -= motionControllerHead.forward * num;
			}
			else
			{
				navigationRig.position -= toDirection.normalized * (num - magnitude);
			}
		}
		if (!_mainHUDAnchoredOnMonitor)
		{
			if (_alignRotationOffset == AlignRotationOffset.Left)
			{
				navigationRig.Rotate(0f, 35f, 0f);
			}
			else if (_alignRotationOffset == AlignRotationOffset.Right)
			{
				navigationRig.Rotate(0f, -35f, 0f);
			}
		}
		SyncMonitorRigPosition();
	}

	public void AlignRigFacingSelectedController(bool rotationOnly = false)
	{
		AlignRigFacingController(selectedController, rotationOnly);
	}

	protected void SyncWorldUIOverlaySky()
	{
		if (SkyshopLightController.singleton != null)
		{
			SkyshopLightController.singleton.overlaySkyActive = worldUIActivated && _worldUIShowOverlaySky;
		}
	}

	public void ActivateWorldUI()
	{
		if (worldUI != null && !worldUIActivated)
		{
			worldUIActiveFlag = new AsyncFlag("World UI Active");
			PauseAutoSimulation(worldUIActiveFlag);
			PauseRender(worldUIActiveFlag);
			worldUI.gameObject.SetActive(true);
			worldUIActivated = true;
			if (MonitorCenterCamera != null)
			{
				MonitorCenterCamera.transform.localEulerAngles = Vector3.zero;
			}
			SyncMonitorCameraFOV();
			SyncWorldUIOverlaySky();
			SyncMonitorAuxUI();
		}
		HideMainHUD();
	}

	public void DeactivateWorldUI()
	{
		if (worldUI != null && worldUIActivated)
		{
			worldUIActiveFlag.Raise();
			worldUI.gameObject.SetActive(false);
			worldUIActivated = false;
			SyncMonitorCameraFOV();
			SyncWorldUIOverlaySky();
			SyncMonitorAuxUI();
			ShowMainHUDAuto();
		}
	}

	public void CloseAllWorldUIPanels()
	{
		if (wizardWorldUI != null)
		{
			wizardWorldUI.gameObject.SetActive(false);
		}
		if (fileBrowserWorldUI != null)
		{
			fileBrowserWorldUI.Hide();
		}
		if (templatesFileBrowserWorldUI != null)
		{
			templatesFileBrowserWorldUI.Hide();
		}
		if (hubBrowser != null)
		{
			hubBrowser.Hide();
		}
		if (topWorldUI != null)
		{
			topWorldUI.gameObject.SetActive(false);
		}
	}

	protected void OpenTopWorldUI()
	{
		if (topWorldUI != null)
		{
			ActivateWorldUI();
			topWorldUI.gameObject.SetActive(true);
		}
		if (hubBrowser != null)
		{
			hubBrowser.Hide();
		}
	}

	protected void SyncWorldUIAnchor()
	{
		if (worldUIAnchor != null)
		{
			Vector3 localPosition = worldUIAnchor.transform.localPosition;
			if (MonitorRigActive)
			{
				localPosition.y = _worldUIMonitorAnchorHeight;
				localPosition.z = _worldUIMonitorAnchorDistance;
			}
			else
			{
				localPosition.y = _worldUIVRAnchorHeight;
				localPosition.z = _worldUIVRAnchorDistance;
			}
			worldUIAnchor.transform.localPosition = localPosition;
		}
	}

	protected void SyncWorldUIVRAnchorDistance(float f)
	{
		worldUIVRAnchorDistance = f;
	}

	protected void SyncWorldUIVRAnchorHeight(float f)
	{
		worldUIVRAnchorHeight = f;
	}

	public void SetCustomUI(Transform cui)
	{
		if (customUI != null)
		{
			customUI.gameObject.SetActive(false);
		}
		if (!customUIDisabled)
		{
			customUI = cui;
		}
		else
		{
			customUI = alternateCustomUI;
		}
		activeUI = ActiveUI.Custom;
	}

	public void SetToLastActiveUI()
	{
		activeUI = _lastActiveUI;
	}

	private void ClearAllUI()
	{
		if (mainMenuUI != null)
		{
			mainMenuUI.gameObject.SetActive(false);
		}
		if (selectedController != null)
		{
			selectedController.guihidden = true;
		}
		if (multiButtonPanel != null)
		{
			multiButtonPanel.gameObject.SetActive(false);
		}
		if (sceneControlUI != null)
		{
			sceneControlUI.gameObject.SetActive(true);
		}
		if (embeddedSceneUI != null)
		{
			embeddedSceneUI.gameObject.SetActive(false);
		}
		if (customUI != null)
		{
			customUI.gameObject.SetActive(false);
		}
		if (onlineBrowserUI != null)
		{
			onlineBrowserUI.gameObject.SetActive(false);
		}
		if (packageBuilderUI != null)
		{
			packageBuilderUI.gameObject.SetActive(false);
		}
		if (packageManagerUI != null)
		{
			packageManagerUI.gameObject.SetActive(false);
		}
		if (packageDownloader != null)
		{
			packageDownloader.ClosePanel();
		}
		if (fileBrowserUI != null)
		{
			fileBrowserUI.Hide();
		}
		if (mediaFileBrowserUI != null)
		{
			mediaFileBrowserUI.Hide();
		}
		if (directoryBrowserUI != null)
		{
			directoryBrowserUI.Hide();
		}
	}

	private void SyncActiveUI()
	{
		switch (_activeUI)
		{
		case ActiveUI.MainMenu:
			if (mainMenuUI != null)
			{
				mainMenuUI.gameObject.SetActive(true);
			}
			break;
		case ActiveUI.MainMenuOnly:
			if (mainMenuUI != null)
			{
				mainMenuUI.gameObject.SetActive(true);
			}
			if (sceneControlUI != null)
			{
				sceneControlUI.gameObject.SetActive(false);
			}
			if (sceneControlUIAlt != null)
			{
				sceneControlUIAlt.gameObject.SetActive(false);
			}
			break;
		case ActiveUI.SelectedOptions:
			if (selectedController != null && _mainHUDVisible)
			{
				selectedController.guihidden = false;
			}
			break;
		case ActiveUI.MultiButtonPanel:
			if (multiButtonPanel != null)
			{
				multiButtonPanel.gameObject.SetActive(true);
			}
			break;
		case ActiveUI.EmbeddedScenePanel:
			if (embeddedSceneUI != null)
			{
				embeddedSceneUI.gameObject.SetActive(true);
			}
			break;
		case ActiveUI.OnlineBrowser:
			if (UserPreferences.singleton == null || UserPreferences.singleton.enableWebBrowser)
			{
				if (onlineBrowserUI != null)
				{
					onlineBrowserUI.gameObject.SetActive(true);
				}
			}
			else
			{
				Error("Web Browsing is disabled. To use this feature you must enable browser in User Preferences -> Security tab");
				activeUI = ActiveUI.MainMenu;
				SetMainMenuTab("TabUserPrefs");
				SetUserPrefsTab("TabSecurity");
			}
			break;
		case ActiveUI.PackageBuilder:
			if (packageBuilderUI != null)
			{
				packageBuilderUI.gameObject.SetActive(true);
			}
			break;
		case ActiveUI.PackageManager:
			if (packageManagerUI != null)
			{
				packageManagerUI.gameObject.SetActive(true);
			}
			break;
		case ActiveUI.PackageDownloader:
			if (packageDownloader != null)
			{
				packageDownloader.OpenPanel();
			}
			break;
		case ActiveUI.Custom:
			if (customUI != null)
			{
				customUI.gameObject.SetActive(true);
			}
			break;
		}
		SyncVisibility();
	}

	public void SetActiveUI(string uiName)
	{
		try
		{
			activeUI = (ActiveUI)System.Enum.Parse(typeof(ActiveUI), uiName);
		}
		catch (ArgumentException)
		{
			Error("Attempted to set UI to " + uiName + " which is not a valid UI name");
		}
	}

	public void SetMainMenuTab(string tabName)
	{
		if (mainMenuTabSelector != null)
		{
			mainMenuTabSelector.SetActiveTab(tabName);
		}
	}

	public void SetUserPrefsTab(string tabName)
	{
		if (userPrefsTabSelector != null)
		{
			userPrefsTabSelector.SetActiveTab(tabName);
		}
	}

	private void InitUI()
	{
		if (MonitorModeAuxUI != null)
		{
			if (isMonitorOnly && !UIDisabled)
			{
				MonitorModeAuxUI.gameObject.SetActive(true);
			}
			else
			{
				MonitorModeAuxUI.gameObject.SetActive(false);
			}
		}
		if (loadingUI != null)
		{
			loadingUI.gameObject.SetActive(false);
		}
		if (loadingUIAlt != null)
		{
			loadingUIAlt.gameObject.SetActive(false);
		}
		if (loadingGeometry != null)
		{
			loadingGeometry.gameObject.SetActive(false);
		}
		if (mainHUDAttachPoint != null)
		{
			mainHUDAttachPointStartingPosition = mainHUDAttachPoint.localPosition;
			mainHUDAttachPointStartingRotation = mainHUDAttachPoint.localRotation;
		}
		activeUI = ActiveUI.SelectedOptions;
		if (selectionHUDTransform != null)
		{
			selectionHUD = selectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
		}
		if (rightSelectionHUDTransform != null)
		{
			rightSelectionHUD = rightSelectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(rightSelectionHUD, string.Empty);
		}
		if (leftSelectionHUDTransform != null)
		{
			leftSelectionHUD = leftSelectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(leftSelectionHUD, string.Empty);
		}
		if (worldScaleSlider != null)
		{
			worldScaleSlider.minValue = 0.1f;
			worldScaleSlider.maxValue = 40f;
			worldScaleSlider.value = _worldScale;
			worldScaleSlider.onValueChanged.AddListener(delegate
			{
				worldScale = worldScaleSlider.value;
			});
			SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
			if (component != null)
			{
				component.defaultValue = 1f;
			}
		}
		if (worldScaleSliderAlt != null)
		{
			worldScaleSliderAlt.minValue = 0.1f;
			worldScaleSliderAlt.maxValue = 40f;
			worldScaleSliderAlt.value = _worldScale;
			worldScaleSliderAlt.onValueChanged.AddListener(delegate
			{
				worldScale = worldScaleSliderAlt.value;
			});
			SliderControl component2 = worldScaleSliderAlt.GetComponent<SliderControl>();
			if (component2 != null)
			{
				component2.defaultValue = 1f;
			}
		}
		if (controllerScaleSlider != null)
		{
			controllerScaleSlider.value = _controllerScale;
			controllerScaleSlider.onValueChanged.AddListener(delegate
			{
				controllerScale = controllerScaleSlider.value;
			});
			SliderControl component3 = controllerScaleSlider.GetComponent<SliderControl>();
			if (component3 != null)
			{
				component3.defaultValue = 1f;
			}
		}
		if (playerHeightAdjustSlider != null)
		{
			playerHeightAdjustSlider.value = _playerHeightAdjust;
			playerHeightAdjustSlider.onValueChanged.AddListener(delegate
			{
				playerHeightAdjust = playerHeightAdjustSlider.value;
			});
			SliderControl component4 = playerHeightAdjustSlider.GetComponent<SliderControl>();
			if (component4 != null)
			{
				component4.defaultValue = 0f;
			}
		}
		if (playerHeightAdjustSliderAlt != null)
		{
			playerHeightAdjustSliderAlt.value = _playerHeightAdjust;
			playerHeightAdjustSliderAlt.onValueChanged.AddListener(delegate
			{
				playerHeightAdjust = playerHeightAdjustSliderAlt.value;
			});
			SliderControl component5 = playerHeightAdjustSliderAlt.GetComponent<SliderControl>();
			if (component5 != null)
			{
				component5.defaultValue = 0f;
			}
		}
		if (monitorUIScaleSlider != null)
		{
			monitorUIScaleSlider.value = _monitorUIScale;
			monitorUIScaleSlider.onValueChanged.AddListener(delegate
			{
				monitorUIScale = monitorUIScaleSlider.value;
			});
			SliderControl component6 = monitorUIScaleSlider.GetComponent<SliderControl>();
			if (component6 != null)
			{
				component6.defaultValue = 1f;
			}
		}
		if (monitorUIYOffsetSlider != null)
		{
			monitorUIYOffsetSlider.value = _monitorUIYOffset;
			monitorUIYOffsetSlider.onValueChanged.AddListener(delegate
			{
				monitorUIYOffset = monitorUIYOffsetSlider.value;
			});
			SliderControl component7 = monitorUIYOffsetSlider.GetComponent<SliderControl>();
			if (component7 != null)
			{
				component7.defaultValue = 0f;
			}
		}
		if (monitorCameraFOVSlider != null)
		{
			monitorCameraFOVSlider.value = _monitorCameraFOV;
			monitorCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				monitorCameraFOV = monitorCameraFOVSlider.value;
			});
			SliderControl component8 = monitorCameraFOVSlider.GetComponent<SliderControl>();
			if (component8 != null)
			{
				component8.defaultValue = 40f;
			}
		}
		if (editModeToggle != null)
		{
			editModeToggle.isOn = _gameMode == GameMode.Edit;
			editModeToggle.onValueChanged.AddListener(delegate
			{
				if (editModeToggle.isOn)
				{
					gameMode = GameMode.Edit;
				}
			});
		}
		if (playModeToggle != null)
		{
			playModeToggle.isOn = _gameMode == GameMode.Play;
			playModeToggle.onValueChanged.AddListener(delegate
			{
				if (playModeToggle.isOn)
				{
					gameMode = GameMode.Play;
				}
			});
		}
		if (selectedControllerNameDisplay != null)
		{
			selectedControllerNameDisplay.text = string.Empty;
		}
		if (rayLineMaterialLeft != null)
		{
			rayLineDrawerLeft = new LineDrawer(rayLineMaterialLeft);
		}
		if (rayLineMaterialRight != null)
		{
			rayLineDrawerRight = new LineDrawer(rayLineMaterialRight);
		}
		if (twoStageLineMaterial != null)
		{
			rightTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leftTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			headTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leapRightTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leapLeftTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker1TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker2TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker3TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker4TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker5TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker6TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker7TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker8TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
		}
		if (UISidePopup != null)
		{
			UISidePopup.currentValueNoCallback = _UISide.ToString();
			UIPopup uISidePopup = UISidePopup;
			uISidePopup.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uISidePopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetUISide));
		}
		if (helpToggle != null)
		{
			helpToggle.onValueChanged.AddListener(delegate
			{
				helpOverlayOn = helpToggle.isOn;
			});
		}
		if (helpToggleAlt != null)
		{
			helpToggleAlt.onValueChanged.AddListener(delegate
			{
				helpOverlayOn = helpToggleAlt.isOn;
			});
		}
		SyncHelpOverlay();
		if (lockHeightDuringNavigateToggle != null)
		{
			lockHeightDuringNavigateToggle.onValueChanged.AddListener(delegate
			{
				lockHeightDuringNavigate = lockHeightDuringNavigateToggle.isOn;
			});
		}
		if (lockHeightDuringNavigateToggleAlt != null)
		{
			lockHeightDuringNavigateToggleAlt.onValueChanged.AddListener(delegate
			{
				lockHeightDuringNavigate = lockHeightDuringNavigateToggleAlt.isOn;
			});
		}
		SyncLockHeightDuringNavigate();
		if (freeMoveFollowFloorToggle != null)
		{
			freeMoveFollowFloorToggle.onValueChanged.AddListener(delegate
			{
				freeMoveFollowFloor = freeMoveFollowFloorToggle.isOn;
			});
		}
		if (freeMoveFollowFloorToggleAlt != null)
		{
			freeMoveFollowFloorToggleAlt.onValueChanged.AddListener(delegate
			{
				freeMoveFollowFloor = freeMoveFollowFloorToggleAlt.isOn;
			});
		}
		SyncFreeMoveFollowFloor();
		if (disableAllNavigationToggle != null)
		{
			disableAllNavigationToggle.onValueChanged.AddListener(delegate
			{
				disableAllNavigation = disableAllNavigationToggle.isOn;
			});
		}
		SyncDisableAllNavigation();
		if (disableGrabNavigationToggle != null)
		{
			disableGrabNavigationToggle.onValueChanged.AddListener(delegate
			{
				disableGrabNavigation = disableGrabNavigationToggle.isOn;
			});
		}
		SyncDisableGrabNavigation();
		if (disableTeleportToggle != null)
		{
			disableTeleportToggle.onValueChanged.AddListener(delegate
			{
				disableTeleport = disableTeleportToggle.isOn;
			});
		}
		SyncDisableTeleport();
		if (disableTeleportDuringPossessToggle != null)
		{
			disableTeleportDuringPossessToggle.onValueChanged.AddListener(delegate
			{
				disableTeleportDuringPossess = disableTeleportDuringPossessToggle.isOn;
			});
		}
		SyncDisableTeleportDuringPossess();
		if (teleportAllowRotationToggle != null)
		{
			teleportAllowRotationToggle.onValueChanged.AddListener(delegate
			{
				teleportAllowRotation = teleportAllowRotationToggle.isOn;
			});
		}
		SyncTeleportAllowRotation();
		if (freeMoveMultiplierSlider != null)
		{
			freeMoveMultiplierSlider.onValueChanged.AddListener(delegate
			{
				freeMoveMultiplier = freeMoveMultiplierSlider.value;
			});
		}
		if (grabNavigationPositionMultiplierSlider != null)
		{
			grabNavigationPositionMultiplierSlider.onValueChanged.AddListener(delegate
			{
				grabNavigationPositionMultiplier = grabNavigationPositionMultiplierSlider.value;
			});
		}
		SyncGrabNavigationPositionMultiplier();
		if (grabNavigationRotationMultiplierSlider != null)
		{
			grabNavigationRotationMultiplierSlider.onValueChanged.AddListener(delegate
			{
				grabNavigationRotationMultiplier = grabNavigationRotationMultiplierSlider.value;
			});
		}
		SyncGrabNavigationRotationMultiplier();
		SyncUIToUnlockLevel();
		if (showNavigationHologridToggle != null)
		{
			showNavigationHologridToggle.isOn = _showNavigationHologrid;
			showNavigationHologridToggle.onValueChanged.AddListener(delegate
			{
				showNavigationHologrid = showNavigationHologridToggle.isOn;
			});
		}
		SyncHologridTransparency();
		if (hologridTransparencySlider != null)
		{
			hologridTransparencySlider.value = _hologridTransparency;
			hologridTransparencySlider.onValueChanged.AddListener(delegate
			{
				hologridTransparency = hologridTransparencySlider.value;
			});
			SliderControl component9 = hologridTransparencySlider.GetComponent<SliderControl>();
			if (component9 != null)
			{
				component9.defaultValue = 0.01f;
			}
		}
		if (oculusThumbstickFunctionPopup != null)
		{
			oculusThumbstickFunctionPopup.currentValue = _oculusThumbstickFunction.ToString();
			UIPopup uIPopup = oculusThumbstickFunctionPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetOculusThumbstickFunctionFromString));
		}
		if (allowHeadPossessMousePanAndZoomToggle != null)
		{
			allowHeadPossessMousePanAndZoomToggle.isOn = _allowHeadPossessMousePanAndZoom;
			allowHeadPossessMousePanAndZoomToggle.onValueChanged.AddListener(delegate(bool b)
			{
				allowHeadPossessMousePanAndZoom = b;
			});
		}
		if (allowPossessSpringAdjustmentToggle != null)
		{
			allowPossessSpringAdjustmentToggle.isOn = _allowPossessSpringAdjustment;
			allowPossessSpringAdjustmentToggle.onValueChanged.AddListener(delegate
			{
				allowPossessSpringAdjustment = allowPossessSpringAdjustmentToggle.isOn;
			});
		}
		if (possessPositionSpringSlider != null)
		{
			possessPositionSpringSlider.value = _possessPositionSpring;
			possessPositionSpringSlider.onValueChanged.AddListener(delegate
			{
				possessPositionSpring = possessPositionSpringSlider.value;
			});
			SliderControl component10 = possessPositionSpringSlider.GetComponent<SliderControl>();
			if (component10 != null)
			{
				component10.defaultValue = 10000f;
			}
		}
		if (possessRotationSpringSlider != null)
		{
			possessRotationSpringSlider.value = _possessRotationSpring;
			possessRotationSpringSlider.onValueChanged.AddListener(delegate
			{
				possessRotationSpring = possessRotationSpringSlider.value;
			});
			SliderControl component11 = possessRotationSpringSlider.GetComponent<SliderControl>();
			if (component11 != null)
			{
				component11.defaultValue = 1000f;
			}
		}
		if (generateDepthTextureToggle != null)
		{
			generateDepthTextureToggle.isOn = _generateDepthTexture;
			generateDepthTextureToggle.onValueChanged.AddListener(delegate(bool b)
			{
				generateDepthTexture = b;
			});
		}
		if (useMonitorRigAudioListenerWhenActiveToggle != null)
		{
			useMonitorRigAudioListenerWhenActiveToggle.isOn = _useMonitorRigAudioListenerWhenActive;
			useMonitorRigAudioListenerWhenActiveToggle.onValueChanged.AddListener(delegate(bool b)
			{
				useMonitorRigAudioListenerWhenActive = b;
			});
		}
		if (navigationHologrid != null)
		{
			navigationHologrid.gameObject.SetActive(false);
		}
		if (showMainHUDOnStart && !UIDisabled && !_onStartupSkipStartScreen)
		{
			OpenTopWorldUI();
			ShowMainHUDAuto();
			HideMainHUD();
		}
		else
		{
			HideMainHUD();
		}
		helpText = string.Empty;
		helpColor = Color.white;
		if (loResScreenShotCameraFOVSlider != null)
		{
			loResScreenShotCameraFOVSlider.value = _loResScreenShotCameraFOV;
			loResScreenShotCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				loResScreenShotCameraFOV = loResScreenShotCameraFOVSlider.value;
			});
			SliderControl component12 = loResScreenShotCameraFOVSlider.GetComponent<SliderControl>();
			if (component12 != null)
			{
				component12.defaultValue = 40f;
			}
		}
		if (hiResScreenShotCameraFOVSlider != null)
		{
			hiResScreenShotCameraFOVSlider.value = _hiResScreenShotCameraFOV;
			hiResScreenShotCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				hiResScreenShotCameraFOV = hiResScreenShotCameraFOVSlider.value;
			});
			SliderControl component13 = hiResScreenShotCameraFOVSlider.GetComponent<SliderControl>();
			if (component13 != null)
			{
				component13.defaultValue = 40f;
			}
		}
		if (selectAtomPopup != null)
		{
			selectAtomPopup.currentValue = "None";
			UIPopup uIPopup2 = selectAtomPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SyncControllerPopup));
		}
		if (selectControllerPopup != null)
		{
			selectControllerPopup.currentValue = "None";
			UIPopup uIPopup3 = selectControllerPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SelectFreeController));
		}
		if (alignRotationOffsetPopup != null)
		{
			alignRotationOffsetPopup.currentValue = _alignRotationOffset.ToString();
			UIPopup uIPopup4 = alignRotationOffsetPopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetAlignRotationOffset));
		}
		if (worldUIShowOverlaySkyToggle != null)
		{
			worldUIShowOverlaySkyToggle.isOn = _worldUIShowOverlaySky;
			worldUIShowOverlaySkyToggle.onValueChanged.AddListener(delegate(bool b)
			{
				worldUIShowOverlaySky = b;
			});
		}
		if (worldUIVRAnchorDistanceSlider != null)
		{
			worldUIVRAnchorDistanceSlider.value = _worldUIVRAnchorDistance;
			worldUIVRAnchorDistanceSlider.onValueChanged.AddListener(SyncWorldUIVRAnchorDistance);
		}
		if (worldUIVRAnchorHeightSlider != null)
		{
			worldUIVRAnchorHeightSlider.value = _worldUIVRAnchorHeight;
			worldUIVRAnchorHeightSlider.onValueChanged.AddListener(SyncWorldUIVRAnchorHeight);
		}
		if (pauseAutoSimulationToggle != null)
		{
			pauseAutoSimulationToggle.isOn = _pauseAutoSimulation;
			pauseAutoSimulationToggle.onValueChanged.AddListener(SetPauseAutoSimulation);
		}
		if (pauseRenderToggle != null)
		{
			pauseRenderToggle.isOn = _pauseRender;
			pauseRenderToggle.onValueChanged.AddListener(SetPauseRender);
		}
		if (autoFreezeAnimationOnSwitchToEditModeToggle != null)
		{
			autoFreezeAnimationOnSwitchToEditModeToggle.isOn = _autoFreezeAnimationOnSwitchToEditMode;
			autoFreezeAnimationOnSwitchToEditModeToggle.onValueChanged.AddListener(SyncAutoFreezeAnimationOnSwitchToEditMode);
		}
		if (freezeAnimationToggle != null)
		{
			freezeAnimationToggle.onValueChanged.AddListener(SetFreezeAnimation);
		}
		if (useSceneLoadPositionToggle != null)
		{
			useSceneLoadPositionToggle.isOn = _useSceneLoadPosition;
			useSceneLoadPositionToggle.onValueChanged.AddListener(SetUseSceneLoadPosition);
		}
		if (showHiddenAtomsToggle != null)
		{
			showHiddenAtomsToggle.isOn = _showHiddenAtoms;
			showHiddenAtomsToggle.onValueChanged.AddListener(delegate(bool b)
			{
				showHiddenAtoms = b;
			});
		}
		if (showHiddenAtomsToggleAlt != null)
		{
			showHiddenAtomsToggleAlt.isOn = _showHiddenAtoms;
			showHiddenAtomsToggleAlt.onValueChanged.AddListener(delegate(bool b)
			{
				showHiddenAtoms = b;
			});
		}
		if (allowGrabPlusTriggerHandToggleToggle != null)
		{
			allowGrabPlusTriggerHandToggleToggle.isOn = _allowGrabPlusTriggerHandToggle;
			allowGrabPlusTriggerHandToggleToggle.onValueChanged.AddListener(delegate(bool b)
			{
				allowGrabPlusTriggerHandToggle = b;
			});
		}
		if (alwaysUseAlternateHandsToggle != null)
		{
			alwaysUseAlternateHandsToggle.isOn = _alwaysUseAlternateHands;
			alwaysUseAlternateHandsToggle.onValueChanged.AddListener(delegate(bool b)
			{
				alwaysUseAlternateHands = b;
			});
		}
		if (useLegacyWorldScaleChangeToggle != null)
		{
			useLegacyWorldScaleChangeToggle.isOn = _useLegacyWorldScaleChange;
			useLegacyWorldScaleChangeToggle.onValueChanged.AddListener(delegate(bool b)
			{
				useLegacyWorldScaleChange = b;
			});
		}
		if (disableRenderForAtomsNotInIsolatedSubSceneToggle != null)
		{
			disableRenderForAtomsNotInIsolatedSubSceneToggle.isOn = _disableRenderForAtomsNotInIsolatedSubScene;
			disableRenderForAtomsNotInIsolatedSubSceneToggle.onValueChanged.AddListener(delegate(bool b)
			{
				disableRenderForAtomsNotInIsolatedSubScene = b;
			});
		}
		if (freezePhysicsForAtomsNotInIsolatedSubSceneToggle != null)
		{
			freezePhysicsForAtomsNotInIsolatedSubSceneToggle.isOn = _freezePhysicsForAtomsNotInIsolatedSubScene;
			freezePhysicsForAtomsNotInIsolatedSubSceneToggle.onValueChanged.AddListener(delegate(bool b)
			{
				freezePhysicsForAtomsNotInIsolatedSubScene = b;
			});
		}
		if (disableCollisionForAtomsNotInIsolatedSubSceneToggle != null)
		{
			disableCollisionForAtomsNotInIsolatedSubSceneToggle.isOn = _disableCollisionForAtomsNotInIsolatedSubScene;
			disableCollisionForAtomsNotInIsolatedSubSceneToggle.onValueChanged.AddListener(delegate(bool b)
			{
				disableCollisionForAtomsNotInIsolatedSubScene = b;
			});
		}
		if (endIsolateEditSubSceneButton != null)
		{
			endIsolateEditSubSceneButton.onClick.AddListener(EndIsolateEditSubScene);
		}
		if (quickSaveIsolatedSubSceneButton != null)
		{
			quickSaveIsolatedSubSceneButton.onClick.AddListener(QuickSaveIsolatedSubScene);
		}
		if (quickReloadIsolatedSubSceneButton != null)
		{
			quickReloadIsolatedSubSceneButton.onClick.AddListener(QuickReloadIsolatedSubScene);
		}
		if (selectIsolatedSubSceneButton != null)
		{
			selectIsolatedSubSceneButton.onClick.AddListener(SelectIsolatedSubScene);
		}
		if (disableRenderForAtomsNotInIsolatedAtomToggle != null)
		{
			disableRenderForAtomsNotInIsolatedAtomToggle.isOn = _disableRenderForAtomsNotInIsolatedAtom;
			disableRenderForAtomsNotInIsolatedAtomToggle.onValueChanged.AddListener(delegate(bool b)
			{
				disableRenderForAtomsNotInIsolatedAtom = b;
			});
		}
		if (freezePhysicsForAtomsNotInIsolatedAtomToggle != null)
		{
			freezePhysicsForAtomsNotInIsolatedAtomToggle.isOn = _freezePhysicsForAtomsNotInIsolatedAtom;
			freezePhysicsForAtomsNotInIsolatedAtomToggle.onValueChanged.AddListener(delegate(bool b)
			{
				freezePhysicsForAtomsNotInIsolatedAtom = b;
			});
		}
		if (disableCollisionForAtomsNotInIsolatedAtomToggle != null)
		{
			disableCollisionForAtomsNotInIsolatedAtomToggle.isOn = _disableCollisionForAtomsNotInIsolatedAtom;
			disableCollisionForAtomsNotInIsolatedAtomToggle.onValueChanged.AddListener(delegate(bool b)
			{
				disableCollisionForAtomsNotInIsolatedAtom = b;
			});
		}
		if (endIsolateEditAtomButton != null)
		{
			endIsolateEditAtomButton.onClick.AddListener(EndIsolateEditAtom);
		}
		if (selectIsolatedAtomButton != null)
		{
			selectIsolatedAtomButton.onClick.AddListener(SelectIsolatedAtom);
		}
		if (keyInputFieldAction != null)
		{
			InputFieldAction inputFieldAction = keyInputFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)System.Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(AddKey));
		}
		if (onStartupSkipStartScreenToggle != null)
		{
			onStartupSkipStartScreenToggle.isOn = _onStartupSkipStartScreen;
			onStartupSkipStartScreenToggle.onValueChanged.AddListener(SyncOnStartupSkipStartScreen);
		}
		if (leapMotionEnabledToggle != null)
		{
			leapMotionEnabledToggle.isOn = _leapMotionEnabled;
			leapMotionEnabledToggle.onValueChanged.AddListener(delegate(bool b)
			{
				leapMotionEnabled = b;
			});
		}
		if (openMainHUDOnErrorToggle != null)
		{
			openMainHUDOnErrorToggle.isOn = _openMainHUDOnError;
			openMainHUDOnErrorToggle.onValueChanged.AddListener(delegate(bool b)
			{
				openMainHUDOnError = b;
			});
		}
	}

	private void SyncHelpOverlay()
	{
		if (helpToggle != null)
		{
			helpToggle.isOn = _helpOverlayOn;
		}
		if (helpToggleAlt != null)
		{
			helpToggleAlt.isOn = _helpOverlayOn;
		}
		if (_helpOverlayOn && _helpOverlayOnAux)
		{
			if (isOVR)
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(true);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(false);
				}
			}
			else if (isOpenVR)
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(false);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(true);
				}
			}
			else
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(false);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			if (helpOverlayOVR != null)
			{
				helpOverlayOVR.gameObject.SetActive(false);
			}
			if (helpOverlayVive != null)
			{
				helpOverlayVive.gameObject.SetActive(false);
			}
		}
	}

	protected void SyncHelpText()
	{
		if (helpHUDText != null)
		{
			if (tempHelpText != null)
			{
				helpHUDText.text = tempHelpText;
				helpHUDText.color = Color.white;
			}
			else
			{
				helpHUDText.text = _helpText;
				helpHUDText.color = _helpColor;
			}
		}
	}

	public void ShowTempHelp(string text)
	{
		tempHelpText = text;
		SyncHelpText();
	}

	public void HideTempHelp()
	{
		tempHelpText = null;
		SyncHelpText();
	}

	protected void SyncActiveHands()
	{
		if (!autoSimulation)
		{
			if (leftHandAlternate != null)
			{
				leftHandAlternate.gameObject.SetActive(false);
			}
			if (leftHand != null)
			{
				leftHand.gameObject.SetActive(false);
			}
			if (rightHandAlternate != null)
			{
				rightHandAlternate.gameObject.SetActive(false);
			}
			if (rightHand != null)
			{
				rightHand.gameObject.SetActive(false);
			}
		}
		else if (_alwaysUseAlternateHands)
		{
			if (leftHandAlternate != null)
			{
				leftHandAlternate.gameObject.SetActive(!IsMonitorOnly);
			}
			if (leftHand != null)
			{
				leftHand.gameObject.SetActive(_leapHandLeftConnected);
			}
			if (rightHandAlternate != null)
			{
				rightHandAlternate.gameObject.SetActive(!IsMonitorOnly);
			}
			if (rightHand != null)
			{
				rightHand.gameObject.SetActive(_leapHandRightConnected);
			}
		}
		else
		{
			if (leftHandAlternate != null)
			{
				leftHandAlternate.gameObject.SetActive(_leapHandLeftConnected);
			}
			if (leftHand != null)
			{
				leftHand.gameObject.SetActive(!IsMonitorOnly);
			}
			if (rightHandAlternate != null)
			{
				rightHandAlternate.gameObject.SetActive(_leapHandRightConnected);
			}
			if (rightHand != null)
			{
				rightHand.gameObject.SetActive(!IsMonitorOnly);
			}
		}
	}

	public void SelectController(FreeControllerV3 controller, bool alignView = false, bool alignRotationOnly = true, bool alignUpDown = true, bool openUI = true)
	{
		if (selectedController != controller)
		{
			ClearSelection(false);
			selectedController = controller;
			selectedController.selected = true;
			AddPositionRotationHandlesToSelectedController();
			if (selectedController != null && selectedController.containingAtom != null)
			{
				lastCycleSelectAtomUid = selectedController.containingAtom.uid;
				lastCycleSelectAtomType = selectedController.containingAtom.type;
			}
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = selectedController.containingAtom.uid + ":" + selectedController.name;
			}
			SyncUIToSelectedController();
		}
		if (openUI)
		{
			activeUI = ActiveUI.SelectedOptions;
		}
		if (alignView)
		{
			FocusOnSelectedController(alignRotationOnly, alignUpDown);
		}
	}

	public void SelectController(string atomName, string controllerName, bool alignView = false, bool alignRotationOnly = true, bool alignUpDown = true, bool openUI = true)
	{
		FreeControllerV3 freeControllerV = FreeControllerNameToFreeController(atomName + ":" + controllerName);
		if (freeControllerV != null)
		{
			SelectController(freeControllerV, alignView, alignRotationOnly, alignUpDown, openUI);
		}
	}

	private void SelectFreeController(string cv)
	{
		string currentValue = selectAtomPopup.currentValue;
		if (currentValue != "None")
		{
			string currentValue2 = selectControllerPopup.currentValue;
			if (currentValue2 != "None")
			{
				bool alignView = false;
				if (quickSelectAlignToggle != null && quickSelectAlignToggle.isOn)
				{
					alignView = true;
				}
				bool flag = false;
				if (quickSelectMoveToggle != null && quickSelectMoveToggle.isOn)
				{
					flag = true;
				}
				bool openUI = false;
				if (quickSelectOpenUIToggle != null && quickSelectOpenUIToggle.isOn)
				{
					openUI = true;
				}
				SelectController(currentValue, currentValue2, alignView, !flag, true, openUI);
			}
			else
			{
				ClearSelection(false);
			}
		}
		else
		{
			ClearSelection(false);
		}
	}

	public Atom GetSelectedAtom()
	{
		if (selectedController != null)
		{
			return selectedController.containingAtom;
		}
		return null;
	}

	public FreeControllerV3 GetSelectedController()
	{
		return selectedController;
	}

	public void ClearSelection(bool syncSelectedUI = true)
	{
		if (selectedController != null)
		{
			selectedController.selected = false;
			selectedController.hidden = true;
			selectedController.guihidden = true;
			if (selectedControllerPositionHandle != null)
			{
				selectedControllerPositionHandle.ForceDrop();
				selectedControllerPositionHandle.controller = null;
				selectedControllerPositionHandle.enabled = false;
			}
			if (selectedControllerRotationHandle != null)
			{
				selectedControllerRotationHandle.ForceDrop();
				selectedControllerRotationHandle.controller = null;
				selectedControllerRotationHandle.enabled = false;
			}
			selectedController = null;
			if (syncSelectedUI)
			{
				SyncUIToSelectedController();
			}
		}
		if (LookInputModule.singleton != null)
		{
			LookInputModule.singleton.ClearSelection();
		}
		SyncVisibility();
	}

	public void ToggleRotationMode()
	{
		if (selectedController != null)
		{
			selectedController.NextControlMode();
		}
	}

	public void SetOnlyShowControllers(HashSet<FreeControllerV3> onlyControllers)
	{
		onlyShowControllers = onlyControllers;
		SyncVisibility();
	}

	private void ClearSelectionHUDs()
	{
		if (selectionHUD != null)
		{
			selectionHUD.gameObject.SetActive(false);
			selectionHUD.ClearSelections();
		}
		if (rightSelectionHUD != null)
		{
			rightSelectionHUD.ClearSelections();
		}
		if (leftSelectionHUD != null)
		{
			leftSelectionHUD.ClearSelections();
		}
	}

	protected void SyncCursor()
	{
		cursorLockedLastFrame = false;
		if (selectMode == SelectMode.FreeMoveMouse)
		{
			Cursor.visible = false;
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				cursorLockedLastFrame = true;
			}
		}
		else
		{
			Cursor.visible = potentialGrabbedControllerMouse == null;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void SyncVisibility()
	{
		bool flag = _mainHUDVisible || !UserPreferences.singleton.showTargetsMenuOnly;
		if (mouseCrosshair != null)
		{
			mouseCrosshair.SetActive(selectMode == SelectMode.FreeMoveMouse);
		}
		if (freeMouseMoveModeIndicator != null)
		{
			freeMouseMoveModeIndicator.SetActive(selectMode == SelectMode.FreeMoveMouse);
		}
		bool flag2 = selectMode == SelectMode.Targets || selectMode == SelectMode.FilteredTargets;
		switch (selectMode)
		{
		case SelectMode.Targets:
			if (onlyShowControllers != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (onlyShowControllers.Contains(allController))
					{
						allController.hidden = false;
					}
					else
					{
						allController.hidden = true;
					}
				}
				break;
			}
			foreach (FreeControllerV3 allController2 in allControllers)
			{
				if (gameMode == GameMode.Edit && flag)
				{
					if (UserPreferences.singleton.hideInactiveTargets && allController2.currentPositionState == FreeControllerV3.PositionState.Off && allController2.currentRotationState == FreeControllerV3.RotationState.Off)
					{
						allController2.hidden = true;
					}
					else if (allController2.containingAtom == null)
					{
						allController2.hidden = false;
					}
					else if (allController2.containingAtom.tempHidden)
					{
						allController2.hidden = true;
					}
					else if (_showHiddenAtoms || !allController2.containingAtom.hidden)
					{
						allController2.hidden = false;
					}
					else
					{
						allController2.hidden = true;
					}
				}
				else
				{
					allController2.hidden = true;
				}
			}
			if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
			{
				selectedControllerPositionHandle.enabled = flag;
			}
			if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
			{
				selectedControllerRotationHandle.enabled = flag;
			}
			break;
		case SelectMode.FilteredTargets:
			foreach (FreeControllerV3 allController3 in allControllers)
			{
				if (selectedController == null || selectedController != allController3)
				{
					allController3.hidden = true;
				}
				else if (gameMode == GameMode.Edit || allController3.interactableInPlayMode)
				{
					allController3.hidden = false;
				}
				else
				{
					allController3.hidden = true;
				}
			}
			if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
			{
				selectedControllerPositionHandle.enabled = flag;
			}
			if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
			{
				selectedControllerRotationHandle.enabled = flag;
			}
			break;
		default:
			if (allControllers != null)
			{
				foreach (FreeControllerV3 allController4 in allControllers)
				{
					allController4.hidden = true;
				}
			}
			if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
			{
				selectedControllerPositionHandle.enabled = _mainHUDVisible;
			}
			if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
			{
				selectedControllerRotationHandle.enabled = _mainHUDVisible;
			}
			break;
		}
		Atom atom = null;
		if (selectedController != null)
		{
			atom = selectedController.containingAtom;
		}
		if (selectMode == SelectMode.FilteredTargets && atom != null)
		{
			FreeControllerV3[] freeControllers = atom.freeControllers;
			foreach (FreeControllerV3 freeControllerV in freeControllers)
			{
				if (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode)
				{
					freeControllerV.hidden = false;
				}
				else
				{
					freeControllerV.hidden = true;
				}
			}
		}
		if (fpMap != null)
		{
			foreach (ForceProducerV2 value in fpMap.Values)
			{
				value.drawLines = false;
				if (atom != null && atom == value.containingAtom && flag && flag2)
				{
					value.drawLines = true;
				}
			}
		}
		if (gpMap != null)
		{
			foreach (GrabPoint value2 in gpMap.Values)
			{
				value2.drawLines = false;
				if (atom != null && atom == value2.containingAtom && flag && flag2)
				{
					value2.drawLines = true;
				}
			}
		}
		if (allAnimationPatterns != null)
		{
			foreach (AnimationPattern allAnimationPattern in allAnimationPatterns)
			{
				allAnimationPattern.draw = selectMode == SelectMode.Targets && _mainHUDVisible && !allAnimationPattern.hideCurveUnlessSelected && !allAnimationPattern.containingAtom.hidden && !allAnimationPattern.containingAtom.tempHidden;
				allAnimationPattern.SetDrawColor(Color.red);
				if (((!(atom != null) || !(atom == allAnimationPattern.containingAtom)) && (!(_isolatedAtom != null) || !(_isolatedAtom == allAnimationPattern.containingAtom))) || !flag || !flag2)
				{
					continue;
				}
				allAnimationPattern.draw = true;
				allAnimationPattern.SetDrawColor(Color.blue);
				AnimationStep[] steps = allAnimationPattern.steps;
				foreach (AnimationStep animationStep in steps)
				{
					if (!(animationStep.containingAtom != null) || animationStep.containingAtom.freeControllers == null)
					{
						continue;
					}
					FreeControllerV3[] freeControllers2 = animationStep.containingAtom.freeControllers;
					foreach (FreeControllerV3 freeControllerV2 in freeControllers2)
					{
						if (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode)
						{
							freeControllerV2.hidden = false;
						}
						else
						{
							freeControllerV2.hidden = true;
						}
					}
				}
			}
		}
		if (allAnimationSteps == null)
		{
			return;
		}
		foreach (AnimationStep allAnimationStep in allAnimationSteps)
		{
			if (!(allAnimationStep.animationParent != null) || !flag || !flag2 || ((!(atom != null) || !(atom == allAnimationStep.containingAtom)) && (!(_isolatedAtom != null) || !(_isolatedAtom == allAnimationStep.containingAtom))))
			{
				continue;
			}
			allAnimationStep.animationParent.draw = true;
			allAnimationStep.animationParent.SetDrawColor(Color.blue);
			if (allAnimationStep.animationParent.containingAtom != null && allAnimationStep.animationParent.containingAtom.freeControllers != null)
			{
				FreeControllerV3[] freeControllers3 = allAnimationStep.animationParent.containingAtom.freeControllers;
				foreach (FreeControllerV3 freeControllerV3 in freeControllers3)
				{
					if (gameMode == GameMode.Edit || freeControllerV3.interactableInPlayMode)
					{
						freeControllerV3.hidden = false;
					}
					else
					{
						freeControllerV3.hidden = true;
					}
				}
			}
			AnimationStep[] steps2 = allAnimationStep.animationParent.steps;
			foreach (AnimationStep animationStep2 in steps2)
			{
				if (!(animationStep2.containingAtom != null) || animationStep2.containingAtom.freeControllers == null)
				{
					continue;
				}
				FreeControllerV3[] freeControllers4 = animationStep2.containingAtom.freeControllers;
				foreach (FreeControllerV3 freeControllerV4 in freeControllers4)
				{
					if (gameMode == GameMode.Edit || freeControllerV4.interactableInPlayMode)
					{
						freeControllerV4.hidden = false;
					}
					else
					{
						freeControllerV4.hidden = true;
					}
				}
			}
		}
	}

	protected void SetSelectionHUDHeader(SelectionHUD sh, string txt)
	{
		if (sh != null && sh.headerText != null)
		{
			sh.headerText.text = txt;
		}
	}

	private void ResetSelectionInstances()
	{
		if (selectionInstances != null)
		{
			foreach (Transform selectionInstance in selectionInstances)
			{
				if (selectionInstance != null)
				{
					UnityEngine.Object.Destroy(selectionInstance.gameObject);
				}
			}
		}
		if (selectionInstances == null)
		{
			selectionInstances = new List<Transform>();
		}
		else
		{
			selectionInstances.Clear();
		}
		highlightedSelectTargetsLook = null;
		highlightedSelectTargetsLeft = null;
		highlightedSelectTargetsRight = null;
		highlightedSelectTargetsMouse = null;
	}

	private void SelectModeCommon(string helpT, string selectionText = "", bool setSelectHUDActive = true, bool resetSelectionInstances = true, bool clearSelection = true, bool clearSelectionHUDs = true)
	{
		helpText = helpT;
		if (resetSelectionInstances)
		{
			ResetSelectionInstances();
		}
		if (clearSelection)
		{
			ClearSelection();
		}
		if (clearSelectionHUDs)
		{
			ClearSelectionHUDs();
		}
		if (setSelectHUDActive)
		{
			SetSelectionHUDHeader(selectionHUD, selectionText);
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(true);
			}
		}
		if (hiResScreenshotPreview != null)
		{
			hiResScreenshotPreview.gameObject.SetActive(false);
		}
		if (hiResScreenshotCamera != null)
		{
			hiResScreenshotCamera.enabled = false;
		}
		if (selectMode == SelectMode.SaveScreenshot)
		{
			ProcessSaveScreenshot(true);
		}
	}

	public void SelectModeControllers(SelectControllerCallback scc)
	{
		if (selectMode != SelectMode.Controller)
		{
			SelectModeCommon("Press Select to select Controller. Press Remote Grab to cancel.", "Select Controller", true, true, false);
			selectMode = SelectMode.Controller;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						if (allController.deselectedMesh != null)
						{
							component.mesh = allController.deselectedMesh;
							component.meshScale = allController.deselectedMeshScale;
						}
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
			selectControllerCallback = scc;
		}
		SyncVisibility();
	}

	public void SelectModeForceProducers(SelectForceProducerCallback sfpc)
	{
		if (selectMode != SelectMode.ForceProducer)
		{
			SelectModeCommon("Press Select to select Force Producer. Press Remote Grab to cancel.", "Select Force Producer", true, true, false);
			selectMode = SelectMode.ForceProducer;
			if (selectPrefab != null)
			{
				foreach (string key in fpMap.Keys)
				{
					ForceProducerV2 forceProducerV = ProducerNameToForceProducer(key);
					if (forceProducerV != null)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = key;
						}
						transform.parent = forceProducerV.transform;
						transform.position = forceProducerV.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectForceProducerCallback = sfpc;
		}
		SyncVisibility();
	}

	public void SelectModeForceReceivers(SelectForceReceiverCallback sfrc)
	{
		if (selectMode != SelectMode.ForceReceiver)
		{
			SelectModeCommon("Press Select to select Force Receiver. Press Remote Grab to cancel.", "Select Force Receiver", true, true, false);
			selectMode = SelectMode.ForceReceiver;
			if (selectPrefab != null)
			{
				foreach (string key in frMap.Keys)
				{
					ForceReceiver forceReceiver = ReceiverNameToForceReceiver(key);
					if (forceReceiver != null && !forceReceiver.skipUIDrawing)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = key;
						}
						transform.parent = forceReceiver.transform;
						transform.position = forceReceiver.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectForceReceiverCallback = sfrc;
		}
		SyncVisibility();
	}

	public void SelectModeRigidbody(SelectRigidbodyCallback srbc)
	{
		if (selectMode != SelectMode.Rigidbody)
		{
			SelectModeCommon("Press Select to select Physics Object. Press Remote Grab to cancel.", "Select Physics Object", true, true, false);
			selectMode = SelectMode.Rigidbody;
			if (selectPrefab != null)
			{
				foreach (string key in rbMap.Keys)
				{
					Rigidbody rigidbody = RigidbodyNameToRigidbody(key);
					if (!(rigidbody != null))
					{
						continue;
					}
					ForceReceiver component = rigidbody.GetComponent<ForceReceiver>();
					if (component == null || !component.skipUIDrawing)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component2 = transform.GetComponent<SelectTarget>();
						if (component2 != null)
						{
							component2.selectionName = key;
						}
						transform.parent = rigidbody.transform;
						transform.position = rigidbody.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectRigidbodyCallback = srbc;
		}
		SyncVisibility();
	}

	public void SelectModeAtom(SelectAtomCallback sac)
	{
		if (selectMode != SelectMode.Atom)
		{
			SelectModeCommon("Press Select to select Atom. Press either Remote Grab to cancel.", "Select Atom", true, true, false);
			selectMode = SelectMode.Atom;
			if (selectPrefab != null)
			{
				foreach (string atomUID in atomUIDs)
				{
					Atom atomByUid = GetAtomByUid(atomUID);
					if (!(atomByUid != null))
					{
						continue;
					}
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = atomUID;
						if (atomByUid.mainController != null && atomByUid.mainController.deselectedMesh != null)
						{
							component.mesh = atomByUid.mainController.deselectedMesh;
							component.meshScale = atomByUid.mainController.deselectedMeshScale;
						}
					}
					if (atomByUid.childAtomContainer != null)
					{
						transform.parent = atomByUid.childAtomContainer;
						transform.position = atomByUid.childAtomContainer.position;
					}
					else
					{
						transform.parent = atomByUid.transform;
						transform.position = atomByUid.transform.position;
					}
					selectionInstances.Add(transform);
				}
			}
			selectAtomCallback = sac;
		}
		SyncVisibility();
	}

	public void SelectModePossess(bool excludeHeadClear = false)
	{
		if (selectMode != SelectMode.Possess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to possess. Press Select when complete, or press Remote Grab to cancel possess mode.", string.Empty, false);
			ClearPossess(excludeHeadClear);
			selectMode = SelectMode.Possess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (!allController.possessable || (!allController.canGrabPosition && !allController.canGrabRotation))
					{
						continue;
					}
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						if (allController.deselectedMesh != null)
						{
							component.mesh = allController.deselectedMesh;
							component.meshScale = allController.deselectedMeshScale;
						}
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeTwoStagePossess()
	{
		if (selectMode != SelectMode.TwoStagePossess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to select nodes to be possessed. Once all controls are selected, align motion controllers to desired possess from point. Press Select to lock in possess, or press Remote Grab to cancel.", string.Empty, false);
			ClearPossess();
			selectMode = SelectMode.TwoStagePossess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (!allController.possessable || (!allController.canGrabPosition && !allController.canGrabRotation))
					{
						continue;
					}
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						if (allController.deselectedMesh != null)
						{
							component.mesh = allController.deselectedMesh;
							component.meshScale = allController.deselectedMeshScale;
						}
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeTwoStagePossessNoClear()
	{
		if (selectMode != SelectMode.TwoStagePossess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to select nodes to be possessed. Once all controls are selected, align motion controllers to desired possess from point. Press Select to lock in possess, or press Remote Grab to cancel.", string.Empty, false);
			selectMode = SelectMode.TwoStagePossess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (!allController.possessable || (!allController.canGrabPosition && !allController.canGrabRotation))
					{
						continue;
					}
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						if (allController.deselectedMesh != null)
						{
							component.mesh = allController.deselectedMesh;
							component.meshScale = allController.deselectedMeshScale;
						}
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeUnpossess()
	{
		if (selectMode == SelectMode.Unpossess)
		{
			return;
		}
		SelectModeCommon("Point at controller you would like to unpossess and press Select to unpossess. Press Remote Grab to when finished.", string.Empty);
		selectMode = SelectMode.Unpossess;
		if (!(selectPrefab != null))
		{
			return;
		}
		foreach (FreeControllerV3 allController in allControllers)
		{
			if (!allController.possessed)
			{
				continue;
			}
			Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
			SelectTarget component = transform.GetComponent<SelectTarget>();
			if (component != null)
			{
				component.selectionName = allController.containingAtom.uid + ":" + allController.name;
				if (allController.deselectedMesh != null)
				{
					component.mesh = allController.deselectedMesh;
					component.meshScale = allController.deselectedMeshScale;
				}
			}
			transform.parent = allController.transform;
			transform.position = allController.transform.position;
			selectionInstances.Add(transform);
		}
	}

	public void SelectModePossessAndAlign()
	{
		if (selectMode != SelectMode.PossessAndAlign)
		{
			SelectModeCommon("Press Select to select which controller to move head into, align to, and possess.", string.Empty, false);
			ClearPossess();
			selectMode = SelectMode.PossessAndAlign;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (!allController.possessable || (!allController.canGrabPosition && !allController.canGrabRotation))
					{
						continue;
					}
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						if (allController.deselectedMesh != null)
						{
							component.mesh = allController.deselectedMesh;
							component.meshScale = allController.deselectedMeshScale;
						}
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeAnimationRecord(MotionAnimationMaster animationMaster)
	{
		StopRecording();
		currentAnimationMaster = animationMaster;
		if (selectMode != SelectMode.AnimationRecord)
		{
			SelectModeCommon("Press Select or Spacebar to start recording", string.Empty, false);
			selectMode = SelectMode.AnimationRecord;
		}
		SyncVisibility();
	}

	public void SelectModeAnimationRecord()
	{
		SelectModeAnimationRecord(motionAnimationMaster);
	}

	protected bool CheckIfControllerLinkedToMotionControl(Transform t, FreeControllerV3 fc)
	{
		Rigidbody component = t.GetComponent<Rigidbody>();
		if (component != null && fc.linkToRB == component)
		{
			return true;
		}
		return false;
	}

	public void SelectModeArmedForRecord(IEnumerable<MotionAnimationControl> macsToChooseFrom)
	{
		SelectModeCommon("Press Select to toggle which controllers are armed for record. Green=Armed Red=Not Armed. Press Remote Grab when done.", string.Empty, false, true, false);
		selectMode = SelectMode.ArmedForRecord;
		if (!(selectPrefab != null))
		{
			return;
		}
		foreach (MotionAnimationControl item in macsToChooseFrom)
		{
			Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
			SelectTarget component = transform.GetComponent<SelectTarget>();
			if (component != null)
			{
				component.selectionName = item.containingAtom.uid + ":" + item.name;
				if (item.controller != null && item.controller.deselectedMesh != null)
				{
					component.mesh = item.controller.deselectedMesh;
					component.meshScale = item.controller.deselectedMeshScale;
				}
				if (item.armedForRecord)
				{
					component.SetColor(Color.green);
				}
				else
				{
					component.SetColor(Color.red);
				}
			}
			transform.parent = item.transform;
			transform.position = item.transform.position;
			selectionInstances.Add(transform);
		}
	}

	public void SelectModeArmedForRecord()
	{
		if (motionAnimationMaster != null)
		{
			motionAnimationMaster.SelectControllersArmedForRecord();
		}
	}

	public void SelectModeTeleport()
	{
		if (selectMode != SelectMode.Teleport)
		{
			SelectModeCommon("Aim controller and touch Select to choose where to teleport to. Press Select to teleport or press Remote Grab to cancel teleport mode.", string.Empty, false, true, false);
			ClearPossess();
			selectMode = SelectMode.Teleport;
		}
		SyncVisibility();
	}

	public void SelectModeFreeMove()
	{
		if (worldUIActivated)
		{
			return;
		}
		if (selectMode != SelectMode.FreeMove)
		{
			string helpT;
			if (isOVR)
			{
				helpT = "Use left and right thumbsticks to move freely. Press Remote Grab to cancel free-move mode.";
			}
			else if (isOpenVR)
			{
				string localizedOrigin = freeModeMoveAction.GetLocalizedOrigin(SteamVR_Input_Sources.LeftHand);
				string localizedOrigin2 = freeModeMoveAction.GetLocalizedOrigin(SteamVR_Input_Sources.RightHand);
				helpT = "Use " + localizedOrigin + " and " + localizedOrigin2 + " to move freely. Press Grab to cancel free-move mode";
			}
			else
			{
				helpT = string.Empty;
			}
			SelectModeCommon(helpT, string.Empty, false, true, false);
			selectMode = SelectMode.FreeMove;
		}
		SyncVisibility();
	}

	public void ToggleModeFreeMoveMouse()
	{
		if (!worldUIActivated && MonitorRigActive)
		{
			if (selectMode == SelectMode.FreeMoveMouse)
			{
				SelectModeOff();
			}
			else if (selectMode != SelectMode.FreeMoveMouse)
			{
				SelectModeCommon(string.Empty, string.Empty, false, true, false);
				selectMode = SelectMode.FreeMoveMouse;
			}
			SyncVisibility();
		}
	}

	public void SelectModeFreeMoveMouse()
	{
		if (!worldUIActivated && MonitorRigActive)
		{
			if (selectMode != SelectMode.FreeMoveMouse)
			{
				SelectModeCommon(string.Empty, string.Empty, false, true, false);
				selectMode = SelectMode.FreeMoveMouse;
			}
			SyncVisibility();
		}
	}

	public void SelectModeScreenshot()
	{
		if (selectMode != SelectMode.Screenshot)
		{
			SelectModeCommon("Look where you want to take a screenshot and press Select. Press Remote Grab to cancel screenshot mode.", string.Empty, false, true, false);
			ClearPossess();
			selectMode = SelectMode.Screenshot;
			HideMainHUD();
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = true;
				if (hiResScreenShotCameraFOVSlider != null)
				{
					hiResScreenshotCamera.fieldOfView = hiResScreenShotCameraFOVSlider.value;
				}
				else
				{
					hiResScreenshotCamera.fieldOfView = 40f;
				}
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(true);
			}
		}
		SyncVisibility();
	}

	public void SelectModeCustom(string helpText)
	{
		if (selectMode != SelectMode.Custom)
		{
			SelectModeCommon(helpText, string.Empty, false, true, false);
			selectMode = SelectMode.Custom;
		}
		SyncVisibility();
	}

	public void SelectModeCustomWithTargetControl(string helpText)
	{
		if (selectMode != SelectMode.CustomWithTargetControl)
		{
			SelectModeCommon(helpText, string.Empty, false, true, false);
			selectMode = SelectMode.CustomWithTargetControl;
		}
		SyncVisibility();
	}

	public void SelectModeCustomWithVRTargetControl(string helpText)
	{
		if (selectMode != SelectMode.CustomWithVRTargetControl)
		{
			SelectModeCommon(helpText, string.Empty, false, true, false);
			selectMode = SelectMode.CustomWithVRTargetControl;
		}
		SyncVisibility();
	}

	public void SelectModeOff()
	{
		if (selectMode != 0)
		{
			ResetSelectionInstances();
			selectMode = SelectMode.Off;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(false);
			}
			helpText = string.Empty;
			helpColor = Color.white;
			_pointerModeLeft = false;
			_pointerModeRight = false;
			if (screenshotPreview != null)
			{
				screenshotPreview.gameObject.SetActive(false);
			}
			if (screenshotCamera != null)
			{
				screenshotCamera.enabled = false;
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	private void SelectModeTargets()
	{
		if (selectMode != SelectMode.Targets)
		{
			ResetSelectionInstances();
			selectMode = SelectMode.Targets;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(false);
			}
			helpText = string.Empty;
			helpColor = Color.white;
			_pointerModeLeft = true;
			_pointerModeRight = true;
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	private void SelectModeFiltered()
	{
		if (selectMode != SelectMode.FilteredTargets)
		{
			ResetSelectionInstances();
			selectMode = SelectMode.FilteredTargets;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(false);
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	public bool GetMenuShow()
	{
		if (UserPreferences.singleton != null && UserPreferences.singleton.firstTimeUser)
		{
			return false;
		}
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch) || OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetStateDown(SteamVR_Input_Sources.Any);
		}
		return false;
	}

	public bool GetMenuMoveLeft()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Button.Four, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetMenuMoveRight()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportStart(bool inTeleportMode = false)
	{
		return GetTeleportStartLeft(inTeleportMode) || GetTeleportStartRight(inTeleportMode);
	}

	public bool GetTeleportStartLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.GetDown(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportStartRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.GetDown(OVRInput.Touch.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportShow(bool inTeleportMode = false)
	{
		return GetTeleportShowLeft(inTeleportMode) || GetTeleportShowRight(inTeleportMode);
	}

	public bool GetTeleportShowLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.Get(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportShowRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportFinish(bool inTeleportMode = false)
	{
		return GetTeleportFinishLeft(inTeleportMode) || GetTeleportFinishRight(inTeleportMode);
	}

	public bool GetTeleportFinishLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.GetUp(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportFinishRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetGrabNavigateStartLeft()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			return grabNavigateAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetGrabNavigateStartRight()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			return grabNavigateAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetGrabNavigateLeft()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabNavigateAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetGrabNavigateRight()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabNavigateAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	private void CheckSwapAxis()
	{
		if (isOVR)
		{
			if (oculusThumbstickFunction != 0 && (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch) || OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch)))
			{
				_swapAxis = !_swapAxis;
			}
		}
		else if (isOpenVR && swapFreeMoveAxis.GetStateDown(SteamVR_Input_Sources.Any))
		{
			_swapAxis = !_swapAxis;
		}
	}

	public Vector4 GetFreeNavigateVector(SteamVR_Action_Vector2 moveAction, bool ignoreDisable = false)
	{
		Vector4 result = default(Vector4);
		result.x = 0f;
		result.y = 0f;
		result.z = 0f;
		result.w = 0f;
		if (isOVR)
		{
			if (ignoreDisable || !UserPreferences.singleton.oculusDisableFreeMove)
			{
				JoystickControl.Axis axis = navigationForwardAxis;
				JoystickControl.Axis axis2 = navigationSideAxis;
				JoystickControl.Axis axis3 = navigationUpAxis;
				JoystickControl.Axis axis4 = navigationTurnAxis;
				if (_swapAxis)
				{
					axis = navigationUpAxis;
					axis2 = navigationTurnAxis;
					axis3 = navigationForwardAxis;
					axis4 = navigationSideAxis;
				}
				if (axis != 0)
				{
					if (invertNavigationForwardAxis)
					{
						result.y = 0f - JoystickControl.GetAxis(axis);
					}
					else
					{
						result.y = JoystickControl.GetAxis(axis);
					}
				}
				if (axis2 != 0)
				{
					if (invertNavigationSideAxis)
					{
						result.x = 0f - JoystickControl.GetAxis(axis2);
					}
					else
					{
						result.x = JoystickControl.GetAxis(axis2);
					}
				}
				if (axis3 != 0)
				{
					if (invertNavigationUpAxis)
					{
						result.w = 0f - JoystickControl.GetAxis(axis3);
					}
					else
					{
						result.w = JoystickControl.GetAxis(axis3);
					}
				}
				if (axis4 != 0)
				{
					if (invertNavigationTurnAxis)
					{
						result.z = 0f - JoystickControl.GetAxis(axis4);
					}
					else
					{
						result.z = JoystickControl.GetAxis(axis4);
					}
				}
			}
		}
		else if (isOpenVR)
		{
			Vector2 axis5 = moveAction.GetAxis(SteamVR_Input_Sources.LeftHand);
			Vector2 axis6 = moveAction.GetAxis(SteamVR_Input_Sources.RightHand);
			if (_swapAxis)
			{
				result.x = axis6.x;
				result.y = axis6.y;
				result.z = axis5.x;
				result.w = axis5.y;
			}
			else
			{
				result.x = axis5.x;
				result.y = axis5.y;
				result.z = axis6.x;
				result.w = axis6.y;
			}
		}
		return result;
	}

	private void HideLeftController()
	{
		if (isOVR && touchObjectLeft != null)
		{
			MeshRenderer[] array = touchObjectLeftMeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = false;
			}
		}
	}

	private void HideRightController()
	{
		if (isOVR && touchObjectRight != null)
		{
			MeshRenderer[] array = touchObjectRightMeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = false;
			}
		}
	}

	private void ShowLeftController()
	{
		if (isOVR && touchObjectLeft != null)
		{
			MeshRenderer[] array = touchObjectLeftMeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = true;
			}
		}
	}

	private void ShowRightController()
	{
		if (isOVR && touchObjectRight != null)
		{
			MeshRenderer[] array = touchObjectRightMeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = true;
			}
		}
	}

	private void ProcessGUIInteract()
	{
		if (isOVR)
		{
			bool down = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
			if (GUIhitRight && down)
			{
				rightGUIInteract = true;
			}
			bool down2 = OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
			if (GUIhitLeft && down2)
			{
				leftGUIInteract = true;
			}
			if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.Touch) && rightGUIInteract)
			{
				rightGUIInteract = false;
			}
			if (OVRInput.GetUp(OVRInput.Button.Three, OVRInput.Controller.Touch) && leftGUIInteract)
			{
				leftGUIInteract = false;
			}
		}
		else if (isOpenVR)
		{
			bool stateDown = UIInteractAction.GetStateDown(SteamVR_Input_Sources.RightHand);
			if (GUIhitRight && stateDown)
			{
				rightGUIInteract = true;
			}
			bool stateDown2 = UIInteractAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
			if (GUIhitLeft && stateDown2)
			{
				leftGUIInteract = true;
			}
			if (UIInteractAction.GetStateUp(SteamVR_Input_Sources.RightHand) && rightGUIInteract)
			{
				rightGUIInteract = false;
			}
			if (UIInteractAction.GetStateUp(SteamVR_Input_Sources.LeftHand) && leftGUIInteract)
			{
				leftGUIInteract = false;
			}
		}
	}

	public bool GetTargetShow()
	{
		if (isOVR)
		{
			bool flag = !rightGUIInteract && OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
			bool flag2 = !leftGUIInteract && OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			return flag || flag2 || targetsOnWithButton;
		}
		if (isOpenVR)
		{
			if (!rightGUIInteract && targetShowAction.GetState(SteamVR_Input_Sources.RightHand))
			{
				return true;
			}
			if (!leftGUIInteract && targetShowAction.GetState(SteamVR_Input_Sources.LeftHand))
			{
				return true;
			}
		}
		return targetsOnWithButton;
	}

	public bool GetLeftUIPointerShow()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return targetShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightUIPointerShow()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return targetShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public void SetLeftSelect()
	{
		_setLeftSelect = true;
	}

	public bool GetLeftSelect()
	{
		if (_setLeftSelect)
		{
			_setLeftSelect = false;
			return true;
		}
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return selectAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public void SetRightSelect()
	{
		_setRightSelect = true;
	}

	public bool GetRightSelect()
	{
		if (_setRightSelect)
		{
			_setRightSelect = false;
			return true;
		}
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return selectAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetMouseSelect()
	{
		if (GUIhitMouse || mouseClickUsed)
		{
			return false;
		}
		return Input.GetMouseButtonDown(0);
	}

	public bool GetMouseRelease()
	{
		return Input.GetMouseButtonUp(0);
	}

	public bool GetCancel()
	{
		return GetLeftCancel() || GetRightCancel() || Input.GetKeyDown(KeyCode.Escape);
	}

	public bool GetLeftCancel()
	{
		if (isOVR)
		{
			return GetLeftRemoteGrab();
		}
		if (isOpenVR)
		{
			return GetLeftRemoteGrab();
		}
		return false;
	}

	public bool GetRightCancel()
	{
		if (isOVR)
		{
			return GetRightRemoteGrab();
		}
		if (isOpenVR)
		{
			return GetRightRemoteGrab();
		}
		return false;
	}

	public float GetLeftGrabVal()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabValAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		}
		return 0f;
	}

	public float GetRightGrabVal()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabValAction.GetAxis(SteamVR_Input_Sources.RightHand);
		}
		return 0f;
	}

	public bool GetLeftGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public void DisableRemoteHoldGrab()
	{
		remoteHoldGrabDisabled = true;
	}

	public void EnableRemoteHoldGrab()
	{
		remoteHoldGrabDisabled = false;
	}

	public bool GetLeftRemoteGrab()
	{
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteGrab()
	{
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateUp(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftRemoteGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateUp(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftHoldGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return holdGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightHoldGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return holdGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftRemoteHoldGrab()
	{
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteHoldGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteHoldGrab()
	{
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteHoldGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftToggleHand()
	{
		if (isOVR)
		{
			return (_allowGrabPlusTriggerHandToggle && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch)) || (_allowGrabPlusTriggerHandToggle && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch));
		}
		if (isOpenVR)
		{
			return toggleHandAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightToggleHand()
	{
		if (isOVR)
		{
			return (_allowGrabPlusTriggerHandToggle && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch)) || (_allowGrabPlusTriggerHandToggle && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch));
		}
		if (isOpenVR)
		{
			return toggleHandAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	private void ProcessLookAtTrigger()
	{
		if (_isLoading)
		{
			return;
		}
		castRay.origin = lookCamera.transform.position;
		castRay.direction = lookCamera.transform.forward;
		RaycastHit hitInfo;
		if (Physics.Raycast(castRay, out hitInfo, 50f, lookAtTriggerMask))
		{
			LookAtTrigger component = hitInfo.collider.GetComponent<LookAtTrigger>();
			if (component != null)
			{
				if (currentLookAtTrigger != null)
				{
					if (currentLookAtTrigger != component)
					{
						currentLookAtTrigger.EndLookAt();
						currentLookAtTrigger = component;
						currentLookAtTrigger.StartLookAt();
					}
				}
				else
				{
					currentLookAtTrigger = component;
					currentLookAtTrigger.StartLookAt();
				}
			}
			else if (currentLookAtTrigger != null)
			{
				currentLookAtTrigger.EndLookAt();
				currentLookAtTrigger = null;
			}
		}
		else if (currentLookAtTrigger != null)
		{
			currentLookAtTrigger.EndLookAt();
			currentLookAtTrigger = null;
		}
	}

	private void UnhighlightControllers(List<FreeControllerV3> highlightList)
	{
		foreach (FreeControllerV3 highlight in highlightList)
		{
			highlight.highlighted = false;
		}
		highlightList.Clear();
	}

	private void InitTargets()
	{
		if (selectionHUD != null)
		{
			selectionHUD.gameObject.SetActive(false);
		}
		SyncVisibility();
	}

	private void PrepControllers()
	{
		foreach (FreeControllerV3 allController in allControllers)
		{
			allController.ResetAppliedForces();
		}
	}

	private void ProcessTargetSelectionDoRaycast(SelectionHUD sh, Ray ray, List<FreeControllerV3> hitsList, bool doHighlight = true, bool includeHidden = false, bool setSelectionHUDTransform = true)
	{
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(ray, raycastHits, 50f, targetColliderMask);
		if (num > 0)
		{
			if (wasHitFC == null)
			{
				wasHitFC = new Dictionary<FreeControllerV3, bool>();
			}
			else
			{
				wasHitFC.Clear();
			}
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHits[i];
				FreeControllerV3 freeControllerV = raycastHit.transform.GetComponent<FreeControllerV3>();
				if (freeControllerV == null)
				{
					FreeControllerV3Link component = raycastHit.transform.GetComponent<FreeControllerV3Link>();
					if (component != null)
					{
						freeControllerV = component.linkedController;
					}
				}
				if (freeControllerV != null && !wasHitFC.ContainsKey(freeControllerV) && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!freeControllerV.hidden || !(freeControllerV.containingAtom != null) || (!freeControllerV.containingAtom.hidden && !freeControllerV.containingAtom.tempHidden)) && (!UserPreferences.singleton.hideInactiveTargets || freeControllerV.currentPositionState != FreeControllerV3.PositionState.Off || freeControllerV.currentRotationState != FreeControllerV3.RotationState.Off))
				{
					wasHitFC.Add(freeControllerV, true);
					if (!hitsList.Contains(freeControllerV))
					{
						hitsList.Add(freeControllerV);
					}
				}
			}
			if (wasHitFC.Count == 0 && UserPreferences.singleton.hideInactiveTargets)
			{
				for (int j = 0; j < num; j++)
				{
					RaycastHit raycastHit2 = raycastHits[j];
					FreeControllerV3 freeControllerV2 = raycastHit2.transform.GetComponent<FreeControllerV3>();
					if (freeControllerV2 == null)
					{
						FreeControllerV3Link component2 = raycastHit2.transform.GetComponent<FreeControllerV3Link>();
						if (component2 != null)
						{
							freeControllerV2 = component2.linkedController;
						}
					}
					if (freeControllerV2 != null && !wasHitFC.ContainsKey(freeControllerV2) && (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode) && !freeControllerV2.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV2)) && (!freeControllerV2.hidden || !(freeControllerV2.containingAtom != null) || (!freeControllerV2.containingAtom.hidden && !freeControllerV2.containingAtom.tempHidden)))
					{
						wasHitFC.Add(freeControllerV2, true);
						if (!hitsList.Contains(freeControllerV2))
						{
							hitsList.Add(freeControllerV2);
						}
					}
				}
			}
			FreeControllerV3[] array = hitsList.ToArray();
			FreeControllerV3[] array2 = array;
			foreach (FreeControllerV3 freeControllerV3 in array2)
			{
				if (!wasHitFC.ContainsKey(freeControllerV3))
				{
					freeControllerV3.highlighted = false;
					hitsList.Remove(freeControllerV3);
				}
			}
			if (doHighlight)
			{
				for (int l = 0; l < hitsList.Count; l++)
				{
					FreeControllerV3 freeControllerV4 = hitsList[l];
					if (l == 0)
					{
						freeControllerV4.highlighted = true;
					}
					else
					{
						freeControllerV4.highlighted = false;
					}
				}
			}
			if (sh != null)
			{
				sh.ClearSelections();
				if (hitsList.Count > 0)
				{
					int num2 = 0;
					foreach (FreeControllerV3 hits in hitsList)
					{
						sh.SetSelection(name: (gameMode != GameMode.Play || !setSelectionHUDTransform) ? (hits.containingAtom.uid + ":" + hits.name) : string.Empty, index: num2, selection: hits.transform);
						num2++;
					}
				}
			}
		}
		else
		{
			if (doHighlight)
			{
				foreach (FreeControllerV3 hits2 in hitsList)
				{
					hits2.highlighted = false;
				}
			}
			hitsList.Clear();
		}
		if (!(sh != null))
		{
			return;
		}
		if (hitsList.Count > 0)
		{
			sh.gameObject.SetActive(true);
			if (setSelectionHUDTransform)
			{
				sh.transform.position = hitsList[0].transform.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position, lookCamera.transform.up);
			}
		}
		else
		{
			sh.gameObject.SetActive(false);
		}
	}

	private void AddPositionRotationHandlesToSelectedController()
	{
		if (MonitorCenterCamera != null)
		{
			if (selectedControllerPositionHandle != null)
			{
				selectedControllerPositionHandle.enabled = _mainHUDVisible;
				selectedControllerPositionHandle.controller = selectedController;
			}
			if (selectedControllerRotationHandle != null)
			{
				selectedControllerRotationHandle.enabled = _mainHUDVisible;
				selectedControllerRotationHandle.controller = selectedController;
			}
		}
	}

	private bool ProcessTargetSelectionDoSelect(List<FreeControllerV3> highlightedControllers)
	{
		bool result = false;
		if (highlightedControllers.Count > 0)
		{
			FreeControllerV3 freeControllerV = highlightedControllers[0];
			highlightedControllers.RemoveAt(0);
			highlightedControllers.Add(freeControllerV);
			if (!(selectedController != null) || !(selectedController == freeControllerV))
			{
				if (selectedController != null)
				{
					ClearSelection();
				}
				freeControllerV.selected = true;
				selectedController = freeControllerV;
				if (selectedController != null && selectedController.containingAtom != null)
				{
					lastCycleSelectAtomUid = selectedController.containingAtom.uid;
					lastCycleSelectAtomType = selectedController.containingAtom.type;
				}
				AddPositionRotationHandlesToSelectedController();
				SyncUIToSelectedController();
				activeUI = ActiveUI.SelectedOptions;
				result = true;
			}
		}
		else
		{
			ClearSelection();
		}
		return result;
	}

	public void ProcessTargetSelectionCycleSelectMouse()
	{
		ProcessTargetSelectionCycleSelect(highlightedControllersMouse);
	}

	private void ProcessTargetSelectionCycleSelect(List<FreeControllerV3> highlightedControllers)
	{
		if (highlightedControllers != null && highlightedControllers.Count > 1)
		{
			FreeControllerV3 item = highlightedControllers[0];
			highlightedControllers.RemoveAt(0);
			highlightedControllers.Add(item);
		}
	}

	private void ProcessTargetSelectionCycleBackwardsSelect(List<FreeControllerV3> highlightedControllers)
	{
		if (highlightedControllers != null && highlightedControllers.Count > 1)
		{
			int index = highlightedControllers.Count - 1;
			FreeControllerV3 item = highlightedControllers[index];
			highlightedControllers.RemoveAt(index);
			highlightedControllers.Insert(0, item);
		}
	}

	public List<FreeControllerV3> GetOverlappingTargets(Transform processFrom, float overlapRadius = 0.01f)
	{
		if (overlappingFcs == null)
		{
			overlappingFcs = new List<FreeControllerV3>();
		}
		else
		{
			overlappingFcs.Clear();
		}
		AllocateOverlappingControls();
		int num = Physics.OverlapSphereNonAlloc(processFrom.position, overlapRadius * _worldScale, overlappingControls, targetColliderMask);
		if (num > 0)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				Collider collider = overlappingControls[i];
				FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
				if (component != null && (gameMode == GameMode.Edit || component.interactableInPlayMode) && !component.possessed && (component.currentPositionState == FreeControllerV3.PositionState.On || component.currentRotationState == FreeControllerV3.RotationState.On) && (onlyShowControllers == null || onlyShowControllers.Contains(component)) && (!(component.containingAtom != null) || (!component.containingAtom.hidden && !component.containingAtom.tempHidden)))
				{
					flag = true;
					if (!overlappingFcs.Contains(component))
					{
						overlappingFcs.Add(component);
					}
					float distanceHolder = Vector3.SqrMagnitude(processFrom.position - collider.transform.position);
					component.distanceHolder = distanceHolder;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < num; j++)
				{
					Collider collider2 = overlappingControls[j];
					FreeControllerV3 freeControllerV = null;
					FreeControllerV3Link component2 = collider2.GetComponent<FreeControllerV3Link>();
					if (component2 != null)
					{
						freeControllerV = component2.linkedController;
					}
					if (freeControllerV != null && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && (freeControllerV.currentPositionState == FreeControllerV3.PositionState.On || freeControllerV.currentRotationState == FreeControllerV3.RotationState.On) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!(freeControllerV.containingAtom != null) || (!freeControllerV.containingAtom.hidden && !freeControllerV.containingAtom.tempHidden)))
					{
						if (!overlappingFcs.Contains(freeControllerV))
						{
							overlappingFcs.Add(freeControllerV);
						}
						float distanceHolder2 = Vector3.SqrMagnitude(processFrom.position - collider2.transform.position);
						freeControllerV.distanceHolder = distanceHolder2;
					}
				}
			}
			if (!flag)
			{
				for (int k = 0; k < num; k++)
				{
					Collider collider3 = overlappingControls[k];
					FreeControllerV3 component3 = collider3.GetComponent<FreeControllerV3>();
					if (component3 != null && (gameMode == GameMode.Edit || component3.interactableInPlayMode) && !component3.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(component3)) && (!component3.hidden || !(component3.containingAtom != null) || (!component3.containingAtom.hidden && !component3.containingAtom.tempHidden)))
					{
						flag = true;
						if (!overlappingFcs.Contains(component3))
						{
							overlappingFcs.Add(component3);
						}
						float distanceHolder3 = Vector3.SqrMagnitude(processFrom.position - collider3.transform.position);
						component3.distanceHolder = distanceHolder3;
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < num; l++)
				{
					Collider collider4 = overlappingControls[l];
					FreeControllerV3 freeControllerV2 = null;
					FreeControllerV3Link component4 = collider4.GetComponent<FreeControllerV3Link>();
					if (component4 != null)
					{
						freeControllerV2 = component4.linkedController;
					}
					if (freeControllerV2 != null && (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode) && !freeControllerV2.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV2)) && (!freeControllerV2.hidden || !(freeControllerV2.containingAtom != null) || (!freeControllerV2.containingAtom.hidden && !freeControllerV2.containingAtom.tempHidden)))
					{
						if (!overlappingFcs.Contains(freeControllerV2))
						{
							overlappingFcs.Add(freeControllerV2);
						}
						float distanceHolder4 = Vector3.SqrMagnitude(processFrom.position - collider4.transform.position);
						freeControllerV2.distanceHolder = distanceHolder4;
					}
				}
			}
		}
		return overlappingFcs;
	}

	protected void AllocateOverlappingControls()
	{
		if (overlappingControls == null)
		{
			overlappingControls = new Collider[256];
		}
	}

	public bool ProcessControllerTargetHighlight(SelectionHUD sh, Transform processFromPointer, Transform processFromOverlap, bool ptrMode, List<FreeControllerV3> highlightedControllers, bool uihit, FreeControllerV3 excludeController, out bool isOverlap, float overlapRadius = 0.03f)
	{
		if (overlappingFcs == null)
		{
			overlappingFcs = new List<FreeControllerV3>();
		}
		else
		{
			overlappingFcs.Clear();
		}
		bool result = false;
		AllocateOverlappingControls();
		int num = Physics.OverlapSphereNonAlloc(processFromOverlap.position, overlapRadius * _worldScale, overlappingControls, targetColliderMask);
		if (num > 0)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				Collider collider = overlappingControls[i];
				FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
				if (component != null && (gameMode == GameMode.Edit || component.interactableInPlayMode) && !component.possessed && component.hidden && (component.currentPositionState == FreeControllerV3.PositionState.On || component.currentRotationState == FreeControllerV3.RotationState.On) && (!(excludeController != null) || !(component == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(component)) && (!(component.containingAtom != null) || (!component.containingAtom.hidden && !component.containingAtom.tempHidden)))
				{
					flag = true;
					if (!overlappingFcs.Contains(component))
					{
						overlappingFcs.Add(component);
					}
					float distanceHolder = Vector3.SqrMagnitude(processFromOverlap.position - collider.transform.position);
					component.distanceHolder = distanceHolder;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < num; j++)
				{
					Collider collider2 = overlappingControls[j];
					FreeControllerV3 freeControllerV = null;
					FreeControllerV3Link component2 = collider2.GetComponent<FreeControllerV3Link>();
					if (component2 != null)
					{
						freeControllerV = component2.linkedController;
					}
					if (freeControllerV != null && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && freeControllerV.hidden && (freeControllerV.currentPositionState == FreeControllerV3.PositionState.On || freeControllerV.currentRotationState == FreeControllerV3.RotationState.On) && (!(excludeController != null) || !(freeControllerV == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!(freeControllerV.containingAtom != null) || (!freeControllerV.containingAtom.hidden && !freeControllerV.containingAtom.tempHidden)))
					{
						if (!overlappingFcs.Contains(freeControllerV))
						{
							overlappingFcs.Add(freeControllerV);
						}
						float distanceHolder2 = Vector3.SqrMagnitude(processFromOverlap.position - collider2.transform.position);
						freeControllerV.distanceHolder = distanceHolder2;
					}
				}
			}
			if (!flag)
			{
				for (int k = 0; k < num; k++)
				{
					Collider collider3 = overlappingControls[k];
					FreeControllerV3 component3 = collider3.GetComponent<FreeControllerV3>();
					if (component3 != null && (gameMode == GameMode.Edit || component3.interactableInPlayMode) && !component3.possessed && (!(excludeController != null) || !(component3 == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(component3)) && (!component3.hidden || !(component3.containingAtom != null) || (!component3.containingAtom.hidden && !component3.containingAtom.tempHidden)))
					{
						flag = true;
						if (!overlappingFcs.Contains(component3))
						{
							overlappingFcs.Add(component3);
						}
						float distanceHolder3 = Vector3.SqrMagnitude(processFromOverlap.position - collider3.transform.position);
						component3.distanceHolder = distanceHolder3;
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < num; l++)
				{
					Collider collider4 = overlappingControls[l];
					FreeControllerV3 freeControllerV2 = null;
					FreeControllerV3Link component4 = collider4.GetComponent<FreeControllerV3Link>();
					if (component4 != null)
					{
						freeControllerV2 = component4.linkedController;
					}
					if (freeControllerV2 != null && (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode) && !freeControllerV2.possessed && (!(excludeController != null) || !(freeControllerV2 == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV2)) && (!freeControllerV2.hidden || !(freeControllerV2.containingAtom != null) || (!freeControllerV2.containingAtom.hidden && !freeControllerV2.containingAtom.tempHidden)))
					{
						if (!overlappingFcs.Contains(freeControllerV2))
						{
							overlappingFcs.Add(freeControllerV2);
						}
						float distanceHolder4 = Vector3.SqrMagnitude(processFromOverlap.position - collider4.transform.position);
						freeControllerV2.distanceHolder = distanceHolder4;
					}
				}
			}
		}
		if (sh != null)
		{
			sh.ClearSelections();
			if (overlappingFcs.Count > 0)
			{
				highlightedControllers.Clear();
				if (alreadyDisplayed == null)
				{
					alreadyDisplayed = new List<FreeControllerV3>();
				}
				else
				{
					alreadyDisplayed.Clear();
				}
				overlappingFcs.Sort((FreeControllerV3 c1, FreeControllerV3 c2) => c1.distanceHolder.CompareTo(c2.distanceHolder));
				if (gameMode == GameMode.Edit)
				{
					sh.gameObject.SetActive(true);
				}
				else
				{
					sh.gameObject.SetActive(false);
				}
				sh.useDrawFromPosition = true;
				sh.drawFrom = processFromOverlap.position;
				int num2 = 0;
				foreach (FreeControllerV3 overlappingFc in overlappingFcs)
				{
					if (!alreadyDisplayed.Contains(overlappingFc))
					{
						if (num2 == 0)
						{
							highlightedControllers.Add(overlappingFc);
						}
						sh.SetSelection(name: (gameMode != GameMode.Play) ? (overlappingFc.containingAtom.uid + ":" + overlappingFc.name) : string.Empty, index: num2, selection: overlappingFc.transform);
						num2++;
						alreadyDisplayed.Add(overlappingFc);
					}
				}
				sh.transform.position = processFromOverlap.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position, lookCamera.transform.up);
			}
			else if (!ptrMode)
			{
				highlightedControllers.Clear();
				sh.gameObject.SetActive(false);
			}
		}
		if ((overlappingFcs.Count == 0 && ptrMode) || pointersAlwaysEnabled)
		{
			result = !MonitorRigActive;
		}
		isOverlap = overlappingFcs.Count != 0;
		if (overlappingFcs.Count == 0 && ptrMode)
		{
			castRay.origin = processFromPointer.position;
			castRay.direction = processFromPointer.forward;
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFromPointer.position;
			if (!uihit)
			{
				ProcessTargetSelectionDoRaycast(sh, castRay, highlightedControllers, false);
			}
		}
		return result;
	}

	private void ClearAllGrabbedControllers()
	{
		if (leftFullGrabbedController != null)
		{
			leftFullGrabbedController.isGrabbing = false;
			leftFullGrabbedController.RestorePreLinkState();
			leftFullGrabbedController = null;
		}
		if (leftGrabbedController != null)
		{
			leftGrabbedController.isGrabbing = false;
			leftGrabbedController.RestorePreLinkState();
			leftGrabbedController = null;
		}
		if (rightFullGrabbedController != null)
		{
			rightFullGrabbedController.isGrabbing = false;
			rightFullGrabbedController.RestorePreLinkState();
			rightFullGrabbedController = null;
		}
		if (rightGrabbedController != null)
		{
			rightGrabbedController.isGrabbing = false;
			rightGrabbedController.RestorePreLinkState();
			rightGrabbedController = null;
		}
		if (grabbedControllerMouse != null)
		{
			grabbedControllerMouse.isGrabbing = false;
			grabbedControllerMouse.RestorePreLinkState();
			grabbedControllerMouse = null;
		}
	}

	private bool ProcessTargetSelectionDoGrabRight(Transform rightControl, bool isRemote)
	{
		bool result = false;
		if ((bool)rightGrabbedController)
		{
			rightGrabbedController.RestorePreLinkState();
			rightGrabbedController.isGrabbing = false;
			rightGrabbedController = null;
		}
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersRight.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersRight[i];
			if ((freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation) && (!(rightFullGrabbedController != null) || !(rightFullGrabbedController == freeControllerV2)))
			{
				freeControllerV = freeControllerV2;
				highlightedControllersRight.RemoveAt(i);
				highlightedControllersRight.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null)
		{
			Rigidbody component = rightControl.GetComponent<Rigidbody>();
			rightGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (leftFullGrabbedController == rightGrabbedController)
			{
				leftFullGrabbedController.RestorePreLinkState();
				leftFullGrabbedController = null;
				leftHandControl = null;
			}
			if (leftGrabbedController == rightGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (rightGrabbedController.canGrabPosition)
				{
					if (rightGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (rightGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					rightGrabbedController.isGrabbing = true;
					rightGrabbedControllerIsRemote = isRemote;
					if (rightFullGrabbedController != null)
					{
						Rigidbody followWhenOffRB = rightFullGrabbedController.followWhenOffRB;
						if (followWhenOffRB != null)
						{
							rightGrabbedController.SelectLinkToRigidbody(followWhenOffRB, linkState, true);
						}
						else
						{
							rightGrabbedController.SelectLinkToRigidbody(component, linkState);
						}
					}
					else
					{
						rightGrabbedController.SelectLinkToRigidbody(component, linkState);
					}
				}
			}
		}
		return result;
	}

	private bool ProcessTargetSelectionDoFullGrabRight(Transform rightControl, bool isRemote, bool isRemoteGrab)
	{
		bool result = false;
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersRight.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersRight[i];
			if (freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation)
			{
				freeControllerV = freeControllerV2;
				highlightedControllersRight.RemoveAt(i);
				highlightedControllersRight.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null && ((isRemote && isRemoteGrab) || (!isRemote && !isRemoteGrab)))
		{
			Rigidbody component = rightControl.GetComponent<Rigidbody>();
			rightFullGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (leftFullGrabbedController == rightFullGrabbedController)
			{
				leftFullGrabbedController.RestorePreLinkState();
				leftFullGrabbedController = null;
				if (leftHandControl != null)
				{
					leftHandControl.possessed = false;
				}
				leftHandControl = null;
			}
			if (leftGrabbedController == rightFullGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (rightGrabbedController == rightFullGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (rightFullGrabbedController.canGrabPosition)
				{
					if (rightFullGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (rightFullGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					rightFullGrabbedController.isGrabbing = true;
					rightFullGrabbedControllerIsRemote = isRemote;
					rightFullGrabbedController.SelectLinkToRigidbody(component, linkState);
					rightHandControl = rightFullGrabbedController.GetComponent<HandControl>();
					if (rightHandControl == null)
					{
						HandControlLink component2 = rightFullGrabbedController.GetComponent<HandControlLink>();
						if (component2 != null)
						{
							rightHandControl = component2.handControl;
						}
					}
					if (rightHandControl != null)
					{
						rightHandControl.possessed = true;
					}
				}
			}
		}
		else if (rightGrabbedController != null && ((rightGrabbedControllerIsRemote && isRemoteGrab) || (!rightGrabbedControllerIsRemote && !isRemoteGrab)))
		{
			result = true;
			rightFullGrabbedControllerIsRemote = rightGrabbedControllerIsRemote;
			rightFullGrabbedController = rightGrabbedController;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == rightFullGrabbedController)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			rightHandControl = rightFullGrabbedController.GetComponent<HandControl>();
			if (rightHandControl == null)
			{
				HandControlLink component3 = rightFullGrabbedController.GetComponent<HandControlLink>();
				if (component3 != null)
				{
					rightHandControl = component3.handControl;
				}
			}
			if (rightHandControl != null)
			{
				rightHandControl.possessed = true;
			}
			rightGrabbedController = null;
		}
		return result;
	}

	private bool ProcessTargetSelectionDoGrabLeft(Transform leftControl, bool isRemote)
	{
		bool result = false;
		if ((bool)leftGrabbedController)
		{
			leftGrabbedController.isGrabbing = false;
			leftGrabbedController.RestorePreLinkState();
			leftGrabbedController = null;
		}
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersLeft.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersLeft[i];
			if ((freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation) && (!(leftFullGrabbedController != null) || !(leftFullGrabbedController == freeControllerV2)))
			{
				freeControllerV = freeControllerV2;
				highlightedControllersLeft.RemoveAt(i);
				highlightedControllersLeft.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null)
		{
			Rigidbody component = leftControl.GetComponent<Rigidbody>();
			leftGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (rightFullGrabbedController == leftGrabbedController)
			{
				rightFullGrabbedController.RestorePreLinkState();
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (rightGrabbedController == leftGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (leftGrabbedController.canGrabPosition)
				{
					if (leftGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (leftGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					leftGrabbedControllerIsRemote = isRemote;
					leftGrabbedController.isGrabbing = true;
					if (leftFullGrabbedController != null)
					{
						Rigidbody followWhenOffRB = leftFullGrabbedController.followWhenOffRB;
						if (followWhenOffRB != null)
						{
							leftGrabbedController.SelectLinkToRigidbody(followWhenOffRB, linkState, true);
						}
						else
						{
							leftGrabbedController.SelectLinkToRigidbody(component, linkState);
						}
					}
					else
					{
						leftGrabbedController.SelectLinkToRigidbody(component, linkState);
					}
				}
			}
		}
		return result;
	}

	private bool ProcessTargetSelectionDoFullGrabLeft(Transform leftControl, bool isRemote, bool isRemoteGrab)
	{
		bool result = false;
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersLeft.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersLeft[i];
			if (freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation)
			{
				freeControllerV = freeControllerV2;
				highlightedControllersLeft.RemoveAt(i);
				highlightedControllersLeft.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null && ((isRemote && isRemoteGrab) || (!isRemote && !isRemoteGrab)))
		{
			Rigidbody component = leftControl.GetComponent<Rigidbody>();
			leftFullGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (rightFullGrabbedController == leftFullGrabbedController)
			{
				rightFullGrabbedController.RestorePreLinkState();
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (rightGrabbedController == leftFullGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (leftGrabbedController == leftFullGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (leftFullGrabbedController.canGrabPosition)
				{
					if (leftFullGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (leftFullGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					leftFullGrabbedController.isGrabbing = true;
					leftFullGrabbedControllerIsRemote = isRemote;
					leftFullGrabbedController.SelectLinkToRigidbody(component, linkState);
					leftHandControl = leftFullGrabbedController.GetComponent<HandControl>();
					if (leftHandControl == null)
					{
						HandControlLink component2 = leftFullGrabbedController.GetComponent<HandControlLink>();
						if (component2 != null)
						{
							leftHandControl = component2.handControl;
						}
					}
					if (leftHandControl != null)
					{
						leftHandControl.possessed = true;
					}
				}
			}
		}
		else if (leftGrabbedController != null && ((leftGrabbedControllerIsRemote && isRemoteGrab) || (!leftGrabbedControllerIsRemote && !isRemoteGrab)))
		{
			result = true;
			leftFullGrabbedControllerIsRemote = leftGrabbedControllerIsRemote;
			leftFullGrabbedController = leftGrabbedController;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == leftFullGrabbedController)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			leftGrabbedController = null;
			leftHandControl = leftFullGrabbedController.GetComponent<HandControl>();
			if (leftHandControl == null)
			{
				HandControlLink component3 = leftFullGrabbedController.GetComponent<HandControlLink>();
				if (component3 != null)
				{
					leftHandControl = component3.handControl;
				}
			}
			if (leftHandControl != null)
			{
				leftHandControl.possessed = true;
			}
		}
		return result;
	}

	private void ProcessCycle()
	{
		leftCycleX = 0;
		leftCycleY = 0;
		rightCycleX = 0;
		rightCycleY = 0;
		if (!isOpenVR)
		{
			return;
		}
		Vector2 axis = cycleUsingXAxisAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		Vector2 axis2 = cycleUsingYAxisAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		if (cycleEngageAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
		{
			_leftCycleOn = true;
			_leftCycleXPosition = axis.x;
			_leftCycleYPosition = axis2.y;
		}
		if (cycleEngageAction.GetStateUp(SteamVR_Input_Sources.LeftHand))
		{
			_leftCycleOn = false;
		}
		if (_leftCycleOn)
		{
			float num = axis.x - _leftCycleXPosition;
			if ((num > 0f && num > cycleClick) || (num < 0f && 0f - num > cycleClick))
			{
				leftCycleX = (int)(num / cycleClick);
				_leftCycleXPosition = axis.x;
			}
			float num2 = axis2.y - _leftCycleYPosition;
			if ((num2 > 0f && num2 > cycleClick) || (num2 < 0f && 0f - num2 > cycleClick))
			{
				leftCycleY = (int)(num2 / cycleClick);
				_leftCycleYPosition = axis2.y;
			}
		}
		Vector2 axis3 = cycleUsingXAxisAction.GetAxis(SteamVR_Input_Sources.RightHand);
		Vector2 axis4 = cycleUsingYAxisAction.GetAxis(SteamVR_Input_Sources.RightHand);
		if (cycleEngageAction.GetStateDown(SteamVR_Input_Sources.RightHand))
		{
			_rightCycleOn = true;
			_rightCycleXPosition = axis3.x;
			_rightCycleYPosition = axis4.y;
		}
		if (cycleEngageAction.GetStateUp(SteamVR_Input_Sources.RightHand))
		{
			_rightCycleOn = false;
		}
		if (_rightCycleOn)
		{
			float num3 = axis3.x - _rightCycleXPosition;
			if ((num3 > 0f && num3 > cycleClick) || (num3 < 0f && 0f - num3 > cycleClick))
			{
				rightCycleX = (int)(num3 / cycleClick);
				_rightCycleXPosition = axis3.x;
			}
			float num4 = axis4.y - _rightCycleYPosition;
			if ((num4 > 0f && num4 > cycleClick) || (num4 < 0f && 0f - num4 > cycleClick))
			{
				rightCycleY = (int)(num4 / cycleClick);
				_rightCycleYPosition = axis4.y;
			}
		}
	}

	private void ProcessMotionControllerTargetHighlight()
	{
		if (!isMonitorOnly)
		{
			if (highlightedControllersLeft == null)
			{
				highlightedControllersLeft = new List<FreeControllerV3>();
			}
			if (highlightedControllersRight == null)
			{
				highlightedControllersRight = new List<FreeControllerV3>();
			}
			centerHandLeft.rotation = motionControllerLeft.rotation;
			if (ProcessControllerTargetHighlight(leftSelectionHUD, motionControllerLeft, centerHandLeft, _pointerModeLeft, highlightedControllersLeft, GUIhitLeft, leftFullGrabbedController, out isLeftOverlap))
			{
				drawRayLineLeft = !MonitorRigActive;
			}
			if (leftGrabbedController != null || leftFullGrabbedController != null || leftPossessedController != null)
			{
				HideLeftController();
				leftSelectionHUD.gameObject.SetActive(false);
			}
			else if (_mainHUDVisible || !UserPreferences.singleton.showControllersMenuOnly)
			{
				ShowLeftController();
			}
			else
			{
				HideLeftController();
			}
			centerHandRight.rotation = motionControllerRight.rotation;
			if (ProcessControllerTargetHighlight(rightSelectionHUD, motionControllerRight, centerHandRight, _pointerModeRight, highlightedControllersRight, GUIhitRight, rightFullGrabbedController, out isRightOverlap))
			{
				drawRayLineRight = !MonitorRigActive;
			}
			if (rightGrabbedController != null || rightFullGrabbedController != null || rightPossessedController != null)
			{
				HideRightController();
				rightSelectionHUD.gameObject.SetActive(false);
			}
			else if (_mainHUDVisible || !UserPreferences.singleton.showControllersMenuOnly)
			{
				ShowRightController();
			}
			else
			{
				HideRightController();
			}
		}
	}

	private void ProcessMotionControllerTargetControl(bool canSelect = true)
	{
		if (isMonitorOnly)
		{
			return;
		}
		if (canSelect && !didStartRightNavigate && GetRightSelect())
		{
			ProcessTargetSelectionDoSelect(highlightedControllersRight);
		}
		if (commonHandModelControl != null && !_leapHandRightConnected && GetRightToggleHand())
		{
			commonHandModelControl.ToggleRightHandEnabled();
		}
		else
		{
			if (!isRightOverlap && GetRightRemoteGrab())
			{
				ProcessTargetSelectionDoGrabRight(motionControllerRight, true);
			}
			else if (isRightOverlap && GetRightGrab())
			{
				ProcessTargetSelectionDoGrabRight(motionControllerRight, false);
			}
			bool flag = false;
			if (GetRightHoldGrab())
			{
				if ((bool)rightFullGrabbedController)
				{
					flag = true;
					rightFullGrabbedController.isGrabbing = false;
					rightFullGrabbedController.RestorePreLinkState();
					rightFullGrabbedController = null;
					if (rightHandControl != null)
					{
						rightHandControl.possessed = false;
					}
					rightHandControl = null;
				}
				else
				{
					flag = ProcessTargetSelectionDoFullGrabRight(motionControllerRight, !isRightOverlap, false);
				}
			}
			if (!flag && !remoteHoldGrabDisabled && GetRightRemoteHoldGrab())
			{
				if ((bool)rightFullGrabbedController)
				{
					rightFullGrabbedController.isGrabbing = false;
					rightFullGrabbedController.RestorePreLinkState();
					rightFullGrabbedController = null;
					if (rightHandControl != null)
					{
						rightHandControl.possessed = false;
					}
					rightHandControl = null;
				}
				else
				{
					ProcessTargetSelectionDoFullGrabRight(motionControllerRight, !isRightOverlap, true);
				}
			}
		}
		if (isOpenVR)
		{
			Vector2 axis = pushPullAction.GetAxis(SteamVR_Input_Sources.RightHand);
			if (rightGrabbedController != null && rightGrabbedControllerIsRemote)
			{
				if (!Mathf.Approximately(axis.sqrMagnitude, 0f))
				{
					if (float.IsNaN(axis.y))
					{
						axis.y = 0f;
					}
					axis.y = Mathf.Clamp(axis.y, -100f, 100f);
					rightGrabbedController.MoveLinkConnectorTowards(motionControllerRight, axis.y * 0.05f);
				}
			}
			else if (rightFullGrabbedController != null && rightFullGrabbedControllerIsRemote && !Mathf.Approximately(axis.sqrMagnitude, 0f))
			{
				if (float.IsNaN(axis.y))
				{
					axis.y = 0f;
				}
				axis.y = Mathf.Clamp(axis.y, -100f, 100f);
				rightFullGrabbedController.MoveLinkConnectorTowards(motionControllerRight, axis.y * 0.05f);
			}
		}
		if (!GUIhitRight && highlightedControllersRight.Count > 1)
		{
			if (rightCycleX < 0 || rightCycleY < 0)
			{
				ProcessTargetSelectionCycleBackwardsSelect(highlightedControllersRight);
				float num = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
				hapticAction.Execute(0f, num, 1f / num, 1f, SteamVR_Input_Sources.RightHand);
			}
			else if (rightCycleX > 0 || rightCycleY > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersRight);
				float num2 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
				hapticAction.Execute(0f, num2, 1f / num2, 1f, SteamVR_Input_Sources.RightHand);
			}
		}
		if (canSelect && !didStartLeftNavigate && GetLeftSelect())
		{
			ProcessTargetSelectionDoSelect(highlightedControllersLeft);
		}
		if (commonHandModelControl != null && !_leapHandLeftConnected && GetLeftToggleHand())
		{
			commonHandModelControl.ToggleLeftHandEnabled();
		}
		else
		{
			if (!isLeftOverlap && GetLeftRemoteGrab())
			{
				ProcessTargetSelectionDoGrabLeft(motionControllerLeft, true);
			}
			else if (isLeftOverlap && GetLeftGrab())
			{
				ProcessTargetSelectionDoGrabLeft(motionControllerLeft, false);
			}
			bool flag2 = false;
			if (GetLeftHoldGrab())
			{
				if ((bool)leftFullGrabbedController)
				{
					flag2 = true;
					leftFullGrabbedController.isGrabbing = false;
					leftFullGrabbedController.RestorePreLinkState();
					leftFullGrabbedController = null;
					if (leftHandControl != null)
					{
						leftHandControl.possessed = false;
					}
					leftHandControl = null;
				}
				else
				{
					flag2 = ProcessTargetSelectionDoFullGrabLeft(motionControllerLeft, !isLeftOverlap, false);
				}
			}
			if (!flag2 && !remoteHoldGrabDisabled && GetLeftRemoteHoldGrab())
			{
				if ((bool)leftFullGrabbedController)
				{
					leftFullGrabbedController.isGrabbing = false;
					leftFullGrabbedController.RestorePreLinkState();
					leftFullGrabbedController = null;
					if (leftHandControl != null)
					{
						leftHandControl.possessed = false;
					}
					leftHandControl = null;
				}
				else
				{
					flag2 = ProcessTargetSelectionDoFullGrabLeft(motionControllerLeft, !isLeftOverlap, true);
				}
			}
		}
		if (isOpenVR)
		{
			Vector2 axis2 = pushPullAction.GetAxis(SteamVR_Input_Sources.LeftHand);
			if (leftGrabbedController != null && leftGrabbedControllerIsRemote)
			{
				if (!Mathf.Approximately(axis2.sqrMagnitude, 0f))
				{
					if (float.IsNaN(axis2.y))
					{
						axis2.y = 0f;
					}
					axis2.y = Mathf.Clamp(axis2.y, -100f, 100f);
					leftGrabbedController.MoveLinkConnectorTowards(motionControllerLeft, axis2.y * 0.05f);
				}
			}
			else if (leftFullGrabbedController != null && leftFullGrabbedControllerIsRemote && !Mathf.Approximately(axis2.sqrMagnitude, 0f))
			{
				if (float.IsNaN(axis2.y))
				{
					axis2.y = 0f;
				}
				axis2.y = Mathf.Clamp(axis2.y, -100f, 100f);
				leftFullGrabbedController.MoveLinkConnectorTowards(motionControllerLeft, axis2.y * 0.05f);
			}
		}
		if (!GUIhitLeft && highlightedControllersLeft.Count > 1)
		{
			if (leftCycleX < 0 || leftCycleY < 0)
			{
				ProcessTargetSelectionCycleBackwardsSelect(highlightedControllersLeft);
				float num3 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
				hapticAction.Execute(0f, num3, 1f / num3, 1f, SteamVR_Input_Sources.LeftHand);
			}
			else if (leftCycleX > 0 || leftCycleY > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersLeft);
				float num4 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
				hapticAction.Execute(0f, num4, 1f / num4, 1f, SteamVR_Input_Sources.LeftHand);
			}
		}
		if (((rightGrabbedControllerIsRemote && GetRightRemoteGrabRelease()) || (!rightGrabbedControllerIsRemote && GetRightGrabRelease())) && (bool)rightGrabbedController)
		{
			rightGrabbedController.isGrabbing = false;
			rightGrabbedController.RestorePreLinkState();
			rightGrabbedController = null;
		}
		if (((leftGrabbedControllerIsRemote && GetLeftRemoteGrabRelease()) || (!leftGrabbedControllerIsRemote && GetLeftGrabRelease())) && (bool)leftGrabbedController)
		{
			leftGrabbedController.isGrabbing = false;
			leftGrabbedController.RestorePreLinkState();
			leftGrabbedController = null;
		}
	}

	private void ProcessCommonTargetSelection()
	{
		if (highlightedControllersLook == null)
		{
			highlightedControllersLook = new List<FreeControllerV3>();
		}
		if (!(lookCamera != null) || !useLookSelect)
		{
			return;
		}
		if (GUIhit)
		{
			UnhighlightControllers(highlightedControllersLook);
			if (selectionHUD != null)
			{
				selectionHUD.ClearSelections();
				selectionHUD.gameObject.SetActive(false);
			}
		}
		else if (selectMode != 0)
		{
			Transform transform = lookCamera.transform;
			castRay.origin = transform.position;
			castRay.direction = transform.forward;
			ProcessTargetSelectionDoRaycast(selectionHUD, castRay, highlightedControllersLook);
		}
	}

	private void ProcessTargetShow(bool canSelect = true)
	{
		bool flag = gameMode == GameMode.Edit && GetTargetShow();
		if (canSelect)
		{
			if (selectMode != SelectMode.Targets && flag)
			{
				SelectModeTargets();
			}
			if (selectMode != 0 && !flag)
			{
				SelectModeOff();
			}
			return;
		}
		if (flag)
		{
			_pointerModeLeft = true;
			_pointerModeRight = true;
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (onlyShowControllers != null)
					{
						if (onlyShowControllers.Contains(allController))
						{
							allController.hidden = false;
						}
						else
						{
							allController.hidden = true;
						}
					}
					else if (gameMode == GameMode.Edit || allController.interactableInPlayMode)
					{
						if (UserPreferences.singleton.hideInactiveTargets && allController.currentPositionState == FreeControllerV3.PositionState.Off && allController.currentRotationState == FreeControllerV3.RotationState.Off)
						{
							allController.hidden = true;
						}
						else if (allController.containingAtom == null)
						{
							allController.hidden = false;
						}
						else if (allController.containingAtom.tempHidden)
						{
							allController.hidden = true;
						}
						else if (_showHiddenAtoms || !allController.containingAtom.hidden)
						{
							allController.hidden = false;
						}
						else
						{
							allController.hidden = true;
						}
					}
					else
					{
						allController.hidden = true;
					}
				}
				return;
			}
		}
		_pointerModeLeft = false;
		_pointerModeRight = false;
		foreach (FreeControllerV3 allController2 in allControllers)
		{
			allController2.hidden = true;
		}
	}

	public void ToggleTargetsOnWithButton()
	{
		targetsOnWithButton = !targetsOnWithButton;
	}

	private void ProcessControllerTargetSelection()
	{
		if (!useLookSelect)
		{
			return;
		}
		if (highlightedControllersLook == null)
		{
			highlightedControllersLook = new List<FreeControllerV3>();
		}
		if (buttonToggleTargets != null && JoystickControl.GetButtonDown(buttonToggleTargets))
		{
			ToggleTargetsOnWithButton();
		}
		if (!GUIhit)
		{
			if (buttonSelect != null && buttonSelect != string.Empty && JoystickControl.GetButtonDown(buttonSelect))
			{
				ProcessTargetSelectionDoSelect(highlightedControllersLook);
			}
			if (buttonCycleSelection != null && buttonCycleSelection != string.Empty && JoystickControl.GetButtonDown(buttonCycleSelection) && highlightedControllersLook.Count > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersLook);
			}
		}
		if (buttonUnselect != null && buttonUnselect != string.Empty && JoystickControl.GetButtonDown(buttonUnselect))
		{
			ClearSelection();
		}
	}

	private void ProcessMouseChange()
	{
		mouseAxisX = Input.GetAxisRaw("Mouse X");
		mouseAxisY = Input.GetAxisRaw("Mouse Y");
		currentMousePosition = Input.mousePosition;
		if (cursorLockedLastFrame)
		{
			lastMousePosition = currentMousePosition;
		}
		mouseChange = currentMousePosition - lastMousePosition;
		mouseChangeScaled = mouseChange * 0.1f;
		lastMousePosition = Input.mousePosition;
		if (selectMode != SelectMode.FreeMoveMouse && mouseAxisX == 0f && mouseAxisY == 0f && (mouseChange.x != 0f || mouseChange.y != 0f))
		{
			useMouseRDPMode = true;
		}
		else
		{
			useMouseRDPMode = false;
		}
	}

	private float GetMouseXChange()
	{
		if (useMouseRDPMode)
		{
			return mouseChangeScaled.x;
		}
		return mouseAxisX;
	}

	private float GetMouseYChange()
	{
		if (useMouseRDPMode)
		{
			return mouseChangeScaled.y;
		}
		return mouseAxisY;
	}

	private void ProcessMouseTargetControl(bool canSelect = true)
	{
		if (!(MonitorCenterCamera != null) || !MonitorRigActive)
		{
			return;
		}
		if (highlightedControllersMouse == null)
		{
			highlightedControllersMouse = new List<FreeControllerV3>();
		}
		bool flag = RuntimeTools.ActiveTool != null;
		bool flag2 = false;
		if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.HasSelectedAxis)
		{
			flag2 = true;
		}
		if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.HasSelectedAxis)
		{
			flag2 = true;
		}
		Ray ray = MonitorCenterCamera.ScreenPointToRay(Input.mousePosition);
		if (!GUIhitMouse && !flag2)
		{
			ProcessTargetSelectionDoRaycast(mouseSelectionHUD, ray, highlightedControllersMouse, true, true, false);
		}
		else
		{
			foreach (FreeControllerV3 item in highlightedControllersMouse)
			{
				item.highlighted = false;
			}
			highlightedControllersMouse.Clear();
		}
		if (Input.GetMouseButtonDown(0))
		{
			eligibleForMouseSelect = false;
			potentialGrabbedControllerMouse = null;
			if (grabbedControllerMouse != null)
			{
				grabbedControllerMouse.isGrabbing = false;
				grabbedControllerMouse.RestorePreLinkState();
				grabbedControllerMouse = null;
			}
			if (!GUIhitMouse && !flag)
			{
				eligibleForMouseSelect = true;
				dragActivated = false;
				if (highlightedControllersMouse.Count > 0)
				{
					potentialGrabbedControllerMouse = highlightedControllersMouse[0];
				}
				if (potentialGrabbedControllerMouse != null)
				{
					mouseClickUsed = true;
				}
				if (potentialGrabbedControllerMouse != null)
				{
					grabbedControllerMouseDistance = (potentialGrabbedControllerMouse.transform.position - ray.origin).magnitude;
					mouseDownLastWorldPosition = ray.origin + ray.direction * grabbedControllerMouseDistance;
				}
			}
		}
		if (!flag && Input.GetMouseButton(0) && potentialGrabbedControllerMouse != null)
		{
			float num = GetMouseXChange() * 10f;
			float num2 = GetMouseYChange() * 10f;
			if (!dragActivated && (Mathf.Abs(num) >= 1f || Mathf.Abs(num2) >= 1f))
			{
				dragActivated = true;
				if (mouseGrab != null)
				{
					Rigidbody component = mouseGrab.GetComponent<Rigidbody>();
					if (component != null)
					{
						bool flag3 = true;
						FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
						if (potentialGrabbedControllerMouse.canGrabPosition)
						{
							if (potentialGrabbedControllerMouse.canGrabRotation)
							{
								linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
							}
						}
						else if (potentialGrabbedControllerMouse.canGrabRotation)
						{
							linkState = FreeControllerV3.SelectLinkState.Rotation;
						}
						else
						{
							flag3 = false;
						}
						if (flag3)
						{
							mouseGrab.position = potentialGrabbedControllerMouse.transform.position;
							mouseGrab.rotation = potentialGrabbedControllerMouse.transform.rotation;
							grabbedControllerMouse = potentialGrabbedControllerMouse;
							grabbedControllerMouse.isGrabbing = true;
							grabbedControllerMouse.SelectLinkToRigidbody(component, linkState);
						}
					}
				}
			}
			if (dragActivated && grabbedControllerMouse != null)
			{
				Vector3 vector = ray.origin + ray.direction * grabbedControllerMouseDistance;
				bool key = Input.GetKey(KeyCode.LeftControl);
				bool key2 = Input.GetKey(KeyCode.LeftShift);
				if (grabbedControllerMouse.canGrabRotation && (key || key2))
				{
					if (key2)
					{
						mouseGrab.Rotate(MonitorCenterCamera.transform.up, 0f - num, Space.World);
						mouseGrab.Rotate(MonitorCenterCamera.transform.right, num2, Space.World);
					}
					else
					{
						mouseGrab.Rotate(MonitorCenterCamera.transform.forward, 0f - num, Space.World);
						mouseGrab.Rotate(MonitorCenterCamera.transform.right, num2, Space.World);
					}
				}
				if (grabbedControllerMouse.canGrabPosition && !key && !key2)
				{
					Vector3 vector2 = vector - mouseDownLastWorldPosition;
					mouseGrab.position += vector2;
				}
				mouseDownLastWorldPosition = vector;
			}
		}
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		potentialGrabbedControllerMouse = null;
		if (!dragActivated && !flag && !GUIhitMouse && eligibleForMouseSelect)
		{
			ProcessTargetSelectionDoRaycast(null, ray, highlightedControllersMouse, true, true);
			if (canSelect)
			{
				ProcessTargetSelectionDoSelect(highlightedControllersMouse);
			}
		}
		if (grabbedControllerMouse != null)
		{
			grabbedControllerMouse.isGrabbing = false;
			grabbedControllerMouse.RestorePreLinkState();
			grabbedControllerMouse = null;
		}
	}

	protected void SyncUISide()
	{
		if (MonitorRigActive)
		{
			UISideAlign.useNeutralRotation = true;
			UISideAlign.globalSide = UISideAlign.Side.Left;
		}
		else
		{
			UISideAlign.useNeutralRotation = false;
			UISideAlign.globalSide = _UISide;
		}
	}

	public void SetUISide(string side)
	{
		try
		{
			UISide = (UISideAlign.Side)System.Enum.Parse(typeof(UISideAlign.Side), side);
		}
		catch (FormatException)
		{
			LogError("Tried to set UI side to " + side + " which is not a valid side type");
		}
	}

	protected void SyncOnStartupSkipStartScreen(bool b)
	{
		onStartupSkipStartScreen = b;
	}

	private void SetMonitorRigPositionZero()
	{
		if (MonitorRig != null)
		{
			Vector3 localPosition = MonitorRig.localPosition;
			localPosition.x = 0f;
			MonitorRig.localPosition = localPosition;
		}
	}

	private void SetMonitorRigPositionOffset()
	{
		if (MonitorRig != null)
		{
			Vector3 localPosition = MonitorRig.localPosition;
			localPosition.x = _monitorRigRightOffsetWhenUIOpen * focusDistance;
			MonitorRig.localPosition = localPosition;
		}
	}

	public void SyncMonitorRigPosition()
	{
		if (_mainHUDVisible && _mainHUDAnchoredOnMonitor && headPossessedController == null && (UserPreferences.singleton == null || UserPreferences.singleton.useMonitorViewOffsetWhenUIOpen))
		{
			SetMonitorRigPositionOffset();
		}
		else
		{
			SetMonitorRigPositionZero();
		}
	}

	public void SelectModeOffAndShowMainHUDAuto()
	{
		SelectModeOff();
		ShowMainHUDAuto();
	}

	public void ShowMainHUDAuto()
	{
		if (MonitorRigActive)
		{
			ShowMainHUD(true, true);
		}
		else
		{
			ShowMainHUD();
		}
	}

	public void ShowMainHUD(bool setAnchors = true, bool forceMonitor = false)
	{
		SyncUISide();
		_mainHUDVisible = true;
		if (isMonitorOnly || forceMonitor)
		{
			_helpOverlayOnAux = false;
			if (helpToggle != null)
			{
				helpToggle.gameObject.SetActive(false);
			}
			if (helpToggleAlt != null)
			{
				helpToggleAlt.gameObject.SetActive(false);
			}
			_mainHUDAnchoredOnMonitor = true;
		}
		else
		{
			_helpOverlayOnAux = true;
			if (helpToggle != null)
			{
				helpToggle.gameObject.SetActive(true);
			}
			if (helpToggleAlt != null)
			{
				helpToggleAlt.gameObject.SetActive(true);
			}
			_mainHUDAnchoredOnMonitor = false;
		}
		SyncHelpOverlay();
		SyncMonitorRigPosition();
		if (mainHUDPivot != null)
		{
			Vector3 localEulerAngles = default(Vector3);
			if (isMonitorOnly || forceMonitor)
			{
				localEulerAngles.x = mainHUDPivotXRotationMonitor;
			}
			else
			{
				localEulerAngles.x = mainHUDPivotXRotationVR;
			}
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			mainHUDPivot.localEulerAngles = localEulerAngles;
		}
		if (mainHUD != null)
		{
			mainHUD.gameObject.SetActive(true);
		}
		SyncActiveUI();
		if (setAnchors)
		{
			HUDAnchor.SetAnchorsToReference();
		}
		if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
		{
			selectedControllerPositionHandle.enabled = true;
		}
		if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
		{
			selectedControllerRotationHandle.enabled = true;
		}
		SyncVisibility();
	}

	public void HideMainHUD()
	{
		_mainHUDVisible = false;
		SyncMonitorRigPosition();
		if (mainHUD != null)
		{
			mainHUD.gameObject.SetActive(false);
		}
		if (selectedController != null)
		{
			selectedController.guihidden = true;
		}
		if (selectedControllerPositionHandle != null)
		{
			selectedControllerPositionHandle.enabled = false;
		}
		if (selectedControllerRotationHandle != null)
		{
			selectedControllerRotationHandle.enabled = false;
		}
		if (customUI != null)
		{
			customUI.gameObject.SetActive(false);
		}
		HideTempHelp();
		SyncVisibility();
	}

	public void MoveMainHUD(Vector3 v)
	{
		if (mainHUDAttachPoint != null)
		{
			mainHUDAttachPoint.position = v;
		}
	}

	public void MoveMainHUD(Transform t)
	{
		if (mainHUDAttachPoint != null && t != null)
		{
			mainHUDAttachPoint.position = t.position;
			mainHUDAttachPoint.rotation = t.rotation;
		}
	}

	public void SetHelpHUDText(string txt)
	{
		helpText = txt;
	}

	public void ToggleMainHUDAuto()
	{
		if (mainHUD != null)
		{
			if (_mainHUDVisible)
			{
				HideMainHUD();
			}
			else
			{
				ShowMainHUDAuto();
			}
		}
	}

	public void ToggleMainHUD()
	{
		if (mainHUD != null)
		{
			if (_mainHUDVisible)
			{
				HideMainHUD();
			}
			else
			{
				ShowMainHUD();
			}
		}
	}

	public void ToggleMainHUDMonitor()
	{
		if (mainHUD != null)
		{
			if (_mainHUDVisible)
			{
				HideMainHUD();
			}
			else
			{
				ShowMainHUDMonitor();
			}
		}
	}

	public void ShowMainHUDMonitor()
	{
		ShowMainHUD(true, true);
	}

	private void AssignUICamera(Camera c)
	{
		if (c != null)
		{
			LookInputModule.singleton.referenceCamera = c;
			{
				foreach (Canvas allCanvase in allCanvases)
				{
					if (allCanvase != null && allCanvase.renderMode == RenderMode.WorldSpace)
					{
						allCanvase.worldCamera = c;
					}
				}
				return;
			}
		}
		Error("Tried to call AssignUICamera with a null camera");
	}

	private void ProcessUI()
	{
		if (!UIDisabled)
		{
			if (GetMenuShow())
			{
				if (_mainHUDVisible)
				{
					HideMainHUD();
				}
				else
				{
					ShowMainHUD();
				}
			}
			if (_mainHUDVisible)
			{
				if (GetMenuMoveLeft())
				{
					MoveMainHUD(motionControllerLeft);
				}
				if (GetMenuMoveRight())
				{
					MoveMainHUD(motionControllerRight);
				}
			}
		}
		if (!(LookInputModule.singleton != null))
		{
			return;
		}
		if (useLookSelect)
		{
			AssignUICamera(lookCamera);
			LookInputModule.singleton.ProcessMain();
			GUIhit = LookInputModule.singleton.guiRaycastHit;
		}
		else if (leftControllerCamera != null)
		{
			if (leftControllerCamera.gameObject.activeInHierarchy)
			{
				AssignUICamera(leftControllerCamera);
				LookInputModule.singleton.ProcessMain();
				GUIhitLeft = LookInputModule.singleton.guiRaycastHit;
			}
			else
			{
				GUIhitLeft = false;
			}
			if (GUIhitLeft)
			{
				drawRayLineLeft = !MonitorRigActive;
			}
			if (rightControllerCamera != null)
			{
				if (rightControllerCamera.gameObject.activeInHierarchy)
				{
					AssignUICamera(rightControllerCamera);
					LookInputModule.singleton.ProcessRight();
					GUIhitRight = LookInputModule.singleton.guiRaycastHit;
				}
				else
				{
					GUIhitRight = false;
				}
				if (GUIhitRight)
				{
					drawRayLineRight = !MonitorRigActive;
				}
			}
			else
			{
				Error("Right controller camera is null while processing UI");
			}
		}
		else if (rightControllerCamera != null)
		{
			if (rightControllerCamera.gameObject.activeInHierarchy)
			{
				AssignUICamera(rightControllerCamera);
				LookInputModule.singleton.ProcessRight();
				GUIhitRight = LookInputModule.singleton.guiRaycastHit;
			}
			else
			{
				GUIhitRight = false;
			}
			if (GUIhitRight)
			{
				drawRayLineRight = !MonitorRigActive;
			}
		}
		AssignUICamera(MonitorCenterCamera);
		LookInputModule.singleton.ProcessMouseAlt(selectMode == SelectMode.FreeMoveMouse);
		GUIhitMouse = LookInputModule.singleton.mouseRaycastHit;
	}

	private void ProcessUIMove()
	{
		if (!UIDisabled && _mainHUDAnchoredOnMonitor && MonitorCenterCamera != null && MonitorUIAnchor != null && MonitorUIAttachPoint != null)
		{
			Vector3 position = default(Vector3);
			position.x = 5f;
			position.y = _monitorUIYOffset + 60f;
			position.z = (float)MonitorCenterCamera.pixelHeight * 0.08f / _monitorUIScale / fixedMonitorUIScale / monitorCameraFOV * worldScale;
			Vector3 position2 = MonitorCenterCamera.ScreenToWorldPoint(position);
			MonitorUIAnchor.position = position2;
			MoveMainHUD(MonitorUIAttachPoint);
			HUDAnchor.SetAnchorsToReference();
		}
	}

	public void RemoveCanvas(Canvas c)
	{
		allCanvases.Remove(c);
	}

	public void AddCanvas(Canvas c)
	{
		if (overrideCanvasSortingLayer)
		{
			IgnoreCanvas component = c.GetComponent<IgnoreCanvas>();
			if (component == null)
			{
				c.sortingLayerName = overrideCanvasSortingLayerName;
			}
		}
		allCanvases.Add(c);
	}

	protected void AllocateRaycastHits()
	{
		if (raycastHits == null)
		{
			raycastHits = new RaycastHit[256];
		}
	}

	private void ProcessSelectDoRaycast(SelectionHUD sh, Ray ray, List<SelectTarget> hitsList, bool doHighlight = true, bool setSelectionHUDTransform = true)
	{
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(ray, raycastHits, 50f, selectColliderMask);
		if (num > 0)
		{
			if (wasHitST == null)
			{
				wasHitST = new Dictionary<SelectTarget, bool>();
			}
			else
			{
				wasHitST.Clear();
			}
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHits[i];
				SelectTarget component = raycastHit.transform.GetComponent<SelectTarget>();
				if (component != null && !wasHitST.ContainsKey(component))
				{
					wasHitST.Add(component, true);
					if (!hitsList.Contains(component))
					{
						hitsList.Add(component);
					}
				}
			}
			SelectTarget[] array = hitsList.ToArray();
			SelectTarget[] array2 = array;
			foreach (SelectTarget selectTarget in array2)
			{
				if (!wasHitST.ContainsKey(selectTarget))
				{
					selectTarget.highlighted = false;
					hitsList.Remove(selectTarget);
				}
			}
			if (doHighlight)
			{
				for (int k = 0; k < hitsList.Count; k++)
				{
					SelectTarget selectTarget2 = hitsList[k];
					if (k == 0)
					{
						selectTarget2.highlighted = true;
					}
					else
					{
						selectTarget2.highlighted = false;
					}
				}
			}
			if (sh != null)
			{
				sh.ClearSelections();
				if (hitsList.Count > 0)
				{
					int num2 = 0;
					foreach (SelectTarget hits in hitsList)
					{
						sh.SetSelection(num2, hits.transform, hits.selectionName);
						num2++;
					}
				}
			}
		}
		else
		{
			if (doHighlight)
			{
				foreach (SelectTarget hits2 in hitsList)
				{
					hits2.highlighted = false;
				}
			}
			hitsList.Clear();
		}
		if (!(sh != null))
		{
			return;
		}
		if (hitsList.Count > 0)
		{
			sh.gameObject.SetActive(true);
			if (setSelectionHUDTransform)
			{
				sh.transform.position = hitsList[0].transform.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position);
			}
		}
		else
		{
			sh.gameObject.SetActive(false);
		}
	}

	private void ProcessSelectDoSelect(List<SelectTarget> highlightedSelectTargets)
	{
		SelectTarget selectTarget = highlightedSelectTargets[0];
		FreeControllerV3 value;
		switch (selectMode)
		{
		case SelectMode.Controller:
			if (fcMap.TryGetValue(selectTarget.selectionName, out value))
			{
				selectControllerCallback(value);
				SelectModeOff();
			}
			break;
		case SelectMode.ForceProducer:
		{
			ForceProducerV2 value3;
			if (fpMap.TryGetValue(selectTarget.selectionName, out value3))
			{
				selectForceProducerCallback(value3);
				SelectModeOff();
			}
			break;
		}
		case SelectMode.ForceReceiver:
		{
			ForceReceiver value4;
			if (frMap.TryGetValue(selectTarget.selectionName, out value4))
			{
				selectForceReceiverCallback(value4);
				SelectModeOff();
			}
			break;
		}
		case SelectMode.Rigidbody:
		{
			Rigidbody value2;
			if (rbMap.TryGetValue(selectTarget.selectionName, out value2))
			{
				selectRigidbodyCallback(value2);
				SelectModeOff();
			}
			break;
		}
		case SelectMode.Atom:
		{
			Atom value5;
			if (atoms.TryGetValue(selectTarget.selectionName, out value5))
			{
				selectAtomCallback(value5);
				SelectModeOff();
			}
			break;
		}
		case SelectMode.ArmedForRecord:
		{
			MotionAnimationControl value6;
			if (macMap.TryGetValue(selectTarget.selectionName, out value6))
			{
				value6.armedForRecord = !value6.armedForRecord;
				if (value6.armedForRecord)
				{
					selectTarget.SetColor(Color.green);
				}
				else
				{
					selectTarget.SetColor(Color.red);
				}
			}
			break;
		}
		case SelectMode.PossessAndAlign:
			if (fcMap.TryGetValue(selectTarget.selectionName, out value))
			{
				HeadPossess(value, true);
				if (isMonitorOnly)
				{
					SelectModeOff();
				}
				else
				{
					SelectModePossess(true);
				}
			}
			break;
		case SelectMode.Unpossess:
			if (fcMap.TryGetValue(selectTarget.selectionName, out value))
			{
				ClearPossess(false, value);
				if (GetCancel())
				{
					SelectModeOff();
				}
			}
			break;
		case SelectMode.Possess:
		case SelectMode.TwoStagePossess:
		case SelectMode.AnimationRecord:
			break;
		}
	}

	private void ProcessSelectCycleSelect(List<SelectTarget> highlightedSelectTargets)
	{
		if (highlightedSelectTargets != null && highlightedSelectTargets.Count > 1)
		{
			SelectTarget item = highlightedSelectTargets[0];
			highlightedSelectTargets.RemoveAt(0);
			highlightedSelectTargets.Add(item);
		}
	}

	private void ProcessSelectCycleBackwardsSelect(List<SelectTarget> highlightedSelectTargets)
	{
		if (highlightedSelectTargets != null && highlightedSelectTargets.Count > 1)
		{
			int index = highlightedSelectTargets.Count - 1;
			SelectTarget item = highlightedSelectTargets[index];
			highlightedSelectTargets.RemoveAt(index);
			highlightedSelectTargets.Insert(0, item);
		}
	}

	private void ProcessSelectTargetHighlight(SelectionHUD sh, Transform processFrom, bool isLeft)
	{
		castRay.origin = processFrom.position;
		castRay.direction = processFrom.forward;
		if (isLeft)
		{
			drawRayLineLeft = !MonitorRigActive;
			ProcessSelectDoRaycast(sh, castRay, highlightedSelectTargetsLeft, false);
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFrom.position;
		}
		else
		{
			drawRayLineRight = !MonitorRigActive;
			ProcessSelectDoRaycast(sh, castRay, highlightedSelectTargetsRight, false);
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFrom.position;
		}
	}

	private void ProcessMotionControllerSelect()
	{
		if (isMonitorOnly)
		{
			return;
		}
		if (highlightedSelectTargetsLeft == null)
		{
			highlightedSelectTargetsLeft = new List<SelectTarget>();
		}
		if (highlightedSelectTargetsRight == null)
		{
			highlightedSelectTargetsRight = new List<SelectTarget>();
		}
		if ((bool)motionControllerLeft && !GUIhitLeft)
		{
			ProcessSelectTargetHighlight(leftSelectionHUD, motionControllerLeft, true);
		}
		if ((bool)motionControllerRight && !GUIhitRight)
		{
			ProcessSelectTargetHighlight(rightSelectionHUD, motionControllerRight, false);
		}
		if (GetLeftSelect() && highlightedSelectTargetsLeft.Count > 0)
		{
			ProcessSelectDoSelect(highlightedSelectTargetsLeft);
		}
		if (GetRightSelect() && highlightedSelectTargetsRight.Count > 0)
		{
			ProcessSelectDoSelect(highlightedSelectTargetsRight);
		}
		if (isOpenVR)
		{
			if (highlightedSelectTargetsLeft != null && highlightedSelectTargetsLeft.Count > 1)
			{
				if (leftCycleX < 0 || leftCycleY < 0)
				{
					ProcessSelectCycleBackwardsSelect(highlightedSelectTargetsLeft);
					float num = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
					hapticAction.Execute(0f, num, 1f / num, 1f, SteamVR_Input_Sources.LeftHand);
				}
				else if (leftCycleX > 0 || leftCycleY > 0)
				{
					ProcessSelectCycleSelect(highlightedSelectTargetsLeft);
					float num2 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
					hapticAction.Execute(0f, num2, 1f / num2, 1f, SteamVR_Input_Sources.LeftHand);
				}
			}
			if (highlightedSelectTargetsRight != null && highlightedSelectTargetsRight.Count > 0)
			{
				if (rightCycleX < 0 || rightCycleY < 0)
				{
					ProcessSelectCycleBackwardsSelect(highlightedSelectTargetsRight);
					float num3 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
					hapticAction.Execute(0f, num3, 1f / num3, 1f, SteamVR_Input_Sources.RightHand);
				}
				else if (rightCycleX > 0 || rightCycleY > 0)
				{
					ProcessSelectCycleSelect(highlightedSelectTargetsRight);
					float num4 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
					hapticAction.Execute(0f, num4, 1f / num4, 1f, SteamVR_Input_Sources.RightHand);
				}
			}
		}
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessSelect()
	{
		if (highlightedSelectTargetsLook == null)
		{
			highlightedSelectTargetsLook = new List<SelectTarget>();
		}
		if (useLookSelect && lookCamera != null && !GUIhit)
		{
			Transform transform = lookCamera.transform;
			castRay.origin = transform.position;
			castRay.direction = transform.forward;
			ProcessSelectDoRaycast(selectionHUD, castRay, highlightedSelectTargetsLook);
			if (buttonSelect != null && buttonSelect != string.Empty && JoystickControl.GetButtonDown(buttonSelect) && highlightedSelectTargetsLook.Count > 0)
			{
				ProcessSelectDoSelect(highlightedSelectTargetsLook);
			}
			if (buttonCycleSelection != null && buttonCycleSelection != string.Empty && JoystickControl.GetButtonDown(buttonCycleSelection) && highlightedSelectTargetsLook.Count > 0)
			{
				ProcessSelectCycleSelect(highlightedSelectTargetsLook);
			}
			if (buttonUnselect != null && buttonUnselect != string.Empty && JoystickControl.GetButtonDown(buttonUnselect))
			{
				SelectModeOff();
			}
		}
	}

	private void ProcessMouseSelect()
	{
		if (highlightedSelectTargetsMouse == null)
		{
			highlightedSelectTargetsMouse = new List<SelectTarget>();
		}
		if (MonitorRigActive && !GUIhitMouse)
		{
			Ray ray = MonitorCenterCamera.ScreenPointToRay(Input.mousePosition);
			ProcessSelectDoRaycast(mouseSelectionHUD, ray, highlightedSelectTargetsMouse, true, false);
			if (Input.GetMouseButtonDown(0) && highlightedSelectTargetsMouse.Count > 0)
			{
				ProcessSelectDoSelect(highlightedSelectTargetsMouse);
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				ProcessSelectCycleSelect(highlightedSelectTargetsMouse);
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SelectModeOff();
			}
		}
	}

	private FreeControllerV3 ProcessControllerPossess(Transform processFrom)
	{
		if (processFrom != null)
		{
			AllocateOverlappingControls();
			int num = Physics.OverlapSphereNonAlloc(processFrom.position, 0.01f * _worldScale, overlappingControls, targetColliderMask);
			if (num > 0)
			{
				if (overlappingFcs == null)
				{
					overlappingFcs = new List<FreeControllerV3>();
				}
				else
				{
					overlappingFcs.Clear();
				}
				for (int i = 0; i < num; i++)
				{
					Collider collider = overlappingControls[i];
					FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
					if (component != null && component.possessable && (component.canGrabPosition || component.canGrabRotation) && !component.possessed && !component.startedPossess)
					{
						if (!overlappingFcs.Contains(component))
						{
							overlappingFcs.Add(component);
						}
						float distanceHolder = Vector3.SqrMagnitude(processFrom.position - collider.transform.position);
						component.distanceHolder = distanceHolder;
					}
				}
				if (overlappingFcs.Count > 0)
				{
					overlappingFcs.Sort((FreeControllerV3 c1, FreeControllerV3 c2) => c1.distanceHolder.CompareTo(c2.distanceHolder));
					return overlappingFcs[0];
				}
			}
		}
		return null;
	}

	private void AlignRigAndController(FreeControllerV3 controller)
	{
		Possessor component = motionControllerHead.GetComponent<Possessor>();
		Vector3 forwardPossessAxis = controller.GetForwardPossessAxis();
		Vector3 upPossessAxis = controller.GetUpPossessAxis();
		Vector3 up = navigationRig.up;
		Vector3 vector = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
		Vector3 vector2 = Vector3.ProjectOnPlane(forwardPossessAxis, up);
		float num = Vector3.Dot(vector, vector2);
		if (num < -0.98f)
		{
			navigationRig.Rotate(0f, 180f, 0f);
			vector2 = -vector2;
		}
		if (Vector3.Dot(upPossessAxis, up) < 0f && Vector3.Dot(motionControllerHead.up, up) > 0f)
		{
			vector2 = -vector2;
		}
		Quaternion quaternion = Quaternion.FromToRotation(vector, vector2);
		navigationRig.rotation = quaternion * navigationRig.rotation;
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.transform.LookAt(MonitorCenterCamera.transform.position + forwardPossessAxis);
			Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
		}
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Transform followWhenOff = controller.followWhenOff;
		if (controller.possessPoint != null && followWhenOff != null)
		{
			position = followWhenOff.position;
			rotation = followWhenOff.rotation;
			followWhenOff.position = controller.control.position;
			followWhenOff.rotation = controller.control.rotation;
		}
		if (controller.canGrabRotation)
		{
			controller.AlignTo(component.autoSnapPoint, true);
		}
		Vector3 vector3 = ((!(controller.possessPoint != null)) ? controller.control.position : controller.possessPoint.position);
		Vector3 vector4 = vector3 - component.autoSnapPoint.position;
		Vector3 vector5 = navigationRig.position + vector4;
		float num2 = Vector3.Dot(vector5 - navigationRig.position, up);
		vector5 += up * (0f - num2);
		navigationRig.position = vector5;
		playerHeightAdjust += num2;
		headPossessedController.PossessMoveAndAlignTo(component.autoSnapPoint);
		if (controller.possessPoint != null && followWhenOff != null)
		{
			followWhenOff.position = position;
			followWhenOff.rotation = rotation;
		}
	}

	private void HeadPossess(FreeControllerV3 headPossess, bool alignRig = false, bool usePossessorSnapPoint = true, bool adjustSpring = true)
	{
		if (!headPossess.canGrabPosition && !headPossess.canGrabRotation)
		{
			return;
		}
		Possessor component = motionControllerHead.GetComponent<Possessor>();
		Rigidbody component2 = motionControllerHead.GetComponent<Rigidbody>();
		headPossessedController = headPossess;
		if (headPossessedActivateTransform != null)
		{
			headPossessedActivateTransform.gameObject.SetActive(true);
		}
		if (headPossessedText != null)
		{
			if (headPossessedController.containingAtom != null)
			{
				headPossessedText.text = headPossessedController.containingAtom.uid + ":" + headPossessedController.name;
			}
			else
			{
				headPossessedText.text = headPossessedController.name;
			}
		}
		headPossessedController.possessed = true;
		if (rightFullGrabbedController == headPossessedController)
		{
			rightFullGrabbedController = null;
			if (rightHandControl != null)
			{
				rightHandControl.possessed = false;
			}
			rightHandControl = null;
		}
		if (leftFullGrabbedController == headPossessedController)
		{
			leftFullGrabbedController = null;
			if (leftHandControl != null)
			{
				leftHandControl.possessed = false;
			}
			leftHandControl = null;
		}
		if (leftGrabbedController == headPossessedController)
		{
			leftGrabbedController = null;
		}
		if (rightGrabbedController == headPossessedController)
		{
			rightGrabbedController = null;
		}
		if (headPossessedController.canGrabPosition)
		{
			MotionAnimationControl component3 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component3 != null)
			{
				component3.suspendPositionPlayback = true;
			}
			if (_allowPossessSpringAdjustment && adjustSpring)
			{
				headPossessedController.RBHoldPositionSpring = _possessPositionSpring;
			}
		}
		if (headPossessedController.canGrabRotation)
		{
			MotionAnimationControl component4 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component4 != null)
			{
				component4.suspendRotationPlayback = true;
			}
			if (_allowPossessSpringAdjustment && adjustSpring)
			{
				headPossessedController.RBHoldRotationSpring = _possessRotationSpring;
			}
		}
		SyncMonitorRigPosition();
		if (alignRig)
		{
			AlignRigAndController(headPossessedController);
		}
		else if (component != null && component.autoSnapPoint != null && usePossessorSnapPoint)
		{
			headPossessedController.PossessMoveAndAlignTo(component.autoSnapPoint);
		}
		if (!(component2 != null))
		{
			return;
		}
		FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
		if (headPossessedController.canGrabPosition)
		{
			if (headPossessedController.canGrabRotation)
			{
				linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
			}
		}
		else if (headPossessedController.canGrabRotation)
		{
			linkState = FreeControllerV3.SelectLinkState.Rotation;
		}
		headPossessedController.SelectLinkToRigidbody(component2, linkState);
	}

	protected bool MotionControlPossess(Transform motionController, FreeControllerV3 controllerToPossess, bool usePossessorSnapPoint = true, bool adjustSpring = true)
	{
		if (controllerToPossess.canGrabPosition || controllerToPossess.canGrabRotation)
		{
			Possessor component = motionController.GetComponent<Possessor>();
			Rigidbody component2 = motionController.GetComponent<Rigidbody>();
			if (playerNavCollider != null && playerNavCollider.underlyingControl == controllerToPossess)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			controllerToPossess.possessed = true;
			if (rightFullGrabbedController == controllerToPossess)
			{
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (leftFullGrabbedController == controllerToPossess)
			{
				leftFullGrabbedController = null;
				if (leftHandControl != null)
				{
					leftHandControl.possessed = false;
				}
				leftHandControl = null;
			}
			if (leftGrabbedController == controllerToPossess)
			{
				leftGrabbedController = null;
			}
			if (rightGrabbedController == controllerToPossess)
			{
				rightGrabbedController = null;
			}
			if (controllerToPossess.canGrabPosition)
			{
				MotionAnimationControl component3 = controllerToPossess.GetComponent<MotionAnimationControl>();
				if (component3 != null)
				{
					component3.suspendPositionPlayback = true;
				}
				if (_allowPossessSpringAdjustment && adjustSpring)
				{
					controllerToPossess.RBHoldPositionSpring = _possessPositionSpring;
				}
			}
			if (controllerToPossess.canGrabRotation)
			{
				MotionAnimationControl component4 = controllerToPossess.GetComponent<MotionAnimationControl>();
				if (component4 != null)
				{
					component4.suspendRotationPlayback = true;
				}
				if (_allowPossessSpringAdjustment && adjustSpring)
				{
					controllerToPossess.RBHoldRotationSpring = _possessRotationSpring;
				}
			}
			if (component != null && component.autoSnapPoint != null && usePossessorSnapPoint)
			{
				controllerToPossess.PossessMoveAndAlignTo(component.autoSnapPoint);
			}
			if (component2 != null)
			{
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (controllerToPossess.canGrabPosition)
				{
					if (controllerToPossess.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (controllerToPossess.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				controllerToPossess.SelectLinkToRigidbody(component2, linkState);
			}
			return true;
		}
		return false;
	}

	private void ProcessPossess()
	{
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (rightPossessedController == null)
		{
			FreeControllerV3 freeControllerV = ProcessControllerPossess(motionControllerRight);
			if (freeControllerV != null && MotionControlPossess(motionControllerRight, freeControllerV))
			{
				flag2 = true;
				if (commonHandModelControl != null && !_leapHandRightConnected)
				{
					commonHandModelControl.rightHandEnabled = false;
				}
				if (alternateControllerHandModelControl != null)
				{
					alternateControllerHandModelControl.rightHandEnabled = false;
				}
				rightPossessedController = freeControllerV;
				HandControl handControl = freeControllerV.GetComponent<HandControl>();
				if (handControl == null)
				{
					HandControlLink component = freeControllerV.GetComponent<HandControlLink>();
					if (component != null)
					{
						handControl = component.handControl;
					}
				}
				if (handControl != null)
				{
					rightPossessHandControl = handControl;
					rightPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (leftPossessedController == null)
		{
			FreeControllerV3 freeControllerV2 = ProcessControllerPossess(motionControllerLeft);
			if (freeControllerV2 != null && MotionControlPossess(motionControllerLeft, freeControllerV2))
			{
				flag3 = true;
				if (commonHandModelControl != null && !_leapHandLeftConnected)
				{
					commonHandModelControl.leftHandEnabled = false;
				}
				if (alternateControllerHandModelControl != null)
				{
					alternateControllerHandModelControl.leftHandEnabled = false;
				}
				leftPossessedController = freeControllerV2;
				HandControl handControl2 = freeControllerV2.GetComponent<HandControl>();
				if (handControl2 == null)
				{
					HandControlLink component2 = freeControllerV2.GetComponent<HandControlLink>();
					if (component2 != null)
					{
						handControl2 = component2.handControl;
					}
				}
				if (handControl2 != null)
				{
					leftPossessHandControl = handControl2;
					leftPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag3 = true;
		}
		if (leapRightPossessedController == null)
		{
			FreeControllerV3 freeControllerV3 = ProcessControllerPossess(leapHandRight);
			if (freeControllerV3 != null && MotionControlPossess(leapHandRight, freeControllerV3))
			{
				flag2 = true;
				if (commonHandModelControl != null)
				{
					commonHandModelControl.rightHandEnabled = false;
				}
				leapRightPossessedController = freeControllerV3;
				HandControl handControl3 = freeControllerV3.GetComponent<HandControl>();
				if (handControl3 == null)
				{
					HandControlLink component3 = freeControllerV3.GetComponent<HandControlLink>();
					if (component3 != null)
					{
						handControl3 = component3.handControl;
					}
				}
				if (handControl3 != null)
				{
					leapRightPossessHandControl = handControl3;
					leapRightPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (leapLeftPossessedController == null)
		{
			FreeControllerV3 freeControllerV4 = ProcessControllerPossess(leapHandLeft);
			if (freeControllerV4 != null && MotionControlPossess(leapHandLeft, freeControllerV4))
			{
				flag3 = true;
				if (commonHandModelControl != null)
				{
					commonHandModelControl.leftHandEnabled = false;
				}
				leapLeftPossessedController = freeControllerV4;
				HandControl handControl4 = freeControllerV4.GetComponent<HandControl>();
				if (handControl4 == null)
				{
					HandControlLink component4 = freeControllerV4.GetComponent<HandControlLink>();
					if (component4 != null)
					{
						handControl4 = component4.handControl;
					}
				}
				if (handControl4 != null)
				{
					leapLeftPossessHandControl = handControl4;
					leapLeftPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag3 = true;
		}
		if (headPossessedController == null)
		{
			FreeControllerV3 freeControllerV5 = ProcessControllerPossess(motionControllerHead);
			if (freeControllerV5 != null)
			{
				HeadPossess(freeControllerV5);
				flag4 = headPossessedController != null;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag4 = true;
		}
		if (GetCancel())
		{
			ClearPossess();
			SelectModeOff();
		}
		if ((flag2 && flag3 && flag4) || GetLeftSelect() || GetRightSelect() || GetMouseSelect())
		{
			SelectModeOff();
		}
	}

	private void ProcessTwoStagePossess()
	{
		if (rightPossessedController == null && rightStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV = ProcessControllerPossess(motionControllerRight);
			if (freeControllerV != null)
			{
				rightStartPossessedController = freeControllerV;
				rightStartPossessedController.startedPossess = true;
			}
		}
		if (leftPossessedController == null && leftStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV2 = ProcessControllerPossess(motionControllerLeft);
			if (freeControllerV2 != null)
			{
				leftStartPossessedController = freeControllerV2;
				leftStartPossessedController.startedPossess = true;
			}
		}
		if (headPossessedController == null && headStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV3 = ProcessControllerPossess(motionControllerHead);
			if (freeControllerV3 != null)
			{
				headStartPossessedController = freeControllerV3;
				headStartPossessedController.startedPossess = true;
			}
		}
		if (leapRightPossessedController == null && leapRightStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV4 = ProcessControllerPossess(leapHandRight);
			if (freeControllerV4 != null)
			{
				leapRightStartPossessedController = freeControllerV4;
				leapRightStartPossessedController.startedPossess = true;
			}
		}
		if (leapLeftPossessedController == null && leapLeftStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV5 = ProcessControllerPossess(leapHandLeft);
			if (freeControllerV5 != null)
			{
				leapLeftStartPossessedController = freeControllerV5;
				leapLeftStartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker1 != null && viveTracker1.gameObject.activeSelf && tracker1PossessedController == null && tracker1StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV6 = ProcessControllerPossess(viveTracker1.transform);
			if (freeControllerV6 != null)
			{
				tracker1StartPossessedController = freeControllerV6;
				tracker1StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker2 != null && viveTracker2.gameObject.activeSelf && tracker2PossessedController == null && tracker2StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV7 = ProcessControllerPossess(viveTracker2.transform);
			if (freeControllerV7 != null)
			{
				tracker2StartPossessedController = freeControllerV7;
				tracker2StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker3 != null && viveTracker3.gameObject.activeSelf && tracker3PossessedController == null && tracker3StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV8 = ProcessControllerPossess(viveTracker3.transform);
			if (freeControllerV8 != null)
			{
				tracker3StartPossessedController = freeControllerV8;
				tracker3StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker4 != null && viveTracker4.gameObject.activeSelf && tracker4PossessedController == null && tracker4StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV9 = ProcessControllerPossess(viveTracker4.transform);
			if (freeControllerV9 != null)
			{
				tracker4StartPossessedController = freeControllerV9;
				tracker4StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker5 != null && viveTracker5.gameObject.activeSelf && tracker5PossessedController == null && tracker5StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV10 = ProcessControllerPossess(viveTracker5.transform);
			if (freeControllerV10 != null)
			{
				tracker5StartPossessedController = freeControllerV10;
				tracker5StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker6 != null && viveTracker6.gameObject.activeSelf && tracker6PossessedController == null && tracker6StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV11 = ProcessControllerPossess(viveTracker6.transform);
			if (freeControllerV11 != null)
			{
				tracker6StartPossessedController = freeControllerV11;
				tracker6StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker7 != null && viveTracker7.gameObject.activeSelf && tracker7PossessedController == null && tracker7StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV12 = ProcessControllerPossess(viveTracker7.transform);
			if (freeControllerV12 != null)
			{
				tracker7StartPossessedController = freeControllerV12;
				tracker7StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker8 != null && viveTracker8.gameObject.activeSelf && tracker8PossessedController == null && tracker8StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV13 = ProcessControllerPossess(viveTracker8.transform);
			if (freeControllerV13 != null)
			{
				tracker8StartPossessedController = freeControllerV13;
				tracker8StartPossessedController.startedPossess = true;
			}
		}
		if (rightStartPossessedController != null && rightTwoStageLineDrawer != null)
		{
			rightTwoStageLineDrawer.SetLinePoints(motionControllerRight.position, rightStartPossessedController.transform.position);
			rightTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leftStartPossessedController != null && leftTwoStageLineDrawer != null)
		{
			leftTwoStageLineDrawer.SetLinePoints(motionControllerLeft.position, leftStartPossessedController.transform.position);
			leftTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (headStartPossessedController != null && headTwoStageLineDrawer != null)
		{
			headTwoStageLineDrawer.SetLinePoints(motionControllerHead.position, headStartPossessedController.transform.position);
			headTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leapRightStartPossessedController != null && leapRightTwoStageLineDrawer != null)
		{
			leapRightTwoStageLineDrawer.SetLinePoints(leapHandRight.transform.position, leapRightStartPossessedController.transform.position);
			leapRightTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leapLeftStartPossessedController != null && leapLeftTwoStageLineDrawer != null)
		{
			leapLeftTwoStageLineDrawer.SetLinePoints(leapHandLeft.transform.position, leapLeftStartPossessedController.transform.position);
			leapLeftTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker1StartPossessedController != null && tracker1TwoStageLineDrawer != null)
		{
			tracker1TwoStageLineDrawer.SetLinePoints(viveTracker1.transform.position, tracker1StartPossessedController.transform.position);
			tracker1TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker2StartPossessedController != null && tracker2TwoStageLineDrawer != null)
		{
			tracker2TwoStageLineDrawer.SetLinePoints(viveTracker2.transform.position, tracker2StartPossessedController.transform.position);
			tracker2TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker3StartPossessedController != null && tracker3TwoStageLineDrawer != null)
		{
			tracker3TwoStageLineDrawer.SetLinePoints(viveTracker3.transform.position, tracker3StartPossessedController.transform.position);
			tracker3TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker4StartPossessedController != null && tracker4TwoStageLineDrawer != null)
		{
			tracker4TwoStageLineDrawer.SetLinePoints(viveTracker4.transform.position, tracker4StartPossessedController.transform.position);
			tracker4TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker5StartPossessedController != null && tracker5TwoStageLineDrawer != null)
		{
			tracker5TwoStageLineDrawer.SetLinePoints(viveTracker5.transform.position, tracker5StartPossessedController.transform.position);
			tracker5TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker6StartPossessedController != null && tracker6TwoStageLineDrawer != null)
		{
			tracker6TwoStageLineDrawer.SetLinePoints(viveTracker6.transform.position, tracker6StartPossessedController.transform.position);
			tracker6TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker7StartPossessedController != null && tracker7TwoStageLineDrawer != null)
		{
			tracker7TwoStageLineDrawer.SetLinePoints(viveTracker7.transform.position, tracker7StartPossessedController.transform.position);
			tracker7TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker8StartPossessedController != null && tracker8TwoStageLineDrawer != null)
		{
			tracker8TwoStageLineDrawer.SetLinePoints(viveTracker8.transform.position, tracker8StartPossessedController.transform.position);
			tracker8TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (GetCancel())
		{
			ClearPossess();
			SelectModeOff();
		}
		if (GetLeftSelect() || GetRightSelect() || GetMouseSelect())
		{
			CompleteTwoStagePossess();
		}
	}

	protected void CompleteTwoStagePossess()
	{
		if (rightStartPossessedController != null)
		{
			if (MotionControlPossess(motionControllerRight, rightStartPossessedController, false, false))
			{
				rightPossessedController = rightStartPossessedController;
				HandControl handControl = rightPossessedController.GetComponent<HandControl>();
				if (handControl == null)
				{
					HandControlLink component = rightPossessedController.GetComponent<HandControlLink>();
					if (component != null)
					{
						handControl = component.handControl;
					}
				}
				if (handControl != null)
				{
					rightPossessHandControl = handControl;
					rightPossessHandControl.possessed = true;
				}
			}
			rightStartPossessedController.startedPossess = false;
			rightStartPossessedController = null;
		}
		if (leftStartPossessedController != null)
		{
			if (MotionControlPossess(motionControllerLeft, leftStartPossessedController, false, false))
			{
				leftPossessedController = leftStartPossessedController;
				HandControl handControl2 = leftPossessedController.GetComponent<HandControl>();
				if (handControl2 == null)
				{
					HandControlLink component2 = leftPossessedController.GetComponent<HandControlLink>();
					if (component2 != null)
					{
						handControl2 = component2.handControl;
					}
				}
				if (handControl2 != null)
				{
					leftPossessHandControl = handControl2;
					leftPossessHandControl.possessed = true;
				}
			}
			leftStartPossessedController.startedPossess = false;
			leftStartPossessedController = null;
		}
		if (headStartPossessedController != null)
		{
			HeadPossess(headStartPossessedController, false, false, false);
			headStartPossessedController.startedPossess = false;
			headStartPossessedController = null;
		}
		if (leapRightStartPossessedController != null)
		{
			if (MotionControlPossess(leapHandRight, leapRightStartPossessedController, false, false))
			{
				leapRightPossessedController = leapRightStartPossessedController;
				HandControl handControl3 = leapRightPossessedController.GetComponent<HandControl>();
				if (handControl3 == null)
				{
					HandControlLink component3 = leapRightPossessedController.GetComponent<HandControlLink>();
					if (component3 != null)
					{
						handControl3 = component3.handControl;
					}
				}
				if (handControl3 != null)
				{
					leapRightPossessHandControl = handControl3;
					leapRightPossessHandControl.possessed = true;
				}
			}
			leapRightStartPossessedController.startedPossess = false;
			leapRightStartPossessedController = null;
		}
		if (leapLeftStartPossessedController != null)
		{
			if (MotionControlPossess(leapHandLeft, leapLeftStartPossessedController, false, false))
			{
				leapLeftPossessedController = leapLeftStartPossessedController;
				HandControl handControl4 = leapLeftPossessedController.GetComponent<HandControl>();
				if (handControl4 == null)
				{
					HandControlLink component4 = leapLeftPossessedController.GetComponent<HandControlLink>();
					if (component4 != null)
					{
						handControl4 = component4.handControl;
					}
				}
				if (handControl4 != null)
				{
					leapLeftPossessHandControl = handControl4;
					leapLeftPossessHandControl.possessed = true;
				}
			}
			leapLeftStartPossessedController.startedPossess = false;
			leapLeftStartPossessedController = null;
		}
		if (tracker1StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker1.transform, tracker1StartPossessedController, false, false))
			{
				tracker1PossessedController = tracker1StartPossessedController;
				tracker1Visible = false;
			}
			tracker1StartPossessedController.startedPossess = false;
			tracker1StartPossessedController = null;
		}
		if (tracker2StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker2.transform, tracker2StartPossessedController, false, false))
			{
				tracker2PossessedController = tracker2StartPossessedController;
				tracker2Visible = false;
			}
			tracker2StartPossessedController.startedPossess = false;
			tracker2StartPossessedController = null;
		}
		if (tracker3StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker3.transform, tracker3StartPossessedController, false, false))
			{
				tracker3PossessedController = tracker3StartPossessedController;
				tracker3Visible = false;
			}
			tracker3StartPossessedController.startedPossess = false;
			tracker3StartPossessedController = null;
		}
		if (tracker4StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker4.transform, tracker4StartPossessedController, false, false))
			{
				tracker4PossessedController = tracker4StartPossessedController;
				tracker4Visible = false;
			}
			tracker4StartPossessedController.startedPossess = false;
			tracker4StartPossessedController = null;
		}
		if (tracker5StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker5.transform, tracker5StartPossessedController, false, false))
			{
				tracker5PossessedController = tracker5StartPossessedController;
				tracker5Visible = false;
			}
			tracker5StartPossessedController.startedPossess = false;
			tracker5StartPossessedController = null;
		}
		if (tracker6StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker6.transform, tracker6StartPossessedController, false, false))
			{
				tracker6PossessedController = tracker6StartPossessedController;
				tracker6Visible = false;
			}
			tracker6StartPossessedController.startedPossess = false;
			tracker6StartPossessedController = null;
		}
		if (tracker7StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker7.transform, tracker7StartPossessedController, false, false))
			{
				tracker7PossessedController = tracker7StartPossessedController;
				tracker7Visible = false;
			}
			tracker7StartPossessedController.startedPossess = false;
			tracker7StartPossessedController = null;
		}
		if (tracker8StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker8.transform, tracker8StartPossessedController, false, false))
			{
				tracker8PossessedController = tracker8StartPossessedController;
				tracker8Visible = false;
			}
			tracker8StartPossessedController.startedPossess = false;
			tracker8StartPossessedController = null;
		}
		SelectModeOff();
	}

	public void ClearHeadPossess()
	{
		if (headPossessedController != null)
		{
			ClearPossess(false, headPossessedController);
		}
	}

	public void ClearPossess()
	{
		ClearPossess(false, null);
	}

	public void ClearPossess(bool excludeHeadClear)
	{
		ClearPossess(excludeHeadClear, null);
	}

	public void ClearPossess(bool excludeHeadClear, FreeControllerV3 specificController)
	{
		if (selectMode == SelectMode.Possess || selectMode == SelectMode.TwoStagePossess || selectMode == SelectMode.PossessAndAlign)
		{
			SelectModeOff();
		}
		if (leftPossessedController != null && (specificController == null || leftPossessedController == specificController))
		{
			leftPossessedController.RestorePreLinkState();
			leftPossessedController.possessed = false;
			leftPossessedController.startedPossess = false;
			MotionAnimationControl component = leftPossessedController.GetComponent<MotionAnimationControl>();
			if (component != null)
			{
				component.suspendPositionPlayback = false;
				component.suspendRotationPlayback = false;
			}
			leftPossessedController = null;
			if (leftPossessHandControl != null)
			{
				leftPossessHandControl.possessed = false;
			}
			leftPossessHandControl = null;
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.leftHandEnabled = true;
			}
		}
		if (leftStartPossessedController != null)
		{
			leftStartPossessedController.startedPossess = false;
			leftStartPossessedController = null;
		}
		if (rightPossessedController != null && (specificController == null || rightPossessedController == specificController))
		{
			rightPossessedController.RestorePreLinkState();
			rightPossessedController.possessed = false;
			rightPossessedController.startedPossess = false;
			MotionAnimationControl component2 = rightPossessedController.GetComponent<MotionAnimationControl>();
			if (component2 != null)
			{
				component2.suspendPositionPlayback = false;
				component2.suspendRotationPlayback = false;
			}
			rightPossessedController = null;
			if (rightPossessHandControl != null)
			{
				rightPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.rightHandEnabled = true;
			}
		}
		if (rightStartPossessedController != null)
		{
			rightStartPossessedController.startedPossess = false;
			rightStartPossessedController = null;
		}
		if (leapRightPossessedController != null && (specificController == null || leapRightPossessedController == specificController))
		{
			leapRightPossessedController.RestorePreLinkState();
			leapRightPossessedController.possessed = false;
			leapRightPossessedController.startedPossess = false;
			MotionAnimationControl component3 = leapRightPossessedController.GetComponent<MotionAnimationControl>();
			if (component3 != null)
			{
				component3.suspendPositionPlayback = false;
				component3.suspendRotationPlayback = false;
			}
			leapRightPossessedController = null;
			if (leapRightPossessHandControl != null)
			{
				leapRightPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
		}
		if (leapRightStartPossessedController != null)
		{
			leapRightStartPossessedController.startedPossess = false;
			leapRightStartPossessedController = null;
		}
		if (leapLeftPossessedController != null && (specificController == null || leapLeftPossessedController == specificController))
		{
			leapLeftPossessedController.RestorePreLinkState();
			leapLeftPossessedController.possessed = false;
			leapLeftPossessedController.startedPossess = false;
			MotionAnimationControl component4 = leapLeftPossessedController.GetComponent<MotionAnimationControl>();
			if (component4 != null)
			{
				component4.suspendPositionPlayback = false;
				component4.suspendRotationPlayback = false;
			}
			leapLeftPossessedController = null;
			if (leapLeftPossessHandControl != null)
			{
				leapLeftPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
		}
		if (leapLeftStartPossessedController != null)
		{
			leapLeftStartPossessedController.startedPossess = false;
			leapLeftStartPossessedController = null;
		}
		if (tracker1PossessedController != null && (specificController == null || tracker1PossessedController == specificController))
		{
			tracker1PossessedController.RestorePreLinkState();
			tracker1PossessedController.possessed = false;
			tracker1PossessedController.startedPossess = false;
			MotionAnimationControl component5 = tracker1PossessedController.GetComponent<MotionAnimationControl>();
			if (component5 != null)
			{
				component5.suspendPositionPlayback = false;
				component5.suspendRotationPlayback = false;
			}
			tracker1PossessedController = null;
			tracker1Visible = true;
		}
		if (tracker1StartPossessedController != null)
		{
			tracker1StartPossessedController.startedPossess = false;
			tracker1StartPossessedController = null;
		}
		if (tracker2PossessedController != null && (specificController == null || tracker2PossessedController == specificController))
		{
			tracker2PossessedController.RestorePreLinkState();
			tracker2PossessedController.possessed = false;
			tracker2PossessedController.startedPossess = false;
			MotionAnimationControl component6 = tracker2PossessedController.GetComponent<MotionAnimationControl>();
			if (component6 != null)
			{
				component6.suspendPositionPlayback = false;
				component6.suspendRotationPlayback = false;
			}
			tracker2PossessedController = null;
			tracker2Visible = true;
		}
		if (tracker2StartPossessedController != null)
		{
			tracker2StartPossessedController.startedPossess = false;
			tracker2StartPossessedController = null;
		}
		if (tracker3PossessedController != null && (specificController == null || tracker3PossessedController == specificController))
		{
			tracker3PossessedController.RestorePreLinkState();
			tracker3PossessedController.possessed = false;
			tracker3PossessedController.startedPossess = false;
			MotionAnimationControl component7 = tracker3PossessedController.GetComponent<MotionAnimationControl>();
			if (component7 != null)
			{
				component7.suspendPositionPlayback = false;
				component7.suspendRotationPlayback = false;
			}
			tracker3PossessedController = null;
			tracker3Visible = true;
		}
		if (tracker3StartPossessedController != null)
		{
			tracker3StartPossessedController.startedPossess = false;
			tracker3StartPossessedController = null;
		}
		if (tracker4PossessedController != null && (specificController == null || tracker4PossessedController == specificController))
		{
			tracker4PossessedController.RestorePreLinkState();
			tracker4PossessedController.possessed = false;
			tracker4PossessedController.startedPossess = false;
			MotionAnimationControl component8 = tracker4PossessedController.GetComponent<MotionAnimationControl>();
			if (component8 != null)
			{
				component8.suspendPositionPlayback = false;
				component8.suspendRotationPlayback = false;
			}
			tracker4PossessedController = null;
			tracker4Visible = true;
		}
		if (tracker5StartPossessedController != null)
		{
			tracker5StartPossessedController.startedPossess = false;
			tracker5StartPossessedController = null;
		}
		if (tracker5PossessedController != null && (specificController == null || tracker5PossessedController == specificController))
		{
			tracker5PossessedController.RestorePreLinkState();
			tracker5PossessedController.possessed = false;
			tracker5PossessedController.startedPossess = false;
			MotionAnimationControl component9 = tracker5PossessedController.GetComponent<MotionAnimationControl>();
			if (component9 != null)
			{
				component9.suspendPositionPlayback = false;
				component9.suspendRotationPlayback = false;
			}
			tracker5PossessedController = null;
			tracker5Visible = true;
		}
		if (tracker6StartPossessedController != null)
		{
			tracker6StartPossessedController.startedPossess = false;
			tracker6StartPossessedController = null;
		}
		if (tracker6PossessedController != null && (specificController == null || tracker6PossessedController == specificController))
		{
			tracker6PossessedController.RestorePreLinkState();
			tracker6PossessedController.possessed = false;
			tracker6PossessedController.startedPossess = false;
			MotionAnimationControl component10 = tracker6PossessedController.GetComponent<MotionAnimationControl>();
			if (component10 != null)
			{
				component10.suspendPositionPlayback = false;
				component10.suspendRotationPlayback = false;
			}
			tracker6PossessedController = null;
			tracker6Visible = true;
		}
		if (tracker7StartPossessedController != null)
		{
			tracker7StartPossessedController.startedPossess = false;
			tracker7StartPossessedController = null;
		}
		if (tracker7PossessedController != null && (specificController == null || tracker7PossessedController == specificController))
		{
			tracker7PossessedController.RestorePreLinkState();
			tracker7PossessedController.possessed = false;
			tracker7PossessedController.startedPossess = false;
			MotionAnimationControl component11 = tracker7PossessedController.GetComponent<MotionAnimationControl>();
			if (component11 != null)
			{
				component11.suspendPositionPlayback = false;
				component11.suspendRotationPlayback = false;
			}
			tracker7PossessedController = null;
			tracker7Visible = true;
		}
		if (tracker8StartPossessedController != null)
		{
			tracker8StartPossessedController.startedPossess = false;
			tracker8StartPossessedController = null;
		}
		if (tracker8PossessedController != null && (specificController == null || tracker8PossessedController == specificController))
		{
			tracker8PossessedController.RestorePreLinkState();
			tracker8PossessedController.possessed = false;
			tracker8PossessedController.startedPossess = false;
			MotionAnimationControl component12 = tracker8PossessedController.GetComponent<MotionAnimationControl>();
			if (component12 != null)
			{
				component12.suspendPositionPlayback = false;
				component12.suspendRotationPlayback = false;
			}
			tracker8PossessedController = null;
			tracker8Visible = true;
		}
		if (headPossessedController != null && !excludeHeadClear && (specificController == null || headPossessedController == specificController))
		{
			headPossessedController.RestorePreLinkState();
			headPossessedController.possessed = false;
			MotionAnimationControl component13 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component13 != null)
			{
				component13.suspendPositionPlayback = false;
				component13.suspendRotationPlayback = false;
			}
			headPossessedController = null;
			if (headPossessedActivateTransform != null)
			{
				headPossessedActivateTransform.gameObject.SetActive(false);
			}
		}
		if (headStartPossessedController != null)
		{
			headStartPossessedController.startedPossess = false;
			headStartPossessedController = null;
		}
	}

	protected void VerifyPossess()
	{
		if (rightPossessedController != null)
		{
			Rigidbody component = motionControllerRight.GetComponent<Rigidbody>();
			if (component != null && rightPossessedController.linkToRB != component)
			{
				ClearPossess(true, rightPossessedController);
			}
			else if (rightPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && rightPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && rightPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && rightPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, rightPossessedController);
			}
		}
		if (leftPossessedController != null)
		{
			Rigidbody component2 = motionControllerLeft.GetComponent<Rigidbody>();
			if (component2 != null && leftPossessedController.linkToRB != component2)
			{
				ClearPossess(true, leftPossessedController);
			}
			else if (leftPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leftPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leftPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leftPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, leftPossessedController);
			}
		}
		if (headPossessedController != null)
		{
			Rigidbody component3 = motionControllerHead.GetComponent<Rigidbody>();
			if (component3 != null && headPossessedController.linkToRB != component3)
			{
				ClearPossess(true, headPossessedController);
			}
			else if (headPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && headPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && headPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && headPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, headPossessedController);
			}
		}
		if (leapRightPossessedController != null)
		{
			Rigidbody component4 = leapHandRight.GetComponent<Rigidbody>();
			if (component4 != null && leapRightPossessedController.linkToRB != component4)
			{
				ClearPossess(true, leapRightPossessedController);
			}
			else if (leapRightPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leapRightPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leapRightPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leapRightPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, leapRightPossessedController);
			}
		}
		if (leapLeftPossessedController != null)
		{
			Rigidbody component5 = leapHandLeft.GetComponent<Rigidbody>();
			if (component5 != null && leapLeftPossessedController.linkToRB != component5)
			{
				ClearPossess(true, leapLeftPossessedController);
			}
			else if (leapLeftPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leapLeftPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leapLeftPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leapLeftPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, leapLeftPossessedController);
			}
		}
		if (tracker1PossessedController != null)
		{
			Rigidbody component6 = viveTracker1.GetComponent<Rigidbody>();
			if (component6 != null && tracker1PossessedController.linkToRB != component6)
			{
				ClearPossess(true, tracker1PossessedController);
			}
			else if (tracker1PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker1PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker1PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker1PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker1PossessedController);
			}
		}
		if (tracker2PossessedController != null)
		{
			Rigidbody component7 = viveTracker2.GetComponent<Rigidbody>();
			if (component7 != null && tracker2PossessedController.linkToRB != component7)
			{
				ClearPossess(true, tracker2PossessedController);
			}
			else if (tracker2PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker2PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker2PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker2PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker2PossessedController);
			}
		}
		if (tracker3PossessedController != null)
		{
			Rigidbody component8 = viveTracker3.GetComponent<Rigidbody>();
			if (component8 != null && tracker3PossessedController.linkToRB != component8)
			{
				ClearPossess(true, tracker3PossessedController);
			}
			else if (tracker3PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker3PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker3PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker3PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker3PossessedController);
			}
		}
		if (tracker4PossessedController != null)
		{
			Rigidbody component9 = viveTracker4.GetComponent<Rigidbody>();
			if (component9 != null && tracker4PossessedController.linkToRB != component9)
			{
				ClearPossess(true, tracker4PossessedController);
			}
			else if (tracker4PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker4PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker4PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker4PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker4PossessedController);
			}
		}
		if (tracker5PossessedController != null)
		{
			Rigidbody component10 = viveTracker5.GetComponent<Rigidbody>();
			if (component10 != null && tracker5PossessedController.linkToRB != component10)
			{
				ClearPossess(true, tracker5PossessedController);
			}
			else if (tracker5PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker5PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker5PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker5PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker5PossessedController);
			}
		}
		if (tracker6PossessedController != null)
		{
			Rigidbody component11 = viveTracker6.GetComponent<Rigidbody>();
			if (component11 != null && tracker6PossessedController.linkToRB != component11)
			{
				ClearPossess(true, tracker6PossessedController);
			}
			else if (tracker6PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker6PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker6PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker6PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker6PossessedController);
			}
		}
		if (tracker7PossessedController != null)
		{
			Rigidbody component12 = viveTracker7.GetComponent<Rigidbody>();
			if (component12 != null && tracker7PossessedController.linkToRB != component12)
			{
				ClearPossess(true, tracker7PossessedController);
			}
			else if (tracker7PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker7PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker7PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker7PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker7PossessedController);
			}
		}
		if (tracker8PossessedController != null)
		{
			Rigidbody component13 = viveTracker8.GetComponent<Rigidbody>();
			if (component13 != null && tracker8PossessedController.linkToRB != component13)
			{
				ClearPossess(true, tracker8PossessedController);
			}
			else if (tracker8PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker8PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker8PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker8PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(true, tracker8PossessedController);
			}
		}
	}

	public void StopPlayback()
	{
		if (currentAnimationMaster != null)
		{
			if (currentAnimationMaster.isRecording)
			{
				currentAnimationMaster.StopRecord();
				SelectModeOff();
			}
			else
			{
				currentAnimationMaster.StopPlayback();
			}
		}
	}

	public void StopRecording()
	{
		if (currentAnimationMaster != null && currentAnimationMaster.isRecording)
		{
			currentAnimationMaster.StopRecord();
			SelectModeOff();
		}
	}

	public void StartPlayback()
	{
		if (currentAnimationMaster != null)
		{
			currentAnimationMaster.StartPlayback();
		}
	}

	public void ProcessAnimationRecord()
	{
		if (!(currentAnimationMaster != null))
		{
			return;
		}
		if (currentAnimationMaster.isRecording)
		{
			helpText = "Recording...press Select or Spacebar to stop recording\n" + currentAnimationMaster.playbackCounter.ToString("F0");
		}
		if (!GetLeftSelect() && !GetRightSelect() && !GetMouseSelect() && !Input.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		if (currentAnimationMaster.isRecording)
		{
			StopPlayback();
			return;
		}
		helpText = "Recording...press Select or Spacebar key to stop recording\n" + currentAnimationMaster.playbackCounter.ToString("F0");
		helpColor = Color.red;
		if (currentAnimationMaster != null)
		{
			currentAnimationMaster.StartRecord();
		}
	}

	public void ArmAllControlledControllersForRecord(ICollection<MotionAnimationControl> filteredMacs = null)
	{
		foreach (FreeControllerV3 allController in allControllers)
		{
			if (allController.currentPositionState == FreeControllerV3.PositionState.ParentLink || allController.currentPositionState == FreeControllerV3.PositionState.PhysicsLink || allController.currentRotationState == FreeControllerV3.RotationState.ParentLink || allController.currentRotationState == FreeControllerV3.RotationState.PhysicsLink)
			{
				MotionAnimationControl component = allController.GetComponent<MotionAnimationControl>();
				if (component != null && (filteredMacs == null || filteredMacs.Contains(component)) && (CheckIfControllerLinkedToMotionControl(motionControllerLeft, allController) || CheckIfControllerLinkedToMotionControl(motionControllerRight, allController) || CheckIfControllerLinkedToMotionControl(motionControllerHead, allController) || (viveTracker1 != null && CheckIfControllerLinkedToMotionControl(viveTracker1.transform, allController)) || (viveTracker2 != null && CheckIfControllerLinkedToMotionControl(viveTracker2.transform, allController)) || (viveTracker3 != null && CheckIfControllerLinkedToMotionControl(viveTracker3.transform, allController)) || (viveTracker4 != null && CheckIfControllerLinkedToMotionControl(viveTracker4.transform, allController)) || (viveTracker5 != null && CheckIfControllerLinkedToMotionControl(viveTracker5.transform, allController)) || (viveTracker6 != null && CheckIfControllerLinkedToMotionControl(viveTracker6.transform, allController)) || (viveTracker7 != null && CheckIfControllerLinkedToMotionControl(viveTracker7.transform, allController)) || (viveTracker8 != null && CheckIfControllerLinkedToMotionControl(viveTracker8.transform, allController))))
				{
					component.armedForRecord = true;
				}
			}
		}
	}

	public static IEnumerator AssetManagerReady()
	{
		yield return new WaitUntil(() => _singleton != null && _singleton.assetManagerReady);
	}

	private IEnumerator InitAssetManager()
	{
		AssetBundleManager.SetSourceAssetBundleDirectory(AssetBundleManager.GetStreamingAssetsDirectory() + "/");
		AssetBundleLoadManifestOperation request = AssetBundleManager.Initialize();
		if (request != null)
		{
			yield return StartCoroutine(request);
		}
		UnityEngine.Debug.Log("Asset Manager Ready");
		_assetManagerReady = true;
	}

	protected void InitAssetBundleDictionaries()
	{
		if (assetBundleAssetNameToPrefab == null)
		{
			assetBundleAssetNameToPrefab = new Dictionary<string, GameObject>();
		}
		if (assetBundleAssetNameRefCounts == null)
		{
			assetBundleAssetNameRefCounts = new Dictionary<string, int>();
		}
	}

	public GameObject GetCachedPrefab(string assetBundleName, string assetName)
	{
		string key = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		GameObject value = null;
		if (assetBundleAssetNameToPrefab.TryGetValue(assetBundleName + ":" + assetName, out value))
		{
			int value2;
			if (assetBundleAssetNameRefCounts.TryGetValue(key, out value2))
			{
				value2++;
				assetBundleAssetNameRefCounts.Remove(key);
				assetBundleAssetNameRefCounts.Add(key, value2);
				AssetBundleManager.RegisterAssetBundleAdditionalUse(assetBundleName);
			}
			else
			{
				UnityEngine.Debug.LogError("Asset bundle ref count dictionary corruption");
			}
		}
		return value;
	}

	public void RegisterPrefab(string assetBundleName, string assetName, GameObject prefab)
	{
		string key = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		int value;
		if (assetBundleAssetNameRefCounts.TryGetValue(key, out value))
		{
			value++;
			assetBundleAssetNameRefCounts.Remove(key);
		}
		else
		{
			value = 1;
			assetBundleAssetNameToPrefab.Add(key, prefab);
		}
		assetBundleAssetNameRefCounts.Add(key, value);
	}

	public void UnregisterPrefab(string assetBundleName, string assetName)
	{
		string text = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		int value;
		if (assetBundleAssetNameRefCounts.TryGetValue(text, out value))
		{
			value--;
			assetBundleAssetNameRefCounts.Remove(text);
			if (value == 0)
			{
				assetBundleAssetNameToPrefab.Remove(text);
			}
			else
			{
				assetBundleAssetNameRefCounts.Add(text, value);
			}
			AssetBundleManager.UnloadAssetBundle(assetBundleName);
		}
		else
		{
			LogError("Tried to UnregisterPrefab " + text + " that was not registered");
		}
	}

	protected void UnregisterAllPrefabsFromAtoms()
	{
		if (assetBundleAssetNameRefCounts == null)
		{
			return;
		}
		foreach (AtomAsset value2 in atomAssetByType.Values)
		{
			string key = value2.assetBundleName + ":" + value2.assetName;
			int value;
			if (assetBundleAssetNameRefCounts.TryGetValue(key, out value))
			{
				assetBundleAssetNameRefCounts.Remove(key);
				assetBundleAssetNameToPrefab.Remove(key);
				for (int i = 0; i < value; i++)
				{
					AssetBundleManager.UnloadAssetBundle(value2.assetBundleName);
				}
			}
		}
	}

	protected IEnumerator LoadAtomFromBundleAsync(AtomAsset aa, string useuid = null, bool userInvoked = false, bool forceSelect = false, bool forceFocus = false)
	{
		yield return AssetManagerReady();
		float startTime = Time.realtimeSinceStartup;
		GameObject go = GetCachedPrefab(aa.assetBundleName, aa.assetName);
		if (go == null)
		{
			AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(aa.assetBundleName, aa.assetName, typeof(GameObject));
			if (request == null)
			{
				Error("Failed to load Atom " + aa.assetName);
				yield break;
			}
			yield return StartCoroutine(request);
			go = request.GetAsset<GameObject>();
			if (go != null)
			{
				RegisterPrefab(aa.assetBundleName, aa.assetName, go);
			}
			else
			{
				Error("Asset " + aa.assetName + " is missing game object");
			}
		}
		if (!(go != null))
		{
			yield break;
		}
		Atom component = go.GetComponent<Atom>();
		if (component != null)
		{
			startTime = Time.realtimeSinceStartup;
			Transform transform = AddAtom(component, useuid, userInvoked, forceSelect, forceFocus);
			if (transform != null)
			{
				Atom component2 = transform.GetComponent<Atom>();
				if (component2 != null)
				{
					component2.loadedFromBundle = true;
				}
			}
		}
		else
		{
			Error("Asset " + aa.assetName + " is missing Atom component");
		}
	}

	public void PauseSyncAtomLists()
	{
		_pauseSyncAtomLists = true;
	}

	public void ResumeSyncAtomLists()
	{
		_pauseSyncAtomLists = false;
		SyncSortedAtomUIDs();
		SyncSortedAtomUIDsWithForceProducers();
		SyncSortedAtomUIDsWithForceReceivers();
		SyncSortedAtomUIDsWithFreeControllers();
		SyncSortedAtomUIDsWithRhythmControllers();
		SyncSortedAtomUIDsWithAudioSourceControls();
		SyncSortedAtomUIDsWithRigidbodies();
		SyncHiddenAtoms();
		SyncSelectAtomPopup();
	}

	private void SyncSortedAtomUIDs()
	{
		if (!_isLoading)
		{
			sortedAtomUIDs.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithForceReceivers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithForceReceivers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithForceProducers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithForceProducers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithRhythmControllers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithRhythmControllers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithAudioSourceControls()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithAudioSourceControls.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithFreeControllers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithFreeControllers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithRigidbodies()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithRigidbodies.Sort();
		}
	}

	public void SyncHiddenAtoms()
	{
		if (_isLoading)
		{
			return;
		}
		if (hiddenAtomUIDs == null)
		{
			hiddenAtomUIDs = new List<string>();
		}
		else
		{
			hiddenAtomUIDs.Clear();
		}
		if (visibleAtomUIDs == null)
		{
			visibleAtomUIDs = new List<string>();
		}
		else
		{
			visibleAtomUIDs.Clear();
		}
		foreach (string sortedAtomUID in sortedAtomUIDs)
		{
			Atom atomByUid = GetAtomByUid(sortedAtomUID);
			if (atomByUid != null)
			{
				if (atomByUid.hidden || atomByUid.tempHidden)
				{
					hiddenAtomUIDs.Add(sortedAtomUID);
				}
				else
				{
					visibleAtomUIDs.Add(sortedAtomUID);
				}
			}
		}
		if (hiddenAtomUIDsWithFreeControllers == null)
		{
			hiddenAtomUIDsWithFreeControllers = new List<string>();
		}
		else
		{
			hiddenAtomUIDsWithFreeControllers.Clear();
		}
		if (visibleAtomUIDsWithFreeControllers == null)
		{
			visibleAtomUIDsWithFreeControllers = new List<string>();
		}
		else
		{
			visibleAtomUIDsWithFreeControllers.Clear();
		}
		foreach (string sortedAtomUIDsWithFreeController in sortedAtomUIDsWithFreeControllers)
		{
			Atom atomByUid2 = GetAtomByUid(sortedAtomUIDsWithFreeController);
			if (atomByUid2 != null)
			{
				if (atomByUid2.hidden || atomByUid2.tempHidden)
				{
					hiddenAtomUIDsWithFreeControllers.Add(sortedAtomUIDsWithFreeController);
				}
				else
				{
					visibleAtomUIDsWithFreeControllers.Add(sortedAtomUIDsWithFreeController);
				}
			}
		}
		SyncSelectAtomPopup();
		SyncVisibility();
	}

	protected void DetermineAtomsInAtom(Atom startingAtom, Atom atom, HashSet<Atom> atomsHash, HashSet<Atom> nestedSubSceneAtomsHash, bool isInNestedSubScene = false)
	{
		bool flag = isInNestedSubScene;
		if (!flag && atom != startingAtom && atom.isSubSceneType)
		{
			flag = true;
		}
		foreach (Atom child in atom.GetChildren())
		{
			DetermineAtomsInAtom(startingAtom, child, atomsHash, nestedSubSceneAtomsHash, flag);
		}
		if (isInNestedSubScene)
		{
			nestedAtomsInSubSceneHash.Add(atom);
		}
		else
		{
			atomsHash.Add(atom);
		}
	}

	protected void SyncIsolatedSubScene()
	{
		bool flag = false;
		if (_isolatedSubScene != null)
		{
			if (atomsInSubSceneHash == null)
			{
				atomsInSubSceneHash = new HashSet<Atom>();
			}
			else
			{
				atomsInSubSceneHash.Clear();
			}
			if (nestedAtomsInSubSceneHash == null)
			{
				nestedAtomsInSubSceneHash = new HashSet<Atom>();
			}
			else
			{
				nestedAtomsInSubSceneHash.Clear();
			}
			DetermineAtomsInAtom(_isolatedSubScene.containingAtom, _isolatedSubScene.containingAtom, atomsInSubSceneHash, nestedAtomsInSubSceneHash);
			foreach (Atom atoms in atomsList)
			{
				if (atomsInSubSceneHash.Contains(atoms))
				{
					if (atoms.tempHidden)
					{
						atoms.tempHidden = false;
						flag = true;
					}
					atoms.tempFreezePhysics = false;
					atoms.tempDisableCollision = false;
					atoms.tempDisableRender = false;
				}
				else if (nestedAtomsInSubSceneHash.Contains(atoms))
				{
					if (!atoms.tempHidden)
					{
						atoms.tempHidden = true;
						flag = true;
					}
					atoms.tempFreezePhysics = false;
					atoms.tempDisableCollision = false;
					atoms.tempDisableRender = false;
				}
				else
				{
					if (!atoms.tempHidden)
					{
						atoms.tempHidden = true;
						flag = true;
					}
					atoms.tempFreezePhysics = _freezePhysicsForAtomsNotInIsolatedSubScene;
					atoms.tempDisableCollision = _disableCollisionForAtomsNotInIsolatedSubScene;
					atoms.tempDisableRender = _disableRenderForAtomsNotInIsolatedSubScene;
				}
			}
		}
		else
		{
			foreach (Atom atoms2 in atomsList)
			{
				if (atoms2.tempHidden)
				{
					atoms2.tempHidden = false;
					flag = true;
				}
				atoms2.tempFreezePhysics = false;
				atoms2.tempDisableCollision = false;
				atoms2.tempDisableRender = false;
			}
		}
		if (flag)
		{
			SyncHiddenAtoms();
		}
	}

	public void StartIsolateEditSubScene(SubScene subScene)
	{
		EndIsolateEditAtom();
		EndIsolateEditSubScene();
		isolatedSubScene = subScene;
		isolatedSubScene.isIsolateEditing = true;
	}

	public void EndIsolateEditSubScene()
	{
		if (isolatedSubScene != null)
		{
			isolatedSubScene.isIsolateEditing = false;
		}
		isolatedSubScene = null;
	}

	public void QuickSaveIsolatedSubScene()
	{
		if (_isolatedSubScene != null)
		{
			_isolatedSubScene.StoreSubScene();
		}
	}

	public void QuickReloadIsolatedSubScene()
	{
		if (isolatedSubScene != null)
		{
			_isolatedSubScene.LoadSubScene();
		}
	}

	public void SelectIsolatedSubScene()
	{
		if (_isolatedSubScene != null)
		{
			SelectController(_isolatedSubScene.containingAtom.mainController);
		}
	}

	protected void SyncIsolatedAtom()
	{
		bool flag = false;
		if (_isolatedAtom != null)
		{
			foreach (Atom atoms in atomsList)
			{
				if (atoms == _isolatedAtom)
				{
					if (atoms.tempHidden)
					{
						atoms.tempHidden = false;
						flag = true;
					}
					atoms.tempFreezePhysics = false;
					atoms.tempDisableCollision = false;
					atoms.tempDisableRender = false;
				}
				else
				{
					if (!atoms.tempHidden)
					{
						atoms.tempHidden = true;
						flag = true;
					}
					atoms.tempFreezePhysics = _freezePhysicsForAtomsNotInIsolatedAtom;
					atoms.tempDisableCollision = _disableCollisionForAtomsNotInIsolatedAtom;
					atoms.tempDisableRender = _disableRenderForAtomsNotInIsolatedAtom;
				}
			}
		}
		else
		{
			foreach (Atom atoms2 in atomsList)
			{
				if (atoms2.tempHidden)
				{
					atoms2.tempHidden = false;
					flag = true;
				}
				atoms2.tempFreezePhysics = false;
				atoms2.tempDisableCollision = false;
				atoms2.tempDisableRender = false;
			}
		}
		if (flag)
		{
			SyncHiddenAtoms();
		}
	}

	public void StartIsolateEditAtom(Atom atom)
	{
		EndIsolateEditAtom();
		EndIsolateEditSubScene();
		isolatedAtom = atom;
	}

	public void EndIsolateEditAtom()
	{
		isolatedAtom = null;
	}

	public void SelectIsolatedAtom()
	{
		if (_isolatedAtom != null)
		{
			SelectController(_isolatedAtom.mainController);
		}
	}

	public void ToggleShowHiddenAtoms()
	{
		showHiddenAtoms = !_showHiddenAtoms;
	}

	public List<FreeControllerV3> GetAllFreeControllers()
	{
		return allControllers;
	}

	public List<AnimationPattern> GetAllAnimationPatterns()
	{
		return allAnimationPatterns;
	}

	public List<AnimationStep> GetAllAnimationSteps()
	{
		return allAnimationSteps;
	}

	private string CreateUID(string name)
	{
		if (!uids.ContainsKey(name))
		{
			uids.Add(name, true);
			return name;
		}
		for (int i = 2; i < maxUID; i++)
		{
			string text = name + "#" + i;
			if (!uids.ContainsKey(text))
			{
				uids.Add(text, true);
				return text;
			}
		}
		Error("Exceeded UID limit of " + maxUID + " for " + name);
		return null;
	}

	public string GetTempUID()
	{
		string text = Guid.NewGuid().ToString();
		while (uids.ContainsKey(text))
		{
			text = Guid.NewGuid().ToString();
		}
		uids.Add(text, true);
		return text;
	}

	public void ReleaseTempUID(string uid)
	{
		uids.Remove(uid);
	}

	private void SyncForceReceiverNames()
	{
		_forceReceiverNames = new string[frMap.Keys.Count];
		frMap.Keys.CopyTo(_forceReceiverNames, 0);
		if (onForceReceiverNamesChangedHandlers != null)
		{
			onForceReceiverNamesChangedHandlers(_forceReceiverNames);
		}
	}

	public List<string> GetForceReceiverNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			ForceReceiver[] forceReceivers = value.forceReceivers;
			foreach (ForceReceiver forceReceiver in forceReceivers)
			{
				list.Add(forceReceiver.name);
			}
		}
		return list;
	}

	public ForceReceiver ReceiverNameToForceReceiver(string receiverName)
	{
		ForceReceiver value;
		if (frMap != null && receiverName != null && frMap.TryGetValue(receiverName, out value))
		{
			return value;
		}
		return null;
	}

	private void SyncForceProducerNames()
	{
		_forceProducerNames = new string[fpMap.Keys.Count];
		fpMap.Keys.CopyTo(_forceProducerNames, 0);
		if (onForceProducerNamesChangedHandlers != null)
		{
			onForceProducerNamesChangedHandlers(_forceProducerNames);
		}
	}

	public List<string> GetForceProducerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			ForceProducerV2[] forceProducers = value.forceProducers;
			foreach (ForceProducerV2 forceProducerV in forceProducers)
			{
				list.Add(forceProducerV.name);
			}
		}
		return list;
	}

	public ForceProducerV2 ProducerNameToForceProducer(string producerName)
	{
		ForceProducerV2 value;
		if (fpMap != null && producerName != null && fpMap.TryGetValue(producerName, out value))
		{
			return value;
		}
		return null;
	}

	private void SyncRhythmControllerNames()
	{
		_rhythmControllerNames = new string[rcMap.Keys.Count];
		rcMap.Keys.CopyTo(_rhythmControllerNames, 0);
		if (onRhythmControllerNamesChangedHandlers != null)
		{
			onRhythmControllerNamesChangedHandlers(_rhythmControllerNames);
		}
	}

	public List<string> GetRhythmControllerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			RhythmController[] rhythmControllers = value.rhythmControllers;
			foreach (RhythmController rhythmController in rhythmControllers)
			{
				list.Add(rhythmController.name);
			}
		}
		return list;
	}

	public RhythmController RhythmControllerrNameToRhythmController(string controllerName)
	{
		RhythmController value;
		if (rcMap != null && controllerName != null && rcMap.TryGetValue(controllerName, out value))
		{
			return value;
		}
		return null;
	}

	private void SyncAudioSourceControlNames()
	{
		_audioSourceControlNames = new string[ascMap.Keys.Count];
		ascMap.Keys.CopyTo(_audioSourceControlNames, 0);
		if (onAudioSourceControlNamesChangedHandlers != null)
		{
			onAudioSourceControlNamesChangedHandlers(_audioSourceControlNames);
		}
	}

	public List<string> GetAudioSourceControlNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			AudioSourceControl[] audioSourceControls = value.audioSourceControls;
			foreach (AudioSourceControl audioSourceControl in audioSourceControls)
			{
				list.Add(audioSourceControl.name);
			}
		}
		return list;
	}

	public AudioSourceControl AudioSourceControlrNameToAudioSourceControl(string controllerName)
	{
		AudioSourceControl value;
		if (ascMap != null && controllerName != null && ascMap.TryGetValue(controllerName, out value))
		{
			return value;
		}
		return null;
	}

	private void SyncFreeControllerNames()
	{
		_freeControllerNames = new string[fcMap.Keys.Count];
		fcMap.Keys.CopyTo(_freeControllerNames, 0);
		if (onFreeControllerNamesChangedHandlers != null)
		{
			onFreeControllerNamesChangedHandlers(_freeControllerNames);
		}
	}

	public List<string> GetFreeControllerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			FreeControllerV3[] freeControllers = value.freeControllers;
			foreach (FreeControllerV3 freeControllerV in freeControllers)
			{
				list.Add(freeControllerV.name);
			}
		}
		return list;
	}

	public FreeControllerV3 FreeControllerNameToFreeController(string controllerName)
	{
		FreeControllerV3 value;
		if (fcMap != null && controllerName != null && fcMap.TryGetValue(controllerName, out value))
		{
			return value;
		}
		return null;
	}

	private void SyncRigidbodyNames()
	{
		_rigidbodyNames = new string[rbMap.Keys.Count];
		rbMap.Keys.CopyTo(_rigidbodyNames, 0);
		if (onRigidbodyNamesChangedHandlers != null)
		{
			onRigidbodyNamesChangedHandlers(_rigidbodyNames);
		}
	}

	public List<string> GetRigidbodyNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		Atom value;
		if (atomUID != null && atoms.TryGetValue(atomUID, out value))
		{
			Rigidbody[] linkableRigidbodies = value.linkableRigidbodies;
			foreach (Rigidbody rigidbody in linkableRigidbodies)
			{
				list.Add(rigidbody.name);
			}
		}
		return list;
	}

	public Rigidbody RigidbodyNameToRigidbody(string rigidbodyName)
	{
		Rigidbody value;
		if (rbMap != null && rigidbodyName != null && rbMap.TryGetValue(rigidbodyName, out value))
		{
			return value;
		}
		return null;
	}

	public List<string> GetAtomCategories()
	{
		return atomCategories;
	}

	protected virtual void SetAddAtomAtomPopupValues(string category)
	{
		if (!(atomPrefabPopup != null) || category == null)
		{
			return;
		}
		List<string> value;
		if (atomCategoryToAtomTypes != null && atomCategoryToAtomTypes.TryGetValue(category, out value))
		{
			int num = 0;
			atomPrefabPopup.numPopupValues = value.Count;
			foreach (string item in value)
			{
				atomPrefabPopup.setPopupValue(num, item);
				num++;
			}
			atomPrefabPopup.currentValue = "None";
		}
		else
		{
			atomPrefabPopup.numPopupValues = 0;
		}
	}

	public List<Atom> GetAtoms()
	{
		return new List<Atom>(atomsList);
	}

	public List<string> GetAtomUIDs()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDs;
		}
		return atomUIDs;
	}

	public List<string> GetVisibleAtomUIDs()
	{
		if (showHiddenAtoms)
		{
			return sortedAtomUIDs;
		}
		if (sortAtomUIDs)
		{
			return visibleAtomUIDs;
		}
		return atomUIDs;
	}

	public List<string> GetAtomUIDsWithForceReceivers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithForceReceivers;
		}
		return atomUIDsWithForceReceivers;
	}

	public List<string> GetAtomUIDsWithForceProducers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithForceProducers;
		}
		return atomUIDsWithForceProducers;
	}

	public List<string> GetAtomUIDsWithRhythmControllers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithRhythmControllers;
		}
		return atomUIDsWithRhythmControllers;
	}

	public List<string> GetAtomUIDsWithAudioSourceControls()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithAudioSourceControls;
		}
		return atomUIDsWithAudioSourceControls;
	}

	public List<string> GetAtomUIDsWithFreeControllers()
	{
		if (showHiddenAtoms)
		{
			return sortedAtomUIDsWithFreeControllers;
		}
		if (sortAtomUIDs)
		{
			return visibleAtomUIDsWithFreeControllers;
		}
		return atomUIDsWithFreeControllers;
	}

	public List<string> GetAtomUIDsWithRigidbodies()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithRigidbodies;
		}
		return atomUIDsWithRigidbodies;
	}

	public Atom GetAtomByUid(string uid)
	{
		Atom value = null;
		if (uid != null)
		{
			atoms.TryGetValue(uid, out value);
		}
		return value;
	}

	public void NotifySubSceneLoad(SubScene subScene)
	{
		if (onSubSceneLoadedHandlers != null)
		{
			onSubSceneLoadedHandlers(subScene);
		}
	}

	public void AtomParentChanged(Atom atom, Atom newParent)
	{
		if (onAtomParentChangedHandlers != null)
		{
			onAtomParentChangedHandlers(atom, newParent);
		}
		if (!(motionAnimationMaster != null))
		{
			return;
		}
		foreach (MotionAnimationControl value in macMap.Values)
		{
			if (value.animationMaster == null)
			{
				motionAnimationMaster.RegisterAnimationControl(value);
			}
		}
	}

	public void AtomSubSceneChanged(Atom atom, SubScene newSubScene)
	{
		if (onAtomParentChangedHandlers != null)
		{
			onAtomSubSceneChangedHandlers(atom, newSubScene);
		}
	}

	public void AddAtomByPopupValue()
	{
		if (atomPrefabPopup != null && atomPrefabPopup.currentValue != "None")
		{
			StartCoroutine(AddAtomByType(atomPrefabPopup.currentValue, null, true));
		}
	}

	public void OpenAddAtomUIToAtomType(string type)
	{
		bool flag = false;
		Atom[] array = atomPrefabs;
		foreach (Atom atom in array)
		{
			if (type == atom.type)
			{
				atomCategoryPopup.currentValue = atom.category;
				atomPrefabPopup.currentValue = type;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AtomAsset[] array2 = atomAssets;
			foreach (AtomAsset atomAsset in array2)
			{
				if (type == atomAsset.assetName)
				{
					atomCategoryPopup.currentValue = atomAsset.category;
					atomPrefabPopup.currentValue = type;
					break;
				}
			}
		}
		activeUI = ActiveUI.MainMenu;
		SetMainMenuTab("TabAddAtom");
	}

	public void AddAtomByTypeForceSelect(string type)
	{
		AddAtomByType(type, true, true);
	}

	public void AddAtomByType(string type, bool userInvoked = false, bool forceSelect = false, bool forceFocus = false)
	{
		StartCoroutine(AddAtomByType(type, null, userInvoked, forceSelect, forceFocus));
	}

	public IEnumerator AddAtomByType(string type, string useuid = null, bool userInvoked = false, bool forceSelect = false, bool forceFocus = false)
	{
		AsyncFlag loadIconFlag = new AsyncFlag("Load Atom " + type);
		SetLoadingIconFlag(loadIconFlag);
		if (type != null && type != string.Empty)
		{
			lastAddedAtom = null;
			Atom atomOfTypeFromPool;
			Atom atom = (atomOfTypeFromPool = GetAtomOfTypeFromPool(type));
			AtomAsset aa;
			if ((bool)atomOfTypeFromPool)
			{
				AddAtom(atom, useuid, userInvoked, forceSelect, forceFocus, false);
				if (userInvoked)
				{
					ResetSimulation(5, "AddAtom", true);
				}
			}
			else if (atomAssetByType.TryGetValue(type, out aa))
			{
				yield return StartCoroutine(LoadAtomFromBundleAsync(aa, useuid, userInvoked, forceSelect));
			}
			else if (atomPrefabByType.TryGetValue(type, out atom))
			{
				AddAtom(atom, useuid, userInvoked, forceSelect, forceFocus);
			}
			else
			{
				Error("Atom type " + type + " does not exist. Cannot add");
			}
			if (userInvoked && lastAddedAtom != null && lastAddedAtom.mainPresetControl != null)
			{
				yield return null;
				lastAddedAtom.mainPresetControl.LoadUserDefaults();
			}
		}
		loadIconFlag.Raise();
	}

	public Transform AddAtom(Atom atom, string useuid = null, bool userInvoked = false, bool forceSelect = false, bool forceFocus = false, bool instantiate = true)
	{
		string text = ((useuid == null) ? CreateUID(atom.name) : CreateUID(useuid));
		if (text != null)
		{
			Transform transform;
			if (instantiate)
			{
				transform = UnityEngine.Object.Instantiate(atom.transform);
			}
			else
			{
				atom.destroyed = false;
				transform = atom.transform;
			}
			transform.SetParent(atomContainerTransform, true);
			Atom component = transform.GetComponent<Atom>();
			component.uid = text;
			component.name = text;
			InitAtom(component);
			if (userInvoked)
			{
				SubAtom[] componentsInChildren = atom.GetComponentsInChildren<SubAtom>();
				SubAtom[] array = componentsInChildren;
				foreach (SubAtom subAtom in array)
				{
					Atom atomPrefab = subAtom.atomPrefab;
					if (atomPrefab != null)
					{
						Transform transform2 = AddAtom(atomPrefab);
						if (transform2 != null)
						{
							transform2.position = subAtom.transform.position;
							transform2.rotation = subAtom.transform.rotation;
						}
					}
				}
			}
			// An atom added after the load brought its own materials in with it, and nothing else
			// sweeps them. Scoped to the atom: a whole-scene sweep belongs to the end of a load.
			MeshVR.VamShaderProvider.UseProjectShaders(component.gameObject);
			if (onAtomAddedHandlers != null)
			{
				onAtomAddedHandlers(component);
			}
			if (onAtomUIDsChangedHandlers != null)
			{
				onAtomUIDsChangedHandlers(GetAtomUIDs());
			}
			if (onAtomUIDsWithForceReceiversChangedHandlers != null && component.forceReceivers.Length > 0)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
			if (onAtomUIDsWithForceProducersChangedHandlers != null && component.forceProducers.Length > 0)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
			if (onAtomUIDsWithFreeControllersChangedHandlers != null && component.freeControllers.Length > 0)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null && component.linkableRigidbodies.Length > 0)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
			SyncVisibility();
			SyncForceReceiverNames();
			SyncForceProducerNames();
			SyncFreeControllerNames();
			SyncRigidbodyNames();
			SyncHiddenAtoms();
			SyncSelectAtomPopup();
			if (forceSelect || (userInvoked && component.mainController != null && selectAtomOnAddToggle != null && selectAtomOnAddToggle.isOn))
			{
				SelectController(component.mainController);
			}
			if (forceFocus || (userInvoked && component.mainController != null && focusAtomOnAddToggle != null && focusAtomOnAddToggle.isOn))
			{
				FocusOnController(component.mainController, false);
			}
			lastAddedAtom = component;
			return transform;
		}
		return null;
	}

	public void RenameAtom(Atom atom, string requestedID)
	{
		if (!(atom != null) || atom.uid == requestedID)
		{
			return;
		}
		string uid = atom.uid;
		string text = CreateUID(requestedID);
		atoms.Remove(atom.uid);
		uids.Remove(atom.uid);
		int num = -1;
		if (atomUIDs.Contains(atom.uid))
		{
			num = atomUIDs.IndexOf(atom.uid);
			atomUIDs.RemoveAt(num);
			sortedAtomUIDs.Remove(atom.uid);
		}
		int num2 = -1;
		if (atomUIDsWithForceReceivers.Contains(atom.uid))
		{
			num2 = atomUIDsWithForceReceivers.IndexOf(atom.uid);
			atomUIDsWithForceReceivers.RemoveAt(num2);
			sortedAtomUIDsWithForceReceivers.Remove(atom.uid);
		}
		int num3 = -1;
		if (atomUIDsWithForceProducers.Contains(atom.uid))
		{
			num3 = atomUIDsWithForceProducers.IndexOf(atom.uid);
			atomUIDsWithForceProducers.RemoveAt(num3);
			sortedAtomUIDsWithForceProducers.Remove(atom.uid);
		}
		int num4 = -1;
		if (atomUIDsWithRhythmControllers.Contains(atom.uid))
		{
			num4 = atomUIDsWithRhythmControllers.IndexOf(atom.uid);
			atomUIDsWithRhythmControllers.RemoveAt(num4);
			sortedAtomUIDsWithRhythmControllers.Remove(atom.uid);
		}
		int num5 = -1;
		if (atomUIDsWithAudioSourceControls.Contains(atom.uid))
		{
			num5 = atomUIDsWithAudioSourceControls.IndexOf(atom.uid);
			atomUIDsWithAudioSourceControls.RemoveAt(num5);
			sortedAtomUIDsWithAudioSourceControls.Remove(atom.uid);
		}
		int num6 = -1;
		if (atomUIDsWithFreeControllers.Contains(atom.uid))
		{
			num6 = atomUIDsWithFreeControllers.IndexOf(atom.uid);
			atomUIDsWithFreeControllers.RemoveAt(num6);
			sortedAtomUIDsWithFreeControllers.Remove(atom.uid);
		}
		int num7 = -1;
		if (atomUIDsWithRigidbodies.Contains(atom.uid))
		{
			num7 = atomUIDsWithRigidbodies.IndexOf(atom.uid);
			atomUIDsWithRigidbodies.RemoveAt(num7);
			sortedAtomUIDsWithRigidbodies.Remove(atom.uid);
		}
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Remove(key);
		}
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Remove(key2);
		}
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			string key3 = atom.uid + ":" + rhythmController.name;
			rcMap.Remove(key3);
		}
		AudioSourceControl[] audioSourceControls = atom.audioSourceControls;
		foreach (AudioSourceControl audioSourceControl in audioSourceControls)
		{
			string key4 = atom.uid + ":" + audioSourceControl.name;
			ascMap.Remove(key4);
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key5 = atom.uid + ":" + grabPoint.name;
			gpMap.Remove(key5);
		}
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			string key6 = atom.uid + ":" + forceReceiver.name;
			frMap.Remove(key6);
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody in linkableRigidbodies)
		{
			string key7 = atom.uid + ":" + rigidbody.name;
			rbMap.Remove(key7);
		}
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
		{
			string key8 = atom.uid + ":" + motionAnimationControl.name;
			macMap.Remove(key8);
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key9 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Remove(key9);
		}
		atom.uid = text;
		atom.name = text;
		atoms.Add(atom.uid, atom);
		if (num != -1)
		{
			atomUIDs.Insert(num, atom.uid);
			sortedAtomUIDs.Add(atom.uid);
			SyncSortedAtomUIDs();
			if (onAtomUIDsChangedHandlers != null)
			{
				onAtomUIDsChangedHandlers(GetAtomUIDs());
			}
		}
		if (num2 != -1)
		{
			atomUIDsWithForceReceivers.Insert(num2, atom.uid);
			sortedAtomUIDsWithForceReceivers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceReceivers();
			if (onAtomUIDsWithForceReceiversChangedHandlers != null)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
		}
		if (num3 != -1)
		{
			atomUIDsWithForceProducers.Insert(num3, atom.uid);
			sortedAtomUIDsWithForceProducers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceProducers();
			if (onAtomUIDsWithForceProducersChangedHandlers != null)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
		}
		if (num4 != -1)
		{
			atomUIDsWithRhythmControllers.Insert(num4, atom.uid);
			sortedAtomUIDsWithRhythmControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithRhythmControllers();
		}
		if (num5 != -1)
		{
			atomUIDsWithAudioSourceControls.Insert(num5, atom.uid);
			sortedAtomUIDsWithAudioSourceControls.Add(atom.uid);
			SyncSortedAtomUIDsWithAudioSourceControls();
		}
		if (num6 != -1)
		{
			atomUIDsWithFreeControllers.Insert(num6, atom.uid);
			sortedAtomUIDsWithFreeControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithFreeControllers();
			if (onAtomUIDsWithFreeControllersChangedHandlers != null)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
		}
		if (num7 != -1)
		{
			atomUIDsWithRigidbodies.Insert(num7, atom.uid);
			sortedAtomUIDsWithRigidbodies.Add(atom.uid);
			SyncSortedAtomUIDsWithRigidbodies();
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
		}
		SyncHiddenAtoms();
		FreeControllerV3[] freeControllers2 = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV2 in freeControllers2)
		{
			string key10 = atom.uid + ":" + freeControllerV2.name;
			fcMap.Add(key10, freeControllerV2);
		}
		ForceProducerV2[] forceProducers2 = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV2 in forceProducers2)
		{
			string key11 = atom.uid + ":" + forceProducerV2.name;
			fpMap.Add(key11, forceProducerV2);
		}
		ForceReceiver[] forceReceivers2 = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver2 in forceReceivers2)
		{
			string key12 = atom.uid + ":" + forceReceiver2.name;
			frMap.Add(key12, forceReceiver2);
		}
		RhythmController[] rhythmControllers2 = atom.rhythmControllers;
		foreach (RhythmController rhythmController2 in rhythmControllers2)
		{
			string key13 = atom.uid + ":" + rhythmController2.name;
			rcMap.Add(key13, rhythmController2);
		}
		AudioSourceControl[] audioSourceControls2 = atom.audioSourceControls;
		foreach (AudioSourceControl audioSourceControl2 in audioSourceControls2)
		{
			string key14 = atom.uid + ":" + audioSourceControl2.name;
			ascMap.Add(key14, audioSourceControl2);
		}
		GrabPoint[] grabPoints2 = atom.grabPoints;
		foreach (GrabPoint grabPoint2 in grabPoints2)
		{
			string key15 = atom.uid + ":" + grabPoint2.name;
			gpMap.Add(key15, grabPoint2);
		}
		Rigidbody[] linkableRigidbodies2 = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody2 in linkableRigidbodies2)
		{
			string key16 = atom.uid + ":" + rigidbody2.name;
			rbMap.Add(key16, rigidbody2);
		}
		MotionAnimationControl[] motionAnimationControls2 = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl2 in motionAnimationControls2)
		{
			string key17 = atom.uid + ":" + motionAnimationControl2.name;
			macMap.Add(key17, motionAnimationControl2);
		}
		PlayerNavCollider[] playerNavColliders2 = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider2 in playerNavColliders2)
		{
			string key18 = atom.uid + ":" + playerNavCollider2.name;
			pncMap.Add(key18, playerNavCollider2);
		}
		if (onAtomUIDRenameHandlers != null)
		{
			onAtomUIDRenameHandlers(uid, text);
		}
		SyncSelectAtomPopup();
	}

	public void RemoveAtom(Atom atom)
	{
		RemoveAtom(atom, true);
	}

	public void RemoveAtom(Atom atom, bool syncList)
	{
		if (!(atom != null) || atom.destroyed)
		{
			return;
		}
		if (atom == isolatedAtom)
		{
			EndIsolateEditAtom();
		}
		atom.OnPreRemove();
		List<Atom> list = new List<Atom>();
		foreach (Atom atoms in atomsList)
		{
			if (atoms != null)
			{
				list.Add(atoms);
			}
		}
		foreach (Atom item4 in list)
		{
			if (item4 != null && item4.parentAtom == atom)
			{
				item4.parentAtom = null;
			}
		}
		if (selectedController != null && selectedController.containingAtom != null && selectedController.containingAtom == atom)
		{
			ClearSelection();
		}
		if (atom.parentAtom != null)
		{
			atom.parentAtom = null;
		}
		this.atoms.Remove(atom.uid);
		if (syncList)
		{
			atomsList.Remove(atom);
		}
		atomUIDs.Remove(atom.uid);
		sortedAtomUIDs.Remove(atom.uid);
		uids.Remove(atom.uid);
		if (this.playerNavCollider != null && this.playerNavCollider.containingAtom == atom)
		{
			DisconnectNavRigFromPlayerNavCollider();
		}
		if (onAtomRemovedHandlers != null)
		{
			onAtomRemovedHandlers(atom);
		}
		if (onAtomUIDsChangedHandlers != null)
		{
			onAtomUIDsChangedHandlers(GetAtomUIDs());
		}
		if (atomUIDsWithForceReceivers.Remove(atom.uid))
		{
			sortedAtomUIDsWithForceReceivers.Remove(atom.uid);
			if (onAtomUIDsWithForceReceiversChangedHandlers != null)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
		}
		if (atomUIDsWithForceProducers.Remove(atom.uid))
		{
			sortedAtomUIDsWithForceProducers.Remove(atom.uid);
			if (onAtomUIDsWithForceProducersChangedHandlers != null)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
		}
		if (atomUIDsWithRhythmControllers.Remove(atom.uid))
		{
			sortedAtomUIDsWithRhythmControllers.Remove(atom.uid);
		}
		if (atomUIDsWithAudioSourceControls.Remove(atom.uid))
		{
			sortedAtomUIDsWithAudioSourceControls.Remove(atom.uid);
		}
		if (atomUIDsWithFreeControllers.Remove(atom.uid))
		{
			sortedAtomUIDsWithFreeControllers.Remove(atom.uid);
			if (onAtomUIDsWithFreeControllersChangedHandlers != null)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
		}
		if (atomUIDsWithRigidbodies.Remove(atom.uid))
		{
			sortedAtomUIDsWithRigidbodies.Remove(atom.uid);
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
		}
		SyncHiddenAtoms();
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			allControllers.Remove(freeControllerV);
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Remove(key);
		}
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Remove(key2);
		}
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			string key3 = atom.uid + ":" + rhythmController.name;
			rcMap.Remove(key3);
		}
		AudioSourceControl[] audioSourceControls = atom.audioSourceControls;
		foreach (AudioSourceControl audioSourceControl in audioSourceControls)
		{
			string key4 = atom.uid + ":" + audioSourceControl.name;
			ascMap.Remove(key4);
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key5 = atom.uid + ":" + grabPoint.name;
			gpMap.Remove(key5);
		}
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			string key6 = atom.uid + ":" + forceReceiver.name;
			frMap.Remove(key6);
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody in linkableRigidbodies)
		{
			string key7 = atom.uid + ":" + rigidbody.name;
			rbMap.Remove(key7);
		}
		AnimationPattern[] animationPatterns = atom.animationPatterns;
		foreach (AnimationPattern item in animationPatterns)
		{
			allAnimationPatterns.Remove(item);
		}
		AnimationStep[] animationSteps = atom.animationSteps;
		foreach (AnimationStep item2 in animationSteps)
		{
			allAnimationSteps.Remove(item2);
		}
		Animator[] animators = atom.animators;
		foreach (Animator item3 in animators)
		{
			allAnimators.Remove(item3);
		}
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
		{
			string key8 = atom.uid + ":" + motionAnimationControl.name;
			macMap.Remove(key8);
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key9 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Remove(key9);
		}
		foreach (Canvas canvase in atom.canvases)
		{
			allCanvases.Remove(canvase);
		}
		if (motionAnimationMaster != null)
		{
			MotionAnimationControl[] motionAnimationControls2 = atom.motionAnimationControls;
			foreach (MotionAnimationControl mac in motionAnimationControls2)
			{
				motionAnimationMaster.DeregisterAnimationControl(mac);
			}
		}
		SyncForceReceiverNames();
		SyncForceProducerNames();
		SyncFreeControllerNames();
		SyncRigidbodyNames();
		SyncSelectAtomPopup();
		atom.OnRemove();
		atom.destroyed = true;
		ValidateAllAtoms();
		if (atomPoolContainer != null && atom.isPoolable)
		{
			PutAtomBackInPool(atom);
			return;
		}
		UnityEngine.Object.Destroy(atom.gameObject);
		AtomAsset value;
		if (atom.loadedFromBundle && atom.type != null && atom.type != string.Empty && atomAssetByType.TryGetValue(atom.type, out value))
		{
			UnregisterPrefab(value.assetBundleName, value.assetName);
		}
	}

	public void ValidateAllAtoms()
	{
		foreach (Atom atoms in atomsList)
		{
			atoms.Validate();
		}
	}

	private void InitAtom(Atom atom)
	{
		atoms.Add(atom.uid, atom);
		atomsList.Add(atom);
		atomUIDs.Add(atom.uid);
		sortedAtomUIDs.Add(atom.uid);
		SyncSortedAtomUIDs();
		bool flag = false;
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			flag = true;
			allControllers.Add(freeControllerV);
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Add(key, freeControllerV);
		}
		if (flag)
		{
			atomUIDsWithFreeControllers.Add(atom.uid);
			sortedAtomUIDsWithFreeControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithFreeControllers();
		}
		bool flag2 = false;
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			flag2 = true;
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Add(key2, forceProducerV);
		}
		if (flag2)
		{
			atomUIDsWithForceProducers.Add(atom.uid);
			sortedAtomUIDsWithForceProducers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceProducers();
		}
		bool flag3 = false;
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			flag3 = true;
			string key3 = atom.uid + ":" + forceReceiver.name;
			frMap.Add(key3, forceReceiver);
		}
		if (flag3)
		{
			atomUIDsWithForceReceivers.Add(atom.uid);
			sortedAtomUIDsWithForceReceivers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceReceivers();
		}
		bool flag4 = false;
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			flag4 = true;
			string key4 = atom.uid + ":" + rhythmController.name;
			rcMap.Add(key4, rhythmController);
		}
		if (flag4)
		{
			atomUIDsWithRhythmControllers.Add(atom.uid);
			sortedAtomUIDsWithRhythmControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithRhythmControllers();
		}
		bool flag5 = false;
		AudioSourceControl[] audioSourceControls = atom.audioSourceControls;
		foreach (AudioSourceControl audioSourceControl in audioSourceControls)
		{
			flag5 = true;
			string key5 = atom.uid + ":" + audioSourceControl.name;
			ascMap.Add(key5, audioSourceControl);
		}
		if (flag5)
		{
			atomUIDsWithAudioSourceControls.Add(atom.uid);
			sortedAtomUIDsWithAudioSourceControls.Add(atom.uid);
			SyncSortedAtomUIDsWithAudioSourceControls();
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key6 = atom.uid + ":" + grabPoint.name;
			gpMap.Add(key6, grabPoint);
		}
		bool flag6 = false;
		Rigidbody[] rigidbodies = atom.rigidbodies;
		foreach (Rigidbody rigidbody in rigidbodies)
		{
			rigidbody.maxAngularVelocity = maxAngularVelocity;
			rigidbody.maxDepenetrationVelocity = maxDepenetrationVelocity;
			rigidbody.solverIterations = _solverIterations;
		}
		PhysicsSimulator[] physicsSimulators = atom.physicsSimulators;
		foreach (PhysicsSimulator physicsSimulator in physicsSimulators)
		{
			physicsSimulator.solverIterations = _solverIterations;
		}
		PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable = atom.physicsSimulatorsStorable;
		foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in physicsSimulatorsStorable)
		{
			physicsSimulatorJSONStorable.solverIterations = _solverIterations;
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody2 in linkableRigidbodies)
		{
			flag6 = true;
			string key7 = atom.uid + ":" + rigidbody2.name;
			rbMap.Add(key7, rigidbody2);
		}
		if (flag6)
		{
			atomUIDsWithRigidbodies.Add(atom.uid);
			sortedAtomUIDsWithRigidbodies.Add(atom.uid);
			SyncSortedAtomUIDsWithRigidbodies();
		}
		AnimationPattern[] animationPatterns = atom.animationPatterns;
		foreach (AnimationPattern item in animationPatterns)
		{
			allAnimationPatterns.Add(item);
		}
		AnimationStep[] animationSteps = atom.animationSteps;
		foreach (AnimationStep item2 in animationSteps)
		{
			allAnimationSteps.Add(item2);
		}
		Animator[] animators = atom.animators;
		foreach (Animator item3 in animators)
		{
			allAnimators.Add(item3);
		}
		foreach (Canvas canvase in atom.canvases)
		{
			if (overrideCanvasSortingLayer)
			{
				IgnoreCanvas component = canvase.GetComponent<IgnoreCanvas>();
				if (component == null)
				{
					canvase.sortingLayerName = overrideCanvasSortingLayerName;
				}
			}
			allCanvases.Add(canvase);
		}
		if (motionAnimationMaster != null)
		{
			MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
			foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
			{
				string key8 = atom.uid + ":" + motionAnimationControl.name;
				macMap.Add(key8, motionAnimationControl);
				motionAnimationMaster.RegisterAnimationControl(motionAnimationControl);
			}
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key9 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Add(key9, playerNavCollider);
		}
		SyncHiddenAtoms();
		atom.useRigidbodyInterpolation = _useInterpolation;
	}

	public void SetAtomAssetsFromFile()
	{
		if (atomAssetsFile == null || !(atomAssetsFile != string.Empty))
		{
			return;
		}
		string text = File.ReadAllText(atomAssetsFile);
		string[] array = text.Split('\n');
		List<AtomAsset> list = new List<AtomAsset>();
		string[] array2 = array;
		foreach (string input in array2)
		{
			string[] array3 = Regex.Split(input, "\\s+");
			if (array3.Length >= 3)
			{
				AtomAsset atomAsset = new AtomAsset();
				atomAsset.assetBundleName = array3[0];
				atomAsset.assetName = array3[1];
				atomAsset.category = array3[2];
				list.Add(atomAsset);
			}
		}
		atomAssets = list.ToArray();
	}

	private void InitAtoms()
	{
		atomPrefabByType = new Dictionary<string, Atom>();
		atomAssetByType = new Dictionary<string, AtomAsset>();
		AtomAsset[] array = atomAssets;
		foreach (AtomAsset atomAsset in array)
		{
			if (atomAsset != null)
			{
				string assetName = atomAsset.assetName;
				if (!atomAssetByType.ContainsKey(assetName))
				{
					atomAssetByType.Add(assetName, atomAsset);
				}
				else
				{
					Error("Atom asset " + assetName + " is a duplicate");
				}
			}
		}
		AtomAsset[] array2 = indirectAtomAssets;
		foreach (AtomAsset atomAsset2 in array2)
		{
			if (atomAsset2 != null)
			{
				string assetName2 = atomAsset2.assetName;
				if (!atomAssetByType.ContainsKey(assetName2))
				{
					atomAssetByType.Add(assetName2, atomAsset2);
				}
				else
				{
					Error("Atom asset " + assetName2 + " is a duplicate");
				}
			}
		}
		Atom[] array3 = atomPrefabs;
		foreach (Atom atom in array3)
		{
			if (atom != null)
			{
				string type = atom.type;
				if (!atomPrefabByType.ContainsKey(type))
				{
					atomPrefabByType.Add(type, atom);
					continue;
				}
				Error("Atom " + atom.name + " uses type " + type + " that is already used");
			}
		}
		Atom[] array4 = indirectAtomPrefabs;
		foreach (Atom atom2 in array4)
		{
			if (atom2 != null)
			{
				string type2 = atom2.type;
				if (!atomPrefabByType.ContainsKey(type2))
				{
					atomPrefabByType.Add(type2, atom2);
					continue;
				}
				Error("Atom " + atom2.name + " uses type " + type2 + " that is already used");
			}
		}
		atomTypes = new List<string>();
		atomCategories = new List<string>();
		atomCategoryToAtomTypes = new Dictionary<string, List<string>>();
		Atom[] array5 = atomPrefabs;
		foreach (Atom atom3 in array5)
		{
			atomTypes.Add(atom3.type);
			List<string> value;
			if (atomCategoryToAtomTypes.TryGetValue(atom3.category, out value))
			{
				value.Add(atom3.type);
				continue;
			}
			atomCategories.Add(atom3.category);
			value = new List<string>();
			value.Add(atom3.type);
			atomCategoryToAtomTypes.Add(atom3.category, value);
		}
		AtomAsset[] array6 = atomAssets;
		foreach (AtomAsset atomAsset3 in array6)
		{
			atomTypes.Add(atomAsset3.assetName);
			List<string> value2;
			if (atomCategoryToAtomTypes.TryGetValue(atomAsset3.category, out value2))
			{
				value2.Add(atomAsset3.assetName);
				continue;
			}
			atomCategories.Add(atomAsset3.category);
			value2 = new List<string>();
			value2.Add(atomAsset3.assetName);
			atomCategoryToAtomTypes.Add(atomAsset3.category, value2);
		}
		atomTypes.Sort();
		atomCategories.Sort();
		foreach (List<string> value3 in atomCategoryToAtomTypes.Values)
		{
			value3.Sort();
		}
		if (atomCategoryPopup != null)
		{
			atomCategoryPopup.currentValue = "None";
			atomCategoryPopup.numPopupValues = atomCategories.Count;
			for (int num = 0; num < atomCategories.Count; num++)
			{
				atomCategoryPopup.setPopupValue(num, atomCategories[num]);
			}
			UIPopup uIPopup = atomCategoryPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)System.Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAddAtomAtomPopupValues));
		}
		if (atomPrefabPopup != null)
		{
			atomPrefabPopup.currentValue = "None";
		}
		atoms = new Dictionary<string, Atom>();
		atomsList = new List<Atom>();
		atomUIDs = new List<string>();
		atomUIDsWithForceReceivers = new List<string>();
		atomUIDsWithForceProducers = new List<string>();
		atomUIDsWithRhythmControllers = new List<string>();
		atomUIDsWithAudioSourceControls = new List<string>();
		atomUIDsWithFreeControllers = new List<string>();
		atomUIDsWithRigidbodies = new List<string>();
		sortedAtomUIDs = new List<string>();
		sortedAtomUIDsWithForceReceivers = new List<string>();
		sortedAtomUIDsWithForceProducers = new List<string>();
		sortedAtomUIDsWithRhythmControllers = new List<string>();
		sortedAtomUIDsWithAudioSourceControls = new List<string>();
		sortedAtomUIDsWithFreeControllers = new List<string>();
		sortedAtomUIDsWithRigidbodies = new List<string>();
		hiddenAtomUIDs = new List<string>();
		hiddenAtomUIDsWithFreeControllers = new List<string>();
		visibleAtomUIDs = new List<string>();
		visibleAtomUIDsWithFreeControllers = new List<string>();
		uids = new Dictionary<string, bool>();
		frMap = new Dictionary<string, ForceReceiver>();
		fpMap = new Dictionary<string, ForceProducerV2>();
		rcMap = new Dictionary<string, RhythmController>();
		ascMap = new Dictionary<string, AudioSourceControl>();
		gpMap = new Dictionary<string, GrabPoint>();
		fcMap = new Dictionary<string, FreeControllerV3>();
		rbMap = new Dictionary<string, Rigidbody>();
		macMap = new Dictionary<string, MotionAnimationControl>();
		pncMap = new Dictionary<string, PlayerNavCollider>();
		allControllers = new List<FreeControllerV3>();
		allAnimationPatterns = new List<AnimationPattern>();
		allAnimationSteps = new List<AnimationStep>();
		allAnimators = new List<Animator>();
		allCanvases = new List<Canvas>();
		Atom[] componentsInChildren = atomContainerTransform.GetComponentsInChildren<Atom>();
		startingAtoms = new HashSet<Atom>();
		Atom[] array7 = componentsInChildren;
		foreach (Atom atom4 in array7)
		{
			if (!atom4.exclude)
			{
				string text = CreateUID(atom4.name);
				if (text != null)
				{
					atom4.uid = text;
					atom4.name = text;
					InitAtom(atom4);
					startingAtoms.Add(atom4);
				}
			}
		}
		if (onAtomUIDsChangedHandlers != null)
		{
			onAtomUIDsChangedHandlers(GetAtomUIDs());
		}
		if (onAtomUIDsWithForceReceiversChangedHandlers != null)
		{
			onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
		}
		if (onAtomUIDsWithForceProducersChangedHandlers != null)
		{
			onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
		}
		if (onAtomUIDsWithFreeControllersChangedHandlers != null)
		{
			onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
		}
		if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
		{
			onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
		}
		SyncForceReceiverNames();
		SyncForceProducerNames();
		SyncFreeControllerNames();
		SyncRigidbodyNames();
		SyncSelectAtomPopup();
	}

	protected void SyncNavigationHologridVisibility()
	{
		if (navigationHologridShowTime > 0f)
		{
			navigationHologridTransparencyMultiplier = 5f;
		}
		else
		{
			navigationHologridTransparencyMultiplier = 1f;
		}
		if (navigationHologridVisible || navigationHologridShowTime > 0f)
		{
			if (!navigationHologrid.gameObject.activeSelf)
			{
				navigationHologrid.gameObject.SetActive(true);
				SyncHologridTransparency();
			}
		}
		else if (navigationHologrid.gameObject.activeSelf)
		{
			navigationHologrid.gameObject.SetActive(false);
		}
		if (navigationHologridShowTime > 0f)
		{
			navigationHologridShowTime -= Time.unscaledDeltaTime;
		}
	}

	protected void SplashNavigationHologrid(float seconds)
	{
		navigationHologridShowTime = seconds;
		SyncNavigationHologridVisibility();
	}

	private void SyncHologridTransparency()
	{
		if (!(navigationHologrid != null))
		{
			return;
		}
		MeshRenderer component = navigationHologrid.GetComponent<MeshRenderer>();
		if (component != null)
		{
			Material material = component.material;
			if (material != null)
			{
				material.SetFloat("_Alpha", _hologridTransparency * navigationHologridTransparencyMultiplier);
			}
		}
	}

	public void ResetNavigationRigPositionRotation()
	{
		if (navigationRig != null)
		{
			ClearPossess();
			DisconnectNavRigFromPlayerNavCollider();
			navigationRig.localPosition = Vector3.zero;
			navigationRig.localRotation = Quaternion.identity;
			playerHeightAdjust = 0f;
			ResetMonitorCenterCamera();
		}
	}

	public void SetSceneLoadPosition()
	{
		if (navigationRig != null)
		{
			sceneLoadPlayerHeightAdjust = _playerHeightAdjust;
			sceneLoadPosition = navigationRig.position;
			sceneLoadRotation = navigationRig.rotation;
			if (MonitorCenterCamera != null)
			{
				sceneLoadMonitorCameraLocalRotation = MonitorCenterCamera.transform.localEulerAngles;
			}
		}
	}

	public void MoveToSceneLoadPosition()
	{
		if (navigationRig != null)
		{
			navigationRig.position = sceneLoadPosition;
			navigationRig.rotation = sceneLoadRotation;
			playerHeightAdjust = sceneLoadPlayerHeightAdjust;
			if (MonitorCenterCamera != null)
			{
				MonitorCenterCamera.transform.localEulerAngles = sceneLoadMonitorCameraLocalRotation;
			}
		}
	}

	public void SetUseSceneLoadPosition(bool b)
	{
		useSceneLoadPosition = b;
	}

	private void SyncLockHeightDuringNavigate()
	{
		if (lockHeightDuringNavigateToggle != null)
		{
			lockHeightDuringNavigateToggle.isOn = _lockHeightDuringNavigate;
		}
		if (lockHeightDuringNavigateToggleAlt != null)
		{
			lockHeightDuringNavigateToggleAlt.isOn = _lockHeightDuringNavigate;
		}
	}

	private void SyncDisableAllNavigation()
	{
		if (disableAllNavigationToggle != null)
		{
			disableAllNavigationToggle.isOn = _disableAllNavigation;
		}
	}

	public void ToggleDisableAllNavigation()
	{
		disableAllNavigation = !_disableAllNavigation;
	}

	private void SyncFreeMoveFollowFloor()
	{
		if (freeMoveFollowFloorToggle != null)
		{
			freeMoveFollowFloorToggle.isOn = _freeMoveFollowFloor;
		}
		if (freeMoveFollowFloorToggleAlt != null)
		{
			freeMoveFollowFloorToggleAlt.isOn = _freeMoveFollowFloor;
		}
	}

	private void SyncTeleportAllowRotation()
	{
		if (teleportAllowRotationToggle != null)
		{
			teleportAllowRotationToggle.isOn = _teleportAllowRotation;
		}
	}

	private void SyncDisableTeleport()
	{
		if (disableTeleportToggle != null)
		{
			disableTeleportToggle.isOn = _disableTeleport;
		}
	}

	private void SyncDisableTeleportDuringPossess()
	{
		if (disableTeleportDuringPossessToggle != null)
		{
			disableTeleportDuringPossessToggle.isOn = _disableTeleportDuringPossess;
		}
	}

	private void SyncFreeMoveMultiplier()
	{
		if (freeMoveMultiplierSlider != null)
		{
			freeMoveMultiplierSlider.value = _freeMoveMultiplier;
		}
	}

	private void SyncDisableGrabNavigation()
	{
		if (disableGrabNavigationToggle != null)
		{
			disableGrabNavigationToggle.isOn = _disableGrabNavigation;
		}
	}

	private void SyncGrabNavigationPositionMultiplier()
	{
		if (grabNavigationPositionMultiplierSlider != null)
		{
			grabNavigationPositionMultiplierSlider.value = _grabNavigationPositionMultiplier;
		}
	}

	private void SyncGrabNavigationRotationMultiplier()
	{
		if (grabNavigationRotationMultiplierSlider != null)
		{
			grabNavigationRotationMultiplierSlider.value = _grabNavigationRotationMultiplier;
		}
	}

	private void SyncPlayerHeightAdjust()
	{
		if (heightAdjustTransform != null && navigationRig != null)
		{
			Vector3 localPosition = heightAdjustTransform.localPosition;
			localPosition.y = _playerHeightAdjust / _worldScale;
			heightAdjustTransform.localPosition = localPosition;
		}
	}

	public void playerHeightAdjustAdjust(float val)
	{
		playerHeightAdjust += val;
	}

	private void InitMotionControllerNaviation()
	{
		if (!(navigationPlayArea != null))
		{
			return;
		}
		if (regularPlayArea != null)
		{
			regularPlayAreaMR = regularPlayArea.GetComponent<MeshRenderer>();
		}
		if (regularPlayAreaMR != null)
		{
			regularPlayAreaMR.enabled = false;
		}
		navigationPlayAreaMR = navigationPlayArea.GetComponent<MeshRenderer>();
		if (navigationPlayAreaMR != null)
		{
			navigationPlayAreaMR.enabled = false;
		}
		if (navigationCurve != null)
		{
			navigationCurve.draw = false;
		}
		navigationPlayerMR = null;
		navigationCameraMR = null;
		if (!(lookCamera != null))
		{
			return;
		}
		if (navigationPlayer != null)
		{
			navigationPlayerMR = navigationPlayer.GetComponent<MeshRenderer>();
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
		}
		if (navigationCamera != null)
		{
			navigationCameraMR = navigationCamera.GetComponentInChildren<MeshRenderer>();
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
		}
	}

	private void ProcessTeleportMode()
	{
		if (navigationPlayArea != null)
		{
			if (regularPlayAreaMR != null)
			{
				regularPlayAreaMR.enabled = false;
			}
			if (navigationPlayAreaMR != null)
			{
				navigationPlayAreaMR.enabled = false;
			}
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
			if (navigationCurve != null)
			{
				navigationCurve.draw = false;
			}
		}
		if (GetTeleportStart(true))
		{
			ProcessTeleportStart();
		}
		if (GetTeleportShowLeft(true))
		{
			ProcessTeleportShow(true);
		}
		else if (GetTeleportShowRight(true))
		{
			ProcessTeleportShow(false);
		}
		if (GetTeleportFinish(true))
		{
			ProcessTeleportFinish();
		}
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessTeleportStart()
	{
		if (navigationRig != null)
		{
			isTeleporting = true;
			startNavigateRotation = navigationRig.rotation;
		}
	}

	private void ProcessTeleportShow(bool isLeft)
	{
		if (navigationPlayArea != null && navigationRig != null)
		{
			navigationPlayArea.rotation = navigationRig.rotation;
		}
		if (regularPlayAreaMR != null)
		{
			regularPlayAreaMR.enabled = true;
		}
		if (navigationPlayAreaMR != null)
		{
			navigationPlayAreaMR.enabled = true;
		}
		if (navigationPlayerMR != null)
		{
			navigationPlayerMR.enabled = true;
		}
		if (navigationCameraMR != null)
		{
			navigationCameraMR.enabled = true;
		}
		bool flag = false;
		if (useLookForNavigation && lookCamera != null)
		{
			castRay.origin = lookCamera.transform.position;
			castRay.direction = lookCamera.transform.forward;
		}
		else if (isLeft)
		{
			castRay.origin = motionControllerLeft.position;
			castRay.direction = motionControllerLeft.forward;
		}
		else
		{
			castRay.origin = motionControllerRight.position;
			castRay.direction = motionControllerRight.forward;
			flag = true;
		}
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num <= 0)
		{
			return;
		}
		int num2 = -1;
		float num3 = navigationDistance;
		for (int i = 0; i < num; i++)
		{
			float magnitude = (raycastHits[i].point - castRay.origin).magnitude;
			if (magnitude < num3)
			{
				num2 = i;
				num3 = magnitude;
			}
		}
		if (lookCamera != null && navigationPlayArea != null)
		{
			if (navigationPlayer != null)
			{
				Vector3 localPosition = lookCamera.transform.localPosition;
				localPosition.y = 0f;
				navigationPlayer.localPosition = localPosition;
			}
			if (navigationCamera != null)
			{
				navigationCamera.localRotation = lookCamera.transform.localRotation;
			}
			Vector3 vector = navigationPlayer.position - navigationPlayArea.position;
			navigationPlayArea.position = raycastHits[num2].point - vector;
			Collider collider = raycastHits[num2].collider;
			PlayerNavCollider playerNavCollider = (teleportPlayerNavCollider = collider.GetComponent<PlayerNavCollider>());
			if (_teleportAllowRotation)
			{
				navigationPlayArea.rotation = startNavigateRotation;
				if (playerNavCollider != null)
				{
					Quaternion quaternion = Quaternion.FromToRotation(navigationPlayArea.up, playerNavCollider.transform.up);
					navigationPlayArea.rotation = quaternion * navigationPlayArea.rotation;
				}
				Vector3 vector2 = ((!isLeft) ? (Quaternion2Angles.GetAngles(Quaternion.Inverse(lookCamera.transform.rotation) * motionControllerRight.rotation, Quaternion2Angles.RotationOrder.ZXY) * 57.29578f) : (Quaternion2Angles.GetAngles(Quaternion.Inverse(lookCamera.transform.rotation) * motionControllerLeft.rotation, Quaternion2Angles.RotationOrder.ZXY) * 57.29578f));
				navigationPlayArea.Rotate(Vector3.up, (0f - vector2.z) * 2f);
			}
			else if (playerNavCollider != null)
			{
				Quaternion quaternion2 = Quaternion.FromToRotation(navigationPlayArea.up, playerNavCollider.transform.up);
				navigationPlayArea.rotation = quaternion2 * navigationPlayArea.rotation;
			}
		}
		if (navigationCurve != null && navigationCurve.points != null && navigationCurve.points.Length == 3)
		{
			if (useLookForNavigation)
			{
				navigationCurve.points[0].transform.position = lookCamera.transform.position;
			}
			else if (flag)
			{
				navigationCurve.points[0].transform.position = motionControllerRight.position;
			}
			else
			{
				navigationCurve.points[0].transform.position = motionControllerLeft.position;
			}
			if (navigationPlayer != null)
			{
				navigationCurve.points[2].transform.position = navigationPlayer.position;
			}
			else
			{
				navigationCurve.points[2].transform.position = raycastHits[num2].point;
			}
			Vector3 position = (navigationCurve.points[0].transform.position + navigationCurve.points[2].transform.position) * 0.5f;
			position.y += 1f * navigationCurve.transform.lossyScale.y;
			navigationCurve.points[1].transform.position = position;
			navigationCurve.draw = true;
		}
	}

	private void DisconnectNavRigFromPlayerNavCollider()
	{
		if (playerNavCollider != null)
		{
			playerNavTrackerGO.transform.SetParent(null);
			playerNavCollider = null;
			Vector3 position = navigationRig.position;
			Quaternion rotation = navigationRig.rotation;
			navigationRigParent.localPosition = Vector3.zero;
			navigationRigParent.localRotation = Quaternion.identity;
			navigationRig.position = position;
			navigationRig.rotation = rotation;
		}
	}

	private void ProcessPlayerNavMove()
	{
		if (playerNavCollider != null && navigationRigParent != null)
		{
			navigationRigParent.transform.position = playerNavTrackerGO.transform.position;
			navigationRigParent.transform.rotation = playerNavTrackerGO.transform.rotation;
		}
	}

	private void ConnectNavRigToPlayerNavCollider()
	{
		if (navigationRigParent != null)
		{
			navigationRigParent.position = navigationRig.position;
			navigationRigParent.rotation = navigationRig.rotation;
			navigationRig.position = navigationRigParent.position;
			navigationRig.rotation = navigationRigParent.rotation;
			if ((bool)playerNavCollider)
			{
				playerNavTrackerGO.transform.SetParent(playerNavCollider.transform);
				playerNavTrackerGO.transform.position = navigationRigParent.position;
				playerNavTrackerGO.transform.rotation = navigationRigParent.rotation;
			}
			else
			{
				playerNavTrackerGO.transform.SetParent(null);
			}
		}
	}

	private void ProcessTeleportFinish()
	{
		if (isTeleporting)
		{
			navigationRig.position = navigationPlayArea.position;
			navigationRig.rotation = navigationPlayArea.rotation;
			playerNavCollider = teleportPlayerNavCollider;
			ConnectNavRigToPlayerNavCollider();
		}
		isTeleporting = false;
	}

	private void ProcessMotionControllerNavigation()
	{
		if (isMonitorOnly)
		{
			return;
		}
		didStartLeftNavigate = false;
		didStartRightNavigate = false;
		if (navigationPlayArea != null)
		{
			if (regularPlayAreaMR != null)
			{
				regularPlayAreaMR.enabled = false;
			}
			if (navigationPlayAreaMR != null)
			{
				navigationPlayAreaMR.enabled = false;
			}
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
			if (navigationCurve != null)
			{
				navigationCurve.draw = false;
			}
		}
		if (navigationDisabled || _disableAllNavigation || worldUIActivated || !(navigationRig != null))
		{
			return;
		}
		bool flag = GetLeftSelect() && highlightedControllersLeft.Count > 0;
		bool flag2 = GetRightSelect() && highlightedControllersRight.Count > 0;
		if (!_disableGrabNavigation && GetGrabNavigateStartLeft() && !flag)
		{
			startGrabNavigatePositionLeft = motionControllerLeft.position;
			startGrabNavigateRotationLeft = motionControllerLeft.rotation;
			isGrabNavigatingLeft = true;
			didStartLeftNavigate = true;
		}
		if (isGrabNavigatingLeft && GetGrabNavigateLeft())
		{
			Vector3 position = navigationRig.position;
			position += (startGrabNavigatePositionLeft - motionControllerLeft.position) * _grabNavigationPositionMultiplier;
			Vector3 up = navigationRig.up;
			float num = Vector3.Dot(position - navigationRig.position, up);
			position += up * (0f - num);
			navigationRig.position = position;
			if (!_lockHeightDuringNavigate)
			{
				playerHeightAdjust += num;
			}
			startGrabNavigatePositionLeft = motionControllerLeft.position;
			float num2 = (Quaternion2Angles.GetAngles(motionControllerLeft.rotation * Quaternion.Inverse(startGrabNavigateRotationLeft), Quaternion2Angles.RotationOrder.ZXY) * 57.29578f).y * _grabNavigationRotationMultiplier;
			if (num2 > 0f)
			{
				num2 -= _grabNavigationRotationResistance;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
			}
			if (num2 < 0f)
			{
				num2 += _grabNavigationRotationResistance;
				if (num2 > 0f)
				{
					num2 = 0f;
				}
			}
			navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, 0f - num2);
			startGrabNavigateRotationLeft = motionControllerLeft.rotation;
		}
		else
		{
			isGrabNavigatingLeft = false;
		}
		if (!_disableGrabNavigation && GetGrabNavigateStartRight() && !flag2)
		{
			startGrabNavigatePositionRight = motionControllerRight.position;
			startGrabNavigateRotationRight = motionControllerRight.rotation;
			isGrabNavigatingRight = true;
			didStartRightNavigate = true;
		}
		if (isGrabNavigatingRight && GetGrabNavigateRight())
		{
			Vector3 position2 = navigationRig.position;
			position2 += (startGrabNavigatePositionRight - motionControllerRight.position) * _grabNavigationPositionMultiplier;
			Vector3 up2 = navigationRig.up;
			float num3 = Vector3.Dot(position2 - navigationRig.position, up2);
			position2 += up2 * (0f - num3);
			navigationRig.position = position2;
			if (!_lockHeightDuringNavigate)
			{
				playerHeightAdjust += num3;
			}
			startGrabNavigatePositionRight = motionControllerRight.position;
			float num4 = (Quaternion2Angles.GetAngles(motionControllerRight.rotation * Quaternion.Inverse(startGrabNavigateRotationRight), Quaternion2Angles.RotationOrder.ZXY) * 57.29578f).y * _grabNavigationRotationMultiplier;
			if (num4 > 0f)
			{
				num4 -= _grabNavigationRotationResistance;
				if (num4 < 0f)
				{
					num4 = 0f;
				}
			}
			if (num4 < 0f)
			{
				num4 += _grabNavigationRotationResistance;
				if (num4 > 0f)
				{
					num4 = 0f;
				}
			}
			navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, 0f - num4);
			startGrabNavigateRotationRight = motionControllerRight.rotation;
		}
		else
		{
			isGrabNavigatingRight = false;
		}
		if (navigationHologrid != null)
		{
			if (_showNavigationHologrid && (isGrabNavigatingLeft || isGrabNavigatingRight))
			{
				navigationHologridVisible = true;
			}
			else
			{
				navigationHologridVisible = false;
			}
		}
		if (!_disableTeleport && (!_disableTeleportDuringPossess || (leftPossessedController == null && rightPossessedController == null && headPossessedController == null)))
		{
			if (GetTeleportStartLeft() && !flag)
			{
				ProcessTeleportStart();
				didStartLeftNavigate = true;
			}
			if (GetTeleportStartRight() && !flag2)
			{
				ProcessTeleportStart();
				didStartRightNavigate = true;
			}
			if (GetTeleportShowLeft() && highlightedControllersLeft.Count == 0)
			{
				ProcessTeleportShow(true);
			}
			else if (GetTeleportShowRight() && highlightedControllersRight.Count == 0)
			{
				ProcessTeleportShow(false);
			}
			if (GetTeleportFinish())
			{
				ProcessTeleportFinish();
			}
		}
	}

	public void AdjustNavigationRigHeight()
	{
		if (!(navigationRig != null) || !(lookCamera != null) || !_freeMoveFollowFloor)
		{
			return;
		}
		Vector3 position = navigationRig.position;
		Plane plane = new Plane(navigationRig.up, navigationRig.transform.position);
		castRay.origin = lookCamera.transform.position;
		castRay.direction = -navigationRig.transform.up;
		float enter;
		if (!plane.Raycast(castRay, out enter))
		{
			castRay.direction = navigationRig.transform.up;
			plane.Raycast(castRay, out enter);
		}
		castRay.origin = (castRay.GetPoint(enter) + lookCamera.transform.position) * 0.5f;
		castRay.direction = -navigationRig.up;
		float num = navigationDistance;
		Vector3 direction = castRay.direction;
		Vector3 vector = navigationRig.position;
		bool flag = false;
		AllocateRaycastHits();
		int num2 = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num2 > 0)
		{
			flag = true;
			for (int i = 0; i < num2; i++)
			{
				float magnitude = (raycastHits[i].point - castRay.origin).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					vector = raycastHits[i].point;
				}
			}
		}
		castRay.direction = navigationRig.up;
		num2 = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num2 > 0)
		{
			for (int j = 0; j < num2; j++)
			{
				float magnitude2 = (raycastHits[j].point - castRay.origin).magnitude;
				if (magnitude2 < num)
				{
					direction = castRay.direction;
					num = magnitude2;
					vector = raycastHits[j].point;
				}
			}
		}
		if (flag)
		{
			Vector3 vector2 = direction * Vector3.Dot(vector - navigationRig.position, direction);
			navigationRig.position = Vector3.Lerp(navigationRig.position, navigationRig.position + vector2, Time.deltaTime * 2f);
		}
	}

	private void ProcessControllerNavigation(SteamVR_Action_Vector2 moveAction, bool ignoreDisable = false)
	{
		CheckSwapAxis();
		if (navigationRig != null && lookCamera != null && !navigationDisabled && !_disableAllNavigation && !worldUIActivated)
		{
			Vector4 freeNavigateVector = GetFreeNavigateVector(moveAction, ignoreDisable);
			float num = _freeMoveMultiplier * _worldScale;
			bool flag = false;
			if (freeNavigateVector.x > 0.01f || freeNavigateVector.x < -0.01f)
			{
				Vector3 vector = Vector3.ProjectOnPlane(lookCamera.transform.right, navigationRig.up);
				vector.Normalize();
				Vector3 position = navigationRig.position;
				position += vector * (freeNavigateVector.x * 0.5f * Time.unscaledDeltaTime) * num;
				navigationRig.position = position;
				flag = true;
			}
			if (freeNavigateVector.y > 0.01f || freeNavigateVector.y < -0.01f)
			{
				Vector3 vector2 = Vector3.ProjectOnPlane(lookCamera.transform.forward, navigationRig.up);
				vector2.Normalize();
				Vector3 position2 = navigationRig.position;
				position2 += vector2 * (freeNavigateVector.y * 0.5f * Time.unscaledDeltaTime) * num;
				navigationRig.position = position2;
				flag = true;
			}
			if (freeNavigateVector.z > 0.01f || freeNavigateVector.z < -0.01f)
			{
				navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, freeNavigateVector.z * 50f * Time.unscaledDeltaTime);
				flag = true;
			}
			if ((freeNavigateVector.w > 0.01f || freeNavigateVector.w < -0.01f) && !_lockHeightDuringNavigate)
			{
				playerHeightAdjust += freeNavigateVector.w * 0.5f * Time.unscaledDeltaTime * num;
				flag = true;
			}
			if (flag)
			{
				AdjustNavigationRigHeight();
			}
		}
	}

	public void FocusOnController(FreeControllerV3 controller, bool rotationOnly = true, bool alignUpDown = true)
	{
		if (!(MonitorCenterCamera != null) || !(controller != null))
		{
			return;
		}
		AlignRigFacingController(controller, rotationOnly);
		SetMonitorRigPositionZero();
		Vector3 position;
		if (controller.focusPoint != null)
		{
			position = controller.focusPoint.position;
			focusDistance = (controller.focusPoint.position - MonitorCenterCamera.transform.position).magnitude;
		}
		else
		{
			position = controller.transform.position;
			focusDistance = (controller.transform.position - MonitorCenterCamera.transform.position).magnitude;
		}
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.transform.LookAt(position);
			Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
			if (!alignUpDown)
			{
				localEulerAngles.x = 0f;
			}
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
		}
		SyncMonitorRigPosition();
	}

	public void FocusOnSelectedController()
	{
		FocusOnSelectedController(true);
	}

	public void FocusOnSelectedController(bool rotationOnly, bool alignUpDown = true)
	{
		FocusOnController(selectedController, rotationOnly, alignUpDown);
	}

	public void ResetFocusPoint()
	{
		focusDistance = 1.5f;
		SyncMonitorRigPosition();
	}

	public void ResetMonitorCenterCamera()
	{
		ResetFocusPoint();
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.transform.localEulerAngles = Vector3.zero;
		}
	}

	private void ProcessFreeMoveNavigation()
	{
		ProcessControllerNavigation(freeModeMoveAction, true);
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessKeyBindings()
	{
		if (disableInternalKeyBindings || (!(LookInputModule.singleton == null) && LookInputModule.singleton.inputFieldActive) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightAlt))
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			ToggleMainMonitor();
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			ToggleMonitorUI();
		}
		bool key = Input.GetKey(KeyCode.LeftShift);
		if (IsMonitorRigActive && !navigationDisabled && !worldUIActivated)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				ToggleModeFreeMoveMouse();
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				if (key)
				{
					FocusOnSelectedController(false);
				}
				else
				{
					FocusOnSelectedController();
				}
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				ResetFocusPoint();
			}
		}
		if (!UIDisabled)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				gameMode = GameMode.Edit;
			}
			if (Input.GetKeyDown(KeyCode.P))
			{
				gameMode = GameMode.Play;
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				CycleSelectAtomOfType("Person");
			}
			if (Input.GetKeyDown(KeyCode.U) && (UserPreferences.singleton == null || !UserPreferences.singleton.firstTimeUser))
			{
				ToggleMainHUDMonitor();
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				ToggleTargetsOnWithButton();
			}
			if (Input.GetKeyDown(KeyCode.H))
			{
				ToggleShowHiddenAtoms();
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				ProcessTargetSelectionCycleSelectMouse();
			}
		}
	}

	private void ProcessMouseControl()
	{
		mouseClickUsed = false;
		if (!(navigationRig != null) || !MonitorRigActive || navigationDisabled || worldUIActivated)
		{
			return;
		}
		if (Input.GetMouseButtonDown(1))
		{
		}
		if (Input.GetMouseButtonUp(1))
		{
		}
		bool flag = headPossessedController != null;
		if (Input.GetMouseButton(1) && !flag)
		{
			bool key = Input.GetKey(KeyCode.LeftControl);
			bool key2 = Input.GetKey(KeyCode.LeftShift);
			SetMonitorRigPositionZero();
			if (!key)
			{
				Vector3 point = MonitorCenterCamera.transform.position + MonitorCenterCamera.transform.forward * focusDistance;
				float mouseXChange = GetMouseXChange();
				mouseXChange = Mathf.Clamp(mouseXChange, -10f, 10f);
				if (mouseXChange > 0.01f || mouseXChange < -0.01f)
				{
					navigationRig.RotateAround(point, navigationRig.up, mouseXChange * 2f);
				}
			}
			if (!key2)
			{
				float mouseYChange = GetMouseYChange();
				mouseYChange = Mathf.Clamp(mouseYChange, -10f, 10f);
				if (mouseYChange > 0.01f || mouseYChange < -0.01f)
				{
					Vector3 vector = MonitorCenterCamera.transform.position + MonitorCenterCamera.transform.forward * focusDistance;
					Vector3 position = MonitorCenterCamera.transform.position;
					Vector3 up = navigationRig.up;
					Vector3 vector2 = position - up * mouseYChange * 0.1f * focusDistance;
					Vector3 vector3 = vector2 - vector;
					vector3.Normalize();
					vector2 = vector + vector3 * focusDistance;
					Vector3 vector4 = vector2 - position;
					Vector3 position2 = navigationRig.position + vector4;
					float num = Vector3.Dot(vector4, up);
					position2 += up * (0f - num);
					navigationRig.position = position2;
					playerHeightAdjust += num;
					MonitorCenterCamera.transform.LookAt(vector);
					Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
					localEulerAngles.y = 0f;
					localEulerAngles.z = 0f;
					MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
				}
			}
			SyncMonitorRigPosition();
		}
		else if (Input.GetMouseButton(2) && (!flag || _allowHeadPossessMousePanAndZoom))
		{
			bool key3 = Input.GetKey(KeyCode.LeftControl);
			bool key4 = Input.GetKey(KeyCode.LeftShift);
			bool flag2 = false;
			Vector3 position3 = navigationRig.position;
			if (!key3)
			{
				float mouseXChange2 = GetMouseXChange();
				mouseXChange2 = Mathf.Clamp(mouseXChange2, -10f, 10f);
				if (mouseXChange2 > 0.01f || mouseXChange2 < -0.01f)
				{
					position3 += MonitorCenterCamera.transform.right * (0f - mouseXChange2) * 0.03f;
					flag2 = true;
				}
			}
			if (!key4)
			{
				float mouseYChange2 = GetMouseYChange();
				mouseYChange2 = Mathf.Clamp(mouseYChange2, -10f, 10f);
				if (mouseYChange2 > 0.01f || mouseYChange2 < -0.01f)
				{
					position3 += MonitorCenterCamera.transform.up * (0f - mouseYChange2) * 0.03f;
					flag2 = true;
				}
			}
			if (flag2)
			{
				Vector3 up2 = navigationRig.up;
				Vector3 lhs = position3 - navigationRig.position;
				float num2 = Vector3.Dot(lhs, up2);
				position3 += up2 * (0f - num2);
				navigationRig.position = position3;
				playerHeightAdjust += num2;
			}
		}
		float y = Input.mouseScrollDelta.y;
		if (GUIhitMouse || (!(y > 0.5f) && !(y < -0.5f)) || (flag && !_allowHeadPossessMousePanAndZoom))
		{
			return;
		}
		Vector2 vector5 = MonitorCenterCamera.ScreenToViewportPoint(Input.mousePosition);
		if (vector5.x >= 0f && vector5.x <= 1f && vector5.y >= 0f && vector5.y <= 1f)
		{
			float num3 = 0.1f;
			if (y < -0.5f)
			{
				num3 = 0f - num3;
			}
			Vector3 forward = MonitorCenterCamera.transform.forward;
			Vector3 vector6 = num3 * forward * focusDistance;
			Vector3 position4 = navigationRig.position + vector6;
			focusDistance *= 1f - num3;
			Vector3 up3 = navigationRig.up;
			float num4 = Vector3.Dot(vector6, up3);
			position4 += up3 * (0f - num4);
			navigationRig.position = position4;
			playerHeightAdjust += num4;
			SyncMonitorRigPosition();
		}
	}

	private void ProcessKeyboardFreeNavigation()
	{
		if (!disableInternalNavigationKeyBindings && navigationRig != null && lookCamera != null && (LookInputModule.singleton == null || !LookInputModule.singleton.inputFieldActive) && !navigationDisabled && !worldUIActivated && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.RightAlt))
		{
			bool flag = false;
			Vector2 vector = default(Vector2);
			vector.x = 0f;
			vector.y = 0f;
			float num = freeMoveMultiplier * _worldScale;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				num *= 3f;
			}
			if (Input.GetKey(KeyCode.W))
			{
				vector.y = num;
			}
			if (Input.GetKey(KeyCode.A))
			{
				vector.x = 0f - num;
			}
			if (Input.GetKey(KeyCode.S))
			{
				vector.y = 0f - num;
			}
			if (Input.GetKey(KeyCode.D))
			{
				vector.x = num;
			}
			if (vector.y != 0f)
			{
				flag = true;
				Vector3 vector2 = Vector3.ProjectOnPlane(lookCamera.transform.forward, navigationRig.up);
				vector2.Normalize();
				Vector3 position = navigationRig.position;
				position += vector2 * (vector.y * Time.unscaledDeltaTime);
				navigationRig.position = position;
			}
			if (vector.x != 0f)
			{
				flag = true;
				Vector3 vector3 = Vector3.ProjectOnPlane(lookCamera.transform.right, navigationRig.up);
				vector3.Normalize();
				Vector3 position2 = navigationRig.position;
				position2 += vector3 * (vector.x * Time.unscaledDeltaTime);
				navigationRig.position = position2;
			}
			float num2 = 0f;
			if (Input.GetKey(KeyCode.Z))
			{
				num2 = num;
			}
			if (Input.GetKey(KeyCode.X))
			{
				num2 = 0f - num;
			}
			if (num2 != 0f)
			{
				flag = true;
				playerHeightAdjust += num2 * 0.5f * Time.unscaledDeltaTime;
			}
			if (flag)
			{
				AdjustNavigationRigHeight();
			}
		}
	}

	private void ProcessMouseFreeNavigation()
	{
		if (!(navigationRig != null) || !(lookCamera != null) || navigationDisabled || worldUIActivated || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			return;
		}
		float mouseXChange = GetMouseXChange();
		mouseXChange = Mathf.Clamp(mouseXChange, -10f, 10f);
		if (mouseXChange > 0.01f || mouseXChange < -0.01f)
		{
			if (_mainHUDAnchoredOnMonitor && MonitorCenterCamera != null)
			{
				navigationRig.RotateAround(MonitorCenterCamera.transform.position, navigationRig.up, mouseXChange);
			}
			else if (lookCamera != null)
			{
				navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, mouseXChange);
			}
		}
		float mouseYChange = GetMouseYChange();
		mouseYChange = Mathf.Clamp(mouseYChange, -10f, 10f);
		if ((mouseYChange > 0.01f || mouseYChange < -0.01f) && MonitorCenterCamera != null)
		{
			Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
			if (localEulerAngles.x > 180f)
			{
				localEulerAngles.x -= 360f;
			}
			if (localEulerAngles.x < -180f)
			{
				localEulerAngles.x += 360f;
			}
			localEulerAngles.x -= mouseYChange;
			localEulerAngles.x = Mathf.Clamp(localEulerAngles.x, -89f, 89f);
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
		}
	}

	private void ProcessTimeScale()
	{
		if (_isLoading)
		{
			useInterpolation = false;
			return;
		}
		if (Time.timeScale < 1f)
		{
			useInterpolation = true;
			return;
		}
		bool isPresent = XRDevice.isPresent;
		if (isMonitorOnly || !isPresent)
		{
			if (Time.fixedDeltaTime > 0.014f)
			{
				useInterpolation = true;
			}
			else
			{
				useInterpolation = false;
			}
			return;
		}
		if (Time.fixedDeltaTime <= 0.0069f)
		{
			useInterpolation = false;
			return;
		}
		float refreshRate = XRDevice.refreshRate;
		if (refreshRate != 0f)
		{
			bool flag = false;
			if (refreshRate <= 59f)
			{
				flag = true;
			}
			else if (refreshRate > 59f && refreshRate < 61f && Time.fixedDeltaTime > 0.0166f && Time.fixedDeltaTime < 0.0167f)
			{
				flag = true;
			}
			else if (refreshRate > 71f && refreshRate < 73f && Time.fixedDeltaTime > 0.0138f && Time.fixedDeltaTime < 0.0139f)
			{
				flag = true;
			}
			else if (refreshRate > 79f && refreshRate < 81f && Time.fixedDeltaTime > 0.0124f && Time.fixedDeltaTime < 0.0126f)
			{
				flag = true;
			}
			else if (refreshRate > 89f && refreshRate < 91f && Time.fixedDeltaTime > 0.0111f && Time.fixedDeltaTime < 0.0112f)
			{
				flag = true;
			}
			else if (refreshRate > 119f && refreshRate < 121f && Time.fixedDeltaTime > 0.0083f && Time.fixedDeltaTime < 0.0084f)
			{
				flag = true;
			}
			else if (refreshRate > 143f && refreshRate < 145f && Time.fixedDeltaTime > 0.0069f && Time.fixedDeltaTime < 0.007f)
			{
				flag = true;
			}
			if (flag)
			{
				useInterpolation = false;
			}
			else
			{
				useInterpolation = true;
			}
		}
		else if (Time.fixedDeltaTime > 0.012f)
		{
			useInterpolation = true;
		}
		else
		{
			useInterpolation = false;
		}
	}

	protected void SyncLeapEnabled()
	{
		if (!(LeapRig != null))
		{
			return;
		}
		if (LeapServiceProviders != null)
		{
			LeapXRServiceProvider[] leapServiceProviders = LeapServiceProviders;
			foreach (LeapXRServiceProvider leapXRServiceProvider in leapServiceProviders)
			{
				leapXRServiceProvider.enabled = !leapDisabled && _leapMotionEnabled;
			}
		}
		LeapRig.gameObject.SetActive(!leapDisabled && _leapMotionEnabled);
	}

	protected void CheckAutoConnectLeapHands()
	{
		if (leapHandModelControl != null && leapHandMountLeft != null)
		{
			if (_leapHandLeftConnected)
			{
				if (!leapMotionEnabled || !leapHandModelControl.leftHandEnabled)
				{
					DisconnectLeapHandLeft();
				}
			}
			else if (leapMotionEnabled && leapHandModelControl.leftHandEnabled && leapHandMountLeft.gameObject.activeInHierarchy)
			{
				ConnectLeapHandLeft();
			}
		}
		if (!(leapHandModelControl != null) || !(leapHandMountRight != null))
		{
			return;
		}
		if (_leapHandRightConnected)
		{
			if (!leapMotionEnabled || !leapHandModelControl.rightHandEnabled)
			{
				DisconnectLeapHandRight();
			}
		}
		else if (leapMotionEnabled && leapHandModelControl.rightHandEnabled && leapHandMountRight.gameObject.activeInHierarchy)
		{
			ConnectLeapHandRight();
		}
	}

	protected void ConnectLeapHandLeft()
	{
		if (leftHand != null && leapHandMountLeft != null)
		{
			_leapHandLeftConnected = true;
			leftHand.transform.SetParent(leapHandMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationLeft = true;
		}
		if (ovrHandInputLeft != null)
		{
			ovrHandInputLeft.enabled = false;
		}
		if (steamVRHandInputLeft != null)
		{
			steamVRHandInputLeft.enabled = false;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void DisconnectLeapHandLeft()
	{
		_leapHandLeftConnected = false;
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationLeft = false;
		}
		if (ovrHandInputLeft != null)
		{
			ovrHandInputLeft.enabled = true;
		}
		if (steamVRHandInputLeft != null)
		{
			steamVRHandInputLeft.enabled = true;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void ConnectLeapHandRight()
	{
		if (rightHand != null && leapHandMountRight != null)
		{
			_leapHandRightConnected = true;
			rightHand.transform.SetParent(leapHandMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationRight = true;
		}
		if (ovrHandInputRight != null)
		{
			ovrHandInputRight.enabled = false;
		}
		if (steamVRHandInputRight != null)
		{
			steamVRHandInputRight.enabled = false;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void DisconnectLeapHandRight()
	{
		_leapHandRightConnected = false;
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationRight = false;
		}
		if (ovrHandInputRight != null)
		{
			ovrHandInputRight.enabled = true;
		}
		if (steamVRHandInputRight != null)
		{
			steamVRHandInputRight.enabled = true;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void SyncTrackerVisibility()
	{
		SyncTracker1Visibility();
		SyncTracker2Visibility();
		SyncTracker3Visibility();
		SyncTracker4Visibility();
		SyncTracker5Visibility();
		SyncTracker6Visibility();
		SyncTracker7Visibility();
		SyncTracker8Visibility();
	}

	public void SyncTracker1Visibility()
	{
		if (!(viveTracker1 != null) || !(viveTracker1Model != null))
		{
			return;
		}
		bool active = _tracker1Visible && !_hideTrackers;
		viveTracker1Model.enabled = active;
		foreach (Transform item in viveTracker1Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker2Visibility()
	{
		if (!(viveTracker2 != null) || !(viveTracker2Model != null))
		{
			return;
		}
		bool active = _tracker2Visible && !_hideTrackers;
		viveTracker2Model.enabled = active;
		foreach (Transform item in viveTracker2Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker3Visibility()
	{
		if (!(viveTracker3 != null) || !(viveTracker3Model != null))
		{
			return;
		}
		bool active = _tracker3Visible && !_hideTrackers;
		viveTracker3Model.enabled = active;
		foreach (Transform item in viveTracker3Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker4Visibility()
	{
		if (!(viveTracker4 != null) || !(viveTracker4Model != null))
		{
			return;
		}
		bool active = _tracker4Visible && !_hideTrackers;
		viveTracker4Model.enabled = active;
		foreach (Transform item in viveTracker4Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker5Visibility()
	{
		if (!(viveTracker5 != null) || !(viveTracker5Model != null))
		{
			return;
		}
		bool active = _tracker5Visible && !_hideTrackers;
		viveTracker5Model.enabled = active;
		foreach (Transform item in viveTracker5Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker6Visibility()
	{
		if (!(viveTracker6 != null) || !(viveTracker6Model != null))
		{
			return;
		}
		bool active = _tracker6Visible && !_hideTrackers;
		viveTracker6Model.enabled = active;
		foreach (Transform item in viveTracker6Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker7Visibility()
	{
		if (!(viveTracker7 != null) || !(viveTracker7Model != null))
		{
			return;
		}
		bool active = _tracker7Visible && !_hideTrackers;
		viveTracker7Model.enabled = active;
		foreach (Transform item in viveTracker7Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker8Visibility()
	{
		if (!(viveTracker8 != null) || !(viveTracker8Model != null))
		{
			return;
		}
		bool active = _tracker8Visible && !_hideTrackers;
		viveTracker8Model.enabled = active;
		foreach (Transform item in viveTracker8Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	private void ConnectCenterCameraTarget(Transform parent, Vector3 offset, bool isMonitor)
	{
		if (!(centerCameraTarget != null))
		{
			return;
		}
		centerCameraTarget.transform.SetParent(parent);
		centerCameraTarget.transform.localPosition = offset;
		centerCameraTarget.transform.localRotation = Quaternion.identity;
		centerCameraTarget.FindCamera();
		centerCameraTarget.isMonitorCamera = isMonitor;
		if (centerCameraTargetDisableWhenMonitor != null)
		{
			GameObject[] array = centerCameraTargetDisableWhenMonitor;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(!isMonitor);
			}
		}
	}

	protected void SyncGenerateDepthTexture()
	{
		if (_generateDepthTexture)
		{
			if (MonitorCenterCamera != null)
			{
				MonitorCenterCamera.depthTextureMode |= DepthTextureMode.Depth;
			}
			if (OVRCenterCamera != null)
			{
				OVRCenterCamera.depthTextureMode |= DepthTextureMode.Depth;
			}
			if (ViveCenterCamera != null)
			{
				ViveCenterCamera.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
	}

	protected void InitAudioListeners()
	{
		additionalAudioListeners = new LinkedList<AudioListener>();
		if (MonitorCenterCamera != null)
		{
			monitorRigAudioListener = MonitorCenterCamera.GetComponent<AudioListener>();
		}
		if (OVRCenterCamera != null)
		{
			ovrRigAudioListener = OVRCenterCamera.GetComponent<AudioListener>();
		}
		if (ViveCenterCamera != null)
		{
			openVRRigAudioListener = ViveCenterCamera.GetComponent<AudioListener>();
		}
		SyncAudioListener();
	}

	protected void SyncAudioListener()
	{
		foreach (AudioListener additionalAudioListener in additionalAudioListeners)
		{
			additionalAudioListener.enabled = false;
		}
		if (monitorRigAudioListener != null)
		{
			monitorRigAudioListener.enabled = false;
		}
		if (ovrRigAudioListener != null)
		{
			ovrRigAudioListener.enabled = false;
		}
		if (openVRRigAudioListener != null)
		{
			openVRRigAudioListener.enabled = false;
		}
		if (overrideAudioListener != null)
		{
			currentAudioListener = overrideAudioListener;
			overrideAudioListener.enabled = true;
		}
		else if (isMonitorOnly || (MonitorRigActive && _useMonitorRigAudioListenerWhenActive))
		{
			if (monitorRigAudioListener != null)
			{
				monitorRigAudioListener.enabled = true;
			}
			currentAudioListener = monitorRigAudioListener;
		}
		else if (isOVR)
		{
			if (ovrRigAudioListener != null)
			{
				ovrRigAudioListener.enabled = true;
			}
			currentAudioListener = ovrRigAudioListener;
		}
		else if (isOpenVR)
		{
			if (openVRRigAudioListener != null)
			{
				openVRRigAudioListener.enabled = true;
			}
			currentAudioListener = openVRRigAudioListener;
		}
	}

	public void PushAudioListener(AudioListener audioListener)
	{
		if (!additionalAudioListeners.Contains(audioListener))
		{
			additionalAudioListeners.AddLast(audioListener);
			overrideAudioListener = additionalAudioListeners.Last.Value;
			SyncAudioListener();
		}
	}

	public void RemoveAudioListener(AudioListener audioListener)
	{
		if (additionalAudioListeners.Contains(audioListener))
		{
			additionalAudioListeners.Remove(audioListener);
			audioListener.enabled = false;
			if (additionalAudioListeners.Count > 0)
			{
				overrideAudioListener = additionalAudioListeners.Last.Value;
			}
			else
			{
				overrideAudioListener = null;
			}
			SyncAudioListener();
		}
	}

	private void SyncMonitorAuxUI()
	{
		if (MonitorModeAuxUI != null)
		{
			MonitorModeAuxUI.gameObject.SetActive(MonitorRigActive && !UIDisabled && !worldUIActivated);
		}
	}

	public void ToggleMonitorUI()
	{
		if (!(MonitorUI != null) || UIDisabled)
		{
			return;
		}
		if (MonitorUI.gameObject.activeSelf)
		{
			_toggleMonitorSaveMainHUDVisible = _mainHUDVisible;
			MonitorUI.gameObject.SetActive(false);
			_monitorUIVisible = false;
			HideMainHUD();
			return;
		}
		MonitorUI.gameObject.SetActive(true);
		_monitorUIVisible = true;
		if (_toggleMonitorSaveMainHUDVisible)
		{
			ShowMainHUDMonitor();
		}
	}

	public void HideMonitorUI()
	{
		if (MonitorUI != null && !UIDisabled && MonitorUI.gameObject.activeSelf)
		{
			_toggleMonitorSaveMainHUDVisible = _mainHUDVisible;
			_monitorUIVisible = false;
			MonitorUI.gameObject.SetActive(false);
			HideMainHUD();
		}
	}

	public void ShowMonitorUI()
	{
		if (MonitorUI != null && !UIDisabled && !MonitorUI.gameObject.activeSelf)
		{
			MonitorUI.gameObject.SetActive(true);
			_monitorUIVisible = true;
			if (_toggleMonitorSaveMainHUDVisible)
			{
				ShowMainHUDMonitor();
			}
		}
	}

	private void SyncMonitorCameraFOV()
	{
		if (MonitorCenterCamera != null)
		{
			if (worldUIActivated)
			{
				MonitorCenterCamera.fieldOfView = 40f;
			}
			else
			{
				MonitorCenterCamera.fieldOfView = _monitorCameraFOV;
			}
		}
	}

	public void ToggleMainMonitor()
	{
		if (!(MonitorRig != null))
		{
			return;
		}
		if (MonitorRigActive)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(false);
			SyncAudioListener();
			SyncMonitorAuxUI();
			SyncWorldUIAnchor();
			if (centerCameraTarget != null && !isMonitorOnly && saveCenterEyeAttachPoint != null)
			{
				ConnectCenterCameraTarget(saveCenterEyeAttachPoint, Vector3.zero, false);
			}
		}
		else
		{
			if (UserPreferences.singleton != null)
			{
				UserPreferences.singleton.overlayUI = true;
			}
			if (commonHandModelControl != null)
			{
				commonHandModelControl.useCollision = false;
			}
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.useCollision = false;
			}
			MonitorRigActive = true;
			MonitorRig.gameObject.SetActive(true);
			SyncAudioListener();
			SyncMonitorAuxUI();
			SyncWorldUIAnchor();
			if (MonitorUIAttachPoint != null)
			{
				MoveMainHUD(MonitorUIAttachPoint);
			}
			if (centerCameraTarget != null && !isMonitorOnly)
			{
				saveCenterEyeAttachPoint = centerCameraTarget.transform.parent;
				ConnectCenterCameraTarget(MonitorCenterCamera.transform, MonitorCenterCameraOffset, true);
			}
			if (_mainHUDVisible)
			{
				HideMainHUD();
				ShowMainHUDAuto();
			}
			else
			{
				ShowMainHUDAuto();
				HideMainHUD();
			}
		}
		SyncUISide();
	}

	protected void SetMonitorRig()
	{
		isOVR = false;
		isOpenVR = false;
		isMonitorOnly = true;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(false);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(false);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = true;
			MonitorRig.gameObject.SetActive(true);
		}
		SyncWorldUIAnchor();
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(false);
		}
		if (MonitorCenterCamera != null && centerCameraTarget != null)
		{
			ConnectCenterCameraTarget(MonitorCenterCamera.transform, MonitorCenterCameraOffset, true);
		}
		if (MonitorUIAttachPoint != null)
		{
			MoveMainHUD(MonitorUIAttachPoint);
			mainHUDAttachPointStartingPosition = mainHUDAttachPoint.localPosition;
			mainHUDAttachPointStartingRotation = mainHUDAttachPoint.localRotation;
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(true);
			}
		}
		SyncActiveHands();
	}

	public void SetOculusThumbstickFunctionFromString(string str)
	{
		switch (str)
		{
		case "GrabWorld":
			oculusThumbstickFunction = ThumbstickFunction.GrabWorld;
			break;
		case "SwapAxis":
			oculusThumbstickFunction = ThumbstickFunction.SwapAxis;
			break;
		case "Both":
			oculusThumbstickFunction = ThumbstickFunction.Both;
			break;
		default:
			UnityEngine.Debug.LogWarning("Tried to set oculusThumbstickFunction to " + str + " which is not a valid type");
			break;
		}
	}

	protected void SetOculusRig()
	{
		isOVR = true;
		isOpenVR = false;
		isMonitorOnly = false;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(true);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(false);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(false);
		}
		SyncWorldUIAnchor();
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(true);
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(true);
			}
		}
		if (OVRCenterCamera != null && centerCameraTarget != null)
		{
			ConnectCenterCameraTarget(OVRCenterCamera.transform, Vector3.zero, false);
		}
		if (touchObjectLeft != null)
		{
			touchObjectLeftMeshRenderers = touchObjectLeft.GetComponentsInChildren<MeshRenderer>(true);
			Camera component = touchObjectLeft.GetComponent<Camera>();
			if (component != null)
			{
				leftControllerCamera = component;
			}
		}
		if (touchObjectRight != null)
		{
			touchObjectRightMeshRenderers = touchObjectRight.GetComponentsInChildren<MeshRenderer>(true);
			Camera component2 = touchObjectRight.GetComponent<Camera>();
			if (component2 != null)
			{
				rightControllerCamera = component2;
			}
		}
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		if (leftHandAlternate != null)
		{
			leftHandAlternate.transform.SetParent(handMountLeft);
			leftHandAlternate.transform.localPosition = Vector3.zero;
			leftHandAlternate.transform.localRotation = Quaternion.identity;
		}
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		if (rightHandAlternate != null)
		{
			rightHandAlternate.transform.SetParent(handMountRight);
			rightHandAlternate.transform.localPosition = Vector3.zero;
			rightHandAlternate.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
	}

	protected void SetOpenVRRig()
	{
		isOVR = false;
		isOpenVR = true;
		isMonitorOnly = false;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(false);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(true);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(false);
		}
		SyncWorldUIAnchor();
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(true);
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(true);
			}
		}
		if (ViveCenterCamera != null && centerCameraTarget != null)
		{
			ConnectCenterCameraTarget(ViveCenterCamera.transform, Vector3.zero, false);
		}
		if (viveObjectLeft != null)
		{
			Camera component = viveObjectLeft.GetComponent<Camera>();
			if (component != null)
			{
				leftControllerCamera = component;
			}
			else
			{
				Error("Could not find camera on left controller");
			}
		}
		if (viveObjectRight != null)
		{
			Camera component2 = viveObjectRight.GetComponent<Camera>();
			if (component2 != null)
			{
				rightControllerCamera = component2;
			}
			else
			{
				Error("Could not find camera on right controller");
			}
		}
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		if (leftHandAlternate != null)
		{
			leftHandAlternate.transform.SetParent(handMountLeft);
			leftHandAlternate.transform.localPosition = Vector3.zero;
			leftHandAlternate.transform.localRotation = Quaternion.identity;
		}
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		if (rightHandAlternate != null)
		{
			rightHandAlternate.transform.SetParent(handMountRight);
			rightHandAlternate.transform.localPosition = Vector3.zero;
			rightHandAlternate.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (!Application.isEditor)
		{
			string dataPath = Application.dataPath;
			int num = dataPath.LastIndexOf('/');
			dataPath = dataPath.Remove(num, dataPath.Length - num);
			string pchApplicationManifestFullPath = Path.Combine(dataPath, "vrmanifest");
			EVRApplicationError eVRApplicationError = OpenVR.Applications.AddApplicationManifest(pchApplicationManifestFullPath, true);
			if (eVRApplicationError != 0)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR]</b> Error adding vr manifest file: " + eVRApplicationError);
			}
			else
			{
				UnityEngine.Debug.Log("<b>[SteamVR]</b> Successfully added VR manifest to SteamVR");
			}
			int id = Process.GetCurrentProcess().Id;
			EVRApplicationError eVRApplicationError2 = OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
			if (eVRApplicationError2 != 0)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR]</b> Error identifying application: " + eVRApplicationError2);
			}
			else
			{
				UnityEngine.Debug.Log($"<b>[SteamVR]</b> Successfully identified process as project to SteamVR ({SteamVR_Settings.instance.editorAppKey})");
			}
		}
	}

	public void OpenSteamVRBindingsInBrowser()
	{
		Process.Start("http://localhost:8998/dashboard/controllerbinding.html?app=" + SteamVR_Settings.instance.editorAppKey);
	}

	protected void DetermineVRRig()
	{
		Application.targetFrameRate = 300;
		if (VRDisabled)
		{
			SetMonitorRig();
			return;
		}
		string loadedDeviceName = XRSettings.loadedDeviceName;
		UnityEngine.Debug.Log("XR device active is " + XRSettings.isDeviceActive);
		UnityEngine.Debug.Log("XR device present is " + XRDevice.isPresent);
		UnityEngine.Debug.Log("Loaded XR device is " + loadedDeviceName);
		UnityEngine.Debug.Log("XR device model is " + XRDevice.model);
		UnityEngine.Debug.Log("XR device refresh rate is " + XRDevice.refreshRate);
		if (!XRSettings.isDeviceActive || loadedDeviceName == null || loadedDeviceName == string.Empty)
		{
			SetMonitorRig();
		}
		else if (loadedDeviceName == "Oculus")
		{
			SetOculusRig();
		}
		else
		{
			SetOpenVRRig();
		}
	}

	public string GetMD5Hash(byte[] bytes)
	{
		if (md5 == null)
		{
			md5 = new MD5CryptoServiceProvider();
		}
		byte[] array = md5.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	protected void DestroyScriptController(MVRScriptController mvrsc)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (mvrsc.script != null)
		{
			try
			{
				UnityEngine.Object.DestroyImmediate(mvrsc.script);
			}
			catch (Exception)
			{
			}
			mvrsc.script = null;
		}
		if (mvrsc.gameObject != null)
		{
			UnityEngine.Object.Destroy(mvrsc.gameObject);
			mvrsc.gameObject = null;
		}
	}

	protected MVRScriptController CreateScriptController(string scriptUid, string url, ScriptType type)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		MVRScriptController mVRScriptController = new MVRScriptController();
		GameObject gameObject = new GameObject(scriptUid + "temp");
		gameObject.transform.SetParent(bootstrapPluginContainer);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		mVRScriptController.gameObject = gameObject;
		ScriptProxy scriptProxy = type.CreateInstance(gameObject);
		if (scriptProxy == null)
		{
			LogError("Failed to create instance of " + url);
			UnityEngine.Object.Destroy(gameObject);
			return null;
		}
		string text = scriptUid + "_" + scriptProxy.GetInstanceType().ToString();
		gameObject.name = text;
		MVRScript component = gameObject.GetComponent<MVRScript>();
		component.exclude = true;
		mVRScriptController.script = component;
		if (component.ShouldIgnore())
		{
			try
			{
				DestroyScriptController(mVRScriptController);
			}
			catch
			{
			}
			mVRScriptController = null;
		}
		else
		{
			try
			{
				component.ForceAwake();
				component.containingAtom = null;
				component.manager = null;
				try
				{
					component.Init();
				}
				catch (Exception ex)
				{
					LogError("Exception during plugin script Init: " + ex);
				}
			}
			catch (Exception ex2)
			{
				LogError("Exception during plugin script Awake: " + ex2);
				DestroyScriptController(mVRScriptController);
				mVRScriptController = null;
			}
		}
		return mVRScriptController;
	}

	protected void LoadBootstrapPlugin(string url)
	{
		FileManager.AssertNotCalledFromPlugin();
		if (bootstrapPluginDomain == null)
		{
			bootstrapPluginDomain = ScriptDomain.CreateDomain("MVRBootstrapPlugins", true);
			IEnumerable<string> enumerable = GetResolvedVersionDefines();
			if (enumerable != null)
			{
				foreach (string item in enumerable)
				{
					bootstrapPluginDomain.CompilerService.AddConditionalSymbol(item);
				}
			}
		}
		if (bootstrapPluginScriptControllers == null)
		{
			bootstrapPluginScriptControllers = new Dictionary<string, List<MVRScriptController>>();
		}
		try
		{
			Location.Reset();
			string text = url.Replace("/", "_");
			text = text.Replace("\\", "_");
			text = text.Replace(".", "_");
			text = text.Replace(":", "_");
			if (url.EndsWith(".cslist") || url.EndsWith(".dll"))
			{
				ScriptAssembly scriptAssembly = null;
				if (url.EndsWith(".cslist"))
				{
					string directoryName = FileManager.GetDirectoryName(url);
					List<string> list = new List<string>();
					bool flag = false;
					using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(url, true))
					{
						StreamReader streamReader = fileEntryStreamReader.StreamReader;
						string text2;
						while ((text2 = streamReader.ReadLine()) != null)
						{
							string text3 = text2.Trim();
							if (directoryName != string.Empty && !text3.Contains(":/"))
							{
								text3 = directoryName + "/" + text3;
							}
							if (!(text3 != string.Empty))
							{
								continue;
							}
							text3 = text3.Replace('/', '\\');
							if (FileManager.IsFileInPackage(text3))
							{
								VarFileEntry varFileEntry = FileManager.GetVarFileEntry(text3);
								if (varFileEntry != null)
								{
									flag = true;
									list.Add(text3);
								}
							}
							else
							{
								list.Add(text3);
							}
						}
					}
					if (list.Count <= 0)
					{
						return;
					}
					try
					{
						if (flag)
						{
							List<string> list2 = new List<string>();
							StringBuilder stringBuilder = new StringBuilder();
							foreach (string item2 in list)
							{
								string text4 = FileManager.ReadAllText(item2, true);
								stringBuilder.Append(text4);
								list2.Add(text4);
							}
							string suggestedAssemblyNamePrefix = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
							bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(suggestedAssemblyNamePrefix);
							scriptAssembly = bootstrapPluginDomain.CompileAndLoadScriptSources(list2.ToArray());
						}
						else
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							foreach (string item3 in list)
							{
								string value = FileManager.ReadAllText(item3, true);
								stringBuilder2.Append(value);
							}
							string suggestedAssemblyNamePrefix2 = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(Encoding.ASCII.GetBytes(stringBuilder2.ToString()));
							bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(suggestedAssemblyNamePrefix2);
							scriptAssembly = bootstrapPluginDomain.CompileAndLoadScriptFiles(list.ToArray());
						}
						if (scriptAssembly == null)
						{
							LogError("Compile of " + url + " failed. Errors:");
							string[] errors = bootstrapPluginDomain.CompilerService.Errors;
							foreach (string text5 in errors)
							{
								if (!text5.StartsWith("[CS]"))
								{
									LogError(text5 + "\n");
								}
							}
							return;
						}
					}
					catch (Exception ex)
					{
						LogError("Compile of " + url + " failed. Exception: " + ex);
						LogError("Compile of " + url + " failed. Errors:");
						string[] errors2 = bootstrapPluginDomain.CompilerService.Errors;
						foreach (string err in errors2)
						{
							LogError(err);
						}
						return;
					}
				}
				else if (FileManager.IsFileInPackage(url))
				{
					VarFileEntry varFileEntry2 = FileManager.GetVarFileEntry(url);
					if (varFileEntry2 != null)
					{
						VarPackage package = varFileEntry2.Package;
						byte[] array = FileManager.ReadAllBytes(url, true);
						string text6 = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(array);
						FileManager.RegisterPluginHashToPluginPath(text6, url);
						bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(text6);
						scriptAssembly = bootstrapPluginDomain.LoadAssembly(array);
					}
				}
				else
				{
					byte[] bytes = FileManager.ReadAllBytes(url, true);
					string text7 = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(bytes);
					FileManager.RegisterPluginHashToPluginPath(text7, url);
					bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(text7);
					scriptAssembly = bootstrapPluginDomain.LoadAssembly(url);
				}
				if (scriptAssembly != null)
				{
					ScriptType[] array2 = scriptAssembly.FindAllSubtypesOf<MVRScript>();
					if (array2.Length > 0)
					{
						List<MVRScriptController> list3 = new List<MVRScriptController>();
						ScriptType[] array3 = array2;
						foreach (ScriptType type in array3)
						{
							MVRScriptController mVRScriptController = CreateScriptController(text, url, type);
							if (mVRScriptController != null)
							{
								list3.Add(mVRScriptController);
							}
						}
						bootstrapPluginScriptControllers.Add(url, list3);
					}
					else
					{
						LogError("No MVRScript types found");
					}
				}
				else
				{
					LogError("Unable to load assembly from " + url);
				}
				return;
			}
			ScriptType scriptType = null;
			try
			{
				if (FileManager.IsFileInPackage(url))
				{
					VarFileEntry varFileEntry3 = FileManager.GetVarFileEntry(url);
					if (varFileEntry3 != null)
					{
						byte[] bytes2 = FileManager.ReadAllBytes(url, true);
						string suggestedAssemblyNamePrefix3 = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(bytes2);
						bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(suggestedAssemblyNamePrefix3);
						scriptType = bootstrapPluginDomain.CompileAndLoadScriptSource(FileManager.ReadAllText(url));
					}
				}
				else
				{
					byte[] bytes3 = FileManager.ReadAllBytes(url, true);
					string suggestedAssemblyNamePrefix4 = "MVRBootstrapPlugin_" + text + "_" + GetMD5Hash(bytes3);
					bootstrapPluginDomain.CompilerService.SetSuggestedAssemblyNamePrefix(suggestedAssemblyNamePrefix4);
					scriptType = bootstrapPluginDomain.CompileAndLoadScriptFile(url);
				}
				if (scriptType == null)
				{
					LogError("Compile of " + url + " failed. Errors:");
					string[] errors3 = bootstrapPluginDomain.CompilerService.Errors;
					foreach (string err2 in errors3)
					{
						LogError(err2);
					}
					return;
				}
			}
			catch (Exception ex2)
			{
				LogError("Compile of " + url + " failed. Exception: " + ex2);
				LogError("Compile of " + url + " failed. Errors:");
				string[] errors4 = bootstrapPluginDomain.CompilerService.Errors;
				foreach (string err3 in errors4)
				{
					LogError(err3);
				}
				return;
			}
			if (scriptType.IsSubtypeOf<MVRScript>())
			{
				MVRScriptController mVRScriptController2 = CreateScriptController(text, url, scriptType);
				if (mVRScriptController2 != null)
				{
					List<MVRScriptController> list4 = new List<MVRScriptController>();
					list4.Add(mVRScriptController2);
					bootstrapPluginScriptControllers.Add(url, list4);
				}
			}
			else
			{
				LogError("Script loaded at " + url + " must inherit from MVRScript");
			}
		}
		catch (Exception ex3)
		{
			LogError("Exception during compile of " + url + ": " + ex3);
		}
	}

	protected void UnloadBootstrapPlugin(string url)
	{
		FileManager.AssertNotCalledFromPlugin();
		List<MVRScriptController> value;
		if (bootstrapPluginScriptControllers == null || !bootstrapPluginScriptControllers.TryGetValue(url, out value))
		{
			return;
		}
		foreach (MVRScriptController item in value)
		{
			DestroyScriptController(item);
		}
		bootstrapPluginScriptControllers.Remove(url);
	}

	protected void UnloadAllBootstrapPlugins()
	{
		FileManager.AssertNotCalledFromPlugin();
		if (bootstrapPluginScriptControllers == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<MVRScriptController>> bootstrapPluginScriptController in bootstrapPluginScriptControllers)
		{
			foreach (MVRScriptController item in bootstrapPluginScriptController.Value)
			{
				DestroyScriptController(item);
			}
		}
	}

	protected void SyncVamX()
	{
		vamXVersion = 0;
		string text = NormalizeLoadPath(vamXBootstrapPluginPath);
		UnityEngine.Debug.Log("Check for vamX at " + text);
		if (FileManager.FileExists(text))
		{
			VarPackage package = FileManager.GetPackage("vamX.1.latest");
			if (package != null)
			{
				vamXVersion = package.Version;
			}
			else
			{
				vamXVersion = 0;
			}
			UnityEngine.Debug.Log("vamX was detected");
			_vamXInstalled = true;
		}
		else
		{
			UnityEngine.Debug.Log("vamX was not detected");
			_vamXInstalled = false;
		}
		if (vamXPanel != null)
		{
			vamXPanel.gameObject.SetActive(_vamXInstalled);
		}
		if (vamXEnabledGameObjects != null)
		{
			GameObject[] array = vamXEnabledGameObjects;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(_vamXInstalled);
				}
			}
		}
		if (vamXEnabledAndAdvancedSceneEditGameObjects != null)
		{
			GameObject[] array2 = vamXEnabledAndAdvancedSceneEditGameObjects;
			foreach (GameObject gameObject2 in array2)
			{
				if (gameObject2 != null)
				{
					gameObject2.SetActive(_vamXInstalled && !advancedSceneEditDisabled);
				}
			}
		}
		if (vamXDisabledGameObjects != null)
		{
			GameObject[] array3 = vamXDisabledGameObjects;
			foreach (GameObject gameObject3 in array3)
			{
				if (gameObject3 != null)
				{
					gameObject3.SetActive(!_vamXInstalled);
				}
			}
		}
		if (vamXDisabledAndAdvancedSceneEditGameObjects != null)
		{
			GameObject[] array4 = vamXDisabledAndAdvancedSceneEditGameObjects;
			foreach (GameObject gameObject4 in array4)
			{
				if (gameObject4 != null)
				{
					gameObject4.SetActive(!_vamXInstalled && !advancedSceneEditDisabled);
				}
			}
		}
		if (vamXDisabledAndAdvancedSceneEditDisabledGameObjects != null)
		{
			GameObject[] array5 = vamXDisabledAndAdvancedSceneEditDisabledGameObjects;
			foreach (GameObject gameObject5 in array5)
			{
				if (gameObject5 != null)
				{
					gameObject5.SetActive(!_vamXInstalled && advancedSceneEditDisabled);
				}
			}
		}
		if (_vamXInstalled)
		{
			if (_vamXWasInstalled)
			{
				if (lastLoadedvamXBootstrapPath != text)
				{
					UnityEngine.Debug.Log("Reloading vamX plugin due to version change");
					UnloadBootstrapPlugin(lastLoadedvamXBootstrapPath);
					LoadBootstrapPlugin(text);
					lastLoadedvamXBootstrapPath = text;
				}
			}
			else
			{
				UnityEngine.Debug.Log("Adding vamX bootstrap plugin at " + text);
				LoadBootstrapPlugin(text);
				lastLoadedvamXBootstrapPath = text;
				_vamXWasInstalled = true;
			}
		}
		else if (_vamXWasInstalled && !_vamXInstalled)
		{
			UnityEngine.Debug.Log("Removing vamX bootstrap plugin");
			UnloadBootstrapPlugin(lastLoadedvamXBootstrapPath);
			_vamXWasInstalled = false;
			lastLoadedvamXBootstrapPath = null;
		}
		SyncVersionText();
	}

	public void OpenVamXTutorialScene()
	{
		string text = NormalizeLoadPath(vamXTutorialScene);
		if (FileManager.FileExists(text))
		{
			Load(text);
		}
		else
		{
			LogError("vamX tutorial scene " + text + " does not exist");
		}
	}

	public void OpenVamXMainScene()
	{
		string text = NormalizeLoadPath(vamXMainScene);
		if (FileManager.FileExists(text))
		{
			Load(text);
		}
		else
		{
			LogError("vamX main scene " + text + " does not exist");
		}
	}

	private void Awake()
	{
		_singleton = this;
		ZipConstants.DefaultCodePage = 0;
		DetermineVRRig();
		InitAudioListeners();
		SetSceneLoadPosition();
		SyncVersion();
		FileManager.ClearSecureReadPaths();
		FileManager.RegisterSecureReadPath(".");
		FileManager.ClearSecureWritePaths();
		FileManager.RegisterInternalSecureWritePath("Keys");
		FileManager.RegisterInternalSecureWritePath("Saves");
		FileManager.RegisterInternalSecureWritePath("Cache");
		FileManager.RegisterInternalSecureWritePath("Custom");
		FileManager.RegisterInternalSecureWritePath("AddonPackages");
		FileManager.RegisterInternalSecureWritePath("AddonPackagesBuilder");
		FileManager.RegisterInternalSecureWritePath("Temp");
		FileManager.RegisterPluginSecureWritePath("Saves", false);
		FileManager.RegisterPluginSecureWritePath("Custom", false);
		FileManager.RegisterPluginSecureWritePath("Saves/PluginData", true);
		FileManager.RegisterPluginSecureWritePath("Custom/PluginData", true);
		FileManager.CreateDirectory("Saves");
		FileManager.CreateDirectory("Saves/PluginData");
		FileManager.CreateDirectory("Saves/scene");
		FileManager.CreateDirectory("Custom");
		FileManager.CreateDirectory("Custom/PluginData");
		FileManager.demoPackagePrefixes = demoPackagePrefixes;
		FileManager.RegisterRestrictedReadPath("BrowserProfile");
		FileManager.RegisterRefreshHandler(OnPackageRefresh);
		if (hubBrowser != null)
		{
			HubBrowse hubBrowse = hubBrowser;
			hubBrowse.preShowCallbacks = (HubBrowse.PreShowCallback)System.Delegate.Combine(hubBrowse.preShowCallbacks, new HubBrowse.PreShowCallback(ActivateWorldUI));
			HubBrowse hubBrowse2 = hubBrowser;
			hubBrowse2.enableHubCallbacks = (HubBrowse.EnableHubCallback)System.Delegate.Combine(hubBrowse2.enableHubCallbacks, (HubBrowse.EnableHubCallback)delegate
			{
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.enableHub = true;
				}
			});
			if (UserPreferences.singleton != null)
			{
				hubBrowser.HubEnabled = UserPreferences.singleton.enableHub;
			}
			HubBrowse hubBrowse3 = hubBrowser;
			hubBrowse3.enableWebBrowserCallbacks = (HubBrowse.EnableWebBrowserCallback)System.Delegate.Combine(hubBrowse3.enableWebBrowserCallbacks, (HubBrowse.EnableWebBrowserCallback)delegate
			{
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.enableWebBrowser = true;
				}
			});
			if (UserPreferences.singleton != null)
			{
				hubBrowser.WebBrowserEnabled = UserPreferences.singleton.enableWebBrowser;
			}
		}
		SyncToKeyFile();
	}

	private IEnumerator DelayLoadSessionPlugins()
	{
		yield return null;
		while (UserPreferences.singleton != null && UserPreferences.singleton.firstTimeUser)
		{
			yield return null;
		}
		if (sessionPresetManagerControl != null)
		{
			sessionPresetManagerControl.LoadUserDefaults();
		}
	}

	private IEnumerator DelayStart()
	{
		onStartScene = true;
		yield return null;
		yield return null;
		yield return null;
		StartScene();
	}

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		Caching.ClearCache();
		SetSavesDirFromCommandline();
		castRay = default(Ray);
		playerNavTrackerGO = new GameObject();
		StartCoroutine(InitAssetManager());
		monitorCameraFOV = startingMonitorCameraFOV;
		SyncMonitorCameraFOV();
		InitAtomPool();
		InitAtoms();
		InitTargets();
		InitMotionControllerNaviation();
		if (UserPreferences.singleton != null && UserPreferences.singleton.shouldLoadPrefsFileOnStart)
		{
			UserPreferences.singleton.RestorePreferences();
		}
		InitUI();
		ResetFocusPoint();
		SyncLeapEnabled();
		SyncGameMode();
		BuildMigrationMappings();
		bool flag = false;
		bool flag2 = false;
		SyncVamX();
		if (!UIDisabled)
		{
			StartCoroutine(DelayLoadSessionPlugins());
			flag = BuildFilesToMigrateMap();
			flag2 = BuildObsoleteDirectoriesList();
			if (flag || flag2)
			{
				OpenTopWorldUI();
			}
		}
		bool flag3 = false;
		if (UserPreferences.singleton != null && UserPreferences.singleton.firstTimeUser)
		{
			flag3 = true;
		}
		if (!flag && !flag2 && startSceneEnabled && !flag3 && (!showMainHUDOnStart || UIDisabled || _onStartupSkipStartScreen))
		{
			StartCoroutine(DelayStart());
		}
	}

	private void Update()
	{
		CheckResumeSimulation();
		CheckResumeAutoSimulation();
		CheckResumeRender();
		CheckLoadingIcon();
		CheckMessageAndErrorQueue();
		SyncAllErrorsInputFields();
		SyncAllMessagesInputFields();
		if (Time.unscaledDeltaTime > 0.015f)
		{
			DebugHUD.Alert1();
		}
		if (CameraTarget.centerTarget != null && CameraTarget.centerTarget.targetCamera != null)
		{
			lookCamera = CameraTarget.centerTarget.targetCamera;
		}
		drawRayLineRight = false;
		drawRayLineLeft = false;
		SyncNavigationHologridVisibility();
		PrepControllers();
		ProcessTimeScale();
		if (!onStartScene)
		{
			CheckAutoConnectLeapHands();
			ProcessUI();
			ProcessGUIInteract();
			ProcessPlayerNavMove();
			ProcessMouseChange();
			ProcessMouseControl();
			ProcessKeyBindings();
			ProcessKeyboardFreeNavigation();
			ProcessCycle();
			VerifyPossess();
			if (selectMode == SelectMode.FreeMoveMouse)
			{
				if (mouseCrosshairPointer != null)
				{
					mouseCrosshairPointer.anchoredPosition = Input.mousePosition;
				}
				ProcessMouseFreeNavigation();
				ProcessLookAtTrigger();
				ProcessCommonTargetSelection();
				ProcessTargetShow(false);
				ProcessMouseTargetControl();
			}
			else if (selectMode == SelectMode.FreeMove)
			{
				ProcessFreeMoveNavigation();
				ProcessLookAtTrigger();
			}
			else if (selectMode == SelectMode.Teleport)
			{
				ProcessTeleportMode();
			}
			else
			{
				ProcessControllerNavigation(freeMoveAction);
				switch (selectMode)
				{
				case SelectMode.Screenshot:
					drawRayLineLeft = !MonitorRigActive;
					drawRayLineRight = !MonitorRigActive;
					ProcessMotionControllerNavigation();
					ProcessHiResScreenshot();
					break;
				case SelectMode.SaveScreenshot:
					drawRayLineLeft = !MonitorRigActive;
					drawRayLineRight = !MonitorRigActive;
					ProcessMotionControllerNavigation();
					ProcessSaveScreenshot();
					break;
				case SelectMode.Off:
				case SelectMode.FilteredTargets:
				case SelectMode.Targets:
					ProcessLookAtTrigger();
					ProcessCommonTargetSelection();
					ProcessTargetShow();
					ProcessMouseTargetControl();
					ProcessMotionControllerTargetHighlight();
					ProcessMotionControllerNavigation();
					ProcessMotionControllerTargetControl();
					break;
				case SelectMode.Possess:
					ProcessMotionControllerNavigation();
					ProcessPossess();
					break;
				case SelectMode.TwoStagePossess:
					ProcessMotionControllerNavigation();
					ProcessTwoStagePossess();
					break;
				case SelectMode.AnimationRecord:
					ProcessTargetShow(false);
					ProcessMouseTargetControl(false);
					ProcessMotionControllerTargetHighlight();
					ProcessMotionControllerNavigation();
					ProcessMotionControllerTargetControl(false);
					ProcessAnimationRecord();
					break;
				case SelectMode.Custom:
					drawRayLineLeft = false;
					drawRayLineRight = false;
					ProcessMotionControllerNavigation();
					break;
				case SelectMode.CustomWithTargetControl:
					ProcessTargetShow(false);
					ProcessMouseTargetControl(false);
					ProcessMotionControllerTargetHighlight();
					ProcessMotionControllerNavigation();
					ProcessMotionControllerTargetControl(false);
					break;
				case SelectMode.CustomWithVRTargetControl:
					ProcessTargetShow(false);
					ProcessMotionControllerTargetHighlight();
					ProcessMotionControllerNavigation();
					ProcessMotionControllerTargetControl(false);
					break;
				default:
					ProcessMotionControllerSelect();
					ProcessMotionControllerNavigation();
					ProcessMouseSelect();
					break;
				}
			}
			ProcessUIMove();
			SyncCursor();
		}
		if (!MonitorRigActive)
		{
			if (_mainHUDVisible)
			{
				drawRayLineLeft = true;
				drawRayLineRight = true;
			}
			else if (UserPreferences.singleton.alwaysShowPointersOnTouch)
			{
				drawRayLineRight = (drawRayLineLeft = GetTargetShow());
			}
		}
		if (leftControllerCamera != null && !leftControllerCamera.gameObject.activeInHierarchy)
		{
			drawRayLineLeft = false;
		}
		if (rightControllerCamera != null && !rightControllerCamera.gameObject.activeInHierarchy)
		{
			drawRayLineRight = false;
		}
		if (drawRayLineLeft && motionControllerLeft != null)
		{
			if (rayLineDrawerLeft != null)
			{
				rayLineDrawerLeft.SetLinePoints(motionControllerLeft.position, motionControllerLeft.position + 50f * motionControllerLeft.forward);
				rayLineDrawerLeft.Draw(base.gameObject.layer);
			}
			if (rayLineLeft != null)
			{
				rayLineLeft.transform.position = motionControllerLeft.position;
				rayLineLeft.transform.rotation = motionControllerLeft.rotation;
				rayLineLeft.gameObject.SetActive(true);
			}
		}
		else if (rayLineLeft != null)
		{
			rayLineLeft.gameObject.SetActive(false);
		}
		if (drawRayLineRight && motionControllerRight != null)
		{
			if (rayLineDrawerRight != null)
			{
				rayLineDrawerRight.SetLinePoints(motionControllerRight.position, motionControllerRight.position + 50f * motionControllerRight.forward);
				rayLineDrawerRight.Draw(base.gameObject.layer);
			}
			if (rayLineRight != null)
			{
				rayLineRight.transform.position = motionControllerRight.position;
				rayLineRight.transform.rotation = motionControllerRight.rotation;
				rayLineRight.gameObject.SetActive(true);
			}
		}
		else if (rayLineRight != null)
		{
			rayLineRight.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		UnloadAllBootstrapPlugins();
		FileManager.UnregisterRefreshHandler(OnPackageRefresh);
	}
}
