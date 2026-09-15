using UnityEngine;

public class AQUAS_BubbleBehaviour : MonoBehaviour
{
	public float averageUpdrift;

	public float waterLevel;

	public GameObject mainCamera;

	public GameObject smallBubble;

	private int smallBubbleCount;

	private int maxSmallBubbleCount;

	private AQUAS_SmallBubbleBehaviour smallBubbleBehaviour;

	private void Start()
	{
		maxSmallBubbleCount = Random.Range(20, 30);
		smallBubbleCount = 0;
		smallBubbleBehaviour = smallBubble.GetComponent<AQUAS_SmallBubbleBehaviour>();
	}

	private void Update()
	{
		base.transform.Translate(Vector3.up * Time.deltaTime * averageUpdrift, Space.World);
		SmallBubbleSpawner();
		if (mainCamera.transform.position.y > waterLevel || base.transform.position.y > waterLevel)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void SmallBubbleSpawner()
	{
		if (smallBubbleCount <= maxSmallBubbleCount)
		{
			smallBubble.transform.localScale = base.transform.localScale * Random.Range(0.05f, 0.2f);
			smallBubbleBehaviour.averageUpdrift = averageUpdrift * 0.5f;
			smallBubbleBehaviour.waterLevel = waterLevel;
			smallBubbleBehaviour.mainCamera = mainCamera;
			Object.Instantiate(smallBubble, new Vector3(base.transform.position.x + Random.Range(-0.1f, 0.1f), base.transform.position.y - Random.Range(0.01f, 1f), base.transform.position.z + Random.Range(-0.1f, 0.1f)), Quaternion.identity);
			smallBubbleCount++;
		}
	}
}
