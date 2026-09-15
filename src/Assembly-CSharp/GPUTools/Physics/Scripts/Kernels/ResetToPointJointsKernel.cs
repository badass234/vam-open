using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ResetToPointJointsKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> Transforms { get; set; }

		public ResetToPointJointsKernel()
			: base("Compute/ResetToPointJoints", "CSResetToPointJoints")
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
