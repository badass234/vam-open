using System.ComponentModel;

namespace Oculus.Platform
{
	public enum MatchmakingCriterionImportance
	{
		[Description("REQUIRED")]
		Required,
		[Description("HIGH")]
		High,
		[Description("MEDIUM")]
		Medium,
		[Description("LOW")]
		Low,
		[Description("UNKNOWN")]
		Unknown
	}
}
