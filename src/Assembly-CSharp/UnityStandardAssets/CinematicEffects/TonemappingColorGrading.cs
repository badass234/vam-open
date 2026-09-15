using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Color Adjustments/Tonemapping and Color Grading")]
	public class TonemappingColorGrading : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		public class DrawFilmicCurveAttribute : Attribute
		{
		}

		public enum Passes
		{
			ThreeD,
			OneD,
			ThreeDDebug,
			OneDDebug
		}

		[Serializable]
		public struct FilmicCurve
		{
			public bool enabled;

			[Range(-4f, 4f)]
			[Tooltip("Exposure Bias|Adjusts the overall exposure of the scene")]
			public float exposureBias;

			[Range(0f, 2f)]
			[Tooltip("Contrast|Contrast adjustment (log-space)")]
			public float contrast;

			[Range(0f, 1f)]
			[Tooltip("Toe|Toe of the filmic curve; affects the darker areas of the scene")]
			public float toe;

			[Range(0f, 1f)]
			[Tooltip("Shoulder|Shoulder of the filmic curve; brings overexposed highlights back into range")]
			public float lutShoulder;

			public static FilmicCurve defaultFilmicCurve = new FilmicCurve
			{
				enabled = false,
				exposureBias = 0f,
				contrast = 1f,
				toe = 0f,
				lutShoulder = 0f
			};
		}

		public class ColorWheelGroup : PropertyAttribute
		{
			public int minSizePerWheel = 60;

			public int maxSizePerWheel = 150;

			public ColorWheelGroup()
			{
			}

			public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
			{
				this.minSizePerWheel = minSizePerWheel;
				this.maxSizePerWheel = maxSizePerWheel;
			}
		}

		[Serializable]
		public struct ColorGradingColors
		{
			[Tooltip("Shadows|Shadows color")]
			public Color shadows;

			[Tooltip("Midtones|Midtones color")]
			public Color midtones;

			[Tooltip("Highlights|Highlights color")]
			public Color highlights;

			public static ColorGradingColors defaultGradingColors = new ColorGradingColors
			{
				shadows = new Color(1f, 1f, 1f),
				midtones = new Color(1f, 1f, 1f),
				highlights = new Color(1f, 1f, 1f)
			};
		}

		[Serializable]
		public struct ColorGrading
		{
			public bool enabled;

			[ColorUsage(false)]
			[Tooltip("White Balance|Adjusts the white color before tonemapping")]
			public Color whiteBalance;

			[Range(0f, 2f)]
			[Tooltip("Vibrance|Pushes the intensity of all colors")]
			public float saturation;

			[Range(0f, 5f)]
			[Tooltip("Gamma|Adjusts the gamma")]
			public float gamma;

			[ColorWheelGroup]
			public ColorGradingColors lutColors;

			public static ColorGrading defaultColorGrading = new ColorGrading
			{
				whiteBalance = Color.white,
				enabled = false,
				saturation = 1f,
				gamma = 1f,
				lutColors = ColorGradingColors.defaultGradingColors
			};
		}

		public struct SimplePolyFunc
		{
			public float A;

			public float B;

			public float x0;

			public float y0;

			public float signX;

			public float signY;

			public float logA;

			public float Eval(float x)
			{
				return signY * Mathf.Exp(logA + B * Mathf.Log(signX * x - x0)) + y0;
			}

			public void Initialize(float x_end, float y_end, float m)
			{
				A = 0f;
				B = 1f;
				x0 = 0f;
				y0 = 0f;
				signX = 1f;
				signY = 1f;
				if (!(m <= 0f) && !(y_end <= 0f) && !(x_end <= 0f))
				{
					B = m * x_end / y_end;
					float num = Mathf.Pow(x_end, B);
					A = y_end / num;
					logA = Mathf.Log(y_end) - B * Mathf.Log(x_end);
				}
			}
		}

		[NonSerialized]
		public bool fastMode;

		public bool debugClamp;

		[NonSerialized]
		private bool m_Dirty = true;

		[SerializeField]
		[SettingsGroup]
		[DrawFilmicCurve]
		private FilmicCurve m_FilmicCurve = FilmicCurve.defaultFilmicCurve;

		[SerializeField]
		[SettingsGroup]
		private ColorGrading m_ColorGrading = ColorGrading.defaultColorGrading;

		private Texture3D m_LutTex;

		private Texture2D m_LutCurveTex1D;

		[SerializeField]
		[Tooltip("Lookup Texture|Custom lookup texture")]
		private Texture2D m_UserLutTexture;

		public Shader tonemapShader;

		public bool validRenderTextureFormat = true;

		private Material m_TonemapMaterial;

		private int m_UserLutDim = 16;

		private Color[] m_UserLutData;

		public FilmicCurve filmicCurve
		{
			get
			{
				return m_FilmicCurve;
			}
			set
			{
				m_FilmicCurve = value;
				SetDirty();
			}
		}

		public ColorGrading colorGrading
		{
			get
			{
				return m_ColorGrading;
			}
			set
			{
				m_ColorGrading = value;
				SetDirty();
			}
		}

		private bool isLinearColorSpace => QualitySettings.activeColorSpace == ColorSpace.Linear;

		public Texture2D userLutTexture
		{
			get
			{
				return m_UserLutTexture;
			}
			set
			{
				m_UserLutTexture = value;
				SetDirty();
			}
		}

		public Material tonemapMaterial
		{
			get
			{
				if (m_TonemapMaterial == null)
				{
					m_TonemapMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(tonemapShader);
				}
				return m_TonemapMaterial;
			}
		}

		public void SetDirty()
		{
			m_Dirty = true;
		}

		private void OnValidate()
		{
			SetDirty();
		}

		protected void OnEnable()
		{
			if (tonemapShader == null)
			{
				tonemapShader = Shader.Find("Hidden/TonemappingColorGrading");
			}
			if (!ImageEffectHelper.IsSupported(tonemapShader, false, true, this))
			{
				base.enabled = false;
				Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
			}
		}

		private float GetHighlightRecovery()
		{
			return Mathf.Max(0f, m_FilmicCurve.lutShoulder * 3f);
		}

		public float GetWhitePoint()
		{
			return Mathf.Pow(2f, Mathf.Max(0f, GetHighlightRecovery()));
		}

		private static float LutToLin(float x, float lutA)
		{
			x = ((!(x >= 1f)) ? x : 1f);
			float num = x / lutA;
			return num / (1f - num);
		}

		private static float LinToLut(float x, float lutA)
		{
			return Mathf.Sqrt(x / (x + lutA));
		}

		private static float LiftGammaGain(float x, float lift, float invGamma, float gain)
		{
			float num = Mathf.Sqrt(x);
			float num2 = gain * (lift * (1f - num) + Mathf.Pow(num, invGamma));
			return num2 * num2;
		}

		private static float LogContrast(float x, float linRef, float contrast)
		{
			x = Mathf.Max(x, 1E-05f);
			float num = Mathf.Log(linRef);
			float num2 = Mathf.Log(x);
			float power = num + (num2 - num) * contrast;
			return Mathf.Exp(power);
		}

		private static Color NormalizeColor(Color c)
		{
			float num = (c.r + c.g + c.b) / 3f;
			if (num == 0f)
			{
				return new Color(1f, 1f, 1f, 1f);
			}
			Color result = default(Color);
			result.r = c.r / num;
			result.g = c.g / num;
			result.b = c.b / num;
			result.a = 1f;
			return result;
		}

		public static float GetLutA()
		{
			return 1.05f;
		}

		private void SetIdentityLut()
		{
			int num = 16;
			Color[] array = new Color[num * num * num];
			float num2 = 1f / (1f * (float)num - 1f);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num; k++)
					{
						array[i + j * num + k * num * num] = new Color((float)i * 1f * num2, (float)j * 1f * num2, (float)k * 1f * num2, 1f);
					}
				}
			}
			m_UserLutData = array;
			m_UserLutDim = num;
		}

		private int ClampLutDim(int src)
		{
			return Mathf.Clamp(src, 0, m_UserLutDim - 1);
		}

		private Color SampleLutNearest(int r, int g, int b)
		{
			r = ClampLutDim(r);
			g = ClampLutDim(g);
			g = ClampLutDim(b);
			return m_UserLutData[r + g * m_UserLutDim + b * m_UserLutDim * m_UserLutDim];
		}

		private Color SampleLutNearestUnsafe(int r, int g, int b)
		{
			return m_UserLutData[r + g * m_UserLutDim + b * m_UserLutDim * m_UserLutDim];
		}

		private Color SampleLutLinear(float srcR, float srcG, float srcB)
		{
			float num = 0f;
			float num2 = m_UserLutDim - 1;
			float num3 = srcR * num2 + num;
			float num4 = srcG * num2 + num;
			float num5 = srcB * num2 + num;
			int src = Mathf.FloorToInt(num3);
			int src2 = Mathf.FloorToInt(num4);
			int src3 = Mathf.FloorToInt(num5);
			src = ClampLutDim(src);
			src2 = ClampLutDim(src2);
			src3 = ClampLutDim(src3);
			int r = ClampLutDim(src + 1);
			int g = ClampLutDim(src2 + 1);
			int b = ClampLutDim(src3 + 1);
			float t = num3 - (float)src;
			float t2 = num4 - (float)src2;
			float t3 = num5 - (float)src3;
			Color a = SampleLutNearestUnsafe(src, src2, src3);
			Color b2 = SampleLutNearestUnsafe(src, src2, b);
			Color a2 = SampleLutNearestUnsafe(src, g, src3);
			Color b3 = SampleLutNearestUnsafe(src, g, b);
			Color a3 = SampleLutNearestUnsafe(r, src2, src3);
			Color b4 = SampleLutNearestUnsafe(r, src2, b);
			Color a4 = SampleLutNearestUnsafe(r, g, src3);
			Color b5 = SampleLutNearestUnsafe(r, g, b);
			Color a5 = Color.Lerp(a, b2, t3);
			Color b6 = Color.Lerp(a2, b3, t3);
			Color a6 = Color.Lerp(a3, b4, t3);
			Color b7 = Color.Lerp(a4, b5, t3);
			Color a7 = Color.Lerp(a5, b6, t2);
			Color b8 = Color.Lerp(a6, b7, t2);
			return Color.Lerp(a7, b8, t);
		}

		private void UpdateUserLut()
		{
			if (userLutTexture == null)
			{
				SetIdentityLut();
				return;
			}
			if (!ValidDimensions(userLutTexture))
			{
				Debug.LogWarning("The given 2D texture " + userLutTexture.name + " cannot be used as a 3D LUT. Reverting to identity.");
				SetIdentityLut();
				return;
			}
			int height = userLutTexture.height;
			Color[] pixels = userLutTexture.GetPixels();
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < height; j++)
				{
					for (int k = 0; k < height; k++)
					{
						int num = height - j - 1;
						Color color = pixels[k * height + i + num * height * height];
						array[i + j * height + k * height * height] = color;
					}
				}
			}
			m_UserLutDim = height;
			m_UserLutData = array;
		}

		public float EvalFilmicHelper(float src, float lutA, SimplePolyFunc polyToe, SimplePolyFunc polyLinear, SimplePolyFunc polyShoulder, float x0, float x1, float linearW)
		{
			float num = LutToLin(src, lutA);
			if (m_FilmicCurve.enabled)
			{
				float linRef = 0.18f;
				num = LogContrast(num, linRef, m_FilmicCurve.contrast);
				SimplePolyFunc simplePolyFunc = polyToe;
				if (num >= x0)
				{
					simplePolyFunc = polyLinear;
				}
				if (num >= x1)
				{
					simplePolyFunc = polyShoulder;
				}
				num = Mathf.Min(num, linearW);
				num = simplePolyFunc.Eval(num);
			}
			return num;
		}

		private float EvalCurveGradingHelper(float src, float lift, float invGamma, float gain)
		{
			float num = src;
			if (m_ColorGrading.enabled)
			{
				num = LiftGammaGain(num, lift, invGamma, gain);
			}
			num = Mathf.Max(num, 0f);
			if (m_ColorGrading.enabled)
			{
				num = Mathf.Pow(num, m_ColorGrading.gamma);
			}
			return num;
		}

		private void Create3DLut(float lutA, SimplePolyFunc polyToe, SimplePolyFunc polyLinear, SimplePolyFunc polyShoulder, float x0, float x1, float linearW, float liftR, float invGammaR, float gainR, float liftG, float invGammaG, float gainG, float liftB, float invGammaB, float gainB)
		{
			int num = 32;
			Color[] array = new Color[num * num * num];
			float num2 = 1f / (1f * (float)num - 1f);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num; k++)
					{
						float src = (float)i * 1f * num2;
						float src2 = (float)j * 1f * num2;
						float src3 = (float)k * 1f * num2;
						float srcR = EvalFilmicHelper(src, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
						float srcG = EvalFilmicHelper(src2, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
						float srcB = EvalFilmicHelper(src3, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
						Color color = SampleLutLinear(srcR, srcG, srcB);
						srcR = color.r;
						srcG = color.g;
						srcB = color.b;
						srcR = EvalCurveGradingHelper(srcR, liftR, invGammaR, gainR);
						srcG = EvalCurveGradingHelper(srcG, liftG, invGammaG, gainG);
						srcB = EvalCurveGradingHelper(srcB, liftB, invGammaB, gainB);
						if (m_ColorGrading.enabled)
						{
							float num3 = srcR * 0.2125f + srcG * 0.7154f + srcB * 0.0721f;
							srcR = num3 + (srcR - num3) * m_ColorGrading.saturation;
							srcG = num3 + (srcG - num3) * m_ColorGrading.saturation;
							srcB = num3 + (srcB - num3) * m_ColorGrading.saturation;
						}
						array[i + j * num + k * num * num] = new Color(srcR, srcG, srcB, 1f);
					}
				}
			}
			if (m_LutTex == null)
			{
				m_LutTex = new Texture3D(num, num, num, TextureFormat.RGB24, false);
				m_LutTex.filterMode = FilterMode.Bilinear;
				m_LutTex.wrapMode = TextureWrapMode.Clamp;
				m_LutTex.hideFlags = HideFlags.DontSave;
			}
			m_LutTex.SetPixels(array);
			m_LutTex.Apply();
		}

		private void Create1DLut(float lutA, SimplePolyFunc polyToe, SimplePolyFunc polyLinear, SimplePolyFunc polyShoulder, float x0, float x1, float linearW, float liftR, float invGammaR, float gainR, float liftG, float invGammaG, float gainG, float liftB, float invGammaB, float gainB)
		{
			int num = 128;
			Color[] array = new Color[num * 2];
			float num2 = 1f / (1f * (float)num - 1f);
			for (int i = 0; i < num; i++)
			{
				float src = (float)i * 1f * num2;
				float src2 = (float)i * 1f * num2;
				float src3 = (float)i * 1f * num2;
				float srcR = EvalFilmicHelper(src, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
				float srcG = EvalFilmicHelper(src2, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
				float srcB = EvalFilmicHelper(src3, lutA, polyToe, polyLinear, polyShoulder, x0, x1, linearW);
				Color color = SampleLutLinear(srcR, srcG, srcB);
				srcR = color.r;
				srcG = color.g;
				srcB = color.b;
				srcR = EvalCurveGradingHelper(srcR, liftR, invGammaR, gainR);
				srcG = EvalCurveGradingHelper(srcG, liftG, invGammaG, gainG);
				srcB = EvalCurveGradingHelper(srcB, liftB, invGammaB, gainB);
				if (isLinearColorSpace)
				{
					srcR = Mathf.LinearToGammaSpace(srcR);
					srcG = Mathf.LinearToGammaSpace(srcG);
					srcB = Mathf.LinearToGammaSpace(srcB);
				}
				array[i + 0 * num] = new Color(srcR, srcG, srcB, 1f);
				array[i + num] = new Color(srcR, srcG, srcB, 1f);
			}
			if (m_LutCurveTex1D == null)
			{
				m_LutCurveTex1D = new Texture2D(num, 2, TextureFormat.RGB24, false);
				m_LutCurveTex1D.filterMode = FilterMode.Bilinear;
				m_LutCurveTex1D.wrapMode = TextureWrapMode.Clamp;
				m_LutCurveTex1D.hideFlags = HideFlags.DontSave;
			}
			m_LutCurveTex1D.SetPixels(array);
			m_LutCurveTex1D.Apply();
		}

		private void UpdateLut()
		{
			UpdateUserLut();
			float lutA = GetLutA();
			float p = 2.2f;
			float num = Mathf.Pow(1f / 3f, p);
			float f = 0.7f;
			float num2 = Mathf.Pow(f, p);
			float f2 = Mathf.Pow(f, 1f + m_FilmicCurve.lutShoulder * 1f);
			float num3 = Mathf.Pow(f2, p);
			float num4 = num / num2;
			float num5 = num4 * num3;
			float num6 = num5 * (1f - m_FilmicCurve.toe * 0.5f);
			float num7 = num6;
			float num8 = num2 - num;
			float num9 = num3 - num7;
			float num10 = 0f;
			if (num8 > 0f && num9 > 0f)
			{
				num10 = num9 / num8;
			}
			SimplePolyFunc simplePolyFunc = default(SimplePolyFunc);
			simplePolyFunc.x0 = num;
			simplePolyFunc.y0 = num7;
			simplePolyFunc.A = num10;
			simplePolyFunc.B = 1f;
			simplePolyFunc.signX = 1f;
			simplePolyFunc.signY = 1f;
			simplePolyFunc.logA = Mathf.Log(num10);
			SimplePolyFunc polyToe = simplePolyFunc;
			polyToe.Initialize(num, num7, num10);
			float whitePoint = GetWhitePoint();
			float x_end = whitePoint - num2;
			float y_end = 1f - num3;
			SimplePolyFunc polyShoulder = simplePolyFunc;
			polyShoulder.Initialize(x_end, y_end, num10);
			polyShoulder.signX = -1f;
			polyShoulder.x0 = 0f - whitePoint;
			polyShoulder.signY = -1f;
			polyShoulder.y0 = 1f;
			Color color = NormalizeColor(m_ColorGrading.lutColors.shadows);
			Color color2 = NormalizeColor(m_ColorGrading.lutColors.midtones);
			Color color3 = NormalizeColor(m_ColorGrading.lutColors.highlights);
			float num11 = (color.r + color.g + color.b) / 3f;
			float num12 = (color2.r + color2.g + color2.b) / 3f;
			float num13 = (color3.r + color3.g + color3.b) / 3f;
			float num14 = 0.1f;
			float num15 = 0.5f;
			float num16 = 0.5f;
			float liftR = (color.r - num11) * num14;
			float liftG = (color.g - num11) * num14;
			float liftB = (color.b - num11) * num14;
			float b = Mathf.Pow(2f, (color2.r - num12) * num15);
			float b2 = Mathf.Pow(2f, (color2.g - num12) * num15);
			float b3 = Mathf.Pow(2f, (color2.b - num12) * num15);
			float gainR = Mathf.Pow(2f, (color3.r - num13) * num16);
			float gainG = Mathf.Pow(2f, (color3.g - num13) * num16);
			float gainB = Mathf.Pow(2f, (color3.b - num13) * num16);
			float a = 0.01f;
			float invGammaR = 1f / Mathf.Max(a, b);
			float invGammaG = 1f / Mathf.Max(a, b2);
			float invGammaB = 1f / Mathf.Max(a, b3);
			if (!fastMode)
			{
				Create3DLut(lutA, polyToe, simplePolyFunc, polyShoulder, num, num2, whitePoint, liftR, invGammaR, gainR, liftG, invGammaG, gainG, liftB, invGammaB, gainB);
			}
			else
			{
				Create1DLut(lutA, polyToe, simplePolyFunc, polyShoulder, num, num2, whitePoint, liftR, invGammaR, gainR, liftG, invGammaG, gainG, liftB, invGammaB, gainB);
			}
		}

		public bool ValidDimensions(Texture2D tex2d)
		{
			if (!tex2d)
			{
				return false;
			}
			int height = tex2d.height;
			if (height != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width)))
			{
				return false;
			}
			return true;
		}

		public void Convert(Texture2D temp2DTex)
		{
		}

		private void OnDisable()
		{
			if ((bool)m_TonemapMaterial)
			{
				UnityEngine.Object.DestroyImmediate(m_TonemapMaterial);
				m_TonemapMaterial = null;
			}
			if ((bool)m_LutTex)
			{
				UnityEngine.Object.DestroyImmediate(m_LutTex);
				m_LutTex = null;
			}
			if ((bool)m_LutCurveTex1D)
			{
				UnityEngine.Object.DestroyImmediate(m_LutCurveTex1D);
				m_LutCurveTex1D = null;
			}
		}

		[ImageEffectTransformsToLDR]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (tonemapMaterial == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (m_LutTex == null || m_Dirty)
			{
				UpdateLut();
				m_Dirty = false;
			}
			if (fastMode)
			{
				tonemapMaterial.SetTexture("_LutTex1D", m_LutCurveTex1D);
			}
			else
			{
				tonemapMaterial.SetTexture("_LutTex", m_LutTex);
			}
			float lutA = GetLutA();
			float num = Mathf.Pow(2f, (!m_FilmicCurve.enabled) ? 0f : m_FilmicCurve.exposureBias);
			Vector4 value = new Vector4(num, num, num, 1f);
			Color c = new Color(1f, 1f, 1f, 1f);
			if (m_ColorGrading.enabled)
			{
				c.r = Mathf.Pow(m_ColorGrading.whiteBalance.r, 2.2f);
				c.g = Mathf.Pow(m_ColorGrading.whiteBalance.g, 2.2f);
				c.b = Mathf.Pow(m_ColorGrading.whiteBalance.b, 2.2f);
				Color color = NormalizeColor(c);
				value.x *= color.r;
				value.y *= color.g;
				value.z *= color.b;
			}
			tonemapMaterial.SetFloat("_LutA", lutA);
			tonemapMaterial.SetVector("_LutExposureMult", value);
			tonemapMaterial.SetFloat("_Vibrance", (!m_ColorGrading.enabled) ? 1f : m_ColorGrading.saturation);
			Graphics.Blit(pass: (!debugClamp) ? (fastMode ? 1 : 0) : ((!fastMode) ? 2 : 3), source: source, dest: destination, mat: tonemapMaterial);
		}
	}
}
