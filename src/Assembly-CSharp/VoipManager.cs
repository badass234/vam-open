using Oculus.Platform;
using Oculus.Platform.Models;

public class VoipManager
{
	public VoipManager()
	{
		Voip.SetVoipConnectRequestCallback(VoipConnectRequestCallback);
		Voip.SetVoipStateChangeCallback(VoipStateChangedCallback);
	}

	public void ConnectTo(ulong userID)
	{
		if (PlatformManager.MyID < userID)
		{
			Voip.Start(userID);
			PlatformManager.LogOutput("Voip connect to " + userID);
		}
	}

	public void Disconnect(ulong userID)
	{
		if (userID != 0)
		{
			Voip.Stop(userID);
			RemotePlayer remoteUser = PlatformManager.GetRemoteUser(userID);
			if (remoteUser != null)
			{
				remoteUser.voipConnectionState = PeerConnectionState.Unknown;
			}
		}
	}

	private void VoipConnectRequestCallback(Message<NetworkingPeer> msg)
	{
		PlatformManager.LogOutput("Voip request from " + msg.Data.ID);
		RemotePlayer remoteUser = PlatformManager.GetRemoteUser(msg.Data.ID);
		if (remoteUser != null)
		{
			PlatformManager.LogOutput("Voip request accepted from " + msg.Data.ID);
			Voip.Accept(msg.Data.ID);
		}
	}

	private void VoipStateChangedCallback(Message<NetworkingPeer> msg)
	{
		PlatformManager.LogOutput("Voip state to " + msg.Data.ID + " changed to  " + msg.Data.State);
		RemotePlayer remoteUser = PlatformManager.GetRemoteUser(msg.Data.ID);
		if (remoteUser != null)
		{
			remoteUser.voipConnectionState = msg.Data.State;
			if (msg.Data.State == PeerConnectionState.Timeout && PlatformManager.MyID < msg.Data.ID)
			{
				Voip.Start(msg.Data.ID);
				PlatformManager.LogOutput("Voip re-connect to " + msg.Data.ID);
			}
		}
	}
}
