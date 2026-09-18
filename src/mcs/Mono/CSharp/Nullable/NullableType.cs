namespace Mono.CSharp.Nullable
{
	public class NullableType : TypeExpr
	{
		private readonly TypeSpec underlying;

		public NullableType(TypeSpec type, Location loc)
		{
			underlying = type;
			base.loc = loc;
		}

		public override TypeSpec ResolveAsType(IMemberContext ec, bool allowUnboundTypeArguments = false)
		{
			eclass = ExprClass.Type;
			TypeSpec typeSpec = ec.Module.PredefinedTypes.Nullable.Resolve();
			if (typeSpec == null)
			{
				return null;
			}
			TypeArguments args = new TypeArguments(new TypeExpression(underlying, loc));
			GenericTypeExpr genericTypeExpr = new GenericTypeExpr(typeSpec, args, loc);
			type = genericTypeExpr.ResolveAsType(ec);
			return type;
		}
	}
}
