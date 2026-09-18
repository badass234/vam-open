using System.Reflection.Emit;

namespace Mono.CSharp
{
	public sealed class ClassCast : TypeCast
	{
		private readonly bool forced;

		public ClassCast(Expression child, TypeSpec return_type)
			: base(child, return_type)
		{
		}

		public ClassCast(Expression child, TypeSpec return_type, bool forced)
			: base(child, return_type)
		{
			this.forced = forced;
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			bool flag = TypeManager.IsGenericParameter(child.Type);
			if (flag)
			{
				ec.Emit(OpCodes.Box, child.Type);
			}
			if (type.IsGenericParameter)
			{
				ec.Emit(OpCodes.Unbox_Any, type);
			}
			else if (!flag || forced)
			{
				ec.Emit(OpCodes.Castclass, type);
			}
		}
	}
}
