using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ArrayIndexCast : TypeCast
	{
		public ArrayIndexCast(Expression expr, TypeSpec returnType)
			: base(expr, returnType)
		{
			if (expr.Type == returnType)
			{
				throw new ArgumentException("unnecessary array index conversion");
			}
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			using (ec.Set(ResolveContext.Options.CheckedScope))
			{
				return base.CreateExpressionTree(ec);
			}
		}

		public override void Emit(EmitContext ec)
		{
			child.Emit(ec);
			switch (child.Type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.UInt:
				ec.Emit(OpCodes.Conv_U);
				break;
			case BuiltinTypeSpec.Type.Long:
				ec.Emit(OpCodes.Conv_Ovf_I);
				break;
			case BuiltinTypeSpec.Type.ULong:
				ec.Emit(OpCodes.Conv_Ovf_I_Un);
				break;
			default:
				throw new InternalErrorException("Cannot emit cast to unknown array element type", type);
			}
		}
	}
}
