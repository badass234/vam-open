using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildStiffnessJoints : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private GroupedData<GPDistanceJoint> stiffnessJoints;

		private GpuBuffer<GPDistanceJoint> stiffnessJointsBuffer;

		public BuildStiffnessJoints(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreateStiffnessJoints();
			stiffnessJointsBuffer = new GpuBuffer<GPDistanceJoint>(stiffnessJoints.Data, GPDistanceJoint.Size());
			settings.Runtime.StiffnessJoints = stiffnessJoints;
			settings.Runtime.StiffnessJointsBuffer = stiffnessJointsBuffer;
		}

		protected override void OnUpdateSettings()
		{
			CreateStiffnessJoints();
			settings.Runtime.StiffnessJoints = stiffnessJoints;
			stiffnessJointsBuffer.Data = stiffnessJoints.Data;
			stiffnessJointsBuffer.PushData();
		}

		private void CreateStiffnessJoints()
		{
			stiffnessJoints = new GroupedData<GPDistanceJoint>();
			List<Int2ListContainer> stiffnessJointGroups = settings.GeometryData.StiffnessJointGroups;
			GPParticle[] data = settings.Runtime.Particles.Data;
			foreach (Int2ListContainer item2 in stiffnessJointGroups)
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
				stiffnessJoints.AddGroup(list);
			}
		}

		protected override void OnDispose()
		{
			if (stiffnessJoints != null)
			{
				stiffnessJoints.Dispose();
			}
			if (stiffnessJointsBuffer != null)
			{
				stiffnessJointsBuffer.Dispose();
			}
		}
	}
}
