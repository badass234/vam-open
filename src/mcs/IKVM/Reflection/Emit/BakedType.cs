namespace IKVM.Reflection.Emit
{
	internal sealed class BakedType : TypeInfo
	{
		public override string AssemblyQualifiedName => underlyingType.AssemblyQualifiedName;

		public override Type BaseType => underlyingType.BaseType;

		internal override TypeName TypeName => underlyingType.TypeName;

		public override string Name => TypeNameParser.Escape(underlyingType.__Name);

		public override string FullName => GetFullName();

		public override TypeAttributes Attributes => underlyingType.Attributes;

		public override Type DeclaringType => underlyingType.DeclaringType;

		public override bool IsGenericType => underlyingType.IsGenericType;

		public override bool IsGenericTypeDefinition => underlyingType.IsGenericTypeDefinition;

		public override bool ContainsGenericParameters => underlyingType.ContainsGenericParameters;

		public override int MetadataToken => underlyingType.MetadataToken;

		public override Module Module => underlyingType.Module;

		internal override bool IsBaked => true;

		internal BakedType(TypeBuilder typeBuilder)
			: base(typeBuilder)
		{
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return underlyingType.__GetDeclaredInterfaces();
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			return underlyingType.__GetDeclaredMethods();
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			return underlyingType.__GetMethodImplMap();
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			return underlyingType.__GetDeclaredFields();
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			return underlyingType.__GetDeclaredEvents();
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			return underlyingType.__GetDeclaredProperties();
		}

		public override Type[] __GetDeclaredTypes()
		{
			return underlyingType.__GetDeclaredTypes();
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			return underlyingType.__GetLayout(out packingSize, out typeSize);
		}

		public override Type[] GetGenericArguments()
		{
			return underlyingType.GetGenericArguments();
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return underlyingType.GetGenericTypeArgument(index);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			return underlyingType.__GetGenericArgumentsCustomModifiers();
		}

		internal override int GetModuleBuilderToken()
		{
			return underlyingType.GetModuleBuilderToken();
		}
	}
}
