using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class SoftJointLimitSurrogate : ISerializationSurrogate
	{
		public float limit;

		public float bounciness;

		public float contactDistance;

		public static implicit operator SoftJointLimit(SoftJointLimitSurrogate v)
		{
			SoftJointLimit result = default(SoftJointLimit);
			result.limit = v.limit;
			result.bounciness = v.bounciness;
			result.contactDistance = v.contactDistance;
			return result;
		}

		public static implicit operator SoftJointLimitSurrogate(SoftJointLimit v)
		{
			SoftJointLimitSurrogate softJointLimitSurrogate = new SoftJointLimitSurrogate();
			softJointLimitSurrogate.limit = v.limit;
			softJointLimitSurrogate.bounciness = v.bounciness;
			softJointLimitSurrogate.contactDistance = v.contactDistance;
			return softJointLimitSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			SoftJointLimit softJointLimit = (SoftJointLimit)obj;
			info.AddValue("limit", softJointLimit.limit);
			info.AddValue("bounciness", softJointLimit.bounciness);
			info.AddValue("contactDistance", softJointLimit.contactDistance);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			SoftJointLimit softJointLimit = (SoftJointLimit)obj;
			softJointLimit.limit = (float)info.GetValue("limit", typeof(float));
			softJointLimit.bounciness = (float)info.GetValue("bounciness", typeof(float));
			softJointLimit.contactDistance = (float)info.GetValue("contactDistance", typeof(float));
			return softJointLimit;
		}
	}
}
