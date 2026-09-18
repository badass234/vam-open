namespace Mono.CSharp
{
	internal class DynamicTypeExpr : TypeExpr
	{
		public DynamicTypeExpr(Location loc)
		{
			base.loc = loc;
		}

		public override TypeSpec ResolveAsType(IMemberContext ec, bool allowUnboundTypeArguments)
		{
			eclass = ExprClass.Type;
			type = ec.Module.Compiler.BuiltinTypes.Dynamic;
			return type;
		}
	}
}
