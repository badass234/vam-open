using GPUTools.Cloth.Scripts.Runtime.Data;
using GPUTools.Hair.Scripts.Utils;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Render
{
	public class ClothRender : MonoBehaviour
	{
		private ClothDataFacade data;

		public void Initialize(ClothDataFacade data)
		{
			this.data = data;
			Update();
		}

		private void UpdateBounds()
		{
			data.MeshProvider.Mesh.bounds = base.transform.InverseTransformBounds(data.Bounds);
		}

		private void Update()
		{
			for (int i = 0; i < data.Materials.Length; i++)
			{
				Material material = data.Materials[i];
				material.EnableKeyword("VERTEX_FROM_BUFFER");
				material.SetBuffer("_ClothVertices", data.ClothVertices.ComputeBuffer);
			}
			if (data.CustomBounds)
			{
				UpdateBounds();
			}
		}
	}
}
