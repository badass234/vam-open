using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRLipSyncMicInput : MonoBehaviour
{
	public enum micActivation
	{
		HoldToSpeak,
		PushToSpeak,
		ConstantSpeak
	}

	public AudioSource audioSource;

	public bool GuiSelectDevice = true;

	[SerializeField]
	private float sensitivity = 100f;

	[SerializeField]
	private float sourceVolume = 100f;

	[SerializeField]
	private int micFrequency = 16000;

	public micActivation micControl;

	public string selectedDevice;

	public float loudness;

	private bool micSelected;

	private int minFreq;

	private int maxFreq;

	private bool focused = true;

	public float Sensitivity
	{
		get
		{
			return sensitivity;
		}
		set
		{
			sensitivity = Mathf.Clamp(value, 0f, 100f);
		}
	}

	public float SourceVolume
	{
		get
		{
			return sourceVolume;
		}
		set
		{
			sourceVolume = Mathf.Clamp(value, 0f, 100f);
		}
	}

	public float MicFrequency
	{
		get
		{
			return micFrequency;
		}
		set
		{
			micFrequency = (int)Mathf.Clamp(value, 0f, 96000f);
		}
	}

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
		audioSource.loop = true;
		audioSource.mute = false;
		if (Microphone.devices.Length != 0)
		{
			selectedDevice = Microphone.devices[0].ToString();
			micSelected = true;
			GetMicCaps();
		}
	}

	private void Update()
	{
		if (!focused)
		{
			StopMicrophone();
		}
		if (!Application.isPlaying)
		{
			StopMicrophone();
		}
		audioSource.volume = sourceVolume / 100f;
		loudness = Mathf.Clamp(GetAveragedVolume() * sensitivity * (sourceVolume / 10f), 0f, 100f);
		if (micControl == micActivation.HoldToSpeak)
		{
			if (Microphone.IsRecording(selectedDevice) && !Input.GetKey(KeyCode.Space))
			{
				StopMicrophone();
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				StartMicrophone();
			}
			if (Input.GetKeyUp(KeyCode.Space))
			{
				StopMicrophone();
			}
		}
		if (micControl == micActivation.PushToSpeak && Input.GetKeyDown(KeyCode.Space))
		{
			if (Microphone.IsRecording(selectedDevice))
			{
				StopMicrophone();
			}
			else if (!Microphone.IsRecording(selectedDevice))
			{
				StartMicrophone();
			}
		}
		if (micControl == micActivation.ConstantSpeak && !Microphone.IsRecording(selectedDevice))
		{
			StartMicrophone();
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			micSelected = false;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		focused = focus;
		if (!focused)
		{
			StopMicrophone();
		}
	}

	private void OnApplicationPause(bool focus)
	{
		focused = focus;
		if (!focused)
		{
			StopMicrophone();
		}
	}

	private void OnDisable()
	{
		StopMicrophone();
	}

	private void OnGUI()
	{
		MicDeviceGUI(Screen.width / 2 - 150, Screen.height / 2 - 75, 300f, 50f, 10f, -300f);
	}

	public void MicDeviceGUI(float left, float top, float width, float height, float buttonSpaceTop, float buttonSpaceLeft)
	{
		if (Microphone.devices.Length < 1 || !GuiSelectDevice || micSelected)
		{
			return;
		}
		for (int i = 0; i < Microphone.devices.Length; i++)
		{
			if (GUI.Button(new Rect(left + (width + buttonSpaceLeft) * (float)i, top + (height + buttonSpaceTop) * (float)i, width, height), Microphone.devices[i].ToString()))
			{
				StopMicrophone();
				selectedDevice = Microphone.devices[i].ToString();
				micSelected = true;
				GetMicCaps();
				StartMicrophone();
			}
		}
	}

	public void GetMicCaps()
	{
		if (micSelected)
		{
			Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);
			if (minFreq == 0 && maxFreq == 0)
			{
				Debug.LogWarning("GetMicCaps warning:: min and max frequencies are 0");
				minFreq = 44100;
				maxFreq = 44100;
			}
			if (micFrequency > maxFreq)
			{
				micFrequency = maxFreq;
			}
		}
	}

	public void StartMicrophone()
	{
		if (micSelected)
		{
			audioSource.clip = Microphone.Start(selectedDevice, true, 1, micFrequency);
			while (Microphone.GetPosition(selectedDevice) <= 0)
			{
			}
			audioSource.Play();
		}
	}

	public void StopMicrophone()
	{
		if (micSelected)
		{
			if (audioSource != null && audioSource.clip != null && audioSource.clip.name == "Microphone")
			{
				audioSource.Stop();
			}
			Microphone.End(selectedDevice);
		}
	}

	private float GetAveragedVolume()
	{
		return 0f;
	}
}
