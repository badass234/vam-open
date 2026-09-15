using System;
using UnityEngine;
using UnityEngine.XR;

public class OvrAvatarLocalDriver : OvrAvatarDriver
{
	private const float mobileBaseHeadHeight = 1.7f;

	private float voiceAmplitude;

	private ControllerPose GetMalibuControllerPose(OVRInput.Controller controller)
	{
		ovrAvatarButton ovrAvatarButton2 = (ovrAvatarButton)0;
		if (OVRInput.Get(OVRInput.Button.One, controller))
		{
			ovrAvatarButton2 |= ovrAvatarButton.One;
		}
		ControllerPose result = default(ControllerPose);
		result.buttons = ovrAvatarButton2;
		result.touches = (OVRInput.Get(OVRInput.Touch.PrimaryTouchpad) ? ovrAvatarTouch.One : ((ovrAvatarTouch)0));
		result.joystickPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, controller);
		result.indexTrigger = 0f;
		result.handTrigger = 0f;
		result.isActive = (OVRInput.GetActiveController() & controller) != 0;
		return result;
	}

	private ControllerPose GetControllerPose(OVRInput.Controller controller)
	{
		ovrAvatarButton ovrAvatarButton2 = (ovrAvatarButton)0;
		if (OVRInput.Get(OVRInput.Button.One, controller))
		{
			ovrAvatarButton2 |= ovrAvatarButton.One;
		}
		if (OVRInput.Get(OVRInput.Button.Two, controller))
		{
			ovrAvatarButton2 |= ovrAvatarButton.Two;
		}
		if (OVRInput.Get(OVRInput.Button.Start, controller))
		{
			ovrAvatarButton2 |= ovrAvatarButton.Three;
		}
		if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller))
		{
			ovrAvatarButton2 |= ovrAvatarButton.Joystick;
		}
		ovrAvatarTouch ovrAvatarTouch2 = (ovrAvatarTouch)0;
		if (OVRInput.Get(OVRInput.Touch.One, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.One;
		}
		if (OVRInput.Get(OVRInput.Touch.Two, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.Two;
		}
		if (OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.Joystick;
		}
		if (OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.ThumbRest;
		}
		if (OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.Index;
		}
		if (!OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.Pointing;
		}
		if (!OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controller))
		{
			ovrAvatarTouch2 |= ovrAvatarTouch.ThumbUp;
		}
		ControllerPose result = default(ControllerPose);
		result.buttons = ovrAvatarButton2;
		result.touches = ovrAvatarTouch2;
		result.joystickPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
		result.indexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
		result.handTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
		result.isActive = (OVRInput.GetActiveController() & controller) != 0;
		return result;
	}

	private void CalculateCurrentPose()
	{
		Vector3 localPosition = InputTracking.GetLocalPosition(XRNode.CenterEye);
		if (OvrAvatarDriver.GetIsTrackedRemote())
		{
			CurrentPose = new PoseFrame
			{
				voiceAmplitude = voiceAmplitude,
				headPosition = localPosition,
				headRotation = InputTracking.GetLocalRotation(XRNode.CenterEye),
				handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTrackedRemote),
				handLeftRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTrackedRemote),
				handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote),
				handRightRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote),
				controllerLeftPose = GetMalibuControllerPose(OVRInput.Controller.LTrackedRemote),
				controllerRightPose = GetMalibuControllerPose(OVRInput.Controller.RTrackedRemote)
			};
		}
		else
		{
			CurrentPose = new PoseFrame
			{
				voiceAmplitude = voiceAmplitude,
				headPosition = localPosition,
				headRotation = InputTracking.GetLocalRotation(XRNode.CenterEye),
				handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch),
				handLeftRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch),
				handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
				handRightRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch),
				controllerLeftPose = GetControllerPose(OVRInput.Controller.LTouch),
				controllerRightPose = GetControllerPose(OVRInput.Controller.RTouch)
			};
		}
	}

	public override void UpdateTransforms(IntPtr sdkAvatar)
	{
		CalculateCurrentPose();
		UpdateTransformsFromPose(sdkAvatar);
	}
}
