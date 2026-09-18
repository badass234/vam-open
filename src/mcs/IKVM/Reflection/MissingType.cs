using System;

namespace IKVM.Reflection
{
	internal sealed class MissingType : Type
	{
		private readonly Module module;

		private readonly Type declaringType;

		private readonly string ns;

		private readonly string name;

		private Type[] typeArgs;

		private int token;

		private int flags;

		private bool cyclicTypeForwarder;

		public override bool __IsMissing => true;

		public override Type DeclaringType => declaringType;

		internal override TypeName TypeName => new TypeName(ns, name);

		public override string Name => TypeNameParser.Escape(name);

		public override string FullName => GetFullName();

		public override Module Module => module;

		public override int MetadataToken => token;

		public override bool IsValueType
		{
			get
			{
				switch (typeFlags & (TypeFlags.ValueType | TypeFlags.NotValueType))
				{
				case TypeFlags.ValueType:
					return true;
				case TypeFlags.NotValueType:
					return false;
				case TypeFlags.ValueType | TypeFlags.NotValueType:
					if (WindowsRuntimeProjection.IsProjectedValueType(ns, name, module))
					{
						typeFlags &= ~TypeFlags.NotValueType;
						return true;
					}
					if (WindowsRuntimeProjection.IsProjectedReferenceType(ns, name, module))
					{
						typeFlags &= ~TypeFlags.ValueType;
						return false;
					}
					break;
				}
				if (module.universe.ResolveMissingTypeIsValueType(this))
				{
					typeFlags |= TypeFlags.ValueType;
				}
				else
				{
					typeFlags |= TypeFlags.NotValueType;
				}
				return (typeFlags & TypeFlags.ValueType) != 0;
			}
		}

		public override Type BaseType
		{
			get
			{
				throw new MissingMemberException(this);
			}
		}

		public override TypeAttributes Attributes
		{
			get
			{
				throw new MissingMemberException(this);
			}
		}

		public override bool IsGenericType
		{
			get
			{
				throw new MissingMemberException(this);
			}
		}

		public override bool IsGenericTypeDefinition
		{
			get
			{
				throw new MissingMemberException(this);
			}
		}

		internal override bool IsBaked
		{
			get
			{
				throw new MissingMemberException(this);
			}
		}

		public override bool __IsTypeForwarder => (flags & 0x200000) != 0;

		public override bool __IsCyclicTypeForwarder => cyclicTypeForwarder;

		internal MissingType(Module module, Type declaringType, string ns, string name)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.ns = ns;
			this.name = name;
			MarkKnownType(ns, name);
			if (WindowsRuntimeProjection.IsProjectedValueType(ns, name, module))
			{
				typeFlags |= TypeFlags.ValueType;
			}
			else if (WindowsRuntimeProjection.IsProjectedReferenceType(ns, name, module))
			{
				typeFlags |= TypeFlags.NotValueType;
			}
		}

		internal override MethodBase FindMethod(string name, MethodSignature signature)
		{
			MethodInfo methodInfo = new MissingMethod(this, name, signature);
			if (name == ".ctor")
			{
				return new ConstructorInfoImpl(methodInfo);
			}
			return methodInfo;
		}

		internal override FieldInfo FindField(string name, FieldSignature signature)
		{
			return new MissingField(this, name, signature);
		}

		internal override Type FindNestedType(TypeName name)
		{
			return null;
		}

		internal override Type FindNestedTypeIgnoreCase(TypeName lowerCaseName)
		{
			return null;
		}

		public override Type[] __GetDeclaredTypes()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			throw new MissingMemberException(this);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			throw new MissingMemberException(this);
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			throw new MissingMemberException(this);
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			throw new MissingMemberException(this);
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			throw new MissingMemberException(this);
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			throw new MissingMemberException(this);
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] GetGenericArguments()
		{
			throw new MissingMemberException(this);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			throw new MissingMemberException(this);
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			if (typeArgs == null)
			{
				typeArgs = new Type[index + 1];
			}
			else if (typeArgs.Length <= index)
			{
				Array.Resize(ref typeArgs, index + 1);
			}
			return typeArgs[index] ?? (typeArgs[index] = new MissingTypeParameter(this, index));
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			return this;
		}

		internal override Type SetMetadataTokenForMissing(int token, int flags)
		{
			this.token = token;
			this.flags = flags;
			return this;
		}

		internal override Type SetCyclicTypeForwarder()
		{
			cyclicTypeForwarder = true;
			return this;
		}
	}
}
