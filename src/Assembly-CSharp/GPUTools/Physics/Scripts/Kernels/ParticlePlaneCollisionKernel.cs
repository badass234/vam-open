using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticlePlaneCollisionKernel : KernelBase
	{
		[GpuData("step")]
		public GpuValue<float> Step { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("planes")]
		public GpuBuffer<Vector4> Planes { get; set; }

		public ParticlePlaneCollisionKernel()
			: base("Compute/ParticlePlaneCollision", "CSParticlePlaneCollision")
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
