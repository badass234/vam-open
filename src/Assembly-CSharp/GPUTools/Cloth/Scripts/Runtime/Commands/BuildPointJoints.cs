using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildPointJoints : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private List<GPPointJoint> pointJoints;

		private List<GPPointJoint> allPointJoints;

		private GpuBuffer<GPPointJoint> pointJointsBuffer;

		private GpuBuffer<GPPointJoint> allPointJointsBuffer;

		public BuildPointJoints(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			CreatePointJoints();
			if (pointJoints.Count > 0)
			{
				pointJointsBuffer = new GpuBuffer<GPPointJoint>(pointJoints.ToArray(), GPPointJoint.Size());
				settings.Runtime.PointJoints = pointJointsBuffer;
			}
			if (allPointJoints.Count > 0)
			{
				allPointJointsBuffer = new GpuBuffer<GPPointJoint>(allPointJoints.ToArray(), GPPointJoint.Size());
				settings.Runtime.AllPointJoints = allPointJointsBuffer;
			}
		}

		protected override void OnUpdateSettings()
		{
			CreatePointJoints();
			if (pointJointsBuffer != null)
			{
				pointJointsBuffer.Data = pointJoints.ToArray();
				pointJointsBuffer.PushData();
			}
			if (allPointJointsBuffer != null)
			{
				allPointJointsBuffer.Data = allPointJoints.ToArray();
				allPointJointsBuffer.PushData();
			}
		}

		private void CreatePointJoints()
		{
			Vector3[] particles = settings.GeometryData.Particles;
			int[] physicsToMeshVerticesMap = settings.GeometryData.PhysicsToMeshVerticesMap;
			float[] particlesBlend = settings.GeometryData.ParticlesBlend;
			pointJoints = new List<GPPointJoint>();
			allPointJoints = new List<GPPointJoint>();
			for (int i = 0; i < particles.Length; i++)
			{
				Vector3 point = particles[i];
				int num = physicsToMeshVerticesMap[i];
				float f = particlesBlend[num];
				int matrixId = ((settings.MeshProvider.Type != 0) ? num : 0);
				float rigidity = Mathf.Pow(f, 4f);
				pointJoints.Add(new GPPointJoint(i, matrixId, point, rigidity));
				allPointJoints.Add(new GPPointJoint(i, matrixId, point, rigidity));
			}
		}

		protected override void OnDispose()
		{
			if (settings.Runtime.PointJoints != null)
			{
				settings.Runtime.PointJoints.Dispose();
			}
			if (settings.Runtime.AllPointJoints != null)
			{
				settings.Runtime.AllPointJoints.Dispose();
			}
		}
	}
}
