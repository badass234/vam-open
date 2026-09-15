using System.ComponentModel;

namespace Oculus.Platform
{
	public enum RoomJoinPolicy
	{
		[Description("NONE")]
		None,
		[Description("EVERYONE")]
		Everyone,
		[Description("FRIENDS_OF_MEMBERS")]
		FriendsOfMembers,
		[Description("FRIENDS_OF_OWNER")]
		FriendsOfOwner,
		[Description("INVITED_USERS")]
		InvitedUsers,
		[Description("UNKNOWN")]
		Unknown
	}
}
