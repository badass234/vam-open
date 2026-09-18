using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class CheckedExpr : Expression
	{
		public Expression Expr;

		public CheckedExpr(Expression e, Location l)
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
			using (ec.With(ResolveContext.Options.AllCheckStateFlags, true))
			{
				return Expr.CreateExpressionTree(ec);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			using (ec.With(ResolveContext.Options.AllCheckStateFlags, true))
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
			using (ec.With(BuilderContext.Options.CheckedScope, true))
			{
				Expr.Emit(ec);
			}
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			using (ec.With(BuilderContext.Options.CheckedScope, true))
			{
				Expr.EmitBranchable(ec, target, on_true);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			Expr.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			using (ctx.With(BuilderContext.Options.CheckedScope, true))
			{
				return Expr.MakeExpression(ctx);
			}
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((CheckedExpr)t).Expr = Expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
