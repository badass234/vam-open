using System.Collections.Generic;

namespace Mono.CSharp
{
	public abstract class CompletingExpression : ExpressionStatement
	{
		public static void AppendResults(List<string> results, string prefix, IEnumerable<string> names)
		{
			foreach (string name in names)
			{
				if (name != null && (prefix == null || name.StartsWith(prefix)) && !results.Contains(name))
				{
					if (prefix != null)
					{
						results.Add(name.Substring(prefix.Length));
					}
					else
					{
						results.Add(name);
					}
				}
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return null;
		}

		public override void EmitStatement(EmitContext ec)
		{
		}

		public override void Emit(EmitContext ec)
		{
		}
	}
}
