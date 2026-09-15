using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
	private Vector3 screenPoint;

	private Vector3 offset;

	private bool dragged;

	private Vector3 newPosition;

	private Rigidbody body;

	private void Awake()
	{
		body = base.gameObject.GetComponent<Rigidbody>();
		newPosition = base.transform.position;
	}

	private void OnMouseDown()
	{
		screenPoint = Camera.main.WorldToScreenPoint(base.gameObject.transform.position);
		offset = base.gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}

	private void OnMouseDrag()
	{
		dragged = true;
	}

	private void FixedUpdate()
	{
		if (dragged)
		{
			dragged = false;
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			newPosition = Camera.main.ScreenToWorldPoint(position) + offset;
			if (body != null)
			{
				body.velocity = (newPosition - base.transform.position) / Time.deltaTime;
			}
		}
	}

	private void LateUpdate()
	{
		base.transform.position = newPosition;
	}
}
