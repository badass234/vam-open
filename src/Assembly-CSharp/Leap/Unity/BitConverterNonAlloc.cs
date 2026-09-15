using System;
using System.Runtime.InteropServices;

namespace Leap.Unity
{
	public static class BitConverterNonAlloc
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct ConversionStruct
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(4)]
			public byte Byte4;

			[FieldOffset(5)]
			public byte Byte5;

			[FieldOffset(6)]
			public byte Byte6;

			[FieldOffset(7)]
			public byte Byte7;

			[FieldOffset(0)]
			public ushort UInt16;

			[FieldOffset(0)]
			public short Int16;

			[FieldOffset(0)]
			public uint UInt32;

			[FieldOffset(0)]
			public int Int32;

			[FieldOffset(0)]
			public ulong UInt64;

			[FieldOffset(0)]
			public long Int64;

			[FieldOffset(0)]
			public float Single;

			[FieldOffset(0)]
			public double Double;
		}

		[ThreadStatic]
		private static ConversionStruct _c;

		public static ushort ToUInt16(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			return _c.UInt16;
		}

		public static short ToInt16(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			return _c.Int16;
		}

		public static uint ToUInt32(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.UInt32;
		}

		public static int ToInt32(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.Int32;
		}

		public static ulong ToUInt64(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.UInt64;
		}

		public static long ToInt64(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.Int64;
		}

		public static float ToSingle(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.Single;
		}

		public static double ToDouble(byte[] bytes, int offset = 0)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.Double;
		}

		public static ushort ToUInt16(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			return _c.UInt16;
		}

		public static short ToInt16(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			return _c.Int16;
		}

		public static uint ToUInt32(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.UInt32;
		}

		public static int ToInt32(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.Int32;
		}

		public static ulong ToUInt64(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.UInt64;
		}

		public static long ToInt64(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.Int64;
		}

		public static float ToSingle(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			return _c.Single;
		}

		public static double ToDouble(byte[] bytes, ref int offset)
		{
			_c.Byte0 = bytes[offset++];
			_c.Byte1 = bytes[offset++];
			_c.Byte2 = bytes[offset++];
			_c.Byte3 = bytes[offset++];
			_c.Byte4 = bytes[offset++];
			_c.Byte5 = bytes[offset++];
			_c.Byte6 = bytes[offset++];
			_c.Byte7 = bytes[offset++];
			return _c.Double;
		}

		public static void GetBytes(ushort value, byte[] bytes, int offset = 0)
		{
			_c.UInt16 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
		}

		public static void GetBytes(short value, byte[] bytes, int offset = 0)
		{
			_c.Int16 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
		}

		public static void GetBytes(uint value, byte[] bytes, int offset = 0)
		{
			_c.UInt32 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(int value, byte[] bytes, int offset = 0)
		{
			_c.Int32 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(ulong value, byte[] bytes, int offset = 0)
		{
			_c.UInt64 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}

		public static void GetBytes(long value, byte[] bytes, int offset = 0)
		{
			_c.Int64 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}

		public static void GetBytes(float value, byte[] bytes, int offset = 0)
		{
			_c.Single = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(double value, byte[] bytes, int offset = 0)
		{
			_c.Double = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}

		public static void GetBytes(ushort value, byte[] bytes, ref int offset)
		{
			_c.UInt16 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
		}

		public static void GetBytes(short value, byte[] bytes, ref int offset)
		{
			_c.Int16 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
		}

		public static void GetBytes(uint value, byte[] bytes, ref int offset)
		{
			_c.UInt32 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(int value, byte[] bytes, ref int offset)
		{
			_c.Int32 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(ulong value, byte[] bytes, ref int offset)
		{
			_c.UInt64 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}

		public static void GetBytes(long value, byte[] bytes, ref int offset)
		{
			_c.Int64 = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}

		public static void GetBytes(float value, byte[] bytes, ref int offset)
		{
			_c.Single = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
		}

		public static void GetBytes(double value, byte[] bytes, ref int offset)
		{
			_c.Double = value;
			bytes[offset++] = _c.Byte0;
			bytes[offset++] = _c.Byte1;
			bytes[offset++] = _c.Byte2;
			bytes[offset++] = _c.Byte3;
			bytes[offset++] = _c.Byte4;
			bytes[offset++] = _c.Byte5;
			bytes[offset++] = _c.Byte6;
			bytes[offset++] = _c.Byte7;
		}
	}
}
