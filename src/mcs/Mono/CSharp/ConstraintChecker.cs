namespace Mono.CSharp
{
	internal struct ConstraintChecker
	{
		private IMemberContext mc;

		private bool recursive_checks;

		public ConstraintChecker(IMemberContext ctx)
		{
			mc = ctx;
			recursive_checks = false;
		}

		public static bool Check(IMemberContext mc, TypeSpec type, Location loc)
		{
			if (type.DeclaringType != null && !Check(mc, type.DeclaringType, loc))
			{
				return false;
			}
			while (type is ElementTypeSpec)
			{
				type = ((ElementTypeSpec)type).Element;
			}
			if (type.Arity == 0)
			{
				return true;
			}
			InflatedTypeSpec inflatedTypeSpec = type as InflatedTypeSpec;
			if (inflatedTypeSpec == null)
			{
				return true;
			}
			TypeParameterSpec[] constraints = inflatedTypeSpec.Constraints;
			if (constraints == null)
			{
				return true;
			}
			if (inflatedTypeSpec.HasConstraintsChecked)
			{
				return true;
			}
			ConstraintChecker constraintChecker = new ConstraintChecker(mc);
			constraintChecker.recursive_checks = true;
			if (constraintChecker.CheckAll(inflatedTypeSpec.GetDefinition(), type.TypeArguments, constraints, loc))
			{
				inflatedTypeSpec.HasConstraintsChecked = true;
				return true;
			}
			return false;
		}

		public bool CheckAll(MemberSpec context, TypeSpec[] targs, TypeParameterSpec[] tparams, Location loc)
		{
			if (targs == null)
			{
				return true;
			}
			for (int i = 0; i < tparams.Length; i++)
			{
				TypeSpec typeSpec = targs[i];
				if (!CheckConstraint(context, typeSpec, tparams[i], loc))
				{
					return false;
				}
				if (recursive_checks && !Check(mc, typeSpec, loc))
				{
					return false;
				}
			}
			return true;
		}

		private bool CheckConstraint(MemberSpec context, TypeSpec atype, TypeParameterSpec tparam, Location loc)
		{
			if (tparam.HasSpecialClass && !TypeSpec.IsReferenceType(atype))
			{
				if (mc != null)
				{
					mc.Module.Compiler.Report.Error(452, loc, "The type `{0}' must be a reference type in order to use it as type parameter `{1}' in the generic type or method `{2}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError());
				}
				return false;
			}
			if (tparam.HasSpecialStruct && (!TypeSpec.IsValueType(atype) || atype.IsNullableType))
			{
				if (mc != null)
				{
					mc.Module.Compiler.Report.Error(453, loc, "The type `{0}' must be a non-nullable value type in order to use it as type parameter `{1}' in the generic type or method `{2}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError());
				}
				return false;
			}
			bool result = true;
			if (tparam.HasTypeConstraint)
			{
				if (!CheckConversion(mc, context, atype, tparam, tparam.BaseType, loc))
				{
					if (mc == null)
					{
						return false;
					}
					result = false;
				}
			}
			if (tparam.InterfacesDefined != null)
			{
				TypeSpec[] interfacesDefined = tparam.InterfacesDefined;
				foreach (TypeSpec ttype in interfacesDefined)
				{
					if (!CheckConversion(mc, context, atype, tparam, ttype, loc))
					{
						if (mc == null)
						{
							return false;
						}
						result = false;
						break;
					}
				}
			}
			if (tparam.TypeArguments != null)
			{
				TypeSpec[] interfacesDefined = tparam.TypeArguments;
				foreach (TypeSpec ttype2 in interfacesDefined)
				{
					if (!CheckConversion(mc, context, atype, tparam, ttype2, loc))
					{
						if (mc == null)
						{
							return false;
						}
						result = false;
						break;
					}
				}
			}
			if (!tparam.HasSpecialConstructor)
			{
				return result;
			}
			if (!HasDefaultConstructor(atype))
			{
				if (mc != null)
				{
					mc.Module.Compiler.Report.SymbolRelatedToPreviousError(atype);
					mc.Module.Compiler.Report.Error(310, loc, "The type `{0}' must have a public parameterless constructor in order to use it as parameter `{1}' in the generic type or method `{2}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError());
				}
				return false;
			}
			return result;
		}

		private static bool HasDynamicTypeArgument(TypeSpec[] targs)
		{
			foreach (TypeSpec typeSpec in targs)
			{
				if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					return true;
				}
				if (HasDynamicTypeArgument(typeSpec.TypeArguments))
				{
					return true;
				}
			}
			return false;
		}

		private bool CheckConversion(IMemberContext mc, MemberSpec context, TypeSpec atype, TypeParameterSpec tparam, TypeSpec ttype, Location loc)
		{
			if (atype == ttype)
			{
				return true;
			}
			if (atype.IsGenericParameter)
			{
				TypeParameterSpec typeParameterSpec = (TypeParameterSpec)atype;
				if (typeParameterSpec.HasDependencyOn(ttype))
				{
					return true;
				}
				if (Convert.ImplicitTypeParameterConversion(null, typeParameterSpec, ttype) != null)
				{
					return true;
				}
			}
			else if (TypeSpec.IsValueType(atype))
			{
				if (atype.IsNullableType)
				{
					if (TypeSpec.IsBaseClass(atype, ttype, false))
					{
						return true;
					}
				}
				else if (Convert.ImplicitBoxingConversion(null, atype, ttype) != null)
				{
					return true;
				}
			}
			else if (Convert.ImplicitReferenceConversionExists(atype, ttype) || Convert.ImplicitBoxingConversion(null, atype, ttype) != null)
			{
				return true;
			}
			if (mc != null)
			{
				mc.Module.Compiler.Report.SymbolRelatedToPreviousError(tparam);
				if (atype.IsGenericParameter)
				{
					mc.Module.Compiler.Report.Error(314, loc, "The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no boxing or type parameter conversion from `{0}' to `{3}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError(), ttype.GetSignatureForError());
				}
				else if (TypeSpec.IsValueType(atype))
				{
					if (atype.IsNullableType)
					{
						if (ttype.IsInterface)
						{
							mc.Module.Compiler.Report.Error(313, loc, "The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. The nullable type `{0}' never satisfies interface constraint `{3}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError(), ttype.GetSignatureForError());
						}
						else
						{
							mc.Module.Compiler.Report.Error(312, loc, "The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. The nullable type `{0}' does not satisfy constraint `{3}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError(), ttype.GetSignatureForError());
						}
					}
					else
					{
						mc.Module.Compiler.Report.Error(315, loc, "The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no boxing conversion from `{0}' to `{3}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError(), ttype.GetSignatureForError());
					}
				}
				else
				{
					mc.Module.Compiler.Report.Error(311, loc, "The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no implicit reference conversion from `{0}' to `{3}'", atype.GetSignatureForError(), tparam.GetSignatureForError(), context.GetSignatureForError(), ttype.GetSignatureForError());
				}
			}
			return false;
		}

		private static bool HasDefaultConstructor(TypeSpec atype)
		{
			TypeParameterSpec typeParameterSpec = atype as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				if (!typeParameterSpec.HasSpecialConstructor)
				{
					return typeParameterSpec.HasSpecialStruct;
				}
				return true;
			}
			if (atype.IsStruct || atype.IsEnum)
			{
				return true;
			}
			if (atype.IsAbstract)
			{
				return false;
			}
			MemberSpec memberSpec = MemberCache.FindMember(atype.GetDefinition(), MemberFilter.Constructor(ParametersCompiled.EmptyReadOnlyParameters), BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly);
			if (memberSpec != null)
			{
				return (memberSpec.Modifiers & Modifiers.PUBLIC) != 0;
			}
			return false;
		}
	}
}
