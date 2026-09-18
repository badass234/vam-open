namespace Mono.CSharp
{
	public class DoubleLiteral : DoubleConstant, ILiteralConstant
	{
		public override bool IsLiteral => true;

		public DoubleLiteral(BuiltinTypes types, double d, Location loc)
			: base(types, d, loc)
		{
		}

		public override void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec target, bool expl)
		{
			if (target.BuiltinType == BuiltinTypeSpec.Type.Float)
			{
				Error_664(ec, loc, "float", "f");
			}
			else if (target.BuiltinType == BuiltinTypeSpec.Type.Decimal)
			{
				Error_664(ec, loc, "decimal", "m");
			}
			else
			{
				base.Error_ValueCannotBeConverted(ec, target, expl);
			}
		}

		private static void Error_664(ResolveContext ec, Location loc, string type, string suffix)
		{
			ec.Report.Error(664, loc, "Literal of type double cannot be implicitly converted to type `{0}'. Add suffix `{1}' to create a literal of this type", type, suffix);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
