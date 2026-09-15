using UnityEngine;

public class Torchelight : MonoBehaviour
{
	public GameObject TorchLight;

	public GameObject MainFlame;

	public GameObject BaseFlame;

	public GameObject Etincelles;

	public GameObject Fumee;

	public float MaxLightIntensity;

	public float IntensityLight;

	private void Start()
	{
		TorchLight.GetComponent<Light>().intensity = IntensityLight;
		ParticleSystem.EmissionModule emission = MainFlame.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 20f;
		emission = BaseFlame.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 15f;
		emission = Etincelles.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 7f;
		emission = Fumee.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 12f;
	}

	private void Update()
	{
		if (IntensityLight < 0f)
		{
			IntensityLight = 0f;
		}
		if (IntensityLight > MaxLightIntensity)
		{
			IntensityLight = MaxLightIntensity;
		}
		TorchLight.GetComponent<Light>().intensity = IntensityLight / 2f + Mathf.Lerp(IntensityLight - 0.1f, IntensityLight + 0.1f, Mathf.Cos(Time.time * 30f));
		TorchLight.GetComponent<Light>().color = new Color(Mathf.Min(IntensityLight / 1.5f, 1f), Mathf.Min(IntensityLight / 2f, 1f), 0f);
		ParticleSystem.EmissionModule emission = MainFlame.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 20f;
		emission = BaseFlame.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 15f;
		emission = Etincelles.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 7f;
		emission = Fumee.GetComponent<ParticleSystem>().emission;
		emission.rateOverTime = IntensityLight * 12f;
	}
}
