using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class RectOffsetSurrogate : ISerializationSurrogate
	{
		public int left;

		public int right;

		public int top;

		public int bottom;

		public static implicit operator RectOffset(RectOffsetSurrogate v)
		{
			RectOffset rectOffset = new RectOffset();
			rectOffset.left = v.left;
			rectOffset.right = v.right;
			rectOffset.top = v.top;
			rectOffset.bottom = v.bottom;
			return rectOffset;
		}

		public static implicit operator RectOffsetSurrogate(RectOffset v)
		{
			RectOffsetSurrogate rectOffsetSurrogate = new RectOffsetSurrogate();
			rectOffsetSurrogate.left = v.left;
			rectOffsetSurrogate.right = v.right;
			rectOffsetSurrogate.top = v.top;
			rectOffsetSurrogate.bottom = v.bottom;
			return rectOffsetSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			RectOffset rectOffset = (RectOffset)obj;
			info.AddValue("left", rectOffset.left);
			info.AddValue("right", rectOffset.right);
			info.AddValue("top", rectOffset.top);
			info.AddValue("bottom", rectOffset.bottom);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			RectOffset rectOffset = (RectOffset)obj;
			rectOffset.left = (int)info.GetValue("left", typeof(int));
			rectOffset.right = (int)info.GetValue("right", typeof(int));
			rectOffset.top = (int)info.GetValue("top", typeof(int));
			rectOffset.bottom = (int)info.GetValue("bottom", typeof(int));
			return rectOffset;
		}
	}
}
