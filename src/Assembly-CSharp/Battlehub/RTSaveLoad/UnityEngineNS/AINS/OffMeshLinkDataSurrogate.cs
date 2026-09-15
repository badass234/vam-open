using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine.AI;

namespace Battlehub.RTSaveLoad.UnityEngineNS.AINS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class OffMeshLinkDataSurrogate : ISerializationSurrogate
	{
		public static implicit operator OffMeshLinkData(OffMeshLinkDataSurrogate v)
		{
			return default(OffMeshLinkData);
		}

		public static implicit operator OffMeshLinkDataSurrogate(OffMeshLinkData v)
		{
			return new OffMeshLinkDataSurrogate();
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
