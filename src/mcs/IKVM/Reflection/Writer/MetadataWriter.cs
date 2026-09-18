using System;
using System.IO;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	internal sealed class MetadataWriter : MetadataRW
	{
		private readonly ModuleBuilder moduleBuilder;

		private readonly Stream stream;

		private readonly byte[] buffer = new byte[8];

		internal ModuleBuilder ModuleBuilder => moduleBuilder;

		internal int Position => (int)stream.Position;

		internal MetadataWriter(ModuleBuilder module, Stream stream)
			: base(module, module.Strings.IsBig, module.Guids.IsBig, module.Blobs.IsBig)
		{
			moduleBuilder = module;
			this.stream = stream;
		}

		internal void Write(ByteBuffer bb)
		{
			bb.WriteTo(stream);
		}

		internal void WriteAsciiz(string value)
		{
			foreach (char c in value)
			{
				stream.WriteByte((byte)c);
			}
			stream.WriteByte(0);
		}

		internal void Write(byte[] value)
		{
			stream.Write(value, 0, value.Length);
		}

		internal void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		internal void Write(byte value)
		{
			stream.WriteByte(value);
		}

		internal void Write(ushort value)
		{
			Write((short)value);
		}

		internal void Write(short value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
		}

		internal void Write(uint value)
		{
			Write((int)value);
		}

		internal void Write(int value)
		{
			buffer[0] = (byte)value;
			buffer[1] = (byte)(value >> 8);
			buffer[2] = (byte)(value >> 16);
			buffer[3] = (byte)(value >> 24);
			stream.Write(buffer, 0, 4);
		}

		internal void Write(ulong value)
		{
			Write((long)value);
		}

		internal void Write(long value)
		{
			buffer[0] = (byte)value;
			buffer[1] = (byte)(value >> 8);
			buffer[2] = (byte)(value >> 16);
			buffer[3] = (byte)(value >> 24);
			buffer[4] = (byte)(value >> 32);
			buffer[5] = (byte)(value >> 40);
			buffer[6] = (byte)(value >> 48);
			buffer[7] = (byte)(value >> 56);
			stream.Write(buffer, 0, 8);
		}

		internal void WriteCompressedUInt(int value)
		{
			if (value <= 127)
			{
				Write((byte)value);
				return;
			}
			if (value <= 16383)
			{
				Write((byte)(0x80 | (value >> 8)));
				Write((byte)value);
				return;
			}
			Write((byte)(0xC0 | (value >> 24)));
			Write((byte)(value >> 16));
			Write((byte)(value >> 8));
			Write((byte)value);
		}

		internal static int GetCompressedUIntLength(int value)
		{
			if (value <= 127)
			{
				return 1;
			}
			if (value <= 16383)
			{
				return 2;
			}
			return 4;
		}

		internal void WriteStringIndex(int index)
		{
			if (bigStrings)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteGuidIndex(int index)
		{
			if (bigGuids)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteBlobIndex(int index)
		{
			if (bigBlobs)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteTypeDefOrRef(int token)
		{
			switch (token >> 24)
			{
			case 2:
				token = ((token & 0xFFFFFF) << 2) | 0;
				break;
			case 1:
				token = ((token & 0xFFFFFF) << 2) | 1;
				break;
			case 27:
				token = ((token & 0xFFFFFF) << 2) | 2;
				break;
			default:
				throw new InvalidOperationException();
			case 0:
				break;
			}
			if (bigTypeDefOrRef)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteEncodedTypeDefOrRef(int encodedToken)
		{
			if (bigTypeDefOrRef)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteHasCustomAttribute(int token)
		{
			int num = CustomAttributeTable.EncodeHasCustomAttribute(token);
			if (bigHasCustomAttribute)
			{
				Write(num);
			}
			else
			{
				Write((short)num);
			}
		}

		internal void WriteCustomAttributeType(int token)
		{
			switch (token >> 24)
			{
			case 6:
				token = ((token & 0xFFFFFF) << 3) | 2;
				break;
			case 10:
				token = ((token & 0xFFFFFF) << 3) | 3;
				break;
			default:
				throw new InvalidOperationException();
			}
			if (bigCustomAttributeType)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteField(int index)
		{
			if (bigField)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteMethodDef(int index)
		{
			if (bigMethodDef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteParam(int index)
		{
			if (bigParam)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteTypeDef(int index)
		{
			if (bigTypeDef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteEvent(int index)
		{
			if (bigEvent)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteProperty(int index)
		{
			if (bigProperty)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteGenericParam(int index)
		{
			if (bigGenericParam)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteModuleRef(int index)
		{
			if (bigModuleRef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteResolutionScope(int token)
		{
			switch (token >> 24)
			{
			case 0:
				token = ((token & 0xFFFFFF) << 2) | 0;
				break;
			case 26:
				token = ((token & 0xFFFFFF) << 2) | 1;
				break;
			case 35:
				token = ((token & 0xFFFFFF) << 2) | 2;
				break;
			case 1:
				token = ((token & 0xFFFFFF) << 2) | 3;
				break;
			default:
				throw new InvalidOperationException();
			}
			if (bigResolutionScope)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteMemberRefParent(int token)
		{
			switch (token >> 24)
			{
			case 2:
				token = ((token & 0xFFFFFF) << 3) | 0;
				break;
			case 1:
				token = ((token & 0xFFFFFF) << 3) | 1;
				break;
			case 26:
				token = ((token & 0xFFFFFF) << 3) | 2;
				break;
			case 6:
				token = ((token & 0xFFFFFF) << 3) | 3;
				break;
			case 27:
				token = ((token & 0xFFFFFF) << 3) | 4;
				break;
			default:
				throw new InvalidOperationException();
			}
			if (bigMemberRefParent)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteMethodDefOrRef(int token)
		{
			switch (token >> 24)
			{
			case 6:
				token = ((token & 0xFFFFFF) << 1) | 0;
				break;
			case 10:
				token = ((token & 0xFFFFFF) << 1) | 1;
				break;
			default:
				throw new InvalidOperationException();
			}
			if (bigMethodDefOrRef)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteHasConstant(int token)
		{
			int num = ConstantTable.EncodeHasConstant(token);
			if (bigHasConstant)
			{
				Write(num);
			}
			else
			{
				Write((short)num);
			}
		}

		internal void WriteHasSemantics(int encodedToken)
		{
			if (bigHasSemantics)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteImplementation(int token)
		{
			switch (token >> 24)
			{
			case 38:
				token = ((token & 0xFFFFFF) << 2) | 0;
				break;
			case 35:
				token = ((token & 0xFFFFFF) << 2) | 1;
				break;
			case 39:
				token = ((token & 0xFFFFFF) << 2) | 2;
				break;
			default:
				throw new InvalidOperationException();
			case 0:
				break;
			}
			if (bigImplementation)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteTypeOrMethodDef(int encodedToken)
		{
			if (bigTypeOrMethodDef)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteHasDeclSecurity(int encodedToken)
		{
			if (bigHasDeclSecurity)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteMemberForwarded(int token)
		{
			switch (token >> 24)
			{
			case 4:
				token = ((token & 0xFFFFFF) << 1) | 0;
				break;
			case 6:
				token = ((token & 0xFFFFFF) << 1) | 1;
				break;
			default:
				throw new InvalidOperationException();
			}
			if (bigMemberForwarded)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteHasFieldMarshal(int token)
		{
			int num = FieldMarshalTable.EncodeHasFieldMarshal(token);
			if (bigHasFieldMarshal)
			{
				Write(num);
			}
			else
			{
				Write((short)num);
			}
		}
	}
}
