using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class UnCheckedExpr : Expression
	{
		public Expression Expr;

		public UnCheckedExpr(Expression e, Location l)
		{
			Expr = e;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			return Expr.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			using (ec.With(ResolveContext.Options.AllCheckStateFlags, false))
			{
				return Expr.CreateExpressionTree(ec);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			using (ec.With(ResolveContext.Options.AllCheckStateFlags, false))
			{
				Expr = Expr.Resolve(ec);
			}
			if (Expr == null)
			{
				return null;
			}
			if (Expr is Constant || Expr is MethodGroupExpr || Expr is AnonymousMethodExpression || Expr is DefaultValueExpression)
			{
				return Expr;
			}
			eclass = Expr.eclass;
			type = Expr.Type;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			using (ec.With(BuilderContext.Options.CheckedScope, false))
			{
				Expr.Emit(ec);
			}
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			using (ec.With(BuilderContext.Options.CheckedScope, false))
			{
				Expr.EmitBranchable(ec, target, on_true);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			Expr.FlowAnalysis(fc);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((UnCheckedExpr)t).Expr = Expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
