namespace Mono.CSharp
{
	public class CharLiteral : CharConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public CharLiteral(BuiltinTypes types, char c, Location loc)
			: base(types, c, loc)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
