namespace Mono.CSharp
{
	internal class GenericTypeExpr : TypeExpr
	{
		private TypeArguments args;

		private TypeSpec open_type;

		public GenericTypeExpr(TypeSpec open_type, TypeArguments args, Location l)
		{
			this.open_type = open_type;
			loc = l;
			this.args = args;
		}

		public override string GetSignatureForError()
		{
			return type.GetSignatureForError();
		}

		public override TypeSpec ResolveAsType(IMemberContext mc, bool allowUnboundTypeArguments = false)
		{
			if (eclass != ExprClass.Unresolved)
			{
				return type;
			}
			if (!args.Resolve(mc, allowUnboundTypeArguments))
			{
				return null;
			}
			TypeSpec[] arguments = args.Arguments;
			if (arguments == null)
			{
				return null;
			}
			InflatedTypeSpec inflatedTypeSpec = (InflatedTypeSpec)(type = open_type.MakeGenericType(mc, arguments));
			eclass = ExprClass.Type;
			if (!inflatedTypeSpec.HasConstraintsChecked && mc.Module.HasTypesFullyDefined)
			{
				TypeParameterSpec[] constraints = inflatedTypeSpec.Constraints;
				if (constraints != null && new ConstraintChecker(mc).CheckAll(open_type, arguments, constraints, loc))
				{
					inflatedTypeSpec.HasConstraintsChecked = true;
				}
			}
			return type;
		}

		public override bool Equals(object obj)
		{
			GenericTypeExpr genericTypeExpr = obj as GenericTypeExpr;
			if (genericTypeExpr == null)
			{
				return false;
			}
			if (type == null || genericTypeExpr.type == null)
			{
				return false;
			}
			return type == genericTypeExpr.type;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
