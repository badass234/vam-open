using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine.Playables;

namespace Battlehub.RTSaveLoad.UnityEngineNS.ExperimentalNS.DirectorNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class PlayableGraphSurrogate : ISerializationSurrogate
	{
		public static implicit operator PlayableGraph(PlayableGraphSurrogate v)
		{
			return default(PlayableGraph);
		}

		public static implicit operator PlayableGraphSurrogate(PlayableGraph v)
		{
			return new PlayableGraphSurrogate();
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
