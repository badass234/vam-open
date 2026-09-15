using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Runtime.Render;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereCutKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("pointToPreviousPointDistances")]
		public GpuBuffer<float> PointToPreviousPointDistances { get; set; }

		[GpuData("renderParticles")]
		public GpuBuffer<RenderParticle> RenderParticles { get; set; }

		[GpuData("cutLineSpheres")]
		public GpuBuffer<GPLineSphere> CutLineSpheres { get; set; }

		public ParticleLineSphereCutKernel()
			: base("Compute/ParticleLineSphereCut", "CSParticleLineSphereCut")
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
