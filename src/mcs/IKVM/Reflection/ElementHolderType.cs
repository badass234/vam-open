using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal abstract class ElementHolderType : TypeInfo
	{
		protected readonly Type elementType;

		private int token;

		private readonly CustomModifiers mods;

		public sealed override string Name => elementType.Name + GetSuffix();

		public sealed override string Namespace => elementType.Namespace;

		public sealed override string FullName => elementType.FullName + GetSuffix();

		public sealed override Module Module => elementType.Module;

		public sealed override bool ContainsGenericParameters
		{
			get
			{
				Type type = elementType;
				while (type.HasElementType)
				{
					type = type.GetElementType();
				}
				return type.ContainsGenericParameters;
			}
		}

		protected sealed override bool ContainsMissingTypeImpl
		{
			get
			{
				Type type = elementType;
				while (type.HasElementType)
				{
					type = type.GetElementType();
				}
				if (!type.__ContainsMissingType)
				{
					return mods.ContainsMissingType;
				}
				return true;
			}
		}

		internal sealed override Universe Universe => elementType.Universe;

		internal sealed override bool IsBaked => elementType.IsBaked;

		protected ElementHolderType(Type elementType, CustomModifiers mods, byte sigElementType)
			: base(sigElementType)
		{
			this.elementType = elementType;
			this.mods = mods;
		}

		protected bool EqualsHelper(ElementHolderType other)
		{
			if (other != null && other.elementType.Equals(elementType))
			{
				return other.mods.Equals(mods);
			}
			return false;
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			return mods;
		}

		public sealed override string ToString()
		{
			return elementType.ToString() + GetSuffix();
		}

		public sealed override Type GetElementType()
		{
			return elementType;
		}

		internal sealed override int GetModuleBuilderToken()
		{
			if (token == 0)
			{
				token = ((ModuleBuilder)elementType.Module).ImportType(this);
			}
			return token;
		}

		internal sealed override Type BindTypeParameters(IGenericBinder binder)
		{
			Type type = elementType.BindTypeParameters(binder);
			CustomModifiers customModifiers = mods.Bind(binder);
			if ((object)type == elementType && customModifiers.Equals(mods))
			{
				return this;
			}
			return Wrap(type, customModifiers);
		}

		internal override void CheckBaked()
		{
			elementType.CheckBaked();
		}

		internal sealed override int GetCurrentToken()
		{
			return 0;
		}

		internal abstract string GetSuffix();

		protected abstract Type Wrap(Type type, CustomModifiers mods);
	}
}
