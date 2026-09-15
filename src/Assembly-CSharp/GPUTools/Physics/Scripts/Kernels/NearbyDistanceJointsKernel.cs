using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class NearbyDistanceJointsKernel : KernelBase
	{
		[GpuData("nearbyDistanceScale")]
		public GpuValue<float> NearbyDistanceScale { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("nearbyDistanceJoints")]
		public GpuBuffer<GPDistanceJoint> NearbyDistanceJointsBuffer { get; set; }

		[GpuData("nearbyJointPower")]
		public GpuValue<float> NearbyJointPower { get; set; }

		[GpuData("nearbyJointPowerRolloff")]
		public GpuValue<float> NearbyJointPowerRolloff { get; set; }

		public GroupedData<GPDistanceJoint> NearbyDistanceJoints { get; set; }

		public NearbyDistanceJointsKernel(GroupedData<GPDistanceJoint> groupedData, GpuBuffer<GPDistanceJoint> nearbyDistanceJointsBuffer)
			: base("Compute/NearbyDistanceJoints", "CSNearbyDistanceJoints")
		{
			NearbyDistanceJoints = groupedData;
			NearbyDistanceJointsBuffer = nearbyDistanceJointsBuffer;
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
			for (int i = 0; i < NearbyDistanceJoints.GroupsData.Count; i++)
			{
				GroupData groupData = NearbyDistanceJoints.GroupsData[i];
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
