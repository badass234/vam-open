using System;

namespace Mono.CSharp
{
	internal static class AttributeTester
	{
		public static void Report_ObsoleteMessage(ObsoleteAttribute oa, string member, Location loc, Report Report)
		{
			if (oa.IsError)
			{
				Report.Error(619, loc, "`{0}' is obsolete: `{1}'", member, oa.Message);
			}
			else if (oa.Message == null || oa.Message.Length == 0)
			{
				Report.Warning(612, 1, loc, "`{0}' is obsolete", member);
			}
			else
			{
				Report.Warning(618, 2, loc, "`{0}' is obsolete: `{1}'", member, oa.Message);
			}
		}
	}
}
