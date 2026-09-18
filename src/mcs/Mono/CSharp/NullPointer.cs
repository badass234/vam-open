using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class NullPointer : NullConstant
	{
		public NullPointer(TypeSpec type, Location loc)
			: base(type, loc)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Error_PointerInsideExpressionTree(ec);
			return base.CreateExpressionTree(ec);
		}

		public override void Emit(EmitContext ec)
		{
			ec.EmitInt(0);
			ec.Emit(OpCodes.Conv_U);
		}
	}
}
