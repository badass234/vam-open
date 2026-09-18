namespace Mono.CSharp
{
	public class BlockConstant : BlockVariable
	{
		public BlockConstant(FullNamedExpression type, LocalVariable li)
			: base(type, li)
		{
		}

		public override void Emit(EmitContext ec)
		{
		}

		protected override Expression ResolveInitializer(BlockContext bc, LocalVariable li, Expression initializer)
		{
			initializer = initializer.Resolve(bc);
			if (initializer == null)
			{
				return null;
			}
			Constant constant = initializer as Constant;
			if (constant == null)
			{
				initializer.Error_ExpressionMustBeConstant(bc, initializer.Location, li.Name);
				return null;
			}
			constant = constant.ConvertImplicitly(li.Type);
			if (constant == null)
			{
				if (TypeSpec.IsReferenceType(li.Type))
				{
					initializer.Error_ConstantCanBeInitializedWithNullOnly(bc, li.Type, initializer.Location, li.Name);
				}
				else
				{
					initializer.Error_ValueCannotBeConverted(bc, li.Type, false);
				}
				return null;
			}
			li.ConstantValue = constant;
			return initializer;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
