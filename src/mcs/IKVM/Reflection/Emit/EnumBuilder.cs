namespace IKVM.Reflection.Emit
{
	public sealed class EnumBuilder : TypeInfo
	{
		private readonly TypeBuilder typeBuilder;

		private readonly FieldBuilder fieldBuilder;

		internal override TypeName TypeName => typeBuilder.TypeName;

		public override string Name => typeBuilder.Name;

		public override string FullName => typeBuilder.FullName;

		public override Type BaseType => typeBuilder.BaseType;

		public override TypeAttributes Attributes => typeBuilder.Attributes;

		public override Module Module => typeBuilder.Module;

		public TypeToken TypeToken => typeBuilder.TypeToken;

		public FieldBuilder UnderlyingField => fieldBuilder;

		internal override bool IsBaked => typeBuilder.IsBaked;

		internal EnumBuilder(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
			: base(typeBuilder)
		{
			this.typeBuilder = typeBuilder;
			this.fieldBuilder = fieldBuilder;
		}

		public FieldBuilder DefineLiteral(string literalName, object literalValue)
		{
			FieldBuilder obj = typeBuilder.DefineField(literalName, typeBuilder, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			obj.SetConstant(literalValue);
			return obj;
		}

		public Type CreateType()
		{
			return typeBuilder.CreateType();
		}

		public TypeInfo CreateTypeInfo()
		{
			return typeBuilder.CreateTypeInfo();
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			typeBuilder.SetCustomAttribute(con, binaryAttribute);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			typeBuilder.SetCustomAttribute(customBuilder);
		}

		public override Type GetEnumUnderlyingType()
		{
			return fieldBuilder.FieldType;
		}
	}
}
