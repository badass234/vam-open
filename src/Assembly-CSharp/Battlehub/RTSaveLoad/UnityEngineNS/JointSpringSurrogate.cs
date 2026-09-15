using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointSpringSurrogate : ISerializationSurrogate
	{
		public float spring;

		public float damper;

		public float targetPosition;

		public static implicit operator JointSpring(JointSpringSurrogate v)
		{
			JointSpring result = default(JointSpring);
			result.spring = v.spring;
			result.damper = v.damper;
			result.targetPosition = v.targetPosition;
			return result;
		}

		public static implicit operator JointSpringSurrogate(JointSpring v)
		{
			JointSpringSurrogate jointSpringSurrogate = new JointSpringSurrogate();
			jointSpringSurrogate.spring = v.spring;
			jointSpringSurrogate.damper = v.damper;
			jointSpringSurrogate.targetPosition = v.targetPosition;
			return jointSpringSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointSpring jointSpring = (JointSpring)obj;
			info.AddValue("spring", jointSpring.spring);
			info.AddValue("damper", jointSpring.damper);
			info.AddValue("targetPosition", jointSpring.targetPosition);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointSpring jointSpring = (JointSpring)obj;
			jointSpring.spring = (float)info.GetValue("spring", typeof(float));
			jointSpring.damper = (float)info.GetValue("damper", typeof(float));
			jointSpring.targetPosition = (float)info.GetValue("targetPosition", typeof(float));
			return jointSpring;
		}
	}
}
