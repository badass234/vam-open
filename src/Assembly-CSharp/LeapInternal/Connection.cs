using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Leap;

namespace LeapInternal
{
	public class Connection
	{
		private static Dictionary<int, Connection> connectionDictionary;

		private static long _handIdOffset;

		private static long _handPositionOffset;

		private static long _handOrientationOffset;

		private static LEAP_CONNECTION_INFO pInfo;

		private DeviceList _devices = new DeviceList();

		private FailedDeviceList _failedDevices;

		private DistortionData _currentLeftDistortionData = new DistortionData();

		private DistortionData _currentRightDistortionData = new DistortionData();

		private int _frameBufferLength = 60;

		private IntPtr _leapConnection;

		private bool _isRunning;

		private Thread _polster;

		private ulong _requestedPolicies;

		private ulong _activePolicies;

		private Dictionary<uint, string> _configRequests = new Dictionary<uint, string>();

		private EventHandler<LeapEventArgs> _leapInit;

		private EventHandler<ConnectionEventArgs> _leapConnectionEvent;

		public EventHandler<ConnectionLostEventArgs> LeapConnectionLost;

		public EventHandler<DeviceEventArgs> LeapDevice;

		public EventHandler<DeviceEventArgs> LeapDeviceLost;

		public EventHandler<DeviceFailureEventArgs> LeapDeviceFailure;

		public EventHandler<PolicyEventArgs> LeapPolicyChange;

		public EventHandler<FrameEventArgs> LeapFrame;

		public EventHandler<InternalFrameEventArgs> LeapInternalFrame;

		public EventHandler<LogEventArgs> LeapLogEvent;

		public EventHandler<SetConfigResponseEventArgs> LeapConfigResponse;

		public EventHandler<ConfigChangeEventArgs> LeapConfigChange;

		public EventHandler<DistortionEventArgs> LeapDistortionChange;

		public EventHandler<DroppedFrameEventArgs> LeapDroppedFrame;

		public EventHandler<ImageEventArgs> LeapImage;

		public EventHandler<PointMappingChangeEventArgs> LeapPointMappingChange;

		public EventHandler<HeadPoseEventArgs> LeapHeadPoseChange;

		public Action<BeginProfilingForThreadArgs> LeapBeginProfilingForThread;

		public Action<EndProfilingForThreadArgs> LeapEndProfilingForThread;

		public Action<BeginProfilingBlockArgs> LeapBeginProfilingBlock;

		public Action<EndProfilingBlockArgs> LeapEndProfilingBlock;

		private bool _disposed;

		private LEAP_ALLOCATOR _pLeapAllocator = default(LEAP_ALLOCATOR);

		private eLeapRS _lastResult;

		public int ConnectionKey { get; private set; }

		public CircularObjectBuffer<LEAP_TRACKING_EVENT> Frames { get; set; }

		public SynchronizationContext EventContext { get; set; }

		public bool IsServiceConnected
		{
			get
			{
				if (_leapConnection == IntPtr.Zero)
				{
					return false;
				}
				eLeapRS connectionInfo = LeapC.GetConnectionInfo(_leapConnection, ref pInfo);
				reportAbnormalResults("LeapC GetConnectionInfo call was ", connectionInfo);
				if (pInfo.status == eLeapConnectionStatus.eLeapConnectionStatus_Connected)
				{
					return true;
				}
				return false;
			}
		}

		public DeviceList Devices
		{
			get
			{
				if (_devices == null)
				{
					_devices = new DeviceList();
				}
				return _devices;
			}
		}

		public FailedDeviceList FailedDevices
		{
			get
			{
				if (_failedDevices == null)
				{
					_failedDevices = new FailedDeviceList();
				}
				return _failedDevices;
			}
		}

		public event EventHandler<LeapEventArgs> LeapInit
		{
			add
			{
				_leapInit = (EventHandler<LeapEventArgs>)Delegate.Combine(_leapInit, value);
				if (_leapConnection != IntPtr.Zero)
				{
					value(this, new LeapEventArgs(LeapEvent.EVENT_INIT));
				}
			}
			remove
			{
				_leapInit = (EventHandler<LeapEventArgs>)Delegate.Remove(_leapInit, value);
			}
		}

		public event EventHandler<ConnectionEventArgs> LeapConnection
		{
			add
			{
				_leapConnectionEvent = (EventHandler<ConnectionEventArgs>)Delegate.Combine(_leapConnectionEvent, value);
				if (IsServiceConnected)
				{
					value(this, new ConnectionEventArgs());
				}
			}
			remove
			{
				_leapConnectionEvent = (EventHandler<ConnectionEventArgs>)Delegate.Remove(_leapConnectionEvent, value);
			}
		}

		static Connection()
		{
			connectionDictionary = new Dictionary<int, Connection>();
			_handIdOffset = Marshal.OffsetOf(typeof(LEAP_HAND), "id").ToInt64();
			pInfo.size = (uint)Marshal.SizeOf(pInfo);
			long num = Marshal.OffsetOf(typeof(LEAP_HAND), "palm").ToInt64();
			_handPositionOffset = Marshal.OffsetOf(typeof(LEAP_PALM), "position").ToInt64() + num;
			_handOrientationOffset = Marshal.OffsetOf(typeof(LEAP_PALM), "orientation").ToInt64() + num;
		}

		private Connection(int connectionKey)
		{
			ConnectionKey = connectionKey;
			_leapConnection = IntPtr.Zero;
			Frames = new CircularObjectBuffer<LEAP_TRACKING_EVENT>(_frameBufferLength);
		}

		public static Connection GetConnection(int connectionKey = 0)
		{
			Connection value;
			if (!connectionDictionary.TryGetValue(connectionKey, out value))
			{
				value = new Connection(connectionKey);
				connectionDictionary.Add(connectionKey, value);
			}
			return value;
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
				if (disposing)
				{
				}
				Stop();
				LeapC.DestroyConnection(_leapConnection);
				_leapConnection = IntPtr.Zero;
				_disposed = true;
			}
		}

		~Connection()
		{
			Dispose(false);
		}

		public void Start()
		{
			if (_isRunning)
			{
				return;
			}
			eLeapRS eLeapRS2;
			if (_leapConnection == IntPtr.Zero)
			{
				eLeapRS2 = LeapC.CreateConnection(out _leapConnection);
				if (eLeapRS2 != 0 || _leapConnection == IntPtr.Zero)
				{
					reportAbnormalResults("LeapC CreateConnection call was ", eLeapRS2);
					return;
				}
			}
			eLeapRS2 = LeapC.OpenConnection(_leapConnection);
			if (eLeapRS2 != 0)
			{
				reportAbnormalResults("LeapC OpenConnection call was ", eLeapRS2);
				return;
			}
			if (_pLeapAllocator.allocate == null)
			{
				_pLeapAllocator.allocate = MemoryManager.Pin;
			}
			if (_pLeapAllocator.deallocate == null)
			{
				_pLeapAllocator.deallocate = MemoryManager.Unpin;
			}
			LeapC.SetAllocator(_leapConnection, ref _pLeapAllocator);
			_isRunning = true;
			AppDomain.CurrentDomain.DomainUnload += delegate
			{
				Dispose(true);
			};
			_polster = new Thread(processMessages);
			_polster.Name = "LeapC Worker";
			_polster.IsBackground = true;
			_polster.Start();
		}

		public void Stop()
		{
			if (_isRunning)
			{
				_isRunning = false;
				LeapC.CloseConnection(_leapConnection);
				_polster.Join();
			}
		}

		private void processMessages()
		{
			bool flag = false;
			try
			{
				_leapInit.DispatchOnContext(this, EventContext, new LeapEventArgs(LeapEvent.EVENT_INIT));
				while (_isRunning)
				{
					if (LeapBeginProfilingForThread != null && !flag)
					{
						LeapBeginProfilingForThread(new BeginProfilingForThreadArgs("Worker Thread", "Handle Event"));
						flag = true;
					}
					LEAP_CONNECTION_MESSAGE msg = default(LEAP_CONNECTION_MESSAGE);
					uint timeout = 150u;
					eLeapRS eLeapRS2 = LeapC.PollConnection(_leapConnection, timeout, ref msg);
					if (eLeapRS2 != 0)
					{
						reportAbnormalResults("LeapC PollConnection call was ", eLeapRS2);
						continue;
					}
					if (LeapBeginProfilingBlock != null && flag)
					{
						LeapBeginProfilingBlock(new BeginProfilingBlockArgs("Handle Event"));
					}
					switch (msg.type)
					{
					case eLeapEventType.eLeapEventType_Connection:
					{
						LEAP_CONNECTION_EVENT t13;
						StructMarshal<LEAP_CONNECTION_EVENT>.PtrToStruct(msg.eventStructPtr, out t13);
						handleConnection(ref t13);
						break;
					}
					case eLeapEventType.eLeapEventType_ConnectionLost:
					{
						LEAP_CONNECTION_LOST_EVENT t12;
						StructMarshal<LEAP_CONNECTION_LOST_EVENT>.PtrToStruct(msg.eventStructPtr, out t12);
						handleConnectionLost(ref t12);
						break;
					}
					case eLeapEventType.eLeapEventType_Device:
					{
						LEAP_DEVICE_EVENT t11;
						StructMarshal<LEAP_DEVICE_EVENT>.PtrToStruct(msg.eventStructPtr, out t11);
						handleDevice(ref t11);
						break;
					}
					case eLeapEventType.eLeapEventType_DeviceFailure:
					{
						LEAP_DEVICE_FAILURE_EVENT t10;
						StructMarshal<LEAP_DEVICE_FAILURE_EVENT>.PtrToStruct(msg.eventStructPtr, out t10);
						handleFailedDevice(ref t10);
						break;
					}
					case eLeapEventType.eLeapEventType_Policy:
					{
						LEAP_POLICY_EVENT t9;
						StructMarshal<LEAP_POLICY_EVENT>.PtrToStruct(msg.eventStructPtr, out t9);
						handlePolicyChange(ref t9);
						break;
					}
					case eLeapEventType.eLeapEventType_Tracking:
					{
						LEAP_TRACKING_EVENT t8;
						StructMarshal<LEAP_TRACKING_EVENT>.PtrToStruct(msg.eventStructPtr, out t8);
						handleTrackingMessage(ref t8);
						break;
					}
					case eLeapEventType.eLeapEventType_LogEvent:
					{
						LEAP_LOG_EVENT t7;
						StructMarshal<LEAP_LOG_EVENT>.PtrToStruct(msg.eventStructPtr, out t7);
						reportLogMessage(ref t7);
						break;
					}
					case eLeapEventType.eLeapEventType_DeviceLost:
					{
						LEAP_DEVICE_EVENT t6;
						StructMarshal<LEAP_DEVICE_EVENT>.PtrToStruct(msg.eventStructPtr, out t6);
						handleLostDevice(ref t6);
						break;
					}
					case eLeapEventType.eLeapEventType_ConfigChange:
					{
						LEAP_CONFIG_CHANGE_EVENT t5;
						StructMarshal<LEAP_CONFIG_CHANGE_EVENT>.PtrToStruct(msg.eventStructPtr, out t5);
						handleConfigChange(ref t5);
						break;
					}
					case eLeapEventType.eLeapEventType_ConfigResponse:
						handleConfigResponse(ref msg);
						break;
					case eLeapEventType.eLeapEventType_DroppedFrame:
					{
						LEAP_DROPPED_FRAME_EVENT t4;
						StructMarshal<LEAP_DROPPED_FRAME_EVENT>.PtrToStruct(msg.eventStructPtr, out t4);
						handleDroppedFrame(ref t4);
						break;
					}
					case eLeapEventType.eLeapEventType_Image:
					{
						LEAP_IMAGE_EVENT t3;
						StructMarshal<LEAP_IMAGE_EVENT>.PtrToStruct(msg.eventStructPtr, out t3);
						handleImage(ref t3);
						break;
					}
					case eLeapEventType.eLeapEventType_PointMappingChange:
					{
						LEAP_POINT_MAPPING_CHANGE_EVENT t2;
						StructMarshal<LEAP_POINT_MAPPING_CHANGE_EVENT>.PtrToStruct(msg.eventStructPtr, out t2);
						handlePointMappingChange(ref t2);
						break;
					}
					case eLeapEventType.eLeapEventType_HeadPose:
					{
						LEAP_HEAD_POSE_EVENT t;
						StructMarshal<LEAP_HEAD_POSE_EVENT>.PtrToStruct(msg.eventStructPtr, out t);
						handleHeadPoseChange(ref t);
						break;
					}
					}
					if (LeapEndProfilingBlock != null && flag)
					{
						LeapEndProfilingBlock(new EndProfilingBlockArgs("Handle Event"));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("Exception: " + ex);
				_isRunning = false;
			}
			finally
			{
				if (LeapEndProfilingForThread != null && flag)
				{
					LeapEndProfilingForThread(default(EndProfilingForThreadArgs));
				}
			}
		}

		private void handleTrackingMessage(ref LEAP_TRACKING_EVENT trackingMsg)
		{
			Frames.Put(ref trackingMsg);
			if (LeapFrame != null)
			{
				LeapFrame.DispatchOnContext(this, EventContext, new FrameEventArgs(new Leap.Frame().CopyFrom(ref trackingMsg)));
			}
		}

		public ulong GetInterpolatedFrameSize(long time)
		{
			ulong pncbEvent = 0uL;
			eLeapRS frameSize = LeapC.GetFrameSize(_leapConnection, time, out pncbEvent);
			reportAbnormalResults("LeapC get interpolated frame call was ", frameSize);
			return pncbEvent;
		}

		public void GetInterpolatedFrame(Leap.Frame toFill, long time)
		{
			ulong interpolatedFrameSize = GetInterpolatedFrameSize(time);
			IntPtr intPtr = Marshal.AllocHGlobal((int)interpolatedFrameSize);
			eLeapRS eLeapRS2 = LeapC.InterpolateFrame(_leapConnection, time, intPtr, interpolatedFrameSize);
			reportAbnormalResults("LeapC get interpolated frame call was ", eLeapRS2);
			if (eLeapRS2 == eLeapRS.eLeapRS_Success)
			{
				LEAP_TRACKING_EVENT t;
				StructMarshal<LEAP_TRACKING_EVENT>.PtrToStruct(intPtr, out t);
				toFill.CopyFrom(ref t);
			}
			Marshal.FreeHGlobal(intPtr);
		}

		public void GetInterpolatedFrameFromTime(Leap.Frame toFill, long time, long sourceTime)
		{
			ulong interpolatedFrameSize = GetInterpolatedFrameSize(time);
			IntPtr intPtr = Marshal.AllocHGlobal((int)interpolatedFrameSize);
			eLeapRS eLeapRS2 = LeapC.InterpolateFrameFromTime(_leapConnection, time, sourceTime, intPtr, interpolatedFrameSize);
			reportAbnormalResults("LeapC get interpolated frame from time call was ", eLeapRS2);
			if (eLeapRS2 == eLeapRS.eLeapRS_Success)
			{
				LEAP_TRACKING_EVENT t;
				StructMarshal<LEAP_TRACKING_EVENT>.PtrToStruct(intPtr, out t);
				toFill.CopyFrom(ref t);
			}
			Marshal.FreeHGlobal(intPtr);
		}

		public Leap.Frame GetInterpolatedFrame(long time)
		{
			Leap.Frame frame = new Leap.Frame();
			GetInterpolatedFrame(frame, time);
			return frame;
		}

		public void GetInterpolatedHeadPose(ref LEAP_HEAD_POSE_EVENT toFill, long time)
		{
			eLeapRS result = LeapC.InterpolateHeadPose(_leapConnection, time, ref toFill);
			reportAbnormalResults("LeapC get interpolated head pose call was ", result);
		}

		public LEAP_HEAD_POSE_EVENT GetInterpolatedHeadPose(long time)
		{
			LEAP_HEAD_POSE_EVENT toFill = default(LEAP_HEAD_POSE_EVENT);
			GetInterpolatedHeadPose(ref toFill, time);
			return toFill;
		}

		public void GetInterpolatedLeftRightTransform(long time, long sourceTime, long leftId, long rightId, out LeapTransform leftTransform, out LeapTransform rightTransform)
		{
			leftTransform = LeapTransform.Identity;
			rightTransform = LeapTransform.Identity;
			ulong interpolatedFrameSize = GetInterpolatedFrameSize(time);
			IntPtr intPtr = Marshal.AllocHGlobal((int)interpolatedFrameSize);
			eLeapRS eLeapRS2 = LeapC.InterpolateFrameFromTime(_leapConnection, time, sourceTime, intPtr, interpolatedFrameSize);
			reportAbnormalResults("LeapC get interpolated frame from time call was ", eLeapRS2);
			if (eLeapRS2 == eLeapRS.eLeapRS_Success)
			{
				LEAP_TRACKING_EVENT t;
				StructMarshal<LEAP_TRACKING_EVENT>.PtrToStruct(intPtr, out t);
				long num = t.pHands.ToInt64();
				long num2 = num + _handIdOffset;
				long num3 = num + _handPositionOffset;
				long num4 = num + _handOrientationOffset;
				int size = StructMarshal<LEAP_HAND>.Size;
				uint nHands = t.nHands;
				while (nHands-- != 0)
				{
					int num5 = Marshal.ReadInt32(new IntPtr(num2));
					LEAP_VECTOR t2;
					StructMarshal<LEAP_VECTOR>.PtrToStruct(new IntPtr(num3), out t2);
					LEAP_QUATERNION t3;
					StructMarshal<LEAP_QUATERNION>.PtrToStruct(new IntPtr(num4), out t3);
					LeapTransform leapTransform = new LeapTransform(t2.ToLeapVector(), t3.ToLeapQuaternion());
					if (num5 == leftId)
					{
						leftTransform = leapTransform;
					}
					else if (num5 == rightId)
					{
						rightTransform = leapTransform;
					}
					num2 += size;
					num3 += size;
					num4 += size;
				}
			}
			Marshal.FreeHGlobal(intPtr);
		}

		private void handleConnection(ref LEAP_CONNECTION_EVENT connectionMsg)
		{
			if (_leapConnectionEvent != null)
			{
				_leapConnectionEvent.DispatchOnContext(this, EventContext, new ConnectionEventArgs());
			}
		}

		private void handleConnectionLost(ref LEAP_CONNECTION_LOST_EVENT connectionMsg)
		{
			if (LeapConnectionLost != null)
			{
				LeapConnectionLost.DispatchOnContext(this, EventContext, new ConnectionLostEventArgs());
			}
		}

		private void handleDevice(ref LEAP_DEVICE_EVENT deviceMsg)
		{
			IntPtr handle = deviceMsg.device.handle;
			if (handle == IntPtr.Zero)
			{
				return;
			}
			LEAP_DEVICE_INFO info = default(LEAP_DEVICE_INFO);
			IntPtr pDevice;
			if (LeapC.OpenDevice(deviceMsg.device, out pDevice) != 0)
			{
				return;
			}
			info.serial = IntPtr.Zero;
			info.size = (uint)Marshal.SizeOf(info);
			if (LeapC.GetDeviceInfo(pDevice, ref info) != 0)
			{
				return;
			}
			info.serial = Marshal.AllocCoTaskMem((int)info.serial_length);
			if (LeapC.GetDeviceInfo(pDevice, ref info) == eLeapRS.eLeapRS_Success)
			{
				Device device = new Device(handle, info.h_fov, info.v_fov, (float)info.range / 1000f, (float)info.baseline / 1000f, (Device.DeviceType)info.type, info.status == eLeapDeviceStatus.eLeapDeviceStatus_Streaming, Marshal.PtrToStringAnsi(info.serial));
				Marshal.FreeCoTaskMem(info.serial);
				_devices.AddOrUpdate(device);
				if (LeapDevice != null)
				{
					LeapDevice.DispatchOnContext(this, EventContext, new DeviceEventArgs(device));
				}
			}
		}

		private void handleLostDevice(ref LEAP_DEVICE_EVENT deviceMsg)
		{
			Device device = _devices.FindDeviceByHandle(deviceMsg.device.handle);
			if (device != null)
			{
				_devices.Remove(device);
				if (LeapDeviceLost != null)
				{
					LeapDeviceLost.DispatchOnContext(this, EventContext, new DeviceEventArgs(device));
				}
			}
		}

		private void handleFailedDevice(ref LEAP_DEVICE_FAILURE_EVENT deviceMsg)
		{
			string serial = "Unavailable";
			string message;
			switch (deviceMsg.status)
			{
			case eLeapDeviceStatus.eLeapDeviceStatus_BadCalibration:
				message = "Bad Calibration. Device failed because of a bad calibration record.";
				break;
			case eLeapDeviceStatus.eLeapDeviceStatus_BadControl:
				message = "Bad Control Interface. Device failed because of a USB control interface error.";
				break;
			case eLeapDeviceStatus.eLeapDeviceStatus_BadFirmware:
				message = "Bad Firmware. Device failed because of a firmware error.";
				break;
			case eLeapDeviceStatus.eLeapDeviceStatus_BadTransport:
				message = "Bad Transport. Device failed because of a USB communication error.";
				break;
			default:
				message = "Device failed for an unknown reason";
				break;
			}
			Device device = _devices.FindDeviceByHandle(deviceMsg.hDevice);
			if (device != null)
			{
				_devices.Remove(device);
			}
			if (LeapDeviceFailure != null)
			{
				LeapDeviceFailure.DispatchOnContext(this, EventContext, new DeviceFailureEventArgs((uint)deviceMsg.status, message, serial));
			}
		}

		private void handleConfigChange(ref LEAP_CONFIG_CHANGE_EVENT configEvent)
		{
			string value = string.Empty;
			_configRequests.TryGetValue(configEvent.requestId, out value);
			if (value != null)
			{
				_configRequests.Remove(configEvent.requestId);
			}
			if (LeapConfigChange != null)
			{
				LeapConfigChange.DispatchOnContext(this, EventContext, new ConfigChangeEventArgs(value, configEvent.status, configEvent.requestId));
			}
		}

		private void handleConfigResponse(ref LEAP_CONNECTION_MESSAGE configMsg)
		{
			LEAP_CONFIG_RESPONSE_EVENT t;
			StructMarshal<LEAP_CONFIG_RESPONSE_EVENT>.PtrToStruct(configMsg.eventStructPtr, out t);
			string value = string.Empty;
			_configRequests.TryGetValue(t.requestId, out value);
			if (value != null)
			{
				_configRequests.Remove(t.requestId);
			}
			uint requestId = t.requestId;
			Config.ValueType dataType;
			object value2;
			if (t.value.type != eLeapValueType.eLeapValueType_String)
			{
				switch (t.value.type)
				{
				case eLeapValueType.eLeapValueType_Boolean:
					dataType = Config.ValueType.TYPE_BOOLEAN;
					value2 = t.value.boolValue;
					break;
				case eLeapValueType.eLeapValueType_Int32:
					dataType = Config.ValueType.TYPE_INT32;
					value2 = t.value.intValue;
					break;
				case eLeapValueType.eLeapValueType_Float:
					dataType = Config.ValueType.TYPE_FLOAT;
					value2 = t.value.floatValue;
					break;
				default:
					dataType = Config.ValueType.TYPE_UNKNOWN;
					value2 = new object();
					break;
				}
			}
			else
			{
				LEAP_CONFIG_RESPONSE_EVENT_WITH_REF_TYPE t2;
				StructMarshal<LEAP_CONFIG_RESPONSE_EVENT_WITH_REF_TYPE>.PtrToStruct(configMsg.eventStructPtr, out t2);
				dataType = Config.ValueType.TYPE_STRING;
				value2 = t2.value.stringValue;
			}
			SetConfigResponseEventArgs eventArgs = new SetConfigResponseEventArgs(value, dataType, value2, requestId);
			if (LeapConfigResponse != null)
			{
				LeapConfigResponse.DispatchOnContext(this, EventContext, eventArgs);
			}
		}

		private void reportLogMessage(ref LEAP_LOG_EVENT logMsg)
		{
			if (LeapLogEvent != null)
			{
				LeapLogEvent.DispatchOnContext(this, EventContext, new LogEventArgs(publicSeverity(logMsg.severity), logMsg.timestamp, Marshal.PtrToStringAnsi(logMsg.message)));
			}
		}

		private MessageSeverity publicSeverity(eLeapLogSeverity leapCSeverity)
		{
			switch (leapCSeverity)
			{
			case eLeapLogSeverity.eLeapLogSeverity_Unknown:
				return MessageSeverity.MESSAGE_UNKNOWN;
			case eLeapLogSeverity.eLeapLogSeverity_Information:
				return MessageSeverity.MESSAGE_INFORMATION;
			case eLeapLogSeverity.eLeapLogSeverity_Warning:
				return MessageSeverity.MESSAGE_WARNING;
			case eLeapLogSeverity.eLeapLogSeverity_Critical:
				return MessageSeverity.MESSAGE_CRITICAL;
			default:
				return MessageSeverity.MESSAGE_UNKNOWN;
			}
		}

		private void handlePointMappingChange(ref LEAP_POINT_MAPPING_CHANGE_EVENT pointMapping)
		{
			if (LeapPointMappingChange != null)
			{
				LeapPointMappingChange.DispatchOnContext(this, EventContext, new PointMappingChangeEventArgs(pointMapping.frame_id, pointMapping.timestamp, pointMapping.nPoints));
			}
		}

		private void handleDroppedFrame(ref LEAP_DROPPED_FRAME_EVENT droppedFrame)
		{
			if (LeapDroppedFrame != null)
			{
				LeapDroppedFrame.DispatchOnContext(this, EventContext, new DroppedFrameEventArgs(droppedFrame.frame_id, droppedFrame.reason));
			}
		}

		private void handleHeadPoseChange(ref LEAP_HEAD_POSE_EVENT headPose)
		{
			if (LeapHeadPoseChange != null)
			{
				LeapHeadPoseChange.DispatchOnContext(this, EventContext, new HeadPoseEventArgs(headPose.head_position, headPose.head_orientation));
			}
		}

		private DistortionData createDistortionData(LEAP_IMAGE image, Image.CameraType camera)
		{
			DistortionData distortionData = new DistortionData();
			distortionData.Version = image.matrix_version;
			distortionData.Width = LeapC.DistortionSize;
			distortionData.Height = LeapC.DistortionSize;
			distortionData.Data = new float[(int)(distortionData.Width * distortionData.Height * 2f)];
			Marshal.Copy(image.distortionMatrix, distortionData.Data, 0, distortionData.Data.Length);
			if (LeapDistortionChange != null)
			{
				LeapDistortionChange.DispatchOnContext(this, EventContext, new DistortionEventArgs(distortionData, camera));
			}
			return distortionData;
		}

		private void handleImage(ref LEAP_IMAGE_EVENT imageMsg)
		{
			if (LeapImage != null)
			{
				if (_currentLeftDistortionData.Version != imageMsg.leftImage.matrix_version || !_currentLeftDistortionData.IsValid)
				{
					_currentLeftDistortionData = createDistortionData(imageMsg.leftImage, Image.CameraType.LEFT);
				}
				if (_currentRightDistortionData.Version != imageMsg.rightImage.matrix_version || !_currentRightDistortionData.IsValid)
				{
					_currentRightDistortionData = createDistortionData(imageMsg.rightImage, Image.CameraType.RIGHT);
				}
				ImageData leftImage = new ImageData(Image.CameraType.LEFT, imageMsg.leftImage, _currentLeftDistortionData);
				ImageData rightImage = new ImageData(Image.CameraType.RIGHT, imageMsg.rightImage, _currentRightDistortionData);
				Image image = new Image(imageMsg.info.frame_id, imageMsg.info.timestamp, leftImage, rightImage);
				LeapImage.DispatchOnContext(this, EventContext, new ImageEventArgs(image));
			}
		}

		private void handlePolicyChange(ref LEAP_POLICY_EVENT policyMsg)
		{
			if (LeapPolicyChange != null)
			{
				LeapPolicyChange.DispatchOnContext(this, EventContext, new PolicyEventArgs(policyMsg.current_policy, _activePolicies));
			}
			_activePolicies = policyMsg.current_policy;
			if (_activePolicies == _requestedPolicies)
			{
			}
		}

		public void SetPolicy(Controller.PolicyFlag policy)
		{
			ulong num = (ulong)flagForPolicy(policy);
			_requestedPolicies |= num;
			num = _requestedPolicies;
			ulong clear = ~_requestedPolicies;
			eLeapRS result = LeapC.SetPolicyFlags(_leapConnection, num, clear);
			reportAbnormalResults("LeapC SetPolicyFlags call was ", result);
		}

		public void ClearPolicy(Controller.PolicyFlag policy)
		{
			ulong num = (ulong)flagForPolicy(policy);
			_requestedPolicies &= ~num;
			eLeapRS result = LeapC.SetPolicyFlags(_leapConnection, _requestedPolicies, ~_requestedPolicies);
			reportAbnormalResults("LeapC SetPolicyFlags call was ", result);
		}

		private eLeapPolicyFlag flagForPolicy(Controller.PolicyFlag singlePolicy)
		{
			switch (singlePolicy)
			{
			case Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES:
				return eLeapPolicyFlag.eLeapPolicyFlag_BackgroundFrames;
			case Controller.PolicyFlag.POLICY_IMAGES:
				return eLeapPolicyFlag.eLeapPolicyFlag_Images;
			case Controller.PolicyFlag.POLICY_OPTIMIZE_HMD:
				return eLeapPolicyFlag.eLeapPolicyFlag_OptimizeHMD;
			case Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME:
				return eLeapPolicyFlag.eLeapPolicyFlag_AllowPauseResume;
			case Controller.PolicyFlag.POLICY_MAP_POINTS:
				return eLeapPolicyFlag.eLeapPolicyFlag_MapPoints;
			case Controller.PolicyFlag.POLICY_DEFAULT:
				return (eLeapPolicyFlag)0u;
			default:
				return (eLeapPolicyFlag)0u;
			}
		}

		public bool IsPolicySet(Controller.PolicyFlag policy)
		{
			ulong num = (ulong)flagForPolicy(policy);
			return (_activePolicies & num) == num;
		}

		public uint GetConfigValue(string config_key)
		{
			uint request_id = 0u;
			eLeapRS result = LeapC.RequestConfigValue(_leapConnection, config_key, out request_id);
			reportAbnormalResults("LeapC RequestConfigValue call was ", result);
			_configRequests[request_id] = config_key;
			return request_id;
		}

		public uint SetConfigValue<T>(string config_key, T value) where T : IConvertible
		{
			uint requestId = 0u;
			Type type = value.GetType();
			eLeapRS result;
			if (type == typeof(bool))
			{
				result = LeapC.SaveConfigValue(_leapConnection, config_key, Convert.ToBoolean(value), out requestId);
			}
			else if (type == typeof(int))
			{
				result = LeapC.SaveConfigValue(_leapConnection, config_key, Convert.ToInt32(value), out requestId);
			}
			else if (type == typeof(float))
			{
				result = LeapC.SaveConfigValue(_leapConnection, config_key, Convert.ToSingle(value), out requestId);
			}
			else
			{
				if (type != typeof(string))
				{
					throw new ArgumentException("Only boolean, Int32, float, and string types are supported.");
				}
				result = LeapC.SaveConfigValue(_leapConnection, config_key, Convert.ToString(value), out requestId);
			}
			reportAbnormalResults("LeapC SaveConfigValue call was ", result);
			_configRequests[requestId] = config_key;
			return requestId;
		}

		public Vector PixelToRectilinear(Image.CameraType camera, Vector pixel)
		{
			LEAP_VECTOR lEAP_VECTOR = LeapC.LeapPixelToRectilinear(pixel: new LEAP_VECTOR(pixel), hConnection: _leapConnection, camera: (camera == Image.CameraType.LEFT) ? eLeapPerspectiveType.eLeapPerspectiveType_stereo_left : eLeapPerspectiveType.eLeapPerspectiveType_stereo_right);
			return new Vector(lEAP_VECTOR.x, lEAP_VECTOR.y, lEAP_VECTOR.z);
		}

		public Vector RectilinearToPixel(Image.CameraType camera, Vector ray)
		{
			LEAP_VECTOR lEAP_VECTOR = LeapC.LeapRectilinearToPixel(rectilinear: new LEAP_VECTOR(ray), hConnection: _leapConnection, camera: (camera == Image.CameraType.LEFT) ? eLeapPerspectiveType.eLeapPerspectiveType_stereo_left : eLeapPerspectiveType.eLeapPerspectiveType_stereo_right);
			return new Vector(lEAP_VECTOR.x, lEAP_VECTOR.y, lEAP_VECTOR.z);
		}

		public void TelemetryProfiling(ref LEAP_TELEMETRY_DATA telemetryData)
		{
			eLeapRS result = LeapC.LeapTelemetryProfiling(_leapConnection, ref telemetryData);
			reportAbnormalResults("LeapC TelemetryProfiling call was ", result);
		}

		public void GetPointMapping(ref PointMapping pm)
		{
			ulong pSize = 0uL;
			IntPtr intPtr = IntPtr.Zero;
			eLeapRS pointMapping;
			while (true)
			{
				pointMapping = LeapC.GetPointMapping(_leapConnection, intPtr, ref pSize);
				if (pointMapping != eLeapRS.eLeapRS_InsufficientBuffer)
				{
					break;
				}
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
				intPtr = Marshal.AllocHGlobal((int)pSize);
			}
			reportAbnormalResults("LeapC get point mapping call was ", pointMapping);
			if (pointMapping != 0)
			{
				pm.points = null;
				pm.ids = null;
				return;
			}
			LEAP_POINT_MAPPING t;
			StructMarshal<LEAP_POINT_MAPPING>.PtrToStruct(intPtr, out t);
			int nPoints = (int)t.nPoints;
			pm.frameId = t.frame_id;
			pm.timestamp = t.timestamp;
			pm.points = new Vector[nPoints];
			pm.ids = new uint[nPoints];
			float[] array = new float[3 * nPoints];
			int[] array2 = new int[nPoints];
			Marshal.Copy(t.points, array, 0, 3 * nPoints);
			Marshal.Copy(t.ids, array2, 0, nPoints);
			int num = 0;
			for (int i = 0; i < nPoints; i++)
			{
				pm.points[i].x = array[num++];
				pm.points[i].y = array[num++];
				pm.points[i].z = array[num++];
				pm.ids[i] = (uint)array2[i];
			}
			Marshal.FreeHGlobal(intPtr);
		}

		private void reportAbnormalResults(string context, eLeapRS result)
		{
			if (result != 0 && result != _lastResult)
			{
				string message = context + " " + result;
				if (LeapLogEvent != null)
				{
					LeapLogEvent.DispatchOnContext(this, EventContext, new LogEventArgs(MessageSeverity.MESSAGE_CRITICAL, LeapC.GetNow(), message));
				}
				_lastResult = result;
			}
		}
	}
}
