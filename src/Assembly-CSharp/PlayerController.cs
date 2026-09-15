using Oculus.Platform;
using UnityEngine;

public class PlayerController : PlatformManager
{
	public Camera spyCamera;

	private GameObject cameraRig;

	private bool showUI = true;

	public override void Awake()
	{
		base.Awake();
		cameraRig = localPlayerHead.gameObject;
	}

	public override void Start()
	{
		OVRManager.instance.trackingOriginType = OVRManager.TrackingOrigin.EyeLevel;
		base.Start();
		spyCamera.enabled = false;
	}

	public override void Update()
	{
		base.Update();
		checkInput();
	}

	private void checkInput()
	{
		if (UnityEngine.Application.platform == RuntimePlatform.Android)
		{
			if (OVRInput.GetDown(OVRInput.Button.Back))
			{
				Rooms.LaunchInvitableUserFlow(roomManager.roomID);
			}
			if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad))
			{
				ToggleCamera();
			}
			if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
			{
				ToggleUI();
			}
		}
		else
		{
			if (OVRInput.GetDown(OVRInput.Button.Three))
			{
				Rooms.LaunchInvitableUserFlow(roomManager.roomID);
			}
			if (OVRInput.GetDown(OVRInput.Button.Four))
			{
				ToggleCamera();
			}
			if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
			{
				ToggleUI();
			}
		}
	}

	private void ToggleCamera()
	{
		spyCamera.enabled = !spyCamera.enabled;
		localAvatar.ShowThirdPerson = !localAvatar.ShowThirdPerson;
		cameraRig.SetActive(!cameraRig.activeSelf);
	}

	private void ToggleUI()
	{
		showUI = !showUI;
		helpPanel.SetActive(showUI);
		localAvatar.ShowLeftController(showUI);
	}
}
