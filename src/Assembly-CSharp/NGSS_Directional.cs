using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class NGSS_Directional : MonoBehaviour
{
	public enum SAMPLER_COUNT
	{
		SAMPLERS_16,
		SAMPLERS_25,
		SAMPLERS_32,
		SAMPLERS_64
	}

	[Header("MAIN SETTINGS")]
	[Tooltip("If false, NGSS Directional shadows replacement will be removed from Graphics settings when OnDisable is called in this component.")]
	public bool KEEP_NGSS_ONDISABLE = true;

	[Header("OPTIMIZATION")]
	[Tooltip("Optimize shadows performance by skipping fragments that are either 100% lit or 100% shadowed. Some macro noisy artefacts can be seen if shadows are too soft or sampling amount is below 64.")]
	public bool EARLY_BAILOUT_OPTIMIZATION = true;

	[Tooltip("Recommended values: Mobile = 16, Consoles = 25, Desktop VR = 32, Desktop High = 64")]
	public SAMPLER_COUNT SAMPLERS_COUNT = SAMPLER_COUNT.SAMPLERS_64;

	[Header("SOFTNESS")]
	[Tooltip("Overall softness for both PCF and PCSS shadows.")]
	[Range(0f, 2f)]
	public float GLOBAL_SOFTNESS = 1f;

	[Header("CASCADES")]
	[Tooltip("Blends cascades at seams intersection.\nAdditional overhead required for this option.")]
	public bool CASCADES_BLENDING = true;

	[Tooltip("Blends cascades at seams intersection.\nAdditional overhead required for this option.")]
	[Range(0f, 2f)]
	public float CASCADES_BLENDING_VALUE = 1f;

	[Header("NOISE")]
	[Tooltip("If disabled, noise will be computed normally.\nIf enabled, noise will be computed statically from an internal screen-space texture.")]
	public bool NOISE_STATIC;

	[Tooltip("Amount of noise. The higher the value the more Noise.")]
	[Range(0f, 2f)]
	public float NOISE_SCALE_VALUE = 1f;

	[Header("PCSS")]
	[Tooltip("PCSS Requires inline sampling and SM3.5, only available in Unity 2017.\nIt provides Area Light like soft-shadows.\nDisable it if you are looking for PCF filtering (uniform soft-shadows) which runs with SM3.0.")]
	public bool PCSS_ENABLED = true;

	[Tooltip("PCSS softness when shadows is close to caster.")]
	[Range(0f, 2f)]
	public float PCSS_SOFTNESS_MIN = 1f;

	[Tooltip("PCSS softness when shadows is far from caster.")]
	[Range(0f, 2f)]
	public float PCSS_SOFTNESS_MAX = 1f;

	private bool isInitialized;

	private bool isGraphicSet;

	private void OnDisable()
	{
		isInitialized = false;
		if (!KEEP_NGSS_ONDISABLE && isGraphicSet)
		{
			isGraphicSet = false;
			GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
			GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
		}
	}

	private void OnEnable()
	{
		if (IsNotSupported())
		{
			Debug.LogWarning("Unsupported graphics API, NGSS requires at least SM3.0 or higher and DX9 is not supported.", this);
			base.enabled = false;
		}
		else
		{
			Init();
		}
	}

	private void Init()
	{
		if (!isInitialized)
		{
			if (!isGraphicSet)
			{
				GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
				GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/NGSS_Directional"));
				isGraphicSet = true;
			}
			isInitialized = true;
		}
	}

	private bool IsNotSupported()
	{
		return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationVita || SystemInfo.graphicsDeviceType == GraphicsDeviceType.N3DS;
	}

	private void Update()
	{
		if (NOISE_STATIC)
		{
			Shader.EnableKeyword("NGSS_NOISE_STATIC_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_NOISE_STATIC_DIR");
		}
		if (CASCADES_BLENDING && QualitySettings.shadowCascades > 1)
		{
			Shader.EnableKeyword("NGSS_USE_CASCADE_BLENDING");
			Shader.SetGlobalFloat("NGSS_CASCADE_BLEND_DISTANCE", CASCADES_BLENDING_VALUE * 0.125f);
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_CASCADE_BLENDING");
		}
		if (EARLY_BAILOUT_OPTIMIZATION)
		{
			Shader.EnableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
		}
		Shader.SetGlobalFloat("NGSS_POISSON_SAMPLING_NOISE_DIR", NOISE_SCALE_VALUE / 0.01f);
		Shader.SetGlobalFloat("NGSS_STATIC_NOISE_MOBILE_VALUE", NOISE_SCALE_VALUE * 0.5f);
		Shader.SetGlobalFloat("NGSS_PCSS_GLOBAL_SOFTNESS", GLOBAL_SOFTNESS / (QualitySettings.shadowDistance * 0.66f));
		Shader.SetGlobalFloat("NGSS_PCSS_GLOBAL_SOFTNESS_MOBILE", 1f - GLOBAL_SOFTNESS * 75f / QualitySettings.shadowDistance);
		if (PCSS_ENABLED)
		{
			Shader.EnableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		float num = PCSS_SOFTNESS_MIN * 0.05f;
		float num2 = PCSS_SOFTNESS_MAX * 0.25f;
		Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MIN", (!(num > num2)) ? num : num2);
		Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MAX", (!(num2 < num)) ? num2 : num);
		Shader.DisableKeyword("DIR_POISSON_64");
		Shader.DisableKeyword("DIR_POISSON_32");
		Shader.DisableKeyword("DIR_POISSON_25");
		Shader.DisableKeyword("DIR_POISSON_16");
		Shader.EnableKeyword((SAMPLERS_COUNT == SAMPLER_COUNT.SAMPLERS_64) ? "DIR_POISSON_64" : ((SAMPLERS_COUNT == SAMPLER_COUNT.SAMPLERS_32) ? "DIR_POISSON_32" : ((SAMPLERS_COUNT != SAMPLER_COUNT.SAMPLERS_25) ? "DIR_POISSON_16" : "DIR_POISSON_25")));
	}
}
