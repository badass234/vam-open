using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereBrushKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("brushLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> BrushLineSpheres { get; set; }

		public ParticleLineSphereBrushKernel()
			: base("Compute/ParticleLineSphereBrush", "CSParticleLineSphereBrush")
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
