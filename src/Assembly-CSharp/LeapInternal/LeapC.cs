using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	public class LeapC
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LEAP_RECORDING_PARAMETERS
		{
			public uint mode;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LEAP_RECORDING_STATUS
		{
			public uint mode;
		}

		public static int DistortionSize = 64;

		private LeapC()
		{
		}

		[DllImport("LeapC", EntryPoint = "LeapGetNow")]
		public static extern long GetNow();

		[DllImport("LeapC", EntryPoint = "LeapCreateClockRebaser")]
		public static extern eLeapRS CreateClockRebaser(out IntPtr phClockRebaser);

		[DllImport("LeapC", EntryPoint = "LeapDestroyClockRebaser")]
		public static extern eLeapRS DestroyClockRebaser(IntPtr hClockRebaser);

		[DllImport("LeapC", EntryPoint = "LeapUpdateRebase")]
		public static extern eLeapRS UpdateRebase(IntPtr hClockRebaser, long userClock, long leapClock);

		[DllImport("LeapC", EntryPoint = "LeapRebaseClock")]
		public static extern eLeapRS RebaseClock(IntPtr hClockRebaser, long userClock, out long leapClock);

		[DllImport("LeapC", EntryPoint = "LeapCreateConnection")]
		public static extern eLeapRS CreateConnection(ref LEAP_CONNECTION_CONFIG pConfig, out IntPtr pConnection);

		[DllImport("LeapC", EntryPoint = "LeapCreateConnection")]
		private static extern eLeapRS CreateConnection(IntPtr nulled, out IntPtr pConnection);

		public static eLeapRS CreateConnection(out IntPtr pConnection)
		{
			return CreateConnection(IntPtr.Zero, out pConnection);
		}

		[DllImport("LeapC", EntryPoint = "LeapGetConnectionInfo")]
		public static extern eLeapRS GetConnectionInfo(IntPtr hConnection, ref LEAP_CONNECTION_INFO pInfo);

		[DllImport("LeapC", EntryPoint = "LeapOpenConnection")]
		public static extern eLeapRS OpenConnection(IntPtr hConnection);

		[DllImport("LeapC", EntryPoint = "LeapSetAllocator")]
		public static extern eLeapRS SetAllocator(IntPtr hConnection, ref LEAP_ALLOCATOR pAllocator);

		[DllImport("LeapC", EntryPoint = "LeapGetDeviceList")]
		public static extern eLeapRS GetDeviceList(IntPtr hConnection, [In][Out] LEAP_DEVICE_REF[] pArray, out uint pnArray);

		[DllImport("LeapC", EntryPoint = "LeapGetDeviceList")]
		private static extern eLeapRS GetDeviceList(IntPtr hConnection, [In][Out] IntPtr pArray, out uint pnArray);

		public static eLeapRS GetDeviceCount(IntPtr hConnection, out uint deviceCount)
		{
			return GetDeviceList(hConnection, IntPtr.Zero, out deviceCount);
		}

		[DllImport("LeapC", EntryPoint = "LeapOpenDevice")]
		public static extern eLeapRS OpenDevice(LEAP_DEVICE_REF rDevice, out IntPtr pDevice);

		[DllImport("LeapC", CharSet = CharSet.Ansi, EntryPoint = "LeapGetDeviceInfo")]
		public static extern eLeapRS GetDeviceInfo(IntPtr hDevice, ref LEAP_DEVICE_INFO info);

		[DllImport("LeapC", EntryPoint = "LeapSetPolicyFlags")]
		public static extern eLeapRS SetPolicyFlags(IntPtr hConnection, ulong set, ulong clear);

		[DllImport("LeapC", EntryPoint = "LeapSetDeviceFlags")]
		public static extern eLeapRS SetDeviceFlags(IntPtr hDevice, ulong set, ulong clear, out ulong prior);

		[DllImport("LeapC", EntryPoint = "LeapPollConnection")]
		public static extern eLeapRS PollConnection(IntPtr hConnection, uint timeout, ref LEAP_CONNECTION_MESSAGE msg);

		[DllImport("LeapC", EntryPoint = "LeapGetFrameSize")]
		public static extern eLeapRS GetFrameSize(IntPtr hConnection, long timestamp, out ulong pncbEvent);

		[DllImport("LeapC", EntryPoint = "LeapInterpolateFrame")]
		public static extern eLeapRS InterpolateFrame(IntPtr hConnection, long timestamp, IntPtr pEvent, ulong ncbEvent);

		[DllImport("LeapC", EntryPoint = "LeapInterpolateFrameFromTime")]
		public static extern eLeapRS InterpolateFrameFromTime(IntPtr hConnection, long timestamp, long sourceTimestamp, IntPtr pEvent, ulong ncbEvent);

		[DllImport("LeapC", EntryPoint = "LeapInterpolateHeadPose")]
		public static extern eLeapRS InterpolateHeadPose(IntPtr hConnection, long timestamp, ref LEAP_HEAD_POSE_EVENT headPose);

		[DllImport("LeapC")]
		public static extern LEAP_VECTOR LeapPixelToRectilinear(IntPtr hConnection, eLeapPerspectiveType camera, LEAP_VECTOR pixel);

		[DllImport("LeapC")]
		public static extern LEAP_VECTOR LeapRectilinearToPixel(IntPtr hConnection, eLeapPerspectiveType camera, LEAP_VECTOR rectilinear);

		[DllImport("LeapC", EntryPoint = "LeapCloseDevice")]
		public static extern void CloseDevice(IntPtr pDevice);

		[DllImport("LeapC", EntryPoint = "LeapCloseConnection")]
		public static extern eLeapRS CloseConnection(IntPtr hConnection);

		[DllImport("LeapC", EntryPoint = "LeapDestroyConnection")]
		public static extern void DestroyConnection(IntPtr connection);

		[DllImport("LeapC", EntryPoint = "LeapSaveConfigValue")]
		private static extern eLeapRS SaveConfigValue(IntPtr hConnection, string key, IntPtr value, out uint requestId);

		[DllImport("LeapC", EntryPoint = "LeapRequestConfigValue")]
		public static extern eLeapRS RequestConfigValue(IntPtr hConnection, string name, out uint request_id);

		public static eLeapRS SaveConfigValue(IntPtr hConnection, string key, bool value, out uint requestId)
		{
			LEAP_VARIANT_VALUE_TYPE valueStruct = default(LEAP_VARIANT_VALUE_TYPE);
			valueStruct.type = eLeapValueType.eLeapValueType_Boolean;
			valueStruct.boolValue = (value ? 1 : 0);
			return SaveConfigWithValueType(hConnection, key, valueStruct, out requestId);
		}

		public static eLeapRS SaveConfigValue(IntPtr hConnection, string key, int value, out uint requestId)
		{
			LEAP_VARIANT_VALUE_TYPE valueStruct = default(LEAP_VARIANT_VALUE_TYPE);
			valueStruct.type = eLeapValueType.eLeapValueType_Int32;
			valueStruct.intValue = value;
			return SaveConfigWithValueType(hConnection, key, valueStruct, out requestId);
		}

		public static eLeapRS SaveConfigValue(IntPtr hConnection, string key, float value, out uint requestId)
		{
			LEAP_VARIANT_VALUE_TYPE valueStruct = default(LEAP_VARIANT_VALUE_TYPE);
			valueStruct.type = eLeapValueType.eLeapValueType_Float;
			valueStruct.floatValue = value;
			return SaveConfigWithValueType(hConnection, key, valueStruct, out requestId);
		}

		public static eLeapRS SaveConfigValue(IntPtr hConnection, string key, string value, out uint requestId)
		{
			LEAP_VARIANT_REF_TYPE valueStruct = default(LEAP_VARIANT_REF_TYPE);
			valueStruct.type = eLeapValueType.eLeapValueType_String;
			valueStruct.stringValue = value;
			return SaveConfigWithRefType(hConnection, key, valueStruct, out requestId);
		}

		private static eLeapRS SaveConfigWithValueType(IntPtr hConnection, string key, LEAP_VARIANT_VALUE_TYPE valueStruct, out uint requestId)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(valueStruct));
			eLeapRS eLeapRS2 = eLeapRS.eLeapRS_UnknownError;
			try
			{
				Marshal.StructureToPtr(valueStruct, intPtr, false);
				return SaveConfigValue(hConnection, key, intPtr, out requestId);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		private static eLeapRS SaveConfigWithRefType(IntPtr hConnection, string key, LEAP_VARIANT_REF_TYPE valueStruct, out uint requestId)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(valueStruct));
			eLeapRS eLeapRS2 = eLeapRS.eLeapRS_UnknownError;
			try
			{
				Marshal.StructureToPtr(valueStruct, intPtr, false);
				return SaveConfigValue(hConnection, key, intPtr, out requestId);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		[DllImport("LeapC", EntryPoint = "LeapGetPointMappingSize")]
		public static extern eLeapRS GetPointMappingSize(IntPtr hConnection, ref ulong pSize);

		[DllImport("LeapC", EntryPoint = "LeapGetPointMapping")]
		public static extern eLeapRS GetPointMapping(IntPtr hConnection, IntPtr pointMapping, ref ulong pSize);

		[DllImport("LeapC", EntryPoint = "LeapRecordingOpen")]
		public static extern eLeapRS RecordingOpen(ref IntPtr ppRecording, string userPath, LEAP_RECORDING_PARAMETERS parameters);

		[DllImport("LeapC", EntryPoint = "LeapRecordingClose")]
		public static extern eLeapRS RecordingClose(ref IntPtr ppRecording);

		[DllImport("LeapC")]
		public static extern eLeapRS LeapRecordingGetStatus(IntPtr pRecording, ref LEAP_RECORDING_STATUS status);

		[DllImport("LeapC", EntryPoint = "LeapRecordingReadSize")]
		public static extern eLeapRS RecordingReadSize(IntPtr pRecording, ref ulong pncbEvent);

		[DllImport("LeapC", EntryPoint = "LeapRecordingRead")]
		public static extern eLeapRS RecordingRead(IntPtr pRecording, ref LEAP_TRACKING_EVENT pEvent, ulong ncbEvent);

		[DllImport("LeapC", EntryPoint = "LeapRecordingWrite")]
		public static extern eLeapRS RecordingWrite(IntPtr pRecording, ref LEAP_TRACKING_EVENT pEvent, ref ulong pnBytesWritten);

		[DllImport("LeapC")]
		public static extern eLeapRS LeapTelemetryProfiling(IntPtr hConnection, ref LEAP_TELEMETRY_DATA telemetryData);

		[DllImport("LeapC", EntryPoint = "LeapTelemetryGetNow")]
		public static extern ulong TelemetryGetNow();
	}
}
