namespace IKVM.Reflection
{
	internal sealed class ParameterInfoWrapper : ParameterInfo
	{
		private readonly MemberInfo member;

		private readonly ParameterInfo forward;

		public override string Name => forward.Name;

		public override Type ParameterType => forward.ParameterType;

		public override ParameterAttributes Attributes => forward.Attributes;

		public override int Position => forward.Position;

		public override object RawDefaultValue => forward.RawDefaultValue;

		public override MemberInfo Member => member;

		public override int MetadataToken => forward.MetadataToken;

		internal override Module Module => member.Module;

		internal ParameterInfoWrapper(MemberInfo member, ParameterInfo forward)
		{
			this.member = member;
			this.forward = forward;
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			return forward.__GetCustomModifiers();
		}

		public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
		{
			return forward.__TryGetFieldMarshal(out fieldMarshal);
		}
	}
}
