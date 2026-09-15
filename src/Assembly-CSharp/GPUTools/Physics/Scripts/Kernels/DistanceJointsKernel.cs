using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class DistanceJointsKernel : KernelBase
	{
		[GpuData("distanceScale")]
		public GpuValue<float> DistanceScale { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("distanceJoints")]
		public GpuBuffer<GPDistanceJoint> DistanceJointsBuffer { get; set; }

		[GpuData("distanceJointPower")]
		public GpuValue<float> DistanceJointPower { get; set; }

		public GroupedData<GPDistanceJoint> DistanceJoints { get; set; }

		public DistanceJointsKernel(GroupedData<GPDistanceJoint> groupedData, GpuBuffer<GPDistanceJoint> distanceJointsBuffer)
			: base("Compute/DistanceJoints", "CSDistanceJoints")
		{
			DistanceJoints = groupedData;
			DistanceJointsBuffer = distanceJointsBuffer;
		}

		public override void Dispatch()
		{
			if (!base.IsEnabled)
			{
				return;
			}
			if (Props.Count == 0)
			{
				CacheAttributes();
			}
			BindAttributes();
			for (int i = 0; i < DistanceJoints.GroupsData.Count; i++)
			{
				GroupData groupData = DistanceJoints.GroupsData[i];
				base.Shader.SetInt("startGroup", groupData.Start);
				base.Shader.SetInt("sizeGroup", groupData.Num);
				int num = Mathf.CeilToInt((float)groupData.Num / 256f);
				if (num > 0)
				{
					base.Shader.Dispatch(KernelId, num, 1, 1);
				}
			}
		}
	}
}
