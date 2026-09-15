using System;
using System.Collections.Generic;
using System.IO;

public class OvrAvatarPacket
{
	public IntPtr ovrNativePacket = IntPtr.Zero;

	private List<float> frameTimes = new List<float>();

	private List<OvrAvatarDriver.PoseFrame> frames = new List<OvrAvatarDriver.PoseFrame>();

	private List<byte[]> encodedAudioPackets = new List<byte[]>();

	public float Duration => frameTimes[frameTimes.Count - 1];

	public OvrAvatarDriver.PoseFrame FinalFrame => frames[frames.Count - 1];

	public OvrAvatarPacket()
	{
	}

	public OvrAvatarPacket(OvrAvatarDriver.PoseFrame initialPose)
	{
		frameTimes.Add(0f);
		frames.Add(initialPose);
	}

	private OvrAvatarPacket(List<float> frameTimes, List<OvrAvatarDriver.PoseFrame> frames, List<byte[]> audioPackets)
	{
		this.frameTimes = frameTimes;
		this.frames = frames;
	}

	public void AddFrame(OvrAvatarDriver.PoseFrame frame, float deltaSeconds)
	{
		frameTimes.Add(Duration + deltaSeconds);
		frames.Add(frame);
	}

	public OvrAvatarDriver.PoseFrame GetPoseFrame(float seconds)
	{
		if (frames.Count == 1)
		{
			return frames[0];
		}
		int i;
		for (i = 1; i < frameTimes.Count && frameTimes[i] < seconds; i++)
		{
		}
		OvrAvatarDriver.PoseFrame a = frames[i - 1];
		OvrAvatarDriver.PoseFrame b = frames[i];
		float num = frameTimes[i - 1];
		float num2 = frameTimes[i];
		float t = (seconds - num) / (num2 - num);
		return OvrAvatarDriver.PoseFrame.Interpolate(a, b, t);
	}

	public static OvrAvatarPacket Read(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		int num = binaryReader.ReadInt32();
		List<float> list = new List<float>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(binaryReader.ReadSingle());
		}
		List<OvrAvatarDriver.PoseFrame> list2 = new List<OvrAvatarDriver.PoseFrame>(num);
		for (int j = 0; j < num; j++)
		{
			list2.Add(binaryReader.ReadPoseFrame());
		}
		int num2 = binaryReader.ReadInt32();
		List<byte[]> list3 = new List<byte[]>(num2);
		for (int k = 0; k < num2; k++)
		{
			int count = binaryReader.ReadInt32();
			byte[] item = binaryReader.ReadBytes(count);
			list3.Add(item);
		}
		return new OvrAvatarPacket(list, list2, list3);
	}

	public void Write(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		int count = frameTimes.Count;
		binaryWriter.Write(count);
		for (int i = 0; i < count; i++)
		{
			binaryWriter.Write(frameTimes[i]);
		}
		for (int j = 0; j < count; j++)
		{
			OvrAvatarDriver.PoseFrame frame = frames[j];
			binaryWriter.Write(frame);
		}
		int count2 = encodedAudioPackets.Count;
		binaryWriter.Write(count2);
		for (int k = 0; k < count2; k++)
		{
			byte[] array = encodedAudioPackets[k];
			binaryWriter.Write(array.Length);
			binaryWriter.Write(array);
		}
	}
}
