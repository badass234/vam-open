using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("AQUAS/Render Queue Controller")]
public class AQUAS_RenderQueueEditor : MonoBehaviour
{
	public int renderQueueIndex = -1;

	private void Update()
	{
		base.gameObject.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueueIndex;
	}
}
