using System;
using System.Text;

namespace Mono.CSharp
{
	public sealed class AttributeEncoder
	{
		[Flags]
		public enum EncodedTypeProperties
		{
			None = 0,
			DynamicType = 1,
			TypeParameter = 2
		}

		public static readonly byte[] Empty;

		private byte[] buffer;

		private int pos;

		private const ushort Version = 1;

		static AttributeEncoder()
		{
			Empty = new byte[4];
			Empty[0] = 1;
		}

		public AttributeEncoder()
		{
			buffer = new byte[32];
			Encode((ushort)1);
		}

		public void Encode(bool value)
		{
			Encode((byte)(value ? 1 : 0));
		}

		public void Encode(byte value)
		{
			if (pos == buffer.Length)
			{
				Grow(1);
			}
			buffer[pos++] = value;
		}

		public void Encode(sbyte value)
		{
			Encode((byte)value);
		}

		public void Encode(short value)
		{
			if (pos + 2 > buffer.Length)
			{
				Grow(2);
			}
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
		}

		public void Encode(ushort value)
		{
			Encode((short)value);
		}

		public void Encode(int value)
		{
			if (pos + 4 > buffer.Length)
			{
				Grow(4);
			}
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
			buffer[pos++] = (byte)(value >> 16);
			buffer[pos++] = (byte)(value >> 24);
		}

		public void Encode(uint value)
		{
			Encode((int)value);
		}

		public void Encode(long value)
		{
			if (pos + 8 > buffer.Length)
			{
				Grow(8);
			}
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
			buffer[pos++] = (byte)(value >> 16);
			buffer[pos++] = (byte)(value >> 24);
			buffer[pos++] = (byte)(value >> 32);
			buffer[pos++] = (byte)(value >> 40);
			buffer[pos++] = (byte)(value >> 48);
			buffer[pos++] = (byte)(value >> 56);
		}

		public void Encode(ulong value)
		{
			Encode((long)value);
		}

		public void Encode(float value)
		{
			Encode(SingleConverter.SingleToInt32Bits(value));
		}

		public void Encode(double value)
		{
			Encode(BitConverter.DoubleToInt64Bits(value));
		}

		public void Encode(string value)
		{
			if (value == null)
			{
				Encode(byte.MaxValue);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			WriteCompressedValue(bytes.Length);
			if (pos + bytes.Length > buffer.Length)
			{
				Grow(bytes.Length);
			}
			Buffer.BlockCopy(bytes, 0, buffer, pos, bytes.Length);
			pos += bytes.Length;
		}

		public EncodedTypeProperties Encode(TypeSpec type)
		{
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
				Encode((byte)2);
				break;
			case BuiltinTypeSpec.Type.Char:
				Encode((byte)3);
				break;
			case BuiltinTypeSpec.Type.SByte:
				Encode((byte)4);
				break;
			case BuiltinTypeSpec.Type.Byte:
				Encode((byte)5);
				break;
			case BuiltinTypeSpec.Type.Short:
				Encode((byte)6);
				break;
			case BuiltinTypeSpec.Type.UShort:
				Encode((byte)7);
				break;
			case BuiltinTypeSpec.Type.Int:
				Encode((byte)8);
				break;
			case BuiltinTypeSpec.Type.UInt:
				Encode((byte)9);
				break;
			case BuiltinTypeSpec.Type.Long:
				Encode((byte)10);
				break;
			case BuiltinTypeSpec.Type.ULong:
				Encode((byte)11);
				break;
			case BuiltinTypeSpec.Type.Float:
				Encode((byte)12);
				break;
			case BuiltinTypeSpec.Type.Double:
				Encode((byte)13);
				break;
			case BuiltinTypeSpec.Type.String:
				Encode((byte)14);
				break;
			case BuiltinTypeSpec.Type.Type:
				Encode((byte)80);
				break;
			case BuiltinTypeSpec.Type.Object:
				Encode((byte)81);
				break;
			case BuiltinTypeSpec.Type.Dynamic:
				Encode((byte)81);
				return EncodedTypeProperties.DynamicType;
			default:
				if (type.IsArray)
				{
					Encode((byte)29);
					return Encode(TypeManager.GetElementType(type));
				}
				if (type.Kind == MemberKind.Enum)
				{
					Encode((byte)85);
					EncodeTypeName(type);
				}
				break;
			}
			return EncodedTypeProperties.None;
		}

		public void EncodeTypeName(TypeSpec type)
		{
			Type metaInfo = type.GetMetaInfo();
			Encode(type.MemberDefinition.IsImported ? metaInfo.AssemblyQualifiedName : metaInfo.FullName);
		}

		public void EncodeTypeName(TypeContainer type)
		{
			Encode(type.GetSignatureForMetadata());
		}

		public void EncodeNamedPropertyArgument(PropertySpec property, Constant value)
		{
			Encode((ushort)1);
			Encode((byte)84);
			Encode(property.MemberType);
			Encode(property.Name);
			value.EncodeAttributeValue(null, this, property.MemberType, property.MemberType);
		}

		public void EncodeNamedFieldArgument(FieldSpec field, Constant value)
		{
			Encode((ushort)1);
			Encode((byte)83);
			Encode(field.MemberType);
			Encode(field.Name);
			value.EncodeAttributeValue(null, this, field.MemberType, field.MemberType);
		}

		public void EncodeNamedArguments<T>(T[] members, Constant[] values) where T : MemberSpec, IInterfaceMemberSpec
		{
			Encode((ushort)members.Length);
			for (int i = 0; i < members.Length; i++)
			{
				T val = members[i];
				if (val.Kind == MemberKind.Field)
				{
					Encode((byte)83);
				}
				else
				{
					if (val.Kind != MemberKind.Property)
					{
						throw new NotImplementedException(val.Kind.ToString());
					}
					Encode((byte)84);
				}
				Encode(val.MemberType);
				Encode(val.Name);
				values[i].EncodeAttributeValue(null, this, val.MemberType, val.MemberType);
			}
		}

		public void EncodeEmptyNamedArguments()
		{
			Encode((ushort)0);
		}

		private void Grow(int inc)
		{
			int newSize = System.Math.Max(pos * 4, pos + inc + 2);
			Array.Resize(ref buffer, newSize);
		}

		private void WriteCompressedValue(int value)
		{
			if (value < 128)
			{
				Encode((byte)value);
			}
			else if (value < 16384)
			{
				Encode((byte)(0x80 | (value >> 8)));
				Encode((byte)value);
			}
			else
			{
				Encode(value);
			}
		}

		public byte[] ToArray()
		{
			byte[] array = new byte[pos];
			Array.Copy(buffer, array, pos);
			return array;
		}
	}
}
