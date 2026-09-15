using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Kernels
{
	public class CreateVertexOnlyDataKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("clothOnlyVertices")]
		public GpuBuffer<Vector3> ClothOnlyVertices { get; set; }

		[GpuData("meshToPhysicsVerticesMap")]
		public GpuBuffer<int> MeshToPhysicsVerticesMap { get; set; }

		public CreateVertexOnlyDataKernel()
			: base("Compute/CreateVertexOnlyData", "CSCreateVertexOnlyData")
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
