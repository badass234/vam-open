using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereGrabKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("grabLineSpheres")]
		public GpuBuffer<GPLineSphereWithMatrixDelta> GrabLineSpheres { get; set; }

		public ParticleLineSphereGrabKernel()
			: base("Compute/ParticleLineSphereGrab", "CSParticleLineSphereGrab")
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
