using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointLimitsSurrogate : ISerializationSurrogate
	{
		public float min;

		public float max;

		public float bounciness;

		public float bounceMinVelocity;

		public float contactDistance;

		public static implicit operator JointLimits(JointLimitsSurrogate v)
		{
			JointLimits result = default(JointLimits);
			result.min = v.min;
			result.max = v.max;
			result.bounciness = v.bounciness;
			result.bounceMinVelocity = v.bounceMinVelocity;
			result.contactDistance = v.contactDistance;
			return result;
		}

		public static implicit operator JointLimitsSurrogate(JointLimits v)
		{
			JointLimitsSurrogate jointLimitsSurrogate = new JointLimitsSurrogate();
			jointLimitsSurrogate.min = v.min;
			jointLimitsSurrogate.max = v.max;
			jointLimitsSurrogate.bounciness = v.bounciness;
			jointLimitsSurrogate.bounceMinVelocity = v.bounceMinVelocity;
			jointLimitsSurrogate.contactDistance = v.contactDistance;
			return jointLimitsSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointLimits jointLimits = (JointLimits)obj;
			info.AddValue("min", jointLimits.min);
			info.AddValue("max", jointLimits.max);
			info.AddValue("bounciness", jointLimits.bounciness);
			info.AddValue("bounceMinVelocity", jointLimits.bounceMinVelocity);
			info.AddValue("contactDistance", jointLimits.contactDistance);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointLimits jointLimits = (JointLimits)obj;
			jointLimits.min = (float)info.GetValue("min", typeof(float));
			jointLimits.max = (float)info.GetValue("max", typeof(float));
			jointLimits.bounciness = (float)info.GetValue("bounciness", typeof(float));
			jointLimits.bounceMinVelocity = (float)info.GetValue("bounceMinVelocity", typeof(float));
			jointLimits.contactDistance = (float)info.GetValue("contactDistance", typeof(float));
			return jointLimits;
		}
	}
}
