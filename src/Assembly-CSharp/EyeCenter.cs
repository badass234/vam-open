using UnityEngine;

public class EyeCenter : MonoBehaviour
{
	public Transform leftEye;

	public Transform rightEye;

	protected void Update()
	{
		if (leftEye != null && rightEye != null)
		{
			base.transform.position = (leftEye.position + rightEye.position) * 0.5f;
		}
	}
}
