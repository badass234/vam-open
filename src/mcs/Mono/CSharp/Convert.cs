using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	internal static class Convert
	{
		[Flags]
		public enum UserConversionRestriction
		{
			None = 0,
			ImplicitOnly = 1,
			ProbingOnly = 2,
			NullableSourceOnly = 4
		}

		private static bool ArrayToIList(ArrayContainer array, TypeSpec list, bool isExplicit)
		{
			if (array.Rank != 1 || !list.IsArrayGenericInterface)
			{
				return false;
			}
			TypeSpec typeSpec = list.TypeArguments[0];
			if (array.Element == typeSpec)
			{
				return true;
			}
			if (typeSpec.IsGenericParameter)
			{
				return false;
			}
			if (isExplicit)
			{
				return ExplicitReferenceConversionExists(array.Element, typeSpec);
			}
			return ImplicitReferenceConversionExists(array.Element, typeSpec);
		}

		private static bool IList_To_Array(TypeSpec list, ArrayContainer array)
		{
			if (array.Rank != 1 || !list.IsArrayGenericInterface)
			{
				return false;
			}
			TypeSpec typeSpec = list.TypeArguments[0];
			if (array.Element == typeSpec)
			{
				return true;
			}
			if (!ImplicitReferenceConversionExists(array.Element, typeSpec))
			{
				return ExplicitReferenceConversionExists(array.Element, typeSpec);
			}
			return true;
		}

		public static Expression ImplicitTypeParameterConversion(Expression expr, TypeParameterSpec expr_type, TypeSpec target_type)
		{
			if (target_type.IsGenericParameter)
			{
				if (expr_type.TypeArguments != null && expr_type.HasDependencyOn(target_type))
				{
					if (expr == null)
					{
						return EmptyExpression.Null;
					}
					if (expr_type.IsReferenceType && !((TypeParameterSpec)target_type).IsReferenceType)
					{
						return new BoxedCast(expr, target_type);
					}
					return new ClassCast(expr, target_type);
				}
				return null;
			}
			if (target_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				if (expr == null)
				{
					return EmptyExpression.Null;
				}
				if (expr_type.IsReferenceType)
				{
					return new ClassCast(expr, target_type);
				}
				return new BoxedCast(expr, target_type);
			}
			TypeSpec effectiveBase = expr_type.GetEffectiveBase();
			if (effectiveBase == target_type || TypeSpec.IsBaseClass(effectiveBase, target_type, false) || effectiveBase.ImplementsInterface(target_type, true))
			{
				if (expr == null)
				{
					return EmptyExpression.Null;
				}
				if (expr_type.IsReferenceType)
				{
					return new ClassCast(expr, target_type);
				}
				return new BoxedCast(expr, target_type);
			}
			if (target_type.IsInterface && expr_type.IsConvertibleToInterface(target_type))
			{
				if (expr == null)
				{
					return EmptyExpression.Null;
				}
				if (expr_type.IsReferenceType)
				{
					return new ClassCast(expr, target_type);
				}
				return new BoxedCast(expr, target_type);
			}
			return null;
		}

		private static Expression ExplicitTypeParameterConversionFromT(Expression source, TypeSpec source_type, TypeSpec target_type)
		{
			TypeParameterSpec typeParameterSpec = target_type as TypeParameterSpec;
			if (typeParameterSpec != null && typeParameterSpec.TypeArguments != null && typeParameterSpec.HasDependencyOn(source_type))
			{
				if (source != null)
				{
					return new ClassCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (target_type.IsInterface)
			{
				if (source != null)
				{
					return new ClassCast(source, target_type, true);
				}
				return EmptyExpression.Null;
			}
			return null;
		}

		private static Expression ExplicitTypeParameterConversionToT(Expression source, TypeSpec source_type, TypeParameterSpec target_type)
		{
			TypeSpec effectiveBase = target_type.GetEffectiveBase();
			if (TypeSpecComparer.IsEqual(effectiveBase, source_type) || TypeSpec.IsBaseClass(effectiveBase, source_type, false))
			{
				if (source != null)
				{
					return new ClassCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			return null;
		}

		public static Expression ImplicitReferenceConversion(Expression expr, TypeSpec target_type, bool explicit_cast)
		{
			TypeSpec type = expr.Type;
			if (type.Kind == MemberKind.TypeParameter)
			{
				return ImplicitTypeParameterConversion(expr, (TypeParameterSpec)expr.Type, target_type);
			}
			NullLiteral nullLiteral = expr as NullLiteral;
			if (nullLiteral != null)
			{
				return nullLiteral.ConvertImplicitly(target_type);
			}
			if (ImplicitReferenceConversionExists(type, target_type))
			{
				if (!explicit_cast)
				{
					return expr;
				}
				return EmptyCast.Create(expr, target_type);
			}
			return null;
		}

		public static bool ImplicitReferenceConversionExists(TypeSpec expr_type, TypeSpec target_type)
		{
			return ImplicitReferenceConversionExists(expr_type, target_type, true);
		}

		public static bool ImplicitReferenceConversionExists(TypeSpec expr_type, TypeSpec target_type, bool refOnlyTypeParameter)
		{
			if (target_type.IsStruct)
			{
				return false;
			}
			switch (expr_type.Kind)
			{
			case MemberKind.TypeParameter:
				if (ImplicitTypeParameterConversion(null, (TypeParameterSpec)expr_type, target_type) != null)
				{
					if (refOnlyTypeParameter)
					{
						return TypeSpec.IsReferenceType(expr_type);
					}
					return true;
				}
				return false;
			case MemberKind.Class:
				if (target_type.BuiltinType == BuiltinTypeSpec.Type.Object || target_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					return true;
				}
				if (target_type.IsClass)
				{
					if (TypeSpecComparer.IsEqual(expr_type, target_type))
					{
						return true;
					}
					return TypeSpec.IsBaseClass(expr_type, target_type, true);
				}
				if (target_type.IsInterface)
				{
					return expr_type.ImplementsInterface(target_type, true);
				}
				return false;
			case MemberKind.ArrayType:
			{
				if (expr_type == target_type)
				{
					return true;
				}
				BuiltinTypeSpec.Type builtinType = target_type.BuiltinType;
				if (builtinType == BuiltinTypeSpec.Type.Object || builtinType == BuiltinTypeSpec.Type.Dynamic || builtinType == BuiltinTypeSpec.Type.Array)
				{
					return true;
				}
				ArrayContainer arrayContainer = (ArrayContainer)expr_type;
				ArrayContainer arrayContainer2 = target_type as ArrayContainer;
				if (arrayContainer2 != null && arrayContainer.Rank == arrayContainer2.Rank)
				{
					TypeSpec element = arrayContainer.Element;
					if (!TypeSpec.IsReferenceType(element))
					{
						return false;
					}
					return ImplicitReferenceConversionExists(element, arrayContainer2.Element);
				}
				if (target_type.IsInterface)
				{
					if (expr_type.ImplementsInterface(target_type, false))
					{
						return true;
					}
					if (ArrayToIList(arrayContainer, target_type, false))
					{
						return true;
					}
				}
				return false;
			}
			case MemberKind.Delegate:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Object:
				case BuiltinTypeSpec.Type.Dynamic:
				case BuiltinTypeSpec.Type.Delegate:
				case BuiltinTypeSpec.Type.MulticastDelegate:
					return true;
				default:
					if (TypeSpecComparer.IsEqual(expr_type, target_type))
					{
						return true;
					}
					if (!expr_type.ImplementsInterface(target_type, false))
					{
						return TypeSpecComparer.Variant.IsEqual(expr_type, target_type);
					}
					return true;
				}
			case MemberKind.Interface:
				if (TypeSpecComparer.IsEqual(expr_type, target_type))
				{
					return true;
				}
				if (target_type.IsInterface)
				{
					if (!TypeSpecComparer.Variant.IsEqual(expr_type, target_type))
					{
						return expr_type.ImplementsInterface(target_type, true);
					}
					return true;
				}
				if (target_type.BuiltinType != BuiltinTypeSpec.Type.Object)
				{
					return target_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
				}
				return true;
			case MemberKind.InternalCompilerType:
				if (expr_type == InternalType.NullLiteral)
				{
					if (target_type.Kind == MemberKind.InternalCompilerType)
					{
						return target_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
					}
					if (!TypeSpec.IsReferenceType(target_type))
					{
						return target_type.Kind == MemberKind.PointerType;
					}
					return true;
				}
				if (expr_type.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
				{
					break;
				}
				switch (target_type.Kind)
				{
				case MemberKind.Class:
				case MemberKind.Delegate:
				case MemberKind.Interface:
				case MemberKind.TypeParameter:
				case MemberKind.ArrayType:
					return true;
				default:
					if (target_type == InternalType.Arglist)
					{
						return true;
					}
					return false;
				}
			}
			return false;
		}

		public static Expression ImplicitBoxingConversion(Expression expr, TypeSpec expr_type, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Object:
			case BuiltinTypeSpec.Type.Dynamic:
			case BuiltinTypeSpec.Type.ValueType:
				if (!TypeSpec.IsValueType(expr_type))
				{
					return null;
				}
				if (expr != null)
				{
					return new BoxedCast(expr, target_type);
				}
				return EmptyExpression.Null;
			case BuiltinTypeSpec.Type.Enum:
				if (expr_type.IsEnum)
				{
					if (expr != null)
					{
						return new BoxedCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			}
			if (expr_type.IsNullableType)
			{
				if (!TypeSpec.IsReferenceType(target_type))
				{
					return null;
				}
				Expression expression = ImplicitBoxingConversion(expr, NullableInfo.GetUnderlyingType(expr_type), target_type);
				if (expression != null && expr != null)
				{
					expression = new UnboxCast(expression, target_type);
				}
				return expression;
			}
			if (target_type.IsInterface && TypeSpec.IsValueType(expr_type) && expr_type.ImplementsInterface(target_type, true))
			{
				if (expr != null)
				{
					return new BoxedCast(expr, target_type);
				}
				return EmptyExpression.Null;
			}
			return null;
		}

		public static Expression ImplicitNulableConversion(ResolveContext ec, Expression expr, TypeSpec target_type)
		{
			TypeSpec typeSpec = expr.Type;
			if (typeSpec == InternalType.NullLiteral)
			{
				if (ec != null)
				{
					return LiftedNull.Create(target_type, expr.Location);
				}
				return EmptyExpression.Null;
			}
			TypeSpec underlyingType = NullableInfo.GetUnderlyingType(target_type);
			if (typeSpec.IsNullableType)
			{
				typeSpec = NullableInfo.GetUnderlyingType(typeSpec);
			}
			if (ec == null)
			{
				if (TypeSpecComparer.IsEqual(typeSpec, underlyingType))
				{
					return EmptyExpression.Null;
				}
				if (expr is Constant)
				{
					return ((Constant)expr).ConvertImplicitly(underlyingType);
				}
				return ImplicitNumericConversion(null, typeSpec, underlyingType);
			}
			Expression expression = ((typeSpec == expr.Type) ? expr : Unwrap.Create(expr));
			Expression expression2 = expression;
			if (!TypeSpecComparer.IsEqual(typeSpec, underlyingType))
			{
				expression2 = ((!(expression2 is Constant)) ? ImplicitNumericConversion(expression2, typeSpec, underlyingType) : ((Constant)expression2).ConvertImplicitly(underlyingType));
				if (expression2 == null)
				{
					return null;
				}
			}
			if (typeSpec != expr.Type)
			{
				return new LiftedConversion(expression2, expression, target_type).Resolve(ec);
			}
			return Wrap.Create(expression2, target_type);
		}

		public static Expression ImplicitNumericConversion(Expression expr, TypeSpec target_type)
		{
			return ImplicitNumericConversion(expr, expr.Type, target_type);
		}

		public static bool ImplicitNumericConversionExists(TypeSpec expr_type, TypeSpec target_type)
		{
			return ImplicitNumericConversion(null, expr_type, target_type) != null;
		}

		private static Expression ImplicitNumericConversion(Expression expr, TypeSpec expr_type, TypeSpec target_type)
		{
			switch (expr_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.SByte:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Int:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Short:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I2);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Byte:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Short:
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.Int:
				case BuiltinTypeSpec.Type.UInt:
					if (expr != null)
					{
						return EmptyCast.Create(expr, target_type);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.ULong:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Short:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Int:
					if (expr != null)
					{
						return EmptyCast.Create(expr, target_type);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.UShort:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Int:
				case BuiltinTypeSpec.Type.UInt:
					if (expr != null)
					{
						return EmptyCast.Create(expr, target_type);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.ULong:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Int:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.UInt:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.ULong:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCastDuplex(expr, target_type, OpCodes.Conv_R_Un, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCastDuplex(expr, target_type, OpCodes.Conv_R_Un, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Long:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.ULong:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCastDuplex(expr, target_type, OpCodes.Conv_R_Un, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCastDuplex(expr, target_type, OpCodes.Conv_R_Un, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Char:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.Int:
				case BuiltinTypeSpec.Type.UInt:
					if (expr != null)
					{
						return EmptyCast.Create(expr, target_type);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.ULong:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Long:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_I8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Float:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Double:
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				case BuiltinTypeSpec.Type.Decimal:
					if (expr != null)
					{
						return new OperatorCast(expr, target_type);
					}
					return EmptyExpression.Null;
				}
				break;
			case BuiltinTypeSpec.Type.Float:
				if (target_type.BuiltinType == BuiltinTypeSpec.Type.Double)
				{
					if (expr != null)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
					return EmptyExpression.Null;
				}
				break;
			}
			return null;
		}

		public static bool ImplicitConversionExists(ResolveContext ec, Expression expr, TypeSpec target_type)
		{
			if (ImplicitStandardConversionExists(ec, expr, target_type))
			{
				return true;
			}
			if (expr.Type == InternalType.AnonymousMethod)
			{
				if (!target_type.IsDelegate && !target_type.IsExpressionTreeType)
				{
					return false;
				}
				return ((AnonymousMethodExpression)expr).ImplicitStandardConversionExists(ec, target_type);
			}
			if (expr.Type == InternalType.Arglist)
			{
				return target_type == ec.Module.PredefinedTypes.ArgIterator.TypeSpec;
			}
			return UserDefinedConversion(ec, expr, target_type, UserConversionRestriction.ImplicitOnly | UserConversionRestriction.ProbingOnly, Location.Null) != null;
		}

		public static bool ImplicitStandardConversionExists(ResolveContext rc, Expression expr, TypeSpec target_type)
		{
			if (expr.eclass == ExprClass.MethodGroup)
			{
				if (target_type.IsDelegate && rc.Module.Compiler.Settings.Version != LanguageVersion.ISO_1)
				{
					MethodGroupExpr methodGroupExpr = expr as MethodGroupExpr;
					if (methodGroupExpr != null)
					{
						return DelegateCreation.ImplicitStandardConversionExists(rc, methodGroupExpr, target_type);
					}
				}
				return false;
			}
			return ImplicitStandardConversionExists(expr, target_type);
		}

		public static bool ImplicitStandardConversionExists(Expression expr, TypeSpec target_type)
		{
			TypeSpec type = expr.Type;
			if (type == target_type)
			{
				return true;
			}
			if (target_type.IsNullableType)
			{
				return ImplicitNulableConversion(null, expr, target_type) != null;
			}
			if (ImplicitNumericConversion(null, type, target_type) != null)
			{
				return true;
			}
			if (ImplicitReferenceConversionExists(type, target_type, false))
			{
				return true;
			}
			if (ImplicitBoxingConversion(null, type, target_type) != null)
			{
				return true;
			}
			if (expr is IntConstant)
			{
				int value = ((IntConstant)expr).Value;
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					if (value >= -128 && value <= 127)
					{
						return true;
					}
					break;
				case BuiltinTypeSpec.Type.Byte:
					if (value >= 0 && value <= 255)
					{
						return true;
					}
					break;
				case BuiltinTypeSpec.Type.Short:
					if (value >= -32768 && value <= 32767)
					{
						return true;
					}
					break;
				case BuiltinTypeSpec.Type.UShort:
					if (value >= 0 && value <= 65535)
					{
						return true;
					}
					break;
				case BuiltinTypeSpec.Type.UInt:
					if (value >= 0)
					{
						return true;
					}
					break;
				case BuiltinTypeSpec.Type.ULong:
					if (value >= 0)
					{
						return true;
					}
					break;
				}
			}
			if (expr is LongConstant && target_type.BuiltinType == BuiltinTypeSpec.Type.ULong && ((LongConstant)expr).Value >= 0)
			{
				return true;
			}
			if (expr is IntegralConstant && target_type.IsEnum)
			{
				return ((IntegralConstant)expr).IsZeroInteger;
			}
			if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				MemberKind kind = target_type.Kind;
				if (kind == MemberKind.Struct || kind == MemberKind.Enum)
				{
					return true;
				}
				return false;
			}
			if (target_type.IsPointer && expr.Type.IsPointer && ((PointerContainer)target_type).Element.Kind == MemberKind.Void)
			{
				return true;
			}
			if (type.IsStruct && TypeSpecComparer.IsEqual(type, target_type))
			{
				return true;
			}
			return false;
		}

		public static TypeSpec FindMostEncompassedType(IList<TypeSpec> types)
		{
			TypeSpec typeSpec = null;
			EmptyExpression expr;
			foreach (TypeSpec type in types)
			{
				if (typeSpec == null)
				{
					typeSpec = type;
					continue;
				}
				expr = new EmptyExpression(type);
				if (ImplicitStandardConversionExists(expr, typeSpec))
				{
					typeSpec = type;
				}
			}
			expr = new EmptyExpression(typeSpec);
			foreach (TypeSpec type2 in types)
			{
				if (typeSpec != type2 && !ImplicitStandardConversionExists(expr, type2))
				{
					typeSpec = null;
					break;
				}
			}
			return typeSpec;
		}

		private static TypeSpec FindMostEncompassingType(IList<TypeSpec> types)
		{
			if (types.Count == 0)
			{
				return null;
			}
			if (types.Count == 1)
			{
				return types[0];
			}
			TypeSpec typeSpec = null;
			for (int i = 0; i < types.Count; i++)
			{
				int j;
				for (j = 0; j < types.Count; j++)
				{
					if (j != i && !ImplicitStandardConversionExists(new EmptyExpression(types[j]), types[i]))
					{
						j = 0;
						break;
					}
				}
				if (j != 0)
				{
					if (typeSpec != null)
					{
						return InternalType.FakeInternalType;
					}
					typeSpec = types[i];
				}
			}
			return typeSpec;
		}

		private static TypeSpec FindMostSpecificSource(ResolveContext rc, List<MethodSpec> list, TypeSpec sourceType, Expression source, bool apply_explicit_conv_rules)
		{
			TypeSpec[] array = null;
			for (int i = 0; i < list.Count; i++)
			{
				TypeSpec typeSpec = list[i].Parameters.Types[0];
				if (typeSpec == sourceType)
				{
					return typeSpec;
				}
				if (array == null)
				{
					array = new TypeSpec[list.Count];
				}
				array[i] = typeSpec;
			}
			if (apply_explicit_conv_rules)
			{
				List<TypeSpec> list2 = new List<TypeSpec>();
				TypeSpec[] array2 = array;
				foreach (TypeSpec typeSpec2 in array2)
				{
					if (ImplicitStandardConversionExists(rc, source, typeSpec2))
					{
						list2.Add(typeSpec2);
					}
				}
				if (list2.Count != 0)
				{
					if (source.eclass == ExprClass.MethodGroup)
					{
						return InternalType.FakeInternalType;
					}
					return FindMostEncompassedType(list2);
				}
			}
			if (apply_explicit_conv_rules)
			{
				return FindMostEncompassingType(array);
			}
			return FindMostEncompassedType(array);
		}

		public static TypeSpec FindMostSpecificTarget(IList<MethodSpec> list, TypeSpec target, bool apply_explicit_conv_rules)
		{
			List<TypeSpec> list2 = null;
			foreach (MethodSpec item in list)
			{
				TypeSpec returnType = item.ReturnType;
				if (returnType == target)
				{
					return returnType;
				}
				if (list2 == null)
				{
					list2 = new List<TypeSpec>(list.Count);
				}
				else if (list2.Contains(returnType))
				{
					continue;
				}
				list2.Add(returnType);
			}
			if (apply_explicit_conv_rules)
			{
				List<TypeSpec> list3 = new List<TypeSpec>();
				foreach (TypeSpec item2 in list2)
				{
					if (ImplicitStandardConversionExists(new EmptyExpression(item2), target))
					{
						list3.Add(item2);
					}
				}
				if (list3.Count != 0)
				{
					return FindMostEncompassingType(list3);
				}
			}
			if (apply_explicit_conv_rules)
			{
				return FindMostEncompassedType(list2);
			}
			return FindMostEncompassingType(list2);
		}

		public static Expression ImplicitUserConversion(ResolveContext ec, Expression source, TypeSpec target, Location loc)
		{
			return UserDefinedConversion(ec, source, target, UserConversionRestriction.ImplicitOnly, loc);
		}

		private static Expression ExplicitUserConversion(ResolveContext ec, Expression source, TypeSpec target, Location loc)
		{
			return UserDefinedConversion(ec, source, target, UserConversionRestriction.None, loc);
		}

		private static void FindApplicableUserDefinedConversionOperators(ResolveContext rc, IList<MemberSpec> operators, Expression source, TypeSpec target, UserConversionRestriction restr, ref List<MethodSpec> candidates)
		{
			if (source.Type.IsInterface)
			{
				return;
			}
			Expression expression = null;
			foreach (MethodSpec @operator in operators)
			{
				if (@operator == null)
				{
					continue;
				}
				TypeSpec typeSpec = @operator.Parameters.Types[0];
				if ((source.Type != typeSpec && !ImplicitStandardConversionExists(rc, source, typeSpec) && ((restr & UserConversionRestriction.ImplicitOnly) != UserConversionRestriction.None || !ImplicitStandardConversionExists(new EmptyExpression(typeSpec), source.Type))) || ((restr & UserConversionRestriction.NullableSourceOnly) != UserConversionRestriction.None && !typeSpec.IsNullableType))
				{
					continue;
				}
				typeSpec = @operator.ReturnType;
				if (typeSpec.IsInterface)
				{
					continue;
				}
				if (target != typeSpec)
				{
					if (typeSpec.IsNullableType)
					{
						typeSpec = NullableInfo.GetUnderlyingType(typeSpec);
					}
					if (!ImplicitStandardConversionExists(new EmptyExpression(typeSpec), target))
					{
						if ((restr & UserConversionRestriction.ImplicitOnly) != UserConversionRestriction.None)
						{
							continue;
						}
						if (expression == null)
						{
							expression = new EmptyExpression(target);
						}
						if (!ImplicitStandardConversionExists(expression, typeSpec))
						{
							continue;
						}
					}
				}
				if (candidates == null)
				{
					candidates = new List<MethodSpec>();
				}
				candidates.Add(@operator);
			}
		}

		public static Expression UserDefinedConversion(ResolveContext rc, Expression source, TypeSpec target, UserConversionRestriction restr, Location loc)
		{
			List<MethodSpec> candidates = null;
			TypeSpec type = source.Type;
			TypeSpec typeSpec = target;
			bool flag = false;
			bool flag2 = (restr & UserConversionRestriction.ImplicitOnly) != 0;
			Expression expression;
			if (type.IsNullableType)
			{
				if (flag2 && !TypeSpec.IsReferenceType(typeSpec) && !typeSpec.IsNullableType)
				{
					expression = source;
				}
				else
				{
					expression = Unwrap.CreateUnwrapped(source);
					type = expression.Type;
					flag = true;
				}
			}
			else
			{
				expression = source;
			}
			if (typeSpec.IsNullableType)
			{
				typeSpec = NullableInfo.GetUnderlyingType(typeSpec);
			}
			if ((type.Kind & (MemberKind.Class | MemberKind.Struct | MemberKind.TypeParameter)) != 0 && type.BuiltinType != BuiltinTypeSpec.Type.Decimal)
			{
				bool isStruct = type.IsStruct;
				IList<MemberSpec> userOperator = MemberCache.GetUserOperator(type, Operator.OpType.Implicit, isStruct);
				if (userOperator != null)
				{
					FindApplicableUserDefinedConversionOperators(rc, userOperator, expression, typeSpec, restr, ref candidates);
				}
				if (!flag2)
				{
					userOperator = MemberCache.GetUserOperator(type, Operator.OpType.Explicit, isStruct);
					if (userOperator != null)
					{
						FindApplicableUserDefinedConversionOperators(rc, userOperator, expression, typeSpec, restr, ref candidates);
					}
				}
			}
			if ((target.Kind & (MemberKind.Class | MemberKind.Struct | MemberKind.TypeParameter)) != 0 && typeSpec.BuiltinType != BuiltinTypeSpec.Type.Decimal)
			{
				bool declaredOnly = target.IsStruct | flag2;
				IList<MemberSpec> userOperator2 = MemberCache.GetUserOperator(typeSpec, Operator.OpType.Implicit, declaredOnly);
				if (userOperator2 != null)
				{
					FindApplicableUserDefinedConversionOperators(rc, userOperator2, expression, typeSpec, restr, ref candidates);
				}
				if (!flag2)
				{
					userOperator2 = MemberCache.GetUserOperator(typeSpec, Operator.OpType.Explicit, declaredOnly);
					if (userOperator2 != null)
					{
						FindApplicableUserDefinedConversionOperators(rc, userOperator2, expression, typeSpec, restr, ref candidates);
					}
				}
			}
			if (candidates == null)
			{
				return null;
			}
			TypeSpec typeSpec2;
			TypeSpec typeSpec3;
			MethodSpec methodSpec;
			if (candidates.Count == 1)
			{
				methodSpec = candidates[0];
				typeSpec2 = methodSpec.Parameters.Types[0];
				typeSpec3 = methodSpec.ReturnType;
			}
			else
			{
				typeSpec2 = FindMostSpecificSource(rc, candidates, source.Type, expression, !flag2);
				if (typeSpec2 == null)
				{
					return null;
				}
				typeSpec3 = FindMostSpecificTarget(candidates, target, !flag2);
				if (typeSpec3 == null)
				{
					return null;
				}
				methodSpec = null;
				for (int i = 0; i < candidates.Count; i++)
				{
					if (candidates[i].ReturnType == typeSpec3 && candidates[i].Parameters.Types[0] == typeSpec2)
					{
						methodSpec = candidates[i];
						break;
					}
				}
				if (methodSpec == null)
				{
					if ((restr & UserConversionRestriction.ProbingOnly) == 0)
					{
						MethodSpec methodSpec2 = candidates[0];
						methodSpec = candidates[1];
						rc.Report.Error(457, loc, "Ambiguous user defined operators `{0}' and `{1}' when converting from `{2}' to `{3}'", methodSpec2.GetSignatureForError(), methodSpec.GetSignatureForError(), source.Type.GetSignatureForError(), target.GetSignatureForError());
					}
					return ErrorExpression.Instance;
				}
			}
			if (typeSpec2 != type)
			{
				Constant constant = source as Constant;
				if (constant != null)
				{
					source = constant.Reduce(rc, typeSpec2);
					if (source == null)
					{
						constant = null;
					}
				}
				if (constant == null)
				{
					source = (flag2 ? ImplicitConversionStandard(rc, expression, typeSpec2, loc) : ExplicitConversionStandard(rc, expression, typeSpec2, loc));
				}
			}
			else
			{
				source = expression;
			}
			source = new UserCast(methodSpec, source, loc).Resolve(rc);
			if (typeSpec3 != typeSpec)
			{
				if (typeSpec3.IsNullableType && (target.IsNullableType || !flag2))
				{
					if (typeSpec3 != target)
					{
						Expression expression2 = Unwrap.CreateUnwrapped(source);
						source = (flag2 ? ImplicitConversionStandard(rc, expression2, typeSpec, loc) : ExplicitConversionStandard(rc, expression2, typeSpec, loc));
						if (source == null)
						{
							return null;
						}
						if (target.IsNullableType)
						{
							source = new LiftedConversion(source, expression2, target).Resolve(rc);
						}
					}
				}
				else
				{
					source = (flag2 ? ImplicitConversionStandard(rc, source, typeSpec, loc) : ExplicitConversionStandard(rc, source, typeSpec, loc));
					if (source == null)
					{
						return null;
					}
				}
			}
			if (flag && !typeSpec2.IsNullableType)
			{
				return new LiftedConversion(source, expression, target).Resolve(rc);
			}
			if (target.IsNullableType && !typeSpec3.IsNullableType)
			{
				source = Wrap.Create(source, target);
			}
			return source;
		}

		public static Expression ImplicitConversion(ResolveContext ec, Expression expr, TypeSpec target_type, Location loc)
		{
			if (target_type == null)
			{
				throw new Exception("Target type is null");
			}
			Expression expression = ImplicitConversionStandard(ec, expr, target_type, loc);
			if (expression != null)
			{
				return expression;
			}
			expression = ImplicitUserConversion(ec, expr, target_type, loc);
			if (expression != null)
			{
				return expression;
			}
			return null;
		}

		public static Expression ImplicitConversionStandard(ResolveContext ec, Expression expr, TypeSpec target_type, Location loc)
		{
			return ImplicitConversionStandard(ec, expr, target_type, loc, false);
		}

		private static Expression ImplicitConversionStandard(ResolveContext ec, Expression expr, TypeSpec target_type, Location loc, bool explicit_cast)
		{
			if (expr.eclass == ExprClass.MethodGroup)
			{
				if (!target_type.IsDelegate)
				{
					return null;
				}
				if (ec.Module.Compiler.Settings.Version != LanguageVersion.ISO_1)
				{
					MethodGroupExpr methodGroupExpr = expr as MethodGroupExpr;
					if (methodGroupExpr != null)
					{
						return new ImplicitDelegateCreation(target_type, methodGroupExpr, loc).Resolve(ec);
					}
				}
			}
			TypeSpec type = expr.Type;
			if (type == target_type)
			{
				if (type != InternalType.NullLiteral && type != InternalType.AnonymousMethod)
				{
					return expr;
				}
				return null;
			}
			if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				MemberKind kind = target_type.Kind;
				if (kind <= MemberKind.Delegate)
				{
					if (kind == MemberKind.Class)
					{
						goto IL_00d0;
					}
					if (kind == MemberKind.Struct || kind == MemberKind.Delegate)
					{
						goto IL_00e2;
					}
				}
				else if (kind <= MemberKind.Interface)
				{
					if (kind == MemberKind.Enum || kind == MemberKind.Interface)
					{
						goto IL_00e2;
					}
				}
				else
				{
					if (kind == MemberKind.TypeParameter)
					{
						goto IL_00e2;
					}
					if (kind == MemberKind.ArrayType)
					{
						goto IL_00d0;
					}
				}
				return null;
			}
			if (target_type.IsNullableType)
			{
				return ImplicitNulableConversion(ec, expr, target_type);
			}
			Constant constant = expr as Constant;
			if (constant != null)
			{
				try
				{
					constant = constant.ConvertImplicitly(target_type);
				}
				catch
				{
					throw new InternalErrorException("Conversion error", loc);
				}
				if (constant != null)
				{
					return constant;
				}
			}
			Expression expression = ImplicitNumericConversion(expr, type, target_type);
			if (expression != null)
			{
				return expression;
			}
			expression = ImplicitReferenceConversion(expr, target_type, explicit_cast);
			if (expression != null)
			{
				return expression;
			}
			expression = ImplicitBoxingConversion(expr, type, target_type);
			if (expression != null)
			{
				return expression;
			}
			if (expr is IntegralConstant && target_type.IsEnum)
			{
				IntegralConstant integralConstant = (IntegralConstant)expr;
				if (integralConstant.IsZeroInteger)
				{
					return new EnumConstant(new IntLiteral(ec.BuiltinTypes, 0, integralConstant.Location), target_type);
				}
			}
			PointerContainer pointerContainer = target_type as PointerContainer;
			if (pointerContainer != null)
			{
				if (type.IsPointer)
				{
					if (type == pointerContainer)
					{
						return expr;
					}
					if (pointerContainer.Element.Kind == MemberKind.Void)
					{
						return EmptyCast.Create(expr, target_type);
					}
				}
				if (type == InternalType.NullLiteral)
				{
					return new NullPointer(target_type, loc);
				}
			}
			if (type == InternalType.AnonymousMethod)
			{
				Expression expression2 = ((AnonymousMethodExpression)expr).Compatible(ec, target_type);
				if (expression2 != null)
				{
					return expression2.Resolve(ec);
				}
				return ErrorExpression.Instance;
			}
			if (type == InternalType.Arglist && target_type == ec.Module.PredefinedTypes.ArgIterator.TypeSpec)
			{
				return expr;
			}
			if (type.IsStruct && TypeSpecComparer.IsEqual(type, target_type))
			{
				if (type != target_type)
				{
					return EmptyCast.Create(expr, target_type);
				}
				return expr;
			}
			InterpolatedString interpolatedString = expr as InterpolatedString;
			if (interpolatedString != null && (target_type == ec.Module.PredefinedTypes.IFormattable.TypeSpec || target_type == ec.Module.PredefinedTypes.FormattableString.TypeSpec))
			{
				return interpolatedString.ConvertTo(ec, target_type);
			}
			return null;
			IL_00e2:
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(expr));
			return new DynamicConversion(target_type, explicit_cast ? CSharpBinderFlags.ConvertExplicit : CSharpBinderFlags.None, arguments, loc).Resolve(ec);
			IL_00d0:
			if (target_type.BuiltinType == BuiltinTypeSpec.Type.Object)
			{
				return EmptyCast.Create(expr, target_type);
			}
			goto IL_00e2;
		}

		public static Expression ImplicitConversionRequired(ResolveContext ec, Expression source, TypeSpec target_type, Location loc)
		{
			Expression expression = ImplicitConversion(ec, source, target_type, loc);
			if (expression != null)
			{
				return expression;
			}
			source.Error_ValueCannotBeConverted(ec, target_type, false);
			return null;
		}

		public static Expression ExplicitNumericConversion(ResolveContext rc, Expression expr, TypeSpec target_type)
		{
			switch (expr.Type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.SByte:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I1_U1);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.I1_U2);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.I1_U4);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.I1_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.I1_CH);
				case BuiltinTypeSpec.Type.UIntPtr:
					return new OperatorCast(new ConvCast(expr, rc.BuiltinTypes.ULong, ConvCast.Mode.I1_U8), target_type, target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.Byte:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U1_I1);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.U1_CH);
				}
				break;
			case BuiltinTypeSpec.Type.Short:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_U1);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_U2);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_U4);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.I2_CH);
				case BuiltinTypeSpec.Type.UIntPtr:
					return new OperatorCast(new ConvCast(expr, rc.BuiltinTypes.ULong, ConvCast.Mode.I2_U8), target_type, target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.UShort:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U2_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U2_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.U2_I2);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.U2_CH);
				}
				break;
			case BuiltinTypeSpec.Type.Int:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_U2);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_U4);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.I4_CH);
				case BuiltinTypeSpec.Type.UIntPtr:
					return new OperatorCast(new ConvCast(expr, rc.BuiltinTypes.ULong, ConvCast.Mode.I2_U8), target_type, target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.UInt:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_U2);
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_I4);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.U4_CH);
				}
				break;
			case BuiltinTypeSpec.Type.Long:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_U2);
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_I4);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_U4);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_CH);
				}
				break;
			case BuiltinTypeSpec.Type.ULong:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_U2);
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_I4);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_U4);
				case BuiltinTypeSpec.Type.Long:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_I8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_CH);
				case BuiltinTypeSpec.Type.IntPtr:
					return new OperatorCast(EmptyCast.Create(expr, rc.BuiltinTypes.Long), target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.Char:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.CH_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.CH_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.CH_I2);
				}
				break;
			case BuiltinTypeSpec.Type.Float:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_U2);
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_I4);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_U4);
				case BuiltinTypeSpec.Type.Long:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_I8);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.R4_CH);
				case BuiltinTypeSpec.Type.Decimal:
					return new OperatorCast(expr, target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.Double:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_U1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_U2);
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_I4);
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_U4);
				case BuiltinTypeSpec.Type.Long:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_I8);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_U8);
				case BuiltinTypeSpec.Type.Char:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_CH);
				case BuiltinTypeSpec.Type.Float:
					return new ConvCast(expr, target_type, ConvCast.Mode.R8_R4);
				case BuiltinTypeSpec.Type.Decimal:
					return new OperatorCast(expr, target_type, true);
				}
				break;
			case BuiltinTypeSpec.Type.UIntPtr:
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new ConvCast(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.UInt, true), target_type, ConvCast.Mode.U4_I1);
				case BuiltinTypeSpec.Type.Short:
					return new ConvCast(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.UInt, true), target_type, ConvCast.Mode.U4_I2);
				case BuiltinTypeSpec.Type.Int:
					return EmptyCast.Create(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.UInt, true), target_type);
				case BuiltinTypeSpec.Type.UInt:
					return new OperatorCast(expr, expr.Type, target_type, true);
				case BuiltinTypeSpec.Type.Long:
					return EmptyCast.Create(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.ULong, true), target_type);
				}
				break;
			case BuiltinTypeSpec.Type.IntPtr:
				if (target_type.BuiltinType == BuiltinTypeSpec.Type.UInt)
				{
					return EmptyCast.Create(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.Int, true), target_type);
				}
				if (target_type.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					return EmptyCast.Create(new OperatorCast(expr, expr.Type, rc.BuiltinTypes.Long, true), target_type);
				}
				break;
			case BuiltinTypeSpec.Type.Decimal:
				switch (target_type.BuiltinType)
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
				case BuiltinTypeSpec.Type.Float:
				case BuiltinTypeSpec.Type.Double:
					return new OperatorCast(expr, expr.Type, target_type, true);
				}
				break;
			}
			return null;
		}

		public static bool ExplicitReferenceConversionExists(TypeSpec source_type, TypeSpec target_type)
		{
			Expression expression = ExplicitReferenceConversion(null, source_type, target_type);
			if (expression == null)
			{
				return false;
			}
			if (expression == EmptyExpression.Null)
			{
				return true;
			}
			throw new InternalErrorException("Invalid probing conversion result");
		}

		private static Expression ExplicitReferenceConversion(Expression source, TypeSpec source_type, TypeSpec target_type)
		{
			if (source_type.BuiltinType == BuiltinTypeSpec.Type.Object && TypeManager.IsGenericParameter(target_type))
			{
				if (source != null)
				{
					return new UnboxCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.Kind == MemberKind.TypeParameter)
			{
				return ExplicitTypeParameterConversionFromT(source, source_type, target_type);
			}
			bool flag = target_type.Kind == MemberKind.Struct || target_type.Kind == MemberKind.Enum;
			if ((source_type.BuiltinType == BuiltinTypeSpec.Type.ValueType) & flag)
			{
				if (source != null)
				{
					return new UnboxCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.BuiltinType == BuiltinTypeSpec.Type.Object || source_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				if (target_type.IsPointer)
				{
					return null;
				}
				if (source != null)
				{
					if (!flag)
					{
						if (!(source is Constant))
						{
							return new ClassCast(source, target_type);
						}
						return new EmptyConstantCast((Constant)source, target_type);
					}
					return new UnboxCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.Kind == MemberKind.Class && TypeSpec.IsBaseClass(target_type, source_type, true))
			{
				if (source != null)
				{
					return new ClassCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.Kind == MemberKind.Interface)
			{
				if (!target_type.IsSealed || target_type.ImplementsInterface(source_type, true))
				{
					if (source == null)
					{
						return EmptyExpression.Null;
					}
					if (!flag)
					{
						return new ClassCast(source, target_type);
					}
					return new UnboxCast(source, target_type);
				}
				ArrayContainer arrayContainer = target_type as ArrayContainer;
				if (arrayContainer != null && IList_To_Array(source_type, arrayContainer))
				{
					if (source != null)
					{
						return new ClassCast(source, target_type);
					}
					return EmptyExpression.Null;
				}
				return null;
			}
			ArrayContainer arrayContainer2 = source_type as ArrayContainer;
			if (arrayContainer2 != null)
			{
				ArrayContainer arrayContainer3 = target_type as ArrayContainer;
				if (arrayContainer3 != null)
				{
					if (source_type.BuiltinType == BuiltinTypeSpec.Type.Array)
					{
						if (source != null)
						{
							return new ClassCast(source, target_type);
						}
						return EmptyExpression.Null;
					}
					if (arrayContainer2.Rank == arrayContainer3.Rank)
					{
						source_type = arrayContainer2.Element;
						TypeSpec element = arrayContainer3.Element;
						if ((source_type.Kind & element.Kind & MemberKind.TypeParameter) == MemberKind.TypeParameter)
						{
							if (TypeSpec.IsValueType(source_type))
							{
								return null;
							}
						}
						else if (!TypeSpec.IsReferenceType(source_type))
						{
							return null;
						}
						if (!TypeSpec.IsReferenceType(element))
						{
							return null;
						}
						if (ExplicitReferenceConversionExists(source_type, element))
						{
							if (source != null)
							{
								return new ClassCast(source, target_type);
							}
							return EmptyExpression.Null;
						}
						return null;
					}
				}
				if (ArrayToIList(arrayContainer2, target_type, true))
				{
					if (source != null)
					{
						return new ClassCast(source, target_type);
					}
					return EmptyExpression.Null;
				}
				return null;
			}
			if (target_type.IsInterface && !source_type.IsSealed && !source_type.ImplementsInterface(target_type, true))
			{
				if (source != null)
				{
					return new ClassCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.BuiltinType == BuiltinTypeSpec.Type.Delegate && target_type.IsDelegate)
			{
				if (source != null)
				{
					return new ClassCast(source, target_type);
				}
				return EmptyExpression.Null;
			}
			if (source_type.IsDelegate && target_type.IsDelegate && source_type.MemberDefinition == target_type.MemberDefinition)
			{
				TypeParameterSpec[] typeParameters = source_type.MemberDefinition.TypeParameters;
				TypeSpec[] typeArguments = source_type.TypeArguments;
				TypeSpec[] typeArguments2 = target_type.TypeArguments;
				int i;
				for (i = 0; i < typeParameters.Length; i++)
				{
					if (TypeSpecComparer.IsEqual(typeArguments[i], typeArguments2[i]))
					{
						continue;
					}
					if (typeParameters[i].Variance == Variance.Covariant)
					{
						if (!ImplicitReferenceConversionExists(typeArguments[i], typeArguments2[i]) && !ExplicitReferenceConversionExists(typeArguments[i], typeArguments2[i]))
						{
							break;
						}
					}
					else if (typeParameters[i].Variance != Variance.Contravariant || !TypeSpec.IsReferenceType(typeArguments[i]) || !TypeSpec.IsReferenceType(typeArguments2[i]))
					{
						break;
					}
				}
				if (i == typeParameters.Length)
				{
					if (source != null)
					{
						return new ClassCast(source, target_type);
					}
					return EmptyExpression.Null;
				}
			}
			TypeParameterSpec typeParameterSpec = target_type as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				return ExplicitTypeParameterConversionToT(source, source_type, typeParameterSpec);
			}
			return null;
		}

		public static Expression ExplicitConversionCore(ResolveContext ec, Expression expr, TypeSpec target_type, Location loc)
		{
			TypeSpec type = expr.Type;
			Expression expression = ImplicitConversionStandard(ec, expr, target_type, loc, true);
			if (expression != null)
			{
				return expression;
			}
			if (type.IsEnum)
			{
				TypeSpec typeSpec = (target_type.IsEnum ? EnumSpec.GetUnderlyingType(target_type) : target_type);
				Expression expression2 = EmptyCast.Create(expr, EnumSpec.GetUnderlyingType(type));
				if (expression2.Type == typeSpec)
				{
					expression = expression2;
				}
				if (expression == null)
				{
					expression = ImplicitNumericConversion(expression2, typeSpec);
				}
				if (expression == null)
				{
					expression = ExplicitNumericConversion(ec, expression2, typeSpec);
				}
				if (expression == null && (typeSpec.BuiltinType == BuiltinTypeSpec.Type.IntPtr || typeSpec.BuiltinType == BuiltinTypeSpec.Type.UIntPtr))
				{
					expression = ExplicitUserConversion(ec, expression2, typeSpec, loc);
				}
				if (expression == null)
				{
					return null;
				}
				return EmptyCast.Create(expression, target_type);
			}
			if (target_type.IsEnum)
			{
				if (type.BuiltinType == BuiltinTypeSpec.Type.Enum)
				{
					return new UnboxCast(expr, target_type);
				}
				TypeSpec typeSpec2 = (target_type.IsEnum ? EnumSpec.GetUnderlyingType(target_type) : target_type);
				if (type == typeSpec2)
				{
					return EmptyCast.Create(expr, target_type);
				}
				Constant constant = expr as Constant;
				if (constant != null)
				{
					constant = constant.TryReduce(ec, typeSpec2);
					if (constant != null)
					{
						return constant;
					}
				}
				else
				{
					expression = ImplicitNumericConversion(expr, typeSpec2);
					if (expression != null)
					{
						return EmptyCast.Create(expression, target_type);
					}
					expression = ExplicitNumericConversion(ec, expr, typeSpec2);
					if (expression != null)
					{
						return EmptyCast.Create(expression, target_type);
					}
					if (type.BuiltinType == BuiltinTypeSpec.Type.IntPtr || type.BuiltinType == BuiltinTypeSpec.Type.UIntPtr)
					{
						expression = ExplicitUserConversion(ec, expr, typeSpec2, loc);
						if (expression != null)
						{
							return ExplicitConversionCore(ec, expression, target_type, loc);
						}
					}
				}
			}
			else
			{
				expression = ExplicitNumericConversion(ec, expr, target_type);
				if (expression != null)
				{
					return expression;
				}
			}
			if (type != InternalType.NullLiteral)
			{
				expression = ExplicitReferenceConversion(expr, type, target_type);
				if (expression != null)
				{
					return expression;
				}
			}
			if (ec.IsUnsafe)
			{
				expression = ExplicitUnsafe(expr, target_type);
				if (expression != null)
				{
					return expression;
				}
			}
			return null;
		}

		public static Expression ExplicitUnsafe(Expression expr, TypeSpec target_type)
		{
			TypeSpec type = expr.Type;
			if (target_type.IsPointer)
			{
				if (type.IsPointer)
				{
					return EmptyCast.Create(expr, target_type);
				}
				switch (type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
				case BuiltinTypeSpec.Type.Short:
				case BuiltinTypeSpec.Type.Int:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_I);
				case BuiltinTypeSpec.Type.Byte:
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.UInt:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_U);
				case BuiltinTypeSpec.Type.Long:
					return new ConvCast(expr, target_type, ConvCast.Mode.I8_I);
				case BuiltinTypeSpec.Type.ULong:
					return new ConvCast(expr, target_type, ConvCast.Mode.U8_I);
				}
			}
			if (type.IsPointer)
			{
				switch (target_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.SByte:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_I1);
				case BuiltinTypeSpec.Type.Byte:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_U1);
				case BuiltinTypeSpec.Type.Short:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_I2);
				case BuiltinTypeSpec.Type.UShort:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_U2);
				case BuiltinTypeSpec.Type.Int:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_I4);
				case BuiltinTypeSpec.Type.UInt:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_U4);
				case BuiltinTypeSpec.Type.Long:
					return new ConvCast(expr, target_type, ConvCast.Mode.I_I8);
				case BuiltinTypeSpec.Type.ULong:
					return new OpcodeCast(expr, target_type, OpCodes.Conv_U8);
				}
			}
			return null;
		}

		public static Expression ExplicitConversionStandard(ResolveContext ec, Expression expr, TypeSpec target_type, Location l)
		{
			int errors = ec.Report.Errors;
			Expression expression = ImplicitConversionStandard(ec, expr, target_type, l);
			if (ec.Report.Errors > errors)
			{
				return null;
			}
			if (expression != null)
			{
				return expression;
			}
			expression = ExplicitNumericConversion(ec, expr, target_type);
			if (expression != null)
			{
				return expression;
			}
			expression = ExplicitReferenceConversion(expr, expr.Type, target_type);
			if (expression != null)
			{
				return expression;
			}
			if (ec.IsUnsafe && expr.Type.IsPointer && target_type.IsPointer && ((PointerContainer)expr.Type).Element.Kind == MemberKind.Void)
			{
				return EmptyCast.Create(expr, target_type);
			}
			expr.Error_ValueCannotBeConverted(ec, target_type, true);
			return null;
		}

		public static Expression ExplicitConversion(ResolveContext ec, Expression expr, TypeSpec target_type, Location loc)
		{
			Expression expression = ExplicitConversionCore(ec, expr, target_type, loc);
			if (expression != null)
			{
				if (expression == expr)
				{
					if (target_type.BuiltinType == BuiltinTypeSpec.Type.Float)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R4);
					}
					if (target_type.BuiltinType == BuiltinTypeSpec.Type.Double)
					{
						return new OpcodeCast(expr, target_type, OpCodes.Conv_R8);
					}
				}
				return expression;
			}
			TypeSpec type = expr.Type;
			if (target_type.IsNullableType)
			{
				TypeSpec underlyingType;
				if (type.IsNullableType)
				{
					underlyingType = NullableInfo.GetUnderlyingType(target_type);
					Expression expression2 = Unwrap.Create(expr);
					expression = ExplicitConversion(ec, expression2, underlyingType, expr.Location);
					if (expression == null)
					{
						return null;
					}
					return new LiftedConversion(expression, expression2, target_type).Resolve(ec);
				}
				if (type.BuiltinType == BuiltinTypeSpec.Type.Object)
				{
					return new UnboxCast(expr, target_type);
				}
				underlyingType = TypeManager.GetTypeArguments(target_type)[0];
				expression = ExplicitConversionCore(ec, expr, underlyingType, loc);
				if (expression != null)
				{
					if (!TypeSpec.IsReferenceType(expr.Type))
					{
						return Wrap.Create(expression, target_type);
					}
					return new UnboxCast(expr, target_type);
				}
			}
			else if (type.IsNullableType)
			{
				expression = ImplicitBoxingConversion(expr, NullableInfo.GetUnderlyingType(type), target_type);
				if (expression != null)
				{
					return expression;
				}
				expression = Unwrap.Create(expr, false);
				expression = ExplicitConversionCore(ec, expression, target_type, loc);
				if (expression != null)
				{
					return EmptyCast.Create(expression, target_type);
				}
			}
			expression = ExplicitUserConversion(ec, expr, target_type, loc);
			if (expression != null)
			{
				return expression;
			}
			expr.Error_ValueCannotBeConverted(ec, target_type, true);
			return null;
		}
	}
}
