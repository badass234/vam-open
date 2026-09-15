using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Kernels
{
	public class CompressionJointsKernel : KernelBase
	{
		[GpuData("compressionDistanceScale")]
		public GpuValue<float> CompressionDistanceScale { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("compressionJoints")]
		public GpuBuffer<GPDistanceJoint> CompressionJointsBuffer { get; set; }

		[GpuData("compressionJointPower")]
		public GpuValue<float> CompressionJointPower { get; set; }

		public GroupedData<GPDistanceJoint> CompressionJoints { get; set; }

		public CompressionJointsKernel(GroupedData<GPDistanceJoint> groupedData, GpuBuffer<GPDistanceJoint> compressionJointsBuffer)
			: base("Compute/CompressionJoints", "CSCompressionJoints")
		{
			CompressionJoints = groupedData;
			CompressionJointsBuffer = compressionJointsBuffer;
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
			for (int i = 0; i < CompressionJoints.GroupsData.Count; i++)
			{
				GroupData groupData = CompressionJoints.GroupsData[i];
				base.Shader.SetInt("startGroup", groupData.Start);
				base.Shader.SetInt("sizeGroup", groupData.Num);
				if (groupData.Num > 0)
				{
					int threadGroupsX = Mathf.CeilToInt((float)groupData.Num / 256f);
					base.Shader.Dispatch(KernelId, threadGroupsX, 1, 1);
				}
			}
		}
	}
}
