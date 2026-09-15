using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class SoftJointLimitSpringSurrogate : ISerializationSurrogate
	{
		public float spring;

		public float damper;

		public static implicit operator SoftJointLimitSpring(SoftJointLimitSpringSurrogate v)
		{
			SoftJointLimitSpring result = default(SoftJointLimitSpring);
			result.spring = v.spring;
			result.damper = v.damper;
			return result;
		}

		public static implicit operator SoftJointLimitSpringSurrogate(SoftJointLimitSpring v)
		{
			SoftJointLimitSpringSurrogate softJointLimitSpringSurrogate = new SoftJointLimitSpringSurrogate();
			softJointLimitSpringSurrogate.spring = v.spring;
			softJointLimitSpringSurrogate.damper = v.damper;
			return softJointLimitSpringSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			SoftJointLimitSpring softJointLimitSpring = (SoftJointLimitSpring)obj;
			info.AddValue("spring", softJointLimitSpring.spring);
			info.AddValue("damper", softJointLimitSpring.damper);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			SoftJointLimitSpring softJointLimitSpring = (SoftJointLimitSpring)obj;
			softJointLimitSpring.spring = (float)info.GetValue("spring", typeof(float));
			softJointLimitSpring.damper = (float)info.GetValue("damper", typeof(float));
			return softJointLimitSpring;
		}
	}
}
