using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	public class Wrap : TypeCast
	{
		private Wrap(Expression expr, TypeSpec type)
			: base(expr, type)
		{
			eclass = ExprClass.Value;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			TypeCast typeCast = child as TypeCast;
			if (typeCast != null)
			{
				child.Type = type;
				return typeCast.CreateExpressionTree(ec);
			}
			UserCast userCast = child as UserCast;
			if (userCast != null)
			{
				child.Type = type;
				return userCast.CreateExpressionTree(ec);
			}
			return base.CreateExpressionTree(ec);
		}

		public static Expression Create(Expression expr, TypeSpec type)
		{
			Unwrap unwrap = expr as Unwrap;
			if (unwrap != null && expr.Type == NullableInfo.GetUnderlyingType(type))
			{
				return unwrap.Original;
			}
			return new Wrap(expr, type);
		}

		public override void Emit(EmitContext ec)
		{
			child.Emit(ec);
			ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
		}
	}
}
