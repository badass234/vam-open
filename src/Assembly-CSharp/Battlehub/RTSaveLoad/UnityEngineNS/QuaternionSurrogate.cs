using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class QuaternionSurrogate : ISerializationSurrogate
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public Vector3 eulerAngles;

		public static implicit operator Quaternion(QuaternionSurrogate v)
		{
			Quaternion result = default(Quaternion);
			result.x = v.x;
			result.y = v.y;
			result.z = v.z;
			result.w = v.w;
			result.eulerAngles = v.eulerAngles;
			return result;
		}

		public static implicit operator QuaternionSurrogate(Quaternion v)
		{
			QuaternionSurrogate quaternionSurrogate = new QuaternionSurrogate();
			quaternionSurrogate.x = v.x;
			quaternionSurrogate.y = v.y;
			quaternionSurrogate.z = v.z;
			quaternionSurrogate.w = v.w;
			quaternionSurrogate.eulerAngles = v.eulerAngles;
			return quaternionSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Quaternion quaternion = (Quaternion)obj;
			info.AddValue("x", quaternion.x);
			info.AddValue("y", quaternion.y);
			info.AddValue("z", quaternion.z);
			info.AddValue("w", quaternion.w);
			info.AddValue("eulerAngles", quaternion.eulerAngles);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Quaternion quaternion = (Quaternion)obj;
			quaternion.x = (float)info.GetValue("x", typeof(float));
			quaternion.y = (float)info.GetValue("y", typeof(float));
			quaternion.z = (float)info.GetValue("z", typeof(float));
			quaternion.w = (float)info.GetValue("w", typeof(float));
			quaternion.eulerAngles = (Vector3)info.GetValue("eulerAngles", typeof(Vector3));
			return quaternion;
		}
	}
}
