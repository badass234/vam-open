using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class CompletionElementInitializer : CompletingExpression
	{
		private string partial_name;

		public CompletionElementInitializer(string partial_name, Location l)
		{
			this.partial_name = partial_name;
			loc = l;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			List<string> list = (from l in MemberCache.GetCompletitionMembers(ec, ec.CurrentInitializerVariable.Type, partial_name)
				where (l.Kind & (MemberKind.Field | MemberKind.Property)) != 0
				select l.Name).ToList();
			if (partial_name != null)
			{
				List<string> list2 = new List<string>();
				CompletingExpression.AppendResults(list2, partial_name, list);
				list = list2;
			}
			throw new CompletionResult((partial_name == null) ? "" : partial_name, list.Distinct().ToArray());
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}
	}
}
