using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ParticleLineSphereRigidityDecreaseKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("rigidityDecreaseLineSpheres")]
		public GpuBuffer<GPLineSphere> RigidityDecreaseLineSpheres { get; set; }

		public ParticleLineSphereRigidityDecreaseKernel()
			: base("Compute/ParticleLineSphereRigidityDecrease", "CSParticleLineSphereRigidityDecrease")
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
