using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public class TypeSpec : MemberSpec
	{
		protected Type info;

		protected MemberCache cache;

		protected IList<TypeSpec> ifaces;

		private TypeSpec base_type;

		private Dictionary<TypeSpec[], InflatedTypeSpec> inflated_instances;

		public static readonly TypeSpec[] EmptyTypes;

		private static readonly Type TypeBuilder;

		private static readonly Type GenericTypeBuilder;

		public override int Arity => MemberDefinition.TypeParametersCount;

		public virtual TypeSpec BaseType
		{
			get
			{
				return base_type;
			}
			set
			{
				base_type = value;
			}
		}

		public virtual BuiltinTypeSpec.Type BuiltinType => BuiltinTypeSpec.Type.None;

		public bool HasDynamicElement => (state & StateFlags.HasDynamicElement) != 0;

		public virtual IList<TypeSpec> Interfaces
		{
			get
			{
				if ((state & StateFlags.InterfacesImported) == 0)
				{
					state |= StateFlags.InterfacesImported;
					ImportedTypeDefinition importedTypeDefinition = MemberDefinition as ImportedTypeDefinition;
					if (importedTypeDefinition != null && Kind != MemberKind.MissingType)
					{
						importedTypeDefinition.DefineInterfaces(this);
					}
				}
				return ifaces;
			}
			set
			{
				ifaces = value;
			}
		}

		public bool IsArray => Kind == MemberKind.ArrayType;

		public bool IsAttribute
		{
			get
			{
				if (!IsClass)
				{
					return false;
				}
				TypeSpec typeSpec = this;
				do
				{
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Attribute)
					{
						return true;
					}
					if (typeSpec.IsGeneric)
					{
						return false;
					}
					typeSpec = typeSpec.base_type;
				}
				while (typeSpec != null);
				return false;
			}
		}

		public bool IsInterface => Kind == MemberKind.Interface;

		public bool IsClass => Kind == MemberKind.Class;

		public bool IsConstantCompatible
		{
			get
			{
				if ((Kind & (MemberKind.Class | MemberKind.Delegate | MemberKind.Enum | MemberKind.Interface | MemberKind.ArrayType)) != 0)
				{
					return true;
				}
				switch (BuiltinType)
				{
				case BuiltinTypeSpec.Type.FirstPrimitive:
				case BuiltinTypeSpec.Type.Byte:
				case BuiltinTypeSpec.Type.SByte:
				case BuiltinTypeSpec.Type.Char:
				case BuiltinTypeSpec.Type.Short:
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.Int:
				case BuiltinTypeSpec.Type.UInt:
				case BuiltinTypeSpec.Type.Long:
				case BuiltinTypeSpec.Type.ULong:
				case BuiltinTypeSpec.Type.Float:
				case BuiltinTypeSpec.Type.Double:
				case BuiltinTypeSpec.Type.Decimal:
				case BuiltinTypeSpec.Type.Dynamic:
					return true;
				default:
					return false;
				}
			}
		}

		public bool IsDelegate => Kind == MemberKind.Delegate;

		public virtual bool IsExpressionTreeType
		{
			get
			{
				return false;
			}
			set
			{
				state = (value ? (state | StateFlags.InflatedExpressionType) : (state & ~StateFlags.InflatedExpressionType));
			}
		}

		public bool IsEnum => Kind == MemberKind.Enum;

		public virtual bool IsArrayGenericInterface
		{
			get
			{
				return false;
			}
			set
			{
				state = (value ? (state | StateFlags.GenericIterateInterface) : (state & ~StateFlags.GenericIterateInterface));
			}
		}

		public virtual bool IsGenericTask
		{
			get
			{
				return false;
			}
			set
			{
				state = (value ? (state | StateFlags.GenericTask) : (state & ~StateFlags.GenericTask));
			}
		}

		public bool IsGenericOrParentIsGeneric
		{
			get
			{
				TypeSpec typeSpec = this;
				do
				{
					if (typeSpec.IsGeneric)
					{
						return true;
					}
					typeSpec = typeSpec.declaringType;
				}
				while (typeSpec != null);
				return false;
			}
		}

		public bool IsGenericParameter => Kind == MemberKind.TypeParameter;

		public virtual bool IsNullableType
		{
			get
			{
				return false;
			}
			set
			{
				state = (value ? (state | StateFlags.InflatedNullableType) : (state & ~StateFlags.InflatedNullableType));
			}
		}

		public bool IsNested
		{
			get
			{
				if (declaringType != null)
				{
					return Kind != MemberKind.TypeParameter;
				}
				return false;
			}
		}

		public bool IsPointer => Kind == MemberKind.PointerType;

		public bool IsSealed => (base.Modifiers & Modifiers.SEALED) != 0;

		public bool IsSpecialRuntimeType
		{
			get
			{
				return (state & StateFlags.SpecialRuntimeType) != 0;
			}
			set
			{
				state = (value ? (state | StateFlags.SpecialRuntimeType) : (state & ~StateFlags.SpecialRuntimeType));
			}
		}

		public bool IsStruct => Kind == MemberKind.Struct;

		public bool IsStructOrEnum => (Kind & (MemberKind.Struct | MemberKind.Enum)) != 0;

		public bool IsTypeBuilder
		{
			get
			{
				Type type = GetMetaInfo().GetType();
				if (type != TypeBuilder)
				{
					return type == GenericTypeBuilder;
				}
				return true;
			}
		}

		public bool IsUnmanaged
		{
			get
			{
				if (IsPointer)
				{
					return ((ElementTypeSpec)this).Element.IsUnmanaged;
				}
				TypeDefinition typeDefinition = MemberDefinition as TypeDefinition;
				if (typeDefinition != null)
				{
					return typeDefinition.IsUnmanagedType();
				}
				if (Kind == MemberKind.Void)
				{
					return true;
				}
				if (Kind == MemberKind.TypeParameter)
				{
					return false;
				}
				if (IsNested && base.DeclaringType.IsGenericOrParentIsGeneric)
				{
					return false;
				}
				return IsValueType(this);
			}
		}

		public MemberCache MemberCache
		{
			get
			{
				if (cache == null || (state & StateFlags.PendingMemberCacheMembers) != 0)
				{
					InitializeMemberCache(false);
				}
				return cache;
			}
			set
			{
				if (cache != null)
				{
					throw new InternalErrorException("Membercache reset");
				}
				cache = value;
			}
		}

		public MemberCache MemberCacheTypes
		{
			get
			{
				if (cache == null)
				{
					InitializeMemberCache(true);
				}
				return cache;
			}
		}

		public new ITypeDefinition MemberDefinition => (ITypeDefinition)definition;

		public virtual TypeSpec[] TypeArguments => EmptyTypes;

		static TypeSpec()
		{
			EmptyTypes = new TypeSpec[0];
			Assembly assembly = typeof(object).Assembly;
			TypeBuilder = assembly.GetType("System.Reflection.Emit.TypeBuilder");
			GenericTypeBuilder = assembly.GetType("System.Reflection.MonoGenericClass");
			if (GenericTypeBuilder == null)
			{
				GenericTypeBuilder = assembly.GetType("System.Reflection.Emit.TypeBuilderInstantiation");
			}
		}

		public TypeSpec(MemberKind kind, TypeSpec declaringType, ITypeDefinition definition, Type info, Modifiers modifiers)
			: base(kind, declaringType, definition, modifiers)
		{
			base.declaringType = declaringType;
			this.info = info;
			if (definition != null && definition.TypeParametersCount > 0)
			{
				state |= StateFlags.IsGeneric;
			}
		}

		public virtual bool AddInterface(TypeSpec iface)
		{
			if ((state & StateFlags.InterfacesExpanded) != 0)
			{
				throw new InternalErrorException("Modifying expanded interface list");
			}
			if (ifaces == null)
			{
				ifaces = new List<TypeSpec> { iface };
				return true;
			}
			if (!ifaces.Contains(iface))
			{
				ifaces.Add(iface);
				return true;
			}
			return false;
		}

		public bool AddInterfaceDefined(TypeSpec iface)
		{
			if (!AddInterface(iface))
			{
				return false;
			}
			if (inflated_instances != null)
			{
				InflatedTypeSpec[] array = inflated_instances.Values.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AddInterface(iface);
				}
			}
			return true;
		}

		public static TypeSpec[] GetAllTypeArguments(TypeSpec type)
		{
			IList<TypeSpec> list = EmptyTypes;
			do
			{
				if (type.Arity > 0)
				{
					if (list.Count == 0)
					{
						list = type.TypeArguments;
					}
					else
					{
						List<TypeSpec> obj = (list as List<TypeSpec>) ?? new List<TypeSpec>(list);
						obj.AddRange(type.TypeArguments);
						list = obj;
					}
				}
				type = type.declaringType;
			}
			while (type != null);
			return (list as TypeSpec[]) ?? ((List<TypeSpec>)list).ToArray();
		}

		public AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa)
		{
			if (Kind != MemberKind.Class)
			{
				throw new InternalErrorException();
			}
			if (!pa.IsDefined)
			{
				return Attribute.DefaultUsageAttribute;
			}
			AttributeUsageAttribute attributeUsageAttribute = null;
			for (TypeSpec typeSpec = this; typeSpec != null; typeSpec = typeSpec.BaseType)
			{
				attributeUsageAttribute = typeSpec.MemberDefinition.GetAttributeUsage(pa);
				if (attributeUsageAttribute != null)
				{
					break;
				}
			}
			return attributeUsageAttribute;
		}

		public virtual Type GetMetaInfo()
		{
			return info;
		}

		public virtual TypeSpec GetDefinition()
		{
			return this;
		}

		public sealed override string GetSignatureForDocumentation()
		{
			return GetSignatureForDocumentation(false);
		}

		public virtual string GetSignatureForDocumentation(bool explicitName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (IsNested)
			{
				stringBuilder.Append(base.DeclaringType.GetSignatureForDocumentation(explicitName));
			}
			else if (MemberDefinition.Namespace != null)
			{
				stringBuilder.Append(explicitName ? MemberDefinition.Namespace.Replace('.', '#') : MemberDefinition.Namespace);
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(explicitName ? "#" : ".");
			}
			stringBuilder.Append(Name);
			if (Arity > 0)
			{
				if (this is InflatedTypeSpec)
				{
					stringBuilder.Append("{");
					for (int i = 0; i < Arity; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(",");
						}
						stringBuilder.Append(TypeArguments[i].GetSignatureForDocumentation(explicitName));
					}
					stringBuilder.Append("}");
				}
				else
				{
					stringBuilder.Append("`");
					stringBuilder.Append(Arity.ToString());
				}
			}
			return stringBuilder.ToString();
		}

		public override string GetSignatureForError()
		{
			string text;
			if (IsNested)
			{
				text = base.DeclaringType.GetSignatureForError();
			}
			else
			{
				if (MemberDefinition is AnonymousTypeClass)
				{
					return ((AnonymousTypeClass)MemberDefinition).GetSignatureForError();
				}
				text = MemberDefinition.Namespace;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text += ".";
			}
			return text + Name + GetTypeNameSignature();
		}

		public string GetSignatureForErrorIncludingAssemblyName()
		{
			return $"{GetSignatureForError()} [{MemberDefinition.DeclaringAssembly.FullName}]";
		}

		protected virtual string GetTypeNameSignature()
		{
			if (!base.IsGeneric)
			{
				return null;
			}
			return "<" + TypeManager.CSharpName(MemberDefinition.TypeParameters) + ">";
		}

		public bool ImplementsInterface(TypeSpec iface, bool variantly)
		{
			IList<TypeSpec> interfaces = Interfaces;
			if (interfaces != null)
			{
				for (int i = 0; i < interfaces.Count; i++)
				{
					if (TypeSpecComparer.IsEqual(interfaces[i], iface))
					{
						return true;
					}
					if (variantly && TypeSpecComparer.Variant.IsEqual(interfaces[i], iface))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual void InitializeMemberCache(bool onlyTypes)
		{
			try
			{
				MemberDefinition.LoadMembers(this, onlyTypes, ref cache);
			}
			catch (Exception exception)
			{
				throw new InternalErrorException(exception, "Unexpected error when loading type `{0}'", GetSignatureForError());
			}
			if (onlyTypes)
			{
				state |= StateFlags.PendingMemberCacheMembers;
			}
			else
			{
				state &= ~StateFlags.PendingMemberCacheMembers;
			}
		}

		public static bool IsBaseClass(TypeSpec type, TypeSpec baseClass, bool dynamicIsObject)
		{
			if (dynamicIsObject && baseClass.IsGeneric)
			{
				for (type = type.BaseType; type != null; type = type.BaseType)
				{
					if (TypeSpecComparer.IsEqual(type, baseClass))
					{
						return true;
					}
				}
				return false;
			}
			while (type != null)
			{
				type = type.BaseType;
				if (type == baseClass)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsReferenceType(TypeSpec t)
		{
			switch (t.Kind)
			{
			case MemberKind.TypeParameter:
				return ((TypeParameterSpec)t).IsReferenceType;
			case MemberKind.Struct:
			case MemberKind.Enum:
			case MemberKind.PointerType:
			case MemberKind.Void:
				return false;
			case MemberKind.InternalCompilerType:
				if (t != InternalType.NullLiteral)
				{
					return t.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
				}
				return true;
			default:
				return true;
			}
		}

		public static bool IsNonNullableValueType(TypeSpec t)
		{
			switch (t.Kind)
			{
			case MemberKind.TypeParameter:
				return ((TypeParameterSpec)t).IsValueType;
			case MemberKind.Struct:
				return !t.IsNullableType;
			case MemberKind.Enum:
				return true;
			default:
				return false;
			}
		}

		public static bool IsValueType(TypeSpec t)
		{
			switch (t.Kind)
			{
			case MemberKind.TypeParameter:
				return ((TypeParameterSpec)t).IsValueType;
			case MemberKind.Struct:
			case MemberKind.Enum:
				return true;
			default:
				return false;
			}
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			TypeSpec[] targs = (base.IsGeneric ? MemberDefinition.TypeParameters : EmptyTypes);
			if (base.DeclaringType == inflator.TypeInstance)
			{
				return MakeGenericType(inflator.Context, targs);
			}
			return new InflatedTypeSpec(inflator.Context, this, inflator.TypeInstance, targs);
		}

		public InflatedTypeSpec MakeGenericType(IModuleContext context, TypeSpec[] targs)
		{
			if (targs.Length == 0 && !IsNested)
			{
				throw new ArgumentException("Empty type arguments for type " + GetSignatureForError());
			}
			InflatedTypeSpec value;
			if (inflated_instances == null)
			{
				inflated_instances = new Dictionary<TypeSpec[], InflatedTypeSpec>(TypeSpecComparer.Default);
				if (IsNested)
				{
					value = this as InflatedTypeSpec;
					if (value != null)
					{
						inflated_instances.Add(TypeArguments, value);
					}
				}
			}
			if (!inflated_instances.TryGetValue(targs, out value))
			{
				if (GetDefinition() != this && !IsNested)
				{
					throw new InternalErrorException("`{0}' must be type definition or nested non-inflated type to MakeGenericType", GetSignatureForError());
				}
				value = new InflatedTypeSpec(context, this, declaringType, targs);
				inflated_instances.Add(targs, value);
			}
			return value;
		}

		public virtual TypeSpec Mutate(TypeParameterMutator mutator)
		{
			return this;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			List<MissingTypeSpecReference> list = null;
			if (Kind == MemberKind.MissingType)
			{
				list = new List<MissingTypeSpecReference>();
				list.Add(new MissingTypeSpecReference(this, caller));
				return list;
			}
			TypeSpec[] typeArguments = TypeArguments;
			foreach (TypeSpec typeSpec in typeArguments)
			{
				if (typeSpec.Kind == MemberKind.MissingType)
				{
					if (list == null)
					{
						list = new List<MissingTypeSpecReference>();
					}
					list.Add(new MissingTypeSpecReference(typeSpec, caller));
				}
			}
			if (Interfaces != null)
			{
				foreach (TypeSpec @interface in Interfaces)
				{
					if (@interface.Kind == MemberKind.MissingType)
					{
						if (list == null)
						{
							list = new List<MissingTypeSpecReference>();
						}
						list.Add(new MissingTypeSpecReference(@interface, caller));
					}
				}
			}
			if (MemberDefinition.TypeParametersCount > 0)
			{
				TypeParameterSpec[] typeParameters = MemberDefinition.TypeParameters;
				for (int i = 0; i < typeParameters.Length; i++)
				{
					List<MissingTypeSpecReference> missingDependencies = typeParameters[i].GetMissingDependencies(this);
					if (missingDependencies != null)
					{
						if (list == null)
						{
							list = new List<MissingTypeSpecReference>();
						}
						list.AddRange(missingDependencies);
					}
				}
			}
			if (list != null || BaseType == null)
			{
				return list;
			}
			return BaseType.ResolveMissingDependencies(this);
		}

		public void SetMetaInfo(Type info)
		{
			if (this.info != null)
			{
				throw new InternalErrorException("MetaInfo reset");
			}
			this.info = info;
		}

		public void SetExtensionMethodContainer()
		{
			modifiers |= Modifiers.METHOD_EXTENSION;
		}

		public void UpdateInflatedInstancesBaseType()
		{
			if (inflated_instances == null)
			{
				return;
			}
			foreach (KeyValuePair<TypeSpec[], InflatedTypeSpec> inflated_instance in inflated_instances)
			{
				inflated_instance.Value.BaseType = base_type;
			}
		}
	}
}
