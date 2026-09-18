using System;
using System.Collections.Generic;
using System.Text;

namespace IKVM.Reflection
{
	public abstract class MethodInfo : MethodBase, IGenericContext, IGenericBinder
	{
		public sealed override MemberTypes MemberType => MemberTypes.Method;

		public abstract Type ReturnType { get; }

		public abstract ParameterInfo ReturnParameter { get; }

		internal bool IsNewSlot => (Attributes & MethodAttributes.VtableLayoutMask) != 0;

		internal virtual bool HasThis => !base.IsStatic;

		internal MethodInfo()
		{
		}

		public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			throw new NotSupportedException(GetType().FullName);
		}

		public virtual MethodInfo GetGenericMethodDefinition()
		{
			throw new NotSupportedException(GetType().FullName);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(ReturnType.Name).Append(' ').Append(Name);
			string value;
			if (IsGenericMethod)
			{
				stringBuilder.Append('[');
				value = "";
				Type[] genericArguments = GetGenericArguments();
				foreach (Type value2 in genericArguments)
				{
					stringBuilder.Append(value).Append(value2);
					value = ", ";
				}
				stringBuilder.Append(']');
			}
			stringBuilder.Append('(');
			value = "";
			ParameterInfo[] parameters = GetParameters();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				stringBuilder.Append(value).Append(parameterInfo.ParameterType);
				value = ", ";
			}
			stringBuilder.Append(')');
			return stringBuilder.ToString();
		}

		public MethodInfo GetBaseDefinition()
		{
			MethodInfo methodInfo = this;
			if (methodInfo.IsVirtual)
			{
				Type baseType = DeclaringType.BaseType;
				while (baseType != null && !methodInfo.IsNewSlot)
				{
					MethodInfo methodInfo2 = baseType.FindMethod(Name, MethodSignature) as MethodInfo;
					if (methodInfo2 != null && methodInfo2.IsVirtual)
					{
						methodInfo = methodInfo2;
					}
					baseType = baseType.BaseType;
				}
			}
			return methodInfo;
		}

		public virtual MethodInfo[] __GetMethodImpls()
		{
			throw new NotSupportedException();
		}

		public bool __TryGetImplMap(out ImplMapFlags mappingFlags, out string importName, out string importScope)
		{
			return Module.__TryGetImplMap(GetCurrentToken(), out mappingFlags, out importName, out importScope);
		}

		public ConstructorInfo __AsConstructorInfo()
		{
			return new ConstructorInfoImpl(this);
		}

		Type IGenericContext.GetGenericTypeArgument(int index)
		{
			return DeclaringType.GetGenericTypeArgument(index);
		}

		Type IGenericContext.GetGenericMethodArgument(int index)
		{
			return GetGenericMethodArgument(index);
		}

		internal virtual Type GetGenericMethodArgument(int index)
		{
			throw new InvalidOperationException();
		}

		internal virtual int GetGenericMethodArgumentCount()
		{
			throw new InvalidOperationException();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return this;
		}

		Type IGenericBinder.BindTypeParameter(Type type)
		{
			return DeclaringType.GetGenericTypeArgument(type.GenericParameterPosition);
		}

		Type IGenericBinder.BindMethodParameter(Type type)
		{
			return GetGenericMethodArgument(type.GenericParameterPosition);
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new GenericMethodInstance(DeclaringType.BindTypeParameters(type), this, null);
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new MethodInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			Module module = Module;
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			ImplMapFlags mappingFlags;
			string importName;
			string importScope;
			if ((Attributes & MethodAttributes.PinvokeImpl) != MethodAttributes.PrivateScope && (attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_DllImportAttribute)) && __TryGetImplMap(out mappingFlags, out importName, out importScope))
			{
				list.Add(CustomAttributeData.CreateDllImportPseudoCustomAttribute(module, mappingFlags, importName, importScope, GetMethodImplementationFlags()));
			}
			if ((GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != MethodImplAttributes.IL && (attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_PreserveSigAttribute)))
			{
				list.Add(CustomAttributeData.CreatePreserveSigPseudoCustomAttribute(module));
			}
			return list;
		}
	}
}
