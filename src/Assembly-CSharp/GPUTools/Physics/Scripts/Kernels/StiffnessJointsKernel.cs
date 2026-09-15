using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class StiffnessJointsKernel : KernelBase
	{
		[GpuData("distanceScale")]
		public GpuValue<float> DistanceScale { get; set; }

		[GpuData("stiffness")]
		public GpuValue<float> Stiffness { get; set; }

		[GpuData("compressionResistance")]
		public GpuValue<float> CompressionResistance { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("stiffnessJoints")]
		public GpuBuffer<GPDistanceJoint> StiffnessJointsBuffer { get; set; }

		private GroupedData<GPDistanceJoint> StiffnessJoints { get; set; }

		public StiffnessJointsKernel(GroupedData<GPDistanceJoint> groupedData, GpuBuffer<GPDistanceJoint> stiffnessJointsBuffer)
			: base("Compute/StiffnessJoints", "CSStiffnessJoints")
		{
			StiffnessJoints = groupedData;
			StiffnessJointsBuffer = stiffnessJointsBuffer;
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
			for (int i = 0; i < StiffnessJoints.GroupsData.Count; i++)
			{
				GroupData groupData = StiffnessJoints.GroupsData[i];
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
