using System.ComponentModel;

namespace Oculus.Platform
{
	public enum CloudStorageUpdateStatus
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("OK")]
		Ok,
		[Description("BETTER_VERSION_STORED")]
		BetterVersionStored,
		[Description("MANUAL_MERGE_REQUIRED")]
		ManualMergeRequired
	}
}
