using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Kernels
{
	public class GPUSkinner : KernelBase
	{
		private readonly SkinnedMeshRenderer skin;

		private Transform[] bones;

		private Matrix4x4[] bindposes;

		[GpuData("weights")]
		public GpuBuffer<Weight> WeightsBuffer { get; set; }

		[GpuData("bones")]
		public GpuBuffer<Matrix4x4> BonesBuffer { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> TransformMatricesBuffer { get; set; }

		public GPUSkinner(SkinnedMeshRenderer skin)
			: base("Compute/Skinner", "CSComputeMatrices")
		{
			this.skin = skin;
			Mesh sharedMesh = skin.sharedMesh;
			bones = skin.bones;
			bindposes = sharedMesh.bindposes;
			TransformMatricesBuffer = new GpuBuffer<Matrix4x4>(sharedMesh.vertexCount, 64);
			BonesBuffer = new GpuBuffer<Matrix4x4>(new Matrix4x4[skin.bones.Length], 64);
			WeightsBuffer = new GpuBuffer<Weight>(GetWeightsArray(sharedMesh), 32);
		}

		public override void Dispatch()
		{
			CalculateBones();
			base.Dispatch();
		}

		public override void Dispose()
		{
			TransformMatricesBuffer.Dispose();
			BonesBuffer.Dispose();
			WeightsBuffer.Dispose();
		}

		public override int GetGroupsNumX()
		{
			return Mathf.CeilToInt((float)skin.sharedMesh.vertexCount / 256f);
		}

		private void CalculateBones()
		{
			for (int i = 0; i < BonesBuffer.Data.Length; i++)
			{
				BonesBuffer.Data[i] = bones[i].localToWorldMatrix * bindposes[i];
			}
			BonesBuffer.PushData();
		}

		private Weight[] GetWeightsArray(Mesh mesh)
		{
			Weight[] array = new Weight[mesh.boneWeights.Length];
			BoneWeight[] boneWeights = mesh.boneWeights;
			for (int i = 0; i < boneWeights.Length; i++)
			{
				BoneWeight boneWeight = boneWeights[i];
				Weight weight = default(Weight);
				weight.bi0 = boneWeight.boneIndex0;
				weight.bi1 = boneWeight.boneIndex1;
				weight.bi2 = boneWeight.boneIndex2;
				weight.bi3 = boneWeight.boneIndex3;
				weight.w0 = boneWeight.weight0;
				weight.w1 = boneWeight.weight1;
				weight.w2 = boneWeight.weight2;
				weight.w3 = boneWeight.weight3;
				Weight weight2 = weight;
				array[i] = weight2;
			}
			return array;
		}
	}
}
