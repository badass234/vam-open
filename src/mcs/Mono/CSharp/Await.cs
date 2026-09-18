using System;

namespace Mono.CSharp
{
	public class Await : ExpressionStatement
	{
		private Expression expr;

		private AwaitStatement stmt;

		public Expression Expr => expr;

		public AwaitStatement Statement => stmt;

		public Await(Expression expr, Location loc)
		{
			this.expr = expr;
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			((Await)target).expr = expr.Clone(clonectx);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotImplementedException("ET");
		}

		public override bool ContainsEmitWithAwait()
		{
			return true;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			stmt.Expr.FlowAnalysis(fc);
			stmt.RegisterResumePoint();
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (rc.HasSet(ResolveContext.Options.LockScope))
			{
				rc.Report.Error(1996, loc, "The `await' operator cannot be used in the body of a lock statement");
			}
			if (rc.IsUnsafe)
			{
				rc.Report.Error(4004, loc, "The `await' operator cannot be used in an unsafe context");
			}
			BlockContext bc = (BlockContext)rc;
			stmt = new AwaitStatement(expr, loc);
			if (!stmt.Resolve(bc))
			{
				return null;
			}
			type = stmt.ResultType;
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			stmt.EmitPrologue(ec);
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				stmt.Emit(ec);
			}
		}

		public override Expression EmitToField(EmitContext ec)
		{
			stmt.EmitPrologue(ec);
			return stmt.GetResultExpression(ec);
		}

		public void EmitAssign(EmitContext ec, FieldExpr field)
		{
			stmt.EmitPrologue(ec);
			field.InstanceExpression.Emit(ec);
			stmt.Emit(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			stmt.EmitStatement(ec);
		}

		public override void MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			stmt.MarkReachable(rc);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
