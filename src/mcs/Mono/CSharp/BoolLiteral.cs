namespace Mono.CSharp
{
	public class BoolLiteral : BoolConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public BoolLiteral(BuiltinTypes types, bool val, Location loc)
			: base(types, val, loc)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
