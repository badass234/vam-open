using System;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class FieldBuilder : FieldInfo
	{
		private readonly TypeBuilder typeBuilder;

		private readonly string name;

		private readonly int pseudoToken;

		private FieldAttributes attribs;

		private readonly int nameIndex;

		private readonly int signature;

		private readonly FieldSignature fieldSig;

		public override int __FieldRVA
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override FieldAttributes Attributes => attribs;

		public override Type DeclaringType
		{
			get
			{
				if (!typeBuilder.IsModulePseudoType)
				{
					return typeBuilder;
				}
				return null;
			}
		}

		public override string Name => name;

		public override int MetadataToken => pseudoToken;

		public override Module Module => typeBuilder.Module;

		internal override FieldSignature FieldSignature => fieldSig;

		internal override bool IsBaked => typeBuilder.IsBaked;

		internal FieldBuilder(TypeBuilder type, string name, Type fieldType, CustomModifiers customModifiers, FieldAttributes attribs)
		{
			typeBuilder = type;
			this.name = name;
			pseudoToken = type.ModuleBuilder.AllocPseudoToken();
			nameIndex = type.ModuleBuilder.Strings.Add(name);
			fieldSig = FieldSignature.Create(fieldType, customModifiers);
			ByteBuffer bb = new ByteBuffer(5);
			fieldSig.WriteSig(typeBuilder.ModuleBuilder, bb);
			signature = typeBuilder.ModuleBuilder.Blobs.Add(bb);
			this.attribs = attribs;
			typeBuilder.ModuleBuilder.Field.AddVirtualRecord();
		}

		public void SetConstant(object defaultValue)
		{
			attribs |= FieldAttributes.HasDefault;
			typeBuilder.ModuleBuilder.AddConstant(pseudoToken, defaultValue);
		}

		public override object GetRawConstantValue()
		{
			if (!typeBuilder.IsCreated())
			{
				throw new NotSupportedException();
			}
			return typeBuilder.Module.Constant.GetRawConstantValue(typeBuilder.Module, GetCurrentToken());
		}

		public void __SetDataAndRVA(byte[] data)
		{
			SetDataAndRvaImpl(data, typeBuilder.ModuleBuilder.initializedData, 0);
		}

		public void __SetReadOnlyDataAndRVA(byte[] data)
		{
			SetDataAndRvaImpl(data, typeBuilder.ModuleBuilder.methodBodies, int.MinValue);
		}

		private void SetDataAndRvaImpl(byte[] data, ByteBuffer bb, int readonlyMarker)
		{
			attribs |= FieldAttributes.HasFieldRVA;
			FieldRVATable.Record newRecord = default(FieldRVATable.Record);
			bb.Align(8);
			newRecord.RVA = bb.Position + readonlyMarker;
			newRecord.Field = pseudoToken;
			typeBuilder.ModuleBuilder.FieldRVA.AddRecord(newRecord);
			bb.Write(data);
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			throw new NotImplementedException();
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			int token = pseudoToken;
			if (typeBuilder.ModuleBuilder.IsSaved)
			{
				token = typeBuilder.ModuleBuilder.ResolvePseudoToken(pseudoToken) & 0xFFFFFF;
			}
			SortedTable<FieldLayoutTable.Record>.Enumerator enumerator = Module.FieldLayout.Filter(token).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				offset = Module.FieldLayout.records[current].Offset;
				return true;
			}
			offset = 0;
			return false;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			switch (customBuilder.KnownCA)
			{
			case KnownCA.FieldOffsetAttribute:
				SetOffset((int)customBuilder.DecodeBlob(Module.Assembly).GetConstructorArgument(0));
				break;
			case KnownCA.MarshalAsAttribute:
				FieldMarshal.SetMarshalAsAttribute(typeBuilder.ModuleBuilder, pseudoToken, customBuilder);
				attribs |= FieldAttributes.HasFieldMarshal;
				break;
			case KnownCA.NonSerializedAttribute:
				attribs |= FieldAttributes.NotSerialized;
				break;
			case KnownCA.SpecialNameAttribute:
				attribs |= FieldAttributes.SpecialName;
				break;
			default:
				typeBuilder.ModuleBuilder.SetCustomAttribute(pseudoToken, customBuilder);
				break;
			}
		}

		public void SetOffset(int iOffset)
		{
			FieldLayoutTable.Record newRecord = new FieldLayoutTable.Record
			{
				Offset = iOffset,
				Field = pseudoToken
			};
			typeBuilder.ModuleBuilder.FieldLayout.AddRecord(newRecord);
		}

		public FieldToken GetToken()
		{
			return new FieldToken(pseudoToken);
		}

		internal void WriteFieldRecords(MetadataWriter mw)
		{
			mw.Write((short)attribs);
			mw.WriteStringIndex(nameIndex);
			mw.WriteBlobIndex(signature);
		}

		internal void FixupToken(int token)
		{
			typeBuilder.ModuleBuilder.RegisterTokenFixup(pseudoToken, token);
		}

		internal override int ImportTo(ModuleBuilder other)
		{
			return other.ImportMethodOrField(typeBuilder, name, fieldSig);
		}

		internal override int GetCurrentToken()
		{
			if (typeBuilder.ModuleBuilder.IsSaved)
			{
				return typeBuilder.ModuleBuilder.ResolvePseudoToken(pseudoToken);
			}
			return pseudoToken;
		}
	}
}
