using System;
using System.Collections.Generic;
using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
	public enum State
	{
		INITIALIZING,
		CHECKING_LAUNCH_STATE,
		CREATING_A_ROOM,
		WAITING_IN_A_ROOM,
		JOINING_A_ROOM,
		CONNECTED_IN_A_ROOM,
		LEAVING_A_ROOM,
		SHUTDOWN
	}

	private static readonly Vector3 START_ROTATION_ONE = new Vector3(0f, 180f, 0f);

	private static readonly Vector3 START_POSITION_ONE = new Vector3(0f, 2f, 5f);

	private static readonly Vector3 START_ROTATION_TWO = new Vector3(0f, 0f, 0f);

	private static readonly Vector3 START_POSITION_TWO = new Vector3(0f, 2f, -5f);

	private static readonly Vector3 START_ROTATION_THREE = new Vector3(0f, 270f, 0f);

	private static readonly Vector3 START_POSITION_THREE = new Vector3(5f, 2f, 0f);

	private static readonly Vector3 START_ROTATION_FOUR = new Vector3(0f, 90f, 0f);

	private static readonly Vector3 START_POSITION_FOUR = new Vector3(-5f, 2f, 0f);

	private static readonly Color BLACK = new Color(0f, 0f, 0f);

	private static readonly Color WHITE = new Color(1f, 1f, 1f);

	private static readonly Color CYAN = new Color(0f, 1f, 1f);

	private static readonly Color BLUE = new Color(0f, 0f, 1f);

	private static readonly Color GREEN = new Color(0f, 1f, 0f);

	public Oculus.Platform.CAPI.FilterCallback micFilterDelegate = MicFilter;

	private uint packetSequence;

	public OvrAvatar localAvatarPrefab;

	public OvrAvatar remoteAvatarPrefab;

	public GameObject helpPanel;

	protected MeshRenderer helpMesh;

	public Material riftMaterial;

	public Material gearMaterial;

	protected OvrAvatar localAvatar;

	protected GameObject localTrackingSpace;

	protected GameObject localPlayerHead;

	protected Dictionary<ulong, RemotePlayer> remoteUsers = new Dictionary<ulong, RemotePlayer>();

	public GameObject roomSphere;

	protected MeshRenderer sphereMesh;

	public GameObject roomFloor;

	protected MeshRenderer floorMesh;

	protected State currentState;

	protected static PlatformManager s_instance = null;

	protected RoomManager roomManager;

	protected P2PManager p2pManager;

	protected VoipManager voipManager;

	protected ulong myID;

	protected string myOculusID;

	public static State CurrentState => s_instance.currentState;

	public static ulong MyID
	{
		get
		{
			if (s_instance != null)
			{
				return s_instance.myID;
			}
			return 0uL;
		}
	}

	public static string MyOculusID
	{
		get
		{
			if (s_instance != null && s_instance.myOculusID != null)
			{
				return s_instance.myOculusID;
			}
			return string.Empty;
		}
	}

	public virtual void Update()
	{
		p2pManager.GetRemotePackets();
	}

	public virtual void Awake()
	{
		LogOutputLine("Start Log.");
		helpMesh = helpPanel.GetComponent<MeshRenderer>();
		sphereMesh = roomSphere.GetComponent<MeshRenderer>();
		floorMesh = roomFloor.GetComponent<MeshRenderer>();
		localTrackingSpace = base.transform.Find("OVRCameraRig/TrackingSpace").gameObject;
		localPlayerHead = base.transform.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor").gameObject;
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		TransitionToState(State.INITIALIZING);
		Core.Initialize();
		roomManager = new RoomManager();
		p2pManager = new P2PManager();
		voipManager = new VoipManager();
	}

	public virtual void Start()
	{
		Entitlements.IsUserEntitledToApplication().OnComplete(IsEntitledCallback);
		Request.RunCallbacks();
	}

	private void IsEntitledCallback(Message msg)
	{
		if (msg.IsError)
		{
			TerminateWithError(msg);
			return;
		}
		Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
		Request.RunCallbacks();
	}

	private void GetLoggedInUserCallback(Message<User> msg)
	{
		if (msg.IsError)
		{
			TerminateWithError(msg);
			return;
		}
		myID = msg.Data.ID;
		myOculusID = msg.Data.OculusID;
		localAvatar = UnityEngine.Object.Instantiate(localAvatarPrefab);
		localTrackingSpace = base.transform.Find("OVRCameraRig/TrackingSpace").gameObject;
		localAvatar.transform.SetParent(localTrackingSpace.transform, false);
		localAvatar.transform.localPosition = new Vector3(0f, 0f, 0f);
		localAvatar.transform.localRotation = Quaternion.identity;
		if (UnityEngine.Application.platform == RuntimePlatform.Android)
		{
			helpPanel.transform.SetParent(localAvatar.transform.Find("body"), false);
			helpPanel.transform.localPosition = new Vector3(0f, 0f, 1f);
			helpMesh.material = gearMaterial;
		}
		else
		{
			helpPanel.transform.SetParent(localAvatar.transform.Find("hand_left"), false);
			helpPanel.transform.localPosition = new Vector3(0f, 0.2f, 0.2f);
			helpMesh.material = riftMaterial;
		}
		localAvatar.oculusUserID = myID;
		localAvatar.RecordPackets = true;
		OvrAvatar ovrAvatar = localAvatar;
		ovrAvatar.PacketRecorded = (EventHandler<OvrAvatar.PacketEventArgs>)Delegate.Combine(ovrAvatar.PacketRecorded, new EventHandler<OvrAvatar.PacketEventArgs>(OnLocalAvatarPacketRecorded));
		Quaternion identity = Quaternion.identity;
		switch (UnityEngine.Random.Range(0, 4))
		{
		case 0:
			identity.eulerAngles = START_ROTATION_ONE;
			base.transform.localPosition = START_POSITION_ONE;
			base.transform.localRotation = identity;
			break;
		case 1:
			identity.eulerAngles = START_ROTATION_TWO;
			base.transform.localPosition = START_POSITION_TWO;
			base.transform.localRotation = identity;
			break;
		case 2:
			identity.eulerAngles = START_ROTATION_THREE;
			base.transform.localPosition = START_POSITION_THREE;
			base.transform.localRotation = identity;
			break;
		default:
			identity.eulerAngles = START_ROTATION_FOUR;
			base.transform.localPosition = START_POSITION_FOUR;
			base.transform.localRotation = identity;
			break;
		}
		TransitionToState(State.CHECKING_LAUNCH_STATE);
		if (!roomManager.CheckForInvite())
		{
			roomManager.CreateRoom();
			TransitionToState(State.CREATING_A_ROOM);
		}
		Voip.SetMicrophoneFilterCallback(micFilterDelegate);
	}

	public void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
	{
		uint num = Oculus.Avatar.CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
		byte[] array = new byte[num];
		Oculus.Avatar.CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, num, array);
		foreach (KeyValuePair<ulong, RemotePlayer> remoteUser in remoteUsers)
		{
			LogOutputLine("Sending Packet to  " + remoteUser.Key);
			p2pManager.SendAvatarUpdate(remoteUser.Key, base.transform, packetSequence, array);
		}
		packetSequence++;
	}

	public void OnApplicationQuit()
	{
		roomManager.LeaveCurrentRoom();
		foreach (KeyValuePair<ulong, RemotePlayer> remoteUser in remoteUsers)
		{
			p2pManager.Disconnect(remoteUser.Key);
			voipManager.Disconnect(remoteUser.Key);
		}
		LogOutputLine("End Log.");
	}

	public void AddUser(ulong userID, ref RemotePlayer remoteUser)
	{
		remoteUsers.Add(userID, remoteUser);
	}

	public void LogOutputLine(string line)
	{
		Debug.Log(Time.time + ": " + line);
	}

	public static void TerminateWithError(Message msg)
	{
		s_instance.LogOutputLine("Error: " + msg.GetError().Message);
		UnityEngine.Application.Quit();
	}

	public static void TransitionToState(State newState)
	{
		if ((bool)s_instance)
		{
			s_instance.LogOutputLine(string.Concat("State ", s_instance.currentState, " -> ", newState));
		}
		if ((bool)s_instance && s_instance.currentState != newState)
		{
			s_instance.currentState = newState;
			if (newState == State.SHUTDOWN)
			{
				s_instance.OnApplicationQuit();
			}
		}
		SetSphereColorForState();
	}

	private static void SetSphereColorForState()
	{
		switch (s_instance.currentState)
		{
		case State.INITIALIZING:
		case State.SHUTDOWN:
			s_instance.sphereMesh.material.color = BLACK;
			break;
		case State.WAITING_IN_A_ROOM:
			s_instance.sphereMesh.material.color = WHITE;
			break;
		case State.CONNECTED_IN_A_ROOM:
			s_instance.sphereMesh.material.color = CYAN;
			break;
		case State.CHECKING_LAUNCH_STATE:
		case State.CREATING_A_ROOM:
		case State.JOINING_A_ROOM:
		case State.LEAVING_A_ROOM:
			break;
		}
	}

	public static void SetFloorColorForState(bool host)
	{
		if (host)
		{
			s_instance.floorMesh.material.color = BLUE;
		}
		else
		{
			s_instance.floorMesh.material.color = GREEN;
		}
	}

	public static void MarkAllRemoteUsersAsNotInRoom()
	{
		foreach (KeyValuePair<ulong, RemotePlayer> remoteUser in s_instance.remoteUsers)
		{
			remoteUser.Value.stillInRoom = false;
		}
	}

	public static void MarkRemoteUserInRoom(ulong userID)
	{
		RemotePlayer value = new RemotePlayer();
		if (s_instance.remoteUsers.TryGetValue(userID, out value))
		{
			value.stillInRoom = true;
		}
	}

	public static void ForgetRemoteUsersNotInRoom()
	{
		List<ulong> list = new List<ulong>();
		foreach (KeyValuePair<ulong, RemotePlayer> remoteUser in s_instance.remoteUsers)
		{
			if (!remoteUser.Value.stillInRoom)
			{
				list.Add(remoteUser.Key);
			}
		}
		foreach (ulong item in list)
		{
			RemoveRemoteUser(item);
		}
	}

	public static void LogOutput(string line)
	{
		s_instance.LogOutputLine(Time.time + ": " + line);
	}

	public static bool IsUserInRoom(ulong userID)
	{
		return s_instance.remoteUsers.ContainsKey(userID);
	}

	public static void AddRemoteUser(ulong userID)
	{
		RemotePlayer remoteUser = new RemotePlayer();
		remoteUser.RemoteAvatar = UnityEngine.Object.Instantiate(s_instance.remoteAvatarPrefab);
		remoteUser.RemoteAvatar.oculusUserID = userID;
		remoteUser.RemoteAvatar.ShowThirdPerson = true;
		remoteUser.p2pConnectionState = PeerConnectionState.Unknown;
		remoteUser.voipConnectionState = PeerConnectionState.Unknown;
		remoteUser.stillInRoom = true;
		remoteUser.remoteUserID = userID;
		s_instance.AddUser(userID, ref remoteUser);
		s_instance.p2pManager.ConnectTo(userID);
		s_instance.voipManager.ConnectTo(userID);
		VoipAudioSourceHiLevel voipAudioSourceHiLevel = remoteUser.RemoteAvatar.gameObject.AddComponent<VoipAudioSourceHiLevel>();
		voipAudioSourceHiLevel.senderID = userID;
		s_instance.LogOutputLine("Adding User " + userID);
	}

	public static void RemoveRemoteUser(ulong userID)
	{
		RemotePlayer value = new RemotePlayer();
		if (s_instance.remoteUsers.TryGetValue(userID, out value))
		{
			UnityEngine.Object.Destroy(value.RemoteAvatar.GetComponent<VoipAudioSourceHiLevel>(), 0f);
			UnityEngine.Object.Destroy(value.RemoteAvatar.gameObject, 0f);
			s_instance.remoteUsers.Remove(userID);
			s_instance.LogOutputLine("Removing User " + userID);
		}
	}

	public static void MicFilter(short[] pcmData, UIntPtr pcmDataLength, int frequency, int numChannels)
	{
		float[] array = new float[pcmData.Length];
		for (int i = 0; i < pcmData.Length; i++)
		{
			array[i] = (float)pcmData[i] / 32767f;
		}
		s_instance.localAvatar.UpdateVoiceVisualization(array);
	}

	public static RemotePlayer GetRemoteUser(ulong userID)
	{
		RemotePlayer value = new RemotePlayer();
		if (s_instance.remoteUsers.TryGetValue(userID, out value))
		{
			return value;
		}
		return null;
	}
}
