using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class GradientAlphaKeySurrogate : ISerializationSurrogate
	{
		public float alpha;

		public float time;

		public static implicit operator GradientAlphaKey(GradientAlphaKeySurrogate v)
		{
			GradientAlphaKey result = default(GradientAlphaKey);
			result.alpha = v.alpha;
			result.time = v.time;
			return result;
		}

		public static implicit operator GradientAlphaKeySurrogate(GradientAlphaKey v)
		{
			GradientAlphaKeySurrogate gradientAlphaKeySurrogate = new GradientAlphaKeySurrogate();
			gradientAlphaKeySurrogate.alpha = v.alpha;
			gradientAlphaKeySurrogate.time = v.time;
			return gradientAlphaKeySurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			GradientAlphaKey gradientAlphaKey = (GradientAlphaKey)obj;
			info.AddValue("alpha", gradientAlphaKey.alpha);
			info.AddValue("time", gradientAlphaKey.time);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			GradientAlphaKey gradientAlphaKey = (GradientAlphaKey)obj;
			gradientAlphaKey.alpha = (float)info.GetValue("alpha", typeof(float));
			gradientAlphaKey.time = (float)info.GetValue("time", typeof(float));
			return gradientAlphaKey;
		}
	}
}
