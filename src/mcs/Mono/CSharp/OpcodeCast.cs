using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class OpcodeCast : TypeCast
	{
		private readonly OpCode op;

		public TypeSpec UnderlyingType => child.Type;

		public OpcodeCast(Expression child, TypeSpec return_type, OpCode op)
			: base(child, return_type)
		{
			this.op = op;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			ec.Emit(op);
		}
	}
}
