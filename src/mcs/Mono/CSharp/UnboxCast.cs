using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class UnboxCast : TypeCast
	{
		public UnboxCast(Expression expr, TypeSpec return_type)
			: base(expr, return_type)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			ec.Emit(OpCodes.Unbox_Any, type);
		}
	}
}
