using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildCompressionJoints : BuildChainCommand
	{
		private readonly HairSettings settings;

		private GroupedData<GPDistanceJoint> compressionJoints;

		private GpuBuffer<GPDistanceJoint> compressionJointsBuffer;

		public BuildCompressionJoints(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreateCompressionJoints();
			if (compressionJointsBuffer != null)
			{
				compressionJointsBuffer.Dispose();
			}
			if (compressionJoints.Data.Length > 0)
			{
				compressionJointsBuffer = new GpuBuffer<GPDistanceJoint>(compressionJoints.Data, GPDistanceJoint.Size());
			}
			else
			{
				compressionJointsBuffer = null;
			}
			settings.RuntimeData.CompressionJoints = compressionJoints;
			settings.RuntimeData.CompressionJointsBuffer = compressionJointsBuffer;
		}

		protected override void OnUpdateSettings()
		{
			CreateCompressionJoints();
			settings.RuntimeData.CompressionJoints = compressionJoints;
			if (compressionJointsBuffer != null)
			{
				compressionJointsBuffer.Data = compressionJoints.Data;
				compressionJointsBuffer.PushData();
			}
		}

		private void CreateCompressionJoints()
		{
			int segments = settings.StandsSettings.Segments;
			compressionJoints = new GroupedData<GPDistanceJoint>();
			List<GPDistanceJoint> list = new List<GPDistanceJoint>();
			List<GPDistanceJoint> list2 = new List<GPDistanceJoint>();
			if (settings.RuntimeData.Particles != null)
			{
				for (int i = 0; i < settings.RuntimeData.Particles.Count; i++)
				{
					if (i % segments != 0 && i % segments != 1)
					{
						GPParticle gPParticle = settings.RuntimeData.Particles.Data[i - 2];
						GPParticle gPParticle2 = settings.RuntimeData.Particles.Data[i];
						float distance = Vector3.Distance(gPParticle.Position, gPParticle2.Position) * 0.95f;
						List<GPDistanceJoint> list3 = ((i % 4 != 2 && i % 4 != 3) ? list2 : list);
						GPDistanceJoint item = new GPDistanceJoint(i - 2, i, distance, 1f);
						list3.Add(item);
					}
				}
			}
			compressionJoints.AddGroup(list);
			compressionJoints.AddGroup(list2);
		}

		protected override void OnDispose()
		{
			if (compressionJoints != null)
			{
				compressionJoints.Dispose();
			}
			if (compressionJointsBuffer != null)
			{
				compressionJointsBuffer.Dispose();
			}
		}
	}
}
