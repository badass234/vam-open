namespace Mono.CSharp
{
	public class ULongLiteral : ULongConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public ULongLiteral(BuiltinTypes types, ulong l, Location loc)
			: base(types, l, loc)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
