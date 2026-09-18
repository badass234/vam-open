using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class DefaultParameterValueExpression : CompositeExpression
	{
		public DefaultParameterValueExpression(Expression expr)
			: base(expr)
		{
		}

		public void Resolve(ResolveContext rc, Parameter p)
		{
			Expression expression = Resolve(rc);
			if (expression == null)
			{
				expr = ErrorExpression.Instance;
				return;
			}
			expression = base.Child;
			if (!(expression is Constant) && !(expression is DefaultValueExpression) && (!(expression is New) || !((New)expression).IsGeneratedStructConstructor))
			{
				if (!(expression is ErrorExpression))
				{
					rc.Report.Error(1736, base.Location, "The expression being assigned to optional parameter `{0}' must be a constant or default value", p.Name);
				}
				return;
			}
			TypeSpec typeSpec = p.Type;
			if (type == typeSpec)
			{
				return;
			}
			Expression expression2 = Convert.ImplicitConversionStandard(rc, expression, typeSpec, base.Location);
			if (expression2 != null)
			{
				if (typeSpec.IsNullableType && expression2 is Wrap)
				{
					expression2 = ((Wrap)expression2).Child;
					if (!(expression2 is Constant))
					{
						rc.Report.Error(1770, base.Location, "The expression being assigned to nullable optional parameter `{0}' must be default value", p.Name);
						return;
					}
				}
				if (!expression.IsNull && TypeSpec.IsReferenceType(typeSpec) && typeSpec.BuiltinType != BuiltinTypeSpec.Type.String)
				{
					rc.Report.Error(1763, base.Location, "Optional parameter `{0}' of type `{1}' can only be initialized with `null'", p.Name, typeSpec.GetSignatureForError());
				}
				else
				{
					expr = expression2;
				}
			}
			else
			{
				rc.Report.Error(1750, base.Location, "Optional parameter expression of type `{0}' cannot be converted to parameter type `{1}'", type.GetSignatureForError(), typeSpec.GetSignatureForError());
				expr = ErrorExpression.Instance;
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
