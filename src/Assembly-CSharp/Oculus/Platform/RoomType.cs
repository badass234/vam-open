using System.ComponentModel;

namespace Oculus.Platform
{
	public enum RoomType
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("MATCHMAKING")]
		Matchmaking,
		[Description("MODERATED")]
		Moderated,
		[Description("PRIVATE")]
		Private,
		[Description("SOLO")]
		Solo
	}
}
