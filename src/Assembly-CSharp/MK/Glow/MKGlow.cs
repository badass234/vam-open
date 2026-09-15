using UnityEngine;

namespace MK.Glow
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[RequireComponent(typeof(Camera))]
	public class MKGlow : MonoBehaviour
	{
		private static float[] gaussFilter = new float[11]
		{
			0.5f, 0.5398f, 0.5793f, 0.6179f, 0.6554f, 0.6915f, 0.7257f, 0.758f, 0.7881f, 0.8159f,
			0.8413f
		};

		private const float GLOW_INTENSITY_INNER_MULT = 25f;

		private const float GLOW_INTENSITY_OUTER_MULT = 50f;

		private const float BLUR_SPREAD_INNTER_MULT = 10f;

		private const float BLUR_SPREAD_OUTER_MULT = 50f;

		private const string LENS_KEYWORD = "_MK_LENS";

		private RenderTextureFormat rtFormat = RenderTextureFormat.Default;

		[SerializeField]
		private Shader blurShader;

		[SerializeField]
		private Shader compositeShader;

		[SerializeField]
		private Shader selectiveRenderShader;

		[SerializeField]
		private Shader extractLuminanceShader;

		private Material compositeMaterial;

		private Material blurMaterial;

		private Material extractLuminanceMaterial;

		[SerializeField]
		[Tooltip("Lens Texture")]
		private Texture lensTex;

		[SerializeField]
		[Tooltip("Lens Intensity")]
		private float lensIntensity;

		[SerializeField]
		[Tooltip("recommend: -1")]
		private LayerMask glowLayer = -1;

		[SerializeField]
		[Tooltip("Selective = to specifically bring objects to glow, Fullscreen = complete screen glows")]
		private GlowType glowType = GlowType.Luminance;

		[SerializeField]
		[Tooltip("The glows coloration")]
		private Color glowTint = new Color(1f, 1f, 1f, 0f);

		[SerializeField]
		[Tooltip("Inner width of the glow effect")]
		private float blurSpreadInner = 0.6f;

		[SerializeField]
		[Tooltip("Outer width of the glow effect")]
		private float blurSpreadOuter = 0.7f;

		[SerializeField]
		[Tooltip("Number of used blurs. Lower iterations = better performance")]
		private int blurIterations = 5;

		[SerializeField]
		[Tooltip("The global inner luminous intensity")]
		private float glowIntensityInner = 0.4f;

		[SerializeField]
		[Tooltip("The global outer luminous intensity")]
		private float glowIntensityOuter;

		[SerializeField]
		[Tooltip("Downsampling steps of the blur. Higher samples = better performance, but gains more flickering")]
		private float samples = 2f;

		[SerializeField]
		[Tooltip("Threshold for glow")]
		private float threshold = 1f;

		[SerializeField]
		[Tooltip("Enable and disable the lens")]
		private bool useLens;

		[SerializeField]
		private int antiAliasing = 1;

		private Camera selectiveGlowCamera;

		private GameObject selectiveGlowCameraObject;

		private RenderTexture glowTexRaw;

		private int srcWidth;

		private int srcHeight;

		private VRTextureUsage srcVRUsage = VRTextureUsage.TwoEyes;

		[SerializeField]
		private Camera Cam => GetComponent<Camera>();

		public bool UseLens
		{
			get
			{
				return useLens;
			}
			set
			{
				useLens = value;
			}
		}

		public Texture LensTex
		{
			get
			{
				return lensTex;
			}
			set
			{
				lensTex = value;
			}
		}

		public float LensIntensity
		{
			get
			{
				return lensIntensity;
			}
			set
			{
				lensIntensity = value;
			}
		}

		public LayerMask GlowLayer
		{
			get
			{
				return glowLayer;
			}
			set
			{
				glowLayer = value;
			}
		}

		public GlowType GlowType
		{
			get
			{
				return glowType;
			}
			set
			{
				glowType = value;
			}
		}

		public Color GlowTint
		{
			get
			{
				return glowTint;
			}
			set
			{
				glowTint = value;
			}
		}

		public float Samples
		{
			get
			{
				return samples;
			}
			set
			{
				samples = value;
			}
		}

		public int BlurIterations
		{
			get
			{
				return blurIterations;
			}
			set
			{
				blurIterations = Mathf.Clamp(value, 0, 10);
			}
		}

		public float GlowIntensityInner
		{
			get
			{
				return glowIntensityInner;
			}
			set
			{
				glowIntensityInner = value;
			}
		}

		public float GlowIntensityOuter
		{
			get
			{
				return glowIntensityOuter;
			}
			set
			{
				glowIntensityOuter = value;
			}
		}

		public float BlurSpreadInner
		{
			get
			{
				return blurSpreadInner;
			}
			set
			{
				blurSpreadInner = value;
			}
		}

		public float BlurSpreadOuter
		{
			get
			{
				return blurSpreadOuter;
			}
			set
			{
				blurSpreadOuter = value;
			}
		}

		public float Threshold
		{
			get
			{
				return threshold;
			}
			set
			{
				threshold = value;
			}
		}

		public int AntiAliasing
		{
			get
			{
				return antiAliasing;
			}
			set
			{
				antiAliasing = value;
			}
		}

		private GameObject SelectiveGlowCameraObject
		{
			get
			{
				if (!selectiveGlowCameraObject)
				{
					selectiveGlowCameraObject = new GameObject("selectiveGlowCameraObject");
					selectiveGlowCameraObject.AddComponent<Camera>();
					selectiveGlowCameraObject.hideFlags = HideFlags.HideAndDontSave;
					SelectiveGlowCamera.orthographic = false;
					SelectiveGlowCamera.enabled = false;
					SelectiveGlowCamera.renderingPath = RenderingPath.VertexLit;
					SelectiveGlowCamera.hideFlags = HideFlags.HideAndDontSave;
				}
				return selectiveGlowCameraObject;
			}
		}

		private Camera SelectiveGlowCamera
		{
			get
			{
				if (selectiveGlowCamera == null)
				{
					selectiveGlowCamera = SelectiveGlowCameraObject.GetComponent<Camera>();
				}
				return selectiveGlowCamera;
			}
		}

		private void Awake()
		{
			GlowInitialize();
		}

		private void Reset()
		{
			GlowInitialize();
		}

		private void GlowInitialize()
		{
			Cleanup();
			SetupShaders();
			SetupMaterials();
		}

		private void SetupShaders()
		{
			if (!blurShader)
			{
				blurShader = Shader.Find("Hidden/MK/Glow/Blur");
			}
			if (!compositeShader)
			{
				compositeShader = Shader.Find("Hidden/MK/Glow/Composite");
			}
			if (!selectiveRenderShader)
			{
				selectiveRenderShader = Shader.Find("Hidden/MK/Glow/SelectiveRender");
			}
			if (!extractLuminanceShader)
			{
				extractLuminanceShader = Shader.Find("Hidden/MK/Glow/ExtractLuminance");
			}
		}

		private void Cleanup()
		{
			Object.DestroyImmediate(selectiveGlowCamera);
			Object.DestroyImmediate(SelectiveGlowCameraObject);
		}

		private void OnEnable()
		{
			GlowInitialize();
		}

		private void OnDisable()
		{
			Cleanup();
		}

		private void OnDestroy()
		{
			Cleanup();
		}

		private RenderTexture GetTemporaryRT(int width, int height, VRTextureUsage vrUsage)
		{
			return RenderTexture.GetTemporary(width, height, 0, rtFormat, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, vrUsage);
		}

		private void Blur(RenderTexture main, RenderTexture tmpMain, RenderTexture sec, RenderTexture tmpSec)
		{
			for (int i = 1; i <= blurIterations; i++)
			{
				float num = (float)i * (blurSpreadInner * 10f) / (float)blurIterations / samples;
				num *= gaussFilter[i];
				blurMaterial.SetFloat("_Offset", num);
				Graphics.Blit(main, tmpMain, blurMaterial);
				blurMaterial.SetFloat("_Offset", num);
				Graphics.Blit(tmpMain, main, blurMaterial);
				if (blurSpreadOuter > 0f || glowIntensityOuter > 0f)
				{
					float num2 = (float)i * (blurSpreadOuter * 50f) / (float)blurIterations / samples;
					num2 *= gaussFilter[i];
					blurMaterial.SetFloat("_Offset", num2);
					Graphics.Blit(sec, tmpSec, blurMaterial);
					blurMaterial.SetFloat("_Offset", num2);
					Graphics.Blit(tmpSec, sec, blurMaterial);
				}
			}
		}

		private void Blur(RenderTexture main, RenderTexture tmpMain)
		{
			for (int i = 1; i <= blurIterations; i++)
			{
				float num = (float)i * (blurSpreadInner * 10f) / (float)blurIterations / samples;
				num *= gaussFilter[i];
				blurMaterial.SetFloat("_Offset", num);
				Graphics.Blit(main, tmpMain, blurMaterial);
				blurMaterial.SetFloat("_Offset", num);
				Graphics.Blit(tmpMain, main, blurMaterial);
			}
		}

		private void SetupMaterials()
		{
			if (blurMaterial == null)
			{
				blurMaterial = new Material(blurShader);
				blurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			if (extractLuminanceMaterial == null)
			{
				extractLuminanceMaterial = new Material(extractLuminanceShader);
				extractLuminanceMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			if (compositeMaterial == null)
			{
				compositeMaterial = new Material(compositeShader);
				compositeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void SetupGlowCamera()
		{
			SelectiveGlowCamera.CopyFrom(Cam);
			SelectiveGlowCamera.depthTextureMode = DepthTextureMode.None;
			SelectiveGlowCamera.targetTexture = glowTexRaw;
			SelectiveGlowCamera.clearFlags = CameraClearFlags.Color;
			SelectiveGlowCamera.rect = new Rect(0f, 0f, 1f, 1f);
			SelectiveGlowCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			SelectiveGlowCamera.cullingMask = glowLayer;
			SelectiveGlowCamera.renderingPath = RenderingPath.VertexLit;
		}

		private void LuminanceGlow(RenderTexture src, RenderTexture dest, RenderTexture glowTexInner, RenderTexture tmpGlowTexInner, RenderTexture glowTexOuter, RenderTexture tmpGlowTexOuter)
		{
			Graphics.Blit(src, glowTexInner, extractLuminanceMaterial);
			Graphics.Blit(glowTexInner, glowTexOuter);
			Blur(glowTexInner, tmpGlowTexInner, glowTexOuter, tmpGlowTexOuter);
			compositeMaterial.SetTexture("_MKGlowTexInner", glowTexInner);
			compositeMaterial.SetTexture("_MKGlowTexOuter", glowTexOuter);
			Graphics.Blit(src, dest, compositeMaterial, 0);
		}

		private void FullScreenGlow(RenderTexture src, RenderTexture dest, RenderTexture glowTexInner, RenderTexture tmpGlowTexInner)
		{
			Graphics.Blit(src, glowTexInner);
			Blur(glowTexInner, tmpGlowTexInner);
			compositeMaterial.SetTexture("_MKGlowTexInner", glowTexInner);
			Graphics.Blit(src, dest, compositeMaterial, 1);
		}

		private void SelectiveGlow(RenderTexture src, RenderTexture dest, RenderTexture glowTexInner, RenderTexture tmpGlowTexInner, RenderTexture glowTexOuter, RenderTexture tmpGlowTexOuter)
		{
			Graphics.Blit(glowTexRaw, glowTexInner);
			Graphics.Blit(glowTexRaw, glowTexOuter);
			Blur(glowTexInner, tmpGlowTexInner, glowTexOuter, tmpGlowTexOuter);
			compositeMaterial.SetTexture("_MKGlowTexInner", glowTexInner);
			compositeMaterial.SetTexture("_MKGlowTexOuter", glowTexOuter);
			Graphics.Blit(src, dest, compositeMaterial, 0);
		}

		private void SelectiveGlowFast(RenderTexture src, RenderTexture dest, RenderTexture glowTexInner, RenderTexture tmpGlowTexInner)
		{
			Graphics.Blit(glowTexRaw, glowTexInner);
			Blur(glowTexInner, tmpGlowTexInner, null, null);
			compositeMaterial.SetTexture("_MKGlowTexInner", glowTexInner);
			Graphics.Blit(src, dest, compositeMaterial, 1);
		}

		private void OnPostRender()
		{
			switch (glowType)
			{
			case GlowType.Luminance:
				extractLuminanceMaterial.SetFloat("_Threshold", threshold);
				break;
			case GlowType.Selective:
			case GlowType.SelectiveFast:
				RenderTexture.ReleaseTemporary(glowTexRaw);
				glowTexRaw = RenderTexture.GetTemporary((int)((float)Cam.pixelWidth / samples), (int)((float)Cam.pixelHeight / samples), 24, rtFormat, RenderTextureReadWrite.Default, antiAliasing, RenderTextureMemoryless.None, srcVRUsage);
				SetupGlowCamera();
				SelectiveGlowCamera.RenderWithShader(selectiveRenderShader, "RenderType");
				break;
			}
			blurMaterial.SetFloat("_VRMult", (!Cam.stereoEnabled) ? 1f : 0.5f);
			compositeMaterial.SetFloat("_GlowIntensityInner", glowIntensityInner * ((glowType == GlowType.Fullscreen) ? 10f : (25f * blurSpreadInner)));
			compositeMaterial.SetFloat("_GlowIntensityOuter", glowIntensityOuter * BlurSpreadOuter * 50f);
			compositeMaterial.SetColor("_GlowTint", glowTint);
			if (glowType == GlowType.SelectiveFast)
			{
				SetKeyword(false, "_MK_Outer", compositeMaterial);
			}
			else
			{
				SetKeyword(true, "_MK_Outer", compositeMaterial);
			}
			UpdateLensUsage();
			if (useLens)
			{
				compositeMaterial.SetTexture("_LensTex", lensTex);
				compositeMaterial.SetFloat("_LensIntensity", lensIntensity);
			}
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			rtFormat = src.format;
			float num = src.width;
			float num2 = src.height;
			srcWidth = (int)(num / samples);
			srcHeight = (int)(num2 / samples);
			srcVRUsage = src.vrUsage;
			RenderTexture temporaryRT = GetTemporaryRT(srcWidth, srcHeight, src.vrUsage);
			RenderTexture temporaryRT2 = GetTemporaryRT(srcWidth, srcHeight, src.vrUsage);
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			if (glowType == GlowType.Luminance || glowType == GlowType.Selective)
			{
				renderTexture = GetTemporaryRT(srcWidth / 2, srcHeight / 2, src.vrUsage);
				renderTexture2 = GetTemporaryRT(srcWidth / 2, srcHeight / 2, src.vrUsage);
			}
			switch (glowType)
			{
			case GlowType.Luminance:
				LuminanceGlow(src, dest, temporaryRT, temporaryRT2, renderTexture, renderTexture2);
				break;
			case GlowType.Selective:
				SelectiveGlow(src, dest, temporaryRT, temporaryRT2, renderTexture, renderTexture2);
				break;
			case GlowType.Fullscreen:
				FullScreenGlow(src, dest, temporaryRT, temporaryRT2);
				break;
			case GlowType.SelectiveFast:
				SelectiveGlowFast(src, dest, temporaryRT, temporaryRT2);
				break;
			}
			if (glowType == GlowType.Luminance || glowType == GlowType.Selective)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
				RenderTexture.ReleaseTemporary(renderTexture2);
			}
			RenderTexture.ReleaseTemporary(temporaryRT);
			RenderTexture.ReleaseTemporary(temporaryRT2);
		}

		private void UpdateLensUsage()
		{
			if (useLens && glowType != GlowType.Fullscreen)
			{
				EnableLens(true);
			}
			else
			{
				EnableLens(false);
			}
		}

		private void EnableLens(bool use)
		{
			if (use)
			{
				SetKeyword(true, "_MK_LENS", compositeMaterial);
			}
			else
			{
				SetKeyword(false, "_MK_LENS", compositeMaterial);
			}
		}

		private static void SetKeyword(bool enable, string keyword, Material mat)
		{
			if (enable)
			{
				if (!mat.IsKeywordEnabled(keyword))
				{
					mat.EnableKeyword(keyword);
				}
			}
			else if (mat.IsKeywordEnabled(keyword))
			{
				mat.DisableKeyword(keyword);
			}
		}
	}
}
