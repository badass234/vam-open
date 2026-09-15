using System.Collections;
using Obi;
using UnityEngine;

public class ObiActorTeleport : MonoBehaviour
{
	public ObiActor actor;

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			StartCoroutine(Teleport());
		}
	}

	private IEnumerator Teleport()
	{
		actor.enabled = false;
		actor.transform.position = Random.insideUnitSphere * 2f;
		yield return new WaitForFixedUpdate();
		actor.enabled = true;
	}
}
