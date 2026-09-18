using System.Linq.Expressions;

namespace Mono.CSharp
{
	public class NullLiteral : NullConstant
	{
		public override bool IsLiteral => true;

		public NullLiteral(Location loc)
			: base(InternalType.NullLiteral, loc)
		{
		}

		public override void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec t, bool expl)
		{
			if (t.IsGenericParameter)
			{
				ec.Report.Error(403, loc, "Cannot convert null to the type parameter `{0}' because it could be a value type. Consider using `default ({0})' instead", t.Name);
			}
			else if (TypeSpec.IsValueType(t))
			{
				ec.Report.Error(37, loc, "Cannot convert null to `{0}' because it is a value type", t.GetSignatureForError());
			}
			else
			{
				base.Error_ValueCannotBeConverted(ec, t, expl);
			}
		}

		public override string GetValueAsLiteral()
		{
			return "null";
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Constant(null);
		}
	}
}
