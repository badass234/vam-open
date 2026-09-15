using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class PointJointsPreCalculatedFinalKernel : KernelBase
	{
		[GpuData("isFixed")]
		public GpuValue<int> IsFixed { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("positions")]
		public GpuBuffer<Vector3> Positions { get; set; }

		[GpuData("jointStrength")]
		public GpuValue<float> JointStrength { get; set; }

		public PointJointsPreCalculatedFinalKernel()
			: base("Compute/PointJointsPreCalculatedFinal", "CSPointJointsPreCalculatedFinal")
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
