using UnityEngine;

[ExecuteInEditMode]
public class LightmappingManager : MonoBehaviour
{
	[SerializeField]
	public Renderer[] sceneRenderers;

	public LightMapDataContainerObject lighmapDataContainer;

	[SerializeField]
	public Texture2D[] lightMapTexturesFar;

	private void Awake()
	{
		SetLightMapData();
		SetLightMapTextures();
	}

	public void SetLightMapTextures()
	{
		if (lightMapTexturesFar != null && lightMapTexturesFar.Length > 0)
		{
			LightmapData[] array = new LightmapData[lightMapTexturesFar.Length];
			for (int i = 0; i < lightMapTexturesFar.Length; i++)
			{
				array[i] = new LightmapData();
				array[i].lightmapColor = lightMapTexturesFar[i];
			}
			LightmapSettings.lightmaps = array;
		}
	}

	public void SetLightMapData()
	{
		if (sceneRenderers.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < sceneRenderers.Length; i++)
		{
			if ((bool)sceneRenderers[i])
			{
				sceneRenderers[i].lightmapIndex = lighmapDataContainer.lightmapIndexes[i];
				sceneRenderers[i].lightmapScaleOffset = lighmapDataContainer.lightmapOffsetScales[i];
			}
		}
	}
}
