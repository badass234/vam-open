using UnityEngine;
using UnityEngine.UI;

public class TorchLightController : JSONStorable
{
	public GameObject torchLight;

	public GameObject mainFlame;

	public GameObject baseFlame;

	public GameObject etincelles;

	public GameObject fumee;

	public Slider intensitySlider;

	public Slider intensitySliderAlt;

	public flickering flicker;

	public FlickeringLight flickeringLight;

	protected JSONStorableFloat intensityJSON;

	[SerializeField]
	protected float _intensity = 1f;

	public float intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			if (intensityJSON != null)
			{
				intensityJSON.val = value;
			}
			else if (_intensity != value)
			{
				SyncIntensity(value);
			}
		}
	}

	protected void SyncIntensity()
	{
		Light component = torchLight.GetComponent<Light>();
		if (component != null)
		{
			component.intensity = intensity;
		}
		ParticleSystem component2 = mainFlame.GetComponent<ParticleSystem>();
		ParticleSystem.EmissionModule emission = component2.emission;
		emission.rateOverTime = intensity * 20f;
		component2 = baseFlame.GetComponent<ParticleSystem>();
		emission = component2.emission;
		emission.rateOverTime = intensity * 15f;
		component2 = etincelles.GetComponent<ParticleSystem>();
		emission = component2.emission;
		emission.rateOverTime = intensity * 7f;
		component2 = fumee.GetComponent<ParticleSystem>();
		emission = component2.emission;
		emission.rateOverTime = intensity * 12f;
		if (flicker != null)
		{
			flicker.intensity = intensity;
		}
		if (flickeringLight != null)
		{
			flickeringLight.intensityOrigin = intensity;
		}
	}

	protected void SyncIntensity(float f)
	{
		_intensity = f;
		SyncIntensity();
	}

	protected void Init()
	{
		intensityJSON = new JSONStorableFloat("intensity", _intensity, SyncIntensity, 0f, 5f);
		RegisterFloat(intensityJSON);
		SyncIntensity();
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			TorchLightControllerUI componentInChildren = UITransform.GetComponentInChildren<TorchLightControllerUI>(true);
			if (componentInChildren != null)
			{
				intensityJSON.slider = componentInChildren.intensitySlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			TorchLightControllerUI componentInChildren = UITransformAlt.GetComponentInChildren<TorchLightControllerUI>(true);
			if (componentInChildren != null)
			{
				intensityJSON.sliderAlt = componentInChildren.intensitySlider;
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
