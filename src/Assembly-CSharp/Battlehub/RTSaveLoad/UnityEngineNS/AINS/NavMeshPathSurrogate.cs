using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine.AI;

namespace Battlehub.RTSaveLoad.UnityEngineNS.AINS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class NavMeshPathSurrogate : ISerializationSurrogate
	{
		public static implicit operator NavMeshPath(NavMeshPathSurrogate v)
		{
			return new NavMeshPath();
		}

		public static implicit operator NavMeshPathSurrogate(NavMeshPath v)
		{
			return new NavMeshPathSurrogate();
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
