using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine.SceneManagement;

namespace Battlehub.RTSaveLoad.UnityEngineNS.SceneManagementNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class SceneSurrogate : ISerializationSurrogate
	{
		public static implicit operator Scene(SceneSurrogate v)
		{
			return default(Scene);
		}

		public static implicit operator SceneSurrogate(Scene v)
		{
			return new SceneSurrogate();
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
