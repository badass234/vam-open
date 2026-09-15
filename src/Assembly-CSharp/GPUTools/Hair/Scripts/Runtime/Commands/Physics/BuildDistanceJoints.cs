using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildDistanceJoints : BuildChainCommand
	{
		private readonly HairSettings settings;

		private GroupedData<GPDistanceJoint> distanceJoints;

		private GpuBuffer<GPDistanceJoint> distanceJointsBuffer;

		private float[] pointToPreviousPointDistances;

		public BuildDistanceJoints(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			if (settings.RuntimeData.Particles != null)
			{
				pointToPreviousPointDistances = new float[settings.RuntimeData.Particles.Count];
			}
			else
			{
				pointToPreviousPointDistances = new float[0];
			}
			CreateDistanceJoints();
			if (distanceJointsBuffer != null)
			{
				distanceJointsBuffer.Dispose();
			}
			if (distanceJoints.Data.Length > 0)
			{
				distanceJointsBuffer = new GpuBuffer<GPDistanceJoint>(distanceJoints.Data, GPDistanceJoint.Size());
			}
			else
			{
				distanceJointsBuffer = null;
			}
			settings.RuntimeData.DistanceJoints = distanceJoints;
			settings.RuntimeData.DistanceJointsBuffer = distanceJointsBuffer;
			if (settings.RuntimeData.PointToPreviousPointDistances != null)
			{
				settings.RuntimeData.PointToPreviousPointDistances.Dispose();
			}
			if (pointToPreviousPointDistances.Length > 0)
			{
				settings.RuntimeData.PointToPreviousPointDistances = new GpuBuffer<float>(pointToPreviousPointDistances, 4);
			}
			else
			{
				settings.RuntimeData.PointToPreviousPointDistances = null;
			}
		}

		protected override void OnUpdateSettings()
		{
			CreateDistanceJoints();
			settings.RuntimeData.DistanceJoints = distanceJoints;
			if (distanceJointsBuffer != null)
			{
				distanceJointsBuffer.Data = distanceJoints.Data;
				distanceJointsBuffer.PushData();
			}
			if (settings.RuntimeData.PointToPreviousPointDistances != null)
			{
				settings.RuntimeData.PointToPreviousPointDistances.PushData();
			}
		}

		public void RebuildFromGPUData()
		{
			settings.RuntimeData.DistanceJointsBuffer.PullData();
			GPDistanceJoint[] data = settings.RuntimeData.DistanceJointsBuffer.Data;
		}

		private void CreateDistanceJoints()
		{
			int segments = settings.StandsSettings.Segments;
			if (distanceJoints != null)
			{
				distanceJoints.Dispose();
			}
			distanceJoints = new GroupedData<GPDistanceJoint>();
			List<GPDistanceJoint> list = new List<GPDistanceJoint>();
			List<GPDistanceJoint> list2 = new List<GPDistanceJoint>();
			if (settings.RuntimeData.Particles != null)
			{
				for (int i = 0; i < settings.RuntimeData.Particles.Count; i++)
				{
					if (i % segments != 0)
					{
						GPParticle gPParticle = settings.RuntimeData.Particles.Data[i - 1];
						GPParticle gPParticle2 = settings.RuntimeData.Particles.Data[i];
						float num = Vector3.Distance(gPParticle.Position, gPParticle2.Position);
						pointToPreviousPointDistances[i] = num;
						List<GPDistanceJoint> list3 = ((i % 2 != 0) ? list : list2);
						GPDistanceJoint item = new GPDistanceJoint(i - 1, i, num, 1f);
						list3.Add(item);
					}
				}
			}
			distanceJoints.AddGroup(list);
			distanceJoints.AddGroup(list2);
		}

		protected override void OnDispose()
		{
			if (distanceJoints != null)
			{
				distanceJoints.Dispose();
			}
			if (distanceJointsBuffer != null)
			{
				distanceJointsBuffer.Dispose();
			}
			if (settings.RuntimeData.PointToPreviousPointDistances != null)
			{
				settings.RuntimeData.PointToPreviousPointDistances.Dispose();
			}
		}
	}
}
