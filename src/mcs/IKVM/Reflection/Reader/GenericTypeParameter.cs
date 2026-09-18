using System.Collections.Generic;

namespace IKVM.Reflection.Reader
{
	internal sealed class GenericTypeParameter : TypeParameterType
	{
		private readonly ModuleReader module;

		private readonly int index;

		public override string Namespace => DeclaringType.Namespace;

		public override string Name => module.GetString(module.GenericParam.records[index].Name);

		public override Module Module => module;

		public override int MetadataToken => (42 << 24) + index + 1;

		public override int GenericParameterPosition => module.GenericParam.records[index].Number;

		public override Type DeclaringType
		{
			get
			{
				int owner = module.GenericParam.records[index].Owner;
				if (owner >> 24 != 2)
				{
					return null;
				}
				return module.ResolveType(owner);
			}
		}

		public override MethodBase DeclaringMethod
		{
			get
			{
				int owner = module.GenericParam.records[index].Owner;
				if (owner >> 24 != 6)
				{
					return null;
				}
				return module.ResolveMethod(owner);
			}
		}

		public override GenericParameterAttributes GenericParameterAttributes => (GenericParameterAttributes)module.GenericParam.records[index].Flags;

		internal override bool IsBaked => true;

		internal GenericTypeParameter(ModuleReader module, int index, byte sigElementType)
			: base(sigElementType)
		{
			this.module = module;
			this.index = index;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override Type[] GetGenericParameterConstraints()
		{
			IGenericContext context = (DeclaringMethod as IGenericContext) ?? DeclaringType;
			List<Type> list = new List<Type>();
			foreach (int item in module.GenericParamConstraint.Filter(MetadataToken))
			{
				list.Add(module.ResolveType(module.GenericParamConstraint.records[item].Constraint, context));
			}
			return list.ToArray();
		}

		public override CustomModifiers[] __GetGenericParameterConstraintCustomModifiers()
		{
			IGenericContext context = (DeclaringMethod as IGenericContext) ?? DeclaringType;
			List<CustomModifiers> list = new List<CustomModifiers>();
			foreach (int item2 in module.GenericParamConstraint.Filter(MetadataToken))
			{
				CustomModifiers item = default(CustomModifiers);
				int constraint = module.GenericParamConstraint.records[item2].Constraint;
				if (constraint >> 24 == 27)
				{
					int num = (constraint & 0xFFFFFF) - 1;
					item = CustomModifiers.Read(module, module.GetBlob(module.TypeSpec.records[num]), context);
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			if (module.GenericParam.records[index].Owner >> 24 == 6)
			{
				return binder.BindMethodParameter(this);
			}
			return binder.BindTypeParameter(this);
		}
	}
}
