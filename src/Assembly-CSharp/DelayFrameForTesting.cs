using UnityEngine;

public class DelayFrameForTesting : MonoBehaviour
{
	public int delayCount;

	private void Update()
	{
		float num = 0f;
		for (int i = 0; i < delayCount; i++)
		{
			num += 0.001f;
		}
	}
}
