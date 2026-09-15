using System;
using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class P2PManager
{
	private static readonly byte UPDATE_PACKET = 1;

	private static readonly int POSITION_DATA_LENGTH = 41;

	private static readonly float HEIGHT_OFFSET = 0.65f;

	public P2PManager()
	{
		Net.SetPeerConnectRequestCallback(PeerConnectRequestCallback);
		Net.SetConnectionStateChangedCallback(ConnectionStateChangedCallback);
	}

	public void ConnectTo(ulong userID)
	{
		if (PlatformManager.MyID < userID)
		{
			Net.Connect(userID);
			PlatformManager.LogOutput("P2P connect to " + userID);
		}
	}

	public void Disconnect(ulong userID)
	{
		if (userID != 0)
		{
			Net.Close(userID);
			RemotePlayer remoteUser = PlatformManager.GetRemoteUser(userID);
			if (remoteUser != null)
			{
				remoteUser.p2pConnectionState = PeerConnectionState.Unknown;
			}
		}
	}

	private void PeerConnectRequestCallback(Message<NetworkingPeer> msg)
	{
		PlatformManager.LogOutput("P2P request from " + msg.Data.ID);
		RemotePlayer remoteUser = PlatformManager.GetRemoteUser(msg.Data.ID);
		if (remoteUser != null)
		{
			PlatformManager.LogOutput("P2P request accepted from " + msg.Data.ID);
			Net.Accept(msg.Data.ID);
		}
	}

	private void ConnectionStateChangedCallback(Message<NetworkingPeer> msg)
	{
		PlatformManager.LogOutput("P2P state to " + msg.Data.ID + " changed to  " + msg.Data.State);
		RemotePlayer remoteUser = PlatformManager.GetRemoteUser(msg.Data.ID);
		if (remoteUser != null)
		{
			remoteUser.p2pConnectionState = msg.Data.State;
			if (msg.Data.State == PeerConnectionState.Timeout && PlatformManager.MyID < msg.Data.ID)
			{
				Net.Connect(msg.Data.ID);
				PlatformManager.LogOutput("P2P re-connect to " + msg.Data.ID);
			}
		}
	}

	public void SendAvatarUpdate(ulong userID, Transform bodyTransform, uint sequence, byte[] avatarPacket)
	{
		byte[] array = new byte[avatarPacket.Length + POSITION_DATA_LENGTH];
		array[0] = UPDATE_PACKET;
		int offset = 1;
		PackULong(PlatformManager.MyID, array, ref offset);
		PackFloat(bodyTransform.localPosition.x, array, ref offset);
		PackFloat(bodyTransform.localPosition.y, array, ref offset);
		PackFloat(bodyTransform.localPosition.z, array, ref offset);
		PackFloat(bodyTransform.localRotation.x, array, ref offset);
		PackFloat(bodyTransform.localRotation.y, array, ref offset);
		PackFloat(bodyTransform.localRotation.z, array, ref offset);
		PackFloat(bodyTransform.localRotation.w, array, ref offset);
		PackUInt32(sequence, array, ref offset);
		Buffer.BlockCopy(avatarPacket, 0, array, offset, avatarPacket.Length);
		Net.SendPacket(userID, array, SendPolicy.Unreliable);
	}

	private void PackFloat(float f, byte[] buf, ref int offset)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(f), 0, buf, offset, 4);
		offset += 4;
	}

	private void PackULong(ulong u, byte[] buf, ref int offset)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(u), 0, buf, offset, 8);
		offset += 8;
	}

	private void PackUInt32(uint u, byte[] buf, ref int offset)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(u), 0, buf, offset, 4);
		offset += 4;
	}

	public void GetRemotePackets()
	{
		Packet packet;
		while ((packet = Net.ReadPacket()) != null)
		{
			byte[] packet2 = new byte[packet.Size];
			packet.ReadBytes(packet2);
			if (packet2[0] != UPDATE_PACKET)
			{
				PlatformManager.LogOutput("Invalid packet type: " + packet.Size);
			}
			else
			{
				processAvatarPacket(ref packet2);
			}
		}
	}

	public void processAvatarPacket(ref byte[] packet)
	{
		ulong num = 0uL;
		num = BitConverter.ToUInt64(packet, 1);
		RemotePlayer remoteUser = PlatformManager.GetRemoteUser(num);
		if (remoteUser != null)
		{
			remoteUser.receivedBodyPositionPrior = remoteUser.receivedBodyPosition;
			remoteUser.receivedBodyPosition.x = BitConverter.ToSingle(packet, 9);
			remoteUser.receivedBodyPosition.y = BitConverter.ToSingle(packet, 13) + HEIGHT_OFFSET;
			remoteUser.receivedBodyPosition.z = BitConverter.ToSingle(packet, 17);
			remoteUser.receivedBodyRotationPrior = remoteUser.receivedBodyRotation;
			remoteUser.receivedBodyRotation.x = BitConverter.ToSingle(packet, 21);
			remoteUser.receivedBodyRotation.y = BitConverter.ToSingle(packet, 25);
			remoteUser.receivedBodyRotation.z = BitConverter.ToSingle(packet, 29);
			remoteUser.receivedBodyRotation.w = BitConverter.ToSingle(packet, 33);
			int sequence = BitConverter.ToInt32(packet, 37);
			byte[] array = new byte[packet.Length - POSITION_DATA_LENGTH];
			Buffer.BlockCopy(packet, POSITION_DATA_LENGTH, array, 0, array.Length);
			IntPtr ovrNativePacket = Oculus.Avatar.CAPI.ovrAvatarPacket_Read((uint)array.Length, array);
			remoteUser.RemoteAvatar.GetComponent<OvrAvatarRemoteDriver>().QueuePacket(sequence, new OvrAvatarPacket
			{
				ovrNativePacket = ovrNativePacket
			});
			remoteUser.RemoteAvatar.transform.localPosition = remoteUser.receivedBodyPosition;
			remoteUser.RemoteAvatar.transform.localRotation = remoteUser.receivedBodyRotation;
		}
	}
}
