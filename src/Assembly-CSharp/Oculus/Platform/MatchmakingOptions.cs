using System;

namespace Oculus.Platform
{
	public class MatchmakingOptions
	{
		private IntPtr Handle;

		public MatchmakingOptions()
		{
			Handle = CAPI.ovr_MatchmakingOptions_Create();
		}

		public void SetCreateRoomDataStore(string key, string value)
		{
			CAPI.ovr_MatchmakingOptions_SetCreateRoomDataStoreString(Handle, key, value);
		}

		public void ClearCreateRoomDataStore()
		{
			CAPI.ovr_MatchmakingOptions_ClearCreateRoomDataStore(Handle);
		}

		public void SetCreateRoomJoinPolicy(RoomJoinPolicy value)
		{
			CAPI.ovr_MatchmakingOptions_SetCreateRoomJoinPolicy(Handle, value);
		}

		public void SetCreateRoomMaxUsers(uint value)
		{
			CAPI.ovr_MatchmakingOptions_SetCreateRoomMaxUsers(Handle, value);
		}

		public void AddEnqueueAdditionalUser(ulong userID)
		{
			CAPI.ovr_MatchmakingOptions_AddEnqueueAdditionalUser(Handle, userID);
		}

		public void ClearEnqueueAdditionalUsers()
		{
			CAPI.ovr_MatchmakingOptions_ClearEnqueueAdditionalUsers(Handle);
		}

		public void SetEnqueueDataSettings(string key, int value)
		{
			CAPI.ovr_MatchmakingOptions_SetEnqueueDataSettingsInt(Handle, key, value);
		}

		public void SetEnqueueDataSettings(string key, double value)
		{
			CAPI.ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble(Handle, key, value);
		}

		public void SetEnqueueDataSettings(string key, string value)
		{
			CAPI.ovr_MatchmakingOptions_SetEnqueueDataSettingsString(Handle, key, value);
		}

		public void ClearEnqueueDataSettings()
		{
			CAPI.ovr_MatchmakingOptions_ClearEnqueueDataSettings(Handle);
		}

		public void SetEnqueueIsDebug(bool value)
		{
			CAPI.ovr_MatchmakingOptions_SetEnqueueIsDebug(Handle, value);
		}

		public void SetEnqueueQueryKey(string value)
		{
			CAPI.ovr_MatchmakingOptions_SetEnqueueQueryKey(Handle, value);
		}

		public static explicit operator IntPtr(MatchmakingOptions options)
		{
			return options?.Handle ?? IntPtr.Zero;
		}

		~MatchmakingOptions()
		{
			CAPI.ovr_MatchmakingOptions_Destroy(Handle);
		}
	}
}
