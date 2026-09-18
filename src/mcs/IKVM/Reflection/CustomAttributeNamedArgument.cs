namespace IKVM.Reflection
{
	public struct CustomAttributeNamedArgument
	{
		private readonly MemberInfo member;

		private readonly CustomAttributeTypedArgument value;

		public MemberInfo MemberInfo => member;

		public CustomAttributeTypedArgument TypedValue => value;

		public bool IsField => member.MemberType == MemberTypes.Field;

		public string MemberName => member.Name;

		internal CustomAttributeNamedArgument(MemberInfo member, CustomAttributeTypedArgument value)
		{
			this.member = member;
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			CustomAttributeNamedArgument customAttributeNamedArgument = this;
			CustomAttributeNamedArgument? customAttributeNamedArgument2 = obj as CustomAttributeNamedArgument?;
			return customAttributeNamedArgument == customAttributeNamedArgument2;
		}

		public override int GetHashCode()
		{
			return member.GetHashCode() ^ (53 * value.GetHashCode());
		}

		public static bool operator ==(CustomAttributeNamedArgument arg1, CustomAttributeNamedArgument arg2)
		{
			if (arg1.member.Equals(arg2.member))
			{
				return arg1.value == arg2.value;
			}
			return false;
		}

		public static bool operator !=(CustomAttributeNamedArgument arg1, CustomAttributeNamedArgument arg2)
		{
			return !(arg1 == arg2);
		}
	}
}
