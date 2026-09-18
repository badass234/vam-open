using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class UnreachableExpression : Expression
	{
		public UnreachableExpression(Expression expr)
		{
			loc = expr.Location;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotImplementedException();
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			throw new NotSupportedException();
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			fc.Report.Warning(429, 4, loc, "Unreachable expression code detected");
		}

		public override void Emit(EmitContext ec)
		{
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
		}
	}
}
