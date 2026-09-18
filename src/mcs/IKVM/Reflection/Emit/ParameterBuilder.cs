using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class ParameterBuilder
	{
		private readonly ModuleBuilder moduleBuilder;

		private short flags;

		private readonly short sequence;

		private readonly int nameIndex;

		private readonly string name;

		private int lazyPseudoToken;

		internal int PseudoToken
		{
			get
			{
				if (lazyPseudoToken == 0)
				{
					lazyPseudoToken = moduleBuilder.AllocPseudoToken();
				}
				return lazyPseudoToken;
			}
		}

		public string Name => name;

		public int Position => sequence;

		public int Attributes => flags;

		public bool IsIn => (flags & 1) != 0;

		public bool IsOut => (flags & 2) != 0;

		public bool IsOptional => (flags & 0x10) != 0;

		internal ParameterBuilder(ModuleBuilder moduleBuilder, int sequence, ParameterAttributes attribs, string name)
		{
			this.moduleBuilder = moduleBuilder;
			flags = (short)attribs;
			this.sequence = (short)sequence;
			nameIndex = ((name != null) ? moduleBuilder.Strings.Add(name) : 0);
			this.name = name;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customAttributeBuilder)
		{
			switch (customAttributeBuilder.KnownCA)
			{
			case KnownCA.InAttribute:
				flags |= 1;
				break;
			case KnownCA.OutAttribute:
				flags |= 2;
				break;
			case KnownCA.OptionalAttribute:
				flags |= 16;
				break;
			case KnownCA.MarshalAsAttribute:
				FieldMarshal.SetMarshalAsAttribute(moduleBuilder, PseudoToken, customAttributeBuilder);
				flags |= 8192;
				break;
			default:
				moduleBuilder.SetCustomAttribute(PseudoToken, customAttributeBuilder);
				break;
			}
		}

		public void SetConstant(object defaultValue)
		{
			flags |= 4096;
			moduleBuilder.AddConstant(PseudoToken, defaultValue);
		}

		internal void WriteParamRecord(MetadataWriter mw)
		{
			mw.Write(flags);
			mw.Write(sequence);
			mw.WriteStringIndex(nameIndex);
		}

		internal void FixupToken(int parameterToken)
		{
			if (lazyPseudoToken != 0)
			{
				moduleBuilder.RegisterTokenFixup(lazyPseudoToken, parameterToken);
			}
		}
	}
}
