using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Wilberforce.VAO
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[HelpURL("https://projectwilberforce.github.io/vaomanual/")]
	public class VAOEffectCommandBuffer : MonoBehaviour
	{
		public enum EffectMode
		{
			Simple = 1,
			ColorTint,
			ColorBleed
		}

		public enum LuminanceModeType
		{
			Luma = 1,
			HSVValue
		}

		public enum GiBlurAmmount
		{
			Auto = 1,
			Less,
			More
		}

		public enum CullingPrepassModeType
		{
			Off,
			Greedy,
			Careful
		}

		public enum AdaptiveSamplingType
		{
			Disabled,
			EnabledAutomatic,
			EnabledManual
		}

		public enum BlurModeType
		{
			Disabled,
			Basic,
			Enhanced
		}

		public enum BlurQualityType
		{
			Fast,
			Precise
		}

		public enum ColorBleedSelfOcclusionFixLevelType
		{
			Off,
			Soft,
			Hard
		}

		public enum ScreenTextureFormat
		{
			Auto,
			ARGB32,
			ARGBHalf,
			ARGBFloat,
			Default,
			DefaultHDR
		}

		public enum FarPlaneSourceType
		{
			ProjectionParams,
			Camera
		}

		public enum DistanceFalloffModeType
		{
			Off,
			Absolute,
			Relative
		}

		public enum VAOCameraEventType
		{
			AfterLighting,
			BeforeReflections,
			BeforeImageEffectsOpaque
		}

		public enum HierarchicalBufferStateType
		{
			Off,
			On,
			Auto
		}

		private enum ShaderPass
		{
			CullingPrepass,
			MainPass,
			StandardBlurUniform,
			StandardBlurUniformMultiplyBlend,
			StandardBlurUniformFast,
			StandardBlurUniformFastMultiplyBlend,
			EnhancedBlurFirstPass,
			EnhancedBlurSecondPass,
			EnhancedBlurSecondPassMultiplyBlend,
			Mixing,
			MixingMultiplyBlend,
			BlendBeforeReflections,
			BlendBeforeReflectionsLog,
			DownscaleDepthNormalsPass,
			Copy,
			BlendAfterLightingLog,
			TexCopyImageEffectSPSR
		}

		public float Radius = 0.5f;

		public float Power = 1f;

		public float Presence = 0.1f;

		public int Quality = 16;

		public bool MaxRadiusEnabled = true;

		public float MaxRadius = 0.4f;

		public DistanceFalloffModeType DistanceFalloffMode;

		public float DistanceFalloffStartAbsolute = 100f;

		public float DistanceFalloffStartRelative = 0.1f;

		public float DistanceFalloffSpeedAbsolute = 30f;

		public float DistanceFalloffSpeedRelative = 0.1f;

		public AdaptiveSamplingType AdaptiveType = AdaptiveSamplingType.EnabledAutomatic;

		public float AdaptiveQualityCoefficient = 1f;

		public CullingPrepassModeType CullingPrepassMode = CullingPrepassModeType.Careful;

		public int Downsampling = 1;

		public HierarchicalBufferStateType HierarchicalBufferState = HierarchicalBufferStateType.Auto;

		public bool CommandBufferEnabled = true;

		public bool UseGBuffer = true;

		public bool UsePreciseDepthBuffer = true;

		public VAOCameraEventType VaoCameraEvent;

		public FarPlaneSourceType FarPlaneSource = FarPlaneSourceType.Camera;

		public bool IsLumaSensitive;

		public LuminanceModeType LuminanceMode = LuminanceModeType.Luma;

		public float LumaThreshold = 0.7f;

		public float LumaKneeWidth = 0.3f;

		public float LumaKneeLinearity = 3f;

		public EffectMode Mode = EffectMode.ColorTint;

		public Color ColorTint = Color.black;

		public float ColorBleedPower = 5f;

		public float ColorBleedPresence = 1f;

		public ScreenTextureFormat IntermediateScreenTextureFormat;

		public bool ColorbleedHueSuppresionEnabled;

		public float ColorBleedHueSuppresionThreshold = 7f;

		public float ColorBleedHueSuppresionWidth = 2f;

		public float ColorBleedHueSuppresionSaturationThreshold = 0.5f;

		public float ColorBleedHueSuppresionSaturationWidth = 0.2f;

		public float ColorBleedHueSuppresionBrightness;

		public int ColorBleedQuality = 2;

		public ColorBleedSelfOcclusionFixLevelType ColorBleedSelfOcclusionFixLevel = ColorBleedSelfOcclusionFixLevelType.Hard;

		public bool GiBackfaces;

		public BlurQualityType BlurQuality = BlurQualityType.Precise;

		public BlurModeType BlurMode = BlurModeType.Enhanced;

		public int EnhancedBlurSize = 5;

		public float EnhancedBlurDeviation = 1.8f;

		public bool OutputAOOnly;

		public float HierarchicalBufferPixelsPerLevel = 150f;

		private int CullingPrepassDownsamplingFactor = 8;

		private float AdaptiveQuality = 0.2f;

		private float AdaptiveMin;

		private float AdaptiveMax = -10f;

		private Dictionary<CameraEvent, CommandBuffer> cameraEventsRegistered = new Dictionary<CameraEvent, CommandBuffer>();

		private bool isCommandBufferAlive;

		private Mesh screenQuad;

		private int destinationWidth;

		private int destinationHeight;

		private bool onDestroyCalled;

		public Shader vaoShader;

		private Camera myCamera;

		private bool isSupported;

		private Material VAOMaterial;

		public bool ForcedSwitchPerformed;

		public bool ForcedSwitchPerformedSinglePassStereo;

		public bool ForcedSwitchPerformedSinglePassStereoGBuffer;

		private int lastDownsampling;

		private CullingPrepassModeType lastcullingPrepassType;

		private int lastCullingPrepassDownsamplingFactor;

		private BlurModeType lastBlurMode;

		private BlurQualityType lastBlurQuality;

		private EffectMode lastMode;

		private bool lastUseGBuffer;

		private bool lastOutputAOOnly;

		private CameraEvent lastCameraEvent;

		private bool lastIsHDR;

		private bool lastIsSPSR;

		private bool isHDR;

		public bool isSPSR;

		private ScreenTextureFormat lastIntermediateScreenTextureFormat;

		private int lastCmdBufferEnhancedBlurSize;

		private bool lastHierarchicalBufferEnabled;

		private Texture2D noiseTexture;

		private Vector4[] adaptiveSamples;

		private Vector4[] carefulCache;

		private Vector4[] gaussian;

		private Vector4[] gaussianBuffer = new Vector4[17];

		private Vector4[] samplesLarge = new Vector4[70];

		private int lastSamplesLength;

		private int lastEnhancedBlurSize;

		private float gaussianWeight;

		private float lastDeviation = 0.5f;

		private static float[] adaptiveLengths = new float[16]
		{
			32f, 16f, 16f, 8f, 8f, 8f, 8f, 4f, 4f, 4f,
			4f, 4f, 4f, 4f, 4f, 4f
		};

		private static float[] adaptiveStarts = new float[16]
		{
			0f, 32f, 32f, 48f, 48f, 48f, 48f, 56f, 56f, 56f,
			56f, 56f, 56f, 56f, 56f, 56f
		};

		private static Color[] noiseSamples = new Color[9]
		{
			new Color(1f, 0f, 0f),
			new Color(-0.939692f, 0.342022f, 0f),
			new Color(0.173644f, -0.984808f, 0f),
			new Color(0.173649f, 0.984808f, 0f),
			new Color(-0.500003f, -0.866024f, 0f),
			new Color(0.766045f, 0.642787f, 0f),
			new Color(-0.939694f, -0.342017f, 0f),
			new Color(0.766042f, -0.642791f, 0f),
			new Color(-0.499999f, 0.866026f, 0f)
		};

		private static Vector4[] samp2 = new Vector4[2]
		{
			new Vector4(0.4392292f, 0.0127914f, 0.898284f),
			new Vector4(-0.894406f, -0.162116f, 0.41684f)
		};

		private static Vector4[] samp4 = new Vector4[4]
		{
			new Vector4(-0.07984404f, -0.2016976f, 0.976188f),
			new Vector4(0.4685118f, -0.8404996f, 0.272135f),
			new Vector4(-0.793633f, 0.293059f, 0.533164f),
			new Vector4(0.2998218f, 0.4641494f, 0.83347f)
		};

		private static Vector4[] samp8 = new Vector4[8]
		{
			new Vector4(-0.4999112f, -0.571184f, 0.651028f),
			new Vector4(0.2267525f, -0.668142f, 0.708639f),
			new Vector4(0.0657284f, -0.123769f, 0.990132f),
			new Vector4(0.9259827f, -0.2030669f, 0.318307f),
			new Vector4(-0.9850165f, 0.1247843f, 0.119042f),
			new Vector4(-0.2988613f, 0.2567392f, 0.919112f),
			new Vector4(0.4734727f, 0.2830991f, 0.834073f),
			new Vector4(0.1319883f, 0.9544416f, 0.267621f)
		};

		private static Vector4[] samp16 = new Vector4[16]
		{
			new Vector4(-0.6870962f, -0.7179669f, 0.111458f),
			new Vector4(-0.2574025f, -0.6144419f, 0.745791f),
			new Vector4(-0.408366f, -0.162244f, 0.898284f),
			new Vector4(-0.07098053f, 0.02052395f, 0.997267f),
			new Vector4(0.2019972f, -0.760972f, 0.616538f),
			new Vector4(0.706282f, -0.6368136f, 0.309248f),
			new Vector4(0.169605f, -0.2892981f, 0.942094f),
			new Vector4(0.7644456f, -0.05826119f, 0.64205f),
			new Vector4(-0.745912f, 0.0501786f, 0.664152f),
			new Vector4(-0.7588732f, 0.4313389f, 0.487911f),
			new Vector4(-0.3806622f, 0.3446409f, 0.85809f),
			new Vector4(-0.1296651f, 0.8794711f, 0.45795f),
			new Vector4(0.1557318f, 0.137468f, 0.978187f),
			new Vector4(0.5990864f, 0.2485375f, 0.761133f),
			new Vector4(0.1727637f, 0.5753375f, 0.799462f),
			new Vector4(0.5883294f, 0.7348878f, 0.337355f)
		};

		private static Vector4[] samp32 = new Vector4[32]
		{
			new Vector4(-0.626056f, -0.7776781f, 0.0571977f),
			new Vector4(-0.1335098f, -0.9164876f, 0.377127f),
			new Vector4(-0.2668636f, -0.5663173f, 0.779787f),
			new Vector4(-0.5712572f, -0.4639561f, 0.67706f),
			new Vector4(-0.6571807f, -0.2969118f, 0.692789f),
			new Vector4(-0.8896923f, -0.1314662f, 0.437223f),
			new Vector4(-0.5037534f, -0.03057539f, 0.863306f),
			new Vector4(-0.1773856f, -0.2664998f, 0.947371f),
			new Vector4(-0.02786797f, -0.02453661f, 0.99931f),
			new Vector4(0.173095f, -0.964425f, 0.199805f),
			new Vector4(0.280491f, -0.716259f, 0.638982f),
			new Vector4(0.7610048f, -0.4987299f, 0.414898f),
			new Vector4(0.135136f, -0.388973f, 0.911284f),
			new Vector4(0.4836829f, -0.4782286f, 0.73304f),
			new Vector4(0.1905736f, -0.1039435f, 0.976154f),
			new Vector4(0.4855643f, 0.01388972f, 0.87409f),
			new Vector4(0.5684234f, -0.2864941f, 0.771243f),
			new Vector4(0.8165832f, 0.01384446f, 0.577062f),
			new Vector4(-0.9814694f, 0.18555f, 0.0478435f),
			new Vector4(-0.5357604f, 0.3316899f, 0.776494f),
			new Vector4(-0.1238877f, 0.03315933f, 0.991742f),
			new Vector4(-0.1610546f, 0.3801286f, 0.910804f),
			new Vector4(-0.5923722f, 0.628729f, 0.503781f),
			new Vector4(-0.05504921f, 0.5483891f, 0.834409f),
			new Vector4(-0.3805041f, 0.8377199f, 0.391717f),
			new Vector4(-0.101651f, 0.9530866f, 0.285119f),
			new Vector4(0.1613653f, 0.2561041f, 0.953085f),
			new Vector4(0.4533991f, 0.2896196f, 0.842941f),
			new Vector4(0.6665574f, 0.4639243f, 0.583503f),
			new Vector4(0.8873722f, 0.4278904f, 0.1717f),
			new Vector4(0.2869751f, 0.732805f, 0.616962f),
			new Vector4(0.4188429f, 0.7185978f, 0.555147f)
		};

		private static Vector4[] samp64 = new Vector4[64]
		{
			new Vector4(-0.6700248f, -0.6370129f, 0.381157f),
			new Vector4(-0.7385408f, -0.6073685f, 0.292679f),
			new Vector4(-0.4108568f, -0.8852778f, 0.2179f),
			new Vector4(-0.3058583f, -0.8047022f, 0.508828f),
			new Vector4(0.01087609f, -0.7610992f, 0.648545f),
			new Vector4(-0.3629634f, -0.5480431f, 0.753595f),
			new Vector4(-0.1480379f, -0.6927805f, 0.70579f),
			new Vector4(-0.9533184f, -0.276674f, 0.12098f),
			new Vector4(-0.6387863f, -0.3999016f, 0.65729f),
			new Vector4(-0.891588f, -0.115146f, 0.437964f),
			new Vector4(-0.775663f, 0.0194654f, 0.630848f),
			new Vector4(-0.5360528f, -0.1828935f, 0.824134f),
			new Vector4(-0.513927f, -0.000130296f, 0.857834f),
			new Vector4(-0.4368436f, -0.2831443f, 0.853813f),
			new Vector4(-0.1794069f, -0.4226944f, 0.888337f),
			new Vector4(-0.00183062f, -0.4371257f, 0.899398f),
			new Vector4(-0.2598701f, -0.1719497f, 0.950211f),
			new Vector4(-0.08650014f, -0.004176182f, 0.996243f),
			new Vector4(0.006921067f, -0.001478712f, 0.999975f),
			new Vector4(0.05654667f, -0.9351676f, 0.349662f),
			new Vector4(0.1168661f, -0.754741f, 0.64553f),
			new Vector4(0.3534952f, -0.7472929f, 0.562667f),
			new Vector4(0.1635596f, -0.5863093f, 0.793404f),
			new Vector4(0.5910167f, -0.786864f, 0.177609f),
			new Vector4(0.5820105f, -0.5659724f, 0.5839f),
			new Vector4(0.7254612f, -0.5323696f, 0.436221f),
			new Vector4(0.4016336f, -0.4329237f, 0.807012f),
			new Vector4(0.5287027f, -0.4064075f, 0.745188f),
			new Vector4(0.314015f, -0.2375291f, 0.919225f),
			new Vector4(0.02922117f, -0.2097672f, 0.977315f),
			new Vector4(0.4201531f, -0.1445212f, 0.895871f),
			new Vector4(0.2821195f, -0.01079273f, 0.959319f),
			new Vector4(0.7152653f, -0.1972963f, 0.670425f),
			new Vector4(0.8167331f, -0.1217311f, 0.564029f),
			new Vector4(0.8517836f, 0.01290532f, 0.523735f),
			new Vector4(-0.657816f, 0.134013f, 0.74116f),
			new Vector4(-0.851676f, 0.321285f, 0.414033f),
			new Vector4(-0.603183f, 0.361627f, 0.710912f),
			new Vector4(-0.6607267f, 0.5282444f, 0.533289f),
			new Vector4(-0.323619f, 0.182656f, 0.92839f),
			new Vector4(-0.2080927f, 0.1494067f, 0.966631f),
			new Vector4(-0.4205947f, 0.4184987f, 0.804959f),
			new Vector4(-0.06831062f, 0.3712724f, 0.926008f),
			new Vector4(-0.165943f, 0.5029928f, 0.84821f),
			new Vector4(-0.6137413f, 0.7001954f, 0.364758f),
			new Vector4(-0.3009551f, 0.6550035f, 0.693107f),
			new Vector4(-0.1356791f, 0.6460465f, 0.751143f),
			new Vector4(-0.3677429f, 0.7920387f, 0.487278f),
			new Vector4(-0.08688695f, 0.9677781f, 0.236338f),
			new Vector4(0.07250954f, 0.1327261f, 0.988497f),
			new Vector4(0.5244588f, 0.05565827f, 0.849615f),
			new Vector4(0.2498424f, 0.3364912f, 0.907938f),
			new Vector4(0.2608168f, 0.5340923f, 0.804189f),
			new Vector4(0.3888291f, 0.3207975f, 0.863655f),
			new Vector4(0.6413552f, 0.1619097f, 0.749966f),
			new Vector4(0.8523082f, 0.2647078f, 0.451111f),
			new Vector4(0.5591328f, 0.3038472f, 0.771393f),
			new Vector4(0.9147445f, 0.3917669f, 0.0987938f),
			new Vector4(0.08110893f, 0.7317293f, 0.676752f),
			new Vector4(0.3154335f, 0.7388063f, 0.59554f),
			new Vector4(0.1677455f, 0.9625717f, 0.212877f),
			new Vector4(0.3015989f, 0.9509261f, 0.069128f),
			new Vector4(0.5600207f, 0.5649592f, 0.605969f),
			new Vector4(0.6455291f, 0.7387806f, 0.193637f)
		};

		public bool HierarchicalBufferEnabled
		{
			get
			{
				if (HierarchicalBufferState == HierarchicalBufferStateType.On)
				{
					return true;
				}
				if (HierarchicalBufferState == HierarchicalBufferStateType.Auto)
				{
					return ShouldUseHierarchicalBuffer();
				}
				return false;
			}
		}

		private void Start()
		{
			if (vaoShader == null)
			{
				vaoShader = Shader.Find("Hidden/Wilberforce/VAOShader");
			}
			if (vaoShader == null)
			{
				ReportError("Could not locate VAO Shader. Make sure there is 'VAOShader.shader' file added to the project.");
				isSupported = false;
				base.enabled = false;
				return;
			}
			if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) || SystemInfo.graphicsShaderLevel < 30)
			{
				if (!SystemInfo.supportsImageEffects)
				{
					ReportError("System does not support image effects.");
				}
				if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
				{
					ReportError("System does not support depth texture.");
				}
				if (SystemInfo.graphicsShaderLevel < 30)
				{
					ReportError("This effect needs at least Shader Model 3.0.");
				}
				isSupported = false;
				base.enabled = false;
				return;
			}
			EnsureMaterials();
			if (!VAOMaterial || VAOMaterial.passCount != Enum.GetValues(typeof(ShaderPass)).Length)
			{
				ReportError("Could not create shader.");
				isSupported = false;
				base.enabled = false;
				return;
			}
			EnsureNoiseTexture();
			if (adaptiveSamples == null)
			{
				adaptiveSamples = GenerateAdaptiveSamples();
			}
			isSupported = true;
		}

		private void OnEnable()
		{
			myCamera = GetComponent<Camera>();
			TeardownCommandBuffer();
			isSPSR = isCameraSPSR(myCamera);
		}

		private void OnValidate()
		{
			Radius = Mathf.Clamp(Radius, 0.001f, float.MaxValue);
			Power = Mathf.Clamp(Power, 0f, float.MaxValue);
		}

		private void OnPreRender()
		{
			EnsureVAOVersion();
			bool flag = false;
			bool flag2 = false;
			DepthTextureMode depthTextureMode = myCamera.depthTextureMode;
			if (myCamera.actualRenderingPath == RenderingPath.DeferredShading && UseGBuffer)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
			if (UsePreciseDepthBuffer && (myCamera.actualRenderingPath == RenderingPath.Forward || myCamera.actualRenderingPath == RenderingPath.VertexLit))
			{
				flag = true;
				flag2 = true;
			}
			if (flag && (depthTextureMode & DepthTextureMode.Depth) != DepthTextureMode.Depth)
			{
				myCamera.depthTextureMode |= DepthTextureMode.Depth;
			}
			if (flag2 && (depthTextureMode & DepthTextureMode.DepthNormals) != DepthTextureMode.DepthNormals)
			{
				myCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
			EnsureMaterials();
			EnsureNoiseTexture();
			TrySetUniforms();
			EnsureCommandBuffer(CheckSettingsChanges());
		}

		private void OnPostRender()
		{
			if (!(myCamera == null) && !(myCamera.activeTexture == null))
			{
				if (destinationWidth != myCamera.activeTexture.width || destinationHeight != myCamera.activeTexture.height || !isCommandBufferAlive)
				{
					destinationWidth = myCamera.activeTexture.width;
					destinationHeight = myCamera.activeTexture.height;
					TeardownCommandBuffer();
					EnsureCommandBuffer();
				}
				else
				{
					destinationWidth = myCamera.activeTexture.width;
					destinationHeight = myCamera.activeTexture.height;
				}
			}
		}

		private void OnDisable()
		{
			TeardownCommandBuffer();
		}

		private void OnDestroy()
		{
			TeardownCommandBuffer();
			onDestroyCalled = true;
		}

		protected void PerformOnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!isSupported || !vaoShader.isSupported)
			{
				base.enabled = false;
			}
			else
			{
				if (CommandBufferEnabled)
				{
					return;
				}
				TeardownCommandBuffer();
				int width = source.width / Downsampling;
				int height = source.height / Downsampling;
				RenderTexture renderTexture = null;
				RenderTexture renderTexture2 = null;
				if (HierarchicalBufferEnabled)
				{
					RenderTextureFormat format = RenderTextureFormat.RHalf;
					if (Mode == EffectMode.ColorBleed)
					{
						format = RenderTextureFormat.ARGBHalf;
					}
					renderTexture = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, format);
					renderTexture.filterMode = FilterMode.Bilinear;
					renderTexture2 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, format);
					renderTexture2.filterMode = FilterMode.Bilinear;
					Graphics.Blit(null, renderTexture, VAOMaterial, 13);
					DoShaderBlitCopy(renderTexture, renderTexture2);
					if (renderTexture != null)
					{
						VAOMaterial.SetTexture("depthNormalsTexture2", renderTexture);
					}
					if (renderTexture2 != null)
					{
						VAOMaterial.SetTexture("depthNormalsTexture4", renderTexture2);
					}
				}
				RenderTextureFormat format2 = RenderTextureFormat.RGHalf;
				if (Mode == EffectMode.ColorBleed)
				{
					format2 = ((!isHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
				}
				RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, format2);
				temporary.filterMode = FilterMode.Bilinear;
				VAOMaterial.SetTexture("noiseTexture", noiseTexture);
				RenderTexture renderTexture3 = null;
				RenderTexture renderTexture4 = null;
				if (CullingPrepassMode != 0)
				{
					RenderTextureFormat format3 = RenderTextureFormat.R8;
					renderTexture3 = RenderTexture.GetTemporary(source.width / CullingPrepassDownsamplingFactor, source.height / CullingPrepassDownsamplingFactor, 0, format3);
					renderTexture3.filterMode = FilterMode.Bilinear;
					renderTexture4 = RenderTexture.GetTemporary(source.width / (CullingPrepassDownsamplingFactor * 2), source.height / (CullingPrepassDownsamplingFactor * 2), 0, format3);
					renderTexture4.filterMode = FilterMode.Bilinear;
					Graphics.Blit(source, renderTexture3, VAOMaterial, 0);
					DoShaderBlitCopy(renderTexture3, renderTexture4);
				}
				if (renderTexture4 != null)
				{
					VAOMaterial.SetTexture("cullingPrepassTexture", renderTexture4);
				}
				Graphics.Blit(source, temporary, VAOMaterial, 1);
				VAOMaterial.SetTexture("textureAO", temporary);
				if (BlurMode != 0)
				{
					int width2 = source.width;
					int num = source.height;
					if (BlurQuality == BlurQualityType.Fast)
					{
						num /= 2;
					}
					if (BlurMode == BlurModeType.Enhanced)
					{
						RenderTexture temporary2 = RenderTexture.GetTemporary(width2, num, 0, format2);
						temporary2.filterMode = FilterMode.Bilinear;
						Graphics.Blit(null, temporary2, VAOMaterial, 6);
						VAOMaterial.SetTexture("textureAO", temporary2);
						Graphics.Blit(source, destination, VAOMaterial, 7);
						RenderTexture.ReleaseTemporary(temporary2);
					}
					else
					{
						int pass = ((BlurQuality != 0) ? 2 : 4);
						Graphics.Blit(source, destination, VAOMaterial, pass);
					}
				}
				else
				{
					Graphics.Blit(source, destination, VAOMaterial, 9);
				}
				if (temporary != null)
				{
					RenderTexture.ReleaseTemporary(temporary);
				}
				if (renderTexture3 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture3);
				}
				if (renderTexture4 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture4);
				}
				if (renderTexture != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture);
				}
				if (renderTexture2 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture2);
				}
			}
		}

		private void EnsureCommandBuffer(bool settingsDirty = false)
		{
			if ((!settingsDirty && isCommandBufferAlive) || !CommandBufferEnabled || onDestroyCalled)
			{
				return;
			}
			try
			{
				CreateCommandBuffer();
				lastCameraEvent = GetCameraEvent(VaoCameraEvent);
				isCommandBufferAlive = true;
			}
			catch (Exception ex)
			{
				ReportError("There was an error while trying to create command buffer. " + ex.Message);
			}
		}

		private void CreateCommandBuffer()
		{
			VAOMaterial = null;
			EnsureMaterials();
			TrySetUniforms();
			CameraEvent cameraEvent = GetCameraEvent(VaoCameraEvent);
			CommandBuffer value;
			if (cameraEventsRegistered.TryGetValue(cameraEvent, out value))
			{
				value.Clear();
			}
			else
			{
				value = new CommandBuffer();
				myCamera.AddCommandBuffer(cameraEvent, value);
				value.name = "Volumetric Ambient Occlusion";
				cameraEventsRegistered[cameraEvent] = value;
			}
			bool flag = !OutputAOOnly && Mode != EffectMode.ColorBleed;
			RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
			int? num = null;
			int? primaryTarget = null;
			int pixelWidth = destinationWidth;
			int pixelHeight = destinationHeight;
			if (pixelWidth <= 0)
			{
				pixelWidth = myCamera.pixelWidth;
			}
			if (pixelHeight <= 0)
			{
				pixelHeight = myCamera.pixelHeight;
			}
			int num2 = pixelWidth / Downsampling;
			int num3 = pixelHeight / Downsampling;
			if (!OutputAOOnly)
			{
				if (!isHDR && (cameraEvent == CameraEvent.AfterLighting || cameraEvent == CameraEvent.BeforeReflections))
				{
					renderTargetIdentifier = BuiltinRenderTextureType.GBuffer3;
					num = Shader.PropertyToID("emissionTextureRT");
					value.GetTemporaryRT(num.Value, pixelWidth, pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
					value.Blit(BuiltinRenderTextureType.GBuffer3, num.Value, VAOMaterial, 14);
					value.SetGlobalTexture("emissionTexture", num.Value);
					flag = false;
				}
				if (cameraEvent == CameraEvent.BeforeReflections || (cameraEvent == CameraEvent.AfterLighting && !isHDR && isSPSR))
				{
					primaryTarget = Shader.PropertyToID("occlusionTextureRT");
					value.GetTemporaryRT(primaryTarget.Value, pixelWidth, pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
					value.SetGlobalTexture("occlusionTexture", primaryTarget.Value);
					flag = false;
				}
			}
			int? source = null;
			if (Mode == EffectMode.ColorBleed)
			{
				RenderTextureFormat renderTextureFormat = GetRenderTextureFormat(IntermediateScreenTextureFormat, isHDR);
				source = Shader.PropertyToID("screenTextureRT");
				value.GetTemporaryRT(source.Value, pixelWidth, pixelHeight, 0, FilterMode.Bilinear, renderTextureFormat, RenderTextureReadWrite.Linear);
				value.Blit(BuiltinRenderTextureType.CameraTarget, source.Value);
			}
			int num4 = Shader.PropertyToID("vaoTextureRT");
			RenderTextureFormat format = RenderTextureFormat.RGHalf;
			if (Mode == EffectMode.ColorBleed)
			{
				format = ((!isHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
			}
			value.GetTemporaryRT(num4, num2, num3, 0, FilterMode.Bilinear, format, RenderTextureReadWrite.Linear);
			int? num5 = null;
			int? num6 = null;
			int? num7 = null;
			int? num8 = null;
			if (HierarchicalBufferEnabled)
			{
				RenderTextureFormat format2 = RenderTextureFormat.RHalf;
				if (Mode == EffectMode.ColorBleed)
				{
					format2 = RenderTextureFormat.ARGBHalf;
				}
				num7 = Shader.PropertyToID("downscaled2TextureRT");
				num8 = Shader.PropertyToID("downscaled4TextureRT");
				value.GetTemporaryRT(num7.Value, pixelWidth / 2, pixelHeight / 2, 0, FilterMode.Bilinear, format2, RenderTextureReadWrite.Linear);
				value.GetTemporaryRT(num8.Value, pixelWidth / 4, pixelHeight / 4, 0, FilterMode.Bilinear, format2, RenderTextureReadWrite.Linear);
				value.Blit(null, num7.Value, VAOMaterial, 13);
				value.Blit(num7.Value, num8.Value);
				if (num7.HasValue)
				{
					value.SetGlobalTexture("depthNormalsTexture2", num7.Value);
				}
				if (num8.HasValue)
				{
					value.SetGlobalTexture("depthNormalsTexture4", num8.Value);
				}
			}
			if (CullingPrepassMode != 0)
			{
				num5 = Shader.PropertyToID("cullingPrepassTextureRT");
				num6 = Shader.PropertyToID("cullingPrepassTextureHalfResRT");
				RenderTextureFormat format3 = RenderTextureFormat.R8;
				value.GetTemporaryRT(num5.Value, num2 / CullingPrepassDownsamplingFactor, num3 / CullingPrepassDownsamplingFactor, 0, FilterMode.Bilinear, format3, RenderTextureReadWrite.Linear);
				value.GetTemporaryRT(num6.Value, num2 / (CullingPrepassDownsamplingFactor * 2), num3 / (CullingPrepassDownsamplingFactor * 2), 0, FilterMode.Bilinear, format3, RenderTextureReadWrite.Linear);
				if (Mode == EffectMode.ColorBleed)
				{
					value.Blit(source.Value, num5.Value, VAOMaterial, 0);
				}
				else
				{
					value.Blit(renderTargetIdentifier, num5.Value, VAOMaterial, 0);
				}
				value.Blit(num5.Value, num6.Value);
				value.SetGlobalTexture("cullingPrepassTexture", num6.Value);
			}
			value.SetGlobalTexture("noiseTexture", noiseTexture);
			if (Mode == EffectMode.ColorBleed)
			{
				value.Blit(source.Value, num4, VAOMaterial, 1);
			}
			else
			{
				value.Blit(renderTargetIdentifier, num4, VAOMaterial, 1);
			}
			value.SetGlobalTexture("textureAO", num4);
			if (BlurMode != 0)
			{
				int width = pixelWidth;
				int num9 = pixelHeight;
				if (BlurQuality == BlurQualityType.Fast)
				{
					num9 /= 2;
				}
				if (BlurMode == BlurModeType.Enhanced)
				{
					int num10 = Shader.PropertyToID("tempTextureRT");
					value.GetTemporaryRT(num10, width, num9, 0, FilterMode.Bilinear, format, RenderTextureReadWrite.Linear);
					value.Blit(null, num10, VAOMaterial, 6);
					value.SetGlobalTexture("textureAO", num10);
					DoMixingBlit(value, source, primaryTarget, renderTargetIdentifier, (!flag) ? 7 : 8);
					value.ReleaseTemporaryRT(num10);
				}
				else
				{
					int num11 = ((BlurQuality != 0) ? 2 : 4);
					int num12 = ((BlurQuality != 0) ? 3 : 5);
					DoMixingBlit(value, source, primaryTarget, renderTargetIdentifier, (!flag) ? num11 : num12);
				}
			}
			else
			{
				DoMixingBlit(value, source, primaryTarget, renderTargetIdentifier, (!flag) ? 9 : 10);
			}
			switch (cameraEvent)
			{
			case CameraEvent.BeforeReflections:
				value.SetRenderTarget(new RenderTargetIdentifier[2]
				{
					BuiltinRenderTextureType.GBuffer0,
					renderTargetIdentifier
				}, BuiltinRenderTextureType.GBuffer0);
				value.DrawMesh(GetScreenQuad(), Matrix4x4.identity, VAOMaterial, 0, (!isHDR) ? 12 : 11);
				break;
			case CameraEvent.AfterLighting:
				if (!isHDR && isSPSR)
				{
					value.SetRenderTarget(renderTargetIdentifier);
					value.DrawMesh(GetScreenQuad(), Matrix4x4.identity, VAOMaterial, 0, 15);
				}
				break;
			}
			value.ReleaseTemporaryRT(num4);
			if (source.HasValue)
			{
				value.ReleaseTemporaryRT(source.Value);
			}
			if (num.HasValue)
			{
				value.ReleaseTemporaryRT(num.Value);
			}
			if (primaryTarget.HasValue)
			{
				value.ReleaseTemporaryRT(primaryTarget.Value);
			}
			if (num5.HasValue)
			{
				value.ReleaseTemporaryRT(num5.Value);
			}
			if (num6.HasValue)
			{
				value.ReleaseTemporaryRT(num6.Value);
			}
			if (num7.HasValue)
			{
				value.ReleaseTemporaryRT(num7.Value);
			}
			if (num8.HasValue)
			{
				value.ReleaseTemporaryRT(num8.Value);
			}
		}

		private void TeardownCommandBuffer()
		{
			if (!isCommandBufferAlive)
			{
				return;
			}
			try
			{
				isCommandBufferAlive = false;
				if (myCamera != null)
				{
					foreach (KeyValuePair<CameraEvent, CommandBuffer> item in cameraEventsRegistered)
					{
						myCamera.RemoveCommandBuffer(item.Key, item.Value);
					}
				}
				cameraEventsRegistered.Clear();
				VAOMaterial = null;
				EnsureMaterials();
			}
			catch (Exception ex)
			{
				ReportError("There was an error while trying to destroy command buffer. " + ex.Message);
			}
		}

		protected Mesh GetScreenQuad()
		{
			if (screenQuad == null)
			{
				screenQuad = new Mesh
				{
					vertices = new Vector3[4]
					{
						new Vector3(-1f, -1f, 0f),
						new Vector3(-1f, 1f, 0f),
						new Vector3(1f, 1f, 0f),
						new Vector3(1f, -1f, 0f)
					},
					triangles = new int[6] { 0, 1, 2, 0, 2, 3 },
					uv = new Vector2[4]
					{
						new Vector2(0f, 1f),
						new Vector2(0f, 0f),
						new Vector2(1f, 0f),
						new Vector2(1f, 1f)
					}
				};
			}
			return screenQuad;
		}

		private CameraEvent GetCameraEvent(VAOCameraEventType vaoCameraEvent)
		{
			if (myCamera == null)
			{
				return CameraEvent.BeforeImageEffectsOpaque;
			}
			if (OutputAOOnly)
			{
				return CameraEvent.BeforeImageEffectsOpaque;
			}
			if (Mode == EffectMode.ColorBleed)
			{
				return CameraEvent.BeforeImageEffectsOpaque;
			}
			if (myCamera.actualRenderingPath != RenderingPath.DeferredShading)
			{
				return CameraEvent.BeforeImageEffectsOpaque;
			}
			switch (vaoCameraEvent)
			{
			case VAOCameraEventType.AfterLighting:
				return CameraEvent.AfterLighting;
			case VAOCameraEventType.BeforeImageEffectsOpaque:
				return CameraEvent.BeforeImageEffectsOpaque;
			case VAOCameraEventType.BeforeReflections:
				return CameraEvent.BeforeReflections;
			default:
				return CameraEvent.BeforeImageEffectsOpaque;
			}
		}

		private void DoShaderBlitCopy(Texture sourceTexture, RenderTexture destinationTexture)
		{
			if (isSPSR && !CommandBufferEnabled)
			{
				VAOMaterial.SetTexture("texCopySource", sourceTexture);
				Graphics.Blit(sourceTexture, destinationTexture, VAOMaterial, 16);
			}
			else
			{
				Graphics.Blit(sourceTexture, destinationTexture);
			}
		}

		protected void DoMixingBlit(CommandBuffer commandBuffer, int? source, int? primaryTarget, RenderTargetIdentifier secondaryTarget, int pass)
		{
			if (primaryTarget.HasValue)
			{
				DoBlit(commandBuffer, source, primaryTarget.Value, pass);
			}
			else
			{
				DoBlit(commandBuffer, source, secondaryTarget, pass);
			}
		}

		protected void DoBlit(CommandBuffer commandBuffer, int? source, int target, int pass)
		{
			if (source.HasValue)
			{
				commandBuffer.Blit(source.Value, target, VAOMaterial, pass);
			}
			else
			{
				commandBuffer.Blit(null, target, VAOMaterial, pass);
			}
		}

		protected void DoBlit(CommandBuffer commandBuffer, int? source, RenderTargetIdentifier target, int pass)
		{
			if (source.HasValue)
			{
				commandBuffer.Blit(source.Value, target, VAOMaterial, pass);
			}
			else
			{
				commandBuffer.Blit(null, target, VAOMaterial, pass);
			}
		}

		private void TrySetUniforms()
		{
			if (VAOMaterial == null)
			{
				return;
			}
			int num = myCamera.pixelWidth / Downsampling;
			int num2 = myCamera.pixelHeight / Downsampling;
			Vector4[] array = null;
			switch (Quality)
			{
			case 2:
				VAOMaterial.SetInt("minLevelIndex", 15);
				array = samp2;
				break;
			case 4:
				VAOMaterial.SetInt("minLevelIndex", 7);
				array = samp4;
				break;
			case 8:
				VAOMaterial.SetInt("minLevelIndex", 3);
				array = samp8;
				break;
			case 16:
				VAOMaterial.SetInt("minLevelIndex", 1);
				array = samp16;
				break;
			case 32:
				VAOMaterial.SetInt("minLevelIndex", 0);
				array = samp32;
				break;
			case 64:
				VAOMaterial.SetInt("minLevelIndex", 0);
				array = samp64;
				break;
			default:
				ReportError("Unsupported quality setting " + Quality + " encountered. Reverting to low setting");
				VAOMaterial.SetInt("minLevelIndex", 1);
				Quality = 16;
				array = samp16;
				break;
			}
			if (AdaptiveType != 0)
			{
				switch (Quality)
				{
				case 64:
					AdaptiveQuality = 0.025f;
					break;
				case 32:
					AdaptiveQuality = 0.025f;
					break;
				case 16:
					AdaptiveQuality = 0.05f;
					break;
				case 8:
					AdaptiveQuality = 0.1f;
					break;
				case 4:
					AdaptiveQuality = 0.2f;
					break;
				case 2:
					AdaptiveQuality = 0.4f;
					break;
				}
				if (AdaptiveType == AdaptiveSamplingType.EnabledManual)
				{
					AdaptiveQuality *= AdaptiveQualityCoefficient;
				}
				else
				{
					AdaptiveQualityCoefficient = 1f;
				}
			}
			AdaptiveMax = GetDepthForScreenSize(myCamera, AdaptiveQuality);
			Vector2 vector = new Vector2(1f / (float)num, 1f / (float)num2);
			float depthForScreenSize = GetDepthForScreenSize(myCamera, Mathf.Max(vector.x, vector.y));
			bool flag = GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting && isSPSR && !isHDR;
			VAOMaterial.SetInt("isImageEffectMode", (!CommandBufferEnabled) ? 1 : 0);
			VAOMaterial.SetInt("useSPSRFriendlyTransform", (isSPSR && !CommandBufferEnabled) ? 1 : 0);
			VAOMaterial.SetMatrix("invProjMatrix", myCamera.projectionMatrix.inverse);
			VAOMaterial.SetVector("screenProjection", -0.5f * new Vector4(myCamera.projectionMatrix.m00, myCamera.projectionMatrix.m11, myCamera.projectionMatrix.m02, myCamera.projectionMatrix.m12));
			VAOMaterial.SetFloat("halfRadiusSquared", Radius * 0.5f * (Radius * 0.5f));
			VAOMaterial.SetFloat("halfRadius", Radius * 0.5f);
			VAOMaterial.SetFloat("radius", Radius);
			VAOMaterial.SetInt("sampleCount", Quality);
			VAOMaterial.SetInt("fourSamplesStartIndex", Quality);
			VAOMaterial.SetFloat("aoPower", Power);
			VAOMaterial.SetFloat("aoPresence", Presence);
			VAOMaterial.SetFloat("giPresence", 1f - ColorBleedPresence);
			VAOMaterial.SetFloat("LumaThreshold", LumaThreshold);
			VAOMaterial.SetFloat("LumaKneeWidth", LumaKneeWidth);
			VAOMaterial.SetFloat("LumaTwiceKneeWidthRcp", 1f / (LumaKneeWidth * 2f));
			VAOMaterial.SetFloat("LumaKneeLinearity", LumaKneeLinearity);
			VAOMaterial.SetInt("giBackfaces", (!GiBackfaces) ? 1 : 0);
			VAOMaterial.SetFloat("adaptiveMin", AdaptiveMin);
			VAOMaterial.SetFloat("adaptiveMax", AdaptiveMax);
			VAOMaterial.SetVector("texelSize", (BlurMode != BlurModeType.Basic || BlurQuality != 0) ? vector : (vector * 0.5f));
			VAOMaterial.SetFloat("blurDepthThreshold", Radius);
			VAOMaterial.SetInt("cullingPrepassMode", (int)CullingPrepassMode);
			VAOMaterial.SetVector("cullingPrepassTexelSize", new Vector2(0.5f / (float)(myCamera.pixelWidth / CullingPrepassDownsamplingFactor), 0.5f / (float)(myCamera.pixelHeight / CullingPrepassDownsamplingFactor)));
			VAOMaterial.SetInt("giSelfOcclusionFix", (int)ColorBleedSelfOcclusionFixLevel);
			VAOMaterial.SetInt("adaptiveMode", (int)AdaptiveType);
			VAOMaterial.SetInt("LumaMode", (int)LuminanceMode);
			VAOMaterial.SetFloat("cameraFarPlane", myCamera.farClipPlane);
			VAOMaterial.SetInt("UseCameraFarPlane", (FarPlaneSource == FarPlaneSourceType.Camera) ? 1 : 0);
			VAOMaterial.SetFloat("maxRadiusEnabled", MaxRadiusEnabled ? 1 : 0);
			VAOMaterial.SetFloat("maxRadiusCutoffDepth", GetDepthForScreenSize(myCamera, MaxRadius));
			VAOMaterial.SetFloat("projMatrix11", myCamera.projectionMatrix.m11);
			VAOMaterial.SetFloat("maxRadiusOnScreen", MaxRadius);
			VAOMaterial.SetFloat("enhancedBlurSize", EnhancedBlurSize / 2);
			VAOMaterial.SetInt("flipY", MustForceFlip(myCamera) ? 1 : 0);
			VAOMaterial.SetInt("useGBuffer", ShouldUseGBuffer() ? 1 : 0);
			VAOMaterial.SetInt("hierarchicalBufferEnabled", HierarchicalBufferEnabled ? 1 : 0);
			VAOMaterial.SetInt("hwBlendingEnabled", (CommandBufferEnabled && Mode != EffectMode.ColorBleed && GetCameraEvent(VaoCameraEvent) != CameraEvent.BeforeReflections && !flag) ? 1 : 0);
			VAOMaterial.SetInt("useLogEmissiveBuffer", (CommandBufferEnabled && !isHDR && GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting && !isSPSR) ? 1 : 0);
			VAOMaterial.SetInt("useLogBufferInput", (CommandBufferEnabled && !isHDR && (GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting || GetCameraEvent(VaoCameraEvent) == CameraEvent.BeforeReflections)) ? 1 : 0);
			VAOMaterial.SetInt("outputAOOnly", OutputAOOnly ? 1 : 0);
			VAOMaterial.SetInt("isLumaSensitive", IsLumaSensitive ? 1 : 0);
			VAOMaterial.SetInt("useFastBlur", (BlurQuality == BlurQualityType.Fast) ? 1 : 0);
			VAOMaterial.SetInt("useDedicatedDepthBuffer", (UsePreciseDepthBuffer && (myCamera.actualRenderingPath == RenderingPath.Forward || myCamera.actualRenderingPath == RenderingPath.VertexLit)) ? 1 : 0);
			float depthForScreenSize2 = GetDepthForScreenSize(myCamera, Mathf.Max(vector.x, vector.y) * HierarchicalBufferPixelsPerLevel * 2f);
			float depthForScreenSize3 = GetDepthForScreenSize(myCamera, Mathf.Max(vector.x, vector.y) * HierarchicalBufferPixelsPerLevel);
			depthForScreenSize2 /= 0f - myCamera.farClipPlane;
			depthForScreenSize3 /= 0f - myCamera.farClipPlane;
			VAOMaterial.SetFloat("quarterResBufferMaxDistance", depthForScreenSize2);
			VAOMaterial.SetFloat("halfResBufferMaxDistance", depthForScreenSize3);
			VAOMaterial.SetInt("minRadiusEnabled", (int)DistanceFalloffMode);
			VAOMaterial.SetFloat("minRadiusCutoffDepth", (DistanceFalloffMode != DistanceFalloffModeType.Relative) ? (0f - DistanceFalloffStartAbsolute) : (Mathf.Abs(depthForScreenSize) * (0f - DistanceFalloffStartRelative * DistanceFalloffStartRelative)));
			VAOMaterial.SetFloat("minRadiusSoftness", (DistanceFalloffMode != DistanceFalloffModeType.Relative) ? DistanceFalloffSpeedAbsolute : (Mathf.Abs(depthForScreenSize) * (DistanceFalloffSpeedRelative * DistanceFalloffSpeedRelative)));
			VAOMaterial.SetInt("giSameHueAttenuationEnabled", ColorbleedHueSuppresionEnabled ? 1 : 0);
			VAOMaterial.SetFloat("giSameHueAttenuationThreshold", ColorBleedHueSuppresionThreshold);
			VAOMaterial.SetFloat("giSameHueAttenuationWidth", ColorBleedHueSuppresionWidth);
			VAOMaterial.SetFloat("giSameHueAttenuationSaturationThreshold", ColorBleedHueSuppresionSaturationThreshold);
			VAOMaterial.SetFloat("giSameHueAttenuationSaturationWidth", ColorBleedHueSuppresionSaturationWidth);
			VAOMaterial.SetFloat("giSameHueAttenuationBrightness", ColorBleedHueSuppresionBrightness);
			VAOMaterial.SetFloat("subpixelRadiusCutoffDepth", Mathf.Min(0.99f, depthForScreenSize / (0f - myCamera.farClipPlane)));
			VAOMaterial.SetVector("noiseTexelSizeRcp", new Vector2((float)num / 3f, (float)num2 / 3f));
			if (Quality == 4 || (Quality == 8 && (ColorBleedQuality == 2 || ColorBleedQuality == 4)))
			{
				VAOMaterial.SetInt("giBlur", 3);
			}
			else
			{
				VAOMaterial.SetInt("giBlur", 2);
			}
			if (Mode == EffectMode.ColorBleed)
			{
				VAOMaterial.SetFloat("giPower", ColorBleedPower);
				if (Quality == 2 && ColorBleedQuality == 4)
				{
					VAOMaterial.SetInt("giQuality", 2);
				}
				else
				{
					VAOMaterial.SetInt("giQuality", ColorBleedQuality);
				}
			}
			if (CullingPrepassMode != 0)
			{
				SetVectorArrayNoBuffer("eightSamples", VAOMaterial, samp8);
			}
			if (AdaptiveType != 0)
			{
				SetSampleSet("samples", VAOMaterial, GetAdaptiveSamples());
			}
			else if (CullingPrepassMode == CullingPrepassModeType.Careful)
			{
				SetSampleSet("samples", VAOMaterial, GetCarefulCullingPrepassSamples(array, samp4));
			}
			else
			{
				SetSampleSet("samples", VAOMaterial, array);
			}
			if (Mode == EffectMode.Simple)
			{
				VAOMaterial.SetColor("colorTint", Color.black);
			}
			else
			{
				VAOMaterial.SetColor("colorTint", ColorTint);
			}
			if (BlurMode == BlurModeType.Enhanced)
			{
				if (gaussian == null || gaussian.Length != EnhancedBlurSize || EnhancedBlurDeviation != lastDeviation)
				{
					gaussian = GenerateGaussian(EnhancedBlurSize, EnhancedBlurDeviation, out gaussianWeight, false);
					lastDeviation = EnhancedBlurDeviation;
				}
				VAOMaterial.SetFloat("gaussWeight", gaussianWeight);
				SetVectorArray("gauss", VAOMaterial, gaussian, ref gaussianBuffer, ref lastEnhancedBlurSize, true);
			}
			VAOMaterial.SetFloatArray("adaptiveLengths", adaptiveLengths);
			VAOMaterial.SetFloatArray("adaptiveStarts", adaptiveStarts);
			SetKeywords("WFORCE_VAO_COLORBLEED_OFF", "WFORCE_VAO_COLORBLEED_ON", Mode == EffectMode.ColorBleed);
		}

		private void SetKeywords(string offState, string onState, bool state)
		{
			if (state)
			{
				VAOMaterial.DisableKeyword(offState);
				VAOMaterial.EnableKeyword(onState);
			}
			else
			{
				VAOMaterial.DisableKeyword(onState);
				VAOMaterial.EnableKeyword(offState);
			}
		}

		private void EnsureMaterials()
		{
			if (vaoShader == null)
			{
				vaoShader = Shader.Find("Hidden/Wilberforce/VAOShader");
			}
			if (!VAOMaterial && vaoShader.isSupported)
			{
				VAOMaterial = CreateMaterial(vaoShader);
			}
			if (!vaoShader.isSupported)
			{
				ReportError("Could not create shader (Shader not supported).");
			}
		}

		private static Material CreateMaterial(Shader shader)
		{
			if (!shader)
			{
				return null;
			}
			Material material = new Material(shader);
			material.hideFlags = HideFlags.HideAndDontSave;
			return material;
		}

		private static void DestroyMaterial(Material mat)
		{
			if ((bool)mat)
			{
				UnityEngine.Object.DestroyImmediate(mat);
				mat = null;
			}
		}

		private void SetVectorArrayNoBuffer(string name, Material VAOMaterial, Vector4[] samples)
		{
			VAOMaterial.SetVectorArray(name, samples);
		}

		private void SetVectorArray(string name, Material Material, Vector4[] samples, ref Vector4[] samplesBuffer, ref int lastBufferLength, bool needsUpdate)
		{
			if (needsUpdate || lastBufferLength != samples.Length)
			{
				Array.Copy(samples, samplesBuffer, samples.Length);
				lastBufferLength = samples.Length;
			}
			Material.SetVectorArray(name, samplesBuffer);
		}

		private void SetSampleSet(string name, Material VAOMaterial, Vector4[] samples)
		{
			SetVectorArray(name, VAOMaterial, samples, ref samplesLarge, ref lastSamplesLength, false);
		}

		private Vector4[] GetAdaptiveSamples()
		{
			if (adaptiveSamples == null)
			{
				adaptiveSamples = GenerateAdaptiveSamples();
			}
			return adaptiveSamples;
		}

		private Vector4[] GetCarefulCullingPrepassSamples(Vector4[] samples, Vector4[] carefulSamples)
		{
			if (carefulCache != null && carefulCache.Length == samples.Length + carefulSamples.Length)
			{
				return carefulCache;
			}
			carefulCache = new Vector4[samples.Length + carefulSamples.Length];
			Array.Copy(samples, 0, carefulCache, 0, samples.Length);
			Array.Copy(carefulSamples, 0, carefulCache, samples.Length, carefulSamples.Length);
			return carefulCache;
		}

		private Vector4[] GenerateAdaptiveSamples()
		{
			Vector4[] array = new Vector4[62];
			Array.Copy(samp32, 0, array, 0, 32);
			Array.Copy(samp16, 0, array, 32, 16);
			Array.Copy(samp8, 0, array, 48, 8);
			Array.Copy(samp4, 0, array, 56, 4);
			Array.Copy(samp2, 0, array, 60, 2);
			return array;
		}

		private void EnsureNoiseTexture()
		{
			if (noiseTexture == null)
			{
				noiseTexture = new Texture2D(3, 3, TextureFormat.RGFloat, false, true);
				noiseTexture.SetPixels(noiseSamples);
				noiseTexture.filterMode = FilterMode.Point;
				noiseTexture.wrapMode = TextureWrapMode.Repeat;
				noiseTexture.Apply();
			}
		}

		private static Vector4[] GenerateGaussian(int size, float d, out float weight, bool normalize = true)
		{
			Vector4[] array = new Vector4[size];
			float num = 0f;
			double num2 = 2.0 * (double)d * (double)d;
			double num3 = Math.Sqrt(num2 * Math.PI);
			float num4 = 1f / (float)(size + 1);
			for (int i = 0; i < size; i++)
			{
				float num5 = (float)i / (float)(size + 1);
				num5 += num4;
				num5 *= 6f;
				float num6 = num5 - 3f;
				float num7 = 0f - (float)((0.0 - Math.Exp((double)(0f - num6 * num6) / num2)) / num3);
				array[i].x = num7;
				num += num7;
			}
			if (normalize)
			{
				for (int j = 0; j < size; j++)
				{
					array[j].x /= num;
				}
			}
			weight = num;
			return array;
		}

		private float GetDepthForScreenSize(Camera camera, float sizeOnScreen)
		{
			return (0f - Radius * camera.projectionMatrix.m11) / sizeOnScreen;
		}

		public bool ShouldUseHierarchicalBuffer()
		{
			if (myCamera == null)
			{
				return false;
			}
			Vector2 vector = new Vector2(1f / (float)myCamera.pixelWidth, 1f / (float)myCamera.pixelHeight);
			float depthForScreenSize = GetDepthForScreenSize(myCamera, Mathf.Max(vector.x, vector.y) * HierarchicalBufferPixelsPerLevel * 2f);
			depthForScreenSize /= 0f - myCamera.farClipPlane;
			return depthForScreenSize > 0.1f;
		}

		public bool ShouldUseGBuffer()
		{
			if (myCamera == null)
			{
				return UseGBuffer;
			}
			if (myCamera.actualRenderingPath != RenderingPath.DeferredShading)
			{
				return false;
			}
			if (VaoCameraEvent != VAOCameraEventType.BeforeImageEffectsOpaque)
			{
				return true;
			}
			return UseGBuffer;
		}

		protected void EnsureVAOVersion()
		{
			if ((CommandBufferEnabled && (object)this != null && !(this is VAOEffect)) || (!CommandBufferEnabled && this is VAOEffect))
			{
				return;
			}
			Component[] components = GetComponents<Component>();
			List<KeyValuePair<FieldInfo, object>> parameters = GetParameters();
			int num = -1;
			Component component = null;
			for (int i = 0; i < components.Length; i++)
			{
				if (CommandBufferEnabled && components[i] == this)
				{
					GameObject gameObject = base.gameObject;
					UnityEngine.Object.DestroyImmediate(this);
					component = gameObject.AddComponent<VAOEffectCommandBuffer>();
					(component as VAOEffectCommandBuffer).SetParameters(parameters);
					num = i;
					break;
				}
				if (!CommandBufferEnabled && components[i] == this)
				{
					GameObject gameObject2 = base.gameObject;
					TeardownCommandBuffer();
					UnityEngine.Object.DestroyImmediate(this);
					component = gameObject2.AddComponent<VAOEffect>();
					(component as VAOEffect).SetParameters(parameters);
					num = i;
					break;
				}
			}
			if (num >= 0 && !(component != null))
			{
			}
		}

		private bool CheckSettingsChanges()
		{
			bool result = false;
			if (GetCameraEvent(VaoCameraEvent) != lastCameraEvent)
			{
				TeardownCommandBuffer();
				result = true;
			}
			if (Downsampling != lastDownsampling)
			{
				lastDownsampling = Downsampling;
				result = true;
			}
			if (CullingPrepassMode != lastcullingPrepassType)
			{
				lastcullingPrepassType = CullingPrepassMode;
				result = true;
			}
			if (CullingPrepassDownsamplingFactor != lastCullingPrepassDownsamplingFactor)
			{
				lastCullingPrepassDownsamplingFactor = CullingPrepassDownsamplingFactor;
				result = true;
			}
			if (BlurMode != lastBlurMode)
			{
				lastBlurMode = BlurMode;
				result = true;
			}
			if (Mode != lastMode)
			{
				lastMode = Mode;
				result = true;
			}
			if (UseGBuffer != lastUseGBuffer)
			{
				lastUseGBuffer = UseGBuffer;
				result = true;
			}
			if (OutputAOOnly != lastOutputAOOnly)
			{
				lastOutputAOOnly = OutputAOOnly;
				result = true;
			}
			isHDR = isCameraHDR(myCamera);
			if (isHDR != lastIsHDR)
			{
				lastIsHDR = isHDR;
				result = true;
			}
			if (lastIntermediateScreenTextureFormat != IntermediateScreenTextureFormat)
			{
				lastIntermediateScreenTextureFormat = IntermediateScreenTextureFormat;
				result = true;
			}
			if (lastCmdBufferEnhancedBlurSize != EnhancedBlurSize)
			{
				lastCmdBufferEnhancedBlurSize = EnhancedBlurSize;
				result = true;
			}
			if (lastHierarchicalBufferEnabled != HierarchicalBufferEnabled)
			{
				lastHierarchicalBufferEnabled = HierarchicalBufferEnabled;
				result = true;
			}
			if (lastBlurQuality != BlurQuality)
			{
				lastBlurQuality = BlurQuality;
				result = true;
			}
			return result;
		}

		private void WarmupPass(KeyValuePair<string, string>[] input, int i, RenderTexture tempTarget, int passNumber)
		{
			if (i >= 0)
			{
				SetKeywords(input[i].Key, input[i].Value, false);
				WarmupPass(input, i - 1, tempTarget, passNumber);
				SetKeywords(input[i].Key, input[i].Value, true);
				WarmupPass(input, i - 1, tempTarget, passNumber);
			}
			Graphics.Blit(null, tempTarget, VAOMaterial, passNumber);
		}

		private RenderTextureFormat GetRenderTextureFormat(ScreenTextureFormat format, bool isHDR)
		{
			switch (format)
			{
			case ScreenTextureFormat.Default:
				return RenderTextureFormat.Default;
			case ScreenTextureFormat.DefaultHDR:
				return RenderTextureFormat.DefaultHDR;
			case ScreenTextureFormat.ARGB32:
				return RenderTextureFormat.ARGB32;
			case ScreenTextureFormat.ARGBFloat:
				return RenderTextureFormat.ARGBFloat;
			case ScreenTextureFormat.ARGBHalf:
				return RenderTextureFormat.ARGBHalf;
			default:
				return (!isHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
			}
		}

		private void ReportError(string error)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogError("VAO Effect Error: " + error);
			}
		}

		private void ReportWarning(string error)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogWarning("VAO Effect Warning: " + error);
			}
		}

		private bool isCameraSPSR(Camera camera)
		{
			if (camera == null)
			{
				return false;
			}
			if (camera.stereoEnabled)
			{
				return XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes;
			}
			return false;
		}

		private bool isCameraHDR(Camera camera)
		{
			if (camera != null)
			{
				return camera.allowHDR;
			}
			return false;
		}

		private bool MustForceFlip(Camera camera)
		{
			return false;
		}

		protected List<KeyValuePair<FieldInfo, object>> GetParameters()
		{
			List<KeyValuePair<FieldInfo, object>> list = new List<KeyValuePair<FieldInfo, object>>();
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				list.Add(new KeyValuePair<FieldInfo, object>(fieldInfo, fieldInfo.GetValue(this)));
			}
			return list;
		}

		protected void SetParameters(List<KeyValuePair<FieldInfo, object>> parameters)
		{
			foreach (KeyValuePair<FieldInfo, object> parameter in parameters)
			{
				parameter.Key.SetValue(this, parameter.Value);
			}
		}
	}
}
