using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class TypeInferenceContext
	{
		protected enum BoundKind
		{
			Exact = 0,
			Lower = 1,
			Upper = 2
		}

		private struct BoundInfo : IEquatable<BoundInfo>
		{
			public readonly TypeSpec Type;

			public readonly BoundKind Kind;

			public BoundInfo(TypeSpec type, BoundKind kind)
			{
				Type = type;
				Kind = kind;
			}

			public override int GetHashCode()
			{
				return Type.GetHashCode();
			}

			public Expression GetTypeExpression()
			{
				return new TypeExpression(Type, Location.Null);
			}

			public bool Equals(BoundInfo other)
			{
				if (Type == other.Type)
				{
					return Kind == other.Kind;
				}
				return false;
			}
		}

		private readonly TypeSpec[] tp_args;

		private readonly TypeSpec[] fixed_types;

		private readonly List<BoundInfo>[] bounds;

		public TypeSpec[] InferredTypeArguments => fixed_types;

		public bool UnfixedVariableExists
		{
			get
			{
				TypeSpec[] array = fixed_types;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == null)
					{
						return true;
					}
				}
				return false;
			}
		}

		public TypeInferenceContext(TypeSpec[] typeArguments)
		{
			if (typeArguments.Length == 0)
			{
				throw new ArgumentException("Empty generic arguments");
			}
			fixed_types = new TypeSpec[typeArguments.Length];
			for (int i = 0; i < typeArguments.Length; i++)
			{
				if (typeArguments[i].IsGenericParameter)
				{
					if (bounds == null)
					{
						bounds = new List<BoundInfo>[typeArguments.Length];
						tp_args = new TypeSpec[typeArguments.Length];
					}
					tp_args[i] = typeArguments[i];
				}
				else
				{
					fixed_types[i] = typeArguments[i];
				}
			}
		}

		public TypeInferenceContext()
		{
			fixed_types = new TypeSpec[1];
			tp_args = new TypeSpec[1];
			tp_args[0] = InternalType.Arglist;
			bounds = new List<BoundInfo>[1];
		}

		public void AddCommonTypeBound(TypeSpec type)
		{
			AddToBounds(new BoundInfo(type, BoundKind.Lower), 0, false);
		}

		public void AddCommonTypeBoundAsync(TypeSpec type)
		{
			AddToBounds(new BoundInfo(type, BoundKind.Lower), 0, true);
		}

		private void AddToBounds(BoundInfo bound, int index, bool voidAllowed)
		{
			if ((bound.Type.Kind != MemberKind.Void || voidAllowed) && !bound.Type.IsPointer && !bound.Type.IsSpecialRuntimeType && bound.Type != InternalType.MethodGroup && bound.Type != InternalType.AnonymousMethod && bound.Type != InternalType.VarOutType)
			{
				List<BoundInfo> list = bounds[index];
				if (list == null)
				{
					list = new List<BoundInfo>(2);
					list.Add(bound);
					bounds[index] = list;
				}
				else if (!list.Contains(bound))
				{
					list.Add(bound);
				}
			}
		}

		private bool AllTypesAreFixed(TypeSpec[] types)
		{
			foreach (TypeSpec typeSpec in types)
			{
				if (typeSpec.IsGenericParameter)
				{
					if (!IsFixed(typeSpec))
					{
						return false;
					}
				}
				else if (typeSpec.IsGeneric && !AllTypesAreFixed(typeSpec.TypeArguments))
				{
					return false;
				}
			}
			return true;
		}

		public int ExactInference(TypeSpec u, TypeSpec v)
		{
			if (v.IsArray)
			{
				if (!u.IsArray)
				{
					return 0;
				}
				ArrayContainer arrayContainer = (ArrayContainer)u;
				ArrayContainer arrayContainer2 = (ArrayContainer)v;
				if (arrayContainer.Rank != arrayContainer2.Rank)
				{
					return 0;
				}
				return ExactInference(arrayContainer.Element, arrayContainer2.Element);
			}
			if (TypeManager.IsGenericType(v))
			{
				if (!TypeManager.IsGenericType(u) || v.MemberDefinition != u.MemberDefinition)
				{
					return 0;
				}
				TypeSpec[] typeArguments = TypeManager.GetTypeArguments(u);
				TypeSpec[] typeArguments2 = TypeManager.GetTypeArguments(v);
				if (typeArguments.Length != typeArguments2.Length)
				{
					return 0;
				}
				int num = 0;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					num += ExactInference(typeArguments[i], typeArguments2[i]);
				}
				return System.Math.Min(1, num);
			}
			int num2 = IsUnfixed(v);
			if (num2 == -1)
			{
				return 0;
			}
			AddToBounds(new BoundInfo(u, BoundKind.Exact), num2, false);
			return 1;
		}

		public bool FixAllTypes(ResolveContext ec)
		{
			for (int i = 0; i < tp_args.Length; i++)
			{
				if (!FixType(ec, i))
				{
					return false;
				}
			}
			return true;
		}

		public bool FixDependentTypes(ResolveContext ec, ref bool fixed_any)
		{
			for (int i = 0; i < tp_args.Length; i++)
			{
				if (fixed_types[i] == null && bounds[i] != null)
				{
					if (!FixType(ec, i))
					{
						return false;
					}
					fixed_any = true;
				}
			}
			return true;
		}

		public bool FixIndependentTypeArguments(ResolveContext ec, TypeSpec[] methodParameters, ref bool fixed_any)
		{
			List<TypeSpec> list = new List<TypeSpec>(tp_args);
			for (int i = 0; i < methodParameters.Length; i++)
			{
				TypeSpec typeSpec = methodParameters[i];
				if (!typeSpec.IsDelegate)
				{
					if (!typeSpec.IsExpressionTreeType)
					{
						continue;
					}
					typeSpec = TypeManager.GetTypeArguments(typeSpec)[0];
				}
				if (!typeSpec.IsGenericParameter)
				{
					TypeSpec typeSpec2 = Delegate.GetInvokeMethod(typeSpec).ReturnType;
					while (typeSpec2.IsArray)
					{
						typeSpec2 = ((ArrayContainer)typeSpec2).Element;
					}
					if (typeSpec2.IsGenericParameter || TypeManager.IsGenericType(typeSpec2))
					{
						RemoveDependentTypes(list, typeSpec2);
					}
				}
			}
			foreach (TypeSpec item in list)
			{
				if (item != null)
				{
					int num = IsUnfixed(item);
					if (num >= 0 && !FixType(ec, num))
					{
						return false;
					}
				}
			}
			fixed_any = list.Count > 0;
			return true;
		}

		public bool FixType(ResolveContext ec, int i)
		{
			if (fixed_types[i] != null)
			{
				throw new InternalErrorException("Type argument has been already fixed");
			}
			List<BoundInfo> list = bounds[i];
			if (list == null)
			{
				return false;
			}
			if (list.Count == 1)
			{
				TypeSpec type = list[0].Type;
				if (type == InternalType.NullLiteral)
				{
					return false;
				}
				fixed_types[i] = type;
				return true;
			}
			bool[] array = new bool[list.Count];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = true;
			}
			for (int k = 0; k < array.Length; k++)
			{
				BoundInfo boundInfo = list[k];
				int l = 0;
				switch (boundInfo.Kind)
				{
				case BoundKind.Exact:
					for (; l != array.Length; l++)
					{
						if (k != l && array[l] && list[l].Type != boundInfo.Type)
						{
							array[l] = false;
						}
					}
					continue;
				case BoundKind.Lower:
					for (; l != array.Length; l++)
					{
						if (k != l && array[l] && !Convert.ImplicitConversionExists(ec, boundInfo.GetTypeExpression(), list[l].Type))
						{
							array[l] = false;
						}
					}
					continue;
				case BoundKind.Upper:
					break;
				default:
					continue;
				}
				for (; l != array.Length; l++)
				{
					if (k != l && array[l] && !Convert.ImplicitConversionExists(ec, list[l].GetTypeExpression(), boundInfo.Type))
					{
						array[l] = false;
					}
				}
			}
			TypeSpec typeSpec = null;
			for (int m = 0; m < array.Length; m++)
			{
				if (!array[m])
				{
					continue;
				}
				BoundInfo boundInfo2 = list[m];
				if (boundInfo2.Type == typeSpec)
				{
					continue;
				}
				int n;
				for (n = 0; n < array.Length && (m == n || !array[n] || Convert.ImplicitConversionExists(ec, list[n].GetTypeExpression(), boundInfo2.Type)); n++)
				{
				}
				if (n != array.Length)
				{
					continue;
				}
				if (typeSpec != null)
				{
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						continue;
					}
					if (boundInfo2.Type.BuiltinType != BuiltinTypeSpec.Type.Dynamic && typeSpec != boundInfo2.Type)
					{
						return false;
					}
				}
				typeSpec = boundInfo2.Type;
			}
			if (typeSpec == null)
			{
				return false;
			}
			fixed_types[i] = typeSpec;
			return true;
		}

		public bool HasBounds(int pos)
		{
			return bounds[pos] != null;
		}

		public TypeSpec InflateGenericArgument(IModuleContext context, TypeSpec parameter)
		{
			TypeParameterSpec typeParameterSpec = parameter as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				if (!typeParameterSpec.IsMethodOwned)
				{
					return parameter;
				}
				if (typeParameterSpec.DeclaredPosition < tp_args.Length && tp_args[typeParameterSpec.DeclaredPosition] == parameter)
				{
					return fixed_types[typeParameterSpec.DeclaredPosition] ?? parameter;
				}
				return parameter;
			}
			InflatedTypeSpec inflatedTypeSpec = parameter as InflatedTypeSpec;
			if (inflatedTypeSpec != null)
			{
				TypeSpec[] array = new TypeSpec[inflatedTypeSpec.TypeArguments.Length];
				for (int i = 0; i < array.Length; i++)
				{
					TypeSpec typeSpec = InflateGenericArgument(context, inflatedTypeSpec.TypeArguments[i]);
					if (typeSpec == null)
					{
						return null;
					}
					array[i] = typeSpec;
				}
				return inflatedTypeSpec.GetDefinition().MakeGenericType(context, array);
			}
			ArrayContainer arrayContainer = parameter as ArrayContainer;
			if (arrayContainer != null)
			{
				TypeSpec typeSpec2 = InflateGenericArgument(context, arrayContainer.Element);
				if (typeSpec2 != arrayContainer.Element)
				{
					return ArrayContainer.MakeType(context.Module, typeSpec2);
				}
			}
			return parameter;
		}

		public bool IsReturnTypeNonDependent(MethodSpec invoke, TypeSpec returnType)
		{
			AParametersCollection parameters = invoke.Parameters;
			if (parameters.IsEmpty)
			{
				return true;
			}
			while (returnType.IsArray)
			{
				returnType = ((ArrayContainer)returnType).Element;
			}
			if (returnType.IsGenericParameter)
			{
				if (IsFixed(returnType))
				{
					return false;
				}
			}
			else
			{
				if (!TypeManager.IsGenericType(returnType))
				{
					return false;
				}
				TypeSpec[] typeArguments = TypeManager.GetTypeArguments(returnType);
				if (AllTypesAreFixed(typeArguments))
				{
					return false;
				}
			}
			return AllTypesAreFixed(parameters.Types);
		}

		private bool IsFixed(TypeSpec type)
		{
			return IsUnfixed(type) == -1;
		}

		private int IsUnfixed(TypeSpec type)
		{
			if (!type.IsGenericParameter)
			{
				return -1;
			}
			for (int i = 0; i < tp_args.Length; i++)
			{
				if (tp_args[i] == type)
				{
					if (fixed_types[i] != null)
					{
						break;
					}
					return i;
				}
			}
			return -1;
		}

		public int LowerBoundInference(TypeSpec u, TypeSpec v)
		{
			return LowerBoundInference(u, v, false);
		}

		private int LowerBoundInference(TypeSpec u, TypeSpec v, bool inversed)
		{
			int num = IsUnfixed(v);
			if (num != -1)
			{
				AddToBounds(new BoundInfo(u, (!inversed) ? BoundKind.Lower : BoundKind.Upper), num, false);
				return 1;
			}
			ArrayContainer arrayContainer = u as ArrayContainer;
			if (arrayContainer != null)
			{
				ArrayContainer arrayContainer2 = v as ArrayContainer;
				if (arrayContainer2 != null)
				{
					if (arrayContainer.Rank != arrayContainer2.Rank)
					{
						return 0;
					}
					if (TypeSpec.IsValueType(arrayContainer.Element))
					{
						return ExactInference(arrayContainer.Element, arrayContainer2.Element);
					}
					return LowerBoundInference(arrayContainer.Element, arrayContainer2.Element, inversed);
				}
				if (arrayContainer.Rank != 1 || !v.IsArrayGenericInterface)
				{
					return 0;
				}
				TypeSpec v2 = TypeManager.GetTypeArguments(v)[0];
				if (TypeSpec.IsValueType(arrayContainer.Element))
				{
					return ExactInference(arrayContainer.Element, v2);
				}
				return LowerBoundInference(arrayContainer.Element, v2);
			}
			if (v.IsGenericOrParentIsGeneric)
			{
				List<TypeSpec> list = new List<TypeSpec>();
				ITypeDefinition memberDefinition = v.MemberDefinition;
				for (TypeSpec typeSpec = u; typeSpec != null; typeSpec = typeSpec.BaseType)
				{
					if (memberDefinition == typeSpec.MemberDefinition)
					{
						list.Add(typeSpec);
					}
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						list.Add(typeSpec);
					}
				}
				if (u.Interfaces != null)
				{
					foreach (TypeSpec @interface in u.Interfaces)
					{
						if (memberDefinition == @interface.MemberDefinition)
						{
							list.Add(@interface);
						}
					}
				}
				TypeSpec[] array = null;
				TypeSpec[] allTypeArguments = TypeSpec.GetAllTypeArguments(v);
				foreach (TypeSpec item in list)
				{
					if (array != null)
					{
						TypeSpec[] allTypeArguments2 = TypeSpec.GetAllTypeArguments(item);
						if (!TypeSpecComparer.Equals(array, allTypeArguments2))
						{
							return 0;
						}
						array = allTypeArguments2;
					}
					else if (item.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						array = new TypeSpec[allTypeArguments.Length];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = item;
						}
					}
					else
					{
						array = TypeSpec.GetAllTypeArguments(item);
					}
				}
				if (array != null)
				{
					int num2 = 0;
					int num3 = -1;
					TypeParameterSpec[] array2 = null;
					for (int j = 0; j < array.Length; j++)
					{
						if (num3 < 0)
						{
							while (v.Arity == 0)
							{
								v = v.DeclaringType;
							}
							array2 = v.MemberDefinition.TypeParameters;
							num3 = array2.Length - 1;
						}
						Variance variance = array2[num3--].Variance;
						TypeSpec typeSpec2 = array[j];
						if (variance == Variance.None || TypeSpec.IsValueType(typeSpec2))
						{
							if (ExactInference(typeSpec2, allTypeArguments[j]) == 0)
							{
								num2++;
							}
							continue;
						}
						bool inversed2 = (variance == Variance.Contravariant && !inversed) || ((variance == Variance.Covariant) & inversed);
						if (LowerBoundInference(typeSpec2, allTypeArguments[j], inversed2) == 0)
						{
							num2++;
						}
					}
					return num2;
				}
			}
			return 0;
		}

		public int OutputTypeInference(ResolveContext ec, Expression e, TypeSpec t)
		{
			AnonymousMethodExpression anonymousMethodExpression = e as AnonymousMethodExpression;
			if (anonymousMethodExpression != null)
			{
				TypeSpec typeSpec = anonymousMethodExpression.InferReturnType(ec, this, t);
				MethodSpec invokeMethod = Delegate.GetInvokeMethod(t);
				if (typeSpec == null)
				{
					AParametersCollection parameters = invokeMethod.Parameters;
					if (anonymousMethodExpression.Parameters.Count != parameters.Count)
					{
						return 0;
					}
					return 1;
				}
				TypeSpec returnType = invokeMethod.ReturnType;
				return LowerBoundInference(typeSpec, returnType) + 1;
			}
			if (e is MethodGroupExpr)
			{
				if (!t.IsDelegate)
				{
					if (!t.IsExpressionTreeType)
					{
						return 0;
					}
					t = TypeManager.GetTypeArguments(t)[0];
				}
				MethodSpec invokeMethod2 = Delegate.GetInvokeMethod(t);
				TypeSpec returnType2 = invokeMethod2.ReturnType;
				if (!IsReturnTypeNonDependent(invokeMethod2, returnType2))
				{
					return 0;
				}
				TypeSpec[] array = new TypeSpec[invokeMethod2.Parameters.Count];
				for (int i = 0; i < array.Length; i++)
				{
					TypeSpec typeSpec2 = InflateGenericArgument(ec, invokeMethod2.Parameters.Types[i]);
					if (typeSpec2 == null)
					{
						return 0;
					}
					array[i] = typeSpec2;
				}
				MethodGroupExpr methodGroupExpr = (MethodGroupExpr)e;
				Arguments args = DelegateCreation.CreateDelegateMethodArguments(ec, invokeMethod2.Parameters, array, e.Location);
				methodGroupExpr = methodGroupExpr.OverloadResolve(ec, ref args, null, OverloadResolver.Restrictions.ProbingOnly | OverloadResolver.Restrictions.CovariantDelegate);
				if (methodGroupExpr == null)
				{
					return 0;
				}
				return LowerBoundInference(methodGroupExpr.BestCandidateReturnType, returnType2) + 1;
			}
			return LowerBoundInference(e.Type, t) * 2;
		}

		private void RemoveDependentTypes(List<TypeSpec> types, TypeSpec returnType)
		{
			int num = IsUnfixed(returnType);
			if (num >= 0)
			{
				types[num] = null;
			}
			else if (TypeManager.IsGenericType(returnType))
			{
				TypeSpec[] typeArguments = TypeManager.GetTypeArguments(returnType);
				foreach (TypeSpec returnType2 in typeArguments)
				{
					RemoveDependentTypes(types, returnType2);
				}
			}
		}
	}
}
