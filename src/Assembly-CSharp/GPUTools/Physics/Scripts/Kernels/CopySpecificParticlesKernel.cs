using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class CopySpecificParticlesKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("outParticles")]
		public GpuBuffer<GPParticle> OutParticles { get; set; }

		[GpuData("outParticlesMap")]
		public GpuBuffer<float> OutParticlesMap { get; set; }

		public CopySpecificParticlesKernel()
			: base("Compute/CopySpecificParticles", "CSCopySpecificParticles")
		{
		}

		public override int GetGroupsNumX()
		{
			return 1;
		}
	}
}
