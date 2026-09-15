using System;
using System.Collections.Generic;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarRemoteDriver : OvrAvatarDriver
{
	private Queue<OvrAvatarPacket> packetQueue = new Queue<OvrAvatarPacket>();

	private IntPtr CurrentSDKPacket = IntPtr.Zero;

	private float CurrentPacketTime;

	private const int MinPacketQueue = 1;

	private const int MaxPacketQueue = 4;

	private int CurrentSequence = -1;

	private bool isStreaming;

	private OvrAvatarPacket currentPacket;

	public void QueuePacket(int sequence, OvrAvatarPacket packet)
	{
		if (sequence > CurrentSequence)
		{
			CurrentSequence = sequence;
			packetQueue.Enqueue(packet);
		}
	}

	public override void UpdateTransforms(IntPtr sdkAvatar)
	{
		switch (Mode)
		{
		case PacketMode.SDK:
			UpdateFromSDKPacket(sdkAvatar);
			break;
		case PacketMode.Unity:
			UpdateFromUnityPacket(sdkAvatar);
			break;
		}
	}

	private void UpdateFromSDKPacket(IntPtr sdkAvatar)
	{
		if (CurrentSDKPacket == IntPtr.Zero && packetQueue.Count >= 1)
		{
			CurrentSDKPacket = packetQueue.Dequeue().ovrNativePacket;
		}
		if (!(CurrentSDKPacket != IntPtr.Zero))
		{
			return;
		}
		float num = CAPI.ovrAvatarPacket_GetDurationSeconds(CurrentSDKPacket);
		CAPI.ovrAvatar_UpdatePoseFromPacket(sdkAvatar, CurrentSDKPacket, Mathf.Min(num, CurrentPacketTime));
		CurrentPacketTime += Time.deltaTime;
		if (CurrentPacketTime > num)
		{
			CAPI.ovrAvatarPacket_Free(CurrentSDKPacket);
			CurrentSDKPacket = IntPtr.Zero;
			CurrentPacketTime -= num;
			while (packetQueue.Count > 4)
			{
				packetQueue.Dequeue();
			}
		}
	}

	private void UpdateFromUnityPacket(IntPtr sdkAvatar)
	{
		if (!isStreaming && packetQueue.Count > 1)
		{
			currentPacket = packetQueue.Dequeue();
			isStreaming = true;
		}
		if (!isStreaming)
		{
			return;
		}
		CurrentPacketTime += Time.deltaTime;
		while (CurrentPacketTime > currentPacket.Duration)
		{
			if (packetQueue.Count == 0)
			{
				CurrentPose = currentPacket.FinalFrame;
				CurrentPacketTime = 0f;
				currentPacket = null;
				isStreaming = false;
				return;
			}
			while (packetQueue.Count > 4)
			{
				packetQueue.Dequeue();
			}
			CurrentPacketTime -= currentPacket.Duration;
			currentPacket = packetQueue.Dequeue();
		}
		CurrentPose = currentPacket.GetPoseFrame(CurrentPacketTime);
		UpdateTransformsFromPose(sdkAvatar);
	}
}
