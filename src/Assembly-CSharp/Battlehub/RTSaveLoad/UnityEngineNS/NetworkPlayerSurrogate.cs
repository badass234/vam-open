using System.Runtime.Serialization;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class NetworkPlayerSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			return obj;
		}
	}
}
