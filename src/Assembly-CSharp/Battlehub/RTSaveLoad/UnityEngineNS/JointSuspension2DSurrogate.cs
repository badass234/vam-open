using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointSuspension2DSurrogate : ISerializationSurrogate
	{
		public float dampingRatio;

		public float frequency;

		public float angle;

		public static implicit operator JointSuspension2D(JointSuspension2DSurrogate v)
		{
			JointSuspension2D result = default(JointSuspension2D);
			result.dampingRatio = v.dampingRatio;
			result.frequency = v.frequency;
			result.angle = v.angle;
			return result;
		}

		public static implicit operator JointSuspension2DSurrogate(JointSuspension2D v)
		{
			JointSuspension2DSurrogate jointSuspension2DSurrogate = new JointSuspension2DSurrogate();
			jointSuspension2DSurrogate.dampingRatio = v.dampingRatio;
			jointSuspension2DSurrogate.frequency = v.frequency;
			jointSuspension2DSurrogate.angle = v.angle;
			return jointSuspension2DSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointSuspension2D jointSuspension2D = (JointSuspension2D)obj;
			info.AddValue("dampingRatio", jointSuspension2D.dampingRatio);
			info.AddValue("frequency", jointSuspension2D.frequency);
			info.AddValue("angle", jointSuspension2D.angle);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointSuspension2D jointSuspension2D = (JointSuspension2D)obj;
			jointSuspension2D.dampingRatio = (float)info.GetValue("dampingRatio", typeof(float));
			jointSuspension2D.frequency = (float)info.GetValue("frequency", typeof(float));
			jointSuspension2D.angle = (float)info.GetValue("angle", typeof(float));
			return jointSuspension2D;
		}
	}
}
