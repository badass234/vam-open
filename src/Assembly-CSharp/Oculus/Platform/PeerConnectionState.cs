using System.ComponentModel;

namespace Oculus.Platform
{
	public enum PeerConnectionState
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("CONNECTED")]
		Connected,
		[Description("TIMEOUT")]
		Timeout,
		[Description("CLOSED")]
		Closed
	}
}
