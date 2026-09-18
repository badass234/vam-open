using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class CompletionSimpleName : CompletingExpression
	{
		public string Prefix;

		public CompletionSimpleName(string prefix, Location l)
		{
			loc = l;
			Prefix = prefix;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			List<string> list = new List<string>();
			ec.CurrentMemberDefinition.GetCompletionStartingWith(Prefix, list);
			throw new CompletionResult(Prefix, (from l in list.Distinct()
				select l.Substring(Prefix.Length)).ToArray());
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}
	}
}
