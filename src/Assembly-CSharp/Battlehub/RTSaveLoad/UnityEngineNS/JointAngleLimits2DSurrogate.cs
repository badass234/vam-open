using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointAngleLimits2DSurrogate : ISerializationSurrogate
	{
		public float min;

		public float max;

		public static implicit operator JointAngleLimits2D(JointAngleLimits2DSurrogate v)
		{
			JointAngleLimits2D result = default(JointAngleLimits2D);
			result.min = v.min;
			result.max = v.max;
			return result;
		}

		public static implicit operator JointAngleLimits2DSurrogate(JointAngleLimits2D v)
		{
			JointAngleLimits2DSurrogate jointAngleLimits2DSurrogate = new JointAngleLimits2DSurrogate();
			jointAngleLimits2DSurrogate.min = v.min;
			jointAngleLimits2DSurrogate.max = v.max;
			return jointAngleLimits2DSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointAngleLimits2D jointAngleLimits2D = (JointAngleLimits2D)obj;
			info.AddValue("min", jointAngleLimits2D.min);
			info.AddValue("max", jointAngleLimits2D.max);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointAngleLimits2D jointAngleLimits2D = (JointAngleLimits2D)obj;
			jointAngleLimits2D.min = (float)info.GetValue("min", typeof(float));
			jointAngleLimits2D.max = (float)info.GetValue("max", typeof(float));
			return jointAngleLimits2D;
		}
	}
}
