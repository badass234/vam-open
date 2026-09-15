using System;

[Serializable]
public class PacketRecordSettings
{
	internal bool RecordingFrames;

	public float UpdateRate = 1f / 30f;

	internal float AccumulatedTime;
}
