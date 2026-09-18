using System;

namespace Mono.CSharp
{
	public abstract class ShimExpression : Expression
	{
		protected Expression expr;

		public Expression Expr => expr;

		protected ShimExpression(Expression expr)
		{
			this.expr = expr;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			if (expr != null)
			{
				((ShimExpression)t).expr = expr.Clone(clonectx);
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		public override void Emit(EmitContext ec)
		{
			throw new InternalErrorException("Missing Resolve call");
		}
	}
}
