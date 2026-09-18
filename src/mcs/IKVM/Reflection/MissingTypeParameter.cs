using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	internal sealed class MissingTypeParameter : TypeParameterType
	{
		private readonly MemberInfo owner;

		private readonly int index;

		public override Module Module => owner.Module;

		public override string Name => null;

		public override int GenericParameterPosition => index;

		public override MethodBase DeclaringMethod => owner as MethodBase;

		public override Type DeclaringType => owner as Type;

		internal override bool IsBaked => owner.IsBaked;

		internal MissingTypeParameter(Type owner, int index)
			: this(owner, index, 19)
		{
		}

		internal MissingTypeParameter(MethodInfo owner, int index)
			: this(owner, index, 30)
		{
		}

		private MissingTypeParameter(MemberInfo owner, int index, byte sigElementType)
			: base(sigElementType)
		{
			this.owner = owner;
			this.index = index;
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			if (owner is MethodBase)
			{
				return binder.BindMethodParameter(this);
			}
			return binder.BindTypeParameter(this);
		}
	}
}
