using UnityEngine;

public class MeshRendererExample : MonoBehaviour
{
	private float dir = 1f;

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, dir, 0f) * 50f * Time.deltaTime);
		if ((double)base.transform.rotation.y > 0.8)
		{
			dir = -1f;
		}
		if ((double)base.transform.rotation.y < 0.3)
		{
			dir = 1f;
		}
	}
}
