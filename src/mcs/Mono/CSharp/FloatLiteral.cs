namespace Mono.CSharp
{
	public class FloatLiteral : FloatConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public FloatLiteral(BuiltinTypes types, float f, Location loc)
			: base(types, f, loc)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
