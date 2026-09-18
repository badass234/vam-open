using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class TypeOf : Expression
	{
		private FullNamedExpression QueriedType;

		private TypeSpec typearg;

		public override bool IsSideEffectFree => true;

		public TypeSpec TypeArgument => typearg;

		public FullNamedExpression TypeExpression => QueriedType;

		public TypeOf(FullNamedExpression queried_type, Location l)
		{
			QueriedType = queried_type;
			loc = l;
		}

		public TypeOf(TypeSpec type, Location loc)
		{
			typearg = type;
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			TypeOf typeOf = (TypeOf)t;
			if (QueriedType != null)
			{
				typeOf.QueriedType = (FullNamedExpression)QueriedType.Clone(clonectx);
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(this));
			arguments.Add(new Argument(new TypeOf(new TypeExpression(type, loc), loc)));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (eclass != ExprClass.Unresolved)
			{
				return this;
			}
			if (typearg == null)
			{
				using (ec.Set(ResolveContext.Options.UnsafeScope))
				{
					typearg = QueriedType.ResolveAsType(ec, true);
				}
				if (typearg == null)
				{
					return null;
				}
				if (typearg.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					ec.Report.Error(1962, QueriedType.Location, "The typeof operator cannot be used on the dynamic type");
				}
			}
			type = ec.BuiltinTypes.Type;
			eclass = ExprClass.Value;
			return this;
		}

		private static bool ContainsDynamicType(TypeSpec type)
		{
			if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return true;
			}
			ElementTypeSpec elementTypeSpec = type as ElementTypeSpec;
			if (elementTypeSpec != null)
			{
				return ContainsDynamicType(elementTypeSpec.Element);
			}
			TypeSpec[] typeArguments = type.TypeArguments;
			for (int i = 0; i < typeArguments.Length; i++)
			{
				if (ContainsDynamicType(typeArguments[i]))
				{
					return true;
				}
			}
			return false;
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			if (targetType != type)
			{
				enc.Encode(type);
			}
			if (typearg is InflatedTypeSpec)
			{
				TypeSpec declaringType = typearg;
				do
				{
					if (InflatedTypeSpec.ContainsTypeParameter(declaringType))
					{
						rc.Module.Compiler.Report.Error(416, loc, "`{0}': an attribute argument cannot use type parameters", typearg.GetSignatureForError());
						return;
					}
					declaringType = declaringType.DeclaringType;
				}
				while (declaringType != null);
			}
			if (ContainsDynamicType(typearg))
			{
				Attribute.Error_AttributeArgumentIsDynamic(rc, loc);
			}
			else
			{
				enc.EncodeTypeName(typearg);
			}
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Ldtoken, typearg);
			MethodSpec methodSpec = ec.Module.PredefinedMembers.TypeGetTypeFromHandle.Resolve(loc);
			if (methodSpec != null)
			{
				ec.Emit(OpCodes.Call, methodSpec);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
