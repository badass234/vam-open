using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleSphereCopyKernel : KernelBase
	{
		[GpuData("spheres")]
		public GpuBuffer<GPSphere> Spheres { get; set; }

		[GpuData("oldSpheres")]
		public GpuBuffer<GPSphere> OldSpheres { get; set; }

		public ParticleSphereCopyKernel()
			: base("Compute/ParticleSphereCopy", "CSParticleSphereCopy")
		{
		}

		public override int GetGroupsNumX()
		{
			if (Spheres != null)
			{
				return Mathf.CeilToInt((float)Spheres.Count / 256f);
			}
			return 0;
		}
	}
}
