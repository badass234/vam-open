using UnityEngine;

public class AQUAS_BubbleMorph : MonoBehaviour
{
	private float t;

	private float t2;

	[Space(5f)]
	[Header("Duration of a full morphing cycle")]
	public float tTarget;

	private SkinnedMeshRenderer skinnedMeshRenderer;

	private void Start()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
	}

	private void Update()
	{
		t += Time.deltaTime;
		t2 += Time.deltaTime;
		if (t < tTarget / 2f)
		{
			skinnedMeshRenderer.SetBlendShapeWeight(0, Mathf.Lerp(0f, 50f, t / (tTarget / 2f)));
			skinnedMeshRenderer.SetBlendShapeWeight(1, Mathf.Lerp(50f, 0f, t / (tTarget / 2f)));
		}
		else if (t >= tTarget / 2f && t < tTarget)
		{
			skinnedMeshRenderer.SetBlendShapeWeight(0, Mathf.Lerp(50f, 100f, t / tTarget));
			skinnedMeshRenderer.SetBlendShapeWeight(1, Mathf.Lerp(0f, 50f, t / tTarget));
		}
		else if (t >= tTarget && t < tTarget * 1.5f)
		{
			skinnedMeshRenderer.SetBlendShapeWeight(0, Mathf.Lerp(100f, 50f, t / (tTarget * 1.5f)));
			skinnedMeshRenderer.SetBlendShapeWeight(1, Mathf.Lerp(50f, 100f, t / (tTarget * 1.5f)));
		}
		else if (t >= tTarget * 1.5f && t < tTarget * 2f)
		{
			skinnedMeshRenderer.SetBlendShapeWeight(0, Mathf.Lerp(50f, 0f, t / (tTarget * 2f)));
			skinnedMeshRenderer.SetBlendShapeWeight(1, Mathf.Lerp(100f, 50f, t / (tTarget * 2f)));
		}
		else
		{
			t = 0f;
		}
	}
}
