using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRVoiceModContext : MonoBehaviour
{
	public enum ovrVoiceModParams
	{
		MixInputAudio,
		PitchInputAudio,
		SetBands,
		FormantCorrection,
		Carrier1_TrackPitch,
		Carrier1_Type,
		Carrier1_Gain,
		Carrier1_Frequency,
		Carrier1_Note,
		Carrier1_PulseWidth,
		Carrier1_CycledNoiseSize,
		Carrier2_TrackPitch,
		Carrier2_Type,
		Carrier2_Gain,
		Carrier2_Frequency,
		Carrier2_Note,
		Carrier2_PulseWidth,
		Carrier2_CycledNoiseSize,
		Count
	}

	public struct VMPreset
	{
		public string info;

		public Color color;

		public float mix;

		public float pitch;

		public int bands;

		public int formant;

		public int c1PTrack;

		public int c1Type;

		public float c1Gain;

		public float c1Freq;

		public int c1Note;

		public float c1PW;

		public int c1CNS;

		public int c2PTrack;

		public int c2Type;

		public float c2Gain;

		public float c2Freq;

		public int c2Note;

		public float c2PW;

		public int c2CNS;
	}

	public AudioSource audioSource;

	public float gain = 1f;

	public bool audioMute = true;

	public KeyCode loopback = KeyCode.L;

	private VMPreset[] VMPresets = new VMPreset[10]
	{
		new VMPreset
		{
			info = "-INIT-\nNo pitch shift, no vocode",
			color = Color.gray,
			mix = 1f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 0,
			c1Gain = 0f,
			c1Freq = 440f,
			c1Note = -1,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 0,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = -1,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "FULL VOCODE\nCarrier 1: Full noise",
			color = Color.white,
			mix = 0f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 0,
			c1Gain = 1f,
			c1Freq = 440f,
			c1Note = -1,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 0,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = -1,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "FULL VOCODE\nCarrier 1: Cycled noise 512",
			color = Color.blue,
			mix = 0f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 1,
			c1Gain = 1f,
			c1Freq = 440f,
			c1Note = -1,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 0,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = -1,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "FULL VOCODE\nCarrier 1: Saw Up, Freq 220",
			color = Color.magenta,
			mix = 0f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 2,
			c1Gain = 1f,
			c1Freq = 220f,
			c1Note = -1,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 0,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = -1,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "FULL VOCODE\nCarrier 1: Saw Up, Pitch tracked\n",
			color = Color.cyan,
			mix = 0f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 1,
			c1Type = 2,
			c1Gain = 0.34f,
			c1Freq = 440f,
			c1Note = -1,
			c1PW = 0.1f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 0,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = -1,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "INPUT PLUS VOCODE\nInput 50%, Vocode 50%\nPitch 1.0\nCarrier 1: Full Noise,\nCarrier 2: Cycled Noise 512",
			color = Color.green,
			mix = 0.5f,
			pitch = 1f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 0,
			c1Gain = 0.5f,
			c1Freq = 440f,
			c1Note = 57,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 1,
			c2Gain = 0.5f,
			c2Freq = 440f,
			c2Note = 45,
			c2PW = 0.25f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "INPUT PLUS VOCODE PLUS PITCH DOWN\nInput 50%, Vocode 50%\nPitch 0.75\nCarrier 1: Cycled Noise 512\nCarrier 2: Cycled Noise 768",
			color = Color.red,
			mix = 0.5f,
			pitch = 0.75f,
			bands = 32,
			formant = 0,
			c1PTrack = 0,
			c1Type = 1,
			c1Gain = 0.6f,
			c1Freq = 440f,
			c1Note = 57,
			c1PW = 0.5f,
			c1CNS = 512,
			c2PTrack = 0,
			c2Type = 3,
			c2Gain = 0.2f,
			c2Freq = 440f,
			c2Note = 40,
			c2PW = 0.25f,
			c2CNS = 768
		},
		new VMPreset
		{
			info = "PITCH ONLY\nPitch 1.25 (Formant correction)",
			color = Color.blue,
			mix = 1f,
			pitch = 1.25f,
			bands = 32,
			formant = 1,
			c1PTrack = 0,
			c1Type = 1,
			c1Gain = 1f,
			c1Freq = 440f,
			c1Note = 57,
			c1PW = 0.5f,
			c1CNS = 400,
			c2PTrack = 0,
			c2Type = 3,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = 52,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "PITCH ONLY\nPitch 0.5 (Formant correction)",
			color = Color.green,
			mix = 1f,
			pitch = 0.5f,
			bands = 32,
			formant = 1,
			c1PTrack = 0,
			c1Type = 1,
			c1Gain = 1f,
			c1Freq = 440f,
			c1Note = 57,
			c1PW = 0.5f,
			c1CNS = 400,
			c2PTrack = 0,
			c2Type = 3,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = 52,
			c2PW = 0.5f,
			c2CNS = 512
		},
		new VMPreset
		{
			info = "PITCH ONLY\nPitch 2.0 (Formant correction)",
			color = Color.yellow,
			mix = 1f,
			pitch = 2f,
			bands = 32,
			formant = 1,
			c1PTrack = 0,
			c1Type = 1,
			c1Gain = 1f,
			c1Freq = 440f,
			c1Note = 57,
			c1PW = 0.5f,
			c1CNS = 400,
			c2PTrack = 0,
			c2Type = 3,
			c2Gain = 0f,
			c2Freq = 440f,
			c2Note = 52,
			c2PW = 0.5f,
			c2CNS = 512
		}
	};

	public float VM_MixAudio = 1f;

	public float VM_Pitch = 1f;

	public int VM_Bands = 32;

	public int VM_FormantCorrect;

	public int VM_C1_TrackPitch;

	public int VM_C1_Type;

	public float VM_C1_Gain = 0.5f;

	public float VM_C1_Freq = 440f;

	public int VM_C1_Note = 67;

	public float VM_C1_PulseWidth = 0.5f;

	public int VM_C1_CycledNoiseSize = 512;

	public int VM_C2_TrackPitch;

	public int VM_C2_Type;

	public float VM_C2_Gain = 0.5f;

	public float VM_C2_Freq = 440f;

	public int VM_C2_Note = 67;

	public float VM_C2_PulseWidth = 0.5f;

	public int VM_C2_CycledNoiseSize = 512;

	private uint context;

	private float prevVol;

	private void Awake()
	{
		if (!audioSource)
		{
			audioSource = GetComponent<AudioSource>();
		}
		if ((bool)audioSource)
		{
		}
	}

	private void Start()
	{
		lock (this)
		{
			if (context == 0 && OVRVoiceMod.CreateContext(ref context) != 0)
			{
				Debug.Log("OVRVoiceModContext.Start ERROR: Could not create VoiceMod context.");
				return;
			}
		}
		OVRMessenger.AddListener<OVRTouchpad.TouchEvent>("Touchpad", LocalTouchEventCallback);
		SendVoiceModUpdate();
	}

	private void Update()
	{
		if (Input.GetKeyDown(loopback))
		{
			audioMute = !audioMute;
			OVRDebugConsole.Clear();
			OVRDebugConsole.ClearTimeout(1.5f);
			if (!audioMute)
			{
				OVRDebugConsole.Log("LOOPBACK MODE: ENABLED");
			}
			else
			{
				OVRDebugConsole.Log("LOOPBACK MODE: DISABLED");
			}
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			gain -= 0.1f;
			if (gain < 0.5f)
			{
				gain = 0.5f;
			}
			string text = "LINEAR GAIN: ";
			text += gain;
			OVRDebugConsole.Clear();
			OVRDebugConsole.Log(text);
			OVRDebugConsole.ClearTimeout(1.5f);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			gain += 0.1f;
			if (gain > 3f)
			{
				gain = 3f;
			}
			string text2 = "LINEAR GAIN: ";
			text2 += gain;
			OVRDebugConsole.Clear();
			OVRDebugConsole.Log(text2);
			OVRDebugConsole.ClearTimeout(1.5f);
		}
		UpdateVoiceModUpdate();
	}

	private void OnDestroy()
	{
		lock (this)
		{
			if (context != 0 && OVRVoiceMod.DestroyContext(context) != 0)
			{
				Debug.Log("OVRVoiceModContext.OnDestroy ERROR: Could not delete VoiceMod context.");
			}
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (OVRVoiceMod.IsInitialized() != 0 || audioSource == null)
		{
			return;
		}
		for (int i = 0; i < data.Length; i++)
		{
			data[i] *= gain;
		}
		lock (this)
		{
			if (context != 0)
			{
				OVRVoiceMod.ProcessFrameInterleaved(context, data);
			}
		}
		if (audioMute)
		{
			for (int j = 0; j < data.Length; j++)
			{
				data[j] *= 0f;
			}
		}
	}

	public int SendParameter(ovrVoiceModParams parameter, int value)
	{
		if (OVRVoiceMod.IsInitialized() != 0)
		{
			return -2250;
		}
		return OVRVoiceMod.SendParameter(context, (int)parameter, value);
	}

	public bool SetPreset(int preset)
	{
		if (preset < 0 || preset >= VMPresets.Length)
		{
			return false;
		}
		VM_MixAudio = VMPresets[preset].mix;
		VM_Pitch = VMPresets[preset].pitch;
		VM_Bands = VMPresets[preset].bands;
		VM_FormantCorrect = VMPresets[preset].formant;
		VM_C1_TrackPitch = VMPresets[preset].c1PTrack;
		VM_C1_Type = VMPresets[preset].c1Type;
		VM_C1_Gain = VMPresets[preset].c1Gain;
		VM_C1_Freq = VMPresets[preset].c1Freq;
		VM_C1_Note = VMPresets[preset].c1Note;
		VM_C1_PulseWidth = VMPresets[preset].c1PW;
		VM_C1_CycledNoiseSize = VMPresets[preset].c1CNS;
		VM_C2_TrackPitch = VMPresets[preset].c2PTrack;
		VM_C2_Type = VMPresets[preset].c2Type;
		VM_C2_Gain = VMPresets[preset].c2Gain;
		VM_C2_Freq = VMPresets[preset].c2Freq;
		VM_C2_Note = VMPresets[preset].c2Note;
		VM_C2_PulseWidth = VMPresets[preset].c2PW;
		VM_C2_CycledNoiseSize = VMPresets[preset].c2CNS;
		SendVoiceModUpdate();
		OVRDebugConsole.Clear();
		OVRDebugConsole.Log(VMPresets[preset].info);
		OVRDebugConsole.ClearTimeout(5f);
		return true;
	}

	public int GetNumPresets()
	{
		return VMPresets.Length;
	}

	public Color GetPresetColor(int preset)
	{
		if (preset < 0 || preset >= VMPresets.Length)
		{
			return Color.black;
		}
		return VMPresets[preset].color;
	}

	public float GetAverageAbsVolume()
	{
		if (context == 0)
		{
			return 0f;
		}
		return prevVol = prevVol * 0.8f + OVRVoiceMod.GetAverageAbsVolume(context) * 0.2f;
	}

	private void LocalTouchEventCallback(OVRTouchpad.TouchEvent touchEvent)
	{
		if (touchEvent == OVRTouchpad.TouchEvent.SingleTap)
		{
			audioMute = !audioMute;
			OVRDebugConsole.Clear();
			OVRDebugConsole.ClearTimeout(1.5f);
			if (!audioMute)
			{
				OVRDebugConsole.Log("LOOPBACK MODE: ENABLED");
			}
			else
			{
				OVRDebugConsole.Log("LOOPBACK MODE: DISABLED");
			}
		}
	}

	private void UpdateVoiceModUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SendVoiceModUpdate();
			OVRDebugConsole.Clear();
			OVRDebugConsole.Log("UPDATED VOICE MOD FROM INSPECTOR");
			OVRDebugConsole.ClearTimeout(1f);
		}
	}

	private void SendVoiceModUpdate()
	{
		VM_MixAudio = Mathf.Clamp(VM_MixAudio, 0f, 1f);
		VM_Pitch = Mathf.Clamp(VM_Pitch, 0.5f, 2f);
		VM_Bands = Mathf.Clamp(VM_Bands, 1, 128);
		VM_FormantCorrect = Mathf.Clamp(VM_FormantCorrect, 0, 1);
		VM_C1_TrackPitch = Mathf.Clamp(VM_C1_TrackPitch, 0, 1);
		VM_C1_Type = Mathf.Clamp(VM_C1_Type, 0, 3);
		VM_C1_Gain = Mathf.Clamp(VM_C1_Gain, 0f, 1f);
		VM_C1_Freq = Mathf.Clamp(VM_C1_Freq, 0f, 96000f);
		VM_C1_Note = Mathf.Clamp(VM_C1_Note, -1, 127);
		VM_C1_PulseWidth = Mathf.Clamp(VM_C1_PulseWidth, 0f, 1f);
		VM_C1_CycledNoiseSize = Mathf.Clamp(VM_C1_CycledNoiseSize, 0, 1024);
		VM_C2_TrackPitch = Mathf.Clamp(VM_C2_TrackPitch, 0, 1);
		VM_C2_Type = Mathf.Clamp(VM_C2_Type, 0, 3);
		VM_C2_Gain = Mathf.Clamp(VM_C2_Gain, 0f, 1f);
		VM_C2_Freq = Mathf.Clamp(VM_C2_Freq, 0f, 96000f);
		VM_C2_Note = Mathf.Clamp(VM_C2_Note, -1, 127);
		VM_C2_PulseWidth = Mathf.Clamp(VM_C2_PulseWidth, 0f, 1f);
		VM_C2_CycledNoiseSize = Mathf.Clamp(VM_C2_CycledNoiseSize, 0, 1024);
		SendParameter(ovrVoiceModParams.MixInputAudio, (int)(100f * VM_MixAudio));
		SendParameter(ovrVoiceModParams.PitchInputAudio, (int)(100f * VM_Pitch));
		SendParameter(ovrVoiceModParams.SetBands, VM_Bands);
		SendParameter(ovrVoiceModParams.FormantCorrection, VM_FormantCorrect);
		SendParameter(ovrVoiceModParams.Carrier1_TrackPitch, VM_C1_TrackPitch);
		SendParameter(ovrVoiceModParams.Carrier1_Type, VM_C1_Type);
		SendParameter(ovrVoiceModParams.Carrier1_Gain, (int)(100f * VM_C1_Gain));
		if (VM_C1_Note == -1)
		{
			SendParameter(ovrVoiceModParams.Carrier1_Frequency, (int)(100f * VM_C1_Freq));
		}
		else
		{
			SendParameter(ovrVoiceModParams.Carrier1_Note, VM_C1_Note);
		}
		SendParameter(ovrVoiceModParams.Carrier1_PulseWidth, (int)(100f * VM_C1_PulseWidth));
		SendParameter(ovrVoiceModParams.Carrier1_CycledNoiseSize, VM_C1_CycledNoiseSize);
		SendParameter(ovrVoiceModParams.Carrier2_TrackPitch, VM_C2_TrackPitch);
		SendParameter(ovrVoiceModParams.Carrier2_Type, VM_C2_Type);
		SendParameter(ovrVoiceModParams.Carrier2_Gain, (int)(100f * VM_C2_Gain));
		if (VM_C2_Note == -1)
		{
			SendParameter(ovrVoiceModParams.Carrier2_Frequency, (int)(100f * VM_C2_Freq));
		}
		else
		{
			SendParameter(ovrVoiceModParams.Carrier2_Note, VM_C2_Note);
		}
		SendParameter(ovrVoiceModParams.Carrier2_PulseWidth, (int)(100f * VM_C2_PulseWidth));
		SendParameter(ovrVoiceModParams.Carrier2_CycledNoiseSize, VM_C1_CycledNoiseSize);
	}
}
