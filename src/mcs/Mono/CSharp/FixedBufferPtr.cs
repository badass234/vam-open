using System;

namespace Mono.CSharp
{
	internal class FixedBufferPtr : Expression
	{
		private readonly Expression array;

		public FixedBufferPtr(Expression array, TypeSpec array_type, Location l)
		{
			type = array_type;
			this.array = array;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotImplementedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Error_PointerInsideExpressionTree(ec);
			return null;
		}

		public override void Emit(EmitContext ec)
		{
			array.Emit(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			type = PointerContainer.MakeType(ec.Module, type);
			eclass = ExprClass.Value;
			return this;
		}
	}
}
