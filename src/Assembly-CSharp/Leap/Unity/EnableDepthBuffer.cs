using UnityEngine;

namespace Leap.Unity
{
	[ExecuteInEditMode]
	public class EnableDepthBuffer : MonoBehaviour
	{
		public const string DEPTH_TEXTURE_VARIANT_NAME = "USE_DEPTH_TEXTURE";

		[SerializeField]
		private DepthTextureMode _depthTextureMode = DepthTextureMode.Depth;

		private void Awake()
		{
			GetComponent<Camera>().depthTextureMode = _depthTextureMode;
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) && _depthTextureMode != 0)
			{
				Shader.EnableKeyword("USE_DEPTH_TEXTURE");
			}
			else
			{
				Shader.DisableKeyword("USE_DEPTH_TEXTURE");
			}
		}
	}
}
