using GPUTools.Skinner.Scripts.Kernels;
using UnityEngine;

namespace GPUTools.HairDemo.Scripts.BlendShapesDemo
{
	public class TestBlendShapes : MonoBehaviour
	{
		[SerializeField]
		private SkinnedMeshRenderer skin;

		private GPUSkinnerPro skinner;

		private Vector3[] vertices;

		private void Start()
		{
			skinner = new GPUSkinnerPro(skin);
			skinner.Dispatch();
			vertices = skin.sharedMesh.vertices;
		}

		private void Update()
		{
			skinner.Dispatch();
		}

		private void OnDestroy()
		{
			skinner.Dispose();
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				skinner.TransformMatricesBuffer.PullData();
				Matrix4x4[] data = skinner.TransformMatricesBuffer.Data;
				for (int i = 0; i < vertices.Length; i++)
				{
					Vector3 point = vertices[i];
					Vector3 center = data[i].MultiplyPoint3x4(point);
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere(center, 0.002f);
				}
			}
		}
	}
}
