using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class BoundsSurrogate : ISerializationSurrogate
	{
		public Vector3 center;

		public Vector3 size;

		public Vector3 extents;

		public Vector3 min;

		public Vector3 max;

		public static implicit operator Bounds(BoundsSurrogate v)
		{
			Bounds result = default(Bounds);
			result.center = v.center;
			result.size = v.size;
			result.extents = v.extents;
			result.min = v.min;
			result.max = v.max;
			return result;
		}

		public static implicit operator BoundsSurrogate(Bounds v)
		{
			BoundsSurrogate boundsSurrogate = new BoundsSurrogate();
			boundsSurrogate.center = v.center;
			boundsSurrogate.size = v.size;
			boundsSurrogate.extents = v.extents;
			boundsSurrogate.min = v.min;
			boundsSurrogate.max = v.max;
			return boundsSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Bounds bounds = (Bounds)obj;
			info.AddValue("center", bounds.center);
			info.AddValue("size", bounds.size);
			info.AddValue("extents", bounds.extents);
			info.AddValue("min", bounds.min);
			info.AddValue("max", bounds.max);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Bounds bounds = (Bounds)obj;
			bounds.center = (Vector3)info.GetValue("center", typeof(Vector3));
			bounds.size = (Vector3)info.GetValue("size", typeof(Vector3));
			bounds.extents = (Vector3)info.GetValue("extents", typeof(Vector3));
			bounds.min = (Vector3)info.GetValue("min", typeof(Vector3));
			bounds.max = (Vector3)info.GetValue("max", typeof(Vector3));
			return bounds;
		}
	}
}
