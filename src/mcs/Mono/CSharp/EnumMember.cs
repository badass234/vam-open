using System.Reflection;

namespace Mono.CSharp
{
	public class EnumMember : Const
	{
		private class EnumTypeExpr : TypeExpr
		{
			public override TypeSpec ResolveAsType(IMemberContext ec, bool allowUnboundTypeArguments)
			{
				type = ec.CurrentType;
				eclass = ExprClass.Type;
				return type;
			}
		}

		public EnumMember(Enum parent, MemberName name, Attributes attrs)
			: base(parent, new EnumTypeExpr(), Modifiers.PUBLIC, name, attrs)
		{
		}

		private static bool IsValidEnumType(TypeSpec t)
		{
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				return true;
			default:
				return t.IsEnum;
			}
		}

		public override Constant ConvertInitializer(ResolveContext rc, Constant expr)
		{
			if (expr is EnumConstant)
			{
				expr = ((EnumConstant)expr).Child;
			}
			Enum obj = (Enum)Parent;
			TypeSpec underlyingType = obj.UnderlyingType;
			if (expr != null)
			{
				expr = expr.ImplicitConversionRequired(rc, underlyingType);
				if (expr != null && !IsValidEnumType(expr.Type))
				{
					obj.Error_UnderlyingType(base.Location);
					expr = null;
				}
			}
			if (expr == null)
			{
				expr = New.Constantify(underlyingType, base.Location);
			}
			return new EnumConstant(expr, base.MemberType);
		}

		public override bool Define()
		{
			if (!ResolveMemberType())
			{
				return false;
			}
			FieldBuilder = Parent.TypeBuilder.DefineField(base.Name, base.MemberType.GetMetaInfo(), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			spec = new ConstSpec(Parent.Definition, this, base.MemberType, FieldBuilder, base.ModFlags, initializer);
			Parent.MemberCache.AddMember(spec);
			return true;
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
