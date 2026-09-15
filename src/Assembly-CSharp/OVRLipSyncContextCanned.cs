using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRLipSyncContextCanned : OVRLipSyncContextBase
{
	public OVRLipSyncSequence currentSequence;

	private void Update()
	{
		if (audioSource.isPlaying && currentSequence != null)
		{
			OVRLipSync.Frame frameAtTime = currentSequence.GetFrameAtTime(audioSource.time);
			base.Frame.CopyInput(frameAtTime);
		}
	}
}
