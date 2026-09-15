using UnityEngine;

[ExecuteInEditMode]
public class DepthFix : MonoBehaviour
{
	private void OnWillRenderObject()
	{
		Camera.current.depthTextureMode |= DepthTextureMode.Depth;
	}
}
