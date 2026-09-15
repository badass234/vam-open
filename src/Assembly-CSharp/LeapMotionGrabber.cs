using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public class LeapMotionGrabber : MonoBehaviour
{
	public PinchDetector pinchDetector;

	private FreeControllerV3 grabbedController;

	private Rigidbody rb;

	private GpuGrabSphere grabSphere;

	private bool _controllerGrabOn = true;

	public bool controllerGrabOn
	{
		get
		{
			return _controllerGrabOn;
		}
		set
		{
			if (_controllerGrabOn != value)
			{
				_controllerGrabOn = value;
				if (!_controllerGrabOn)
				{
					ReleaseController();
				}
			}
		}
	}

	public void ReleaseController()
	{
		if (grabbedController != null)
		{
			if (grabbedController.linkToRB == rb)
			{
				grabbedController.RestorePreLinkState();
			}
			grabbedController = null;
		}
	}

	private void Update()
	{
		if (!(rb != null) || !(pinchDetector != null))
		{
			return;
		}
		if (pinchDetector.DidStartPinch)
		{
			if (grabSphere != null)
			{
				grabSphere.enabled = true;
			}
			if (grabbedController == null && _controllerGrabOn)
			{
				List<FreeControllerV3> overlappingTargets = SuperController.singleton.GetOverlappingTargets(base.transform, 0.02f);
				if (overlappingTargets.Count > 0)
				{
					foreach (FreeControllerV3 item in overlappingTargets)
					{
						bool flag = true;
						FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
						if (item.canGrabPosition)
						{
							if (item.canGrabRotation)
							{
								linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
							}
						}
						else if (item.canGrabRotation)
						{
							linkState = FreeControllerV3.SelectLinkState.Rotation;
						}
						else
						{
							flag = false;
						}
						if (!flag)
						{
							continue;
						}
						if (item.linkToRB != null)
						{
							LeapMotionGrabber component = item.linkToRB.GetComponent<LeapMotionGrabber>();
							if (component != null)
							{
								component.ReleaseController();
							}
						}
						grabbedController = item;
						grabbedController.SelectLinkToRigidbody(rb, linkState);
						break;
					}
				}
			}
		}
		if (pinchDetector.DidEndPinch)
		{
			if (grabSphere != null)
			{
				grabSphere.enabled = false;
			}
			ReleaseController();
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		grabSphere = GetComponent<GpuGrabSphere>();
	}

	private void OnDisable()
	{
		ReleaseController();
	}
}
