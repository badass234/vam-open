using System;
using UnityEngine.Audio;

[Serializable]
public class MixerSnapshot
{
	public AudioMixerSnapshot snapshot;

	public float transitionTime = 0.25f;
}
