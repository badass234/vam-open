namespace Mono.CSharp
{
	public class StackFieldExpr : FieldExpr, IExpressionCleanup
	{
		public bool IsAvailableForReuse
		{
			get
			{
				return ((Field)spec.MemberDefinition).IsAvailableForReuse;
			}
			set
			{
				((Field)spec.MemberDefinition).IsAvailableForReuse = value;
			}
		}

		public StackFieldExpr(Field field)
			: base(field, Location.Null)
		{
		}

		public override void AddressOf(EmitContext ec, AddressOp mode)
		{
			base.AddressOf(ec, mode);
			if (mode == AddressOp.Load)
			{
				IsAvailableForReuse = true;
			}
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			PrepareCleanup(ec);
		}

		public void EmitLoad(EmitContext ec)
		{
			base.Emit(ec);
		}

		public void PrepareCleanup(EmitContext ec)
		{
			IsAvailableForReuse = true;
			if (TypeSpec.IsReferenceType(type))
			{
				ec.AddStatementEpilog(this);
			}
		}

		void IExpressionCleanup.EmitCleanup(EmitContext ec)
		{
			EmitAssign(ec, new NullConstant(type, loc), false, false);
		}
	}
}
