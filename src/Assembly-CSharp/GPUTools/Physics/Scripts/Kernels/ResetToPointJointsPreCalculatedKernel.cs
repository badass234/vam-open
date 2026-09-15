using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class ResetToPointJointsPreCalculatedKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("allPointJoints")]
		public GpuBuffer<GPPointJoint> AllPointJoints { get; set; }

		[GpuData("positions")]
		public GpuBuffer<Vector3> Positions { get; set; }

		public ResetToPointJointsPreCalculatedKernel()
			: base("Compute/ResetToPointJointsPreCalculated", "CSResetToPointJointsPreCalculated")
		{
		}

		public override int GetGroupsNumX()
		{
			if (AllPointJoints != null)
			{
				return Mathf.CeilToInt((float)AllPointJoints.ComputeBuffer.count / 256f);
			}
			return 0;
		}
	}
}
