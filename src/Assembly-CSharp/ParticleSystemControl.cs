using UnityEngine;

public class ParticleSystemControl : JSONStorable
{
	public ParticleSystem[] systems1;

	public ParticleSystem[] systems2;

	public Material mat1;

	public Material mat2;

	public bool controlEmission;

	public float systems1EmissionRateLow;

	public float systems1EmissionRateHigh = 20f;

	public float systems2EmissionRateLow;

	public float systems2EmissionRateHigh = 20f;

	public float audioSourceVolumeLow;

	public float audioSourceVolumeHigh = 1f;

	public AudioSource audioSourceControlledByRate;

	[SerializeField]
	protected bool _emissionEnabled = true;

	protected JSONStorableBool emissionEnabledJSON;

	[SerializeField]
	protected float _emissionRate = 1f;

	protected JSONStorableFloat emissionRateJSON;

	protected JSONStorableFloat system1MaterialAlphaJSON;

	protected JSONStorableFloat system2MaterialAlphaJSON;

	protected void SyncEmission()
	{
		if (!controlEmission)
		{
			return;
		}
		ParticleSystem[] array = systems1;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			if (_emissionEnabled)
			{
				emission.rateOverTime = Mathf.Lerp(systems1EmissionRateLow, systems1EmissionRateHigh, _emissionRate);
			}
			else
			{
				emission.rateOverTime = 0f;
			}
		}
		ParticleSystem[] array2 = systems2;
		foreach (ParticleSystem particleSystem2 in array2)
		{
			ParticleSystem.EmissionModule emission2 = particleSystem2.emission;
			if (_emissionEnabled)
			{
				emission2.rateOverTime = Mathf.Lerp(systems2EmissionRateLow, systems2EmissionRateHigh, _emissionRate);
			}
			else
			{
				emission2.rateOverTime = 0f;
			}
		}
		if (audioSourceControlledByRate != null)
		{
			if (_emissionEnabled)
			{
				audioSourceControlledByRate.volume = Mathf.Lerp(audioSourceVolumeLow, audioSourceVolumeHigh, _emissionRate);
			}
			else
			{
				audioSourceControlledByRate.volume = 0f;
			}
		}
	}

	protected void SyncEmissionEnabled(bool b)
	{
		_emissionEnabled = b;
		SyncEmission();
	}

	protected void SyncEmissionRate(float f)
	{
		_emissionRate = f;
		SyncEmission();
	}

	protected void SyncSystem1MaterialAlpha(float f)
	{
		ParticleSystem[] array = systems1;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				Material material = component.material;
				Color color = material.GetColor("_TintColor");
				color.a = f;
				material.SetColor("_TintColor", color);
			}
		}
	}

	protected void SyncSystem2MaterialAlpha(float f)
	{
		ParticleSystem[] array = systems2;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				Material material = component.material;
				Color color = material.GetColor("_TintColor");
				color.a = f;
				material.SetColor("_TintColor", color);
			}
		}
	}

	protected void Init()
	{
		if (mat1 != null)
		{
			system1MaterialAlphaJSON = new JSONStorableFloat("system1MaterialAlpha", mat1.GetColor("_TintColor").a, SyncSystem1MaterialAlpha, 0f, 1f);
			RegisterFloat(system1MaterialAlphaJSON);
		}
		if (mat2 != null)
		{
			system2MaterialAlphaJSON = new JSONStorableFloat("system2MaterialAlpha", mat2.GetColor("_TintColor").a, SyncSystem2MaterialAlpha, 0f, 1f);
			RegisterFloat(system2MaterialAlphaJSON);
		}
		if (controlEmission)
		{
			emissionEnabledJSON = new JSONStorableBool("emissionEnabled", _emissionEnabled, SyncEmissionEnabled);
			RegisterBool(emissionEnabledJSON);
			emissionRateJSON = new JSONStorableFloat("emissionRate", _emissionRate, SyncEmissionRate, 0f, 1f);
			RegisterFloat(emissionRateJSON);
			SyncEmission();
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		ParticleSystemControlUI componentInChildren = t.GetComponentInChildren<ParticleSystemControlUI>();
		if (componentInChildren != null)
		{
			if (system1MaterialAlphaJSON != null)
			{
				system1MaterialAlphaJSON.RegisterSlider(componentInChildren.system1MaterialAlphaSlider, isAlt);
			}
			if (system2MaterialAlphaJSON != null)
			{
				system2MaterialAlphaJSON.RegisterSlider(componentInChildren.system2MaterialAlphaSlider, isAlt);
			}
			if (controlEmission)
			{
				emissionEnabledJSON.RegisterToggle(componentInChildren.emissionEnabledToggle, isAlt);
				emissionRateJSON.RegisterSlider(componentInChildren.emissionRateSlider, isAlt);
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
