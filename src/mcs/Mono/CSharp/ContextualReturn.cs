using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ContextualReturn : Return
	{
		private ExpressionStatement statement;

		public ContextualReturn(Expression expr)
			: base(expr, expr.StartLocation)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return base.Expr.CreateExpressionTree(ec);
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (statement != null)
			{
				statement.EmitStatement(ec);
				if (unwind_protect)
				{
					ec.Emit(OpCodes.Leave, ec.CreateReturnLabel());
				}
				else
				{
					ec.Emit(OpCodes.Ret);
				}
			}
			else
			{
				base.DoEmit(ec);
			}
		}

		protected override bool DoResolve(BlockContext ec)
		{
			if (ec.ReturnType.Kind == MemberKind.Void)
			{
				base.Expr = base.Expr.Resolve(ec);
				if (base.Expr == null)
				{
					return false;
				}
				statement = base.Expr as ExpressionStatement;
				if (statement == null)
				{
					base.Expr.Error_InvalidExpressionStatement(ec);
				}
				return true;
			}
			return base.DoResolve(ec);
		}
	}
}
