using UnityEngine;

public class EnableLink : MonoBehaviour
{
	public Transform linkTransform;

	private void OnEnable()
	{
		if (linkTransform != null)
		{
			linkTransform.gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if (linkTransform != null)
		{
			linkTransform.gameObject.SetActive(false);
		}
	}
}
