using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereProcessKernel : KernelBase
	{
		[GpuData("collisionPrediction")]
		public GpuValue<float> CollisionPrediction { get; set; }

		[GpuData("lineSpheres")]
		public GpuBuffer<GPLineSphere> LineSpheres { get; set; }

		[GpuData("oldLineSpheres")]
		public GpuBuffer<GPLineSphere> OldLineSpheres { get; set; }

		[GpuData("processedLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		public ParticleLineSphereProcessKernel()
			: base("Compute/ParticleLineSphereProcess", "CSParticleLineSphereProcess")
		{
		}

		public override int GetGroupsNumX()
		{
			if (LineSpheres != null)
			{
				return Mathf.CeilToInt((float)LineSpheres.Count / 256f);
			}
			return 0;
		}
	}
}
