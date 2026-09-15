using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class OVRManager : MonoBehaviour
{
	public enum TrackingOrigin
	{
		EyeLevel,
		FloorLevel
	}

	public enum EyeTextureFormat
	{
		Default = 0,
		R16G16B16A16_FP = 2,
		R11G11B10_FP = 3
	}

	public enum TiledMultiResLevel
	{
		Off,
		LMSLow,
		LMSMedium,
		LMSHigh
	}

	public enum CompositionMethod
	{
		External,
		Direct,
		Sandwich
	}

	public enum CameraDevice
	{
		WebCamera0,
		WebCamera1,
		ZEDCamera
	}

	public enum DepthQuality
	{
		Low,
		Medium,
		High
	}

	public enum VirtualGreenScreenType
	{
		Off,
		OuterBoundary,
		PlayArea
	}

	private static OVRProfile _profile;

	private IEnumerable<Camera> disabledCameras;

	private float prevTimeScale;

	private static bool _isHmdPresentCached = false;

	private static bool _isHmdPresent = false;

	private static bool _wasHmdPresent = false;

	private static bool _hasVrFocusCached = false;

	private static bool _hasVrFocus = false;

	private static bool _hadVrFocus = false;

	private static bool _hadInputFocus = true;

	[Header("Performance/Quality")]
	[Tooltip("If true, distortion rendering work is submitted a quarter-frame early to avoid pipeline stalls and increase CPU-GPU parallelism.")]
	public bool queueAhead = true;

	[Tooltip("If true, Unity will use the optimal antialiasing level for quality/performance on the current hardware.")]
	public bool useRecommendedMSAALevel;

	[Tooltip("If true, dynamic resolution will be enabled On PC")]
	public bool enableAdaptiveResolution;

	[Range(0.5f, 2f)]
	[Tooltip("Min RenderScale the app can reach under adaptive resolution mode")]
	public float minRenderScale = 0.7f;

	[Range(0.5f, 2f)]
	[Tooltip("Max RenderScale the app can reach under adaptive resolution mode")]
	public float maxRenderScale = 1f;

	[HideInInspector]
	public bool expandMixedRealityCapturePropertySheet;

	[HideInInspector]
	[Tooltip("If true, Mixed Reality mode will be enabled. It would be always set to false when the game is launching without editor")]
	public bool enableMixedReality;

	[HideInInspector]
	public CompositionMethod compositionMethod;

	[HideInInspector]
	[Tooltip("Extra hidden layers")]
	public LayerMask extraHiddenLayers;

	[HideInInspector]
	[Tooltip("The camera device for direct composition")]
	public CameraDevice capturingCameraDevice;

	[HideInInspector]
	[Tooltip("Flip the camera frame horizontally")]
	public bool flipCameraFrameHorizontally;

	[HideInInspector]
	[Tooltip("Flip the camera frame vertically")]
	public bool flipCameraFrameVertically;

	[HideInInspector]
	[Tooltip("Delay the touch controller pose by a short duration (0 to 0.5 second) to match the physical camera latency")]
	public float handPoseStateLatency;

	[HideInInspector]
	[Tooltip("Delay the foreground / background image in the sandwich composition to match the physical camera latency. The maximum duration is sandwichCompositionBufferedFrames / {Game FPS}")]
	public float sandwichCompositionRenderLatency;

	[HideInInspector]
	[Tooltip("The number of frames are buffered in the SandWich composition. The more buffered frames, the more memory it would consume.")]
	public int sandwichCompositionBufferedFrames = 8;

	[HideInInspector]
	[Tooltip("Chroma Key Color")]
	public Color chromaKeyColor = Color.green;

	[HideInInspector]
	[Tooltip("Chroma Key Similarity")]
	public float chromaKeySimilarity = 0.6f;

	[HideInInspector]
	[Tooltip("Chroma Key Smooth Range")]
	public float chromaKeySmoothRange = 0.03f;

	[HideInInspector]
	[Tooltip("Chroma Key Spill Range")]
	public float chromaKeySpillRange = 0.06f;

	[HideInInspector]
	[Tooltip("Use dynamic lighting (Depth sensor required)")]
	public bool useDynamicLighting;

	[HideInInspector]
	[Tooltip("The quality level of depth image. The lighting could be more smooth and accurate with high quality depth, but it would also be more costly in performance.")]
	public DepthQuality depthQuality = DepthQuality.Medium;

	[HideInInspector]
	[Tooltip("Smooth factor in dynamic lighting. Larger is smoother")]
	public float dynamicLightingSmoothFactor = 8f;

	[HideInInspector]
	[Tooltip("The maximum depth variation across the edges. Make it smaller to smooth the lighting on the edges.")]
	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	[HideInInspector]
	[Tooltip("Type of virutal green screen ")]
	public VirtualGreenScreenType virtualGreenScreenType;

	[HideInInspector]
	[Tooltip("Top Y of virtual green screen")]
	public float virtualGreenScreenTopY = 10f;

	[HideInInspector]
	[Tooltip("Bottom Y of virtual green screen")]
	public float virtualGreenScreenBottomY = -10f;

	[HideInInspector]
	[Tooltip("When using a depth camera (e.g. ZED), whether to use the depth in virtual green screen culling.")]
	public bool virtualGreenScreenApplyDepthCulling;

	[HideInInspector]
	[Tooltip("The tolerance value (in meter) when using the virtual green screen with a depth camera. Make it bigger if the foreground objects got culled incorrectly.")]
	public float virtualGreenScreenDepthTolerance = 0.2f;

	[Header("Tracking")]
	[SerializeField]
	[Tooltip("Defines the current tracking origin type.")]
	private TrackingOrigin _trackingOriginType;

	[Tooltip("If true, head tracking will affect the position of each OVRCameraRig's cameras.")]
	public bool usePositionTracking = true;

	[HideInInspector]
	public bool useRotationTracking = true;

	[Tooltip("If true, the distance between the user's eyes will affect the position of each OVRCameraRig's cameras.")]
	public bool useIPDInPositionTracking = true;

	[Tooltip("If true, each scene load will cause the head pose to reset.")]
	public bool resetTrackerOnLoad;

	[Tooltip("If true, the Reset View in the universal menu will cause the pose to be reset. This should generally be enabled for applications with a stationary position in the virtual world and will allow the View Reset command to place the person back to a predefined location (such as a cockpit seat). Set this to false if you have a locomotion system because resetting the view would effectively teleport the player to potentially invalid locations.")]
	public bool AllowRecenter = true;

	private static bool _isUserPresentCached = false;

	private static bool _isUserPresent = false;

	private static bool _wasUserPresent = false;

	private static bool prevAudioOutIdIsCached = false;

	private static bool prevAudioInIdIsCached = false;

	private static string prevAudioOutId = string.Empty;

	private static string prevAudioInId = string.Empty;

	private static bool wasPositionTracked = false;

	private static bool prevEnableMixedReality = false;

	private bool suppressDisableMixedRealityBecauseOfNoMainCameraWarning;

	private bool multipleMainCameraWarningPresented;

	public static OVRManager instance { get; private set; }

	public static OVRDisplay display { get; private set; }

	public static OVRTracker tracker { get; private set; }

	public static OVRBoundary boundary { get; private set; }

	public static OVRProfile profile
	{
		get
		{
			if (_profile == null)
			{
				_profile = new OVRProfile();
			}
			return _profile;
		}
	}

	public static bool isHmdPresent
	{
		get
		{
			if (!_isHmdPresentCached)
			{
				_isHmdPresentCached = true;
				_isHmdPresent = OVRPlugin.hmdPresent;
			}
			return _isHmdPresent;
		}
		private set
		{
			_isHmdPresentCached = true;
			_isHmdPresent = value;
		}
	}

	public static string audioOutId => OVRPlugin.audioOutId;

	public static string audioInId => OVRPlugin.audioInId;

	public static bool hasVrFocus
	{
		get
		{
			if (!_hasVrFocusCached)
			{
				_hasVrFocusCached = true;
				_hasVrFocus = OVRPlugin.hasVrFocus;
			}
			return _hasVrFocus;
		}
		private set
		{
			_hasVrFocusCached = true;
			_hasVrFocus = value;
		}
	}

	public static bool hasInputFocus => OVRPlugin.hasInputFocus;

	public bool chromatic
	{
		get
		{
			if (!isHmdPresent)
			{
				return false;
			}
			return OVRPlugin.chromatic;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.chromatic = value;
			}
		}
	}

	public bool monoscopic
	{
		get
		{
			if (!isHmdPresent)
			{
				return true;
			}
			return OVRPlugin.monoscopic;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.monoscopic = value;
			}
		}
	}

	public int vsyncCount
	{
		get
		{
			if (!isHmdPresent)
			{
				return 1;
			}
			return OVRPlugin.vsyncCount;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.vsyncCount = value;
			}
		}
	}

	public static float batteryLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 1f;
			}
			return OVRPlugin.batteryLevel;
		}
	}

	public static float batteryTemperature
	{
		get
		{
			if (!isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.batteryTemperature;
		}
	}

	public static int batteryStatus
	{
		get
		{
			if (!isHmdPresent)
			{
				return -1;
			}
			return (int)OVRPlugin.batteryStatus;
		}
	}

	public static float volumeLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.systemVolume;
		}
	}

	public static int cpuLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.cpuLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.cpuLevel = value;
			}
		}
	}

	public static int gpuLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.gpuLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.gpuLevel = value;
			}
		}
	}

	public static bool isPowerSavingActive
	{
		get
		{
			if (!isHmdPresent)
			{
				return false;
			}
			return OVRPlugin.powerSaving;
		}
	}

	public static EyeTextureFormat eyeTextureFormat
	{
		get
		{
			return (EyeTextureFormat)OVRPlugin.GetDesiredEyeTextureFormat();
		}
		set
		{
			OVRPlugin.SetDesiredEyeTextureFormat((OVRPlugin.EyeTextureFormat)value);
		}
	}

	public static bool tiledMultiResSupported => OVRPlugin.tiledMultiResSupported;

	public static TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			if (!OVRPlugin.tiledMultiResSupported)
			{
				Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
			}
			return (TiledMultiResLevel)OVRPlugin.tiledMultiResLevel;
		}
		set
		{
			if (!OVRPlugin.tiledMultiResSupported)
			{
				Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
			}
			OVRPlugin.tiledMultiResLevel = (OVRPlugin.TiledMultiResLevel)value;
		}
	}

	public static bool gpuUtilSupported => OVRPlugin.gpuUtilSupported;

	public static float gpuUtilLevel
	{
		get
		{
			if (!OVRPlugin.gpuUtilSupported)
			{
				Debug.LogWarning("GPU Util is not supported");
			}
			return OVRPlugin.gpuUtilLevel;
		}
	}

	public TrackingOrigin trackingOriginType
	{
		get
		{
			if (!isHmdPresent)
			{
				return _trackingOriginType;
			}
			return (TrackingOrigin)OVRPlugin.GetTrackingOriginType();
		}
		set
		{
			if (isHmdPresent && OVRPlugin.SetTrackingOriginType((OVRPlugin.TrackingOrigin)value))
			{
				_trackingOriginType = value;
			}
		}
	}

	public bool isSupportedPlatform { get; private set; }

	public bool isUserPresent
	{
		get
		{
			if (!_isUserPresentCached)
			{
				_isUserPresentCached = true;
				_isUserPresent = OVRPlugin.userPresent;
			}
			return _isUserPresent;
		}
		private set
		{
			_isUserPresentCached = true;
			_isUserPresent = value;
		}
	}

	public static Version utilitiesVersion => OVRPlugin.wrapperVersion;

	public static Version pluginVersion => OVRPlugin.version;

	public static Version sdkVersion => OVRPlugin.nativeSDKVersion;

	public static event Action HMDAcquired;

	public static event Action HMDLost;

	public static event Action HMDMounted;

	public static event Action HMDUnmounted;

	public static event Action VrFocusAcquired;

	public static event Action VrFocusLost;

	public static event Action InputFocusAcquired;

	public static event Action InputFocusLost;

	public static event Action AudioOutChanged;

	public static event Action AudioInChanged;

	public static event Action TrackingAcquired;

	public static event Action TrackingLost;

	[Obsolete]
	public static event Action HSWDismissed;

	public static bool IsAdaptiveResSupportedByEngine()
	{
		return Application.unityVersion != "2017.1.0f1";
	}

	private static bool MixedRealityEnabledFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-mixedreality")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseDirectCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-directcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseExternalCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-externalcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool CreateMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-create_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	private static bool LoadMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-load_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		if (instance != null)
		{
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		instance = this;
		Debug.Log(string.Concat("Unity v", Application.unityVersion, ", Oculus Utilities v", OVRPlugin.wrapperVersion, ", OVRPlugin v", OVRPlugin.version, ", SDK v", OVRPlugin.nativeSDKVersion, "."));
		string text = GraphicsDeviceType.Direct3D11.ToString() + ", " + GraphicsDeviceType.Direct3D12;
		if (!text.Contains(SystemInfo.graphicsDeviceType.ToString()))
		{
			Debug.LogWarning("VR rendering requires one of the following device types: (" + text + "). Your graphics device: " + SystemInfo.graphicsDeviceType);
		}
		RuntimePlatform platform = Application.platform;
		isSupportedPlatform |= platform == RuntimePlatform.Android;
		isSupportedPlatform |= platform == RuntimePlatform.OSXEditor;
		isSupportedPlatform |= platform == RuntimePlatform.OSXPlayer;
		isSupportedPlatform |= platform == RuntimePlatform.WindowsEditor;
		isSupportedPlatform |= platform == RuntimePlatform.WindowsPlayer;
		if (!isSupportedPlatform)
		{
			Debug.LogWarning("This platform is unsupported");
			return;
		}
		enableMixedReality = false;
		bool flag = LoadMixedRealityCaptureConfigurationFileFromCmd();
		bool flag2 = CreateMixedRealityCaptureConfigurationFileFromCmd();
		if (flag || flag2)
		{
			OVRMixedRealityCaptureSettings oVRMixedRealityCaptureSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
			oVRMixedRealityCaptureSettings.ReadFrom(this);
			if (flag)
			{
				oVRMixedRealityCaptureSettings.CombineWithConfigurationFile();
				oVRMixedRealityCaptureSettings.ApplyTo(this);
			}
			if (flag2)
			{
				oVRMixedRealityCaptureSettings.WriteToConfigurationFile();
			}
			UnityEngine.Object.Destroy(oVRMixedRealityCaptureSettings);
		}
		if (MixedRealityEnabledFromCmd())
		{
			enableMixedReality = true;
		}
		if (enableMixedReality)
		{
			Debug.Log("OVR: Mixed Reality mode enabled");
			if (UseDirectCompositionFromCmd())
			{
				compositionMethod = CompositionMethod.Direct;
			}
			if (UseExternalCompositionFromCmd())
			{
				compositionMethod = CompositionMethod.External;
			}
			Debug.Log("OVR: CompositionMethod : " + compositionMethod);
		}
		if (enableAdaptiveResolution && !IsAdaptiveResSupportedByEngine())
		{
			enableAdaptiveResolution = false;
			Debug.LogError("Your current Unity Engine " + Application.unityVersion + " might have issues to support adaptive resolution, please disable it under OVRManager");
		}
		Initialize();
		if (resetTrackerOnLoad)
		{
			display.RecenterPose();
		}
		OVRPlugin.occlusionMesh = true;
	}

	private void Initialize()
	{
		if (display == null)
		{
			display = new OVRDisplay();
		}
		if (tracker == null)
		{
			tracker = new OVRTracker();
		}
		if (boundary == null)
		{
			boundary = new OVRBoundary();
		}
	}

	private void Update()
	{
		if (OVRPlugin.shouldQuit)
		{
			Application.Quit();
		}
		if (AllowRecenter && OVRPlugin.shouldRecenter)
		{
			display.RecenterPose();
		}
		if (trackingOriginType != _trackingOriginType)
		{
			trackingOriginType = _trackingOriginType;
		}
		tracker.isEnabled = usePositionTracking;
		OVRPlugin.rotation = useRotationTracking;
		OVRPlugin.useIPDInPositionTracking = useIPDInPositionTracking;
		isHmdPresent = OVRPlugin.hmdPresent;
		if (useRecommendedMSAALevel && QualitySettings.antiAliasing != display.recommendedMSAALevel)
		{
			Debug.Log("The current MSAA level is " + QualitySettings.antiAliasing + ", but the recommended MSAA level is " + display.recommendedMSAALevel + ". Switching to the recommended level.");
			QualitySettings.antiAliasing = display.recommendedMSAALevel;
		}
		if (_wasHmdPresent && !isHmdPresent)
		{
			try
			{
				if (OVRManager.HMDLost != null)
				{
					OVRManager.HMDLost();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Caught Exception: " + ex);
			}
		}
		if (!_wasHmdPresent && isHmdPresent)
		{
			try
			{
				if (OVRManager.HMDAcquired != null)
				{
					OVRManager.HMDAcquired();
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("Caught Exception: " + ex2);
			}
		}
		_wasHmdPresent = isHmdPresent;
		isUserPresent = OVRPlugin.userPresent;
		if (_wasUserPresent && !isUserPresent)
		{
			try
			{
				if (OVRManager.HMDUnmounted != null)
				{
					OVRManager.HMDUnmounted();
				}
			}
			catch (Exception ex3)
			{
				Debug.LogError("Caught Exception: " + ex3);
			}
		}
		if (!_wasUserPresent && isUserPresent)
		{
			try
			{
				if (OVRManager.HMDMounted != null)
				{
					OVRManager.HMDMounted();
				}
			}
			catch (Exception ex4)
			{
				Debug.LogError("Caught Exception: " + ex4);
			}
		}
		_wasUserPresent = isUserPresent;
		hasVrFocus = OVRPlugin.hasVrFocus;
		if (_hadVrFocus && !hasVrFocus)
		{
			try
			{
				if (OVRManager.VrFocusLost != null)
				{
					OVRManager.VrFocusLost();
				}
			}
			catch (Exception ex5)
			{
				Debug.LogError("Caught Exception: " + ex5);
			}
		}
		if (!_hadVrFocus && hasVrFocus)
		{
			try
			{
				if (OVRManager.VrFocusAcquired != null)
				{
					OVRManager.VrFocusAcquired();
				}
			}
			catch (Exception ex6)
			{
				Debug.LogError("Caught Exception: " + ex6);
			}
		}
		_hadVrFocus = hasVrFocus;
		bool flag = OVRPlugin.hasInputFocus;
		if (_hadInputFocus && !flag)
		{
			try
			{
				if (OVRManager.InputFocusLost != null)
				{
					OVRManager.InputFocusLost();
				}
			}
			catch (Exception ex7)
			{
				Debug.LogError("Caught Exception: " + ex7);
			}
		}
		if (!_hadInputFocus && flag)
		{
			try
			{
				if (OVRManager.InputFocusAcquired != null)
				{
					OVRManager.InputFocusAcquired();
				}
			}
			catch (Exception ex8)
			{
				Debug.LogError("Caught Exception: " + ex8);
			}
		}
		_hadInputFocus = flag;
		if (enableAdaptiveResolution)
		{
			if (XRSettings.eyeTextureResolutionScale < maxRenderScale)
			{
				XRSettings.eyeTextureResolutionScale = maxRenderScale;
			}
			else
			{
				maxRenderScale = Mathf.Max(maxRenderScale, XRSettings.eyeTextureResolutionScale);
			}
			minRenderScale = Mathf.Min(minRenderScale, maxRenderScale);
			float min = minRenderScale / XRSettings.eyeTextureResolutionScale;
			float value = OVRPlugin.GetEyeRecommendedResolutionScale() / XRSettings.eyeTextureResolutionScale;
			value = Mathf.Clamp(value, min, 1f);
			XRSettings.renderViewportScale = value;
		}
		string text = OVRPlugin.audioOutId;
		if (!prevAudioOutIdIsCached)
		{
			prevAudioOutId = text;
			prevAudioOutIdIsCached = true;
		}
		else if (text != prevAudioOutId)
		{
			try
			{
				if (OVRManager.AudioOutChanged != null)
				{
					OVRManager.AudioOutChanged();
				}
			}
			catch (Exception ex9)
			{
				Debug.LogError("Caught Exception: " + ex9);
			}
			prevAudioOutId = text;
		}
		string text2 = OVRPlugin.audioInId;
		if (!prevAudioInIdIsCached)
		{
			prevAudioInId = text2;
			prevAudioInIdIsCached = true;
		}
		else if (text2 != prevAudioInId)
		{
			try
			{
				if (OVRManager.AudioInChanged != null)
				{
					OVRManager.AudioInChanged();
				}
			}
			catch (Exception ex10)
			{
				Debug.LogError("Caught Exception: " + ex10);
			}
			prevAudioInId = text2;
		}
		if (wasPositionTracked && !tracker.isPositionTracked)
		{
			try
			{
				if (OVRManager.TrackingLost != null)
				{
					OVRManager.TrackingLost();
				}
			}
			catch (Exception ex11)
			{
				Debug.LogError("Caught Exception: " + ex11);
			}
		}
		if (!wasPositionTracked && tracker.isPositionTracked)
		{
			try
			{
				if (OVRManager.TrackingAcquired != null)
				{
					OVRManager.TrackingAcquired();
				}
			}
			catch (Exception ex12)
			{
				Debug.LogError("Caught Exception: " + ex12);
			}
		}
		wasPositionTracked = tracker.isPositionTracked;
		display.Update();
		OVRInput.Update();
		if (!enableMixedReality && !prevEnableMixedReality)
		{
			return;
		}
		Camera mainCamera = FindMainCamera();
		if (Camera.main != null)
		{
			suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;
			if (enableMixedReality)
			{
				OVRMixedReality.Update(base.gameObject, mainCamera, compositionMethod, useDynamicLighting, capturingCameraDevice, depthQuality);
			}
			if (prevEnableMixedReality && !enableMixedReality)
			{
				OVRMixedReality.Cleanup();
			}
			prevEnableMixedReality = enableMixedReality;
		}
		else if (!suppressDisableMixedRealityBecauseOfNoMainCameraWarning)
		{
			Debug.LogWarning("Main Camera is not set, Mixed Reality disabled");
			suppressDisableMixedRealityBecauseOfNoMainCameraWarning = true;
		}
	}

	private Camera FindMainCamera()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
		List<Camera> list = new List<Camera>(4);
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			Camera component = gameObject.GetComponent<Camera>();
			if (component != null && component.enabled)
			{
				OVRCameraRig componentInParent = component.GetComponentInParent<OVRCameraRig>();
				if (componentInParent != null && componentInParent.trackingSpace != null)
				{
					list.Add(component);
				}
			}
		}
		if (list.Count == 0)
		{
			return Camera.main;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		if (!multipleMainCameraWarningPresented)
		{
			Debug.LogWarning("Multiple MainCamera found. Assume the real MainCamera is the camera with the least depth");
			multipleMainCameraWarningPresented = true;
		}
		list.Sort((Camera c0, Camera c1) => (c0.depth < c1.depth) ? (-1) : ((c0.depth > c1.depth) ? 1 : 0));
		return list[0];
	}

	private void OnDisable()
	{
		OVRMixedReality.Cleanup();
	}

	private void LateUpdate()
	{
		OVRHaptics.Process();
	}

	private void FixedUpdate()
	{
		OVRInput.FixedUpdate();
	}

	public void ReturnToLauncher()
	{
		PlatformUIConfirmQuit();
	}

	public static void PlatformUIConfirmQuit()
	{
		if (isHmdPresent)
		{
			OVRPlugin.ShowUI(OVRPlugin.PlatformUI.ConfirmQuit);
		}
	}
}
