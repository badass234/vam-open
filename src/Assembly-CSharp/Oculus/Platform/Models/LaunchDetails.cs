using System;

namespace Oculus.Platform.Models
{
	public class LaunchDetails
	{
		public readonly string DeeplinkMessage;

		public readonly string LaunchSource;

		public readonly LaunchType LaunchType;

		public readonly ulong RoomID;

		public readonly UserList UsersOptional;

		[Obsolete("Deprecated in favor of UsersOptional")]
		public readonly UserList Users;

		public LaunchDetails(IntPtr o)
		{
			DeeplinkMessage = CAPI.ovr_LaunchDetails_GetDeeplinkMessage(o);
			LaunchSource = CAPI.ovr_LaunchDetails_GetLaunchSource(o);
			LaunchType = CAPI.ovr_LaunchDetails_GetLaunchType(o);
			RoomID = CAPI.ovr_LaunchDetails_GetRoomID(o);
			IntPtr intPtr = CAPI.ovr_LaunchDetails_GetUsers(o);
			Users = new UserList(intPtr);
			if (intPtr == IntPtr.Zero)
			{
				UsersOptional = null;
			}
			else
			{
				UsersOptional = Users;
			}
		}
	}
}
