using System.Runtime.InteropServices;
using UnityEngine;

namespace Obi
{
	public class ObiTerrainShapeTracker : ObiShapeTracker
	{
		private Vector3 size;

		private int resolutionU;

		private int resolutionV;

		private GCHandle dataHandle;

		private bool heightmapDataHasChanged;

		public ObiTerrainShapeTracker(TerrainCollider collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.Heightmap);
			UpdateHeightData();
		}

		public void UpdateHeightData()
		{
			TerrainCollider terrainCollider = collider as TerrainCollider;
			if (!(terrainCollider != null))
			{
				return;
			}
			TerrainData terrainData = terrainCollider.terrainData;
			float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
			float[] array = new float[terrainData.heightmapWidth * terrainData.heightmapHeight];
			for (int i = 0; i < terrainData.heightmapHeight; i++)
			{
				for (int j = 0; j < terrainData.heightmapWidth; j++)
				{
					array[i * terrainData.heightmapWidth + j] = heights[i, j];
				}
			}
			Oni.UnpinMemory(dataHandle);
			dataHandle = Oni.PinMemory(array);
			heightmapDataHasChanged = true;
		}

		public override void UpdateIfNeeded()
		{
			TerrainCollider terrainCollider = collider as TerrainCollider;
			if (terrainCollider != null)
			{
				TerrainData terrainData = terrainCollider.terrainData;
				if (terrainData != null && (terrainData.size != size || terrainData.heightmapWidth != resolutionU || terrainData.heightmapHeight != resolutionV || heightmapDataHasChanged))
				{
					size = terrainData.size;
					resolutionU = terrainData.heightmapWidth;
					resolutionV = terrainData.heightmapHeight;
					heightmapDataHasChanged = false;
					adaptor.Set(size, resolutionU, resolutionV, dataHandle.AddrOfPinnedObject());
					Oni.UpdateShape(oniShape, ref adaptor);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			Oni.UnpinMemory(dataHandle);
		}
	}
}
