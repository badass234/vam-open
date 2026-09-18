namespace IKVM.Reflection
{
	internal sealed class GenericParameterInfoImpl : ParameterInfo
	{
		private readonly GenericMethodInstance method;

		private readonly ParameterInfo parameterInfo;

		public override string Name => parameterInfo.Name;

		public override Type ParameterType => parameterInfo.ParameterType.BindTypeParameters(method);

		public override ParameterAttributes Attributes => parameterInfo.Attributes;

		public override int Position => parameterInfo.Position;

		public override object RawDefaultValue => parameterInfo.RawDefaultValue;

		public override MemberInfo Member => method;

		public override int MetadataToken => parameterInfo.MetadataToken;

		internal override Module Module => method.Module;

		internal GenericParameterInfoImpl(GenericMethodInstance method, ParameterInfo parameterInfo)
		{
			this.method = method;
			this.parameterInfo = parameterInfo;
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			return parameterInfo.__GetCustomModifiers().Bind(method);
		}

		public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
		{
			return parameterInfo.__TryGetFieldMarshal(out fieldMarshal);
		}
	}
}
