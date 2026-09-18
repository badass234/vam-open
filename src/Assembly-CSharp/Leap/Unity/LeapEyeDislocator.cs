using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	[RequireComponent(typeof(LeapXRServiceProvider))]
	public class LeapEyeDislocator : MonoBehaviour
	{
		[SerializeField]
		private bool _useCustomBaseline;

		[MinValue(0f)]
		[Units("MM")]
		// Qualified: Unity 2019 added UnityEngine.InspectorNameAttribute, so the bare name is ambiguous
		// between it and Leap's own property-drawer attribute.
		[Leap.Unity.Attributes.InspectorName("Baseline")]
		[SerializeField]
		private float _customBaselineValue = 64f;

		[SerializeField]
		private bool _showEyePositions;

		private LeapServiceProvider _provider;

		private Maybe<float> _deviceBaseline = Maybe.None;

		private bool _hasVisitedPreCull;

		private Camera _cachedCamera;

		private Camera _camera
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

		private void onDevice(Device device)
		{
			_deviceBaseline = Maybe.Some(device.Baseline);
		}

		private void OnEnable()
		{
			_provider = GetComponent<LeapServiceProvider>();
			if (_provider == null)
			{
				_provider = GetComponentInChildren<LeapServiceProvider>();
				if (_provider == null)
				{
					base.enabled = false;
					return;
				}
			}
			_provider.OnDeviceSafe += onDevice;
		}

		private void OnDisable()
		{
			_camera.ResetStereoViewMatrices();
			_provider.OnDeviceSafe -= onDevice;
		}

		private void Update()
		{
			_camera.ResetStereoViewMatrices();
			_hasVisitedPreCull = false;
		}

		private void OnPreCull()
		{
			if (!_hasVisitedPreCull)
			{
				_hasVisitedPreCull = true;
				Maybe<float> maybe = Maybe.None;
				float t;
				if (((!_useCustomBaseline) ? _deviceBaseline : Maybe.Some(_customBaselineValue)).TryGetValue(out t))
				{
					t *= 0.001f;
					Matrix4x4 stereoViewMatrix = _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
					Matrix4x4 stereoViewMatrix2 = _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
					Vector3 a = stereoViewMatrix.inverse.MultiplyPoint3x4(Vector3.zero);
					Vector3 b = stereoViewMatrix2.inverse.MultiplyPoint3x4(Vector3.zero);
					float num = Vector3.Distance(a, b);
					float baselineAdjust = t - num;
					adjustViewMatrix(Camera.StereoscopicEye.Left, baselineAdjust);
					adjustViewMatrix(Camera.StereoscopicEye.Right, baselineAdjust);
				}
			}
		}

		private void adjustViewMatrix(Camera.StereoscopicEye eye, float baselineAdjust)
		{
			float num = ((eye == Camera.StereoscopicEye.Left) ? 1 : (-1));
			Vector3 vector = num * Vector3.right * baselineAdjust * 0.5f;
			Vector3 vector2 = Vector3.zero;
			Vector3 vector3 = Vector3.zero;
			Quaternion q = Quaternion.Euler(0f, 180f, 0f);
			if (_provider is LeapXRServiceProvider)
			{
				LeapXRServiceProvider leapXRServiceProvider = _provider as LeapXRServiceProvider;
				vector2 = Vector3.forward * leapXRServiceProvider.deviceOffsetZAxis;
				vector3 = -Vector3.up * leapXRServiceProvider.deviceOffsetYAxis;
				q = Quaternion.AngleAxis(leapXRServiceProvider.deviceTiltXAxis, Vector3.right);
			}
			else
			{
				Matrix4x4 value = _camera.projectionMatrix * Matrix4x4.TRS(Vector3.zero, q, Vector3.one) * _camera.projectionMatrix.inverse;
				Shader.SetGlobalMatrix("_LeapGlobalWarpedOffset", value);
			}
			Matrix4x4 stereoViewMatrix = _camera.GetStereoViewMatrix(eye);
			_camera.SetStereoViewMatrix(eye, Matrix4x4.TRS(Vector3.zero, q, Vector3.one) * Matrix4x4.Translate(vector2 + vector) * Matrix4x4.Translate(vector3) * stereoViewMatrix);
		}

		private void OnDrawGizmos()
		{
			if (_showEyePositions && Application.isPlaying)
			{
				Matrix4x4 stereoViewMatrix = _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
				Matrix4x4 stereoViewMatrix2 = _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
				Vector3 vector = stereoViewMatrix.inverse.MultiplyPoint3x4(Vector3.zero);
				Vector3 vector2 = stereoViewMatrix2.inverse.MultiplyPoint3x4(Vector3.zero);
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(vector, 0.02f);
				Gizmos.DrawSphere(vector2, 0.02f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(vector, vector2);
			}
		}
	}
}
