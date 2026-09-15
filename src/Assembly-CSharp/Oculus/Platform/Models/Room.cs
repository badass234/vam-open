using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models
{
	public class Room
	{
		public readonly ulong ApplicationID;

		public readonly Dictionary<string, string> DataStore;

		public readonly string Description;

		public readonly ulong ID;

		public readonly UserList InvitedUsersOptional;

		[Obsolete("Deprecated in favor of InvitedUsersOptional")]
		public readonly UserList InvitedUsers;

		public readonly bool IsMembershipLocked;

		public readonly RoomJoinPolicy JoinPolicy;

		public readonly RoomJoinability Joinability;

		public readonly MatchmakingEnqueuedUserList MatchedUsersOptional;

		[Obsolete("Deprecated in favor of MatchedUsersOptional")]
		public readonly MatchmakingEnqueuedUserList MatchedUsers;

		public readonly uint MaxUsers;

		public readonly string Name;

		public readonly User OwnerOptional;

		[Obsolete("Deprecated in favor of OwnerOptional")]
		public readonly User Owner;

		public readonly RoomType Type;

		public readonly UserList UsersOptional;

		[Obsolete("Deprecated in favor of UsersOptional")]
		public readonly UserList Users;

		public readonly uint Version;

		public Room(IntPtr o)
		{
			ApplicationID = CAPI.ovr_Room_GetApplicationID(o);
			DataStore = CAPI.DataStoreFromNative(CAPI.ovr_Room_GetDataStore(o));
			Description = CAPI.ovr_Room_GetDescription(o);
			ID = CAPI.ovr_Room_GetID(o);
			IntPtr intPtr = CAPI.ovr_Room_GetInvitedUsers(o);
			InvitedUsers = new UserList(intPtr);
			if (intPtr == IntPtr.Zero)
			{
				InvitedUsersOptional = null;
			}
			else
			{
				InvitedUsersOptional = InvitedUsers;
			}
			IsMembershipLocked = CAPI.ovr_Room_GetIsMembershipLocked(o);
			JoinPolicy = CAPI.ovr_Room_GetJoinPolicy(o);
			Joinability = CAPI.ovr_Room_GetJoinability(o);
			IntPtr intPtr2 = CAPI.ovr_Room_GetMatchedUsers(o);
			MatchedUsers = new MatchmakingEnqueuedUserList(intPtr2);
			if (intPtr2 == IntPtr.Zero)
			{
				MatchedUsersOptional = null;
			}
			else
			{
				MatchedUsersOptional = MatchedUsers;
			}
			MaxUsers = CAPI.ovr_Room_GetMaxUsers(o);
			Name = CAPI.ovr_Room_GetName(o);
			IntPtr intPtr3 = CAPI.ovr_Room_GetOwner(o);
			Owner = new User(intPtr3);
			if (intPtr3 == IntPtr.Zero)
			{
				OwnerOptional = null;
			}
			else
			{
				OwnerOptional = Owner;
			}
			Type = CAPI.ovr_Room_GetType(o);
			IntPtr intPtr4 = CAPI.ovr_Room_GetUsers(o);
			Users = new UserList(intPtr4);
			if (intPtr4 == IntPtr.Zero)
			{
				UsersOptional = null;
			}
			else
			{
				UsersOptional = Users;
			}
			Version = CAPI.ovr_Room_GetVersion(o);
		}
	}
}
