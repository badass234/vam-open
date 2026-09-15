using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	public class SkinnedMeshCollider : MonoBehaviour
	{
		[SerializeField]
		private bool debugDraw;

		[SerializeField]
		private MeshFilter filter;

		public Vector3[] Vertices
		{
			get
			{
				Vector3[] array = new Vector3[filter.sharedMesh.vertexCount];
				Vector3[] vertices = filter.sharedMesh.vertices;
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = base.transform.TransformPoint(vertices[i]);
				}
				return array;
			}
		}

		private void OnDrawGizmos()
		{
			if (Vertices != null && debugDraw)
			{
				Gizmos.color = Color.red;
				Vector3[] vertices = Vertices;
				foreach (Vector3 position in vertices)
				{
					Gizmos.DrawWireSphere(base.transform.TransformPoint(position), 0.01f);
				}
			}
		}
	}
}
