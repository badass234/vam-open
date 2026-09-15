using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class LayerMaskSurrogate : ISerializationSurrogate
	{
		public int value;

		public static implicit operator LayerMask(LayerMaskSurrogate v)
		{
			LayerMask result = default(LayerMask);
			result.value = v.value;
			return result;
		}

		public static implicit operator LayerMaskSurrogate(LayerMask v)
		{
			LayerMaskSurrogate layerMaskSurrogate = new LayerMaskSurrogate();
			layerMaskSurrogate.value = v.value;
			return layerMaskSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			info.AddValue("value", ((LayerMask)obj).value);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			LayerMask layerMask = (LayerMask)obj;
			layerMask.value = (int)info.GetValue("value", typeof(int));
			return layerMask;
		}
	}
}
