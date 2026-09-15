using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Debug
{
	public class DebugVertices : MonoBehaviour
	{
		public List<Vector3> Vertices;

		public float Radius;

		public static DebugVertices Draw(List<Vector3> vertices, float radius)
		{
			GameObject gameObject = new GameObject("DebugVertices");
			DebugVertices debugVertices = gameObject.AddComponent<DebugVertices>();
			debugVertices.Vertices = vertices;
			debugVertices.Radius = radius;
			return debugVertices;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			foreach (Vector3 vertex in Vertices)
			{
				Gizmos.DrawWireSphere(vertex, Radius);
			}
		}
	}
}
