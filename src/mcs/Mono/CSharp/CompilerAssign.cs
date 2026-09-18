namespace Mono.CSharp
{
	internal class CompilerAssign : Assign
	{
		public CompilerAssign(Expression target, Expression source, Location loc)
			: base(target, source, loc)
		{
			if (target.Type != null)
			{
				type = target.Type;
				eclass = ExprClass.Value;
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression result = base.DoResolve(ec);
			VariableReference variableReference = target as VariableReference;
			if (variableReference != null && variableReference.VariableInfo != null)
			{
				variableReference.VariableInfo.IsEverAssigned = false;
			}
			return result;
		}

		public void UpdateSource(Expression source)
		{
			base.source = source;
		}
	}
}
