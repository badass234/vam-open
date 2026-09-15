using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class SplineJointsReverseWithParticleHoldKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("pointToPreviousPointDistances")]
		public GpuBuffer<float> PointToPreviousPointDistances { get; set; }

		[GpuData("splineJointPower")]
		public GpuValue<float> SplineJointPower { get; set; }

		[GpuData("reverseSplineJointPower")]
		public GpuValue<float> ReverseSplineJointPower { get; set; }

		public SplineJointsReverseWithParticleHoldKernel()
			: base("Compute/SplineJointsReverseWithParticleHold", "CSSplineJointsReverseWithParticleHold")
		{
		}

		public override int GetGroupsNumX()
		{
			if (Particles != null)
			{
				return Mathf.CeilToInt((float)(Particles.Count / Segments.Value) / 256f);
			}
			return 0;
		}
	}
}
