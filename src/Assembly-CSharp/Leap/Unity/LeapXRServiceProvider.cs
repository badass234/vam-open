using System;
using Leap.Unity.Attributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Leap.Unity
{
	public class LeapXRServiceProvider : LeapServiceProvider
	{
		public enum DeviceOffsetMode
		{
			Default,
			ManualHeadOffset,
			Transform
		}

		public enum TemporalWarpingMode
		{
			Auto,
			Manual,
			Images,
			Off
		}

		private const float DEFAULT_DEVICE_OFFSET_Y_AXIS = 0f;

		private const float DEFAULT_DEVICE_OFFSET_Z_AXIS = 0.12f;

		private const float DEFAULT_DEVICE_TILT_X_AXIS = 5f;

		[Header("Advanced")]
		[Tooltip("Allow manual adjustment of the Leap device's virtual offset and tilt. These settings can be used to match the physical position and orientation of the Leap Motion sensor on a tracked device it is mounted on (such as a VR headset).  Temporal Warping not supported in Transform Mode.")]
		[SerializeField]
		[OnEditorChange("deviceOffsetMode")]
		private DeviceOffsetMode _deviceOffsetMode;

		[Tooltip("Adjusts the Leap Motion device's virtual height offset from the tracked headset position. This should match the vertical offset of the physical device with respect to the headset in meters.")]
		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float _deviceOffsetYAxis;

		[Tooltip("Adjusts the Leap Motion device's virtual depth offset from the tracked headset position. This should match the forward offset of the physical device with respect to the headset in meters.")]
		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float _deviceOffsetZAxis = 0.12f;

		[Tooltip("Adjusts the Leap Motion device's virtual X axis tilt. This should match the tilt of the physical device with respect to the headset in degrees.")]
		[SerializeField]
		[Range(-90f, 90f)]
		private float _deviceTiltXAxis = 5f;

		[Tooltip("Allows for the manual placement of the Leap Tracking Device.This device offset mode is incompatible with Temporal Warping.")]
		[SerializeField]
		private Transform _deviceOrigin;

		private const int DEFAULT_WARP_ADJUSTMENT = 17;

		[Tooltip("Temporal warping prevents the hand coordinate system from 'swimming' or 'bouncing' when the headset moves and the user's hands stay still. This phenomenon is caused by the differing amounts of latencies inherent in the two systems. For PC VR and Android VR, temporal warping should set to 'Auto', as the correct value can be chosen automatically for these platforms. Some non-standard platforms may use 'Manual' mode to adjust their latency compensation amount for temporal warping. Use 'Images' for scenarios that overlay Leap device images on tracked hand data.")]
		[SerializeField]
		private TemporalWarpingMode _temporalWarpingMode;

		[Tooltip("The time in milliseconds between the current frame's headset position and the time at which the Leap frame was captured.")]
		[SerializeField]
		private int _customWarpAdjustment = 17;

		[Tooltip("Pass updated transform matrices to hands with materials that utilize the VertexOffsetShader. Won't have any effect on hands that don't take into account shader-global vertex offsets in their material shaders.")]
		[SerializeField]
		protected bool _updateHandInPrecull;

		protected TransformHistory transformHistory = new TransformHistory();

		protected bool manualUpdateHasBeenCalledSinceUpdate;

		protected Vector3 warpedPosition = Vector3.zero;

		protected Quaternion warpedRotation = Quaternion.identity;

		protected Matrix4x4[] _transformArray = new Matrix4x4[2];

		private Pose? _trackingBaseDeltaPose;

		private Camera _cachedCamera;

		[NonSerialized]
		public long imageTimeStamp;

		public DeviceOffsetMode deviceOffsetMode
		{
			get
			{
				return _deviceOffsetMode;
			}
			set
			{
				_deviceOffsetMode = value;
				if (_deviceOffsetMode == DeviceOffsetMode.Default || _deviceOffsetMode == DeviceOffsetMode.Transform)
				{
					deviceOffsetYAxis = 0f;
					deviceOffsetZAxis = 0.12f;
					deviceTiltXAxis = 5f;
				}
				if (_deviceOffsetMode == DeviceOffsetMode.Transform && _temporalWarpingMode != TemporalWarpingMode.Off)
				{
					_temporalWarpingMode = TemporalWarpingMode.Off;
				}
			}
		}

		public float deviceOffsetYAxis
		{
			get
			{
				return _deviceOffsetYAxis;
			}
			set
			{
				_deviceOffsetYAxis = value;
			}
		}

		public float deviceOffsetZAxis
		{
			get
			{
				return _deviceOffsetZAxis;
			}
			set
			{
				_deviceOffsetZAxis = value;
			}
		}

		public float deviceTiltXAxis
		{
			get
			{
				return _deviceTiltXAxis;
			}
			set
			{
				_deviceTiltXAxis = value;
			}
		}

		public Transform deviceOrigin
		{
			get
			{
				return _deviceOrigin;
			}
			set
			{
				_deviceOrigin = value;
			}
		}

		public int warpingAdjustment
		{
			get
			{
				if (_temporalWarpingMode == TemporalWarpingMode.Manual)
				{
					return _customWarpAdjustment;
				}
				return 17;
			}
		}

		public bool updateHandInPrecull
		{
			get
			{
				return _updateHandInPrecull;
			}
			set
			{
				resetShaderTransforms();
				_updateHandInPrecull = value;
			}
		}

		private Camera cachedCamera
		{
			get
			{
				if (_cachedCamera == null)
				{
					_cachedCamera = GetComponent<Camera>();
				}
				return _cachedCamera;
			}
		}

		protected override void Reset()
		{
			base.Reset();
			editTimePose = TestHandFactory.TestHandPose.HeadMountedB;
		}

		protected virtual void OnValidate()
		{
			if (_deviceOffsetMode == DeviceOffsetMode.Transform && _temporalWarpingMode != TemporalWarpingMode.Off)
			{
				_temporalWarpingMode = TemporalWarpingMode.Off;
			}
		}

		protected virtual void OnEnable()
		{
			resetShaderTransforms();
		}

		protected virtual void OnDisable()
		{
			resetShaderTransforms();
		}

		protected override void Start()
		{
			base.Start();
			_cachedCamera = GetComponent<Camera>();
			if (_deviceOffsetMode == DeviceOffsetMode.Transform && _deviceOrigin == null)
			{
				Debug.LogError("Cannot use the Transform device offset mode without specifying a Transform to use as the device origin.", this);
				_deviceOffsetMode = DeviceOffsetMode.Default;
			}
		}

		protected override void Update()
		{
			manualUpdateHasBeenCalledSinceUpdate = false;
			base.Update();
			imageTimeStamp = _leapController.FrameTimestamp();
		}

		private void LateUpdate()
		{
			Matrix4x4 projectionMatrix = _cachedCamera.projectionMatrix;
			GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
			if (graphicsDeviceType == GraphicsDeviceType.Direct3D11 || graphicsDeviceType == GraphicsDeviceType.Direct3D12)
			{
				for (int i = 0; i < 4; i++)
				{
					projectionMatrix[1, i] = 0f - projectionMatrix[1, i];
				}
				for (int j = 0; j < 4; j++)
				{
					projectionMatrix[2, j] = projectionMatrix[2, j] * 0.5f + projectionMatrix[3, j] * 0.5f;
				}
			}
			Vector3 delayedPos;
			Quaternion delayedRot;
			transformHistory.SampleTransform(imageTimeStamp - (long)((float)warpingAdjustment * 1000f), out delayedPos, out delayedRot);
			Quaternion xRNodeCenterEyeLocalRotation = XRSupportUtil.GetXRNodeCenterEyeLocalRotation();
			Quaternion quaternion = ((_temporalWarpingMode == TemporalWarpingMode.Off) ? xRNodeCenterEyeLocalRotation : delayedRot);
			Quaternion quaternion2 = Quaternion.Inverse(xRNodeCenterEyeLocalRotation) * quaternion;
			quaternion2 = Quaternion.Euler(quaternion2.eulerAngles.x, quaternion2.eulerAngles.y, 0f - quaternion2.eulerAngles.z);
			Matrix4x4 value = projectionMatrix * Matrix4x4.TRS(Vector3.zero, quaternion2, Vector3.one) * projectionMatrix.inverse;
			Shader.SetGlobalMatrix("_LeapGlobalWarpedOffset", value);
		}

		private void OnPreCull()
		{
			if (!(_cachedCamera == null))
			{
				Pose pose = new Pose(XRSupportUtil.GetXRNodeCenterEyeLocalPosition(), XRSupportUtil.GetXRNodeCenterEyeLocalRotation());
				if (!_trackingBaseDeltaPose.HasValue)
				{
					_trackingBaseDeltaPose = _cachedCamera.transform.ToLocalPose() * pose.inverse;
				}
				Pose curPose = _trackingBaseDeltaPose.Value * pose;
				transformHistory.UpdateDelay(curPose, _leapController.Now());
				OnPreCullHandTransforms(_cachedCamera);
			}
		}

		protected override long CalculateInterpolationTime(bool endOfFrame = false)
		{
			if (_leapController != null)
			{
				return _leapController.Now() - (long)_smoothedTrackingLatency.value + ((!updateHandInPrecull || endOfFrame) ? 0 : ((long)((double)Time.smoothDeltaTime * 1000000.0 / (double)Time.timeScale)));
			}
			return 0L;
		}

		protected override void initializeFlags()
		{
			if (_leapController != null)
			{
				_leapController.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
			}
		}

		protected override void transformFrame(Frame source, Frame dest)
		{
			LeapTransform warpedMatrix = GetWarpedMatrix(source.Timestamp);
			dest.CopyFrom(source).Transform(warpedMatrix);
		}

		protected void resetShaderTransforms()
		{
			_transformArray[0] = Matrix4x4.identity;
			_transformArray[1] = Matrix4x4.identity;
			Shader.SetGlobalMatrixArray("_LeapHandTransforms", _transformArray);
		}

		protected virtual LeapTransform GetWarpedMatrix(long timestamp, bool updateTemporalCompensation = true)
		{
			if (Application.isPlaying && updateTemporalCompensation && transformHistory.history.IsFull && _temporalWarpingMode != TemporalWarpingMode.Off)
			{
				transformHistory.SampleTransform(timestamp - (long)((float)warpingAdjustment * 1000f) - ((_temporalWarpingMode == TemporalWarpingMode.Images) ? (-20000) : 0), out warpedPosition, out warpedRotation);
			}
			Pose otherPose = Pose.identity;
			if (_deviceOffsetMode == DeviceOffsetMode.Transform && deviceOrigin != null)
			{
				otherPose.position = deviceOrigin.position;
				otherPose.rotation = deviceOrigin.rotation;
			}
			else if (!Application.isPlaying)
			{
				otherPose.position = otherPose.rotation * Vector3.up * deviceOffsetYAxis + otherPose.rotation * Vector3.forward * deviceOffsetZAxis;
				otherPose.rotation = Quaternion.Euler(deviceTiltXAxis, 0f, 0f);
				otherPose = base.transform.ToLocalPose().Then(otherPose);
			}
			else
			{
				transformHistory.SampleTransform(timestamp, out otherPose.position, out otherPose.rotation);
			}
			bool flag = _temporalWarpingMode == TemporalWarpingMode.Off || !Application.isPlaying;
			warpedPosition = ((!flag) ? warpedPosition : otherPose.position);
			warpedRotation = ((!flag) ? warpedRotation : otherPose.rotation);
			if (Application.isPlaying)
			{
				if (_deviceOffsetMode != DeviceOffsetMode.Transform)
				{
					warpedPosition += warpedRotation * Vector3.up * deviceOffsetYAxis + warpedRotation * Vector3.forward * deviceOffsetZAxis;
					warpedRotation *= Quaternion.Euler(deviceTiltXAxis, 0f, 0f);
				}
				warpedRotation *= Quaternion.Euler(-90f, 180f, 0f);
			}
			LeapTransform result = ((!(base.transform.parent != null) || _deviceOffsetMode == DeviceOffsetMode.Transform) ? new LeapTransform(warpedPosition.ToVector(), warpedRotation.ToLeapQuaternion(), base.transform.lossyScale.ToVector() * 0.001f) : new LeapTransform(base.transform.parent.TransformPoint(warpedPosition).ToVector(), (base.transform.parent.rotation * warpedRotation).ToLeapQuaternion(), base.transform.lossyScale.ToVector() * 0.001f));
			result.MirrorZ();
			return result;
		}

		protected void transformHands(ref LeapTransform LeftHand, ref LeapTransform RightHand)
		{
			LeapTransform warpedMatrix = GetWarpedMatrix(0L, false);
			LeftHand = new LeapTransform(warpedMatrix.TransformPoint(LeftHand.translation), warpedMatrix.TransformQuaternion(LeftHand.rotation));
			RightHand = new LeapTransform(warpedMatrix.TransformPoint(RightHand.translation), warpedMatrix.TransformQuaternion(RightHand.rotation));
		}

		protected void OnPreCullHandTransforms(Camera camera)
		{
			if (!updateHandInPrecull)
			{
				return;
			}
			CameraType cameraType = camera.cameraType;
			if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection || cameraType == CameraType.SceneView || !Application.isPlaying || manualUpdateHasBeenCalledSinceUpdate || _leapController == null)
			{
				return;
			}
			manualUpdateHasBeenCalledSinceUpdate = true;
			Hand hand = null;
			Hand hand2 = null;
			LeapTransform leftTransform = LeapTransform.Identity;
			LeapTransform rightTransform = LeapTransform.Identity;
			for (int i = 0; i < CurrentFrame.Hands.Count; i++)
			{
				Hand hand3 = CurrentFrame.Hands[i];
				if (hand3.IsLeft && hand == null)
				{
					hand = hand3;
				}
				else if (hand3.IsRight && hand2 == null)
				{
					hand2 = hand3;
				}
			}
			long num = CalculateInterpolationTime();
			_leapController.GetInterpolatedLeftRightTransform(num + ExtrapolationAmount * 1000, num - BounceAmount * 1000, hand?.Id ?? 0, hand2?.Id ?? 0, out leftTransform, out rightTransform);
			bool flag = leftTransform.translation != Vector.Zero;
			bool flag2 = rightTransform.translation != Vector.Zero;
			transformHands(ref leftTransform, ref rightTransform);
			if (hand2 != null && flag2)
			{
				_transformArray[0] = Matrix4x4.TRS(rightTransform.translation.ToVector3(), rightTransform.rotation.ToQuaternion(), Vector3.one) * Matrix4x4.Inverse(Matrix4x4.TRS(hand2.PalmPosition.ToVector3(), hand2.Rotation.ToQuaternion(), Vector3.one));
			}
			if (hand != null && flag)
			{
				_transformArray[1] = Matrix4x4.TRS(leftTransform.translation.ToVector3(), leftTransform.rotation.ToQuaternion(), Vector3.one) * Matrix4x4.Inverse(Matrix4x4.TRS(hand.PalmPosition.ToVector3(), hand.Rotation.ToQuaternion(), Vector3.one));
			}
			Shader.SetGlobalMatrixArray("_LeapHandTransforms", _transformArray);
		}
	}
}
