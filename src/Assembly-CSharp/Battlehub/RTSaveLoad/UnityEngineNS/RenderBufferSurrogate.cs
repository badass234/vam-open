using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class RenderBufferSurrogate : ISerializationSurrogate
	{
		public static implicit operator RenderBuffer(RenderBufferSurrogate v)
		{
			return default(RenderBuffer);
		}

		public static implicit operator RenderBufferSurrogate(RenderBuffer v)
		{
			return new RenderBufferSurrogate();
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			return obj;
		}
	}
}
