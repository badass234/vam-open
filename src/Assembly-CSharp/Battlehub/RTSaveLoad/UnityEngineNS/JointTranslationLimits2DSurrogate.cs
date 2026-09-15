using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointTranslationLimits2DSurrogate : ISerializationSurrogate
	{
		public float min;

		public float max;

		public static implicit operator JointTranslationLimits2D(JointTranslationLimits2DSurrogate v)
		{
			JointTranslationLimits2D result = default(JointTranslationLimits2D);
			result.min = v.min;
			result.max = v.max;
			return result;
		}

		public static implicit operator JointTranslationLimits2DSurrogate(JointTranslationLimits2D v)
		{
			JointTranslationLimits2DSurrogate jointTranslationLimits2DSurrogate = new JointTranslationLimits2DSurrogate();
			jointTranslationLimits2DSurrogate.min = v.min;
			jointTranslationLimits2DSurrogate.max = v.max;
			return jointTranslationLimits2DSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointTranslationLimits2D jointTranslationLimits2D = (JointTranslationLimits2D)obj;
			info.AddValue("min", jointTranslationLimits2D.min);
			info.AddValue("max", jointTranslationLimits2D.max);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointTranslationLimits2D jointTranslationLimits2D = (JointTranslationLimits2D)obj;
			jointTranslationLimits2D.min = (float)info.GetValue("min", typeof(float));
			jointTranslationLimits2D.max = (float)info.GetValue("max", typeof(float));
			return jointTranslationLimits2D;
		}
	}
}
