using Obi;
using UnityEngine;

public class CraneController : MonoBehaviour
{
	private ObiRopeCursor cursor;

	private void Start()
	{
		cursor = GetComponentInChildren<ObiRopeCursor>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.W) && cursor.rope.RestLength > 6.5f)
		{
			cursor.ChangeLength(cursor.rope.RestLength - 1f * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			cursor.ChangeLength(cursor.rope.RestLength + 1f * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Rotate(0f, Time.deltaTime * 15f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Rotate(0f, (0f - Time.deltaTime) * 15f, 0f);
		}
	}
}
