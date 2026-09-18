namespace Mono.CSharp
{
	public abstract class TypeExpr : FullNamedExpression
	{
		public sealed override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments)
		{
			ResolveAsType(mc);
			return this;
		}

		protected sealed override Expression DoResolve(ResolveContext ec)
		{
			ResolveAsType(ec);
			return this;
		}

		public override bool Equals(object obj)
		{
			TypeExpr typeExpr = obj as TypeExpr;
			if (typeExpr == null)
			{
				return false;
			}
			return base.Type == typeExpr.Type;
		}

		public override int GetHashCode()
		{
			return base.Type.GetHashCode();
		}
	}
}
