using System.Collections.Generic;

namespace Mono.CSharp
{
	internal sealed class WarningMessage : AbstractMessage
	{
		public override bool IsWarning => true;

		public override string MessageType => "warning";

		public WarningMessage(int code, Location loc, string message, List<string> extra_info)
			: base(code, loc, message, extra_info)
		{
		}
	}
}
