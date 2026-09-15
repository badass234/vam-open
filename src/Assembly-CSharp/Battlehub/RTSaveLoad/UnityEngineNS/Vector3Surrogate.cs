using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class Vector3Surrogate : ISerializationSurrogate
	{
		public float x;

		public float y;

		public float z;

		public static implicit operator Vector3(Vector3Surrogate v)
		{
			Vector3 result = default(Vector3);
			result.x = v.x;
			result.y = v.y;
			result.z = v.z;
			return result;
		}

		public static implicit operator Vector3Surrogate(Vector3 v)
		{
			Vector3Surrogate vector3Surrogate = new Vector3Surrogate();
			vector3Surrogate.x = v.x;
			vector3Surrogate.y = v.y;
			vector3Surrogate.z = v.z;
			return vector3Surrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector3 vector = (Vector3)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
			info.AddValue("z", vector.z);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector3 vector = (Vector3)obj;
			vector.x = (float)info.GetValue("x", typeof(float));
			vector.y = (float)info.GetValue("y", typeof(float));
			vector.z = (float)info.GetValue("z", typeof(float));
			return vector;
		}
	}
}
