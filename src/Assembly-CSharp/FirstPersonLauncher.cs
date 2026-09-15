using UnityEngine;

public class FirstPersonLauncher : MonoBehaviour
{
	public GameObject prefab;

	public float power = 2f;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			GameObject gameObject = Object.Instantiate(prefab, ray.origin, Quaternion.identity);
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity = ray.direction * power;
			}
		}
	}
}
