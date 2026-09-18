namespace IKVM.Reflection
{
	internal sealed class ByRefType : ElementHolderType
	{
		public override Type BaseType => null;

		public override TypeAttributes Attributes => TypeAttributes.AnsiClass;

		internal static Type Make(Type type, CustomModifiers mods)
		{
			return type.Universe.CanonicalizeType(new ByRefType(type, mods));
		}

		private ByRefType(Type type, CustomModifiers mods)
			: base(type, mods, 16)
		{
		}

		public override bool Equals(object o)
		{
			return EqualsHelper(o as ByRefType);
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 3;
		}

		internal override string GetSuffix()
		{
			return "&";
		}

		protected override Type Wrap(Type type, CustomModifiers mods)
		{
			return Make(type, mods);
		}
	}
}
