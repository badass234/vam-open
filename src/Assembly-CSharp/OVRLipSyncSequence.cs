using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OVRLipSyncSequence : ScriptableObject
{
	public List<OVRLipSync.Frame> entries = new List<OVRLipSync.Frame>();

	public float length;

	public OVRLipSync.Frame GetFrameAtTime(float time)
	{
		OVRLipSync.Frame result = null;
		if (time < length && entries.Count > 0)
		{
			float num = time / length;
			result = entries[(int)((float)entries.Count * num)];
		}
		return result;
	}
}
