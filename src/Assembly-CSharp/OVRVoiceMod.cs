using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class OVRVoiceMod : MonoBehaviour
{
	public enum ovrVoiceModError
	{
		Unknown = -2250,
		CannotCreateContext = -2251,
		InvalidParam = -2252,
		BadSampleRate = -2253,
		MissingDLL = -2254,
		BadVersion = -2255,
		UndefinedFunction = -2256
	}

	public enum ovrViceModFlag
	{
		None
	}

	public const int ovrVoiceModSuccess = 0;

	public const string strOVRLS = "OVRVoiceMod";

	private static int sOVRVoiceModInit = -2250;

	public static OVRVoiceMod sInstance;

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_Initialize(int SampleRate, int BufferSize);

	[DllImport("OVRVoiceMod")]
	private static extern void ovrVoiceModDll_Shutdown();

	[DllImport("OVRVoiceMod")]
	private static extern IntPtr ovrVoicemodDll_GetVersion(ref int Major, ref int Minor, ref int Patch);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_CreateContext(ref uint Context);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_DestroyContext(uint Context);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_SendParameter(uint Context, int Parameter, int Value);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_ProcessFrame(uint Context, uint Flags, float[] AudioBuffer);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_ProcessFrameInterleaved(uint Context, uint Flags, float[] AudioBuffer);

	[DllImport("OVRVoiceMod")]
	private static extern int ovrVoiceModDll_GetAverageAbsVolume(uint Context, ref float Volume);

	private void Awake()
	{
		if (sInstance == null)
		{
			sInstance = this;
			int outputSampleRate = AudioSettings.outputSampleRate;
			int bufferLength;
			int numBuffers;
			AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
			string message = $"OvrVoiceMod Awake: Queried SampleRate: {outputSampleRate:F0} BufferSize: {bufferLength:F0}";
			Debug.LogWarning(message);
			sOVRVoiceModInit = ovrVoiceModDll_Initialize(outputSampleRate, bufferLength);
			if (sOVRVoiceModInit != 0)
			{
				Debug.LogWarning($"OvrVoiceMod Awake: Failed to init VoiceMod library");
			}
			OVRTouchpad.Create();
		}
		else
		{
			Debug.LogWarning($"OVRVoiceMod Awake: Only one instance of OVRVoiceMod can exist in the scene.");
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (sInstance != this)
		{
			Debug.LogWarning("OVRVoiceMod OnDestroy: This is not the correct OVRVoiceMod instance.");
		}
		ovrVoiceModDll_Shutdown();
		sOVRVoiceModInit = -2250;
	}

	public static int IsInitialized()
	{
		return sOVRVoiceModInit;
	}

	public static int CreateContext(ref uint context)
	{
		if (IsInitialized() != 0)
		{
			return -2251;
		}
		return ovrVoiceModDll_CreateContext(ref context);
	}

	public static int DestroyContext(uint context)
	{
		if (IsInitialized() != 0)
		{
			return -2250;
		}
		return ovrVoiceModDll_DestroyContext(context);
	}

	public static int SendParameter(uint context, int parameter, int value)
	{
		if (IsInitialized() != 0)
		{
			return -2250;
		}
		return ovrVoiceModDll_SendParameter(context, parameter, value);
	}

	public static int ProcessFrame(uint context, float[] audioBuffer)
	{
		if (IsInitialized() != 0)
		{
			return -2250;
		}
		return ovrVoiceModDll_ProcessFrame(context, 0u, audioBuffer);
	}

	public static int ProcessFrameInterleaved(uint context, float[] audioBuffer)
	{
		if (IsInitialized() != 0)
		{
			return -2250;
		}
		return ovrVoiceModDll_ProcessFrameInterleaved(context, 0u, audioBuffer);
	}

	public static float GetAverageAbsVolume(uint context)
	{
		if (IsInitialized() != 0)
		{
			return 0f;
		}
		return 0f;
	}
}
