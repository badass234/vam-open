using UnityEngine;

public class ONSPAmbisonicsNative : MonoBehaviour
{
	public enum ovrAmbisonicsNativeStatus
	{
		Uninitialized = -1,
		NotEnabled,
		Success,
		StreamError,
		ProcessError,
		MaxStatValue
	}

	private static int numFOAChannels = 4;

	private static int paramVSpeakerMode = 6;

	private static int paramAmbiStat = 7;

	private ovrAmbisonicsNativeStatus currentStatus = ovrAmbisonicsNativeStatus.Uninitialized;

	[SerializeField]
	private bool useVirtualSpeakers;

	public bool UseVirtualSpeakers
	{
		get
		{
			return useVirtualSpeakers;
		}
		set
		{
			useVirtualSpeakers = value;
		}
	}

	private void OnEnable()
	{
		AudioSource component = GetComponent<AudioSource>();
		currentStatus = ovrAmbisonicsNativeStatus.Uninitialized;
		if (component == null)
		{
			Debug.Log("Ambisonic ERROR: AudioSource does not exist.");
			return;
		}
		if (component.spatialize)
		{
			Debug.Log("Ambisonic WARNING: Turning spatialize field off for Ambisonic sources.");
			component.spatialize = false;
		}
		if (component.clip == null)
		{
			Debug.Log("Ambisonic ERROR: AudioSource does not contain an audio clip.");
		}
		else if (component.clip.channels != numFOAChannels)
		{
			Debug.Log("Ambisonic ERROR: AudioSource clip does not have correct number of channels.");
		}
	}

	private void Update()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (component == null)
		{
			return;
		}
		if (useVirtualSpeakers)
		{
			component.SetAmbisonicDecoderFloat(paramVSpeakerMode, 1f);
		}
		else
		{
			component.SetAmbisonicDecoderFloat(paramVSpeakerMode, 0f);
		}
		float value = 0f;
		component.GetAmbisonicDecoderFloat(paramAmbiStat, out value);
		ovrAmbisonicsNativeStatus ovrAmbisonicsNativeStatus = (ovrAmbisonicsNativeStatus)value;
		if (ovrAmbisonicsNativeStatus != currentStatus)
		{
			switch (ovrAmbisonicsNativeStatus)
			{
			case ovrAmbisonicsNativeStatus.NotEnabled:
				Debug.Log("Ambisonic Native: Ambisonic not enabled on clip. Check clip field and turn it on");
				break;
			case ovrAmbisonicsNativeStatus.Uninitialized:
				Debug.Log("Ambisonic Native: Stream uninitialized");
				break;
			case ovrAmbisonicsNativeStatus.Success:
				Debug.Log("Ambisonic Native: Stream successfully initialized and playing/playable");
				break;
			case ovrAmbisonicsNativeStatus.StreamError:
				Debug.Log("Ambisonic Native WARNING: Stream error (bad input format?)");
				break;
			case ovrAmbisonicsNativeStatus.ProcessError:
				Debug.Log("Ambisonic Native WARNING: Stream process error (check default speaker setup)");
				break;
			}
		}
		currentStatus = ovrAmbisonicsNativeStatus;
	}
}
