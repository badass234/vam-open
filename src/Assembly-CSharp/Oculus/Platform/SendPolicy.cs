using System.ComponentModel;

namespace Oculus.Platform
{
	public enum SendPolicy
	{
		[Description("UNRELIABLE")]
		Unreliable,
		[Description("RELIABLE")]
		Reliable,
		[Description("UNKNOWN")]
		Unknown
	}
}
