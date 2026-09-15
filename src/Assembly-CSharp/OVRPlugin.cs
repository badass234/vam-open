using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class OVRPlugin
{
	[StructLayout(LayoutKind.Sequential)]
	private class GUID
	{
		public int a;

		public short b;

		public short c;

		public byte d0;

		public byte d1;

		public byte d2;

		public byte d3;

		public byte d4;

		public byte d5;

		public byte d6;

		public byte d7;
	}

	public enum Bool
	{
		False,
		True
	}

	public enum Result
	{
		Success = 0,
		Failure = -1000,
		Failure_InvalidParameter = -1001,
		Failure_NotInitialized = -1002,
		Failure_InvalidOperation = -1003,
		Failure_Unsupported = -1004,
		Failure_NotYetImplemented = -1005,
		Failure_OperationFailed = -1006,
		Failure_InsufficientSize = -1007
	}

	public enum CameraStatus
	{
		CameraStatus_None = 0,
		CameraStatus_Connected = 1,
		CameraStatus_Calibrating = 2,
		CameraStatus_CalibrationFailed = 3,
		CameraStatus_Calibrated = 4,
		CameraStatus_EnumSize = int.MaxValue
	}

	public enum Eye
	{
		None = -1,
		Left,
		Right,
		Count
	}

	public enum Tracker
	{
		None = -1,
		Zero,
		One,
		Two,
		Three,
		Count
	}

	public enum Node
	{
		None = -1,
		EyeLeft,
		EyeRight,
		EyeCenter,
		HandLeft,
		HandRight,
		TrackerZero,
		TrackerOne,
		TrackerTwo,
		TrackerThree,
		Head,
		DeviceObjectZero,
		Count
	}

	public enum Controller
	{
		None = 0,
		LTouch = 1,
		RTouch = 2,
		Touch = 3,
		Remote = 4,
		Gamepad = 16,
		Touchpad = 134217728,
		LTrackedRemote = 16777216,
		RTrackedRemote = 33554432,
		Active = int.MinValue,
		All = -1
	}

	public enum TrackingOrigin
	{
		EyeLevel,
		FloorLevel,
		Count
	}

	public enum RecenterFlags
	{
		Default = 0,
		Controllers = 1073741824,
		IgnoreAll = int.MinValue,
		Count = -2147483647
	}

	public enum BatteryStatus
	{
		Charging,
		Discharging,
		Full,
		NotCharging,
		Unknown
	}

	public enum EyeTextureFormat
	{
		Default = 0,
		R8G8B8A8_sRGB = 0,
		R8G8B8A8 = 1,
		R16G16B16A16_FP = 2,
		R11G11B10_FP = 3,
		B8G8R8A8_sRGB = 4,
		B8G8R8A8 = 5,
		R5G6B5 = 11,
		EnumSize = int.MaxValue
	}

	public enum PlatformUI
	{
		None = -1,
		ConfirmQuit = 1,
		GlobalMenuTutorial = 2
	}

	public enum SystemRegion
	{
		Unspecified,
		Japan,
		China
	}

	public enum SystemHeadset
	{
		None = 0,
		GearVR_R320 = 1,
		GearVR_R321 = 2,
		GearVR_R322 = 3,
		GearVR_R323 = 4,
		GearVR_R324 = 5,
		GearVR_R325 = 6,
		Oculus_Go = 7,
		Rift_DK1 = 4096,
		Rift_DK2 = 4097,
		Rift_CV1 = 4098
	}

	public enum OverlayShape
	{
		Quad = 0,
		Cylinder = 1,
		Cubemap = 2,
		OffcenterCubemap = 4,
		Equirect = 5
	}

	public enum Step
	{
		Render = -1,
		Physics
	}

	public enum CameraDevice
	{
		None = 0,
		WebCamera0 = 100,
		WebCamera1 = 101,
		ZEDCamera = 300
	}

	public enum CameraDeviceDepthSensingMode
	{
		Standard,
		Fill
	}

	public enum CameraDeviceDepthQuality
	{
		Low,
		Medium,
		High
	}

	public enum TiledMultiResLevel
	{
		Off = 0,
		LMSLow = 1,
		LMSMedium = 2,
		LMSHigh = 3,
		EnumSize = int.MaxValue
	}

	public struct CameraDeviceIntrinsicsParameters
	{
		private float fx;

		private float fy;

		private float cx;

		private float cy;

		private double disto0;

		private double disto1;

		private double disto2;

		private double disto3;

		private double disto4;

		private float v_fov;

		private float h_fov;

		private float d_fov;

		private int w;

		private int h;
	}

	private enum OverlayFlag
	{
		None = 0,
		OnTop = 1,
		HeadLocked = 2,
		ShapeFlag_Quad = 0,
		ShapeFlag_Cylinder = 16,
		ShapeFlag_Cubemap = 32,
		ShapeFlag_OffcenterCubemap = 64,
		ShapeFlagRangeMask = 240
	}

	public struct Vector2f
	{
		public float x;

		public float y;
	}

	public struct Vector3f
	{
		public float x;

		public float y;

		public float z;

		public override string ToString()
		{
			return $"{x}, {y}, {z}";
		}
	}

	public struct Quatf
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public override string ToString()
		{
			return $"{x}, {y}, {z}, {w}";
		}
	}

	public struct Posef
	{
		public Quatf Orientation;

		public Vector3f Position;

		public override string ToString()
		{
			return $"Position ({Position}), Orientation({Orientation})";
		}
	}

	public struct PoseStatef
	{
		public Posef Pose;

		public Vector3f Velocity;

		public Vector3f Acceleration;

		public Vector3f AngularVelocity;

		public Vector3f AngularAcceleration;

		private double Time;
	}

	public struct ControllerState4
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		public byte LBatteryPercentRemaining;

		public byte RBatteryPercentRemaining;

		public byte LRecenterCount;

		public byte RRecenterCount;

		public byte Reserved_27;

		public byte Reserved_26;

		public byte Reserved_25;

		public byte Reserved_24;

		public byte Reserved_23;

		public byte Reserved_22;

		public byte Reserved_21;

		public byte Reserved_20;

		public byte Reserved_19;

		public byte Reserved_18;

		public byte Reserved_17;

		public byte Reserved_16;

		public byte Reserved_15;

		public byte Reserved_14;

		public byte Reserved_13;

		public byte Reserved_12;

		public byte Reserved_11;

		public byte Reserved_10;

		public byte Reserved_09;

		public byte Reserved_08;

		public byte Reserved_07;

		public byte Reserved_06;

		public byte Reserved_05;

		public byte Reserved_04;

		public byte Reserved_03;

		public byte Reserved_02;

		public byte Reserved_01;

		public byte Reserved_00;

		public ControllerState4(ControllerState2 cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = cs.LTouchpad;
			RTouchpad = cs.RTouchpad;
			LBatteryPercentRemaining = 0;
			RBatteryPercentRemaining = 0;
			LRecenterCount = 0;
			RRecenterCount = 0;
			Reserved_27 = 0;
			Reserved_26 = 0;
			Reserved_25 = 0;
			Reserved_24 = 0;
			Reserved_23 = 0;
			Reserved_22 = 0;
			Reserved_21 = 0;
			Reserved_20 = 0;
			Reserved_19 = 0;
			Reserved_18 = 0;
			Reserved_17 = 0;
			Reserved_16 = 0;
			Reserved_15 = 0;
			Reserved_14 = 0;
			Reserved_13 = 0;
			Reserved_12 = 0;
			Reserved_11 = 0;
			Reserved_10 = 0;
			Reserved_09 = 0;
			Reserved_08 = 0;
			Reserved_07 = 0;
			Reserved_06 = 0;
			Reserved_05 = 0;
			Reserved_04 = 0;
			Reserved_03 = 0;
			Reserved_02 = 0;
			Reserved_01 = 0;
			Reserved_00 = 0;
		}
	}

	public struct ControllerState2
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		public ControllerState2(ControllerState cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = new Vector2f
			{
				x = 0f,
				y = 0f
			};
			RTouchpad = new Vector2f
			{
				x = 0f,
				y = 0f
			};
		}
	}

	public struct ControllerState
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;
	}

	public struct HapticsBuffer
	{
		public IntPtr Samples;

		public int SamplesCount;
	}

	public struct HapticsState
	{
		public int SamplesAvailable;

		public int SamplesQueued;
	}

	public struct HapticsDesc
	{
		public int SampleRateHz;

		public int SampleSizeInBytes;

		public int MinimumSafeSamplesQueued;

		public int MinimumBufferSamplesCount;

		public int OptimalBufferSamplesCount;

		public int MaximumBufferSamplesCount;
	}

	public struct AppPerfFrameStats
	{
		public int HmdVsyncIndex;

		public int AppFrameIndex;

		public int AppDroppedFrameCount;

		public float AppMotionToPhotonLatency;

		public float AppQueueAheadTime;

		public float AppCpuElapsedTime;

		public float AppGpuElapsedTime;

		public int CompositorFrameIndex;

		public int CompositorDroppedFrameCount;

		public float CompositorLatency;

		public float CompositorCpuElapsedTime;

		public float CompositorGpuElapsedTime;

		public float CompositorCpuStartToGpuEndElapsedTime;

		public float CompositorGpuEndToVsyncElapsedTime;
	}

	public struct AppPerfStats
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public AppPerfFrameStats[] FrameStats;

		public int FrameStatsCount;

		public Bool AnyFrameStatsDropped;

		public float AdaptiveGpuPerformanceScale;
	}

	public struct Sizei
	{
		public int w;

		public int h;
	}

	public struct Sizef
	{
		public float w;

		public float h;
	}

	public struct Vector2i
	{
		public int x;

		public int y;
	}

	public struct Recti
	{
		private Vector2i Pos;

		private Sizei Size;
	}

	public struct Rectf
	{
		private Vector2f Pos;

		private Sizef Size;
	}

	public struct Frustumf
	{
		public float zNear;

		public float zFar;

		public float fovX;

		public float fovY;
	}

	public enum BoundaryType
	{
		OuterBoundary = 1,
		PlayArea = 0x100
	}

	public struct BoundaryTestResult
	{
		public Bool IsTriggering;

		public float ClosestDistance;

		public Vector3f ClosestPoint;

		public Vector3f ClosestPointNormal;
	}

	public struct BoundaryLookAndFeel
	{
		public Colorf Color;
	}

	public struct BoundaryGeometry
	{
		public BoundaryType BoundaryType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public Vector3f[] Points;

		public int PointsCount;
	}

	public struct Colorf
	{
		public float r;

		public float g;

		public float b;

		public float a;
	}

	public struct Fovf
	{
		public float UpTan;

		public float DownTan;

		public float LeftTan;

		public float RightTan;
	}

	public struct CameraIntrinsics
	{
		public bool IsValid;

		public double LastChangedTimeSeconds;

		public Fovf FOVPort;

		public float VirtualNearPlaneDistanceMeters;

		public float VirtualFarPlaneDistanceMeters;

		public Sizei ImageSensorPixelResolution;
	}

	public struct CameraExtrinsics
	{
		public bool IsValid;

		public double LastChangedTimeSeconds;

		public CameraStatus CameraStatusData;

		public Node AttachedToNode;

		public Posef RelativePose;
	}

	public enum LayerLayout
	{
		Stereo = 0,
		Mono = 1,
		DoubleWide = 2,
		Array = 3,
		EnumSize = 15
	}

	public enum LayerFlags
	{
		Static = 1,
		LoadingScreen = 2,
		SymmetricFov = 4,
		TextureOriginAtBottomLeft = 8,
		ChromaticAberrationCorrection = 0x10,
		NoAllocation = 0x20,
		ProtectedContent = 0x40
	}

	public struct LayerDesc
	{
		public OverlayShape Shape;

		public LayerLayout Layout;

		public Sizei TextureSize;

		public int MipLevels;

		public int SampleCount;

		public EyeTextureFormat Format;

		public int LayerFlags;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Fovf[] Fov;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Rectf[] VisibleRect;

		public Sizei MaxViewportSize;

		private EyeTextureFormat DepthFormat;

		public override string ToString()
		{
			string text = ", ";
			return Shape.ToString() + text + Layout.ToString() + text + TextureSize.w + "x" + TextureSize.h + text + MipLevels + text + SampleCount + text + Format.ToString() + text + LayerFlags;
		}
	}

	public struct LayerSubmit
	{
		private int LayerId;

		private int TextureStage;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private Recti[] ViewportRect;

		private Posef Pose;

		private int LayerSubmitFlags;
	}

	private static class OVRP_0_1_0
	{
		public static readonly Version version = new Version(0, 1, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Sizei ovrp_GetEyeTextureSize(Eye eyeId);
	}

	private static class OVRP_0_1_1
	{
		public static readonly Version version = new Version(0, 1, 1);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetOverlayQuad2(Bool onTop, Bool headLocked, IntPtr texture, IntPtr device, Posef pose, Vector3f scale);
	}

	private static class OVRP_0_1_2
	{
		public static readonly Version version = new Version(0, 1, 2);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodePose(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);
	}

	private static class OVRP_0_1_3
	{
		public static readonly Version version = new Version(0, 1, 3);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeVelocity(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeAcceleration(Node nodeId);
	}

	private static class OVRP_0_5_0
	{
		public static readonly Version version = new Version(0, 5, 0);
	}

	private static class OVRP_1_0_0
	{
		public static readonly Version version = new Version(1, 0, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern TrackingOrigin ovrp_GetTrackingOriginType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingOriginType(TrackingOrigin originType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetTrackingCalibratedOrigin();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_RecenterTrackingOrigin(uint flags);
	}

	private static class OVRP_1_1_0
	{
		public static readonly Version version = new Version(1, 1, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")]
		private static extern IntPtr _ovrp_GetVersion();

		public static string ovrp_GetVersion()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetNativeSDKVersion")]
		private static extern IntPtr _ovrp_GetNativeSDKVersion();

		public static string ovrp_GetNativeSDKVersion()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetNativeSDKVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioOutId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioInId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeTextureScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetEyeTextureScale(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingOrientationSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingOrientationEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingOrientationEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingPositionSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingPositionEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingPositionEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodePresent(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodeOrientationTracked(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodePositionTracked(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Frustumf ovrp_GetNodeFrustum(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern ControllerState ovrp_GetControllerState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemCpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetSystemCpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemGpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetSystemGpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetSystemPowerSavingMode();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemDisplayFrequency();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemVSyncCount();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemVolume();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BatteryStatus ovrp_GetSystemBatteryStatus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryTemperature();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetSystemProductName")]
		private static extern IntPtr _ovrp_GetSystemProductName();

		public static string ovrp_GetSystemProductName()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetSystemProductName());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ShowSystemUI(PlatformUI ui);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppMonoscopic();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetAppMonoscopic(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppHasVrFocus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppShouldQuit();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppShouldRecenter();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetAppLatencyTimings")]
		private static extern IntPtr _ovrp_GetAppLatencyTimings();

		public static string ovrp_GetAppLatencyTimings()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetAppLatencyTimings());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetUserPresent();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserIPD();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserIPD(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeDepth();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserEyeDepth(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeHeight();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserEyeHeight(float value);
	}

	private static class OVRP_1_2_0
	{
		public static readonly Version version = new Version(1, 2, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetSystemVSyncCount(int vsyncCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrpi_SetTrackingCalibratedOrigin();
	}

	private static class OVRP_1_3_0
	{
		public static readonly Version version = new Version(1, 3, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetEyeOcclusionMeshEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetEyeOcclusionMeshEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetSystemHeadphonesPresent();
	}

	private static class OVRP_1_5_0
	{
		public static readonly Version version = new Version(1, 5, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemRegion ovrp_GetSystemRegion();
	}

	private static class OVRP_1_6_0
	{
		public static readonly Version version = new Version(1, 6, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingIPDEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingIPDEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern HapticsState ovrp_GetControllerHapticsState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetOverlayQuad3(uint flags, IntPtr textureLeft, IntPtr textureRight, IntPtr device, Posef pose, Vector3f scale, int layerIndex);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeRecommendedResolutionScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppCpuStartToGpuEndTime();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemRecommendedMSAALevel();
	}

	private static class OVRP_1_7_0
	{
		public static readonly Version version = new Version(1, 7, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppChromaticCorrection();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetAppChromaticCorrection(Bool value);
	}

	private static class OVRP_1_8_0
	{
		public static readonly Version version = new Version(1, 8, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetBoundaryConfigured();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BoundaryTestResult ovrp_TestBoundaryNode(Node nodeId, BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BoundaryTestResult ovrp_TestBoundaryPoint(Vector3f point, BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetBoundaryLookAndFeel(BoundaryLookAndFeel lookAndFeel);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ResetBoundaryLookAndFeel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BoundaryGeometry ovrp_GetBoundaryGeometry(BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector3f ovrp_GetBoundaryDimensions(BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetBoundaryVisible();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetBoundaryVisible(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodePose2(int stateId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeVelocity2(int stateId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeAcceleration2(int stateId, Node nodeId);
	}

	private static class OVRP_1_9_0
	{
		public static readonly Version version = new Version(1, 9, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemHeadset ovrp_GetSystemHeadsetType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Controller ovrp_GetActiveController();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Controller ovrp_GetConnectedControllers();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern AppPerfStats ovrp_GetAppPerfStats();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ResetAppPerfStats();
	}

	private static class OVRP_1_10_0
	{
		public static readonly Version version = new Version(1, 10, 0);
	}

	private static class OVRP_1_11_0
	{
		public static readonly Version version = new Version(1, 11, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetDesiredEyeTextureFormat(EyeTextureFormat value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern EyeTextureFormat ovrp_GetDesiredEyeTextureFormat();
	}

	private static class OVRP_1_12_0
	{
		public static readonly Version version = new Version(1, 12, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppFramerate();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern PoseStatef ovrp_GetNodePoseState(Step stepId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern ControllerState2 ovrp_GetControllerState2(uint controllerMask);
	}

	private static class OVRP_1_15_0
	{
		public const int OVRP_EXTERNAL_CAMERA_NAME_SIZE = 32;

		public static readonly Version version = new Version(1, 15, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_InitializeMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ShutdownMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetMixedRealityInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdateExternalCamera();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraCount(out int cameraCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraName(int cameraId, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] char[] cameraName);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraIntrinsics(int cameraId, out CameraIntrinsics cameraIntrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraExtrinsics(int cameraId, out CameraExtrinsics cameraExtrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CalculateLayerDesc(OverlayShape shape, LayerLayout layout, ref Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags, ref LayerDesc layerDesc);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSetupLayer(ref LayerDesc desc, IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueDestroyLayer(IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerTextureStageCount(int layerId, ref int layerTextureStageCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerTexturePtr(int layerId, int stage, Eye eyeId, ref IntPtr textureHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSubmitLayer(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref Posef pose, ref Vector3f scale, int layerIndex);
	}

	private static class OVRP_1_16_0
	{
		public static readonly Version version = new Version(1, 16, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdateCameraDevices();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_IsCameraDeviceAvailable(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, Sizei preferredColorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_OpenCameraDevice(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CloseCameraDevice(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_HasCameraDeviceOpened(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceColorFrameSize(CameraDevice cameraDevice, out Sizei colorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceColorFrameBgraPixels(CameraDevice cameraDevice, out IntPtr colorFrameBgraPixels, out int colorFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerState4(uint controllerMask, ref ControllerState4 controllerState);
	}

	private static class OVRP_1_17_0
	{
		public static readonly Version version = new Version(1, 17, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraPose(CameraDevice camera, out Posef cameraPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ConvertPoseToCameraSpace(CameraDevice camera, ref Posef trackingSpacePose, out Posef cameraSpacePose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceIntrinsicsParameters(CameraDevice camera, out Bool supportIntrinsics, out CameraDeviceIntrinsicsParameters intrinsicsParameters);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DoesCameraDeviceSupportDepth(CameraDevice camera, out Bool supportDepth);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthSensingMode(CameraDevice camera, out CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDevicePreferredDepthQuality(CameraDevice camera, out CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsCameraDeviceDepthFrameAvailable(CameraDevice camera, out Bool available);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthFrameSize(CameraDevice camera, out Sizei depthFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthFramePixels(CameraDevice cameraDevice, out IntPtr depthFramePixels, out int depthFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthConfidencePixels(CameraDevice cameraDevice, out IntPtr depthConfidencePixels, out int depthConfidenceRowPitch);
	}

	private static class OVRP_1_18_0
	{
		public static readonly Version version = new Version(1, 18, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetHandNodePoseStateLatency(double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandNodePoseStateLatency(out double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetAppHasInputFocus(out Bool appHasInputFocus);
	}

	private static class OVRP_1_19_0
	{
		public static readonly Version version = new Version(1, 19, 0);
	}

	private static class OVRP_1_21_0
	{
		public static readonly Version version = new Version(1, 21, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTiledMultiResSupported(out Bool foveationSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTiledMultiResLevel(out TiledMultiResLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetTiledMultiResLevel(TiledMultiResLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetGPUUtilSupported(out Bool gpuUtilSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetGPUUtilLevel(out float gpuUtil);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSystemDisplayFrequency2(out float systemDisplayFrequency);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSystemDisplayAvailableFrequencies(IntPtr systemDisplayAvailableFrequencies, out int numFrequencies);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSystemDisplayFrequency(float requestedFrequency);
	}

	private static class OVRP_1_25_1
	{
		public static readonly Version version = new Version(1, 25, 1);
	}

	public static readonly Version wrapperVersion = OVRP_1_25_1.version;

	private static Version _version;

	private static Version _nativeSDKVersion;

	private const int OverlayShapeFlagShift = 4;

	public const int AppPerfFrameStatsMaxCount = 5;

	private static GUID _nativeAudioOutGuid = new GUID();

	private static Guid _cachedAudioOutGuid;

	private static string _cachedAudioOutString;

	private static GUID _nativeAudioInGuid = new GUID();

	private static Guid _cachedAudioInGuid;

	private static string _cachedAudioInString;

	private static Texture2D cachedCameraFrameTexture = null;

	private static Texture2D cachedCameraDepthTexture = null;

	private static Texture2D cachedCameraDepthConfidenceTexture = null;

	private static OVRNativeBuffer _nativeSystemDisplayFrequenciesAvailable = null;

	private static float[] _cachedSystemDisplayFrequenciesAvailable = null;

	private const string pluginName = "OVRPlugin";

	private static Version _versionZero = new Version(0, 0, 0);

	public static Version version
	{
		get
		{
			if (_version == null)
			{
				try
				{
					string text = OVRP_1_1_0.ovrp_GetVersion();
					if (text != null)
					{
						text = text.Split('-')[0];
						_version = new Version(text);
					}
					else
					{
						_version = _versionZero;
					}
				}
				catch
				{
					_version = _versionZero;
				}
				if (_version == OVRP_0_5_0.version)
				{
					_version = OVRP_0_1_0.version;
				}
				if (_version > _versionZero && _version < OVRP_1_3_0.version)
				{
					throw new PlatformNotSupportedException(string.Concat("Oculus Utilities version ", wrapperVersion, " is too new for OVRPlugin version ", _version.ToString(), ". Update to the latest version of Unity."));
				}
			}
			return _version;
		}
	}

	public static Version nativeSDKVersion
	{
		get
		{
			if (_nativeSDKVersion == null)
			{
				try
				{
					string empty = string.Empty;
					empty = ((!(version >= OVRP_1_1_0.version)) ? _versionZero.ToString() : OVRP_1_1_0.ovrp_GetNativeSDKVersion());
					if (empty != null)
					{
						empty = empty.Split('-')[0];
						_nativeSDKVersion = new Version(empty);
					}
					else
					{
						_nativeSDKVersion = _versionZero;
					}
				}
				catch
				{
					_nativeSDKVersion = _versionZero;
				}
			}
			return _nativeSDKVersion;
		}
	}

	public static bool initialized => OVRP_1_1_0.ovrp_GetInitialized() == Bool.True;

	public static bool chromatic
	{
		get
		{
			if (version >= OVRP_1_7_0.version)
			{
				return OVRP_1_7_0.ovrp_GetAppChromaticCorrection() == Bool.True;
			}
			return true;
		}
		set
		{
			if (version >= OVRP_1_7_0.version)
			{
				OVRP_1_7_0.ovrp_SetAppChromaticCorrection(ToBool(value));
			}
		}
	}

	public static bool monoscopic
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetAppMonoscopic() == Bool.True;
		}
		set
		{
			OVRP_1_1_0.ovrp_SetAppMonoscopic(ToBool(value));
		}
	}

	public static bool rotation
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetTrackingOrientationEnabled() == Bool.True;
		}
		set
		{
			OVRP_1_1_0.ovrp_SetTrackingOrientationEnabled(ToBool(value));
		}
	}

	public static bool position
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetTrackingPositionEnabled() == Bool.True;
		}
		set
		{
			OVRP_1_1_0.ovrp_SetTrackingPositionEnabled(ToBool(value));
		}
	}

	public static bool useIPDInPositionTracking
	{
		get
		{
			if (version >= OVRP_1_6_0.version)
			{
				return OVRP_1_6_0.ovrp_GetTrackingIPDEnabled() == Bool.True;
			}
			return true;
		}
		set
		{
			if (version >= OVRP_1_6_0.version)
			{
				OVRP_1_6_0.ovrp_SetTrackingIPDEnabled(ToBool(value));
			}
		}
	}

	public static bool positionSupported => OVRP_1_1_0.ovrp_GetTrackingPositionSupported() == Bool.True;

	public static bool positionTracked => OVRP_1_1_0.ovrp_GetNodePositionTracked(Node.EyeCenter) == Bool.True;

	public static bool powerSaving => OVRP_1_1_0.ovrp_GetSystemPowerSavingMode() == Bool.True;

	public static bool hmdPresent => OVRP_1_1_0.ovrp_GetNodePresent(Node.EyeCenter) == Bool.True;

	public static bool userPresent => OVRP_1_1_0.ovrp_GetUserPresent() == Bool.True;

	public static bool headphonesPresent => OVRP_1_3_0.ovrp_GetSystemHeadphonesPresent() == Bool.True;

	public static int recommendedMSAALevel
	{
		get
		{
			if (version >= OVRP_1_6_0.version)
			{
				return OVRP_1_6_0.ovrp_GetSystemRecommendedMSAALevel();
			}
			return 2;
		}
	}

	public static SystemRegion systemRegion
	{
		get
		{
			if (version >= OVRP_1_5_0.version)
			{
				return OVRP_1_5_0.ovrp_GetSystemRegion();
			}
			return SystemRegion.Unspecified;
		}
	}

	public static string audioOutId
	{
		get
		{
			try
			{
				if (_nativeAudioOutGuid == null)
				{
					_nativeAudioOutGuid = new GUID();
				}
				IntPtr intPtr = OVRP_1_1_0.ovrp_GetAudioOutId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, _nativeAudioOutGuid);
					Guid guid = new Guid(_nativeAudioOutGuid.a, _nativeAudioOutGuid.b, _nativeAudioOutGuid.c, _nativeAudioOutGuid.d0, _nativeAudioOutGuid.d1, _nativeAudioOutGuid.d2, _nativeAudioOutGuid.d3, _nativeAudioOutGuid.d4, _nativeAudioOutGuid.d5, _nativeAudioOutGuid.d6, _nativeAudioOutGuid.d7);
					if (guid != _cachedAudioOutGuid)
					{
						_cachedAudioOutGuid = guid;
						_cachedAudioOutString = _cachedAudioOutGuid.ToString();
					}
					return _cachedAudioOutString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static string audioInId
	{
		get
		{
			try
			{
				if (_nativeAudioInGuid == null)
				{
					_nativeAudioInGuid = new GUID();
				}
				IntPtr intPtr = OVRP_1_1_0.ovrp_GetAudioInId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, _nativeAudioInGuid);
					Guid guid = new Guid(_nativeAudioInGuid.a, _nativeAudioInGuid.b, _nativeAudioInGuid.c, _nativeAudioInGuid.d0, _nativeAudioInGuid.d1, _nativeAudioInGuid.d2, _nativeAudioInGuid.d3, _nativeAudioInGuid.d4, _nativeAudioInGuid.d5, _nativeAudioInGuid.d6, _nativeAudioInGuid.d7);
					if (guid != _cachedAudioInGuid)
					{
						_cachedAudioInGuid = guid;
						_cachedAudioInString = _cachedAudioInGuid.ToString();
					}
					return _cachedAudioInString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static bool hasVrFocus => OVRP_1_1_0.ovrp_GetAppHasVrFocus() == Bool.True;

	public static bool hasInputFocus
	{
		get
		{
			if (version >= OVRP_1_18_0.version)
			{
				Bool appHasInputFocus = Bool.False;
				if (OVRP_1_18_0.ovrp_GetAppHasInputFocus(out appHasInputFocus) == Result.Success)
				{
					return appHasInputFocus == Bool.True;
				}
				return false;
			}
			return true;
		}
	}

	public static bool shouldQuit => OVRP_1_1_0.ovrp_GetAppShouldQuit() == Bool.True;

	public static bool shouldRecenter => OVRP_1_1_0.ovrp_GetAppShouldRecenter() == Bool.True;

	public static string productName => OVRP_1_1_0.ovrp_GetSystemProductName();

	public static string latency => OVRP_1_1_0.ovrp_GetAppLatencyTimings();

	public static float eyeDepth
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetUserEyeDepth();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserEyeDepth(value);
		}
	}

	public static float eyeHeight
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetUserEyeHeight();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserEyeHeight(value);
		}
	}

	public static float batteryLevel => OVRP_1_1_0.ovrp_GetSystemBatteryLevel();

	public static float batteryTemperature => OVRP_1_1_0.ovrp_GetSystemBatteryTemperature();

	public static int cpuLevel
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemCpuLevel();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetSystemCpuLevel(value);
		}
	}

	public static int gpuLevel
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemGpuLevel();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetSystemGpuLevel(value);
		}
	}

	public static int vsyncCount
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemVSyncCount();
		}
		set
		{
			OVRP_1_2_0.ovrp_SetSystemVSyncCount(value);
		}
	}

	public static float systemVolume => OVRP_1_1_0.ovrp_GetSystemVolume();

	public static float ipd
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetUserIPD();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserIPD(value);
		}
	}

	public static bool occlusionMesh
	{
		get
		{
			return OVRP_1_3_0.ovrp_GetEyeOcclusionMeshEnabled() == Bool.True;
		}
		set
		{
			OVRP_1_3_0.ovrp_SetEyeOcclusionMeshEnabled(ToBool(value));
		}
	}

	public static BatteryStatus batteryStatus => OVRP_1_1_0.ovrp_GetSystemBatteryStatus();

	public static bool tiledMultiResSupported
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				Bool foveationSupported;
				if (OVRP_1_21_0.ovrp_GetTiledMultiResSupported(out foveationSupported) == Result.Success)
				{
					return foveationSupported == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	public static TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			if (version >= OVRP_1_21_0.version && tiledMultiResSupported)
			{
				TiledMultiResLevel level;
				if (OVRP_1_21_0.ovrp_GetTiledMultiResLevel(out level) != 0)
				{
				}
				return level;
			}
			return TiledMultiResLevel.Off;
		}
		set
		{
			if (version >= OVRP_1_21_0.version && tiledMultiResSupported && OVRP_1_21_0.ovrp_SetTiledMultiResLevel(value) == Result.Success)
			{
			}
		}
	}

	public static bool gpuUtilSupported
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				Bool @bool;
				if (OVRP_1_21_0.ovrp_GetGPUUtilSupported(out @bool) == Result.Success)
				{
					return @bool == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	public static float gpuUtilLevel
	{
		get
		{
			if (version >= OVRP_1_21_0.version && gpuUtilSupported)
			{
				float gpuUtil;
				if (OVRP_1_21_0.ovrp_GetGPUUtilLevel(out gpuUtil) == Result.Success)
				{
					return gpuUtil;
				}
				return 0f;
			}
			return 0f;
		}
	}

	public static float[] systemDisplayFrequenciesAvailable
	{
		get
		{
			if (_cachedSystemDisplayFrequenciesAvailable == null)
			{
				_cachedSystemDisplayFrequenciesAvailable = new float[0];
				if (version >= OVRP_1_21_0.version)
				{
					int numFrequencies = 0;
					if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(IntPtr.Zero, out numFrequencies) == Result.Success && numFrequencies > 0)
					{
						int num = numFrequencies;
						_nativeSystemDisplayFrequenciesAvailable = new OVRNativeBuffer(4 * num);
						if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), out numFrequencies) == Result.Success)
						{
							int num2 = ((numFrequencies > num) ? num : numFrequencies);
							if (num2 > 0)
							{
								_cachedSystemDisplayFrequenciesAvailable = new float[num2];
								Marshal.Copy(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), _cachedSystemDisplayFrequenciesAvailable, 0, num2);
							}
						}
					}
				}
			}
			return _cachedSystemDisplayFrequenciesAvailable;
		}
	}

	public static float systemDisplayFrequency
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				float result;
				if (OVRP_1_21_0.ovrp_GetSystemDisplayFrequency2(out result) == Result.Success)
				{
					return result;
				}
				return 0f;
			}
			if (version >= OVRP_1_1_0.version)
			{
				return OVRP_1_1_0.ovrp_GetSystemDisplayFrequency();
			}
			return 0f;
		}
		set
		{
			if (version >= OVRP_1_21_0.version)
			{
				OVRP_1_21_0.ovrp_SetSystemDisplayFrequency(value);
			}
		}
	}

	public static Frustumf GetEyeFrustum(Eye eyeId)
	{
		return OVRP_1_1_0.ovrp_GetNodeFrustum((Node)eyeId);
	}

	public static Sizei GetEyeTextureSize(Eye eyeId)
	{
		return OVRP_0_1_0.ovrp_GetEyeTextureSize(eyeId);
	}

	public static Posef GetTrackerPose(Tracker trackerId)
	{
		return GetNodePose((Node)(trackerId + 5), Step.Render);
	}

	public static Frustumf GetTrackerFrustum(Tracker trackerId)
	{
		return OVRP_1_1_0.ovrp_GetNodeFrustum((Node)(trackerId + 5));
	}

	public static bool ShowUI(PlatformUI ui)
	{
		return OVRP_1_1_0.ovrp_ShowSystemUI(ui) == Bool.True;
	}

	public static bool EnqueueSubmitLayer(bool onTop, bool headLocked, IntPtr leftTexture, IntPtr rightTexture, int layerId, int frameIndex, Posef pose, Vector3f scale, int layerIndex = 0, OverlayShape shape = OverlayShape.Quad)
	{
		if (version >= OVRP_1_6_0.version)
		{
			uint num = 0u;
			if (onTop)
			{
				num |= 1;
			}
			if (headLocked)
			{
				num |= 2;
			}
			if (shape == OverlayShape.Cylinder || shape == OverlayShape.Cubemap)
			{
				if (shape == OverlayShape.Cubemap && version >= OVRP_1_10_0.version)
				{
					num |= (uint)((int)shape << 4);
				}
				else
				{
					if (shape != OverlayShape.Cylinder || !(version >= OVRP_1_16_0.version))
					{
						return false;
					}
					num |= (uint)((int)shape << 4);
				}
			}
			switch (shape)
			{
			case OverlayShape.OffcenterCubemap:
				return false;
			case OverlayShape.Equirect:
				return false;
			default:
				if (version >= OVRP_1_15_0.version && layerId != -1)
				{
					return OVRP_1_15_0.ovrp_EnqueueSubmitLayer(num, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex) == Result.Success;
				}
				return OVRP_1_6_0.ovrp_SetOverlayQuad3(num, leftTexture, rightTexture, IntPtr.Zero, pose, scale, layerIndex) == Bool.True;
			}
		}
		if (layerIndex != 0)
		{
			return false;
		}
		return OVRP_0_1_1.ovrp_SetOverlayQuad2(ToBool(onTop), ToBool(headLocked), leftTexture, IntPtr.Zero, pose, scale) == Bool.True;
	}

	public static LayerDesc CalculateLayerDesc(OverlayShape shape, LayerLayout layout, Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags)
	{
		LayerDesc layerDesc = default(LayerDesc);
		if (version >= OVRP_1_15_0.version)
		{
			OVRP_1_15_0.ovrp_CalculateLayerDesc(shape, layout, ref textureSize, mipLevels, sampleCount, format, layerFlags, ref layerDesc);
		}
		return layerDesc;
	}

	public static bool EnqueueSetupLayer(LayerDesc desc, IntPtr layerID)
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_EnqueueSetupLayer(ref desc, layerID) == Result.Success;
		}
		return false;
	}

	public static bool EnqueueDestroyLayer(IntPtr layerID)
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_EnqueueDestroyLayer(layerID) == Result.Success;
		}
		return false;
	}

	public static IntPtr GetLayerTexture(int layerId, int stage, Eye eyeId)
	{
		IntPtr textureHandle = IntPtr.Zero;
		if (version >= OVRP_1_15_0.version)
		{
			OVRP_1_15_0.ovrp_GetLayerTexturePtr(layerId, stage, eyeId, ref textureHandle);
		}
		return textureHandle;
	}

	public static int GetLayerTextureStageCount(int layerId)
	{
		int layerTextureStageCount = 1;
		if (version >= OVRP_1_15_0.version)
		{
			OVRP_1_15_0.ovrp_GetLayerTextureStageCount(layerId, ref layerTextureStageCount);
		}
		return layerTextureStageCount;
	}

	public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_Update2(0, frameIndex, predictionSeconds) == Bool.True;
		}
		return false;
	}

	public static Posef GetNodePose(Node nodeId, Step stepId)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Pose;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodePose2(0, nodeId);
		}
		return OVRP_0_1_2.ovrp_GetNodePose(nodeId);
	}

	public static Vector3f GetNodeVelocity(Node nodeId, Step stepId)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Velocity;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodeVelocity2(0, nodeId).Position;
		}
		return OVRP_0_1_3.ovrp_GetNodeVelocity(nodeId).Position;
	}

	public static Vector3f GetNodeAngularVelocity(Node nodeId, Step stepId)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularVelocity;
		}
		return default(Vector3f);
	}

	public static Vector3f GetNodeAcceleration(Node nodeId, Step stepId)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Acceleration;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodeAcceleration2(0, nodeId).Position;
		}
		return OVRP_0_1_3.ovrp_GetNodeAcceleration(nodeId).Position;
	}

	public static Vector3f GetNodeAngularAcceleration(Node nodeId, Step stepId)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularAcceleration;
		}
		return default(Vector3f);
	}

	public static bool GetNodePresent(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodePresent(nodeId) == Bool.True;
	}

	public static bool GetNodeOrientationTracked(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodeOrientationTracked(nodeId) == Bool.True;
	}

	public static bool GetNodePositionTracked(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodePositionTracked(nodeId) == Bool.True;
	}

	public static ControllerState GetControllerState(uint controllerMask)
	{
		return OVRP_1_1_0.ovrp_GetControllerState(controllerMask);
	}

	public static ControllerState2 GetControllerState2(uint controllerMask)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetControllerState2(controllerMask);
		}
		return new ControllerState2(OVRP_1_1_0.ovrp_GetControllerState(controllerMask));
	}

	public static ControllerState4 GetControllerState4(uint controllerMask)
	{
		if (version >= OVRP_1_16_0.version)
		{
			ControllerState4 controllerState = default(ControllerState4);
			OVRP_1_16_0.ovrp_GetControllerState4(controllerMask, ref controllerState);
			return controllerState;
		}
		return new ControllerState4(GetControllerState2(controllerMask));
	}

	public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude)
	{
		return OVRP_0_1_2.ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == Bool.True;
	}

	public static HapticsDesc GetControllerHapticsDesc(uint controllerMask)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetControllerHapticsDesc(controllerMask);
		}
		return default(HapticsDesc);
	}

	public static HapticsState GetControllerHapticsState(uint controllerMask)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetControllerHapticsState(controllerMask);
		}
		return default(HapticsState);
	}

	public static bool SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == Bool.True;
		}
		return false;
	}

	public static float GetEyeRecommendedResolutionScale()
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetEyeRecommendedResolutionScale();
		}
		return 1f;
	}

	public static float GetAppCpuStartToGpuEndTime()
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetAppCpuStartToGpuEndTime();
		}
		return 0f;
	}

	public static bool GetBoundaryConfigured()
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryConfigured() == Bool.True;
		}
		return false;
	}

	public static BoundaryTestResult TestBoundaryNode(Node nodeId, BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_TestBoundaryNode(nodeId, boundaryType);
		}
		return default(BoundaryTestResult);
	}

	public static BoundaryTestResult TestBoundaryPoint(Vector3f point, BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_TestBoundaryPoint(point, boundaryType);
		}
		return default(BoundaryTestResult);
	}

	public static bool SetBoundaryLookAndFeel(BoundaryLookAndFeel lookAndFeel)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_SetBoundaryLookAndFeel(lookAndFeel) == Bool.True;
		}
		return false;
	}

	public static bool ResetBoundaryLookAndFeel()
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_ResetBoundaryLookAndFeel() == Bool.True;
		}
		return false;
	}

	public static BoundaryGeometry GetBoundaryGeometry(BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryGeometry(boundaryType);
		}
		return default(BoundaryGeometry);
	}

	public static bool GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount)
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == Bool.True;
		}
		pointsCount = 0;
		return false;
	}

	public static AppPerfStats GetAppPerfStats()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetAppPerfStats();
		}
		return default(AppPerfStats);
	}

	public static bool ResetAppPerfStats()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_ResetAppPerfStats() == Bool.True;
		}
		return false;
	}

	public static float GetAppFramerate()
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetAppFramerate();
		}
		return 0f;
	}

	public static bool SetHandNodePoseStateLatency(double latencyInSeconds)
	{
		if (version >= OVRP_1_18_0.version)
		{
			if (OVRP_1_18_0.ovrp_SetHandNodePoseStateLatency(latencyInSeconds) == Result.Success)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static double GetHandNodePoseStateLatency()
	{
		if (version >= OVRP_1_18_0.version)
		{
			double latencyInSeconds = 0.0;
			if (OVRP_1_18_0.ovrp_GetHandNodePoseStateLatency(out latencyInSeconds) == Result.Success)
			{
				return latencyInSeconds;
			}
			return 0.0;
		}
		return 0.0;
	}

	public static EyeTextureFormat GetDesiredEyeTextureFormat()
	{
		if (version >= OVRP_1_11_0.version)
		{
			uint num = (uint)OVRP_1_11_0.ovrp_GetDesiredEyeTextureFormat();
			if (num == 1)
			{
				num = 0u;
			}
			return (EyeTextureFormat)num;
		}
		return EyeTextureFormat.Default;
	}

	public static bool SetDesiredEyeTextureFormat(EyeTextureFormat value)
	{
		if (version >= OVRP_1_11_0.version)
		{
			return OVRP_1_11_0.ovrp_SetDesiredEyeTextureFormat(value) == Bool.True;
		}
		return false;
	}

	public static bool InitializeMixedReality()
	{
		if (version >= OVRP_1_15_0.version)
		{
			Result result = OVRP_1_15_0.ovrp_InitializeMixedReality();
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool ShutdownMixedReality()
	{
		if (version >= OVRP_1_15_0.version)
		{
			Result result = OVRP_1_15_0.ovrp_ShutdownMixedReality();
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool IsMixedRealityInitialized()
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_GetMixedRealityInitialized() == Bool.True;
		}
		return false;
	}

	public static int GetExternalCameraCount()
	{
		if (version >= OVRP_1_15_0.version)
		{
			int cameraCount = 0;
			if (OVRP_1_15_0.ovrp_GetExternalCameraCount(out cameraCount) != 0)
			{
				return 0;
			}
			return cameraCount;
		}
		return 0;
	}

	public static bool UpdateExternalCamera()
	{
		if (version >= OVRP_1_15_0.version)
		{
			Result result = OVRP_1_15_0.ovrp_UpdateExternalCamera();
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool GetMixedRealityCameraInfo(int cameraId, out CameraExtrinsics cameraExtrinsics, out CameraIntrinsics cameraIntrinsics)
	{
		cameraExtrinsics = default(CameraExtrinsics);
		cameraIntrinsics = default(CameraIntrinsics);
		if (version >= OVRP_1_15_0.version)
		{
			bool result = true;
			if (OVRP_1_15_0.ovrp_GetExternalCameraExtrinsics(cameraId, out cameraExtrinsics) != 0)
			{
				result = false;
			}
			if (OVRP_1_15_0.ovrp_GetExternalCameraIntrinsics(cameraId, out cameraIntrinsics) != 0)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static Vector3f GetBoundaryDimensions(BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryDimensions(boundaryType);
		}
		return default(Vector3f);
	}

	public static bool GetBoundaryVisible()
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryVisible() == Bool.True;
		}
		return false;
	}

	public static bool SetBoundaryVisible(bool value)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_SetBoundaryVisible(ToBool(value)) == Bool.True;
		}
		return false;
	}

	public static SystemHeadset GetSystemHeadsetType()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetSystemHeadsetType();
		}
		return SystemHeadset.None;
	}

	public static Controller GetActiveController()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetActiveController();
		}
		return Controller.None;
	}

	public static Controller GetConnectedControllers()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetConnectedControllers();
		}
		return Controller.None;
	}

	private static Bool ToBool(bool b)
	{
		return b ? Bool.True : Bool.False;
	}

	public static TrackingOrigin GetTrackingOriginType()
	{
		return OVRP_1_0_0.ovrp_GetTrackingOriginType();
	}

	public static bool SetTrackingOriginType(TrackingOrigin originType)
	{
		return OVRP_1_0_0.ovrp_SetTrackingOriginType(originType) == Bool.True;
	}

	public static Posef GetTrackingCalibratedOrigin()
	{
		return OVRP_1_0_0.ovrp_GetTrackingCalibratedOrigin();
	}

	public static bool SetTrackingCalibratedOrigin()
	{
		return OVRP_1_2_0.ovrpi_SetTrackingCalibratedOrigin() == Bool.True;
	}

	public static bool RecenterTrackingOrigin(RecenterFlags flags)
	{
		return OVRP_1_0_0.ovrp_RecenterTrackingOrigin((uint)flags) == Bool.True;
	}

	public static bool UpdateCameraDevices()
	{
		if (version >= OVRP_1_16_0.version)
		{
			Result result = OVRP_1_16_0.ovrp_UpdateCameraDevices();
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceAvailable(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Bool @bool = OVRP_1_16_0.ovrp_IsCameraDeviceAvailable(cameraDevice);
			return @bool == Bool.True;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, int width, int height)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Sizei preferredColorFrameSize = default(Sizei);
			preferredColorFrameSize.w = width;
			preferredColorFrameSize.h = height;
			Result result = OVRP_1_16_0.ovrp_SetCameraDevicePreferredColorFrameSize(cameraDevice, preferredColorFrameSize);
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool OpenCameraDevice(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Result result = OVRP_1_16_0.ovrp_OpenCameraDevice(cameraDevice);
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool CloseCameraDevice(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Result result = OVRP_1_16_0.ovrp_CloseCameraDevice(cameraDevice);
			if (result != 0)
			{
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool HasCameraDeviceOpened(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Bool @bool = OVRP_1_16_0.ovrp_HasCameraDeviceOpened(cameraDevice);
			return @bool == Bool.True;
		}
		return false;
	}

	public static bool IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Bool @bool = OVRP_1_16_0.ovrp_IsCameraDeviceColorFrameAvailable(cameraDevice);
			return @bool == Bool.True;
		}
		return false;
	}

	public static Texture2D GetCameraDeviceColorFrameTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Sizei colorFrameSize = default(Sizei);
			if (OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameSize(cameraDevice, out colorFrameSize) != 0)
			{
				return null;
			}
			IntPtr colorFrameBgraPixels;
			int colorFrameRowPitch;
			if (OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameBgraPixels(cameraDevice, out colorFrameBgraPixels, out colorFrameRowPitch) != 0)
			{
				return null;
			}
			if (colorFrameRowPitch != colorFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraFrameTexture || cachedCameraFrameTexture.width != colorFrameSize.w || cachedCameraFrameTexture.height != colorFrameSize.h)
			{
				cachedCameraFrameTexture = new Texture2D(colorFrameSize.w, colorFrameSize.h, TextureFormat.BGRA32, false);
			}
			cachedCameraFrameTexture.LoadRawTextureData(colorFrameBgraPixels, colorFrameRowPitch * colorFrameSize.h);
			cachedCameraFrameTexture.Apply();
			return cachedCameraFrameTexture;
		}
		return null;
	}

	public static bool DoesCameraDeviceSupportDepth(CameraDevice cameraDevice)
	{
		Bool supportDepth;
		if (version >= OVRP_1_17_0.version)
		{
			return OVRP_1_17_0.ovrp_DoesCameraDeviceSupportDepth(cameraDevice, out supportDepth) == Result.Success && supportDepth == Bool.True;
		}
		return false;
	}

	public static bool SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Result result = OVRP_1_17_0.ovrp_SetCameraDeviceDepthSensingMode(camera, depthSensoringMode);
			return result == Result.Success;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Result result = OVRP_1_17_0.ovrp_SetCameraDevicePreferredDepthQuality(camera, depthQuality);
			return result == Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceDepthFrameAvailable(CameraDevice cameraDevice)
	{
		Bool available;
		if (version >= OVRP_1_17_0.version)
		{
			return OVRP_1_17_0.ovrp_IsCameraDeviceDepthFrameAvailable(cameraDevice, out available) == Result.Success && available == Bool.True;
		}
		return false;
	}

	public static Texture2D GetCameraDeviceDepthFrameTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Sizei depthFrameSize = default(Sizei);
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out depthFrameSize) != 0)
			{
				return null;
			}
			IntPtr depthFramePixels;
			int depthFrameRowPitch;
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFramePixels(cameraDevice, out depthFramePixels, out depthFrameRowPitch) != 0)
			{
				return null;
			}
			if (depthFrameRowPitch != depthFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraDepthTexture || cachedCameraDepthTexture.width != depthFrameSize.w || cachedCameraDepthTexture.height != depthFrameSize.h)
			{
				cachedCameraDepthTexture = new Texture2D(depthFrameSize.w, depthFrameSize.h, TextureFormat.RFloat, false);
				cachedCameraDepthTexture.filterMode = FilterMode.Point;
			}
			cachedCameraDepthTexture.LoadRawTextureData(depthFramePixels, depthFrameRowPitch * depthFrameSize.h);
			cachedCameraDepthTexture.Apply();
			return cachedCameraDepthTexture;
		}
		return null;
	}

	public static Texture2D GetCameraDeviceDepthConfidenceTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Sizei depthFrameSize = default(Sizei);
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out depthFrameSize) != 0)
			{
				return null;
			}
			IntPtr depthConfidencePixels;
			int depthConfidenceRowPitch;
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthConfidencePixels(cameraDevice, out depthConfidencePixels, out depthConfidenceRowPitch) != 0)
			{
				return null;
			}
			if (depthConfidenceRowPitch != depthFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraDepthConfidenceTexture || cachedCameraDepthConfidenceTexture.width != depthFrameSize.w || cachedCameraDepthConfidenceTexture.height != depthFrameSize.h)
			{
				cachedCameraDepthConfidenceTexture = new Texture2D(depthFrameSize.w, depthFrameSize.h, TextureFormat.RFloat, false);
			}
			cachedCameraDepthConfidenceTexture.LoadRawTextureData(depthConfidencePixels, depthConfidenceRowPitch * depthFrameSize.h);
			cachedCameraDepthConfidenceTexture.Apply();
			return cachedCameraDepthConfidenceTexture;
		}
		return null;
	}
}
