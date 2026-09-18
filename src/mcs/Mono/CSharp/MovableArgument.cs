namespace Mono.CSharp
{
	public class MovableArgument : Argument
	{
		private LocalTemporary variable;

		public MovableArgument(Argument arg)
			: this(arg.Expr, arg.ArgType)
		{
		}

		protected MovableArgument(Expression expr, AType modifier)
			: base(expr, modifier)
		{
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			if (variable != null)
			{
				variable.Release(ec);
			}
		}

		public void EmitToVariable(EmitContext ec)
		{
			TypeSpec typeSpec = Expr.Type;
			if (base.IsByRef)
			{
				((IMemoryLocation)Expr).AddressOf(ec, AddressOp.LoadStore);
				typeSpec = ReferenceContainer.MakeType(ec.Module, typeSpec);
			}
			else
			{
				Expr.Emit(ec);
			}
			variable = new LocalTemporary(typeSpec);
			variable.Store(ec);
			Expr = variable;
		}
	}
}
