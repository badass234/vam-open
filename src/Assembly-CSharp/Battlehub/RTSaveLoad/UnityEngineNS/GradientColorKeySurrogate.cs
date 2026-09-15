using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class GradientColorKeySurrogate : ISerializationSurrogate
	{
		public Color color;

		public float time;

		public static implicit operator GradientColorKey(GradientColorKeySurrogate v)
		{
			GradientColorKey result = default(GradientColorKey);
			result.color = v.color;
			result.time = v.time;
			return result;
		}

		public static implicit operator GradientColorKeySurrogate(GradientColorKey v)
		{
			GradientColorKeySurrogate gradientColorKeySurrogate = new GradientColorKeySurrogate();
			gradientColorKeySurrogate.color = v.color;
			gradientColorKeySurrogate.time = v.time;
			return gradientColorKeySurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			GradientColorKey gradientColorKey = (GradientColorKey)obj;
			info.AddValue("color", gradientColorKey.color);
			info.AddValue("time", gradientColorKey.time);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			GradientColorKey gradientColorKey = (GradientColorKey)obj;
			gradientColorKey.color = (Color)info.GetValue("color", typeof(Color));
			gradientColorKey.time = (float)info.GetValue("time", typeof(float));
			return gradientColorKey;
		}
	}
}
