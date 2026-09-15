using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereCollisionKernel : KernelBase
	{
		[GpuData("t")]
		public GpuValue<float> T { get; set; }

		[GpuData("friction")]
		public GpuValue<float> Friction { get; set; }

		[GpuData("staticFriction")]
		public GpuValue<float> StaticFriction { get; set; }

		[GpuData("collisionPower")]
		public GpuValue<float> CollisionPower { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("processedLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		public ParticleLineSphereCollisionKernel()
			: base("Compute/ParticleLineSphereCollision", "CSParticleLineSphereCollision")
		{
		}

		public override int GetGroupsNumX()
		{
			if (Particles != null)
			{
				return Mathf.CeilToInt((float)Particles.Count / 256f);
			}
			return 0;
		}
	}
}
