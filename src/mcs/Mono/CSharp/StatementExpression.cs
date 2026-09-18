namespace Mono.CSharp
{
	public class StatementExpression : Statement
	{
		private ExpressionStatement expr;

		public ExpressionStatement Expr => expr;

		public StatementExpression(ExpressionStatement expr)
		{
			this.expr = expr;
			loc = expr.StartLocation;
		}

		public StatementExpression(ExpressionStatement expr, Location loc)
		{
			this.expr = expr;
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			((StatementExpression)t).expr = (ExpressionStatement)expr.Clone(clonectx);
		}

		protected override void DoEmit(EmitContext ec)
		{
			expr.EmitStatement(ec);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			expr.MarkReachable(rc);
			return rc;
		}

		public override bool Resolve(BlockContext ec)
		{
			expr = expr.ResolveStatement(ec);
			return expr != null;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
