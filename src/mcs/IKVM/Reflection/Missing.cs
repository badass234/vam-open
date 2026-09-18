using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class Missing : ISerializable
	{
		[Serializable]
		private sealed class SingletonSerializationHelper : IObjectReference
		{
			public object GetRealObject(StreamingContext context)
			{
				return Value;
			}
		}

		public static readonly Missing Value = new Missing();

		private Missing()
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof(SingletonSerializationHelper));
		}
	}
}
