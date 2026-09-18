using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class InflatedTypeSpec : TypeSpec
	{
		private TypeSpec[] targs;

		private TypeParameterSpec[] constraints;

		private readonly TypeSpec open_type;

		private readonly IModuleContext context;

		public override TypeSpec BaseType
		{
			get
			{
				if (cache == null || (state & StateFlags.PendingBaseTypeInflate) != 0)
				{
					InitializeMemberCache(true);
				}
				return base.BaseType;
			}
		}

		public TypeParameterSpec[] Constraints
		{
			get
			{
				if (constraints == null)
				{
					constraints = TypeParameterSpec.InflateConstraints(base.MemberDefinition.TypeParameters, (InflatedTypeSpec l) => l.CreateLocalInflator(context), this);
				}
				return constraints;
			}
		}

		public bool HasConstraintsChecked
		{
			get
			{
				return (state & StateFlags.ConstraintsChecked) != 0;
			}
			set
			{
				state = (value ? (state | StateFlags.ConstraintsChecked) : (state & ~StateFlags.ConstraintsChecked));
			}
		}

		public override IList<TypeSpec> Interfaces
		{
			get
			{
				if (cache == null)
				{
					InitializeMemberCache(true);
				}
				return base.Interfaces;
			}
		}

		public override bool IsExpressionTreeType => (open_type.state & StateFlags.InflatedExpressionType) != 0;

		public override bool IsArrayGenericInterface => (open_type.state & StateFlags.GenericIterateInterface) != 0;

		public override bool IsGenericTask => (open_type.state & StateFlags.GenericTask) != 0;

		public override bool IsNullableType => (open_type.state & StateFlags.InflatedNullableType) != 0;

		public override TypeSpec[] TypeArguments => targs;

		public InflatedTypeSpec(IModuleContext context, TypeSpec openType, TypeSpec declaringType, TypeSpec[] targs)
			: base(openType.Kind, declaringType, openType.MemberDefinition, null, openType.Modifiers)
		{
			if (targs == null)
			{
				throw new ArgumentNullException("targs");
			}
			state &= ~(StateFlags.Obsolete_Undetected | StateFlags.Obsolete | StateFlags.CLSCompliant_Undetected | StateFlags.CLSCompliant | StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency | StateFlags.HasDynamicElement);
			state |= openType.state & (StateFlags.Obsolete_Undetected | StateFlags.Obsolete | StateFlags.CLSCompliant_Undetected | StateFlags.CLSCompliant | StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency | StateFlags.HasDynamicElement);
			this.context = context;
			open_type = openType;
			this.targs = targs;
			foreach (TypeSpec typeSpec in targs)
			{
				if (typeSpec.HasDynamicElement || typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					state |= StateFlags.HasDynamicElement;
					break;
				}
			}
			if (open_type.Kind == MemberKind.MissingType)
			{
				base.MemberCache = MemberCache.Empty;
			}
			if ((open_type.Modifiers & Modifiers.COMPILER_GENERATED) != 0)
			{
				state |= StateFlags.ConstraintsChecked;
			}
		}

		public override bool AddInterface(TypeSpec iface)
		{
			iface = CreateLocalInflator(context).Inflate(iface);
			if (iface == null)
			{
				return false;
			}
			return base.AddInterface(iface);
		}

		public static bool ContainsTypeParameter(TypeSpec type)
		{
			if (type.Kind == MemberKind.TypeParameter)
			{
				return true;
			}
			ElementTypeSpec elementTypeSpec = type as ElementTypeSpec;
			if (elementTypeSpec != null)
			{
				return ContainsTypeParameter(elementTypeSpec.Element);
			}
			TypeSpec[] typeArguments = type.TypeArguments;
			for (int i = 0; i < typeArguments.Length; i++)
			{
				if (ContainsTypeParameter(typeArguments[i]))
				{
					return true;
				}
			}
			return false;
		}

		public TypeParameterInflator CreateLocalInflator(IModuleContext context)
		{
			TypeSpec[] array = targs;
			TypeParameterSpec[] tparams;
			if (!base.IsNested)
			{
				tparams = ((targs.Length != 0) ? open_type.MemberDefinition.TypeParameters : TypeParameterSpec.EmptyTypes);
			}
			else
			{
				List<TypeSpec> list = null;
				List<TypeParameterSpec> list2 = null;
				TypeSpec typeSpec = base.DeclaringType;
				do
				{
					if (typeSpec.TypeArguments.Length != 0)
					{
						if (list == null)
						{
							list = new List<TypeSpec>();
							list2 = new List<TypeParameterSpec>();
							if (targs.Length != 0)
							{
								list.AddRange(targs);
								list2.AddRange(open_type.MemberDefinition.TypeParameters);
							}
						}
						list2.AddRange(typeSpec.MemberDefinition.TypeParameters);
						list.AddRange(typeSpec.TypeArguments);
					}
					typeSpec = typeSpec.DeclaringType;
				}
				while (typeSpec != null);
				if (list == null)
				{
					tparams = ((targs.Length != 0) ? open_type.MemberDefinition.TypeParameters : TypeParameterSpec.EmptyTypes);
				}
				else
				{
					array = list.ToArray();
					tparams = list2.ToArray();
				}
			}
			return new TypeParameterInflator(context, this, tparams, array);
		}

		private Type CreateMetaInfo()
		{
			List<Type> list = new List<Type>();
			TypeSpec typeSpec = this;
			TypeSpec typeSpec2 = typeSpec;
			do
			{
				if (typeSpec.GetDefinition().IsGeneric)
				{
					list.InsertRange(0, (typeSpec.TypeArguments != TypeSpec.EmptyTypes) ? typeSpec.TypeArguments.Select((TypeSpec l) => l.GetMetaInfo()) : typeSpec.MemberDefinition.TypeParameters.Select((TypeParameterSpec l) => l.GetMetaInfo()));
				}
				typeSpec2 = typeSpec2.GetDefinition();
				typeSpec = typeSpec.DeclaringType;
			}
			while (typeSpec != null);
			return typeSpec2.GetMetaInfo().MakeGenericType(list.ToArray());
		}

		public override ObsoleteAttribute GetAttributeObsolete()
		{
			return open_type.GetAttributeObsolete();
		}

		protected override bool IsNotCLSCompliant(out bool attrValue)
		{
			if (base.IsNotCLSCompliant(out attrValue))
			{
				return true;
			}
			TypeSpec[] typeArguments = TypeArguments;
			for (int i = 0; i < typeArguments.Length; i++)
			{
				if (typeArguments[i].MemberDefinition.CLSAttributeValue == false)
				{
					return true;
				}
			}
			return false;
		}

		public override TypeSpec GetDefinition()
		{
			return open_type;
		}

		public override Type GetMetaInfo()
		{
			if (info == null)
			{
				info = CreateMetaInfo();
			}
			return info;
		}

		public override string GetSignatureForError()
		{
			if (IsNullableType)
			{
				return targs[0].GetSignatureForError() + "?";
			}
			return base.GetSignatureForError();
		}

		protected override string GetTypeNameSignature()
		{
			if (targs.Length == 0 || base.MemberDefinition is AnonymousTypeClass)
			{
				return null;
			}
			return "<" + TypeManager.CSharpName(targs) + ">";
		}

		public bool HasDynamicArgument()
		{
			for (int i = 0; i < targs.Length; i++)
			{
				TypeSpec typeSpec = targs[i];
				if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					return true;
				}
				if (typeSpec is InflatedTypeSpec)
				{
					if (((InflatedTypeSpec)typeSpec).HasDynamicArgument())
					{
						return true;
					}
				}
				else if (typeSpec.IsArray)
				{
					while (typeSpec.IsArray)
					{
						typeSpec = ((ArrayContainer)typeSpec).Element;
					}
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override void InitializeMemberCache(bool onlyTypes)
		{
			if (cache == null)
			{
				MemberCache memberCache = (onlyTypes ? open_type.MemberCacheTypes : open_type.MemberCache);
				if (cache == null)
				{
					cache = new MemberCache(memberCache);
				}
			}
			TypeParameterInflator inflator = CreateLocalInflator(context);
			if ((state & StateFlags.PendingMemberCacheMembers) == 0)
			{
				open_type.MemberCacheTypes.InflateTypes(cache, inflator);
				if (open_type.Interfaces != null)
				{
					ifaces = new List<TypeSpec>(open_type.Interfaces.Count);
					foreach (TypeSpec @interface in open_type.Interfaces)
					{
						TypeSpec typeSpec = inflator.Inflate(@interface);
						if (typeSpec != null)
						{
							base.AddInterface(typeSpec);
						}
					}
				}
				if (open_type.BaseType == null)
				{
					if (base.IsClass)
					{
						state |= StateFlags.PendingBaseTypeInflate;
					}
				}
				else
				{
					BaseType = inflator.Inflate(open_type.BaseType);
				}
			}
			else if ((state & StateFlags.PendingBaseTypeInflate) != 0)
			{
				if (open_type.BaseType == null)
				{
					return;
				}
				BaseType = inflator.Inflate(open_type.BaseType);
				state &= ~StateFlags.PendingBaseTypeInflate;
			}
			if (onlyTypes)
			{
				state |= StateFlags.PendingMemberCacheMembers;
				return;
			}
			TypeDefinition typeDefinition = open_type.MemberDefinition as TypeDefinition;
			if (typeDefinition == null || typeDefinition.HasMembersDefined)
			{
				if ((state & StateFlags.PendingBaseTypeInflate) != 0)
				{
					BaseType = inflator.Inflate(open_type.BaseType);
					state &= ~StateFlags.PendingBaseTypeInflate;
				}
				state &= ~StateFlags.PendingMemberCacheMembers;
				open_type.MemberCache.InflateMembers(cache, open_type, inflator);
			}
		}

		public override TypeSpec Mutate(TypeParameterMutator mutator)
		{
			TypeSpec[] array = TypeArguments;
			if (array != null)
			{
				array = mutator.Mutate(array);
			}
			TypeSpec typeSpec = base.DeclaringType;
			if (base.IsNested && base.DeclaringType.IsGenericOrParentIsGeneric)
			{
				typeSpec = mutator.Mutate(typeSpec);
			}
			if (array == TypeArguments && typeSpec == base.DeclaringType)
			{
				return this;
			}
			InflatedTypeSpec inflatedTypeSpec = (InflatedTypeSpec)MemberwiseClone();
			if (typeSpec != base.DeclaringType)
			{
				inflatedTypeSpec.declaringType = typeSpec;
				inflatedTypeSpec.state |= StateFlags.PendingMetaInflate;
			}
			if (array != null)
			{
				inflatedTypeSpec.targs = array;
				inflatedTypeSpec.info = null;
			}
			return inflatedTypeSpec;
		}
	}
}
