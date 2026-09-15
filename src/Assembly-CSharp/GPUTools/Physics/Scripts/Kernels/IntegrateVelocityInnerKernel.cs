using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class IntegrateVelocityInnerKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("dtrecip")]
		public GpuValue<float> DTRecip { get; set; }

		[GpuData("step")]
		public GpuValue<float> Step { get; set; }

		public IntegrateVelocityInnerKernel()
			: base("Compute/IntegrateVelocityInner", "CSIntegrateVelocityInner")
		{
		}

		public override int GetGroupsNumX()
		{
			if (Particles != null)
			{
				return Mathf.CeilToInt((float)Particles.ComputeBuffer.count / 256f);
			}
			return 0;
		}
	}
}
