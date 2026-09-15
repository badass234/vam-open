using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRLipSyncTestAudio : MonoBehaviour
{
	public AudioSource audioSource;

	private void Start()
	{
		if (!audioSource)
		{
			audioSource = GetComponent<AudioSource>();
		}
		if ((bool)audioSource)
		{
			string dataPath = Application.dataPath;
			dataPath += "/../";
			dataPath += "TestViseme.wav";
			WWW wWW = new WWW("file:///" + dataPath);
			while (!wWW.isDone)
			{
				Debug.Log(wWW.progress);
			}
			if (wWW.GetAudioClip() != null)
			{
				audioSource.clip = wWW.GetAudioClip();
				audioSource.loop = true;
				audioSource.mute = false;
				audioSource.Play();
			}
		}
	}
}
