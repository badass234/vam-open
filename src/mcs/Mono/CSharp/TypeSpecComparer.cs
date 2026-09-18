using System.Collections.Generic;

namespace Mono.CSharp
{
	internal static class TypeSpecComparer
	{
		public class DefaultImpl : IEqualityComparer<TypeSpec[]>
		{
			bool IEqualityComparer<TypeSpec[]>.Equals(TypeSpec[] x, TypeSpec[] y)
			{
				if (x == y)
				{
					return true;
				}
				if (x.Length != y.Length)
				{
					return false;
				}
				for (int i = 0; i < x.Length; i++)
				{
					if (x[i] != y[i])
					{
						return false;
					}
				}
				return true;
			}

			int IEqualityComparer<TypeSpec[]>.GetHashCode(TypeSpec[] obj)
			{
				int num = 0;
				for (int i = 0; i < obj.Length; i++)
				{
					num = (num << 5) - num + obj[i].GetHashCode();
				}
				return num;
			}
		}

		public static class Override
		{
			public static bool IsEqual(TypeSpec a, TypeSpec b)
			{
				if (a == b)
				{
					return true;
				}
				TypeParameterSpec typeParameterSpec = a as TypeParameterSpec;
				if (typeParameterSpec != null)
				{
					TypeParameterSpec typeParameterSpec2 = b as TypeParameterSpec;
					if (typeParameterSpec2 != null && typeParameterSpec.IsMethodOwned == typeParameterSpec2.IsMethodOwned)
					{
						return typeParameterSpec.DeclaredPosition == typeParameterSpec2.DeclaredPosition;
					}
					return false;
				}
				ArrayContainer arrayContainer = a as ArrayContainer;
				if (arrayContainer != null)
				{
					ArrayContainer arrayContainer2 = b as ArrayContainer;
					if (arrayContainer2 != null && arrayContainer.Rank == arrayContainer2.Rank)
					{
						return IsEqual(arrayContainer.Element, arrayContainer2.Element);
					}
					return false;
				}
				if (a.BuiltinType == BuiltinTypeSpec.Type.Dynamic || b.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					if (b.BuiltinType != BuiltinTypeSpec.Type.Object)
					{
						return a.BuiltinType == BuiltinTypeSpec.Type.Object;
					}
					return true;
				}
				if (a.MemberDefinition != b.MemberDefinition)
				{
					return false;
				}
				do
				{
					for (int i = 0; i < a.TypeArguments.Length; i++)
					{
						if (!IsEqual(a.TypeArguments[i], b.TypeArguments[i]))
						{
							return false;
						}
					}
					a = a.DeclaringType;
					b = b.DeclaringType;
				}
				while (a != null);
				return true;
			}

			public static bool IsEqual(TypeSpec[] a, TypeSpec[] b)
			{
				if (a == b)
				{
					return true;
				}
				if (a.Length != b.Length)
				{
					return false;
				}
				for (int i = 0; i < a.Length; i++)
				{
					if (!IsEqual(a[i], b[i]))
					{
						return false;
					}
				}
				return true;
			}

			public static bool IsSame(TypeSpec[] a, TypeSpec[] b)
			{
				if (a == b)
				{
					return true;
				}
				if (a == null || b == null || a.Length != b.Length)
				{
					return false;
				}
				for (int i = 0; i < a.Length; i++)
				{
					bool flag = false;
					for (int j = 0; j < b.Length; j++)
					{
						if (IsEqual(a[i], b[j]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				return true;
			}

			public static bool IsEqual(AParametersCollection a, AParametersCollection b)
			{
				if (a == b)
				{
					return true;
				}
				if (a.Count != b.Count)
				{
					return false;
				}
				for (int i = 0; i < a.Count; i++)
				{
					if (!IsEqual(a.Types[i], b.Types[i]))
					{
						return false;
					}
					if ((a.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != (b.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
					{
						return false;
					}
				}
				return true;
			}
		}

		public static class Variant
		{
			public static bool IsEqual(TypeSpec type1, TypeSpec type2)
			{
				if (!type1.IsGeneric || !type2.IsGeneric)
				{
					return false;
				}
				ITypeDefinition memberDefinition = type2.MemberDefinition;
				if (type1.MemberDefinition != memberDefinition)
				{
					return false;
				}
				TypeSpec[] typeArguments = type1.TypeArguments;
				TypeSpec[] typeArguments2 = type2.TypeArguments;
				TypeParameterSpec[] typeParameters = memberDefinition.TypeParameters;
				if (!type1.IsInterface && !type1.IsDelegate)
				{
					return false;
				}
				for (int i = 0; i < typeParameters.Length; i++)
				{
					if (TypeSpecComparer.IsEqual(typeArguments[i], typeArguments2[i]))
					{
						continue;
					}
					switch (typeParameters[i].Variance)
					{
					case Variance.None:
						return false;
					case Variance.Covariant:
						if (!Convert.ImplicitReferenceConversionExists(typeArguments[i], typeArguments2[i]))
						{
							return false;
						}
						break;
					default:
						if (!Convert.ImplicitReferenceConversionExists(typeArguments2[i], typeArguments[i]))
						{
							return false;
						}
						break;
					}
				}
				return true;
			}
		}

		public static class Unify
		{
			public static bool IsEqual(TypeSpec a, TypeSpec b)
			{
				if (a.MemberDefinition != b.MemberDefinition)
				{
					IList<TypeSpec> interfaces = a.Interfaces;
					if (interfaces != null)
					{
						foreach (TypeSpec item in interfaces)
						{
							if (item.Arity > 0 && IsEqual(item, b))
							{
								return true;
							}
						}
					}
					return false;
				}
				TypeSpec[] typeArguments = a.TypeArguments;
				TypeSpec[] typeArguments2 = b.TypeArguments;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (!MayBecomeEqualGenericTypes(typeArguments[i], typeArguments2[i]))
					{
						return false;
					}
				}
				if (a.IsNested && b.IsNested)
				{
					return IsEqual(a.DeclaringType, b.DeclaringType);
				}
				return true;
			}

			private static bool ContainsTypeParameter(TypeSpec tparam, TypeSpec type)
			{
				TypeSpec[] typeArguments = type.TypeArguments;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (tparam == typeArguments[i])
					{
						return true;
					}
					if (ContainsTypeParameter(tparam, typeArguments[i]))
					{
						return true;
					}
				}
				return false;
			}

			private static bool MayBecomeEqualGenericTypes(TypeSpec a, TypeSpec b)
			{
				if (a.IsGenericParameter)
				{
					if (b.IsArray)
					{
						return false;
					}
					if (b.IsGenericParameter)
					{
						if (a != b)
						{
							return a.DeclaringType == b.DeclaringType;
						}
						return false;
					}
					return !ContainsTypeParameter(a, b);
				}
				if (b.IsGenericParameter)
				{
					return MayBecomeEqualGenericTypes(b, a);
				}
				if (TypeManager.IsGenericType(a) || TypeManager.IsGenericType(b))
				{
					return IsEqual(a, b);
				}
				ArrayContainer arrayContainer = a as ArrayContainer;
				if (arrayContainer != null)
				{
					ArrayContainer arrayContainer2 = b as ArrayContainer;
					if (arrayContainer2 == null || arrayContainer.Rank != arrayContainer2.Rank)
					{
						return false;
					}
					return MayBecomeEqualGenericTypes(arrayContainer.Element, arrayContainer2.Element);
				}
				return false;
			}
		}

		public static readonly DefaultImpl Default = new DefaultImpl();

		public static bool Equals(TypeSpec[] x, TypeSpec[] y)
		{
			if (x == y)
			{
				return true;
			}
			if (x.Length != y.Length)
			{
				return false;
			}
			for (int i = 0; i < x.Length; i++)
			{
				if (!IsEqual(x[i], y[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsEqual(TypeSpec a, TypeSpec b)
		{
			if (a == b)
			{
				if (a.Kind == MemberKind.InternalCompilerType)
				{
					return a.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
				}
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			if (a.IsArray)
			{
				ArrayContainer arrayContainer = (ArrayContainer)a;
				ArrayContainer arrayContainer2 = b as ArrayContainer;
				if (arrayContainer2 == null)
				{
					return false;
				}
				if (arrayContainer.Rank == arrayContainer2.Rank)
				{
					return IsEqual(arrayContainer.Element, arrayContainer2.Element);
				}
				return false;
			}
			if (!a.IsGeneric || !b.IsGeneric)
			{
				if (a.BuiltinType == BuiltinTypeSpec.Type.Dynamic || b.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					if (b.BuiltinType != BuiltinTypeSpec.Type.Object)
					{
						return a.BuiltinType == BuiltinTypeSpec.Type.Object;
					}
					return true;
				}
				return false;
			}
			if (a.MemberDefinition != b.MemberDefinition)
			{
				return false;
			}
			do
			{
				if (!Equals(a.TypeArguments, b.TypeArguments))
				{
					return false;
				}
				a = a.DeclaringType;
				b = b.DeclaringType;
			}
			while (a != null);
			return true;
		}
	}
}
