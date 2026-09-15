using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereGrowKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("pointToPreviousPointDistances")]
		public GpuBuffer<float> PointToPreviousPointDistances { get; set; }

		[GpuData("growLineSpheres")]
		public GpuBuffer<GPLineSphere> GrowLineSpheres { get; set; }

		public ParticleLineSphereGrowKernel()
			: base("Compute/ParticleLineSphereGrow", "CSParticleLineSphereGrow")
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
