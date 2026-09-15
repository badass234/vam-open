using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
	public delegate void MainLoop();

	[Tooltip("This is how big the light is. Experiment with it.")]
	public float scale;

	[Tooltip("The moves (new targets for properties; intensity, range, position) your light will do per second.")]
	public float speed;

	[HideInInspector]
	public bool MakeSourceStationary;

	[HideInInspector]
	public float positionOffset;

	private Light light;

	public float intensityOrigin;

	private float intensityOffset;

	private float intensityDelta;

	private float rangeOrigin;

	private float rangeOffset;

	private float rangeTarget;

	private float rangeDelta;

	private Vector3 positionOrigin;

	private Vector3 positionDelta;

	private bool setNewTargets;

	private float deltaSum;

	public event MainLoop mainLoop;

	private void Start()
	{
		light = GetComponent<Light>();
		intensityOrigin = light.intensity;
		rangeOrigin = light.range;
		positionOrigin = base.transform.localPosition;
		setNewTargets = true;
		mainLoop += IntensityAndRange;
		if (!MakeSourceStationary)
		{
			mainLoop += Position;
		}
	}

	private void IntensityAndRange()
	{
		if (setNewTargets)
		{
			intensityOffset = intensityOrigin * scale;
			rangeOffset = rangeOrigin * scale * 0.3f;
			intensityDelta = (intensityOrigin + Random.Range(0f - intensityOffset, intensityOffset) - light.intensity) * speed;
			rangeTarget = rangeOrigin + Random.Range(0f - rangeOffset, rangeOffset);
			rangeDelta = (rangeTarget - light.range) * speed;
			setNewTargets = false;
		}
		light.intensity += intensityDelta;
		light.range += rangeDelta;
		if (rangeDelta == 0f)
		{
			setNewTargets = true;
		}
		else if (rangeDelta > 0f)
		{
			if (light.range > rangeTarget)
			{
				setNewTargets = true;
			}
		}
		else if (light.range < rangeTarget || light.range < 0f)
		{
			setNewTargets = true;
		}
	}

	private void Position()
	{
		if (setNewTargets)
		{
			positionDelta = (positionOrigin + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * positionOffset - base.transform.localPosition) * speed;
		}
		base.transform.localPosition += positionDelta;
	}

	private void Update()
	{
		if (deltaSum >= 0.02f)
		{
			this.mainLoop();
			deltaSum -= 0.02f;
		}
		deltaSum += Time.deltaTime;
	}
}
