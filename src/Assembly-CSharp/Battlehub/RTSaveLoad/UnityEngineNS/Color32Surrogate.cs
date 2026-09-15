using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class Color32Surrogate : ISerializationSurrogate
	{
		public byte r;

		public byte g;

		public byte b;

		public byte a;

		public static implicit operator Color32(Color32Surrogate v)
		{
			Color32 result = default(Color32);
			result.r = v.r;
			result.g = v.g;
			result.b = v.b;
			result.a = v.a;
			return result;
		}

		public static implicit operator Color32Surrogate(Color32 v)
		{
			Color32Surrogate color32Surrogate = new Color32Surrogate();
			color32Surrogate.r = v.r;
			color32Surrogate.g = v.g;
			color32Surrogate.b = v.b;
			color32Surrogate.a = v.a;
			return color32Surrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Color32 color = (Color32)obj;
			info.AddValue("r", color.r);
			info.AddValue("g", color.g);
			info.AddValue("b", color.b);
			info.AddValue("a", color.a);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Color32 color = (Color32)obj;
			color.r = (byte)info.GetValue("r", typeof(byte));
			color.g = (byte)info.GetValue("g", typeof(byte));
			color.b = (byte)info.GetValue("b", typeof(byte));
			color.a = (byte)info.GetValue("a", typeof(byte));
			return color;
		}
	}
}
