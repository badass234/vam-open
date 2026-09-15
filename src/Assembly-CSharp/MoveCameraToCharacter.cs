using UnityEngine;

public class MoveCameraToCharacter : MonoBehaviour
{
	public GameObject Target;

	private void Update()
	{
		base.transform.position = Target.transform.position;
	}
}
