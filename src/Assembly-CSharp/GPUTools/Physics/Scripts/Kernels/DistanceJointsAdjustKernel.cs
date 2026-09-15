using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class DistanceJointsAdjustKernel : KernelBase
	{
		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("distanceJoints")]
		public GpuBuffer<GPDistanceJoint> DistanceJointsBuffer { get; set; }

		public GroupedData<GPDistanceJoint> DistanceJoints { get; set; }

		public DistanceJointsAdjustKernel(GroupedData<GPDistanceJoint> groupedData, GpuBuffer<GPDistanceJoint> distanceJointsBuffer)
			: base("Compute/DistanceJointsAdjust", "CSDistanceJointsAdjust")
		{
			DistanceJoints = groupedData;
			DistanceJointsBuffer = distanceJointsBuffer;
		}

		public override int GetGroupsNumX()
		{
			if (DistanceJointsBuffer != null)
			{
				return Mathf.CeilToInt((float)DistanceJointsBuffer.ComputeBuffer.count / 256f);
			}
			return 0;
		}
	}
}
