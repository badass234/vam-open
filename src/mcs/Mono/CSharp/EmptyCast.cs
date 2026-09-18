using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class EmptyCast : TypeCast
	{
		private EmptyCast(Expression child, TypeSpec target_type)
			: base(child, target_type)
		{
		}

		public static Expression Create(Expression child, TypeSpec type)
		{
			Constant constant = child as Constant;
			if (constant != null)
			{
				EnumConstant enumConstant = constant as EnumConstant;
				if (enumConstant != null)
				{
					constant = enumConstant.Child;
				}
				if (!(constant is ReducedExpression.ReducedConstantExpression))
				{
					if (constant.Type == type)
					{
						return constant;
					}
					Constant constant2 = constant.ConvertImplicitly(type);
					if (constant2 != null)
					{
						return constant2;
					}
				}
			}
			EmptyCast emptyCast = child as EmptyCast;
			if (emptyCast != null)
			{
				return new EmptyCast(emptyCast.child, type);
			}
			return new EmptyCast(child, type);
		}

		public override void EmitBranchable(EmitContext ec, Label label, bool on_true)
		{
			child.EmitBranchable(ec, label, on_true);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			child.EmitSideEffect(ec);
		}
	}
}
