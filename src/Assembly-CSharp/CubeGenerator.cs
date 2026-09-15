using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
	public GameObject cubes;

	private void Start()
	{
		InvokeRepeating("UpdateCube", 1f, 2f);
	}

	private void UpdateCube()
	{
		Vector3 position = base.transform.position;
		position.y += 10f;
		position.z -= 4f;
		position += Random.insideUnitSphere * 7f;
		GameObject gameObject = Object.Instantiate(cubes, position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
		gameObject.AddComponent<Buoyancy>().Density = Random.Range(700, 850);
		gameObject.AddComponent<Rigidbody>().mass = Random.Range(100, 150);
		Object.Destroy(gameObject, 30f);
	}
}
