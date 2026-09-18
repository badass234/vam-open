namespace Mono.CSharp
{
	public class EmptyCompletion : CompletingExpression
	{
		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			throw new CompletionResult("", new string[0]);
		}
	}
}
