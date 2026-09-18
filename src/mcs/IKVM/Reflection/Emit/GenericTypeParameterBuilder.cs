using System;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class GenericTypeParameterBuilder : TypeInfo
	{
		private readonly string name;

		private readonly TypeBuilder type;

		private readonly MethodBuilder method;

		private readonly int paramPseudoIndex;

		private readonly int position;

		private int typeToken;

		private Type baseType;

		private GenericParameterAttributes attr;

		public override string AssemblyQualifiedName => null;

		public override bool IsValueType => (GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

		public override Type BaseType => baseType;

		public override TypeAttributes Attributes => TypeAttributes.Public;

		public override string Namespace => DeclaringType.Namespace;

		public override string Name => name;

		public override string FullName => null;

		private ModuleBuilder ModuleBuilder
		{
			get
			{
				if (!(type != null))
				{
					return method.ModuleBuilder;
				}
				return type.ModuleBuilder;
			}
		}

		public override Module Module => ModuleBuilder;

		public override int GenericParameterPosition => position;

		public override Type DeclaringType => type;

		public override MethodBase DeclaringMethod => method;

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				CheckBaked();
				return attr;
			}
		}

		public override int MetadataToken
		{
			get
			{
				CheckBaked();
				return 0x2A000000 | paramPseudoIndex;
			}
		}

		internal override bool IsBaked => ((MemberInfo)(((object)type) ?? ((object)method))).IsBaked;

		internal GenericTypeParameterBuilder(string name, TypeBuilder type, int position)
			: this(name, type, null, position, 19)
		{
		}

		internal GenericTypeParameterBuilder(string name, MethodBuilder method, int position)
			: this(name, null, method, position, 30)
		{
		}

		private GenericTypeParameterBuilder(string name, TypeBuilder type, MethodBuilder method, int position, byte sigElementType)
			: base(sigElementType)
		{
			this.name = name;
			this.type = type;
			this.method = method;
			this.position = position;
			GenericParamTable.Record newRecord = new GenericParamTable.Record
			{
				Number = (short)position,
				Flags = 0,
				Owner = ((type != null) ? type.MetadataToken : method.MetadataToken),
				Name = ModuleBuilder.Strings.Add(name)
			};
			paramPseudoIndex = ModuleBuilder.GenericParam.AddRecord(newRecord);
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return Name;
		}

		public override Type[] GetGenericParameterConstraints()
		{
			throw new NotImplementedException();
		}

		public override CustomModifiers[] __GetGenericParameterConstraintCustomModifiers()
		{
			throw new NotImplementedException();
		}

		internal override void CheckBaked()
		{
			if (type != null)
			{
				type.CheckBaked();
			}
			else
			{
				method.CheckBaked();
			}
		}

		private void AddConstraint(Type type)
		{
			GenericParamConstraintTable.Record newRecord = new GenericParamConstraintTable.Record
			{
				Owner = paramPseudoIndex,
				Constraint = ModuleBuilder.GetTypeTokenForMemberRef(type)
			};
			ModuleBuilder.GenericParamConstraint.AddRecord(newRecord);
		}

		public void SetBaseTypeConstraint(Type baseTypeConstraint)
		{
			baseType = baseTypeConstraint;
			AddConstraint(baseTypeConstraint);
		}

		public void SetInterfaceConstraints(params Type[] interfaceConstraints)
		{
			foreach (Type type in interfaceConstraints)
			{
				AddConstraint(type);
			}
		}

		public void SetGenericParameterAttributes(GenericParameterAttributes genericParameterAttributes)
		{
			attr = genericParameterAttributes;
			ModuleBuilder.GenericParam.PatchAttribute(paramPseudoIndex, genericParameterAttributes);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			ModuleBuilder.SetCustomAttribute(0x2A000000 | paramPseudoIndex, customBuilder);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		internal override int GetModuleBuilderToken()
		{
			if (typeToken == 0)
			{
				ByteBuffer bb = new ByteBuffer(5);
				Signature.WriteTypeSpec(ModuleBuilder, bb, this);
				typeToken = 0x1B000000 | ModuleBuilder.TypeSpec.AddRecord(ModuleBuilder.Blobs.Add(bb));
			}
			return typeToken;
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			if (type != null)
			{
				return binder.BindTypeParameter(this);
			}
			return binder.BindMethodParameter(this);
		}

		internal override int GetCurrentToken()
		{
			if (ModuleBuilder.IsSaved)
			{
				return 0x2A000000 | (Module.GenericParam.GetIndexFixup()[paramPseudoIndex - 1] + 1);
			}
			return 0x2A000000 | paramPseudoIndex;
		}
	}
}
