using System;
using System.Threading;
using LeapInternal;

namespace Leap
{
	public class Controller : IController, IDisposable
	{
		public enum PolicyFlag
		{
			POLICY_DEFAULT = 0,
			POLICY_BACKGROUND_FRAMES = 1,
			POLICY_IMAGES = 2,
			POLICY_OPTIMIZE_HMD = 4,
			POLICY_ALLOW_PAUSE_RESUME = 8,
			POLICY_MAP_POINTS = 0x80
		}

		private Connection _connection;

		private bool _disposed;

		private Config _config;

		private bool _hasInitialized;

		private EventHandler<LeapEventArgs> _init;

		private bool _hasConnected;

		private EventHandler<ConnectionEventArgs> _connect;

		public SynchronizationContext EventContext
		{
			get
			{
				return _connection.EventContext;
			}
			set
			{
				_connection.EventContext = value;
			}
		}

		public bool IsServiceConnected => _connection.IsServiceConnected;

		public bool IsConnected => IsServiceConnected && Devices.Count > 0;

		public Config Config
		{
			get
			{
				if (_config == null)
				{
					_config = new Config(_connection.ConnectionKey);
				}
				return _config;
			}
		}

		public DeviceList Devices => _connection.Devices;

		public event EventHandler<LeapEventArgs> Init
		{
			add
			{
				if (_hasInitialized)
				{
					value(this, new LeapEventArgs(LeapEvent.EVENT_INIT));
				}
				_init = (EventHandler<LeapEventArgs>)Delegate.Combine(_init, value);
			}
			remove
			{
				_init = (EventHandler<LeapEventArgs>)Delegate.Remove(_init, value);
			}
		}

		public event EventHandler<ConnectionEventArgs> Connect
		{
			add
			{
				if (_hasConnected)
				{
					value(this, new ConnectionEventArgs());
				}
				_connect = (EventHandler<ConnectionEventArgs>)Delegate.Combine(_connect, value);
			}
			remove
			{
				_connect = (EventHandler<ConnectionEventArgs>)Delegate.Remove(_connect, value);
			}
		}

		public event EventHandler<ConnectionLostEventArgs> Disconnect
		{
			add
			{
				Connection connection = _connection;
				connection.LeapConnectionLost = (EventHandler<ConnectionLostEventArgs>)Delegate.Combine(connection.LeapConnectionLost, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapConnectionLost = (EventHandler<ConnectionLostEventArgs>)Delegate.Remove(connection.LeapConnectionLost, value);
			}
		}

		public event EventHandler<FrameEventArgs> FrameReady
		{
			add
			{
				Connection connection = _connection;
				connection.LeapFrame = (EventHandler<FrameEventArgs>)Delegate.Combine(connection.LeapFrame, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapFrame = (EventHandler<FrameEventArgs>)Delegate.Remove(connection.LeapFrame, value);
			}
		}

		public event EventHandler<InternalFrameEventArgs> InternalFrameReady
		{
			add
			{
				Connection connection = _connection;
				connection.LeapInternalFrame = (EventHandler<InternalFrameEventArgs>)Delegate.Combine(connection.LeapInternalFrame, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapInternalFrame = (EventHandler<InternalFrameEventArgs>)Delegate.Remove(connection.LeapInternalFrame, value);
			}
		}

		public event EventHandler<DeviceEventArgs> Device
		{
			add
			{
				Connection connection = _connection;
				connection.LeapDevice = (EventHandler<DeviceEventArgs>)Delegate.Combine(connection.LeapDevice, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapDevice = (EventHandler<DeviceEventArgs>)Delegate.Remove(connection.LeapDevice, value);
			}
		}

		public event EventHandler<DeviceEventArgs> DeviceLost
		{
			add
			{
				Connection connection = _connection;
				connection.LeapDeviceLost = (EventHandler<DeviceEventArgs>)Delegate.Combine(connection.LeapDeviceLost, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapDeviceLost = (EventHandler<DeviceEventArgs>)Delegate.Remove(connection.LeapDeviceLost, value);
			}
		}

		public event EventHandler<DeviceFailureEventArgs> DeviceFailure
		{
			add
			{
				Connection connection = _connection;
				connection.LeapDeviceFailure = (EventHandler<DeviceFailureEventArgs>)Delegate.Combine(connection.LeapDeviceFailure, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapDeviceFailure = (EventHandler<DeviceFailureEventArgs>)Delegate.Remove(connection.LeapDeviceFailure, value);
			}
		}

		public event EventHandler<LogEventArgs> LogMessage
		{
			add
			{
				Connection connection = _connection;
				connection.LeapLogEvent = (EventHandler<LogEventArgs>)Delegate.Combine(connection.LeapLogEvent, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapLogEvent = (EventHandler<LogEventArgs>)Delegate.Remove(connection.LeapLogEvent, value);
			}
		}

		public event EventHandler<PolicyEventArgs> PolicyChange
		{
			add
			{
				Connection connection = _connection;
				connection.LeapPolicyChange = (EventHandler<PolicyEventArgs>)Delegate.Combine(connection.LeapPolicyChange, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapPolicyChange = (EventHandler<PolicyEventArgs>)Delegate.Remove(connection.LeapPolicyChange, value);
			}
		}

		public event EventHandler<ConfigChangeEventArgs> ConfigChange
		{
			add
			{
				Connection connection = _connection;
				connection.LeapConfigChange = (EventHandler<ConfigChangeEventArgs>)Delegate.Combine(connection.LeapConfigChange, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapConfigChange = (EventHandler<ConfigChangeEventArgs>)Delegate.Remove(connection.LeapConfigChange, value);
			}
		}

		public event EventHandler<DistortionEventArgs> DistortionChange
		{
			add
			{
				Connection connection = _connection;
				connection.LeapDistortionChange = (EventHandler<DistortionEventArgs>)Delegate.Combine(connection.LeapDistortionChange, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapDistortionChange = (EventHandler<DistortionEventArgs>)Delegate.Remove(connection.LeapDistortionChange, value);
			}
		}

		public event EventHandler<DroppedFrameEventArgs> DroppedFrame
		{
			add
			{
				Connection connection = _connection;
				connection.LeapDroppedFrame = (EventHandler<DroppedFrameEventArgs>)Delegate.Combine(connection.LeapDroppedFrame, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapDroppedFrame = (EventHandler<DroppedFrameEventArgs>)Delegate.Remove(connection.LeapDroppedFrame, value);
			}
		}

		public event EventHandler<ImageEventArgs> ImageReady
		{
			add
			{
				Connection connection = _connection;
				connection.LeapImage = (EventHandler<ImageEventArgs>)Delegate.Combine(connection.LeapImage, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapImage = (EventHandler<ImageEventArgs>)Delegate.Remove(connection.LeapImage, value);
			}
		}

		public event Action<BeginProfilingForThreadArgs> BeginProfilingForThread
		{
			add
			{
				Connection connection = _connection;
				connection.LeapBeginProfilingForThread = (Action<BeginProfilingForThreadArgs>)Delegate.Combine(connection.LeapBeginProfilingForThread, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapBeginProfilingForThread = (Action<BeginProfilingForThreadArgs>)Delegate.Remove(connection.LeapBeginProfilingForThread, value);
			}
		}

		public event Action<EndProfilingForThreadArgs> EndProfilingForThread
		{
			add
			{
				Connection connection = _connection;
				connection.LeapEndProfilingForThread = (Action<EndProfilingForThreadArgs>)Delegate.Combine(connection.LeapEndProfilingForThread, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapEndProfilingForThread = (Action<EndProfilingForThreadArgs>)Delegate.Remove(connection.LeapEndProfilingForThread, value);
			}
		}

		public event Action<BeginProfilingBlockArgs> BeginProfilingBlock
		{
			add
			{
				Connection connection = _connection;
				connection.LeapBeginProfilingBlock = (Action<BeginProfilingBlockArgs>)Delegate.Combine(connection.LeapBeginProfilingBlock, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapBeginProfilingBlock = (Action<BeginProfilingBlockArgs>)Delegate.Remove(connection.LeapBeginProfilingBlock, value);
			}
		}

		public event Action<EndProfilingBlockArgs> EndProfilingBlock
		{
			add
			{
				Connection connection = _connection;
				connection.LeapEndProfilingBlock = (Action<EndProfilingBlockArgs>)Delegate.Combine(connection.LeapEndProfilingBlock, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapEndProfilingBlock = (Action<EndProfilingBlockArgs>)Delegate.Remove(connection.LeapEndProfilingBlock, value);
			}
		}

		public event EventHandler<PointMappingChangeEventArgs> PointMappingChange
		{
			add
			{
				Connection connection = _connection;
				connection.LeapPointMappingChange = (EventHandler<PointMappingChangeEventArgs>)Delegate.Combine(connection.LeapPointMappingChange, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapPointMappingChange = (EventHandler<PointMappingChangeEventArgs>)Delegate.Remove(connection.LeapPointMappingChange, value);
			}
		}

		public event EventHandler<HeadPoseEventArgs> HeadPoseChange
		{
			add
			{
				Connection connection = _connection;
				connection.LeapHeadPoseChange = (EventHandler<HeadPoseEventArgs>)Delegate.Combine(connection.LeapHeadPoseChange, value);
			}
			remove
			{
				Connection connection = _connection;
				connection.LeapHeadPoseChange = (EventHandler<HeadPoseEventArgs>)Delegate.Remove(connection.LeapHeadPoseChange, value);
			}
		}

		public Controller()
			: this(0)
		{
		}

		public Controller(int connectionKey)
		{
			_connection = Connection.GetConnection(connectionKey);
			_connection.EventContext = SynchronizationContext.Current;
			_connection.LeapInit += OnInit;
			_connection.LeapConnection += OnConnect;
			Connection connection = _connection;
			connection.LeapConnectionLost = (EventHandler<ConnectionLostEventArgs>)Delegate.Combine(connection.LeapConnectionLost, new EventHandler<ConnectionLostEventArgs>(OnDisconnect));
			_connection.Start();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
			}
		}

		public void StartConnection()
		{
			_connection.Start();
		}

		public void StopConnection()
		{
			_connection.Stop();
		}

		public void SetPolicy(PolicyFlag policy)
		{
			_connection.SetPolicy(policy);
		}

		public void ClearPolicy(PolicyFlag policy)
		{
			_connection.ClearPolicy(policy);
		}

		public bool IsPolicySet(PolicyFlag policy)
		{
			return _connection.IsPolicySet(policy);
		}

		public Frame Frame(int history = 0)
		{
			Frame frame = new Frame();
			Frame(frame, history);
			return frame;
		}

		public void Frame(Frame toFill, int history = 0)
		{
			LEAP_TRACKING_EVENT t;
			_connection.Frames.Get(out t, history);
			toFill.CopyFrom(ref t);
		}

		public long FrameTimestamp(int history = 0)
		{
			LEAP_TRACKING_EVENT t;
			_connection.Frames.Get(out t, history);
			return t.info.timestamp;
		}

		public Frame GetTransformedFrame(LeapTransform trs, int history = 0)
		{
			return new Frame().CopyFrom(Frame(history)).Transform(trs);
		}

		public Frame GetInterpolatedFrame(long time)
		{
			return _connection.GetInterpolatedFrame(time);
		}

		public void GetInterpolatedFrame(Frame toFill, long time)
		{
			_connection.GetInterpolatedFrame(toFill, time);
		}

		public LEAP_HEAD_POSE_EVENT GetInterpolatedHeadPose(long time)
		{
			return _connection.GetInterpolatedHeadPose(time);
		}

		public void GetInterpolatedHeadPose(ref LEAP_HEAD_POSE_EVENT toFill, long time)
		{
			_connection.GetInterpolatedHeadPose(ref toFill, time);
		}

		public void TelemetryProfiling(ref LEAP_TELEMETRY_DATA telemetryData)
		{
			_connection.TelemetryProfiling(ref telemetryData);
		}

		public ulong TelemetryGetNow()
		{
			return LeapC.TelemetryGetNow();
		}

		public void GetPointMapping(ref PointMapping pointMapping)
		{
			_connection.GetPointMapping(ref pointMapping);
		}

		public void GetInterpolatedLeftRightTransform(long time, long sourceTime, int leftId, int rightId, out LeapTransform leftTransform, out LeapTransform rightTransform)
		{
			_connection.GetInterpolatedLeftRightTransform(time, sourceTime, leftId, rightId, out leftTransform, out rightTransform);
		}

		public void GetInterpolatedFrameFromTime(Frame toFill, long time, long sourceTime)
		{
			_connection.GetInterpolatedFrameFromTime(toFill, time, sourceTime);
		}

		public long Now()
		{
			return LeapC.GetNow();
		}

		public FailedDeviceList FailedDevices()
		{
			return _connection.FailedDevices;
		}

		protected virtual void OnInit(object sender, LeapEventArgs eventArgs)
		{
			_hasInitialized = true;
		}

		protected virtual void OnConnect(object sender, ConnectionEventArgs eventArgs)
		{
			_hasConnected = true;
		}

		protected virtual void OnDisconnect(object sender, ConnectionLostEventArgs eventArgs)
		{
			_hasInitialized = false;
			_hasConnected = false;
		}
	}
}
