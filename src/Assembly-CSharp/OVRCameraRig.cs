using System;
using UnityEngine;
using UnityEngine.XR;

[ExecuteInEditMode]
public class OVRCameraRig : MonoBehaviour
{
	public bool usePerEyeCameras;

	public bool useFixedUpdateForTracking;

	protected bool _skipUpdate;

	protected readonly string trackingSpaceName = "TrackingSpace";

	protected readonly string trackerAnchorName = "TrackerAnchor";

	protected readonly string leftEyeAnchorName = "LeftEyeAnchor";

	protected readonly string centerEyeAnchorName = "CenterEyeAnchor";

	protected readonly string rightEyeAnchorName = "RightEyeAnchor";

	protected readonly string leftHandAnchorName = "LeftHandAnchor";

	protected readonly string rightHandAnchorName = "RightHandAnchor";

	protected Camera _centerEyeCamera;

	protected Camera _leftEyeCamera;

	protected Camera _rightEyeCamera;

	public Camera leftEyeCamera => (!usePerEyeCameras) ? _centerEyeCamera : _leftEyeCamera;

	public Camera rightEyeCamera => (!usePerEyeCameras) ? _centerEyeCamera : _rightEyeCamera;

	public Transform trackingSpace { get; private set; }

	public Transform leftEyeAnchor { get; private set; }

	public Transform centerEyeAnchor { get; private set; }

	public Transform rightEyeAnchor { get; private set; }

	public Transform leftHandAnchor { get; private set; }

	public Transform rightHandAnchor { get; private set; }

	public Transform trackerAnchor { get; private set; }

	public event Action<OVRCameraRig> UpdatedAnchors;

	protected virtual void Awake()
	{
		_skipUpdate = true;
		EnsureGameObjectIntegrity();
	}

	protected virtual void Start()
	{
		UpdateAnchors();
	}

	protected virtual void FixedUpdate()
	{
		if (useFixedUpdateForTracking)
		{
			UpdateAnchors();
		}
	}

	protected virtual void Update()
	{
		_skipUpdate = false;
		if (!useFixedUpdateForTracking)
		{
			UpdateAnchors();
		}
	}

	protected virtual void UpdateAnchors()
	{
		EnsureGameObjectIntegrity();
		if (Application.isPlaying)
		{
			if (_skipUpdate)
			{
				centerEyeAnchor.FromOVRPose(OVRPose.identity, true);
				leftEyeAnchor.FromOVRPose(OVRPose.identity, true);
				rightEyeAnchor.FromOVRPose(OVRPose.identity, true);
				return;
			}
			bool monoscopic = OVRManager.instance.monoscopic;
			OVRPose pose = OVRManager.tracker.GetPose();
			trackerAnchor.localRotation = pose.orientation;
			centerEyeAnchor.localRotation = InputTracking.GetLocalRotation(XRNode.CenterEye);
			leftEyeAnchor.localRotation = ((!monoscopic) ? InputTracking.GetLocalRotation(XRNode.LeftEye) : centerEyeAnchor.localRotation);
			rightEyeAnchor.localRotation = ((!monoscopic) ? InputTracking.GetLocalRotation(XRNode.RightEye) : centerEyeAnchor.localRotation);
			leftHandAnchor.gameObject.SetActive(OVRInput.IsControllerConnected(OVRInput.Controller.LTouch));
			rightHandAnchor.gameObject.SetActive(OVRInput.IsControllerConnected(OVRInput.Controller.RTouch));
			leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
			rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
			trackerAnchor.localPosition = pose.position;
			centerEyeAnchor.localPosition = InputTracking.GetLocalPosition(XRNode.CenterEye);
			leftEyeAnchor.localPosition = ((!monoscopic) ? InputTracking.GetLocalPosition(XRNode.LeftEye) : centerEyeAnchor.localPosition);
			rightEyeAnchor.localPosition = ((!monoscopic) ? InputTracking.GetLocalPosition(XRNode.RightEye) : centerEyeAnchor.localPosition);
			leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
			rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
			RaiseUpdatedAnchorsEvent();
		}
	}

	protected virtual void RaiseUpdatedAnchorsEvent()
	{
		if (this.UpdatedAnchors != null)
		{
			this.UpdatedAnchors(this);
		}
	}

	public virtual void EnsureGameObjectIntegrity()
	{
		if (trackingSpace == null)
		{
			trackingSpace = ConfigureAnchor(null, trackingSpaceName);
		}
		if (leftEyeAnchor == null)
		{
			leftEyeAnchor = ConfigureAnchor(trackingSpace, leftEyeAnchorName);
		}
		if (centerEyeAnchor == null)
		{
			centerEyeAnchor = ConfigureAnchor(trackingSpace, centerEyeAnchorName);
		}
		if (rightEyeAnchor == null)
		{
			rightEyeAnchor = ConfigureAnchor(trackingSpace, rightEyeAnchorName);
		}
		if (leftHandAnchor == null)
		{
			leftHandAnchor = ConfigureAnchor(trackingSpace, leftHandAnchorName);
		}
		if (rightHandAnchor == null)
		{
			rightHandAnchor = ConfigureAnchor(trackingSpace, rightHandAnchorName);
		}
		if (trackerAnchor == null)
		{
			trackerAnchor = ConfigureAnchor(trackingSpace, trackerAnchorName);
		}
		if (_centerEyeCamera == null || _leftEyeCamera == null || _rightEyeCamera == null)
		{
			_centerEyeCamera = centerEyeAnchor.GetComponent<Camera>();
			_leftEyeCamera = leftEyeAnchor.GetComponent<Camera>();
			_rightEyeCamera = rightEyeAnchor.GetComponent<Camera>();
			if (_centerEyeCamera == null)
			{
				_centerEyeCamera = centerEyeAnchor.gameObject.AddComponent<Camera>();
				_centerEyeCamera.tag = "MainCamera";
			}
			if (_leftEyeCamera == null)
			{
				_leftEyeCamera = leftEyeAnchor.gameObject.AddComponent<Camera>();
				_leftEyeCamera.tag = "MainCamera";
			}
			if (_rightEyeCamera == null)
			{
				_rightEyeCamera = rightEyeAnchor.gameObject.AddComponent<Camera>();
				_rightEyeCamera.tag = "MainCamera";
			}
			_centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Both;
			_leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
			_rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;
		}
		if (_centerEyeCamera.enabled == usePerEyeCameras || _leftEyeCamera.enabled == !usePerEyeCameras || _rightEyeCamera.enabled == !usePerEyeCameras)
		{
			_skipUpdate = true;
		}
		_centerEyeCamera.enabled = !usePerEyeCameras;
		_leftEyeCamera.enabled = usePerEyeCameras;
		_rightEyeCamera.enabled = usePerEyeCameras;
	}

	protected virtual Transform ConfigureAnchor(Transform root, string name)
	{
		Transform transform = ((!(root != null)) ? null : base.transform.Find(root.name + "/" + name));
		if (transform == null)
		{
			transform = base.transform.Find(name);
		}
		if (transform == null)
		{
			transform = new GameObject(name).transform;
		}
		transform.name = name;
		transform.parent = ((!(root != null)) ? base.transform : root);
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	public virtual Matrix4x4 ComputeTrackReferenceMatrix()
	{
		if (centerEyeAnchor == null)
		{
			Debug.LogError("centerEyeAnchor is required");
			return Matrix4x4.identity;
		}
		OVRPose oVRPose = default(OVRPose);
		oVRPose.position = InputTracking.GetLocalPosition(XRNode.Head);
		oVRPose.orientation = InputTracking.GetLocalRotation(XRNode.Head);
		OVRPose oVRPose2 = oVRPose.Inverse();
		Matrix4x4 matrix4x = Matrix4x4.TRS(oVRPose2.position, oVRPose2.orientation, Vector3.one);
		return centerEyeAnchor.localToWorldMatrix * matrix4x;
	}
}
