using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class IntegrateKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("invDrag")]
		public GpuValue<float> InvDrag { get; set; }

		[GpuData("accelDT2")]
		public GpuValue<Vector3> AccelDT2 { get; set; }

		public IntegrateKernel()
			: base("Compute/Integrate", "CSIntegrate")
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
