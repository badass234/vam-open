using UnityEngine;

public class flickering : MonoBehaviour
{
	public float intensity;

	public float rate = 0.02f;

	private float t;

	private float randomizer;

	public float intensityTarget;

	private Light light;

	private void Start()
	{
		light = GetComponent<Light>();
		if (light != null)
		{
			intensity = light.intensity;
			intensityTarget = intensity;
			randomizer = Random.Range(0.75f, 1.25f);
		}
	}

	private void Update()
	{
		if (light != null)
		{
			t += Time.deltaTime * randomizer * rate;
			intensityTarget = intensity + Mathf.Sin(t * (1f - Mathf.Sin(t * 25f)) * 5f) * intensity / 5f;
			light.intensity = intensityTarget;
		}
	}
}
