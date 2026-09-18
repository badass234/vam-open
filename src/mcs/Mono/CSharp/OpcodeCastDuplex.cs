using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class OpcodeCastDuplex : OpcodeCast
	{
		private readonly OpCode second;

		public OpcodeCastDuplex(Expression child, TypeSpec returnType, OpCode first, OpCode second)
			: base(child, returnType, first)
		{
			this.second = second;
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			ec.Emit(second);
		}
	}
}
