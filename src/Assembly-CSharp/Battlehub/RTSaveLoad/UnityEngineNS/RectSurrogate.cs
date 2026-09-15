using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class RectSurrogate : ISerializationSurrogate
	{
		public float x;

		public float y;

		public float width;

		public float height;

		public static implicit operator Rect(RectSurrogate v)
		{
			Rect result = default(Rect);
			result.x = v.x;
			result.y = v.y;
			result.width = v.width;
			result.height = v.height;
			return result;
		}

		public static implicit operator RectSurrogate(Rect v)
		{
			RectSurrogate rectSurrogate = new RectSurrogate();
			rectSurrogate.x = v.x;
			rectSurrogate.y = v.y;
			rectSurrogate.width = v.width;
			rectSurrogate.height = v.height;
			return rectSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Rect rect = (Rect)obj;
			info.AddValue("x", rect.x);
			info.AddValue("y", rect.y);
			info.AddValue("width", rect.width);
			info.AddValue("height", rect.height);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Rect rect = (Rect)obj;
			rect.x = (float)info.GetValue("x", typeof(float));
			rect.y = (float)info.GetValue("y", typeof(float));
			rect.width = (float)info.GetValue("width", typeof(float));
			rect.height = (float)info.GetValue("height", typeof(float));
			return rect;
		}
	}
}
