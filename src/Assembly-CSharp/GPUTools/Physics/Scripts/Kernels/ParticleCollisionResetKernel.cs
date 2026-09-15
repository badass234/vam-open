using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleCollisionResetKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		public ParticleCollisionResetKernel()
			: base("Compute/ParticleCollisionReset", "CSParticleCollisionReset")
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
