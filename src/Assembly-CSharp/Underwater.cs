using UnityEngine;

public class Underwater : MonoBehaviour
{
	public float UnderwaterLevel;

	public Color FogColor = new Color(0f, 0.4f, 0.7f, 1f);

	public float FogDensity = 0.04f;

	public FogMode FogMode = FogMode.Exponential;

	private bool defaultFog;

	private Color defaultFogColor;

	private float defaultFogDensity;

	private FogMode defaultFogMod;

	private Material defaultSkybox;

	private void Start()
	{
		defaultFog = RenderSettings.fog;
		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultFogMod = RenderSettings.fogMode;
	}

	private void Update()
	{
		if (base.transform.position.y < UnderwaterLevel)
		{
			RenderSettings.fog = true;
			RenderSettings.fogColor = FogColor;
			RenderSettings.fogDensity = FogDensity;
			RenderSettings.fogMode = FogMode;
		}
		else
		{
			RenderSettings.fog = defaultFog;
			RenderSettings.fogColor = defaultFogColor;
			RenderSettings.fogDensity = defaultFogDensity;
			RenderSettings.fogMode = defaultFogMod;
			RenderSettings.fogStartDistance = -300f;
		}
	}
}
