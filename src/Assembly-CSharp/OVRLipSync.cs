using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class OVRLipSync : MonoBehaviour
{
	public enum Result
	{
		Success = 0,
		Unknown = -2200,
		CannotCreateContext = -2201,
		InvalidParam = -2202,
		BadSampleRate = -2203,
		MissingDLL = -2204,
		BadVersion = -2205,
		UndefinedFunction = -2206
	}

	public enum Viseme
	{
		sil,
		PP,
		FF,
		TH,
		DD,
		kk,
		CH,
		SS,
		nn,
		RR,
		aa,
		E,
		ih,
		oh,
		ou
	}

	public enum Flags
	{
		None,
		DelayCompensateAudio
	}

	public enum Signals
	{
		VisemeOn,
		VisemeOff,
		VisemeAmount,
		VisemeSmoothing
	}

	public enum ContextProviders
	{
		Main,
		Other
	}

	[Serializable]
	public class Frame
	{
		public int frameNumber;

		public int frameDelay;

		public float[] Visemes = new float[VisemeCount];

		public void CopyInput(Frame input)
		{
			frameNumber = input.frameNumber;
			frameDelay = input.frameDelay;
			input.Visemes.CopyTo(Visemes, 0);
		}
	}

	public static readonly int VisemeCount = Enum.GetNames(typeof(Viseme)).Length;

	public static readonly int SignalCount = Enum.GetNames(typeof(Signals)).Length;

	public const string strOVRLS = "OVRLipSync";

	private static Result sInitialized = Result.Unknown;

	public static OVRLipSync sInstance = null;

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_Initialize(int samplerate, int buffersize);

	[DllImport("OVRLipSync")]
	private static extern void ovrLipSyncDll_Shutdown();

	[DllImport("OVRLipSync")]
	private static extern IntPtr ovrLipSyncDll_GetVersion(ref int Major, ref int Minor, ref int Patch);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_CreateContext(ref uint context, ContextProviders provider);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_DestroyContext(uint context);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_ResetContext(uint context);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_SendSignal(uint context, Signals signal, int arg1, int arg2);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_ProcessFrame(uint context, float[] audioBuffer, Flags flags, ref int frameNumber, ref int frameDelay, float[] visemes, int visemeCount);

	[DllImport("OVRLipSync")]
	private static extern int ovrLipSyncDll_ProcessFrameInterleaved(uint context, float[] audioBuffer, Flags flags, ref int frameNumber, ref int frameDelay, float[] visemes, int visemeCount);

	private void Awake()
	{
		if (sInstance == null)
		{
			sInstance = this;
			int outputSampleRate = AudioSettings.outputSampleRate;
			int bufferLength;
			int numBuffers;
			AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
			string message = $"OvrLipSync Awake: Queried SampleRate: {outputSampleRate:F0} BufferSize: {bufferLength:F0}";
			Debug.LogWarning(message);
			sInitialized = Initialize(outputSampleRate, bufferLength);
			if (sInitialized != 0)
			{
				Debug.LogWarning($"OvrLipSync Awake: Failed to init Speech Rec library");
			}
			OVRTouchpad.Create();
		}
		else
		{
			Debug.LogWarning($"OVRLipSync Awake: Only one instance of OVRPLipSync can exist in the scene.");
		}
	}

	private void OnDestroy()
	{
		if (sInstance != this)
		{
			Debug.LogWarning("OVRLipSync OnDestroy: This is not the correct OVRLipSync instance.");
		}
	}

	public static Result Initialize(int sampleRate, int bufferSize)
	{
		sInitialized = (Result)ovrLipSyncDll_Initialize(sampleRate, bufferSize);
		return sInitialized;
	}

	public static void Shutdown()
	{
		ovrLipSyncDll_Shutdown();
		sInitialized = Result.Unknown;
	}

	public static Result IsInitialized()
	{
		return sInitialized;
	}

	public static Result CreateContext(ref uint context, ContextProviders provider)
	{
		if (IsInitialized() != 0)
		{
			return Result.CannotCreateContext;
		}
		return (Result)ovrLipSyncDll_CreateContext(ref context, provider);
	}

	public static Result DestroyContext(uint context)
	{
		if (IsInitialized() != 0)
		{
			return Result.Unknown;
		}
		return (Result)ovrLipSyncDll_DestroyContext(context);
	}

	public static Result ResetContext(uint context)
	{
		if (IsInitialized() != 0)
		{
			return Result.Unknown;
		}
		return (Result)ovrLipSyncDll_ResetContext(context);
	}

	public static Result SendSignal(uint context, Signals signal, int arg1, int arg2)
	{
		if (IsInitialized() != 0)
		{
			return Result.Unknown;
		}
		return (Result)ovrLipSyncDll_SendSignal(context, signal, arg1, arg2);
	}

	public static Result ProcessFrame(uint context, float[] audioBuffer, Flags flags, Frame frame)
	{
		if (IsInitialized() != 0)
		{
			return Result.Unknown;
		}
		return (Result)ovrLipSyncDll_ProcessFrame(context, audioBuffer, flags, ref frame.frameNumber, ref frame.frameDelay, frame.Visemes, frame.Visemes.Length);
	}

	public static Result ProcessFrameInterleaved(uint context, float[] audioBuffer, Flags flags, Frame frame)
	{
		if (IsInitialized() != 0)
		{
			return Result.Unknown;
		}
		return (Result)ovrLipSyncDll_ProcessFrameInterleaved(context, audioBuffer, flags, ref frame.frameNumber, ref frame.frameDelay, frame.Visemes, frame.Visemes.Length);
	}
}
