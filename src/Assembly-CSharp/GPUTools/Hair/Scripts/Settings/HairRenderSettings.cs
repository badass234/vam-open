using System;
using GPUTools.Hair.Scripts.Settings.Abstract;
using GPUTools.Hair.Scripts.Settings.Colors;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings
{
	[Serializable]
	public class HairRenderSettings : HairSettingsBase
	{
		public ColorProviderType ColorProviderType;

		public RootTipColorProvider RootTipColorProvider;

		public ListColorProvider ListColorProvider;

		public GeometryColorProvider GeometryColorProvider;

		public Material material;

		public float PrimarySpecular = 50f;

		public float SecondarySpecular = 50f;

		public Color SpecularColor = new Color(0.15f, 0.15f, 0.15f);

		public float SpecularShift = 0.01f;

		public float Diffuse = 0.75f;

		public float FresnelPower = 10f;

		public float FresnelAttenuation = 1f;

		public float Length1 = 1f;

		public float Length2 = 1f;

		public float Length3 = 1f;

		public bool UseWavinessCurves = true;

		public float WavinessScale;

		public AnimationCurve WavinessScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float WavinessFrequency;

		public AnimationCurve WavinessFrequencyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public Vector3 WavinessAxis = new Vector3(0.1f, 0f, 0.1f);

		public float WavinessRoot = 1f;

		public float WavinessMid = 1f;

		public float WavinessTip = 1f;

		public float WavinessMidpoint = 0.5f;

		public float WavinessCurvePower = 1f;

		public float WavinessScaleRandomness = 1f;

		public float WavinessFrequencyRandomness = 1f;

		public bool WavinessAllowReverse;

		public bool WavinessAllowFlipAxis;

		public float WavinessNormalAdjust;

		public bool StyleModeShowCurls;

		public bool UseInterpolationCurves = true;

		public AnimationCurve InterpolationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

		public float InterpolationRoot = 1f;

		public float InterpolationMid = 1f;

		public float InterpolationTip = 1f;

		public float InterpolationMidpoint = 0.5f;

		public float InterpolationCurvePower = 1f;

		public AnimationCurve WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public float Volume;

		public float BarycentricVolume = 0.015f;

		public float RandomTexColorPower = 1f;

		public float RandomTexColorOffset = 0.3f;

		public float IBLFactor = 0.5f;

		public float MaxSpread = 0.05f;

		public float NormalRandomize;

		public IColorProvider ColorProvider
		{
			get
			{
				if (ColorProviderType == ColorProviderType.RootTip)
				{
					return RootTipColorProvider;
				}
				if (ColorProviderType == ColorProviderType.List)
				{
					return ListColorProvider;
				}
				return GeometryColorProvider;
			}
		}
	}
}
