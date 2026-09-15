using System;

namespace Oculus.Platform.Models
{
	public class UserAndRoom
	{
		public readonly Room RoomOptional;

		[Obsolete("Deprecated in favor of RoomOptional")]
		public readonly Room Room;

		public readonly User User;

		public UserAndRoom(IntPtr o)
		{
			IntPtr intPtr = CAPI.ovr_UserAndRoom_GetRoom(o);
			Room = new Room(intPtr);
			if (intPtr == IntPtr.Zero)
			{
				RoomOptional = null;
			}
			else
			{
				RoomOptional = Room;
			}
			User = new User(CAPI.ovr_UserAndRoom_GetUser(o));
		}
	}
}
