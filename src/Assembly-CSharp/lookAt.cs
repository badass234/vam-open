using UnityEngine;

public class lookAt : MonoBehaviour
{
	public Transform player;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.LookAt(player);
	}
}
