using UnityEngine;

public class ObjectLimit : MonoBehaviour
{
	public float minX;

	public float maxX = 1f;

	public float minY;

	public float maxY = 1f;

	public float minZ;

	public float maxZ = 1f;

	private void Update()
	{
		base.transform.localPosition = new Vector3(Mathf.Clamp(base.gameObject.transform.localPosition.x, minX, maxX), Mathf.Clamp(base.gameObject.transform.localPosition.y, minY, maxY), Mathf.Clamp(base.gameObject.transform.localPosition.z, minZ, maxZ));
	}
}
