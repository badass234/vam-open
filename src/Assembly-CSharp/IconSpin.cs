using UnityEngine;

public class IconSpin : MonoBehaviour
{
	[Range(-10f, 10f)]
	public float mRotationSpeed = 0.5f;

	private void Start()
	{
	}

	private void Update()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.y += mRotationSpeed;
		base.transform.localEulerAngles = localEulerAngles;
	}
}
