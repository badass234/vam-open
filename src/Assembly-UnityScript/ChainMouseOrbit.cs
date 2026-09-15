using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class ChainMouseOrbit : MonoBehaviour
{
	public Transform target;

	public float distance;

	public Vector3 targetOffset;

	public float xSpeed;

	public float ySpeed;

	public int yMinLimit;

	public int yMaxLimit;

	private float x;

	private float y;

	public ChainMouseOrbit()
	{
		distance = 10f;
		xSpeed = 250f;
		ySpeed = 120f;
		yMinLimit = -20;
		yMaxLimit = 80;
	}

	public virtual void Start()
	{
		Vector3 eulerAngles = transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	public virtual void LateUpdate()
	{
		if ((bool)target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, 0f);
			Vector3 b = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position + targetOffset;
			transform.rotation = Quaternion.Slerp(transform.rotation, quaternion, Time.deltaTime * 2f);
			transform.position = Vector3.Slerp(transform.position, b, Time.deltaTime * 2f);
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (!(angle >= -360f))
		{
			angle += 360f;
		}
		if (!(angle <= 360f))
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public virtual void OnGUI()
	{
		if (GUI.Button(new Rect(50f, 30f, 100f, 30f), "Reset"))
		{
			Application.LoadLevel(0);
		}
	}

	public virtual void Main()
	{
	}
}
