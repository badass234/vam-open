using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class Vector2Surrogate : ISerializationSurrogate
	{
		public float x;

		public float y;

		public static implicit operator Vector2(Vector2Surrogate v)
		{
			Vector2 result = default(Vector2);
			result.x = v.x;
			result.y = v.y;
			return result;
		}

		public static implicit operator Vector2Surrogate(Vector2 v)
		{
			Vector2Surrogate vector2Surrogate = new Vector2Surrogate();
			vector2Surrogate.x = v.x;
			vector2Surrogate.y = v.y;
			return vector2Surrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector2 vector = (Vector2)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector2 vector = (Vector2)obj;
			vector.x = (float)info.GetValue("x", typeof(float));
			vector.y = (float)info.GetValue("y", typeof(float));
			return vector;
		}
	}
}
