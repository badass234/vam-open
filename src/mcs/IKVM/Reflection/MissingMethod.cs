using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class MissingMethod : MethodInfo
	{
		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MissingMethod method;

			private readonly int index;

			private ParameterInfo Forwarder
			{
				get
				{
					if (index != -1)
					{
						return method.Forwarder.GetParameters()[index];
					}
					return method.Forwarder.ReturnParameter;
				}
			}

			public override string Name => Forwarder.Name;

			public override Type ParameterType
			{
				get
				{
					if (index != -1)
					{
						return method.signature.GetParameterType(method, index);
					}
					return method.signature.GetReturnType(method);
				}
			}

			public override ParameterAttributes Attributes => Forwarder.Attributes;

			public override int Position => index;

			public override object RawDefaultValue => Forwarder.RawDefaultValue;

			public override MemberInfo Member => method;

			public override int MetadataToken => Forwarder.MetadataToken;

			internal override Module Module => method.Module;

			internal ParameterInfoImpl(MissingMethod method, int index)
			{
				this.method = method;
				this.index = index;
			}

			public override CustomModifiers __GetCustomModifiers()
			{
				if (index != -1)
				{
					return method.signature.GetParameterCustomModifiers(method, index);
				}
				return method.signature.GetReturnTypeCustomModifiers(method);
			}

			public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
			{
				return Forwarder.__TryGetFieldMarshal(out fieldMarshal);
			}

			public override string ToString()
			{
				return Forwarder.ToString();
			}
		}

		private readonly Type declaringType;

		private readonly string name;

		internal MethodSignature signature;

		private MethodInfo forwarder;

		private Type[] typeArgs;

		private MethodInfo Forwarder
		{
			get
			{
				MethodInfo methodInfo = TryGetForwarder();
				if (methodInfo == null)
				{
					throw new MissingMemberException(this);
				}
				return methodInfo;
			}
		}

		public override bool __IsMissing => TryGetForwarder() == null;

		public override Type ReturnType => signature.GetReturnType(this);

		public override ParameterInfo ReturnParameter => new ParameterInfoImpl(this, -1);

		internal override MethodSignature MethodSignature => signature;

		internal override int ParameterCount => signature.GetParameterCount();

		public override MethodAttributes Attributes => Forwarder.Attributes;

		public override int __MethodRVA => Forwarder.__MethodRVA;

		public override CallingConventions CallingConvention => signature.CallingConvention;

		public override string Name => name;

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

		public override Module Module => declaringType.Module;

		public override bool ContainsGenericParameters => Forwarder.ContainsGenericParameters;

		internal override bool HasThis => (signature.CallingConvention & (CallingConventions.HasThis | CallingConventions.ExplicitThis)) == CallingConventions.HasThis;

		public override bool IsGenericMethod => IsGenericMethodDefinition;

		public override bool IsGenericMethodDefinition => signature.GenericParameterCount != 0;

		public override int MetadataToken => Forwarder.MetadataToken;

		internal override bool IsBaked => Forwarder.IsBaked;

		internal MissingMethod(Type declaringType, string name, MethodSignature signature)
		{
			this.declaringType = declaringType;
			this.name = name;
			this.signature = signature;
		}

		private MethodInfo TryGetForwarder()
		{
			if (forwarder == null && !declaringType.__IsMissing)
			{
				MethodBase methodBase = declaringType.FindMethod(name, signature);
				ConstructorInfo constructorInfo = methodBase as ConstructorInfo;
				if (constructorInfo != null)
				{
					forwarder = constructorInfo.GetMethodInfo();
				}
				else
				{
					forwarder = (MethodInfo)methodBase;
				}
			}
			return forwarder;
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] array = new ParameterInfo[signature.GetParameterCount()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ParameterInfoImpl(this, i);
			}
			return array;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return Forwarder.GetMethodImplementationFlags();
		}

		public override MethodBody GetMethodBody()
		{
			return Forwarder.GetMethodBody();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			MethodInfo methodInfo = TryGetForwarder();
			if (methodInfo != null)
			{
				return methodInfo.ImportTo(module);
			}
			return module.ImportMethodOrField(declaringType, Name, MethodSignature);
		}

		public override bool Equals(object obj)
		{
			MissingMethod missingMethod = obj as MissingMethod;
			if (missingMethod != null && missingMethod.declaringType == declaringType && missingMethod.name == name)
			{
				return missingMethod.signature.Equals(signature);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() ^ name.GetHashCode() ^ signature.GetHashCode();
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			MethodInfo methodInfo = TryGetForwarder();
			if (methodInfo != null)
			{
				return methodInfo.BindTypeParameters(type);
			}
			return new GenericMethodInstance(type, this, null);
		}

		public override Type[] GetGenericArguments()
		{
			if (TryGetForwarder() != null)
			{
				return Forwarder.GetGenericArguments();
			}
			if (typeArgs == null)
			{
				typeArgs = new Type[signature.GenericParameterCount];
				for (int i = 0; i < typeArgs.Length; i++)
				{
					typeArgs[i] = new MissingTypeParameter(this, i);
				}
			}
			return Util.Copy(typeArgs);
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			return GetGenericArguments()[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return Forwarder.GetGenericMethodArgumentCount();
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			return Forwarder.GetGenericMethodDefinition();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return Forwarder.GetMethodOnTypeDefinition();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			MethodInfo methodInfo = TryGetForwarder();
			if (methodInfo != null)
			{
				return methodInfo.MakeGenericMethod(typeArguments);
			}
			return new GenericMethodInstance(declaringType, this, typeArguments);
		}

		internal override int GetCurrentToken()
		{
			return Forwarder.GetCurrentToken();
		}
	}
}
