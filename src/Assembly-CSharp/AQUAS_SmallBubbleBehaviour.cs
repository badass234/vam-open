using UnityEngine;

public class AQUAS_SmallBubbleBehaviour : MonoBehaviour
{
	public float averageUpdrift;

	public float waterLevel;

	public GameObject mainCamera;

	private float updriftFactor;

	private void Start()
	{
		updriftFactor = Random.Range((0f - averageUpdrift) * 0.75f, averageUpdrift * 0.75f);
	}

	private void Update()
	{
		base.transform.Translate(Vector3.up * Time.deltaTime * (averageUpdrift + updriftFactor), Space.World);
		if (mainCamera.transform.position.y > waterLevel || base.transform.position.y > waterLevel)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
