using UnityEngine;

public class AQUAS_Caustics : MonoBehaviour
{
	public float fps;

	public Texture2D[] frames;

	public float maxCausticDepth;

	private int frameIndex;

	private Projector projector;

	private void Start()
	{
		projector = GetComponent<Projector>();
		NextFrame();
		InvokeRepeating("NextFrame", 1f / fps, 1f / fps);
		projector.material.SetFloat("_WaterLevel", base.transform.parent.transform.position.y);
		projector.material.SetFloat("_DepthFade", base.transform.parent.transform.position.y - maxCausticDepth);
	}

	private void Update()
	{
		projector.material.SetFloat("_DepthFade", base.transform.parent.transform.position.y - maxCausticDepth);
	}

	private void NextFrame()
	{
		projector.material.SetTexture("_Texture", frames[frameIndex]);
		frameIndex = (frameIndex + 1) % frames.Length;
	}
}
