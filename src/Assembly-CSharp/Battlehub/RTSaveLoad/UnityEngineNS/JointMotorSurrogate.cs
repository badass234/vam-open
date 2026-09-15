using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointMotorSurrogate : ISerializationSurrogate
	{
		public float targetVelocity;

		public float force;

		public bool freeSpin;

		public static implicit operator JointMotor(JointMotorSurrogate v)
		{
			JointMotor result = default(JointMotor);
			result.targetVelocity = v.targetVelocity;
			result.force = v.force;
			result.freeSpin = v.freeSpin;
			return result;
		}

		public static implicit operator JointMotorSurrogate(JointMotor v)
		{
			JointMotorSurrogate jointMotorSurrogate = new JointMotorSurrogate();
			jointMotorSurrogate.targetVelocity = v.targetVelocity;
			jointMotorSurrogate.force = v.force;
			jointMotorSurrogate.freeSpin = v.freeSpin;
			return jointMotorSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointMotor jointMotor = (JointMotor)obj;
			info.AddValue("targetVelocity", jointMotor.targetVelocity);
			info.AddValue("force", jointMotor.force);
			info.AddValue("freeSpin", jointMotor.freeSpin);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointMotor jointMotor = (JointMotor)obj;
			jointMotor.targetVelocity = (float)info.GetValue("targetVelocity", typeof(float));
			jointMotor.force = (float)info.GetValue("force", typeof(float));
			jointMotor.freeSpin = (bool)info.GetValue("freeSpin", typeof(bool));
			return jointMotor;
		}
	}
}
