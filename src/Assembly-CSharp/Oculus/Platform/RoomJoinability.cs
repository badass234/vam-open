using System.ComponentModel;

namespace Oculus.Platform
{
	public enum RoomJoinability
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("ARE_IN")]
		AreIn,
		[Description("ARE_KICKED")]
		AreKicked,
		[Description("CAN_JOIN")]
		CanJoin,
		[Description("IS_FULL")]
		IsFull,
		[Description("NO_VIEWER")]
		NoViewer,
		[Description("POLICY_PREVENTS")]
		PolicyPrevents
	}
}
