using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpeechBlendMicrophoneInput : MonoBehaviour
{
	private AudioSource source;

	private void Start()
	{
		string deviceName = Microphone.devices[0];
		source = GetComponent<AudioSource>();
		source.clip = Microphone.Start(deviceName, true, 5, 44100);
		source.loop = true;
		while (Microphone.GetPosition(null) == 0)
		{
		}
		source.Play();
	}
}
