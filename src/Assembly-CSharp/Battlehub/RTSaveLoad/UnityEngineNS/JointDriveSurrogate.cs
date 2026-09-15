using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class JointDriveSurrogate : ISerializationSurrogate
	{
		public float positionSpring;

		public float positionDamper;

		public float maximumForce;

		public static implicit operator JointDrive(JointDriveSurrogate v)
		{
			JointDrive result = default(JointDrive);
			result.positionSpring = v.positionSpring;
			result.positionDamper = v.positionDamper;
			result.maximumForce = v.maximumForce;
			return result;
		}

		public static implicit operator JointDriveSurrogate(JointDrive v)
		{
			JointDriveSurrogate jointDriveSurrogate = new JointDriveSurrogate();
			jointDriveSurrogate.positionSpring = v.positionSpring;
			jointDriveSurrogate.positionDamper = v.positionDamper;
			jointDriveSurrogate.maximumForce = v.maximumForce;
			return jointDriveSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			JointDrive jointDrive = (JointDrive)obj;
			info.AddValue("positionSpring", jointDrive.positionSpring);
			info.AddValue("positionDamper", jointDrive.positionDamper);
			info.AddValue("maximumForce", jointDrive.maximumForce);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			JointDrive jointDrive = (JointDrive)obj;
			jointDrive.positionSpring = (float)info.GetValue("positionSpring", typeof(float));
			jointDrive.positionDamper = (float)info.GetValue("positionDamper", typeof(float));
			jointDrive.maximumForce = (float)info.GetValue("maximumForce", typeof(float));
			return jointDrive;
		}
	}
}
