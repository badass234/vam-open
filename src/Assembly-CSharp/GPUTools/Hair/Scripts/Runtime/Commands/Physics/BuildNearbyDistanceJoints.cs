using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Types;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildNearbyDistanceJoints : BuildChainCommand
	{
		private readonly HairSettings settings;

		private GroupedData<GPDistanceJoint> nearbyDistanceJoints;

		private GpuBuffer<GPDistanceJoint> nearbyDistanceJointsBuffer;

		public BuildNearbyDistanceJoints(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreateNearbyDistanceJoints();
			if (nearbyDistanceJointsBuffer != null)
			{
				nearbyDistanceJointsBuffer.Dispose();
			}
			if (nearbyDistanceJoints.Data.Length > 0)
			{
				nearbyDistanceJointsBuffer = new GpuBuffer<GPDistanceJoint>(nearbyDistanceJoints.Data, GPDistanceJoint.Size());
			}
			else
			{
				nearbyDistanceJointsBuffer = null;
			}
			settings.RuntimeData.NearbyDistanceJointsBuffer = nearbyDistanceJointsBuffer;
			settings.RuntimeData.NearbyDistanceJoints = nearbyDistanceJoints;
		}

		private void AddToHashSet(HashSet<Vector3> set, int i1, int i2, float distance)
		{
			if (i1 != -1 && i2 != -1)
			{
				set.Add((i1 <= i2) ? new Vector3(i2, i1, distance) : new Vector3(i1, i2, distance));
			}
		}

		private void CreateNearbyDistanceJoints()
		{
			nearbyDistanceJoints = new GroupedData<GPDistanceJoint>();
			int segments = settings.StandsSettings.Segments;
			float num = segments - 1;
			int num2 = 0;
			List<Vector4ListContainer> nearbyVertexGroups = settings.StandsSettings.Provider.GetNearbyVertexGroups();
			if (nearbyVertexGroups == null)
			{
				return;
			}
			foreach (Vector4ListContainer item2 in nearbyVertexGroups)
			{
				List<GPDistanceJoint> list = new List<GPDistanceJoint>();
				nearbyDistanceJoints.AddGroup(list);
				foreach (Vector4 item3 in item2.List)
				{
					int num3 = (int)item3.x;
					float num4 = 1f - (float)(num3 % segments) / num;
					int num5 = (int)item3.y;
					float num6 = 1f - (float)(num5 % segments) / num;
					float elasticity = (num4 + num6) * 0.5f;
					GPDistanceJoint item = new GPDistanceJoint(num3, num5, item3.z, elasticity);
					list.Add(item);
					num2++;
				}
			}
		}

		protected override void OnDispose()
		{
			if (nearbyDistanceJoints != null)
			{
				nearbyDistanceJoints.Dispose();
			}
			if (nearbyDistanceJointsBuffer != null)
			{
				nearbyDistanceJointsBuffer.Dispose();
			}
		}
	}
}
