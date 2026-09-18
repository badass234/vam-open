namespace Mono.CSharp
{
	public class TypeParameterExpr : TypeExpression
	{
		public TypeParameterExpr(TypeParameter type_parameter, Location loc)
			: base(type_parameter.Type, loc)
		{
			eclass = ExprClass.TypeParameter;
		}
	}
}
