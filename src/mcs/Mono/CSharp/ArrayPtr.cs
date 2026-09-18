using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class ArrayPtr : FixedBufferPtr
	{
		public ArrayPtr(Expression array, TypeSpec array_type, Location l)
			: base(array, array_type, l)
		{
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			ec.EmitInt(0);
			ec.Emit(OpCodes.Ldelema, ((PointerContainer)type).Element);
		}
	}
}
