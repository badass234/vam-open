namespace Mono.CSharp.Nullable
{
	internal static class NullableInfo
	{
		public static MethodSpec GetConstructor(TypeSpec nullableType)
		{
			return (MethodSpec)MemberCache.FindMember(nullableType, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(GetUnderlyingType(nullableType))), BindingRestriction.DeclaredOnly);
		}

		public static MethodSpec GetHasValue(TypeSpec nullableType)
		{
			return (MethodSpec)MemberCache.FindMember(nullableType, MemberFilter.Method("get_HasValue", 0, ParametersCompiled.EmptyReadOnlyParameters, null), BindingRestriction.None);
		}

		public static MethodSpec GetGetValueOrDefault(TypeSpec nullableType)
		{
			return (MethodSpec)MemberCache.FindMember(nullableType, MemberFilter.Method("GetValueOrDefault", 0, ParametersCompiled.EmptyReadOnlyParameters, null), BindingRestriction.None);
		}

		public static MethodSpec GetValue(TypeSpec nullableType)
		{
			return (MethodSpec)MemberCache.FindMember(nullableType, MemberFilter.Method("get_Value", 0, ParametersCompiled.EmptyReadOnlyParameters, null), BindingRestriction.None);
		}

		public static TypeSpec GetUnderlyingType(TypeSpec nullableType)
		{
			return ((InflatedTypeSpec)nullableType).TypeArguments[0];
		}

		public static TypeSpec GetEnumUnderlyingType(ModuleContainer module, TypeSpec nullableEnum)
		{
			return MakeType(module, EnumSpec.GetUnderlyingType(GetUnderlyingType(nullableEnum)));
		}

		public static TypeSpec MakeType(ModuleContainer module, TypeSpec underlyingType)
		{
			return module.PredefinedTypes.Nullable.TypeSpec.MakeGenericType(module, new TypeSpec[1] { underlyingType });
		}
	}
}
