using System.ComponentModel;

namespace Oculus.Platform
{
	public enum RoomMembershipLockStatus
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("LOCK")]
		Lock,
		[Description("UNLOCK")]
		Unlock
	}
}
