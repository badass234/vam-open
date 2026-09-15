using System;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[ProtoContract]
	[ProtoInclude(101, typeof(PrimitiveContract<bool>))]
	[ProtoInclude(102, typeof(PrimitiveContract<char>))]
	[ProtoInclude(103, typeof(PrimitiveContract<byte>))]
	[ProtoInclude(104, typeof(PrimitiveContract<short>))]
	[ProtoInclude(105, typeof(PrimitiveContract<int>))]
	[ProtoInclude(106, typeof(PrimitiveContract<long>))]
	[ProtoInclude(107, typeof(PrimitiveContract<ushort>))]
	[ProtoInclude(108, typeof(PrimitiveContract<uint>))]
	[ProtoInclude(110, typeof(PrimitiveContract<ulong>))]
	[ProtoInclude(111, typeof(PrimitiveContract<string>))]
	[ProtoInclude(112, typeof(PrimitiveContract<float>))]
	[ProtoInclude(113, typeof(PrimitiveContract<double>))]
	[ProtoInclude(114, typeof(PrimitiveContract<decimal>))]
	[ProtoInclude(115, typeof(PrimitiveContract<bool[]>))]
	[ProtoInclude(116, typeof(PrimitiveContract<char[]>))]
	[ProtoInclude(117, typeof(PrimitiveContract<byte[]>))]
	[ProtoInclude(118, typeof(PrimitiveContract<short[]>))]
	[ProtoInclude(119, typeof(PrimitiveContract<int[]>))]
	[ProtoInclude(120, typeof(PrimitiveContract<long[]>))]
	[ProtoInclude(121, typeof(PrimitiveContract<ushort[]>))]
	[ProtoInclude(122, typeof(PrimitiveContract<uint[]>))]
	[ProtoInclude(123, typeof(PrimitiveContract<ulong[]>))]
	[ProtoInclude(124, typeof(PrimitiveContract<string[]>))]
	[ProtoInclude(125, typeof(PrimitiveContract<float[]>))]
	[ProtoInclude(126, typeof(PrimitiveContract<double[]>))]
	[ProtoInclude(127, typeof(PrimitiveContract<decimal[]>))]
	[ProtoInclude(128, typeof(PrimitiveContract<Color>))]
	[ProtoInclude(129, typeof(PrimitiveContract<Color[]>))]
	[ProtoInclude(130, typeof(PrimitiveContract<Vector3>))]
	[ProtoInclude(131, typeof(PrimitiveContract<Vector3[]>))]
	[ProtoInclude(132, typeof(PrimitiveContract<Vector4>))]
	[ProtoInclude(133, typeof(PrimitiveContract<Vector4[]>))]
	public abstract class PrimitiveContract
	{
		public object ValueBase
		{
			get
			{
				return ValueImpl;
			}
			set
			{
				ValueImpl = value;
			}
		}

		protected abstract object ValueImpl { get; set; }

		public static PrimitiveContract<T> Create<T>(T value)
		{
			return new PrimitiveContract<T>(value);
		}

		public static PrimitiveContract Create(Type type)
		{
			Type typeFromHandle = typeof(PrimitiveContract<>);
			Type type2 = typeFromHandle.MakeGenericType(type);
			return (PrimitiveContract)Activator.CreateInstance(type2);
		}
	}
	[ProtoContract]
	public class PrimitiveContract<T> : PrimitiveContract
	{
		[ProtoMember(1)]
		public T Value { get; set; }

		protected override object ValueImpl
		{
			get
			{
				return Value;
			}
			set
			{
				Value = (T)value;
			}
		}

		public PrimitiveContract()
		{
		}

		public PrimitiveContract(T value)
		{
			Value = value;
		}
	}
}
