namespace Mono.CSharp
{
	public class IntLiteral : IntConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public IntLiteral(BuiltinTypes types, int l, Location loc)
			: base(types, l, loc)
		{
		}

		public override Constant ConvertImplicitly(TypeSpec type)
		{
			if (Value == 0 && type.IsEnum)
			{
				Constant constant = ConvertImplicitly(EnumSpec.GetUnderlyingType(type));
				if (constant == null)
				{
					return null;
				}
				return new EnumConstant(constant, type);
			}
			return base.ConvertImplicitly(type);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
