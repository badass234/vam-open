using System;
using Oculus.Platform;
using Oculus.Platform.Models;

public class RoomManager
{
	public ulong roomID;

	private ulong invitedRoomID;

	private bool amIServer;

	private bool startupDone;

	public RoomManager()
	{
		amIServer = false;
		startupDone = false;
		Rooms.SetRoomInviteNotificationCallback(AcceptingInviteCallback);
		Rooms.SetUpdateNotificationCallback(RoomUpdateCallback);
	}

	private void AcceptingInviteCallback(Message<string> msg)
	{
		if (msg.IsError)
		{
			PlatformManager.TerminateWithError(msg);
			return;
		}
		PlatformManager.LogOutput("Launched Invite to join Room: " + msg.Data);
		invitedRoomID = Convert.ToUInt64(msg.GetString());
		if (startupDone)
		{
			CheckForInvite();
		}
	}

	public bool CheckForInvite()
	{
		startupDone = true;
		if (invitedRoomID != 0)
		{
			JoinExistingRoom(invitedRoomID);
			return true;
		}
		return false;
	}

	public void CreateRoom()
	{
		Rooms.CreateAndJoinPrivate(RoomJoinPolicy.InvitedUsers, 4u, true).OnComplete(CreateAndJoinPrivateRoomCallback);
	}

	private void CreateAndJoinPrivateRoomCallback(Message<Room> msg)
	{
		if (msg.IsError)
		{
			PlatformManager.TerminateWithError(msg);
			return;
		}
		roomID = msg.Data.ID;
		if (msg.Data.Owner.ID == PlatformManager.MyID)
		{
			amIServer = true;
		}
		else
		{
			amIServer = false;
		}
		PlatformManager.TransitionToState(PlatformManager.State.WAITING_IN_A_ROOM);
		PlatformManager.SetFloorColorForState(amIServer);
	}

	private void OnLaunchInviteWorkflowComplete(Message msg)
	{
		if (msg.IsError)
		{
			PlatformManager.TerminateWithError(msg);
		}
	}

	public void JoinExistingRoom(ulong roomID)
	{
		PlatformManager.TransitionToState(PlatformManager.State.JOINING_A_ROOM);
		Rooms.Join(roomID, true).OnComplete(JoinRoomCallback);
	}

	private void JoinRoomCallback(Message<Room> msg)
	{
		if (!msg.IsError)
		{
			PlatformManager.LogOutput("Joined Room " + msg.Data.ID + " owner: " + msg.Data.Owner.OculusID + " count: " + msg.Data.Users.Count);
			roomID = msg.Data.ID;
			ProcessRoomData(msg);
		}
	}

	private void RoomUpdateCallback(Message<Room> msg)
	{
		if (msg.IsError)
		{
			PlatformManager.TerminateWithError(msg);
			return;
		}
		PlatformManager.LogOutput("Room Update " + msg.Data.ID + " owner: " + msg.Data.Owner.OculusID + " count: " + msg.Data.Users.Count);
		ProcessRoomData(msg);
	}

	public void LeaveCurrentRoom()
	{
		if (roomID != 0)
		{
			Rooms.Leave(roomID);
			roomID = 0uL;
		}
		PlatformManager.TransitionToState(PlatformManager.State.LEAVING_A_ROOM);
	}

	private void ProcessRoomData(Message<Room> msg)
	{
		if (msg.Data.Owner.ID == PlatformManager.MyID)
		{
			amIServer = true;
		}
		else
		{
			amIServer = false;
		}
		if (msg.Data.Users.Count == 1)
		{
			PlatformManager.TransitionToState(PlatformManager.State.WAITING_IN_A_ROOM);
		}
		else
		{
			PlatformManager.TransitionToState(PlatformManager.State.CONNECTED_IN_A_ROOM);
		}
		PlatformManager.MarkAllRemoteUsersAsNotInRoom();
		foreach (User user in msg.Data.Users)
		{
			if (user.ID != PlatformManager.MyID)
			{
				if (!PlatformManager.IsUserInRoom(user.ID))
				{
					PlatformManager.AddRemoteUser(user.ID);
				}
				else
				{
					PlatformManager.MarkRemoteUserInRoom(user.ID);
				}
			}
		}
		PlatformManager.ForgetRemoteUsersNotInRoom();
		PlatformManager.SetFloorColorForState(amIServer);
	}
}
