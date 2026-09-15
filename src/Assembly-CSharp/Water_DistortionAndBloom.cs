using UnityEngine;

[ExecuteInEditMode]
public class Water_DistortionAndBloom : MonoBehaviour
{
	[Range(0.05f, 1f)]
	[Tooltip("Camera render texture resolution")]
	public float RenderTextureResolutoinFactor = 0.5f;

	public LayerMask CullingMask = -17;

	private RenderTexture source;

	private RenderTexture depth;

	private RenderTexture destination;

	private int previuosFrameWidth;

	private int previuosFrameHeight;

	private float previousScale;

	private Camera addCamera;

	private GameObject tempGO;

	private const int kMaxIterations = 16;

	public static Material CheckShaderAndCreateMaterial(Shader s)
	{
		if (s == null || !s.isSupported)
		{
			return null;
		}
		Material material = new Material(s);
		material.hideFlags = HideFlags.DontSave;
		return material;
	}

	private void OnDisable()
	{
		if (tempGO != null)
		{
			Object.DestroyImmediate(tempGO);
		}
		Shader.DisableKeyword("DISTORT_OFF");
		Shader.DisableKeyword("_MOBILEDEPTH_ON");
	}

	private void Start()
	{
		InitializeRenderTarget();
	}

	private void LateUpdate()
	{
		if (previuosFrameWidth != Screen.width || previuosFrameHeight != Screen.height || Mathf.Abs(previousScale - RenderTextureResolutoinFactor) > 0.01f)
		{
			InitializeRenderTarget();
			previuosFrameWidth = Screen.width;
			previuosFrameHeight = Screen.height;
			previousScale = RenderTextureResolutoinFactor;
		}
		Shader.EnableKeyword("DISTORT_OFF");
		Shader.EnableKeyword("_MOBILEDEPTH_ON");
		GrabImage();
		Shader.SetGlobalTexture("_GrabTexture", source);
		Shader.SetGlobalTexture("_CameraDepthTexture", depth);
		Shader.SetGlobalFloat("_GrabTextureScale", RenderTextureResolutoinFactor);
		Shader.DisableKeyword("DISTORT_OFF");
	}

	private void InitializeRenderTarget()
	{
		int width = (int)((float)Screen.width * RenderTextureResolutoinFactor);
		int height = (int)((float)Screen.height * RenderTextureResolutoinFactor);
		source = new RenderTexture(width, height, 0, RenderTextureFormat.RGB565);
		depth = new RenderTexture(width, height, 8, RenderTextureFormat.Depth);
	}

	private void GrabImage()
	{
		Camera camera = Camera.current;
		if (camera == null)
		{
			camera = Camera.main;
		}
		if (tempGO == null)
		{
			tempGO = new GameObject();
			tempGO.hideFlags = HideFlags.HideAndDontSave;
			addCamera = tempGO.AddComponent<Camera>();
			addCamera.enabled = false;
		}
		else
		{
			addCamera = tempGO.GetComponent<Camera>();
		}
		addCamera.CopyFrom(camera);
		addCamera.SetTargetBuffers(source.colorBuffer, depth.depthBuffer);
		addCamera.depth -= 1f;
		addCamera.cullingMask = CullingMask;
		addCamera.Render();
	}
}
