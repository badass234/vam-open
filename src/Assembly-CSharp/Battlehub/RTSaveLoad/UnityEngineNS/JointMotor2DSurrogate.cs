using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointMotor2DSurrogate : ISerializationSurrogate
	{
		public float motorSpeed;

		public float maxMotorTorque;

		public static implicit operator JointMotor2D(JointMotor2DSurrogate v)
		{
			JointMotor2D result = default(JointMotor2D);
			result.motorSpeed = v.motorSpeed;
			result.maxMotorTorque = v.maxMotorTorque;
			return result;
		}

		public static implicit operator JointMotor2DSurrogate(JointMotor2D v)
		{
			JointMotor2DSurrogate jointMotor2DSurrogate = new JointMotor2DSurrogate();
			jointMotor2DSurrogate.motorSpeed = v.motorSpeed;
			jointMotor2DSurrogate.maxMotorTorque = v.maxMotorTorque;
			return jointMotor2DSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointMotor2D jointMotor2D = (JointMotor2D)obj;
			info.AddValue("motorSpeed", jointMotor2D.motorSpeed);
			info.AddValue("maxMotorTorque", jointMotor2D.maxMotorTorque);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointMotor2D jointMotor2D = (JointMotor2D)obj;
			jointMotor2D.motorSpeed = (float)info.GetValue("motorSpeed", typeof(float));
			jointMotor2D.maxMotorTorque = (float)info.GetValue("maxMotorTorque", typeof(float));
			return jointMotor2D;
		}
	}
}
