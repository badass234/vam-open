using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class IntegrateIterWithParticleHoldKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("dt")]
		public GpuValue<float> DT { get; set; }

		[GpuData("invDrag")]
		public GpuValue<float> InvDrag { get; set; }

		[GpuData("accelDT2")]
		public GpuValue<Vector3> AccelDT2 { get; set; }

		public IntegrateIterWithParticleHoldKernel()
			: base("Compute/IntegrateIterWithParticleHold", "CSIntegrateIterWithParticleHold")
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
