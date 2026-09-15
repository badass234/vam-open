using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Commands;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Skinner.Scripts
{
	public class SkinnerSettings : MonoBehaviour
	{
		[SerializeField]
		public bool DebugDraw;

		[SerializeField]
		public SkinnedMeshProvider MeshProvider = new SkinnedMeshProvider();

		private SkinnerCommand command;

		public GpuBuffer<Matrix4x4> SelectedToWorldMatricesBuffer => command.SelectedMatrices;

		public GpuBuffer<Matrix4x4> ToWorldMatricesBuffer => MeshProvider.ToWorldMatricesBuffer;

		public GpuBuffer<Vector3> SelectedWorldVerticesBuffer => command.SelectedPoints;

		public GpuBuffer<Vector3> WorldVerticesBuffer => command.Points;

		public void Initialize(int[] indices = null)
		{
			command = new SkinnerCommand(MeshProvider, indices);
			command.Build();
		}

		private void OnDestroy()
		{
			MeshProvider.Dispose();
			if (command != null)
			{
				command.Dispose();
			}
		}

		public void Dispatch()
		{
			if (MeshProvider.Validate(false) && command != null)
			{
				MeshProvider.Dispatch();
				command.Dispatch();
			}
		}

		private void OnDrawGizmos()
		{
			if (DebugDraw && Application.isPlaying && MeshProvider.Validate(false))
			{
				int[] triangles = MeshProvider.Mesh.triangles;
				Vector3[] vertices = MeshProvider.Mesh.vertices;
				MeshProvider.Dispatch();
				MeshProvider.ToWorldMatricesBuffer.PullData();
				Matrix4x4[] data = MeshProvider.ToWorldMatricesBuffer.Data;
				Gizmos.color = Color.magenta;
				for (int i = 0; i < triangles.Length; i += 3)
				{
					int num = triangles[i];
					int num2 = triangles[i + 1];
					int num3 = triangles[i + 2];
					Vector3 vector = data[num].MultiplyPoint3x4(vertices[num]);
					Vector3 vector2 = data[num2].MultiplyPoint3x4(vertices[num2]);
					Vector3 vector3 = data[num3].MultiplyPoint3x4(vertices[num3]);
					Gizmos.DrawLine(vector, vector2);
					Gizmos.DrawLine(vector2, vector3);
					Gizmos.DrawLine(vector3, vector);
				}
			}
		}
	}
}
