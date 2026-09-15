using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Kernels
{
	public class CreateVertexDataKernel : KernelBase
	{
		[GpuData("facesForNormalNum")]
		public GpuValue<int> FacesForNormalNum { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("clothVertices")]
		public GpuBuffer<ClothVertex> ClothVertices { get; set; }

		[GpuData("meshToPhysicsVerticesMap")]
		public GpuBuffer<int> MeshToPhysicsVerticesMap { get; set; }

		[GpuData("meshVertexToNeiborsMap")]
		public GpuBuffer<int> MeshVertexToNeiborsMap { get; set; }

		[GpuData("meshVertexToNeiborsMapCounts")]
		public GpuBuffer<int> MeshVertexToNeiborsMapCounts { get; set; }

		public CreateVertexDataKernel()
			: base("Compute/CreateVertexData", "CSCreateVertexData")
		{
		}

		public override int GetGroupsNumX()
		{
			if (MeshToPhysicsVerticesMap != null)
			{
				return Mathf.CeilToInt((float)MeshToPhysicsVerticesMap.ComputeBuffer.count / 256f);
			}
			return 0;
		}
	}
}
