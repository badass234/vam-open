namespace Mono.CSharp
{
	internal class GenericOpenTypeExpr : TypeExpression
	{
		public GenericOpenTypeExpr(TypeSpec type, Location loc)
			: base(type.GetDefinition(), loc)
		{
		}
	}
}
