using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ConditionalLogicalOperator : UserOperatorCall
	{
		private readonly bool is_and;

		private Expression oper_expr;

		public ConditionalLogicalOperator(MethodSpec oper, Arguments arguments, Func<ResolveContext, Expression, Expression> expr_tree, bool is_and, Location loc)
			: base(oper, arguments, expr_tree, loc)
		{
			this.is_and = is_and;
			eclass = ExprClass.Unresolved;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			AParametersCollection parameters = oper.Parameters;
			if (!TypeSpecComparer.IsEqual(type, parameters.Types[0]) || !TypeSpecComparer.IsEqual(type, parameters.Types[1]))
			{
				ec.Report.Error(217, loc, "A user-defined operator `{0}' must have each parameter type and return type of the same type in order to be applicable as a short circuit operator", oper.GetSignatureForError());
				return null;
			}
			Expression e = new EmptyExpression(type);
			Expression operatorTrue = Expression.GetOperatorTrue(ec, e, loc);
			Expression operatorFalse = Expression.GetOperatorFalse(ec, e, loc);
			if (operatorTrue == null || operatorFalse == null)
			{
				ec.Report.Error(218, loc, "The type `{0}' must have operator `true' and operator `false' defined when `{1}' is used as a short circuit operator", type.GetSignatureForError(), oper.GetSignatureForError());
				return null;
			}
			oper_expr = (is_and ? operatorFalse : operatorTrue);
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			int num;
			if (ec.HasSet(BuilderContext.Options.AsyncBody))
			{
				num = (arguments[1].Expr.ContainsEmitWithAwait() ? 1 : 0);
				if (num != 0)
				{
					arguments[0] = arguments[0].EmitToField(ec, false);
					arguments[0].Expr.Emit(ec);
					goto IL_0093;
				}
			}
			else
			{
				num = 0;
			}
			arguments[0].Expr.Emit(ec);
			ec.Emit(OpCodes.Dup);
			arguments.RemoveAt(0);
			goto IL_0093;
			IL_0093:
			oper_expr.EmitBranchable(ec, label, true);
			base.Emit(ec);
			if (num != 0)
			{
				Label label2 = ec.DefineLabel();
				ec.Emit(OpCodes.Br_S, label2);
				ec.MarkLabel(label);
				arguments[0].Expr.Emit(ec);
				ec.MarkLabel(label2);
			}
			else
			{
				ec.MarkLabel(label);
			}
		}
	}
}
