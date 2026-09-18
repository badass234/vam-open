using System;
using System.Text;

namespace IKVM.Reflection.Reader
{
	internal sealed class ByteReader
	{
		private byte[] buffer;

		private int pos;

		private int end;

		internal int Length => end - pos;

		internal ByteReader(byte[] buffer, int offset, int length)
		{
			this.buffer = buffer;
			pos = offset;
			end = pos + length;
		}

		internal static ByteReader FromBlob(byte[] blobHeap, int blob)
		{
			ByteReader byteReader = new ByteReader(blobHeap, blob, 4);
			int num = byteReader.ReadCompressedUInt();
			byteReader.end = byteReader.pos + num;
			return byteReader;
		}

		internal byte PeekByte()
		{
			if (pos == end)
			{
				throw new BadImageFormatException();
			}
			return buffer[pos];
		}

		internal byte ReadByte()
		{
			if (pos == end)
			{
				throw new BadImageFormatException();
			}
			return buffer[pos++];
		}

		internal byte[] ReadBytes(int count)
		{
			if (count < 0)
			{
				throw new BadImageFormatException();
			}
			if (end - pos < count)
			{
				throw new BadImageFormatException();
			}
			byte[] array = new byte[count];
			Buffer.BlockCopy(buffer, pos, array, 0, count);
			pos += count;
			return array;
		}

		internal int ReadCompressedUInt()
		{
			byte b = ReadByte();
			if (b <= 127)
			{
				return b;
			}
			if ((b & 0xC0) == 128)
			{
				byte b2 = ReadByte();
				return ((b & 0x3F) << 8) | b2;
			}
			byte b3 = ReadByte();
			byte b4 = ReadByte();
			byte b5 = ReadByte();
			return ((b & 0x3F) << 24) + (b3 << 16) + (b4 << 8) + b5;
		}

		internal int ReadCompressedInt()
		{
			byte b = PeekByte();
			int num = ReadCompressedUInt();
			if ((num & 1) == 0)
			{
				return num >> 1;
			}
			switch (b & 0xC0)
			{
			case 0:
			case 64:
				return (num >> 1) - 64;
			case 128:
				return (num >> 1) - 8192;
			default:
				return (num >> 1) - 268435456;
			}
		}

		internal string ReadString()
		{
			if (PeekByte() == byte.MaxValue)
			{
				pos++;
				return null;
			}
			int num = ReadCompressedUInt();
			string result = Encoding.UTF8.GetString(buffer, pos, num);
			pos += num;
			return result;
		}

		internal char ReadChar()
		{
			return (char)ReadInt16();
		}

		internal sbyte ReadSByte()
		{
			return (sbyte)ReadByte();
		}

		internal short ReadInt16()
		{
			if (end - pos < 2)
			{
				throw new BadImageFormatException();
			}
			byte num = buffer[pos++];
			byte b = buffer[pos++];
			return (short)(num | (b << 8));
		}

		internal ushort ReadUInt16()
		{
			return (ushort)ReadInt16();
		}

		internal int ReadInt32()
		{
			if (end - pos < 4)
			{
				throw new BadImageFormatException();
			}
			byte num = buffer[pos++];
			byte b = buffer[pos++];
			byte b2 = buffer[pos++];
			byte b3 = buffer[pos++];
			return num | (b << 8) | (b2 << 16) | (b3 << 24);
		}

		internal uint ReadUInt32()
		{
			return (uint)ReadInt32();
		}

		internal long ReadInt64()
		{
			long num = ReadUInt32();
			ulong num2 = ReadUInt32();
			return num | (long)(num2 << 32);
		}

		internal ulong ReadUInt64()
		{
			return (ulong)ReadInt64();
		}

		internal float ReadSingle()
		{
			return SingleConverter.Int32BitsToSingle(ReadInt32());
		}

		internal double ReadDouble()
		{
			return BitConverter.Int64BitsToDouble(ReadInt64());
		}

		internal ByteReader Slice(int length)
		{
			if (end - pos < length)
			{
				throw new BadImageFormatException();
			}
			ByteReader result = new ByteReader(buffer, pos, length);
			pos += length;
			return result;
		}

		internal void Align(int alignment)
		{
			alignment--;
			pos = (pos + alignment) & ~alignment;
		}
	}
}
