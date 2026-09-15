using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class PointJointsFixedRigidityKernel : KernelBase
	{
		[GpuData("weight")]
		public GpuValue<float> Weight { get; set; }

		[GpuData("isFixed")]
		public GpuValue<int> IsFixed { get; set; }

		[GpuData("fixedRigidity")]
		public GpuValue<float> FixedRigidity { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> Transforms { get; set; }

		public PointJointsFixedRigidityKernel()
			: base("Compute/PointJointsFixedRigidity", "CSPointJointsFixedRigidity")
		{
		}

		public override int GetGroupsNumX()
		{
			if (PointJoints != null)
			{
				return Mathf.CeilToInt((float)PointJoints.ComputeBuffer.count / 256f);
			}
			return 0;
		}
	}
}
