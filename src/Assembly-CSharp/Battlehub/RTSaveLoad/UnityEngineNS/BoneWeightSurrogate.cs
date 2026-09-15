using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class BoneWeightSurrogate : ISerializationSurrogate
	{
		public float weight0;

		public float weight1;

		public float weight2;

		public float weight3;

		public int boneIndex0;

		public int boneIndex1;

		public int boneIndex2;

		public int boneIndex3;

		public static implicit operator BoneWeight(BoneWeightSurrogate v)
		{
			BoneWeight result = default(BoneWeight);
			result.weight0 = v.weight0;
			result.weight1 = v.weight1;
			result.weight2 = v.weight2;
			result.weight3 = v.weight3;
			result.boneIndex0 = v.boneIndex0;
			result.boneIndex1 = v.boneIndex1;
			result.boneIndex2 = v.boneIndex2;
			result.boneIndex3 = v.boneIndex3;
			return result;
		}

		public static implicit operator BoneWeightSurrogate(BoneWeight v)
		{
			BoneWeightSurrogate boneWeightSurrogate = new BoneWeightSurrogate();
			boneWeightSurrogate.weight0 = v.weight0;
			boneWeightSurrogate.weight1 = v.weight1;
			boneWeightSurrogate.weight2 = v.weight2;
			boneWeightSurrogate.weight3 = v.weight3;
			boneWeightSurrogate.boneIndex0 = v.boneIndex0;
			boneWeightSurrogate.boneIndex1 = v.boneIndex1;
			boneWeightSurrogate.boneIndex2 = v.boneIndex2;
			boneWeightSurrogate.boneIndex3 = v.boneIndex3;
			return boneWeightSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			BoneWeight boneWeight = (BoneWeight)obj;
			info.AddValue("weight0", boneWeight.weight0);
			info.AddValue("weight1", boneWeight.weight1);
			info.AddValue("weight2", boneWeight.weight2);
			info.AddValue("weight3", boneWeight.weight3);
			info.AddValue("boneIndex0", boneWeight.boneIndex0);
			info.AddValue("boneIndex1", boneWeight.boneIndex1);
			info.AddValue("boneIndex2", boneWeight.boneIndex2);
			info.AddValue("boneIndex3", boneWeight.boneIndex3);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			BoneWeight boneWeight = (BoneWeight)obj;
			boneWeight.weight0 = (float)info.GetValue("weight0", typeof(float));
			boneWeight.weight1 = (float)info.GetValue("weight1", typeof(float));
			boneWeight.weight2 = (float)info.GetValue("weight2", typeof(float));
			boneWeight.weight3 = (float)info.GetValue("weight3", typeof(float));
			boneWeight.boneIndex0 = (int)info.GetValue("boneIndex0", typeof(int));
			boneWeight.boneIndex1 = (int)info.GetValue("boneIndex1", typeof(int));
			boneWeight.boneIndex2 = (int)info.GetValue("boneIndex2", typeof(int));
			boneWeight.boneIndex3 = (int)info.GetValue("boneIndex3", typeof(int));
			return boneWeight;
		}
	}
}
