using System;
using System.IO;
using System.Text;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	internal sealed class ByteBuffer
	{
		private byte[] buffer;

		private int pos;

		private int __length;

		internal int Position
		{
			get
			{
				return pos;
			}
			set
			{
				if (value > Length || value > buffer.Length)
				{
					throw new ArgumentOutOfRangeException();
				}
				__length = Math.Max(__length, pos);
				pos = value;
			}
		}

		internal int Length => Math.Max(pos, __length);

		internal ByteBuffer(int initialCapacity)
		{
			buffer = new byte[initialCapacity];
		}

		private ByteBuffer(byte[] wrap, int length)
		{
			buffer = wrap;
			pos = length;
		}

		internal void Insert(int count)
		{
			if (count > 0)
			{
				int length = Length;
				int num = buffer.Length - length;
				if (num < count)
				{
					Grow(count - num);
				}
				Buffer.BlockCopy(buffer, pos, buffer, pos + count, length - pos);
				__length = Math.Max(__length, pos) + count;
			}
			else if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
		}

		private void Grow(int minGrow)
		{
			byte[] dst = new byte[Math.Max(buffer.Length + minGrow, buffer.Length * 2)];
			Buffer.BlockCopy(buffer, 0, dst, 0, buffer.Length);
			buffer = dst;
		}

		internal int GetInt32AtCurrentPosition()
		{
			return buffer[pos] + (buffer[pos + 1] << 8) + (buffer[pos + 2] << 16) + (buffer[pos + 3] << 24);
		}

		internal byte GetByteAtCurrentPosition()
		{
			return buffer[pos];
		}

		internal int GetCompressedUIntLength()
		{
			switch (buffer[pos] & 0xC0)
			{
			default:
				return 1;
			case 128:
				return 2;
			case 192:
				return 4;
			}
		}

		internal void Write(byte[] value)
		{
			if (pos + value.Length > buffer.Length)
			{
				Grow(value.Length);
			}
			Buffer.BlockCopy(value, 0, buffer, pos, value.Length);
			pos += value.Length;
		}

		internal void Write(byte value)
		{
			if (pos == buffer.Length)
			{
				Grow(1);
			}
			buffer[pos++] = value;
		}

		internal void Write(sbyte value)
		{
			Write((byte)value);
		}

		internal void Write(ushort value)
		{
			Write((short)value);
		}

		internal void Write(short value)
		{
			if (pos + 2 > buffer.Length)
			{
				Grow(2);
			}
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
		}

		internal void Write(uint value)
		{
			Write((int)value);
		}

		internal void Write(int value)
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

		internal void Write(ulong value)
		{
			Write((long)value);
		}

		internal void Write(long value)
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

		internal void Write(float value)
		{
			Write(SingleConverter.SingleToInt32Bits(value));
		}

		internal void Write(double value)
		{
			Write(BitConverter.DoubleToInt64Bits(value));
		}

		internal void Write(string str)
		{
			if (str == null)
			{
				Write(byte.MaxValue);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			WriteCompressedUInt(bytes.Length);
			Write(bytes);
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

		internal void WriteCompressedInt(int value)
		{
			if (value >= 0)
			{
				WriteCompressedUInt(value << 1);
			}
			else if (value >= -64)
			{
				value = ((value << 1) & 0x7F) | 1;
				Write((byte)value);
			}
			else if (value >= -8192)
			{
				value = ((value << 1) & 0x3FFF) | 1;
				Write((byte)(0x80 | (value >> 8)));
				Write((byte)value);
			}
			else
			{
				value = ((value << 1) & 0x1FFFFFFF) | 1;
				Write((byte)(0xC0 | (value >> 24)));
				Write((byte)(value >> 16));
				Write((byte)(value >> 8));
				Write((byte)value);
			}
		}

		internal void Write(ByteBuffer bb)
		{
			if (pos + bb.Length > buffer.Length)
			{
				Grow(bb.Length);
			}
			Buffer.BlockCopy(bb.buffer, 0, buffer, pos, bb.Length);
			pos += bb.Length;
		}

		internal void WriteTo(Stream stream)
		{
			stream.Write(buffer, 0, Length);
		}

		internal void Clear()
		{
			pos = 0;
			__length = 0;
		}

		internal void Align(int alignment)
		{
			if (pos + alignment > buffer.Length)
			{
				Grow(alignment);
			}
			int num = (pos + alignment - 1) & ~(alignment - 1);
			while (pos < num)
			{
				buffer[pos++] = 0;
			}
		}

		internal void WriteTypeDefOrRefEncoded(int token)
		{
			switch (token >> 24)
			{
			case 2:
				WriteCompressedUInt(((token & 0xFFFFFF) << 2) | 0);
				break;
			case 1:
				WriteCompressedUInt(((token & 0xFFFFFF) << 2) | 1);
				break;
			case 27:
				WriteCompressedUInt(((token & 0xFFFFFF) << 2) | 2);
				break;
			default:
				throw new InvalidOperationException();
			}
		}

		internal byte[] ToArray()
		{
			int length = Length;
			byte[] array = new byte[length];
			Buffer.BlockCopy(buffer, 0, array, 0, length);
			return array;
		}

		internal static ByteBuffer Wrap(byte[] buf)
		{
			return new ByteBuffer(buf, buf.Length);
		}

		internal static ByteBuffer Wrap(byte[] buf, int length)
		{
			return new ByteBuffer(buf, length);
		}

		internal bool Match(int pos, ByteBuffer bb2, int pos2, int len)
		{
			for (int i = 0; i < len; i++)
			{
				if (buffer[pos + i] != bb2.buffer[pos2 + i])
				{
					return false;
				}
			}
			return true;
		}

		internal int Hash()
		{
			int num = 0;
			int length = Length;
			for (int i = 0; i < length; i++)
			{
				num *= 37;
				num ^= buffer[i];
			}
			return num;
		}

		internal ByteReader GetBlob(int offset)
		{
			return ByteReader.FromBlob(buffer, offset);
		}
	}
}
