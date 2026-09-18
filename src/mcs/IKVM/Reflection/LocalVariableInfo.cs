namespace IKVM.Reflection
{
	public class LocalVariableInfo
	{
		private readonly int index;

		private readonly Type type;

		private readonly bool pinned;

		private readonly CustomModifiers customModifiers;

		public bool IsPinned => pinned;

		public int LocalIndex => index;

		public Type LocalType => type;

		internal LocalVariableInfo(int index, Type type, bool pinned)
		{
			this.index = index;
			this.type = type;
			this.pinned = pinned;
		}

		internal LocalVariableInfo(int index, Type type, bool pinned, CustomModifiers customModifiers)
			: this(index, type, pinned)
		{
			this.customModifiers = customModifiers;
		}

		public CustomModifiers __GetCustomModifiers()
		{
			return customModifiers;
		}

		public override string ToString()
		{
			return string.Format(pinned ? "{0} ({1}) (pinned)" : "{0} ({1})", type, index);
		}
	}
}
