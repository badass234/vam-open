using Obi;
using UnityEngine;

public class CursorController : MonoBehaviour
{
	private ObiRopeCursor cursor;

	public float minLength = 0.1f;

	private void Start()
	{
		cursor = GetComponentInChildren<ObiRopeCursor>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.W) && cursor.rope.RestLength > minLength)
		{
			cursor.ChangeLength(cursor.rope.RestLength - 1f * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			cursor.ChangeLength(cursor.rope.RestLength + 1f * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A))
		{
			cursor.rope.transform.Translate(Vector3.left * Time.deltaTime, Space.World);
		}
		if (Input.GetKey(KeyCode.D))
		{
			cursor.rope.transform.Translate(Vector3.right * Time.deltaTime, Space.World);
		}
	}
}
