using UnityEngine;

public class fire_wiggle : MonoBehaviour
{
	private float t;

	private float wiggle_t;

	public float fire_k = 1f;

	private float initial_start_speed;

	private float initial_emission_rate;

	private float initial_lifetime;

	private float initial_size;

	private float randomizer;

	private void Start()
	{
		randomizer = Random.Range(0.75f, 1.25f);
		ParticleSystem component = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component.main;
		ParticleSystem.EmissionModule emission = component.emission;
		initial_start_speed = main.startSpeed.constant;
		initial_emission_rate = emission.rateOverTime.constant;
		initial_lifetime = main.startLifetime.constant;
		initial_size = main.startSize.constant;
	}

	private void Update()
	{
		t += Time.deltaTime * randomizer;
		wiggle_t += Time.deltaTime * randomizer;
		ParticleSystem component = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component.main;
		ParticleSystem.EmissionModule emission = component.emission;
		if (t > 2f + (2f - Mathf.Sin(wiggle_t)))
		{
			emission.rateOverTime = emission.rateOverTime.constant + (initial_emission_rate * 0.4f * fire_k - emission.rateOverTime.constant) / 30f;
			main.startLifetime = main.startLifetime.constant + (initial_lifetime * 0.9f * fire_k - main.startLifetime.constant) / 30f;
			if (emission.rateOverTime.constant < initial_emission_rate * 0.42f * fire_k)
			{
				emission.rateOverTime = initial_emission_rate * 1.1f * fire_k;
				main.startLifetime = initial_lifetime * 1.1f * fire_k;
				main.startSpeed = initial_start_speed * 0.7f * fire_k;
				main.startSize = initial_size * 1.1f * fire_k;
				randomizer = Random.Range(0.75f, 1.25f);
				t = 0f;
			}
		}
		else
		{
			emission.rateOverTime = emission.rateOverTime.constant + (initial_emission_rate - emission.rateOverTime.constant) / 30f;
			main.startLifetime = main.startLifetime.constant + (initial_lifetime - main.startLifetime.constant) / 100f;
			main.startSpeed = main.startSpeed.constant + (initial_start_speed - main.startSpeed.constant) / 30f;
			main.startSize = main.startSize.constant + (initial_size - main.startSize.constant) / 30f;
		}
	}
}
