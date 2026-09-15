using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildNearbyJoints : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private GroupedData<GPDistanceJoint> nearbyJoints;

		private GpuBuffer<GPDistanceJoint> nearbyJointsBuffer;

		public BuildNearbyJoints(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreateNearbyJoints();
			settings.Runtime.NearbyJoints = nearbyJoints;
			if (nearbyJoints.Data.Length > 0)
			{
				nearbyJointsBuffer = new GpuBuffer<GPDistanceJoint>(nearbyJoints.Data, GPDistanceJoint.Size());
				settings.Runtime.NearbyJointsBuffer = nearbyJointsBuffer;
			}
			else
			{
				settings.Runtime.NearbyJointsBuffer = null;
			}
		}

		protected override void OnUpdateSettings()
		{
			CreateNearbyJoints();
			settings.Runtime.NearbyJoints = nearbyJoints;
			nearbyJointsBuffer.Data = nearbyJoints.Data;
			nearbyJointsBuffer.PushData();
		}

		private void CreateNearbyJoints()
		{
			nearbyJoints = new GroupedData<GPDistanceJoint>();
			List<Int2ListContainer> nearbyJointGroups = settings.GeometryData.NearbyJointGroups;
			GPParticle[] data = settings.Runtime.Particles.Data;
			foreach (Int2ListContainer item2 in nearbyJointGroups)
			{
				List<GPDistanceJoint> list = new List<GPDistanceJoint>();
				foreach (Int2 item3 in item2.List)
				{
					GPParticle gPParticle = data[item3.X];
					GPParticle gPParticle2 = data[item3.Y];
					float distance = Vector3.Distance(gPParticle.Position, gPParticle2.Position) / settings.transform.lossyScale.x;
					GPDistanceJoint item = new GPDistanceJoint(item3.X, item3.Y, distance, settings.Stiffness);
					list.Add(item);
				}
				nearbyJoints.AddGroup(list);
			}
		}

		protected override void OnDispose()
		{
			if (nearbyJoints != null)
			{
				nearbyJoints.Dispose();
			}
			if (nearbyJointsBuffer != null)
			{
				nearbyJointsBuffer.Dispose();
			}
		}
	}
}
