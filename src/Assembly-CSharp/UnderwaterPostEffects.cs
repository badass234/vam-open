using UnityEngine;

public class UnderwaterPostEffects : MonoBehaviour
{
	public Color FogColor = new Color(29f / 85f, 38f / 51f, 73f / 85f, 1f);

	public float FogDensity = 0.05f;

	public bool UseSunShafts = true;

	public float ShuftsIntensity = 5f;

	public WFX_SunShafts.ShaftsScreenBlendMode SunShuftsScreenBlend;

	private Vector3 SunShaftTargetPosition = new Vector3(0f, 7f, 10f);

	private Camera cam;

	private WFX_SunShafts SunShafts;

	private void OnEnable()
	{
		cam = Camera.main;
		SunShafts = cam.gameObject.AddComponent<WFX_SunShafts>();
		SunShafts.sunShaftIntensity = ShuftsIntensity;
		GameObject gameObject = new GameObject("SunShaftTarget");
		SunShafts.sunTransform = gameObject.transform;
		gameObject.transform.parent = cam.transform;
		gameObject.transform.localPosition = SunShaftTargetPosition;
		SunShafts.screenBlendMode = SunShuftsScreenBlend;
		SunShafts.sunShaftsShader = Shader.Find("Hidden/SunShaftsComposite");
		SunShafts.simpleClearShader = Shader.Find("Hidden/SimpleClear");
		Underwater underwater = cam.gameObject.AddComponent<Underwater>();
		underwater.UnderwaterLevel = base.transform.position.y;
		underwater.FogColor = FogColor;
		underwater.FogDensity = FogDensity;
	}

	private void Update()
	{
		if (cam == null)
		{
			return;
		}
		if (cam.transform.position.y < base.transform.position.y)
		{
			if (!SunShafts.enabled)
			{
				SunShafts.enabled = true;
			}
		}
		else if (SunShafts.enabled)
		{
			SunShafts.enabled = false;
		}
	}
}
