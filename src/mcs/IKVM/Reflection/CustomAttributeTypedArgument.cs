namespace IKVM.Reflection
{
	public struct CustomAttributeTypedArgument
	{
		private readonly Type type;

		private readonly object value;

		public Type ArgumentType => type;

		public object Value => value;

		internal CustomAttributeTypedArgument(Type type, object value)
		{
			this.type = type;
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			CustomAttributeTypedArgument customAttributeTypedArgument = this;
			CustomAttributeTypedArgument? customAttributeTypedArgument2 = obj as CustomAttributeTypedArgument?;
			return customAttributeTypedArgument == customAttributeTypedArgument2;
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() ^ (77 * ((value != null) ? value.GetHashCode() : 0));
		}

		public static bool operator ==(CustomAttributeTypedArgument arg1, CustomAttributeTypedArgument arg2)
		{
			if (arg1.type.Equals(arg2.type))
			{
				if (arg1.value != arg2.value)
				{
					if (arg1.value != null)
					{
						return arg1.value.Equals(arg2.value);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public static bool operator !=(CustomAttributeTypedArgument arg1, CustomAttributeTypedArgument arg2)
		{
			return !(arg1 == arg2);
		}
	}
}
