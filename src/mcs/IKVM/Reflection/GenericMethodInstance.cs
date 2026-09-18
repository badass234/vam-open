using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	internal sealed class GenericMethodInstance : MethodInfo
	{
		private readonly Type declaringType;

		private readonly MethodInfo method;

		private readonly Type[] methodArgs;

		private MethodSignature lazyMethodSignature;

		public override Type ReturnType => method.ReturnType.BindTypeParameters(this);

		public override ParameterInfo ReturnParameter => new GenericParameterInfoImpl(this, method.ReturnParameter);

		internal override int ParameterCount => method.ParameterCount;

		public override CallingConventions CallingConvention => method.CallingConvention;

		public override MethodAttributes Attributes => method.Attributes;

		public override string Name => method.Name;

		public override Type DeclaringType
		{
			get
			{
				if (!declaringType.IsModulePseudoType)
				{
					return declaringType;
				}
				return null;
			}
		}

		public override Module Module => method.Module;

		public override int MetadataToken => method.MetadataToken;

		public override int __MethodRVA => method.__MethodRVA;

		public override bool IsGenericMethod => method.IsGenericMethod;

		public override bool IsGenericMethodDefinition
		{
			get
			{
				if (method.IsGenericMethodDefinition)
				{
					return methodArgs == null;
				}
				return false;
			}
		}

		public override bool ContainsGenericParameters
		{
			get
			{
				if (declaringType.ContainsGenericParameters)
				{
					return true;
				}
				if (methodArgs != null)
				{
					Type[] array = methodArgs;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].ContainsGenericParameters)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal override MethodSignature MethodSignature => lazyMethodSignature ?? (lazyMethodSignature = method.MethodSignature.Bind(declaringType, methodArgs));

		internal override bool HasThis => method.HasThis;

		internal override bool IsBaked => method.IsBaked;

		internal GenericMethodInstance(Type declaringType, MethodInfo method, Type[] methodArgs)
		{
			this.declaringType = declaringType;
			this.method = method;
			this.methodArgs = methodArgs;
		}

		public override bool Equals(object obj)
		{
			GenericMethodInstance genericMethodInstance = obj as GenericMethodInstance;
			if (genericMethodInstance != null && genericMethodInstance.method.Equals(method) && genericMethodInstance.declaringType.Equals(declaringType))
			{
				return Util.ArrayEquals(genericMethodInstance.methodArgs, methodArgs);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (declaringType.GetHashCode() * 33) ^ method.GetHashCode() ^ Util.GetHashCode(methodArgs);
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new GenericParameterInfoImpl(this, parameters[i]);
			}
			return parameters;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return method.GetMethodImplementationFlags();
		}

		public override MethodBody GetMethodBody()
		{
			MethodDefImpl methodDefImpl = method as MethodDefImpl;
			if (methodDefImpl != null)
			{
				return methodDefImpl.GetMethodBody(this);
			}
			throw new NotSupportedException();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(declaringType, method, typeArguments);
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (IsGenericMethod)
			{
				if (IsGenericMethodDefinition)
				{
					return this;
				}
				if (declaringType.IsConstructedGenericType)
				{
					return new GenericMethodInstance(declaringType, method, null);
				}
				return method;
			}
			throw new InvalidOperationException();
		}

		public override MethodBase __GetMethodOnTypeDefinition()
		{
			return method;
		}

		public override Type[] GetGenericArguments()
		{
			if (methodArgs == null)
			{
				return method.GetGenericArguments();
			}
			return (Type[])methodArgs.Clone();
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			if (methodArgs == null)
			{
				return method.GetGenericMethodArgument(index);
			}
			return methodArgs[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return method.GetGenericMethodArgumentCount();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return method.GetMethodOnTypeDefinition();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			if (methodArgs == null)
			{
				return module.ImportMethodOrField(declaringType, method.Name, method.MethodSignature);
			}
			return module.ImportMethodSpec(declaringType, method, methodArgs);
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new GenericMethodInstance(declaringType.BindTypeParameters(type), method, null);
		}

		public override MethodInfo[] __GetMethodImpls()
		{
			MethodInfo[] array = method.__GetMethodImpls();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (MethodInfo)array[i].BindTypeParameters(declaringType);
			}
			return array;
		}

		internal override int GetCurrentToken()
		{
			return method.GetCurrentToken();
		}
	}
}
