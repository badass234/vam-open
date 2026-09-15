using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Oculus.Platform
{
	public class CAPI
	{
		public struct ovrKeyValuePair
		{
			public string key_;

			private KeyValuePairType valueType_;

			public string stringValue_;

			public int intValue_;

			public double doubleValue_;

			public ovrKeyValuePair(string key, string value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.String;
				stringValue_ = value;
				intValue_ = 0;
				doubleValue_ = 0.0;
			}

			public ovrKeyValuePair(string key, int value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.Int;
				intValue_ = value;
				stringValue_ = null;
				doubleValue_ = 0.0;
			}

			public ovrKeyValuePair(string key, double value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.Double;
				doubleValue_ = value;
				stringValue_ = null;
				intValue_ = 0;
			}
		}

		public struct ovrMatchmakingCriterion
		{
			public string key_;

			public MatchmakingCriterionImportance importance_;

			public IntPtr parameterArray;

			public uint parameterArrayCount;

			public ovrMatchmakingCriterion(string key, MatchmakingCriterionImportance importance)
			{
				key_ = key;
				importance_ = importance;
				parameterArray = IntPtr.Zero;
				parameterArrayCount = 0u;
			}
		}

		public struct ovrMatchmakingCustomQueryData
		{
			public IntPtr dataArray;

			public uint dataArrayCount;

			public IntPtr criterionArray;

			public uint criterionArrayCount;
		}

		public struct OculusInitParams
		{
			public int sType;

			public string email;

			public string password;

			public ulong appId;

			public string uriPrefixOverride;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void FilterCallback([In][Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 480)] short[] pcmData, UIntPtr pcmDataLength, int frequency, int numChannels);

		public const string DLL_NAME = "LibOVRPlatform64_1";

		private static UTF8Encoding nativeStringEncoding = new UTF8Encoding(false);

		public const int VoipFilterBufferSize = 480;

		public static IntPtr ArrayOfStructsToIntPtr(Array ar)
		{
			int num = 0;
			for (int i = 0; i < ar.Length; i++)
			{
				num += Marshal.SizeOf(ar.GetValue(i));
			}
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			IntPtr intPtr2 = intPtr;
			for (int j = 0; j < ar.Length; j++)
			{
				Marshal.StructureToPtr(ar.GetValue(j), intPtr2, false);
				intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf(ar.GetValue(j)));
			}
			return intPtr;
		}

		public static ovrKeyValuePair[] DictionaryToOVRKeyValuePairs(Dictionary<string, object> dict)
		{
			if (dict == null || dict.Count == 0)
			{
				return null;
			}
			ovrKeyValuePair[] array = new ovrKeyValuePair[dict.Count];
			int num = 0;
			foreach (KeyValuePair<string, object> item in dict)
			{
				if (item.Value.GetType() == typeof(int))
				{
					array[num] = new ovrKeyValuePair(item.Key, (int)item.Value);
				}
				else if (item.Value.GetType() == typeof(string))
				{
					array[num] = new ovrKeyValuePair(item.Key, (string)item.Value);
				}
				else
				{
					if (item.Value.GetType() != typeof(double))
					{
						throw new Exception("Only int, double or string are allowed types in CustomQuery.data");
					}
					array[num] = new ovrKeyValuePair(item.Key, (double)item.Value);
				}
				num++;
			}
			return array;
		}

		public static byte[] IntPtrToByteArray(IntPtr data, ulong size)
		{
			byte[] array = new byte[size];
			Marshal.Copy(data, array, 0, (int)size);
			return array;
		}

		public static Dictionary<string, string> DataStoreFromNative(IntPtr pointer)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = (int)(uint)ovr_DataStore_GetNumKeys(pointer);
			for (int i = 0; i < num; i++)
			{
				string key = ovr_DataStore_GetKey(pointer, i);
				dictionary[key] = ovr_DataStore_GetValue(pointer, key);
			}
			return dictionary;
		}

		public static string StringFromNative(IntPtr pointer)
		{
			if (pointer == IntPtr.Zero)
			{
				return null;
			}
			int nativeStringLengthNotIncludingNullTerminator = GetNativeStringLengthNotIncludingNullTerminator(pointer);
			byte[] array = new byte[nativeStringLengthNotIncludingNullTerminator];
			Marshal.Copy(pointer, array, 0, nativeStringLengthNotIncludingNullTerminator);
			return nativeStringEncoding.GetString(array);
		}

		public static int GetNativeStringLengthNotIncludingNullTerminator(IntPtr pointer)
		{
			int i;
			for (i = 0; Marshal.ReadByte(pointer, i) != 0; i++)
			{
			}
			return i;
		}

		public static DateTime DateTimeFromNative(ulong seconds_since_the_one_true_epoch)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(seconds_since_the_one_true_epoch).ToLocalTime();
		}

		public static byte[] BlobFromNative(uint size, IntPtr pointer)
		{
			byte[] array = new byte[size];
			for (int i = 0; i < (int)size; i++)
			{
				array[i] = Marshal.ReadByte(pointer, i);
			}
			return array;
		}

		public static byte[] FiledataFromNative(uint size, IntPtr pointer)
		{
			byte[] array = new byte[size];
			Marshal.Copy(pointer, array, 0, (int)size);
			return array;
		}

		public static IntPtr StringToNative(string s)
		{
			if (s == null)
			{
				throw new Exception("StringFromNative: null argument");
			}
			int byteCount = nativeStringEncoding.GetByteCount(s);
			byte[] array = new byte[byteCount + 1];
			nativeStringEncoding.GetBytes(s, 0, s.Length, array, 0);
			IntPtr intPtr = Marshal.AllocCoTaskMem(byteCount + 1);
			Marshal.Copy(array, 0, intPtr, byteCount + 1);
			return intPtr;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_UnityInitWrapper(string appId);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_UnityInitGlobals(IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_UnityInitWrapperAsynchronous(string appId);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_UnityInitWrapperStandalone(string accessToken, IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Platform_InitializeStandaloneOculus(ref OculusInitParams init);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_UnityInitWrapperWindows(string appId, IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_UnityInitWrapperWindowsAsynchronous(string appId, IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_SetDeveloperAccessToken(string accessToken);

		public static string ovr_GetLoggedInUserLocale()
		{
			return StringFromNative(ovr_GetLoggedInUserLocale_Native());
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GetLoggedInUserLocale")]
		private static extern IntPtr ovr_GetLoggedInUserLocale_Native();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_PopMessage();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_FreeMessage(IntPtr message);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_NetworkingPeer_GetSendPolicy(IntPtr networkingPeer);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Voip_CreateEncoder();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_DestroyEncoder(IntPtr encoder);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Voip_CreateDecoder();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_DestroyDecoder(IntPtr decoder);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_VoipDecoder_Decode(IntPtr obj, byte[] compressedData, ulong compressedSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Microphone_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Microphone_Destroy(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetSystemVoipPassthrough(bool passthrough);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetSystemVoipMicrophoneMuted(VoipMuteState muted);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_UnityResetTestPlatform();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_HTTP_GetWithMessageType(string url, int messageType);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_CrashApplication();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetMicrophoneFilterCallback(FilterCallback cb);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetMicrophoneFilterCallbackWithFixedSizeBuffer(FilterCallback cb, UIntPtr bufferSizeElements);

		public static void LogNewEvent(string eventName, Dictionary<string, string> values)
		{
			IntPtr intPtr = StringToNative(eventName);
			int num = values?.Count ?? 0;
			IntPtr[] array = new IntPtr[num * 2];
			if (num > 0)
			{
				int num2 = 0;
				foreach (KeyValuePair<string, string> value in values)
				{
					array[num2 * 2] = StringToNative(value.Key);
					array[num2 * 2 + 1] = StringToNative(value.Value);
					num2++;
				}
			}
			ovr_Log_NewEvent(intPtr, array, (UIntPtr)(ulong)num);
			Marshal.FreeCoTaskMem(intPtr);
			IntPtr[] array2 = array;
			foreach (IntPtr ptr in array2)
			{
				Marshal.FreeCoTaskMem(ptr);
			}
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Log_NewEvent(IntPtr eventName, IntPtr[] values, UIntPtr length);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_ApplicationLifecycle_GetLaunchDetails();

		public static ulong ovr_HTTP_StartTransfer(string url, ovrKeyValuePair[] headers)
		{
			IntPtr intPtr = StringToNative(url);
			UIntPtr numItems = (UIntPtr)(ulong)headers.Length;
			ulong result = ovr_HTTP_StartTransfer_Native(intPtr, headers, numItems);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_StartTransfer")]
		private static extern ulong ovr_HTTP_StartTransfer_Native(IntPtr url, ovrKeyValuePair[] headers, UIntPtr numItems);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_HTTP_Write(ulong transferId, byte[] bytes, UIntPtr length);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_HTTP_WriteEOM(ulong transferId);

		public static string ovr_Message_GetStringForJavascript(IntPtr message)
		{
			return StringFromNative(ovr_Message_GetStringForJavascript_Native(message));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Message_GetStringForJavascript")]
		private static extern IntPtr ovr_Message_GetStringForJavascript_Native(IntPtr message);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Net_Accept(ulong peerID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Net_AcceptForCurrentRoom();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Net_Close(ulong peerID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Net_CloseForCurrentRoom();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Net_Connect(ulong peerID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Net_IsConnected(ulong peerID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Net_Ping(ulong peerID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Net_ReadPacket();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Net_SendPacket(ulong userID, UIntPtr length, byte[] bytes, SendPolicy policy);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Net_SendPacketToCurrentRoom(UIntPtr length, byte[] bytes, SendPolicy policy);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_Accept(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetOutputBufferMaxSize();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetPCM(ulong senderID, short[] outputBuffer, UIntPtr outputBufferNumElements);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetPCMFloat(ulong senderID, float[] outputBuffer, UIntPtr outputBufferNumElements);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetPCMSize(ulong senderID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetPCMWithTimestamp(ulong senderID, short[] outputBuffer, UIntPtr outputBufferNumElements, uint[] timestamp);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Voip_GetPCMWithTimestampFloat(ulong senderID, float[] outputBuffer, UIntPtr outputBufferNumElements, uint[] timestamp);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_Voip_GetSyncTimestamp(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern long ovr_Voip_GetSyncTimestampDifference(uint lhs, uint rhs);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern VoipMuteState ovr_Voip_GetSystemVoipMicrophoneMuted();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemVoipStatus ovr_Voip_GetSystemVoipStatus();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetMicrophoneMuted(VoipMuteState state);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_SetOutputSampleRate(VoipSampleRate rate);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_Start(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Voip_Stop(ulong userID);

		public static ulong ovr_Achievements_AddCount(string name, ulong count)
		{
			IntPtr intPtr = StringToNative(name);
			ulong result = ovr_Achievements_AddCount_Native(intPtr, count);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_AddCount")]
		private static extern ulong ovr_Achievements_AddCount_Native(IntPtr name, ulong count);

		public static ulong ovr_Achievements_AddFields(string name, string fields)
		{
			IntPtr intPtr = StringToNative(name);
			IntPtr intPtr2 = StringToNative(fields);
			ulong result = ovr_Achievements_AddFields_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_AddFields")]
		private static extern ulong ovr_Achievements_AddFields_Native(IntPtr name, IntPtr fields);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Achievements_GetAllDefinitions();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Achievements_GetAllProgress();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Achievements_GetDefinitionsByName(string[] names, int count);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Achievements_GetProgressByName(string[] names, int count);

		public static ulong ovr_Achievements_Unlock(string name)
		{
			IntPtr intPtr = StringToNative(name);
			ulong result = ovr_Achievements_Unlock_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_Unlock")]
		private static extern ulong ovr_Achievements_Unlock_Native(IntPtr name);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Application_ExecuteCoordinatedLaunch(ulong appID, ulong roomID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Application_GetInstalledApplications();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Application_GetVersion();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Application_LaunchOtherApp(ulong appID, IntPtr deeplink_options);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_ApplicationLifecycle_GetRegisteredPIDs();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_ApplicationLifecycle_GetSessionKey();

		public static ulong ovr_ApplicationLifecycle_RegisterSessionKey(string sessionKey)
		{
			IntPtr intPtr = StringToNative(sessionKey);
			ulong result = ovr_ApplicationLifecycle_RegisterSessionKey_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationLifecycle_RegisterSessionKey")]
		private static extern ulong ovr_ApplicationLifecycle_RegisterSessionKey_Native(IntPtr sessionKey);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFile_Delete(ulong assetFileID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFile_Download(ulong assetFileID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFile_DownloadCancel(ulong assetFileID);

		public static ulong ovr_Avatar_UpdateMetaData(string avatarMetaData, string imageFilePath)
		{
			IntPtr intPtr = StringToNative(avatarMetaData);
			IntPtr intPtr2 = StringToNative(imageFilePath);
			ulong result = ovr_Avatar_UpdateMetaData_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Avatar_UpdateMetaData")]
		private static extern ulong ovr_Avatar_UpdateMetaData_Native(IntPtr avatarMetaData, IntPtr imageFilePath);

		public static ulong ovr_CloudStorage_Delete(string bucket, string key)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			ulong result = ovr_CloudStorage_Delete_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Delete")]
		private static extern ulong ovr_CloudStorage_Delete_Native(IntPtr bucket, IntPtr key);

		public static ulong ovr_CloudStorage_Load(string bucket, string key)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			ulong result = ovr_CloudStorage_Load_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Load")]
		private static extern ulong ovr_CloudStorage_Load_Native(IntPtr bucket, IntPtr key);

		public static ulong ovr_CloudStorage_LoadBucketMetadata(string bucket)
		{
			IntPtr intPtr = StringToNative(bucket);
			ulong result = ovr_CloudStorage_LoadBucketMetadata_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadBucketMetadata")]
		private static extern ulong ovr_CloudStorage_LoadBucketMetadata_Native(IntPtr bucket);

		public static ulong ovr_CloudStorage_LoadConflictMetadata(string bucket, string key)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			ulong result = ovr_CloudStorage_LoadConflictMetadata_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadConflictMetadata")]
		private static extern ulong ovr_CloudStorage_LoadConflictMetadata_Native(IntPtr bucket, IntPtr key);

		public static ulong ovr_CloudStorage_LoadHandle(string handle)
		{
			IntPtr intPtr = StringToNative(handle);
			ulong result = ovr_CloudStorage_LoadHandle_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadHandle")]
		private static extern ulong ovr_CloudStorage_LoadHandle_Native(IntPtr handle);

		public static ulong ovr_CloudStorage_LoadMetadata(string bucket, string key)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			ulong result = ovr_CloudStorage_LoadMetadata_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadMetadata")]
		private static extern ulong ovr_CloudStorage_LoadMetadata_Native(IntPtr bucket, IntPtr key);

		public static ulong ovr_CloudStorage_ResolveKeepLocal(string bucket, string key, string remoteHandle)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			IntPtr intPtr3 = StringToNative(remoteHandle);
			ulong result = ovr_CloudStorage_ResolveKeepLocal_Native(intPtr, intPtr2, intPtr3);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			Marshal.FreeCoTaskMem(intPtr3);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_ResolveKeepLocal")]
		private static extern ulong ovr_CloudStorage_ResolveKeepLocal_Native(IntPtr bucket, IntPtr key, IntPtr remoteHandle);

		public static ulong ovr_CloudStorage_ResolveKeepRemote(string bucket, string key, string remoteHandle)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			IntPtr intPtr3 = StringToNative(remoteHandle);
			ulong result = ovr_CloudStorage_ResolveKeepRemote_Native(intPtr, intPtr2, intPtr3);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			Marshal.FreeCoTaskMem(intPtr3);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_ResolveKeepRemote")]
		private static extern ulong ovr_CloudStorage_ResolveKeepRemote_Native(IntPtr bucket, IntPtr key, IntPtr remoteHandle);

		public static ulong ovr_CloudStorage_Save(string bucket, string key, byte[] data, uint dataSize, long counter, string extraData)
		{
			IntPtr intPtr = StringToNative(bucket);
			IntPtr intPtr2 = StringToNative(key);
			IntPtr intPtr3 = StringToNative(extraData);
			ulong result = ovr_CloudStorage_Save_Native(intPtr, intPtr2, data, dataSize, counter, intPtr3);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			Marshal.FreeCoTaskMem(intPtr3);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Save")]
		private static extern ulong ovr_CloudStorage_Save_Native(IntPtr bucket, IntPtr key, byte[] data, uint dataSize, long counter, IntPtr extraData);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Entitlement_GetIsViewerEntitled();

		public static ulong ovr_GraphAPI_Get(string url)
		{
			IntPtr intPtr = StringToNative(url);
			ulong result = ovr_GraphAPI_Get_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GraphAPI_Get")]
		private static extern ulong ovr_GraphAPI_Get_Native(IntPtr url);

		public static ulong ovr_GraphAPI_Post(string url)
		{
			IntPtr intPtr = StringToNative(url);
			ulong result = ovr_GraphAPI_Post_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GraphAPI_Post")]
		private static extern ulong ovr_GraphAPI_Post_Native(IntPtr url);

		public static ulong ovr_HTTP_Get(string url)
		{
			IntPtr intPtr = StringToNative(url);
			ulong result = ovr_HTTP_Get_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_Get")]
		private static extern ulong ovr_HTTP_Get_Native(IntPtr url);

		public static ulong ovr_HTTP_GetToFile(string url, string diskFile)
		{
			IntPtr intPtr = StringToNative(url);
			IntPtr intPtr2 = StringToNative(diskFile);
			ulong result = ovr_HTTP_GetToFile_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_GetToFile")]
		private static extern ulong ovr_HTTP_GetToFile_Native(IntPtr url, IntPtr diskFile);

		public static ulong ovr_HTTP_MultiPartPost(string url, string filepath_param_name, string filepath, string access_token, ovrKeyValuePair[] post_params)
		{
			IntPtr intPtr = StringToNative(url);
			IntPtr intPtr2 = StringToNative(filepath_param_name);
			IntPtr intPtr3 = StringToNative(filepath);
			IntPtr intPtr4 = StringToNative(access_token);
			UIntPtr numItems = (UIntPtr)(ulong)post_params.Length;
			ulong result = ovr_HTTP_MultiPartPost_Native(intPtr, intPtr2, intPtr3, intPtr4, post_params, numItems);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			Marshal.FreeCoTaskMem(intPtr3);
			Marshal.FreeCoTaskMem(intPtr4);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_MultiPartPost")]
		private static extern ulong ovr_HTTP_MultiPartPost_Native(IntPtr url, IntPtr filepath_param_name, IntPtr filepath, IntPtr access_token, ovrKeyValuePair[] post_params, UIntPtr numItems);

		public static ulong ovr_HTTP_Post(string url)
		{
			IntPtr intPtr = StringToNative(url);
			ulong result = ovr_HTTP_Post_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_Post")]
		private static extern ulong ovr_HTTP_Post_Native(IntPtr url);

		public static ulong ovr_IAP_ConsumePurchase(string sku)
		{
			IntPtr intPtr = StringToNative(sku);
			ulong result = ovr_IAP_ConsumePurchase_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_IAP_ConsumePurchase")]
		private static extern ulong ovr_IAP_ConsumePurchase_Native(IntPtr sku);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_IAP_GetProductsBySKU(string[] skus, int count);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_IAP_GetViewerPurchases();

		public static ulong ovr_IAP_LaunchCheckoutFlow(string sku)
		{
			IntPtr intPtr = StringToNative(sku);
			ulong result = ovr_IAP_LaunchCheckoutFlow_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_IAP_LaunchCheckoutFlow")]
		private static extern ulong ovr_IAP_LaunchCheckoutFlow_Native(IntPtr sku);

		public static ulong ovr_Leaderboard_GetEntries(string leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt)
		{
			IntPtr intPtr = StringToNative(leaderboardName);
			ulong result = ovr_Leaderboard_GetEntries_Native(intPtr, limit, filter, startAt);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_GetEntries")]
		private static extern ulong ovr_Leaderboard_GetEntries_Native(IntPtr leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt);

		public static ulong ovr_Leaderboard_GetEntriesAfterRank(string leaderboardName, int limit, ulong afterRank)
		{
			IntPtr intPtr = StringToNative(leaderboardName);
			ulong result = ovr_Leaderboard_GetEntriesAfterRank_Native(intPtr, limit, afterRank);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_GetEntriesAfterRank")]
		private static extern ulong ovr_Leaderboard_GetEntriesAfterRank_Native(IntPtr leaderboardName, int limit, ulong afterRank);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Leaderboard_GetNextEntries(IntPtr handle);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Leaderboard_GetPreviousEntries(IntPtr handle);

		public static ulong ovr_Leaderboard_WriteEntry(string leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate)
		{
			IntPtr intPtr = StringToNative(leaderboardName);
			ulong result = ovr_Leaderboard_WriteEntry_Native(intPtr, score, extraData, extraDataLength, forceUpdate);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_WriteEntry")]
		private static extern ulong ovr_Leaderboard_WriteEntry_Native(IntPtr leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_GetStatus();

		public static ulong ovr_Livestreaming_IsAllowedForApplication(string packageName)
		{
			IntPtr intPtr = StringToNative(packageName);
			ulong result = ovr_Livestreaming_IsAllowedForApplication_Native(intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Livestreaming_IsAllowedForApplication")]
		private static extern ulong ovr_Livestreaming_IsAllowedForApplication_Native(IntPtr packageName);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_PauseStream();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_ResumeStream();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_StartPartyStream();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_StartStream(LivestreamingAudience audience, LivestreamingMicrophoneStatus micStatus);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_StopPartyStream();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_StopStream();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_UpdateCommentsOverlayVisibility(bool isVisible);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Livestreaming_UpdateMicStatus(LivestreamingMicrophoneStatus micStatus);

		public static ulong ovr_Matchmaking_Browse(string pool, IntPtr customQueryData)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_Browse_Native(intPtr, customQueryData);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Browse")]
		private static extern ulong ovr_Matchmaking_Browse_Native(IntPtr pool, IntPtr customQueryData);

		public static ulong ovr_Matchmaking_Browse2(string pool, IntPtr matchmakingOptions)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_Browse2_Native(intPtr, matchmakingOptions);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Browse2")]
		private static extern ulong ovr_Matchmaking_Browse2_Native(IntPtr pool, IntPtr matchmakingOptions);

		public static ulong ovr_Matchmaking_Cancel(string pool, string requestHash)
		{
			IntPtr intPtr = StringToNative(pool);
			IntPtr intPtr2 = StringToNative(requestHash);
			ulong result = ovr_Matchmaking_Cancel_Native(intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Cancel")]
		private static extern ulong ovr_Matchmaking_Cancel_Native(IntPtr pool, IntPtr requestHash);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_Cancel2();

		public static ulong ovr_Matchmaking_CreateAndEnqueueRoom(string pool, uint maxUsers, bool subscribeToUpdates, IntPtr customQueryData)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_CreateAndEnqueueRoom_Native(intPtr, maxUsers, subscribeToUpdates, customQueryData);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateAndEnqueueRoom")]
		private static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom_Native(IntPtr pool, uint maxUsers, bool subscribeToUpdates, IntPtr customQueryData);

		public static ulong ovr_Matchmaking_CreateAndEnqueueRoom2(string pool, IntPtr matchmakingOptions)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_CreateAndEnqueueRoom2_Native(intPtr, matchmakingOptions);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateAndEnqueueRoom2")]
		private static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom2_Native(IntPtr pool, IntPtr matchmakingOptions);

		public static ulong ovr_Matchmaking_CreateRoom(string pool, uint maxUsers, bool subscribeToUpdates)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_CreateRoom_Native(intPtr, maxUsers, subscribeToUpdates);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateRoom")]
		private static extern ulong ovr_Matchmaking_CreateRoom_Native(IntPtr pool, uint maxUsers, bool subscribeToUpdates);

		public static ulong ovr_Matchmaking_CreateRoom2(string pool, IntPtr matchmakingOptions)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_CreateRoom2_Native(intPtr, matchmakingOptions);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateRoom2")]
		private static extern ulong ovr_Matchmaking_CreateRoom2_Native(IntPtr pool, IntPtr matchmakingOptions);

		public static ulong ovr_Matchmaking_Enqueue(string pool, IntPtr customQueryData)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_Enqueue_Native(intPtr, customQueryData);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Enqueue")]
		private static extern ulong ovr_Matchmaking_Enqueue_Native(IntPtr pool, IntPtr customQueryData);

		public static ulong ovr_Matchmaking_Enqueue2(string pool, IntPtr matchmakingOptions)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_Enqueue2_Native(intPtr, matchmakingOptions);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Enqueue2")]
		private static extern ulong ovr_Matchmaking_Enqueue2_Native(IntPtr pool, IntPtr matchmakingOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_EnqueueRoom(ulong roomID, IntPtr customQueryData);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_EnqueueRoom2(ulong roomID, IntPtr matchmakingOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_GetAdminSnapshot();

		public static ulong ovr_Matchmaking_GetStats(string pool, uint maxLevel, MatchmakingStatApproach approach)
		{
			IntPtr intPtr = StringToNative(pool);
			ulong result = ovr_Matchmaking_GetStats_Native(intPtr, maxLevel, approach);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_GetStats")]
		private static extern ulong ovr_Matchmaking_GetStats_Native(IntPtr pool, uint maxLevel, MatchmakingStatApproach approach);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_JoinRoom(ulong roomID, bool subscribeToUpdates);

		public static ulong ovr_Matchmaking_ReportResultInsecure(ulong roomID, ovrKeyValuePair[] data)
		{
			UIntPtr numItems = (UIntPtr)(ulong)data.Length;
			return ovr_Matchmaking_ReportResultInsecure_Native(roomID, data, numItems);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_ReportResultInsecure")]
		private static extern ulong ovr_Matchmaking_ReportResultInsecure_Native(ulong roomID, ovrKeyValuePair[] data, UIntPtr numItems);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Matchmaking_StartMatch(ulong roomID);

		public static ulong ovr_Media_ShareToFacebook(string postTextSuggestion, string filePath, MediaContentType contentType)
		{
			IntPtr intPtr = StringToNative(postTextSuggestion);
			IntPtr intPtr2 = StringToNative(filePath);
			ulong result = ovr_Media_ShareToFacebook_Native(intPtr, intPtr2, contentType);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Media_ShareToFacebook")]
		private static extern ulong ovr_Media_ShareToFacebook_Native(IntPtr postTextSuggestion, IntPtr filePath, MediaContentType contentType);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Notification_GetRoomInvites();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Notification_MarkAsRead(ulong notificationID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_GatherInApplication(ulong partyID, ulong appID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_Get(ulong partyID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_GetCurrent();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_GetCurrentForUser(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_Invite(ulong partyID, ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_Join(ulong partyID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_Leave(ulong partyID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_CreateAndJoinPrivate(RoomJoinPolicy joinPolicy, uint maxUsers, bool subscribeToUpdates);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_CreateAndJoinPrivate2(RoomJoinPolicy joinPolicy, uint maxUsers, IntPtr roomOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_Get(ulong roomID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetCurrent();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetCurrentForUser(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetInvitableUsers();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetInvitableUsers2(IntPtr roomOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetModeratedRooms();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetSocialRooms(ulong appID);

		public static ulong ovr_Room_InviteUser(ulong roomID, string inviteToken)
		{
			IntPtr intPtr = StringToNative(inviteToken);
			ulong result = ovr_Room_InviteUser_Native(roomID, intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_InviteUser")]
		private static extern ulong ovr_Room_InviteUser_Native(ulong roomID, IntPtr inviteToken);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_Join(ulong roomID, bool subscribeToUpdates);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_Join2(ulong roomID, IntPtr roomOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_KickUser(ulong roomID, ulong userID, int kickDurationSeconds);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_LaunchInvitableUserFlow(ulong roomID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_Leave(ulong roomID);

		public static ulong ovr_Room_SetDescription(ulong roomID, string description)
		{
			IntPtr intPtr = StringToNative(description);
			ulong result = ovr_Room_SetDescription_Native(roomID, intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_SetDescription")]
		private static extern ulong ovr_Room_SetDescription_Native(ulong roomID, IntPtr description);

		public static ulong ovr_Room_UpdateDataStore(ulong roomID, ovrKeyValuePair[] data)
		{
			UIntPtr numItems = (UIntPtr)(ulong)data.Length;
			return ovr_Room_UpdateDataStore_Native(roomID, data, numItems);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_UpdateDataStore")]
		private static extern ulong ovr_Room_UpdateDataStore_Native(ulong roomID, ovrKeyValuePair[] data, UIntPtr numItems);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_UpdateMembershipLockStatus(ulong roomID, RoomMembershipLockStatus membershipLockStatus);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_UpdateOwner(ulong roomID, ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_UpdatePrivateRoomJoinPolicy(ulong roomID, RoomJoinPolicy newJoinPolicy);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_SystemPermissions_GetStatus(PermissionType permType);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_SystemPermissions_LaunchDeeplink(PermissionType permType);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_Get(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetAccessToken();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetLoggedInUser();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetLoggedInUserFriends();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetLoggedInUserFriendsAndRooms();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetLoggedInUserRecentlyMetUsersAndRooms(IntPtr userOptions);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetOrgScopedID(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetSdkAccounts();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetUserProof();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_LaunchBlockFlow(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_LaunchProfile(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_LaunchReportFlow(ulong userID);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_NewEntitledTestUser();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_NewTestUser();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_NewTestUserFriends();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Voip_SetSystemVoipSuppressed(bool suppressed);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_AchievementDefinition_GetBitfieldLength(IntPtr obj);

		public static string ovr_AchievementDefinition_GetName(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementDefinition_GetName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementDefinition_GetName")]
		private static extern IntPtr ovr_AchievementDefinition_GetName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AchievementDefinition_GetTarget(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern AchievementType ovr_AchievementDefinition_GetType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_AchievementDefinitionArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_AchievementDefinitionArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementDefinitionArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementDefinitionArray_GetNextUrl")]
		private static extern IntPtr ovr_AchievementDefinitionArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_AchievementDefinitionArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AchievementDefinitionArray_HasNextPage(IntPtr obj);

		public static string ovr_AchievementProgress_GetBitfield(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementProgress_GetBitfield_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetBitfield")]
		private static extern IntPtr ovr_AchievementProgress_GetBitfield_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AchievementProgress_GetCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AchievementProgress_GetIsUnlocked(IntPtr obj);

		public static string ovr_AchievementProgress_GetName(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementProgress_GetName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetName")]
		private static extern IntPtr ovr_AchievementProgress_GetName_Native(IntPtr obj);

		public static DateTime ovr_AchievementProgress_GetUnlockTime(IntPtr obj)
		{
			return DateTimeFromNative(ovr_AchievementProgress_GetUnlockTime_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetUnlockTime")]
		private static extern ulong ovr_AchievementProgress_GetUnlockTime_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_AchievementProgressArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_AchievementProgressArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementProgressArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgressArray_GetNextUrl")]
		private static extern IntPtr ovr_AchievementProgressArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_AchievementProgressArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AchievementProgressArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AchievementUpdate_GetJustUnlocked(IntPtr obj);

		public static string ovr_AchievementUpdate_GetName(IntPtr obj)
		{
			return StringFromNative(ovr_AchievementUpdate_GetName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementUpdate_GetName")]
		private static extern IntPtr ovr_AchievementUpdate_GetName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Application_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_ApplicationVersion_GetCurrentCode(IntPtr obj);

		public static string ovr_ApplicationVersion_GetCurrentName(IntPtr obj)
		{
			return StringFromNative(ovr_ApplicationVersion_GetCurrentName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationVersion_GetCurrentName")]
		private static extern IntPtr ovr_ApplicationVersion_GetCurrentName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_ApplicationVersion_GetLatestCode(IntPtr obj);

		public static string ovr_ApplicationVersion_GetLatestName(IntPtr obj)
		{
			return StringFromNative(ovr_ApplicationVersion_GetLatestName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationVersion_GetLatestName")]
		private static extern IntPtr ovr_ApplicationVersion_GetLatestName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFileDeleteResult_GetAssetFileId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AssetFileDeleteResult_GetSuccess(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFileDownloadCancelResult_GetAssetFileId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AssetFileDownloadCancelResult_GetSuccess(IntPtr obj);

		public static string ovr_AssetFileDownloadResult_GetFilepath(IntPtr obj)
		{
			return StringFromNative(ovr_AssetFileDownloadResult_GetFilepath_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFileDownloadResult_GetFilepath")]
		private static extern IntPtr ovr_AssetFileDownloadResult_GetFilepath_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_AssetFileDownloadUpdate_GetAssetFileId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_AssetFileDownloadUpdate_GetBytesTotal(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_AssetFileDownloadUpdate_GetBytesTransferred(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_AssetFileDownloadUpdate_GetCompleted(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_CloudStorageConflictMetadata_GetLocal(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_CloudStorageConflictMetadata_GetRemote(IntPtr obj);

		public static string ovr_CloudStorageData_GetBucket(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageData_GetBucket_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetBucket")]
		private static extern IntPtr ovr_CloudStorageData_GetBucket_Native(IntPtr obj);

		public static byte[] ovr_CloudStorageData_GetData(IntPtr obj)
		{
			return FiledataFromNative(ovr_CloudStorageData_GetDataSize(obj), ovr_CloudStorageData_GetData_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetData")]
		private static extern IntPtr ovr_CloudStorageData_GetData_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_CloudStorageData_GetDataSize(IntPtr obj);

		public static string ovr_CloudStorageData_GetKey(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageData_GetKey_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetKey")]
		private static extern IntPtr ovr_CloudStorageData_GetKey_Native(IntPtr obj);

		public static string ovr_CloudStorageMetadata_GetBucket(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageMetadata_GetBucket_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetBucket")]
		private static extern IntPtr ovr_CloudStorageMetadata_GetBucket_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern long ovr_CloudStorageMetadata_GetCounter(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_CloudStorageMetadata_GetDataSize(IntPtr obj);

		public static string ovr_CloudStorageMetadata_GetExtraData(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageMetadata_GetExtraData_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetExtraData")]
		private static extern IntPtr ovr_CloudStorageMetadata_GetExtraData_Native(IntPtr obj);

		public static string ovr_CloudStorageMetadata_GetKey(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageMetadata_GetKey_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetKey")]
		private static extern IntPtr ovr_CloudStorageMetadata_GetKey_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_CloudStorageMetadata_GetSaveTime(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern CloudStorageDataStatus ovr_CloudStorageMetadata_GetStatus(IntPtr obj);

		public static string ovr_CloudStorageMetadata_GetVersionHandle(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageMetadata_GetVersionHandle_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetVersionHandle")]
		private static extern IntPtr ovr_CloudStorageMetadata_GetVersionHandle_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_CloudStorageMetadataArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_CloudStorageMetadataArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageMetadataArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadataArray_GetNextUrl")]
		private static extern IntPtr ovr_CloudStorageMetadataArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_CloudStorageMetadataArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_CloudStorageMetadataArray_HasNextPage(IntPtr obj);

		public static string ovr_CloudStorageUpdateResponse_GetBucket(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageUpdateResponse_GetBucket_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetBucket")]
		private static extern IntPtr ovr_CloudStorageUpdateResponse_GetBucket_Native(IntPtr obj);

		public static string ovr_CloudStorageUpdateResponse_GetKey(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageUpdateResponse_GetKey_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetKey")]
		private static extern IntPtr ovr_CloudStorageUpdateResponse_GetKey_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern CloudStorageUpdateStatus ovr_CloudStorageUpdateResponse_GetStatus(IntPtr obj);

		public static string ovr_CloudStorageUpdateResponse_GetVersionHandle(IntPtr obj)
		{
			return StringFromNative(ovr_CloudStorageUpdateResponse_GetVersionHandle_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetVersionHandle")]
		private static extern IntPtr ovr_CloudStorageUpdateResponse_GetVersionHandle_Native(IntPtr obj);

		public static uint ovr_DataStore_Contains(IntPtr obj, string key)
		{
			IntPtr intPtr = StringToNative(key);
			uint result = ovr_DataStore_Contains_Native(obj, intPtr);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_Contains")]
		private static extern uint ovr_DataStore_Contains_Native(IntPtr obj, IntPtr key);

		public static string ovr_DataStore_GetKey(IntPtr obj, int index)
		{
			return StringFromNative(ovr_DataStore_GetKey_Native(obj, index));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_GetKey")]
		private static extern IntPtr ovr_DataStore_GetKey_Native(IntPtr obj, int index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_DataStore_GetNumKeys(IntPtr obj);

		public static string ovr_DataStore_GetValue(IntPtr obj, string key)
		{
			IntPtr intPtr = StringToNative(key);
			string result = StringFromNative(ovr_DataStore_GetValue_Native(obj, intPtr));
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_GetValue")]
		private static extern IntPtr ovr_DataStore_GetValue_Native(IntPtr obj, IntPtr key);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_Error_GetCode(IntPtr obj);

		public static string ovr_Error_GetDisplayableMessage(IntPtr obj)
		{
			return StringFromNative(ovr_Error_GetDisplayableMessage_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Error_GetDisplayableMessage")]
		private static extern IntPtr ovr_Error_GetDisplayableMessage_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_Error_GetHttpCode(IntPtr obj);

		public static string ovr_Error_GetMessage(IntPtr obj)
		{
			return StringFromNative(ovr_Error_GetMessage_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Error_GetMessage")]
		private static extern IntPtr ovr_Error_GetMessage_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_HttpTransferUpdate_GetBytes(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_HttpTransferUpdate_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_HttpTransferUpdate_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_HttpTransferUpdate_IsCompleted(IntPtr obj);

		public static string ovr_InstalledApplication_GetApplicationId(IntPtr obj)
		{
			return StringFromNative(ovr_InstalledApplication_GetApplicationId_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetApplicationId")]
		private static extern IntPtr ovr_InstalledApplication_GetApplicationId_Native(IntPtr obj);

		public static string ovr_InstalledApplication_GetPackageName(IntPtr obj)
		{
			return StringFromNative(ovr_InstalledApplication_GetPackageName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetPackageName")]
		private static extern IntPtr ovr_InstalledApplication_GetPackageName_Native(IntPtr obj);

		public static string ovr_InstalledApplication_GetStatus(IntPtr obj)
		{
			return StringFromNative(ovr_InstalledApplication_GetStatus_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetStatus")]
		private static extern IntPtr ovr_InstalledApplication_GetStatus_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_InstalledApplication_GetVersionCode(IntPtr obj);

		public static string ovr_InstalledApplication_GetVersionName(IntPtr obj)
		{
			return StringFromNative(ovr_InstalledApplication_GetVersionName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetVersionName")]
		private static extern IntPtr ovr_InstalledApplication_GetVersionName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_InstalledApplicationArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_InstalledApplicationArray_GetSize(IntPtr obj);

		public static string ovr_LaunchDetails_GetDeeplinkMessage(IntPtr obj)
		{
			return StringFromNative(ovr_LaunchDetails_GetDeeplinkMessage_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LaunchDetails_GetDeeplinkMessage")]
		private static extern IntPtr ovr_LaunchDetails_GetDeeplinkMessage_Native(IntPtr obj);

		public static string ovr_LaunchDetails_GetLaunchSource(IntPtr obj)
		{
			return StringFromNative(ovr_LaunchDetails_GetLaunchSource_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LaunchDetails_GetLaunchSource")]
		private static extern IntPtr ovr_LaunchDetails_GetLaunchSource_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern LaunchType ovr_LaunchDetails_GetLaunchType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_LaunchDetails_GetRoomID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_LaunchDetails_GetUsers(IntPtr obj);

		public static byte[] ovr_LeaderboardEntry_GetExtraData(IntPtr obj)
		{
			return BlobFromNative(ovr_LeaderboardEntry_GetExtraDataLength(obj), ovr_LeaderboardEntry_GetExtraData_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntry_GetExtraData")]
		private static extern IntPtr ovr_LeaderboardEntry_GetExtraData_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_LeaderboardEntry_GetExtraDataLength(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_LeaderboardEntry_GetRank(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern long ovr_LeaderboardEntry_GetScore(IntPtr obj);

		public static DateTime ovr_LeaderboardEntry_GetTimestamp(IntPtr obj)
		{
			return DateTimeFromNative(ovr_LeaderboardEntry_GetTimestamp_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntry_GetTimestamp")]
		private static extern ulong ovr_LeaderboardEntry_GetTimestamp_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_LeaderboardEntry_GetUser(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_LeaderboardEntryArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_LeaderboardEntryArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_LeaderboardEntryArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntryArray_GetNextUrl")]
		private static extern IntPtr ovr_LeaderboardEntryArray_GetNextUrl_Native(IntPtr obj);

		public static string ovr_LeaderboardEntryArray_GetPreviousUrl(IntPtr obj)
		{
			return StringFromNative(ovr_LeaderboardEntryArray_GetPreviousUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntryArray_GetPreviousUrl")]
		private static extern IntPtr ovr_LeaderboardEntryArray_GetPreviousUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_LeaderboardEntryArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_LeaderboardEntryArray_GetTotalCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LeaderboardEntryArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LeaderboardEntryArray_HasPreviousPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LeaderboardUpdateStatus_GetDidUpdate(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LivestreamingApplicationStatus_GetStreamingEnabled(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern LivestreamingStartStatus ovr_LivestreamingStartResult_GetStreamingResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LivestreamingStatus_GetCommentsVisible(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LivestreamingStatus_GetIsPaused(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LivestreamingStatus_GetLivestreamingEnabled(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_LivestreamingStatus_GetLivestreamingType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_LivestreamingStatus_GetMicEnabled(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_LivestreamingVideoStats_GetCommentCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovr_LivestreamingVideoStats_GetReactionCount(IntPtr obj);

		public static string ovr_LivestreamingVideoStats_GetTotalViews(IntPtr obj)
		{
			return StringFromNative(ovr_LivestreamingVideoStats_GetTotalViews_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LivestreamingVideoStats_GetTotalViews")]
		private static extern IntPtr ovr_LivestreamingVideoStats_GetTotalViews_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingAdminSnapshot_GetCandidates(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern double ovr_MatchmakingAdminSnapshot_GetMyCurrentThreshold(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_MatchmakingAdminSnapshotCandidate_GetCanMatch(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetMyTotalScore(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetTheirCurrentThreshold(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetTheirTotalScore(IntPtr obj);

		public static string ovr_MatchmakingAdminSnapshotCandidate_GetTraceId(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingAdminSnapshotCandidate_GetTraceId_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingAdminSnapshotCandidate_GetTraceId")]
		private static extern IntPtr ovr_MatchmakingAdminSnapshotCandidate_GetTraceId_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingAdminSnapshotCandidateArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_MatchmakingAdminSnapshotCandidateArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingBrowseResult_GetEnqueueResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingBrowseResult_GetRooms(IntPtr obj);

		public static string ovr_MatchmakingCandidate_GetEntryHash(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingCandidate_GetEntryHash_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingCandidate_GetEntryHash")]
		private static extern IntPtr ovr_MatchmakingCandidate_GetEntryHash_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_MatchmakingCandidate_GetUserId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingCandidateArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_MatchmakingCandidateArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingCandidateArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingCandidateArray_GetNextUrl")]
		private static extern IntPtr ovr_MatchmakingCandidateArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_MatchmakingCandidateArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_MatchmakingCandidateArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueueResult_GetAdminSnapshot(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingEnqueueResult_GetAverageWait(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingEnqueueResult_GetMatchesInLastHourCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingEnqueueResult_GetMaxExpectedWait(IntPtr obj);

		public static string ovr_MatchmakingEnqueueResult_GetPool(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingEnqueueResult_GetPool_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingEnqueueResult_GetPool")]
		private static extern IntPtr ovr_MatchmakingEnqueueResult_GetPool_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingEnqueueResult_GetRecentMatchPercentage(IntPtr obj);

		public static string ovr_MatchmakingEnqueueResult_GetRequestHash(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingEnqueueResult_GetRequestHash_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingEnqueueResult_GetRequestHash")]
		private static extern IntPtr ovr_MatchmakingEnqueueResult_GetRequestHash_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueueResultAndRoom_GetMatchmakingEnqueueResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueueResultAndRoom_GetRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_MatchmakingEnqueuedUser_GetAdditionalUserID(IntPtr obj, uint index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingEnqueuedUser_GetAdditionalUserIDsSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueuedUser_GetCustomData(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueuedUser_GetUser(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingEnqueuedUserArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_MatchmakingEnqueuedUserArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_MatchmakingNotification_GetAddedByUserId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingNotification_GetRoom(IntPtr obj);

		public static string ovr_MatchmakingNotification_GetTraceId(IntPtr obj)
		{
			return StringFromNative(ovr_MatchmakingNotification_GetTraceId_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingNotification_GetTraceId")]
		private static extern IntPtr ovr_MatchmakingNotification_GetTraceId_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingRoom_GetPingTime(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingRoom_GetRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_MatchmakingRoom_HasPingTime(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingRoomArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_MatchmakingRoomArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingStats_GetDrawCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingStats_GetLossCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingStats_GetSkillLevel(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_MatchmakingStats_GetWinCount(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAchievementDefinitionArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAchievementProgressArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAchievementUpdate(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetApplicationVersion(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAssetFileDeleteResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAssetFileDownloadCancelResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAssetFileDownloadResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetAssetFileDownloadUpdate(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetCloudStorageConflictMetadata(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetCloudStorageData(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetCloudStorageMetadata(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetCloudStorageMetadataArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetCloudStorageUpdateResponse(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetError(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetHttpTransferUpdate(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetInstalledApplicationArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLeaderboardEntryArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLeaderboardUpdateStatus(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLivestreamingApplicationStatus(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLivestreamingStartResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLivestreamingStatus(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetLivestreamingVideoStats(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingAdminSnapshot(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingBrowseResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingEnqueueResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingEnqueueResultAndRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingRoomArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetMatchmakingStats(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetNativeMessage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetNetworkingPeer(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetOrgScopedID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetParty(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPartyID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPidArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPingResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPlatformInitialize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetProductArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPurchase(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetPurchaseArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Message_GetRequestID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetRoomArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetRoomInviteNotification(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetRoomInviteNotificationArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetSdkAccountArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetShareMediaResult(IntPtr obj);

		public static string ovr_Message_GetString(IntPtr obj)
		{
			return StringFromNative(ovr_Message_GetString_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Message_GetString")]
		private static extern IntPtr ovr_Message_GetString_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetSystemPermission(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetSystemVoipState(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern Message.MessageType ovr_Message_GetType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetUser(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetUserAndRoomArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetUserArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetUserProof(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Message_GetUserReportID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Message_IsError(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Microphone_GetNumSamplesAvailable(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Microphone_GetOutputBufferMaxSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Microphone_GetPCM(IntPtr obj, short[] outputBuffer, UIntPtr outputBufferNumElements);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Microphone_GetPCMFloat(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferNumElements);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Microphone_ReadData(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Microphone_SetAcceptableRecordingDelayHint(IntPtr obj, UIntPtr delayMs);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Microphone_Start(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Microphone_Stop(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_NetworkingPeer_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern PeerConnectionState ovr_NetworkingPeer_GetState(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_OrgScopedID_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Packet_Free(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Packet_GetBytes(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern SendPolicy ovr_Packet_GetSendPolicy(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Packet_GetSenderID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_Packet_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Party_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Party_GetInvitedUsers(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Party_GetLeader(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Party_GetRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Party_GetUsers(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_PartyID_GetID(IntPtr obj);

		public static string ovr_Pid_GetId(IntPtr obj)
		{
			return StringFromNative(ovr_Pid_GetId_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Pid_GetId")]
		private static extern IntPtr ovr_Pid_GetId_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_PidArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_PidArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_PingResult_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_PingResult_GetPingTimeUsec(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_PingResult_IsTimeout(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern PlatformInitializeResult ovr_PlatformInitialize_GetResult(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_Price_GetAmountInHundredths(IntPtr obj);

		public static string ovr_Price_GetCurrency(IntPtr obj)
		{
			return StringFromNative(ovr_Price_GetCurrency_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Price_GetCurrency")]
		private static extern IntPtr ovr_Price_GetCurrency_Native(IntPtr obj);

		public static string ovr_Price_GetFormatted(IntPtr obj)
		{
			return StringFromNative(ovr_Price_GetFormatted_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Price_GetFormatted")]
		private static extern IntPtr ovr_Price_GetFormatted_Native(IntPtr obj);

		public static string ovr_Product_GetDescription(IntPtr obj)
		{
			return StringFromNative(ovr_Product_GetDescription_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetDescription")]
		private static extern IntPtr ovr_Product_GetDescription_Native(IntPtr obj);

		public static string ovr_Product_GetFormattedPrice(IntPtr obj)
		{
			return StringFromNative(ovr_Product_GetFormattedPrice_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetFormattedPrice")]
		private static extern IntPtr ovr_Product_GetFormattedPrice_Native(IntPtr obj);

		public static string ovr_Product_GetName(IntPtr obj)
		{
			return StringFromNative(ovr_Product_GetName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetName")]
		private static extern IntPtr ovr_Product_GetName_Native(IntPtr obj);

		public static string ovr_Product_GetSKU(IntPtr obj)
		{
			return StringFromNative(ovr_Product_GetSKU_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetSKU")]
		private static extern IntPtr ovr_Product_GetSKU_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_ProductArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_ProductArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_ProductArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ProductArray_GetNextUrl")]
		private static extern IntPtr ovr_ProductArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_ProductArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_ProductArray_HasNextPage(IntPtr obj);

		public static DateTime ovr_Purchase_GetExpirationTime(IntPtr obj)
		{
			return DateTimeFromNative(ovr_Purchase_GetExpirationTime_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetExpirationTime")]
		private static extern ulong ovr_Purchase_GetExpirationTime_Native(IntPtr obj);

		public static DateTime ovr_Purchase_GetGrantTime(IntPtr obj)
		{
			return DateTimeFromNative(ovr_Purchase_GetGrantTime_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetGrantTime")]
		private static extern ulong ovr_Purchase_GetGrantTime_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Purchase_GetPurchaseID(IntPtr obj);

		public static string ovr_Purchase_GetSKU(IntPtr obj)
		{
			return StringFromNative(ovr_Purchase_GetSKU_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetSKU")]
		private static extern IntPtr ovr_Purchase_GetSKU_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_PurchaseArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_PurchaseArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_PurchaseArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_PurchaseArray_GetNextUrl")]
		private static extern IntPtr ovr_PurchaseArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_PurchaseArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_PurchaseArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetApplicationID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Room_GetDataStore(IntPtr obj);

		public static string ovr_Room_GetDescription(IntPtr obj)
		{
			return StringFromNative(ovr_Room_GetDescription_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_GetDescription")]
		private static extern IntPtr ovr_Room_GetDescription_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_Room_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Room_GetInvitedUsers(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_Room_GetIsMembershipLocked(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern RoomJoinPolicy ovr_Room_GetJoinPolicy(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern RoomJoinability ovr_Room_GetJoinability(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Room_GetMatchedUsers(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_Room_GetMaxUsers(IntPtr obj);

		public static string ovr_Room_GetName(IntPtr obj)
		{
			return StringFromNative(ovr_Room_GetName_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_GetName")]
		private static extern IntPtr ovr_Room_GetName_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Room_GetOwner(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern RoomType ovr_Room_GetType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_Room_GetUsers(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovr_Room_GetVersion(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_RoomArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_RoomArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_RoomArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomArray_GetNextUrl")]
		private static extern IntPtr ovr_RoomArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_RoomArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_RoomArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_RoomInviteNotification_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_RoomInviteNotification_GetRoomID(IntPtr obj);

		public static DateTime ovr_RoomInviteNotification_GetSentTime(IntPtr obj)
		{
			return DateTimeFromNative(ovr_RoomInviteNotification_GetSentTime_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomInviteNotification_GetSentTime")]
		private static extern ulong ovr_RoomInviteNotification_GetSentTime_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_RoomInviteNotificationArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_RoomInviteNotificationArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_RoomInviteNotificationArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomInviteNotificationArray_GetNextUrl")]
		private static extern IntPtr ovr_RoomInviteNotificationArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_RoomInviteNotificationArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_RoomInviteNotificationArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern SdkAccountType ovr_SdkAccount_GetAccountType(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_SdkAccount_GetUserId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_SdkAccountArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_SdkAccountArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ShareMediaStatus ovr_ShareMediaResult_GetStatus(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_SystemPermission_GetHasPermission(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern PermissionGrantStatus ovr_SystemPermission_GetPermissionGrantStatus(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern VoipMuteState ovr_SystemVoipState_GetMicrophoneMuted(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemVoipStatus ovr_SystemVoipState_GetStatus(IntPtr obj);

		public static string ovr_TestUser_GetAccessToken(IntPtr obj)
		{
			return StringFromNative(ovr_TestUser_GetAccessToken_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetAccessToken")]
		private static extern IntPtr ovr_TestUser_GetAccessToken_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_TestUser_GetAppAccessArray(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_TestUser_GetFbAppAccessArray(IntPtr obj);

		public static string ovr_TestUser_GetFriendAccessToken(IntPtr obj)
		{
			return StringFromNative(ovr_TestUser_GetFriendAccessToken_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetFriendAccessToken")]
		private static extern IntPtr ovr_TestUser_GetFriendAccessToken_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_TestUser_GetFriendAppAccessArray(IntPtr obj);

		public static string ovr_TestUser_GetUserAlias(IntPtr obj)
		{
			return StringFromNative(ovr_TestUser_GetUserAlias_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetUserAlias")]
		private static extern IntPtr ovr_TestUser_GetUserAlias_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_TestUser_GetUserId(IntPtr obj);

		public static string ovr_TestUserAppAccess_GetAccessToken(IntPtr obj)
		{
			return StringFromNative(ovr_TestUserAppAccess_GetAccessToken_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUserAppAccess_GetAccessToken")]
		private static extern IntPtr ovr_TestUserAppAccess_GetAccessToken_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_TestUserAppAccess_GetAppId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_TestUserAppAccess_GetUserId(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_TestUserAppAccessArray_GetElement(IntPtr obj, UIntPtr index);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_TestUserAppAccessArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_User_GetID(IntPtr obj);

		public static string ovr_User_GetImageUrl(IntPtr obj)
		{
			return StringFromNative(ovr_User_GetImageUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetImageUrl")]
		private static extern IntPtr ovr_User_GetImageUrl_Native(IntPtr obj);

		public static string ovr_User_GetInviteToken(IntPtr obj)
		{
			return StringFromNative(ovr_User_GetInviteToken_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetInviteToken")]
		private static extern IntPtr ovr_User_GetInviteToken_Native(IntPtr obj);

		public static string ovr_User_GetOculusID(IntPtr obj)
		{
			return StringFromNative(ovr_User_GetOculusID_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetOculusID")]
		private static extern IntPtr ovr_User_GetOculusID_Native(IntPtr obj);

		public static string ovr_User_GetPresence(IntPtr obj)
		{
			return StringFromNative(ovr_User_GetPresence_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetPresence")]
		private static extern IntPtr ovr_User_GetPresence_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UserPresenceStatus ovr_User_GetPresenceStatus(IntPtr obj);

		public static string ovr_User_GetSmallImageUrl(IntPtr obj)
		{
			return StringFromNative(ovr_User_GetSmallImageUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetSmallImageUrl")]
		private static extern IntPtr ovr_User_GetSmallImageUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_UserAndRoom_GetRoom(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_UserAndRoom_GetUser(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_UserAndRoomArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_UserAndRoomArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_UserAndRoomArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserAndRoomArray_GetNextUrl")]
		private static extern IntPtr ovr_UserAndRoomArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_UserAndRoomArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_UserAndRoomArray_HasNextPage(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_UserArray_GetElement(IntPtr obj, UIntPtr index);

		public static string ovr_UserArray_GetNextUrl(IntPtr obj)
		{
			return StringFromNative(ovr_UserArray_GetNextUrl_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserArray_GetNextUrl")]
		private static extern IntPtr ovr_UserArray_GetNextUrl_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_UserArray_GetSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovr_UserArray_HasNextPage(IntPtr obj);

		public static string ovr_UserProof_GetNonce(IntPtr obj)
		{
			return StringFromNative(ovr_UserProof_GetNonce_Native(obj));
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserProof_GetNonce")]
		private static extern IntPtr ovr_UserProof_GetNonce_Native(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovr_UserReportID_GetID(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_VoipDecoder_Decode(IntPtr obj, byte[] compressedData, UIntPtr compressedSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_VoipDecoder_GetDecodedPCM(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_VoipEncoder_AddPCM(IntPtr obj, float[] inputData, uint inputSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_VoipEncoder_GetCompressedData(IntPtr obj, byte[] outputBuffer, UIntPtr intputSize);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ovr_VoipEncoder_GetCompressedDataSize(IntPtr obj);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_ApplicationOptions_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_ApplicationOptions_Destroy(IntPtr handle);

		public static void ovr_ApplicationOptions_SetDeeplinkMessage(IntPtr handle, string value)
		{
			IntPtr intPtr = StringToNative(value);
			ovr_ApplicationOptions_SetDeeplinkMessage_Native(handle, intPtr);
			Marshal.FreeCoTaskMem(intPtr);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationOptions_SetDeeplinkMessage")]
		private static extern void ovr_ApplicationOptions_SetDeeplinkMessage_Native(IntPtr handle, IntPtr value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_MatchmakingOptions_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_Destroy(IntPtr handle);

		public static void ovr_MatchmakingOptions_SetCreateRoomDataStoreString(IntPtr handle, string key, string value)
		{
			IntPtr intPtr = StringToNative(key);
			IntPtr intPtr2 = StringToNative(value);
			ovr_MatchmakingOptions_SetCreateRoomDataStoreString_Native(handle, intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetCreateRoomDataStoreString")]
		private static extern void ovr_MatchmakingOptions_SetCreateRoomDataStoreString_Native(IntPtr handle, IntPtr key, IntPtr value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_ClearCreateRoomDataStore(IntPtr handle);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_SetCreateRoomJoinPolicy(IntPtr handle, RoomJoinPolicy value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_SetCreateRoomMaxUsers(IntPtr handle, uint value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_AddEnqueueAdditionalUser(IntPtr handle, ulong value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_ClearEnqueueAdditionalUsers(IntPtr handle);

		public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsInt(IntPtr handle, string key, int value)
		{
			IntPtr intPtr = StringToNative(key);
			ovr_MatchmakingOptions_SetEnqueueDataSettingsInt_Native(handle, intPtr, value);
			Marshal.FreeCoTaskMem(intPtr);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsInt")]
		private static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsInt_Native(IntPtr handle, IntPtr key, int value);

		public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble(IntPtr handle, string key, double value)
		{
			IntPtr intPtr = StringToNative(key);
			ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble_Native(handle, intPtr, value);
			Marshal.FreeCoTaskMem(intPtr);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble")]
		private static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble_Native(IntPtr handle, IntPtr key, double value);

		public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsString(IntPtr handle, string key, string value)
		{
			IntPtr intPtr = StringToNative(key);
			IntPtr intPtr2 = StringToNative(value);
			ovr_MatchmakingOptions_SetEnqueueDataSettingsString_Native(handle, intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsString")]
		private static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsString_Native(IntPtr handle, IntPtr key, IntPtr value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_ClearEnqueueDataSettings(IntPtr handle);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_MatchmakingOptions_SetEnqueueIsDebug(IntPtr handle, bool value);

		public static void ovr_MatchmakingOptions_SetEnqueueQueryKey(IntPtr handle, string value)
		{
			IntPtr intPtr = StringToNative(value);
			ovr_MatchmakingOptions_SetEnqueueQueryKey_Native(handle, intPtr);
			Marshal.FreeCoTaskMem(intPtr);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueQueryKey")]
		private static extern void ovr_MatchmakingOptions_SetEnqueueQueryKey_Native(IntPtr handle, IntPtr value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_RoomOptions_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_Destroy(IntPtr handle);

		public static void ovr_RoomOptions_SetDataStoreString(IntPtr handle, string key, string value)
		{
			IntPtr intPtr = StringToNative(key);
			IntPtr intPtr2 = StringToNative(value);
			ovr_RoomOptions_SetDataStoreString_Native(handle, intPtr, intPtr2);
			Marshal.FreeCoTaskMem(intPtr);
			Marshal.FreeCoTaskMem(intPtr2);
		}

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomOptions_SetDataStoreString")]
		private static extern void ovr_RoomOptions_SetDataStoreString_Native(IntPtr handle, IntPtr key, IntPtr value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_ClearDataStore(IntPtr handle);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetExcludeRecentlyMet(IntPtr handle, bool value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetMaxUserResults(IntPtr handle, uint value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetOrdering(IntPtr handle, UserOrdering value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetRecentlyMetTimeWindow(IntPtr handle, TimeWindow value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetRoomId(IntPtr handle, ulong value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_RoomOptions_SetTurnOffUpdates(IntPtr handle, bool value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovr_UserOptions_Create();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_UserOptions_Destroy(IntPtr handle);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_UserOptions_SetMaxUsers(IntPtr handle, uint value);

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_UserOptions_SetTimeWindow(IntPtr handle, TimeWindow value);
	}
}
