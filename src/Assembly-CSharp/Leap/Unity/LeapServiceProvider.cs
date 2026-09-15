using System;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class LeapServiceProvider : LeapProvider
	{
		public enum FrameOptimizationMode
		{
			None,
			ReuseUpdateForPhysics,
			ReusePhysicsForUpdate
		}

		public enum PhysicsExtrapolationMode
		{
			None,
			Auto,
			Manual
		}

		protected const double NS_TO_S = 1E-06;

		protected const double S_TO_NS = 1000000.0;

		protected const string HAND_ARRAY_GLOBAL_NAME = "_LeapHandTransforms";

		protected const int MAX_RECONNECTION_ATTEMPTS = 5;

		protected const int RECONNECTION_INTERVAL = 180;

		[Tooltip("When enabled, the provider will only calculate one leap frame instead of two.")]
		[SerializeField]
		protected FrameOptimizationMode _frameOptimization;

		[Tooltip("The mode to use when extrapolating physics.\n None - No extrapolation is used at all.\n Auto - Extrapolation is chosen based on the fixed timestep.\n Manual - Extrapolation time is chosen manually by the user.")]
		[SerializeField]
		protected PhysicsExtrapolationMode _physicsExtrapolation = PhysicsExtrapolationMode.Auto;

		[Tooltip("The amount of time (in seconds) to extrapolate the physics data by.")]
		[SerializeField]
		protected float _physicsExtrapolationTime = 1f / 90f;

		[Tooltip("When checked, profiling data from the LeapCSharp worker thread will be used to populate the UnityProfiler.")]
		[EditTimeOnly]
		[SerializeField]
		protected bool _workerThreadProfiling;

		protected bool _useInterpolation = true;

		protected int ExtrapolationAmount;

		protected int BounceAmount;

		protected Controller _leapController;

		protected bool _isDestroyed;

		protected SmoothedFloat _fixedOffset = new SmoothedFloat();

		protected SmoothedFloat _smoothedTrackingLatency = new SmoothedFloat();

		protected long _unityToLeapOffset;

		protected Frame _untransformedUpdateFrame;

		protected Frame _transformedUpdateFrame;

		protected Frame _untransformedFixedFrame;

		protected Frame _transformedFixedFrame;

		private Action<Device> _onDeviceSafe;

		private int _framesSinceServiceConnectionChecked;

		private int _numberOfReconnectionAttempts;

		public override Frame CurrentFrame
		{
			get
			{
				if (_frameOptimization == FrameOptimizationMode.ReusePhysicsForUpdate)
				{
					return _transformedFixedFrame;
				}
				return _transformedUpdateFrame;
			}
		}

		public override Frame CurrentFixedFrame
		{
			get
			{
				if (_frameOptimization == FrameOptimizationMode.ReuseUpdateForPhysics)
				{
					return _transformedUpdateFrame;
				}
				return _transformedFixedFrame;
			}
		}

		public event Action<Device> OnDeviceSafe
		{
			add
			{
				if (_leapController != null && _leapController.IsConnected)
				{
					foreach (Device device in _leapController.Devices)
					{
						value(device);
					}
				}
				_onDeviceSafe = (Action<Device>)Delegate.Combine(_onDeviceSafe, value);
			}
			remove
			{
				_onDeviceSafe = (Action<Device>)Delegate.Remove(_onDeviceSafe, value);
			}
		}

		protected virtual void Reset()
		{
			editTimePose = TestHandFactory.TestHandPose.DesktopModeA;
		}

		protected virtual void Awake()
		{
			_fixedOffset.delay = 0.4f;
			_smoothedTrackingLatency.SetBlend(0.99f, 0.0111f);
		}

		protected virtual void Start()
		{
			createController();
			_transformedUpdateFrame = new Frame();
			_transformedFixedFrame = new Frame();
			_untransformedUpdateFrame = new Frame();
			_untransformedFixedFrame = new Frame();
		}

		protected virtual void Update()
		{
			if (_workerThreadProfiling)
			{
				LeapProfiling.Update();
			}
			if (!checkConnectionIntegrity())
			{
				return;
			}
			_fixedOffset.Update(Time.time - Time.fixedTime, Time.deltaTime);
			if (_frameOptimization == FrameOptimizationMode.ReusePhysicsForUpdate)
			{
				DispatchUpdateFrameEvent(_transformedFixedFrame);
				return;
			}
			if (_useInterpolation)
			{
				_smoothedTrackingLatency.value = Mathf.Min(_smoothedTrackingLatency.value, 30000f);
				_smoothedTrackingLatency.Update(_leapController.Now() - _leapController.FrameTimestamp(), Time.deltaTime);
				long num = CalculateInterpolationTime() + ExtrapolationAmount * 1000;
				_unityToLeapOffset = num - (long)((double)Time.time * 1000000.0);
				_leapController.GetInterpolatedFrameFromTime(_untransformedUpdateFrame, num, CalculateInterpolationTime() - BounceAmount * 1000);
			}
			else
			{
				_leapController.Frame(_untransformedUpdateFrame);
			}
			if (_untransformedUpdateFrame != null)
			{
				transformFrame(_untransformedUpdateFrame, _transformedUpdateFrame);
				DispatchUpdateFrameEvent(_transformedUpdateFrame);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (_frameOptimization == FrameOptimizationMode.ReuseUpdateForPhysics)
			{
				DispatchFixedFrameEvent(_transformedUpdateFrame);
				return;
			}
			if (_useInterpolation)
			{
				long time;
				switch (_frameOptimization)
				{
				case FrameOptimizationMode.None:
				{
					float num = Time.fixedTime + CalculatePhysicsExtrapolation();
					time = (long)((double)num * 1000000.0) + _unityToLeapOffset;
					break;
				}
				case FrameOptimizationMode.ReusePhysicsForUpdate:
					time = CalculateInterpolationTime() + ExtrapolationAmount * 1000;
					break;
				default:
					throw new InvalidOperationException("Unexpected frame optimization mode: " + _frameOptimization);
				}
				_leapController.GetInterpolatedFrame(_untransformedFixedFrame, time);
			}
			else
			{
				_leapController.Frame(_untransformedFixedFrame);
			}
			if (_untransformedFixedFrame != null)
			{
				transformFrame(_untransformedFixedFrame, _transformedFixedFrame);
				DispatchFixedFrameEvent(_transformedFixedFrame);
			}
		}

		protected virtual void OnDestroy()
		{
			destroyController();
			_isDestroyed = true;
		}

		protected virtual void OnApplicationPause(bool isPaused)
		{
			if (_leapController != null)
			{
				if (isPaused)
				{
					_leapController.StopConnection();
				}
				else
				{
					_leapController.StartConnection();
				}
			}
		}

		protected virtual void OnApplicationQuit()
		{
			destroyController();
			_isDestroyed = true;
		}

		public float CalculatePhysicsExtrapolation()
		{
			switch (_physicsExtrapolation)
			{
			case PhysicsExtrapolationMode.None:
				return 0f;
			case PhysicsExtrapolationMode.Auto:
				return Time.fixedDeltaTime;
			case PhysicsExtrapolationMode.Manual:
				return _physicsExtrapolationTime;
			default:
				throw new InvalidOperationException("Unexpected physics extrapolation mode: " + _physicsExtrapolation);
			}
		}

		public Controller GetLeapController()
		{
			return _leapController;
		}

		public bool IsConnected()
		{
			return GetLeapController().IsConnected;
		}

		public void RetransformFrames()
		{
			transformFrame(_untransformedUpdateFrame, _transformedUpdateFrame);
			transformFrame(_untransformedFixedFrame, _transformedFixedFrame);
		}

		public void CopySettingsToLeapXRServiceProvider(LeapXRServiceProvider leapXRServiceProvider)
		{
			leapXRServiceProvider._frameOptimization = _frameOptimization;
			leapXRServiceProvider._physicsExtrapolation = _physicsExtrapolation;
			leapXRServiceProvider._physicsExtrapolationTime = _physicsExtrapolationTime;
			leapXRServiceProvider._workerThreadProfiling = _workerThreadProfiling;
		}

		protected virtual long CalculateInterpolationTime(bool endOfFrame = false)
		{
			if (_leapController != null)
			{
				return _leapController.Now() - (long)_smoothedTrackingLatency.value;
			}
			return 0L;
		}

		protected virtual void initializeFlags()
		{
			if (_leapController != null)
			{
				_leapController.ClearPolicy(Controller.PolicyFlag.POLICY_DEFAULT);
			}
		}

		protected void createController()
		{
			if (_leapController != null)
			{
				return;
			}
			_leapController = new Controller();
			_leapController.Device += delegate(object s, DeviceEventArgs e)
			{
				if (_onDeviceSafe != null)
				{
					_onDeviceSafe(e.Device);
				}
			};
			if (_leapController.IsConnected)
			{
				initializeFlags();
			}
			else
			{
				_leapController.Device += onHandControllerConnect;
			}
			if (_workerThreadProfiling)
			{
				_leapController.EndProfilingBlock += LeapProfiling.EndProfilingBlock;
				_leapController.BeginProfilingBlock += LeapProfiling.BeginProfilingBlock;
				_leapController.EndProfilingForThread += LeapProfiling.EndProfilingForThread;
				_leapController.BeginProfilingForThread += LeapProfiling.BeginProfilingForThread;
			}
		}

		protected void destroyController()
		{
			if (_leapController != null)
			{
				if (_leapController.IsConnected)
				{
					_leapController.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
				}
				_leapController.StopConnection();
				_leapController = null;
			}
		}

		protected bool checkConnectionIntegrity()
		{
			if (_leapController.IsServiceConnected)
			{
				_framesSinceServiceConnectionChecked = 0;
				_numberOfReconnectionAttempts = 0;
				return true;
			}
			if (_numberOfReconnectionAttempts < 5)
			{
				_framesSinceServiceConnectionChecked++;
				if (_framesSinceServiceConnectionChecked > 180)
				{
					_framesSinceServiceConnectionChecked = 0;
					_numberOfReconnectionAttempts++;
					Debug.LogWarning("Leap Service not connected; attempting to reconnect for try " + _numberOfReconnectionAttempts + "/" + 5 + "...", this);
					using (new ProfilerSample("Reconnection Attempt"))
					{
						destroyController();
						createController();
					}
				}
			}
			return false;
		}

		protected void onHandControllerConnect(object sender, LeapEventArgs args)
		{
			initializeFlags();
			if (_leapController != null)
			{
				_leapController.Device -= onHandControllerConnect;
			}
		}

		protected virtual void transformFrame(Frame source, Frame dest)
		{
			dest.CopyFrom(source).Transform(base.transform.GetLeapMatrix());
		}
	}
}
