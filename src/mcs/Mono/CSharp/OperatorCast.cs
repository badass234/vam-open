using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class OperatorCast : TypeCast
	{
		private readonly MethodSpec conversion_operator;

		public OperatorCast(Expression expr, TypeSpec target_type)
			: this(expr, target_type, target_type, false)
		{
		}

		public OperatorCast(Expression expr, TypeSpec target_type, bool find_explicit)
			: this(expr, target_type, target_type, find_explicit)
		{
		}

		public OperatorCast(Expression expr, TypeSpec declaringType, TypeSpec returnType, bool isExplicit)
			: base(expr, returnType)
		{
			IList<MemberSpec> userOperator = MemberCache.GetUserOperator(declaringType, isExplicit ? Operator.OpType.Explicit : Operator.OpType.Implicit, true);
			if (userOperator != null)
			{
				foreach (MethodSpec item in userOperator)
				{
					if (item.ReturnType == returnType && item.Parameters.Types[0] == expr.Type)
					{
						conversion_operator = item;
						return;
					}
				}
			}
			throw new InternalErrorException("Missing predefined user operator between `{0}' and `{1}'", returnType.GetSignatureForError(), expr.Type.GetSignatureForError());
		}

		public override void Emit(EmitContext ec)
		{
			child.Emit(ec);
			ec.Emit(OpCodes.Call, conversion_operator);
		}
	}
}
