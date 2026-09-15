using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ColorSurrogate : ISerializationSurrogate
	{
		public float r;

		public float g;

		public float b;

		public float a;

		public static implicit operator Color(ColorSurrogate v)
		{
			Color result = default(Color);
			result.r = v.r;
			result.g = v.g;
			result.b = v.b;
			result.a = v.a;
			return result;
		}

		public static implicit operator ColorSurrogate(Color v)
		{
			ColorSurrogate colorSurrogate = new ColorSurrogate();
			colorSurrogate.r = v.r;
			colorSurrogate.g = v.g;
			colorSurrogate.b = v.b;
			colorSurrogate.a = v.a;
			return colorSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Color color = (Color)obj;
			info.AddValue("r", color.r);
			info.AddValue("g", color.g);
			info.AddValue("b", color.b);
			info.AddValue("a", color.a);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Color color = (Color)obj;
			color.r = (float)info.GetValue("r", typeof(float));
			color.g = (float)info.GetValue("g", typeof(float));
			color.b = (float)info.GetValue("b", typeof(float));
			color.a = (float)info.GetValue("a", typeof(float));
			return color;
		}
	}
}
