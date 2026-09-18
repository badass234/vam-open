namespace Mono.CSharp
{
	internal class AsyncInitializerStatement : StatementExpression
	{
		public AsyncInitializerStatement(AsyncInitializer expr)
			: base(expr)
		{
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			base.DoFlowAnalysis(fc);
			AsyncInitializer obj = (AsyncInitializer)base.Expr;
			bool result = !obj.Block.HasReachableClosingBrace;
			if (((AsyncTaskStorey)obj.Storey).ReturnType.IsGenericTask)
			{
				return result;
			}
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (!rc.IsUnreachable)
			{
				reachable = true;
			}
			AsyncInitializer obj = (AsyncInitializer)base.Expr;
			rc = obj.Block.MarkReachable(rc);
			AsyncTaskStorey asyncTaskStorey = (AsyncTaskStorey)obj.Storey;
			if (asyncTaskStorey.ReturnType != null && asyncTaskStorey.ReturnType.IsGenericTask)
			{
				return rc;
			}
			return Reachability.CreateUnreachable();
		}
	}
}
