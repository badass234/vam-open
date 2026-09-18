using System;

namespace Mono.CSharp
{
	public class StatementErrorExpression : Statement
	{
		private Expression expr;

		public Expression Expr => expr;

		public StatementErrorExpression(Expression expr)
		{
			this.expr = expr;
			loc = expr.StartLocation;
		}

		public override bool Resolve(BlockContext bc)
		{
			expr.Error_InvalidExpressionStatement(bc);
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			throw new NotSupportedException();
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			return false;
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			((StatementErrorExpression)target).expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
