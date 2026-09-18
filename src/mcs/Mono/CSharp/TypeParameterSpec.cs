using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mono.CSharp
{
	[DebuggerDisplay("{DisplayDebugInfo()}")]
	public class TypeParameterSpec : TypeSpec
	{
		public new static readonly TypeParameterSpec[] EmptyTypes = new TypeParameterSpec[0];

		private Variance variance;

		private SpecialConstraint spec;

		private int tp_pos;

		private TypeSpec[] targs;

		private TypeSpec[] ifaces_defined;

		private TypeSpec effective_base;

		public int DeclaredPosition
		{
			get
			{
				return tp_pos;
			}
			set
			{
				tp_pos = value;
			}
		}

		public bool HasSpecialConstructor => (spec & SpecialConstraint.Constructor) != 0;

		public bool HasSpecialClass => (spec & SpecialConstraint.Class) != 0;

		public bool HasSpecialStruct => (spec & SpecialConstraint.Struct) != 0;

		public bool HasAnyTypeConstraint
		{
			get
			{
				if ((spec & (SpecialConstraint.Class | SpecialConstraint.Struct)) == 0 && ifaces == null && targs == null)
				{
					return HasTypeConstraint;
				}
				return true;
			}
		}

		public bool HasTypeConstraint
		{
			get
			{
				BuiltinTypeSpec.Type builtinType = BaseType.BuiltinType;
				if (builtinType != BuiltinTypeSpec.Type.Object)
				{
					return builtinType != BuiltinTypeSpec.Type.ValueType;
				}
				return false;
			}
		}

		public override IList<TypeSpec> Interfaces
		{
			get
			{
				if ((state & StateFlags.InterfacesExpanded) == 0)
				{
					if (ifaces != null)
					{
						if (ifaces_defined == null)
						{
							ifaces_defined = ifaces.ToArray();
						}
						for (int i = 0; i < ifaces_defined.Length; i++)
						{
							TypeSpec typeSpec = ifaces_defined[i];
							(typeSpec.MemberDefinition as TypeDefinition)?.DoExpandBaseInterfaces();
							if (typeSpec.Interfaces != null)
							{
								for (int j = 0; j < typeSpec.Interfaces.Count; j++)
								{
									TypeSpec iface = typeSpec.Interfaces[j];
									AddInterface(iface);
								}
							}
						}
					}
					else if (ifaces_defined == null)
					{
						ifaces_defined = ((ifaces == null) ? TypeSpec.EmptyTypes : ifaces.ToArray());
					}
					if (BaseType != null)
					{
						(BaseType.MemberDefinition as TypeDefinition)?.DoExpandBaseInterfaces();
						if (BaseType.Interfaces != null)
						{
							foreach (TypeSpec @interface in BaseType.Interfaces)
							{
								AddInterface(@interface);
							}
						}
					}
					state |= StateFlags.InterfacesExpanded;
				}
				return ifaces;
			}
		}

		public TypeSpec[] InterfacesDefined
		{
			get
			{
				if (ifaces_defined == null)
				{
					ifaces_defined = ((ifaces == null) ? TypeSpec.EmptyTypes : ifaces.ToArray());
				}
				if (ifaces_defined.Length != 0)
				{
					return ifaces_defined;
				}
				return null;
			}
			set
			{
				ifaces_defined = value;
				if (value != null && value.Length != 0)
				{
					ifaces = new List<TypeSpec>(value);
				}
			}
		}

		public bool IsConstrained
		{
			get
			{
				if (spec == SpecialConstraint.None && ifaces == null && targs == null)
				{
					return HasTypeConstraint;
				}
				return true;
			}
		}

		public new bool IsReferenceType
		{
			get
			{
				if ((spec & (SpecialConstraint.Class | SpecialConstraint.Struct)) != SpecialConstraint.None)
				{
					return (spec & SpecialConstraint.Class) != 0;
				}
				if (HasTypeConstraint && TypeSpec.IsReferenceType(BaseType))
				{
					return true;
				}
				if (targs != null)
				{
					TypeSpec[] array = targs;
					foreach (TypeSpec typeSpec in array)
					{
						TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
						if ((typeParameterSpec == null || (typeParameterSpec.spec & (SpecialConstraint.Class | SpecialConstraint.Struct)) == 0) && TypeSpec.IsReferenceType(typeSpec))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public new bool IsValueType
		{
			get
			{
				if (HasSpecialStruct)
				{
					return true;
				}
				if (targs != null)
				{
					TypeSpec[] array = targs;
					for (int i = 0; i < array.Length; i++)
					{
						if (TypeSpec.IsValueType(array[i]))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public override string Name => definition.Name;

		public bool IsMethodOwned => base.DeclaringType == null;

		public SpecialConstraint SpecialConstraint
		{
			get
			{
				return spec;
			}
			set
			{
				spec = value;
			}
		}

		public new TypeSpec[] TypeArguments
		{
			get
			{
				return targs;
			}
			set
			{
				targs = value;
			}
		}

		public Variance Variance => variance;

		public TypeParameterSpec(TypeSpec declaringType, int index, ITypeDefinition definition, SpecialConstraint spec, Variance variance, Type info)
			: base(MemberKind.TypeParameter, declaringType, definition, info, Modifiers.PUBLIC)
		{
			this.variance = variance;
			this.spec = spec;
			state &= ~StateFlags.Obsolete_Undetected;
			tp_pos = index;
		}

		public TypeParameterSpec(int index, ITypeDefinition definition, SpecialConstraint spec, Variance variance, Type info)
			: this(null, index, definition, spec, variance, info)
		{
		}

		public string DisplayDebugInfo()
		{
			string signatureForError = GetSignatureForError();
			if (!IsMethodOwned)
			{
				return signatureForError + "!";
			}
			return signatureForError + "!!";
		}

		public TypeSpec GetEffectiveBase()
		{
			if (HasSpecialStruct)
			{
				return BaseType;
			}
			if (BaseType != null && targs == null)
			{
				if (!BaseType.IsStruct)
				{
					return BaseType;
				}
				return BaseType.BaseType;
			}
			if (effective_base != null)
			{
				return effective_base;
			}
			TypeSpec[] array = new TypeSpec[HasTypeConstraint ? (targs.Length + 1) : targs.Length];
			for (int i = 0; i < targs.Length; i++)
			{
				TypeSpec typeSpec = targs[i];
				if (typeSpec.IsStruct)
				{
					array[i] = typeSpec.BaseType;
					continue;
				}
				TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
				array[i] = ((typeParameterSpec != null) ? typeParameterSpec.GetEffectiveBase() : typeSpec);
			}
			if (HasTypeConstraint)
			{
				array[array.Length - 1] = BaseType;
			}
			return effective_base = Convert.FindMostEncompassedType(array);
		}

		public override string GetSignatureForDocumentation(bool explicitName)
		{
			if (explicitName)
			{
				return Name;
			}
			return (IsMethodOwned ? "``" : "`") + DeclaredPosition;
		}

		public override string GetSignatureForError()
		{
			return Name;
		}

		public bool HasSameConstraintsDefinition(TypeParameterSpec other)
		{
			if (spec != other.spec)
			{
				return false;
			}
			if (BaseType != other.BaseType)
			{
				return false;
			}
			if (!TypeSpecComparer.Override.IsSame(InterfacesDefined, other.InterfacesDefined))
			{
				return false;
			}
			if (!TypeSpecComparer.Override.IsSame(targs, other.targs))
			{
				return false;
			}
			return true;
		}

		public bool HasSameConstraintsImplementation(TypeParameterSpec other)
		{
			if (spec != other.spec)
			{
				return false;
			}
			if (!TypeSpecComparer.Override.IsEqual(BaseType, other.BaseType))
			{
				bool flag = false;
				if (other.targs != null)
				{
					TypeSpec[] array = other.targs;
					foreach (TypeSpec b in array)
					{
						if (TypeSpecComparer.Override.IsEqual(BaseType, b))
						{
							flag = true;
							break;
						}
					}
				}
				else if (targs != null)
				{
					TypeSpec[] array = targs;
					for (int i = 0; i < array.Length; i++)
					{
						if (TypeSpecComparer.Override.IsEqual(array[i], other.BaseType))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (InterfacesDefined != null)
			{
				foreach (TypeSpec @interface in Interfaces)
				{
					bool flag = false;
					if (other.InterfacesDefined != null)
					{
						foreach (TypeSpec interface2 in other.Interfaces)
						{
							if (TypeSpecComparer.Override.IsEqual(@interface, interface2))
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						continue;
					}
					if (other.targs != null)
					{
						TypeSpec[] array = other.targs;
						foreach (TypeSpec b2 in array)
						{
							if (TypeSpecComparer.Override.IsEqual(@interface, b2))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (other.InterfacesDefined != null)
			{
				foreach (TypeSpec interface3 in other.Interfaces)
				{
					bool flag = false;
					if (InterfacesDefined != null)
					{
						foreach (TypeSpec interface4 in Interfaces)
						{
							if (TypeSpecComparer.Override.IsEqual(interface4, interface3))
							{
								flag = true;
								break;
							}
						}
					}
					else if (targs != null)
					{
						TypeSpec[] array = targs;
						for (int i = 0; i < array.Length; i++)
						{
							if (TypeSpecComparer.Override.IsEqual(array[i], interface3))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (targs != null)
			{
				TypeSpec[] array = targs;
				foreach (TypeSpec typeSpec in array)
				{
					bool flag = false;
					if (other.targs != null)
					{
						TypeSpec[] array2 = other.targs;
						foreach (TypeSpec b3 in array2)
						{
							if (TypeSpecComparer.Override.IsEqual(typeSpec, b3))
							{
								flag = true;
								break;
							}
						}
					}
					if (other.InterfacesDefined != null && !flag)
					{
						foreach (TypeSpec interface5 in other.Interfaces)
						{
							if (TypeSpecComparer.Override.IsEqual(interface5, typeSpec))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						flag = TypeSpecComparer.Override.IsEqual(typeSpec, other.BaseType);
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (other.targs != null)
			{
				TypeSpec[] array = other.targs;
				foreach (TypeSpec typeSpec2 in array)
				{
					if (!typeSpec2.IsGenericParameter)
					{
						continue;
					}
					if (targs == null)
					{
						return false;
					}
					bool flag = false;
					TypeSpec[] array2 = targs;
					for (int j = 0; j < array2.Length; j++)
					{
						if (TypeSpecComparer.Override.IsEqual(array2[j], typeSpec2))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static TypeParameterSpec[] InflateConstraints(TypeParameterInflator inflator, TypeParameterSpec[] tparams)
		{
			return InflateConstraints(tparams, (TypeParameterInflator l) => l, inflator);
		}

		public static TypeParameterSpec[] InflateConstraints<T>(TypeParameterSpec[] tparams, Func<T, TypeParameterInflator> inflatorFactory, T arg)
		{
			TypeParameterSpec[] array = null;
			TypeParameterInflator? typeParameterInflator = null;
			for (int i = 0; i < tparams.Length; i++)
			{
				TypeParameterSpec typeParameterSpec = tparams[i];
				if (typeParameterSpec.HasTypeConstraint || typeParameterSpec.InterfacesDefined != null || typeParameterSpec.TypeArguments != null)
				{
					if (array == null)
					{
						array = new TypeParameterSpec[tparams.Length];
						Array.Copy(tparams, array, array.Length);
					}
					if (!typeParameterInflator.HasValue)
					{
						typeParameterInflator = inflatorFactory(arg);
					}
					array[i] = (TypeParameterSpec)array[i].InflateMember(typeParameterInflator.Value);
				}
			}
			if (array == null)
			{
				array = tparams;
			}
			return array;
		}

		public void InflateConstraints(TypeParameterInflator inflator, TypeParameterSpec tps)
		{
			tps.BaseType = inflator.Inflate(BaseType);
			TypeSpec[] interfacesDefined = InterfacesDefined;
			if (interfacesDefined != null)
			{
				tps.ifaces_defined = new TypeSpec[interfacesDefined.Length];
				for (int i = 0; i < interfacesDefined.Length; i++)
				{
					tps.ifaces_defined[i] = inflator.Inflate(interfacesDefined[i]);
				}
			}
			else if (ifaces_defined == TypeSpec.EmptyTypes)
			{
				tps.ifaces_defined = TypeSpec.EmptyTypes;
			}
			IList<TypeSpec> interfaces = Interfaces;
			if (interfaces != null)
			{
				tps.ifaces = new List<TypeSpec>(interfaces.Count);
				for (int j = 0; j < interfaces.Count; j++)
				{
					tps.ifaces.Add(inflator.Inflate(interfaces[j]));
				}
				tps.state |= StateFlags.InterfacesExpanded;
			}
			if (targs != null)
			{
				tps.targs = new TypeSpec[targs.Length];
				for (int k = 0; k < targs.Length; k++)
				{
					tps.targs[k] = inflator.Inflate(targs[k]);
				}
			}
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			TypeParameterSpec typeParameterSpec = (TypeParameterSpec)MemberwiseClone();
			InflateConstraints(inflator, typeParameterSpec);
			return typeParameterSpec;
		}

		protected override void InitializeMemberCache(bool onlyTypes)
		{
			cache = new MemberCache();
			if (BaseType.BuiltinType != BuiltinTypeSpec.Type.Object && BaseType.BuiltinType != BuiltinTypeSpec.Type.ValueType)
			{
				cache.AddBaseType(BaseType);
			}
			TypeSpec[] interfacesDefined;
			if (InterfacesDefined != null)
			{
				interfacesDefined = InterfacesDefined;
				foreach (TypeSpec iface in interfacesDefined)
				{
					cache.AddInterface(iface);
				}
			}
			if (targs == null)
			{
				return;
			}
			interfacesDefined = targs;
			foreach (TypeSpec typeSpec in interfacesDefined)
			{
				TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
				TypeSpec typeSpec2;
				IList<TypeSpec> list;
				if (typeParameterSpec != null)
				{
					typeSpec2 = typeParameterSpec.GetEffectiveBase();
					list = typeParameterSpec.InterfacesDefined;
				}
				else
				{
					typeSpec2 = typeSpec;
					list = typeSpec.Interfaces;
				}
				if (typeSpec2 != null && typeSpec2.BuiltinType != BuiltinTypeSpec.Type.Object && typeSpec2.BuiltinType != BuiltinTypeSpec.Type.ValueType && !typeSpec2.IsStructOrEnum)
				{
					cache.AddBaseType(typeSpec2);
				}
				if (list == null)
				{
					continue;
				}
				foreach (TypeSpec item in list)
				{
					cache.AddInterface(item);
				}
			}
		}

		public bool IsConvertibleToInterface(TypeSpec iface)
		{
			if (Interfaces != null)
			{
				foreach (TypeSpec @interface in Interfaces)
				{
					if (@interface == iface)
					{
						return true;
					}
				}
			}
			if (TypeArguments != null)
			{
				TypeSpec[] typeArguments = TypeArguments;
				foreach (TypeSpec typeSpec in typeArguments)
				{
					TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
					if (typeParameterSpec != null)
					{
						if (typeParameterSpec.IsConvertibleToInterface(iface))
						{
							return true;
						}
					}
					else if (typeSpec.ImplementsInterface(iface, false))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool HasAnyTypeParameterTypeConstrained(IGenericMethodDefinition md)
		{
			TypeParameterSpec[] typeParameters = md.TypeParameters;
			for (int i = 0; i < md.TypeParametersCount; i++)
			{
				if (typeParameters[i].HasAnyTypeConstraint)
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasAnyTypeParameterConstrained(IGenericMethodDefinition md)
		{
			TypeParameterSpec[] typeParameters = md.TypeParameters;
			for (int i = 0; i < md.TypeParametersCount; i++)
			{
				if (typeParameters[i].IsConstrained)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasDependencyOn(TypeSpec type)
		{
			if (TypeArguments != null)
			{
				TypeSpec[] typeArguments = TypeArguments;
				foreach (TypeSpec typeSpec in typeArguments)
				{
					if (TypeSpecComparer.Override.IsEqual(typeSpec, type))
					{
						return true;
					}
					TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
					if (typeParameterSpec != null && typeParameterSpec.HasDependencyOn(type))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override TypeSpec Mutate(TypeParameterMutator mutator)
		{
			return mutator.Mutate(this);
		}
	}
}
