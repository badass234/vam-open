using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class PointJointsPreCalculatedKernel : KernelBase
	{
		[GpuData("t")]
		public GpuValue<float> T { get; set; }

		[GpuData("isFixed")]
		public GpuValue<int> IsFixed { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("positions")]
		public GpuBuffer<Vector3> Positions { get; set; }

		[GpuData("oldPositions")]
		public GpuBuffer<Vector3> OldPositions { get; set; }

		[GpuData("breakThreshold")]
		public GpuValue<float> BreakThreshold { get; set; }

		[GpuData("jointStrength")]
		public GpuValue<float> JointStrength { get; set; }

		[GpuData("jointPrediction")]
		public GpuValue<float> JointPrediction { get; set; }

		public PointJointsPreCalculatedKernel()
			: base("Compute/PointJointsPreCalculated", "CSPointJointsPreCalculated")
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
