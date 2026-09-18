using System;

namespace IKVM.Reflection
{
	internal sealed class DefaultBinder : Binder
	{
		public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
		{
			int num = 0;
			foreach (MethodBase methodBase in match)
			{
				if (MatchParameterTypes(methodBase.GetParameters(), types))
				{
					match[num++] = methodBase;
				}
			}
			if (num == 0)
			{
				return null;
			}
			MethodBase currentBest = match[0];
			bool ambiguous = false;
			for (int j = 1; j < num; j++)
			{
				SelectBestMatch(match[j], types, ref currentBest, ref ambiguous);
			}
			if (ambiguous)
			{
				throw new AmbiguousMatchException();
			}
			return currentBest;
		}

		private static bool MatchParameterTypes(ParameterInfo[] parameters, Type[] types)
		{
			if (parameters.Length != types.Length)
			{
				return false;
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				Type type = types[i];
				Type parameterType = parameters[i].ParameterType;
				if (type != parameterType && !parameterType.IsAssignableFrom(type) && !IsAllowedPrimitiveConversion(type, parameterType))
				{
					return false;
				}
			}
			return true;
		}

		private static void SelectBestMatch(MethodBase candidate, Type[] types, ref MethodBase currentBest, ref bool ambiguous)
		{
			switch (MatchSignatures(currentBest.MethodSignature, candidate.MethodSignature, types))
			{
			case 1:
				return;
			case 2:
				ambiguous = false;
				currentBest = candidate;
				return;
			}
			if (currentBest.MethodSignature.MatchParameterTypes(candidate.MethodSignature))
			{
				int inheritanceDepth = GetInheritanceDepth(currentBest.DeclaringType);
				int inheritanceDepth2 = GetInheritanceDepth(candidate.DeclaringType);
				if (inheritanceDepth > inheritanceDepth2)
				{
					return;
				}
				if (inheritanceDepth < inheritanceDepth2)
				{
					ambiguous = false;
					currentBest = candidate;
					return;
				}
			}
			ambiguous = true;
		}

		private static int GetInheritanceDepth(Type type)
		{
			int num = 0;
			while (type != null)
			{
				num++;
				type = type.BaseType;
			}
			return num;
		}

		private static int MatchSignatures(MethodSignature sig1, MethodSignature sig2, Type[] types)
		{
			for (int i = 0; i < sig1.GetParameterCount(); i++)
			{
				Type parameterType = sig1.GetParameterType(i);
				Type parameterType2 = sig2.GetParameterType(i);
				if (parameterType != parameterType2)
				{
					return MatchTypes(parameterType, parameterType2, types[i]);
				}
			}
			return 0;
		}

		private static int MatchSignatures(PropertySignature sig1, PropertySignature sig2, Type[] types)
		{
			for (int i = 0; i < sig1.ParameterCount; i++)
			{
				Type parameter = sig1.GetParameter(i);
				Type parameter2 = sig2.GetParameter(i);
				if (parameter != parameter2)
				{
					return MatchTypes(parameter, parameter2, types[i]);
				}
			}
			return 0;
		}

		private static int MatchTypes(Type type1, Type type2, Type type)
		{
			if (type1 == type)
			{
				return 1;
			}
			if (type2 == type)
			{
				return 2;
			}
			bool flag = type1.IsAssignableFrom(type2);
			if (flag != type2.IsAssignableFrom(type1))
			{
				if (!flag)
				{
					return 1;
				}
				return 2;
			}
			return 0;
		}

		private static bool IsAllowedPrimitiveConversion(Type source, Type target)
		{
			if (!source.IsPrimitive || !target.IsPrimitive)
			{
				return false;
			}
			TypeCode typeCode = Type.GetTypeCode(source);
			TypeCode typeCode2 = Type.GetTypeCode(target);
			switch (typeCode)
			{
			case TypeCode.Char:
				switch (typeCode2)
				{
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.Byte:
				switch (typeCode2)
				{
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.SByte:
				switch (typeCode2)
				{
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.UInt16:
				switch (typeCode2)
				{
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.Int16:
				switch (typeCode2)
				{
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.UInt32:
				switch (typeCode2)
				{
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.Int32:
				switch (typeCode2)
				{
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.Double:
					return true;
				default:
					return false;
				}
			case TypeCode.UInt64:
				if (typeCode2 == TypeCode.Single || typeCode2 == TypeCode.Double)
				{
					return true;
				}
				return false;
			case TypeCode.Int64:
				if (typeCode2 == TypeCode.Single || typeCode2 == TypeCode.Double)
				{
					return true;
				}
				return false;
			case TypeCode.Single:
				if (typeCode2 == TypeCode.Double)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
		{
			int num = 0;
			foreach (PropertyInfo propertyInfo in match)
			{
				if (indexes != null && !MatchParameterTypes(propertyInfo.GetIndexParameters(), indexes))
				{
					continue;
				}
				if (returnType != null)
				{
					if (propertyInfo.PropertyType.IsPrimitive)
					{
						if (!IsAllowedPrimitiveConversion(returnType, propertyInfo.PropertyType))
						{
							continue;
						}
					}
					else if (!propertyInfo.PropertyType.IsAssignableFrom(returnType))
					{
						continue;
					}
				}
				match[num++] = propertyInfo;
			}
			switch (num)
			{
			case 0:
				return null;
			case 1:
				return match[0];
			default:
			{
				PropertyInfo propertyInfo2 = match[0];
				bool flag = false;
				for (int j = 1; j < num; j++)
				{
					int num2 = MatchTypes(propertyInfo2.PropertyType, match[j].PropertyType, returnType);
					if (num2 == 0 && indexes != null)
					{
						num2 = MatchSignatures(propertyInfo2.PropertySignature, match[j].PropertySignature, indexes);
					}
					if (num2 == 0)
					{
						int inheritanceDepth = GetInheritanceDepth(propertyInfo2.DeclaringType);
						int inheritanceDepth2 = GetInheritanceDepth(match[j].DeclaringType);
						if (propertyInfo2.Name == match[j].Name && inheritanceDepth != inheritanceDepth2)
						{
							num2 = ((inheritanceDepth > inheritanceDepth2) ? 1 : 2);
						}
						else
						{
							flag = true;
						}
					}
					if (num2 == 2)
					{
						flag = false;
						propertyInfo2 = match[j];
					}
				}
				if (flag)
				{
					throw new AmbiguousMatchException();
				}
				return propertyInfo2;
			}
			}
		}
	}
}
