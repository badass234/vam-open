using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ClothSkinningCoefficientSurrogate : ISerializationSurrogate
	{
		public float maxDistance;

		public float collisionSphereDistance;

		public static implicit operator ClothSkinningCoefficient(ClothSkinningCoefficientSurrogate v)
		{
			ClothSkinningCoefficient result = default(ClothSkinningCoefficient);
			result.maxDistance = v.maxDistance;
			result.collisionSphereDistance = v.collisionSphereDistance;
			return result;
		}

		public static implicit operator ClothSkinningCoefficientSurrogate(ClothSkinningCoefficient v)
		{
			ClothSkinningCoefficientSurrogate clothSkinningCoefficientSurrogate = new ClothSkinningCoefficientSurrogate();
			clothSkinningCoefficientSurrogate.maxDistance = v.maxDistance;
			clothSkinningCoefficientSurrogate.collisionSphereDistance = v.collisionSphereDistance;
			return clothSkinningCoefficientSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			ClothSkinningCoefficient clothSkinningCoefficient = (ClothSkinningCoefficient)obj;
			info.AddValue("maxDistance", clothSkinningCoefficient.maxDistance);
			info.AddValue("collisionSphereDistance", clothSkinningCoefficient.collisionSphereDistance);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			ClothSkinningCoefficient clothSkinningCoefficient = (ClothSkinningCoefficient)obj;
			clothSkinningCoefficient.maxDistance = (float)info.GetValue("maxDistance", typeof(float));
			clothSkinningCoefficient.collisionSphereDistance = (float)info.GetValue("collisionSphereDistance", typeof(float));
			return clothSkinningCoefficient;
		}
	}
}
