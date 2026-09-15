using System;
using System.Collections.Generic;
using System.IO;
using Oculus.Avatar;
using UnityEngine;

public class RemoteLoopbackManager : MonoBehaviour
{
	private class PacketLatencyPair
	{
		public byte[] PacketData;

		public float FakeLatency;
	}

	[Serializable]
	public class SimulatedLatencySettings
	{
		[Range(0f, 0.5f)]
		public float FakeLatencyMax = 0.25f;

		[Range(0f, 0.5f)]
		public float FakeLatencyMin = 0.002f;

		[Range(0f, 1f)]
		public float LatencyWeight = 0.25f;

		[Range(0f, 10f)]
		public int MaxSamples = 4;

		internal float AverageWindow;

		internal float LatencySum;

		internal LinkedList<float> LatencyValues = new LinkedList<float>();

		public float NextValue()
		{
			AverageWindow = LatencySum / (float)LatencyValues.Count;
			float num = UnityEngine.Random.Range(FakeLatencyMin, FakeLatencyMax);
			float num2 = AverageWindow * (1f - LatencyWeight) + LatencyWeight * num;
			if (LatencyValues.Count >= MaxSamples)
			{
				LatencySum -= LatencyValues.First.Value;
				LatencyValues.RemoveFirst();
			}
			LatencySum += num2;
			LatencyValues.AddLast(num2);
			return num2;
		}
	}

	public OvrAvatar LocalAvatar;

	public OvrAvatar LoopbackAvatar;

	public SimulatedLatencySettings LatencySettings = new SimulatedLatencySettings();

	private int PacketSequence;

	private LinkedList<PacketLatencyPair> packetQueue = new LinkedList<PacketLatencyPair>();

	private void Start()
	{
		LocalAvatar.RecordPackets = true;
		OvrAvatar localAvatar = LocalAvatar;
		localAvatar.PacketRecorded = (EventHandler<OvrAvatar.PacketEventArgs>)Delegate.Combine(localAvatar.PacketRecorded, new EventHandler<OvrAvatar.PacketEventArgs>(OnLocalAvatarPacketRecorded));
		float num = UnityEngine.Random.Range(LatencySettings.FakeLatencyMin, LatencySettings.FakeLatencyMax);
		LatencySettings.LatencyValues.AddFirst(num);
		LatencySettings.LatencySum += num;
	}

	private void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			if (LocalAvatar.UseSDKPackets)
			{
				uint num = CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
				byte[] buffer = new byte[num];
				CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, num, buffer);
				binaryWriter.Write(PacketSequence++);
				binaryWriter.Write(num);
				binaryWriter.Write(buffer);
			}
			else
			{
				binaryWriter.Write(PacketSequence++);
				args.Packet.Write(memoryStream);
			}
			SendPacketData(memoryStream.ToArray());
		}
	}

	private void Update()
	{
		if (packetQueue.Count <= 0)
		{
			return;
		}
		List<PacketLatencyPair> list = new List<PacketLatencyPair>();
		foreach (PacketLatencyPair item in packetQueue)
		{
			item.FakeLatency -= Time.deltaTime;
			if (item.FakeLatency < 0f)
			{
				ReceivePacketData(item.PacketData);
				list.Add(item);
			}
		}
		foreach (PacketLatencyPair item2 in list)
		{
			packetQueue.Remove(item2);
		}
	}

	private void SendPacketData(byte[] data)
	{
		PacketLatencyPair packetLatencyPair = new PacketLatencyPair();
		packetLatencyPair.PacketData = data;
		packetLatencyPair.FakeLatency = LatencySettings.NextValue();
		packetQueue.AddLast(packetLatencyPair);
	}

	private void ReceivePacketData(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream(data))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			int sequence = binaryReader.ReadInt32();
			OvrAvatarPacket packet;
			if (LoopbackAvatar.UseSDKPackets)
			{
				int count = binaryReader.ReadInt32();
				byte[] buffer = binaryReader.ReadBytes(count);
				IntPtr ovrNativePacket = CAPI.ovrAvatarPacket_Read((uint)data.Length, buffer);
				OvrAvatarPacket ovrAvatarPacket = new OvrAvatarPacket();
				ovrAvatarPacket.ovrNativePacket = ovrNativePacket;
				packet = ovrAvatarPacket;
			}
			else
			{
				packet = OvrAvatarPacket.Read(memoryStream);
			}
			LoopbackAvatar.GetComponent<OvrAvatarRemoteDriver>().QueuePacket(sequence, packet);
		}
	}
}
