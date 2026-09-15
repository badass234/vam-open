using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereCollisionHoldKernel : KernelBase
	{
		[GpuData("step")]
		public GpuValue<float> Step { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("processedLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		public ParticleLineSphereCollisionHoldKernel()
			: base("Compute/ParticleLineSphereCollisionHold", "CSParticleLineSphereCollisionHold")
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
