using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildDistanceJoints : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private GroupedData<GPDistanceJoint> distanceJoints;

		private GpuBuffer<GPDistanceJoint> distanceJointsBuffer;

		public BuildDistanceJoints(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreateDistanceJoints();
			distanceJointsBuffer = new GpuBuffer<GPDistanceJoint>(distanceJoints.Data, GPDistanceJoint.Size());
			settings.Runtime.DistanceJoints = distanceJoints;
			settings.Runtime.DistanceJointsBuffer = distanceJointsBuffer;
		}

		protected override void OnUpdateSettings()
		{
			CreateDistanceJoints();
			settings.Runtime.DistanceJoints = distanceJoints;
			distanceJointsBuffer.Data = distanceJoints.Data;
			distanceJointsBuffer.PushData();
		}

		private void CreateDistanceJoints()
		{
			distanceJoints = new GroupedData<GPDistanceJoint>();
			List<Int2ListContainer> jointGroups = settings.GeometryData.JointGroups;
			GPParticle[] data = settings.Runtime.Particles.Data;
			foreach (Int2ListContainer item2 in jointGroups)
			{
				List<GPDistanceJoint> list = new List<GPDistanceJoint>();
				foreach (Int2 item3 in item2.List)
				{
					GPParticle gPParticle = data[item3.X];
					GPParticle gPParticle2 = data[item3.Y];
					float distance = Vector3.Distance(gPParticle.Position, gPParticle2.Position) / settings.transform.lossyScale.x;
					GPDistanceJoint item = new GPDistanceJoint(item3.X, item3.Y, distance, 1f - settings.Stretchability);
					list.Add(item);
				}
				distanceJoints.AddGroup(list);
			}
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
		}
	}
}
