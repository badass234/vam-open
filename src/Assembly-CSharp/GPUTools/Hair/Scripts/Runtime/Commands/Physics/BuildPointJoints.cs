using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Geometry.Constrains;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildPointJoints : BuildChainCommand
	{
		private readonly HairSettings settings;

		public BuildPointJoints(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			GPPointJoint[] array = new GPPointJoint[settings.StandsSettings.Provider.GetVertices().Count];
			CreatePointJoints(array);
			if (settings.RuntimeData.PointJoints != null)
			{
				settings.RuntimeData.PointJoints.Dispose();
			}
			if (array.Length > 0)
			{
				settings.RuntimeData.PointJoints = new GpuBuffer<GPPointJoint>(array, GPPointJoint.Size());
			}
			else
			{
				settings.RuntimeData.PointJoints = null;
			}
		}

		protected override void OnUpdateSettings()
		{
			if (settings.RuntimeData.PointJoints != null)
			{
				CreatePointJoints(settings.RuntimeData.PointJoints.Data);
				settings.RuntimeData.PointJoints.PushData();
			}
		}

		public void UpdateSettingsPreserveData()
		{
			settings.RuntimeData.PointJoints.PullData();
			CreatePointJoints(settings.RuntimeData.PointJoints.Data, true);
			settings.RuntimeData.PointJoints.PushData();
		}

		public void RebuildFromGPUData()
		{
			settings.RuntimeData.PointJoints.PullData();
			GPPointJoint[] data = settings.RuntimeData.PointJoints.Data;
			List<Vector3> list = new List<Vector3>();
			List<float> list2 = new List<float>();
			for (int i = 0; i < data.Length; i++)
			{
				list.Add(data[i].Point);
				list2.Add(data[i].Rigidity);
			}
			settings.StandsSettings.Provider.SetVertices(list);
			if (settings.PhysicsSettings.UsePaintedRigidity)
			{
				settings.StandsSettings.Provider.SetRigidities(list2);
			}
			else
			{
				settings.StandsSettings.Provider.SetRigidities(null);
			}
			OnUpdateSettings();
		}

		private void CreatePointJoints(GPPointJoint[] pointJoints, bool reuse = false)
		{
			List<Vector3> vertices = settings.StandsSettings.Provider.GetVertices();
			List<float> rigidities = settings.StandsSettings.Provider.GetRigidities();
			int segments = settings.StandsSettings.Segments;
			int[] hairRootToScalpMap = settings.StandsSettings.Provider.GetHairRootToScalpMap();
			Vector3 zero = Vector3.zero;
			bool usePaintedRigidity = settings.PhysicsSettings.UsePaintedRigidity;
			float rootRigidity = settings.PhysicsSettings.RootRigidity;
			float mainRigidity = settings.PhysicsSettings.MainRigidity;
			float tipRigidity = settings.PhysicsSettings.TipRigidity;
			float rigidityRolloffPower = settings.PhysicsSettings.RigidityRolloffPower;
			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 vector = vertices[i];
				int num = i / segments;
				int matrixId = hairRootToScalpMap[num];
				int num2 = i % segments;
				float num3;
				if (num2 == 0)
				{
					num3 = 1.1f;
				}
				else if (usePaintedRigidity && rigidities != null)
				{
					num3 = rigidities[i];
				}
				else if (num2 == 1)
				{
					num3 = rootRigidity;
				}
				else
				{
					float num4 = ((float)num2 - 1f) / (float)(segments - 2);
					float f = 1f - num4;
					float t = Mathf.Pow(f, rigidityRolloffPower);
					num3 = Mathf.Lerp(tipRigidity, mainRigidity, t);
				}
				num3 += JointAreaAdd(vector);
				if (reuse)
				{
					if (num2 == 0)
					{
						pointJoints[i].Rigidity = 1.1f;
					}
					else
					{
						pointJoints[i].Rigidity = Mathf.Clamp01(num3);
					}
				}
				else if (num2 == 0)
				{
					pointJoints[i] = new GPPointJoint(i, matrixId, vector, 1.1f);
				}
				else
				{
					pointJoints[i] = new GPPointJoint(i, matrixId, vector, Mathf.Clamp01(num3));
				}
			}
		}

		private float JointAreaAdd(Vector3 vertex)
		{
			float num = 0f;
			foreach (HairJointArea jointArea in settings.PhysicsSettings.JointAreas)
			{
				float magnitude = (vertex - jointArea.transform.localPosition).magnitude;
				if (magnitude < jointArea.Radius)
				{
					float num2 = (jointArea.Radius - magnitude) / jointArea.Radius;
					num += num2 * settings.PhysicsSettings.JointRigidity;
				}
			}
			return num;
		}

		protected override void OnDispose()
		{
			if (settings.RuntimeData.PointJoints != null)
			{
				settings.RuntimeData.PointJoints.Dispose();
			}
		}
	}
}
