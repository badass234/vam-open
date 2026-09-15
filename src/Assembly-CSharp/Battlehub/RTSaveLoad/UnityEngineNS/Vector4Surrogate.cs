using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class Vector4Surrogate : ISerializationSurrogate
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public static implicit operator Vector4(Vector4Surrogate v)
		{
			Vector4 result = default(Vector4);
			result.x = v.x;
			result.y = v.y;
			result.z = v.z;
			result.w = v.w;
			return result;
		}

		public static implicit operator Vector4Surrogate(Vector4 v)
		{
			Vector4Surrogate vector4Surrogate = new Vector4Surrogate();
			vector4Surrogate.x = v.x;
			vector4Surrogate.y = v.y;
			vector4Surrogate.z = v.z;
			vector4Surrogate.w = v.w;
			return vector4Surrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector4 vector = (Vector4)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
			info.AddValue("z", vector.z);
			info.AddValue("w", vector.w);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector4 vector = (Vector4)obj;
			vector.x = (float)info.GetValue("x", typeof(float));
			vector.y = (float)info.GetValue("y", typeof(float));
			vector.z = (float)info.GetValue("z", typeof(float));
			vector.w = (float)info.GetValue("w", typeof(float));
			return vector;
		}
	}
}
