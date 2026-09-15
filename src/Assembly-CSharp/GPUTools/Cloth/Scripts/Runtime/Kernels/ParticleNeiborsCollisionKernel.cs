using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Kernels
{
	public class ParticleNeiborsCollisionKernel : KernelBase
	{
		[GpuData("step")]
		public GpuValue<float> Step { get; set; }

		[GpuData("t")]
		public GpuValue<float> T { get; set; }

		[GpuData("facesForNormalNum")]
		public GpuValue<int> FacesForNormalNum { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("meshVertexToNeiborsMap")]
		public GpuBuffer<int> MeshVertexToNeiborsMap { get; set; }

		[GpuData("meshVertexToNeiborsMapCounts")]
		public GpuBuffer<int> MeshVertexToNeiborsMapCounts { get; set; }

		public ParticleNeiborsCollisionKernel()
			: base("Compute/ParticleNeiborsCollision", "CSParticleNeiborsCollision")
		{
		}

		public override int GetGroupsNumX()
		{
			if (Particles != null)
			{
				return Mathf.CeilToInt((float)Particles.Count / 256f);
			}
			return 0;
		}
	}
}
