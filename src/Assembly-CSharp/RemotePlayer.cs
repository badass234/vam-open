using Oculus.Platform;
using UnityEngine;

public class RemotePlayer
{
	public ulong remoteUserID;

	public bool stillInRoom;

	public PeerConnectionState p2pConnectionState;

	public PeerConnectionState voipConnectionState;

	public OvrAvatar RemoteAvatar;

	public GameObject remotePlayerBody;

	public Vector3 receivedBodyPosition;

	public Vector3 receivedBodyPositionPrior;

	public Quaternion receivedBodyRotation;

	public Quaternion receivedBodyRotationPrior;
}
