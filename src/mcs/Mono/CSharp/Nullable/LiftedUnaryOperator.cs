using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	public class LiftedUnaryOperator : Unary, IMemoryLocation
	{
		private Unwrap unwrap;

		private Expression user_operator;

		public LiftedUnaryOperator(Operator op, Expression expr, Location loc)
			: base(op, expr, loc)
		{
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			unwrap.AddressOf(ec, mode);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (user_operator != null)
			{
				return user_operator.CreateExpressionTree(ec);
			}
			if (Oper == Operator.UnaryPlus)
			{
				return Expr.CreateExpressionTree(ec);
			}
			return base.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			unwrap = Unwrap.Create(Expr, false);
			if (unwrap == null)
			{
				return null;
			}
			Expression expression = base.ResolveOperator(ec, unwrap);
			if (expression == null)
			{
				Error_OperatorCannotBeApplied(ec, loc, Unary.OperName(Oper), Expr.Type);
				return null;
			}
			if (expression != this)
			{
				if (user_operator == null)
				{
					return expression;
				}
			}
			else
			{
				expression = (Expr = LiftExpression(ec, Expr));
			}
			if (expression == null)
			{
				return null;
			}
			eclass = ExprClass.Value;
			type = expression.Type;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			unwrap.EmitCheck(ec);
			ec.Emit(OpCodes.Brfalse, label);
			if (user_operator != null)
			{
				user_operator.Emit(ec);
			}
			else
			{
				EmitOperator(ec, NullableInfo.GetUnderlyingType(type));
			}
			ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			LiftedNull.Create(type, loc).Emit(ec);
			ec.MarkLabel(label2);
		}

		private static Expression LiftExpression(ResolveContext ec, Expression expr)
		{
			NullableType nullableType = new NullableType(expr.Type, expr.Location);
			if (nullableType.ResolveAsType(ec) == null)
			{
				return null;
			}
			expr.Type = nullableType.Type;
			return expr;
		}

		protected override Expression ResolveEnumOperator(ResolveContext ec, Expression expr, TypeSpec[] predefined)
		{
			expr = base.ResolveEnumOperator(ec, expr, predefined);
			if (expr == null)
			{
				return null;
			}
			Expr = LiftExpression(ec, Expr);
			return LiftExpression(ec, expr);
		}

		protected override Expression ResolveUserOperator(ResolveContext ec, Expression expr)
		{
			expr = base.ResolveUserOperator(ec, expr);
			if (expr == null)
			{
				return null;
			}
			if (Expr is Unwrap)
			{
				user_operator = LiftExpression(ec, expr);
				return user_operator;
			}
			return expr;
		}
	}
}
