using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Runtime.Render;
using GPUTools.Hair.Scripts.Settings;
using GPUTools.Hair.Scripts.Settings.Colors;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Render
{
	public class BuildParticlesData : IBuildCommand
	{
		private readonly HairSettings settings;

		public BuildParticlesData(HairSettings settings)
		{
			this.settings = settings;
		}

		public void Build()
		{
			RenderParticle[] array = ((settings.RuntimeData.Particles == null) ? new RenderParticle[0] : new RenderParticle[settings.RuntimeData.Particles.Count]);
			UpdateBodies(array);
			List<Vector3> list = GenRandoms();
			if (settings.RuntimeData.RenderParticles != null)
			{
				settings.RuntimeData.RenderParticles.Dispose();
			}
			if (settings.RuntimeData.RandomsPerStrand != null)
			{
				settings.RuntimeData.RandomsPerStrand.Dispose();
			}
			if (array.Length > 0)
			{
				settings.RuntimeData.RenderParticles = new GpuBuffer<RenderParticle>(array, RenderParticle.Size());
				settings.RuntimeData.RandomsPerStrand = new GpuBuffer<Vector3>(list.ToArray(), 12);
			}
			else
			{
				settings.RuntimeData.RenderParticles = null;
				settings.RuntimeData.RandomsPerStrand = null;
			}
		}

		public void UpdateSettings()
		{
			if (settings.RuntimeData.RenderParticles != null)
			{
				UpdateBodies(settings.RuntimeData.RenderParticles.Data);
				settings.RuntimeData.RenderParticles.PushData();
			}
		}

		private List<Vector3> GenRandoms()
		{
			List<Vector3> list = new List<Vector3>();
			Random.InitState(5);
			Vector3 item = default(Vector3);
			for (int i = 0; i < settings.StandsSettings.Provider.GetStandsNum(); i++)
			{
				item.x = Random.Range(0f, 1f);
				item.y = Random.Range(0f, 1f);
				item.z = Random.Range(0f, 1f);
				list.Add(item);
			}
			return list;
		}

		private void UpdateBodies(RenderParticle[] renderParticles)
		{
			HairRenderSettings renderSettings = settings.RenderSettings;
			int segmentsNum = settings.StandsSettings.Provider.GetSegmentsNum();
			RootTipColorProvider rootTipColorProvider = null;
			if (renderSettings.ColorProvider is RootTipColorProvider)
			{
				rootTipColorProvider = renderSettings.ColorProvider as RootTipColorProvider;
			}
			float[] particleRootToTipRatios = settings.RuntimeData.ParticleRootToTipRatios;
			float num = Mathf.Clamp(renderSettings.InterpolationMidpoint, 0.001f, 1f);
			float num2 = Mathf.Clamp(renderSettings.WavinessMidpoint, 0.001f, 1f);
			for (int i = 0; i < renderParticles.Length; i++)
			{
				int num3 = i / segmentsNum;
				int num4 = i % segmentsNum;
				float num5 = (float)num4 / (float)(segmentsNum - 1);
				Vector3 color;
				if (rootTipColorProvider != null)
				{
					float y = particleRootToTipRatios[i];
					color = ColorToVector(rootTipColorProvider.GetColor(settings, y));
				}
				else
				{
					color = ColorToVector(renderSettings.ColorProvider.GetColor(settings, num3, num4, segmentsNum));
				}
				float interpolation;
				if (settings.PhysicsSettings.StyleMode)
				{
					interpolation = 0f;
				}
				else if (renderSettings.UseInterpolationCurves)
				{
					interpolation = Mathf.Clamp01(renderSettings.InterpolationCurve.Evaluate(num5));
				}
				else
				{
					interpolation = 0f;
					if (num5 <= renderSettings.InterpolationMidpoint)
					{
						float num6 = num5 / num;
						float f = 1f - num6;
						float t = Mathf.Pow(f, renderSettings.InterpolationCurvePower);
						interpolation = 1f - Mathf.Lerp(renderSettings.InterpolationMid, renderSettings.InterpolationRoot, t);
					}
					else
					{
						float f2 = (num5 - num) / (1f - num);
						float t2 = Mathf.Pow(f2, renderSettings.InterpolationCurvePower);
						interpolation = 1f - Mathf.Lerp(renderSettings.InterpolationMid, renderSettings.InterpolationTip, t2);
					}
				}
				float num8;
				float num7;
				if (settings.PhysicsSettings.StyleMode)
				{
					num7 = 0f;
					num8 = 1f;
				}
				else if (renderSettings.UseWavinessCurves)
				{
					num7 = Mathf.Clamp01(renderSettings.WavinessScaleCurve.Evaluate(num5));
					num8 = Mathf.Clamp01(renderSettings.WavinessFrequencyCurve.Evaluate(num5));
				}
				else
				{
					num7 = 0f;
					if (num5 <= renderSettings.WavinessMidpoint)
					{
						float num9 = num5 / num2;
						float f3 = 1f - num9;
						float t3 = Mathf.Pow(f3, renderSettings.WavinessCurvePower);
						num7 = Mathf.Lerp(renderSettings.WavinessMid, renderSettings.WavinessRoot, t3);
					}
					else
					{
						float f4 = (num5 - num2) / (1f - num2);
						float t4 = Mathf.Pow(f4, renderSettings.WavinessCurvePower);
						num7 = Mathf.Lerp(renderSettings.WavinessMid, renderSettings.WavinessTip, t4);
					}
					num8 = 1f;
				}
				RenderParticle renderParticle = default(RenderParticle);
				renderParticle.RootIndex = num3;
				renderParticle.Color = color;
				renderParticle.Interpolation = interpolation;
				renderParticle.WavinessScale = num7 * renderSettings.WavinessScale;
				renderParticle.WavinessFrequency = num8 * renderSettings.WavinessFrequency;
				RenderParticle renderParticle2 = renderParticle;
				renderParticles[i] = renderParticle2;
			}
		}

		public Vector3 ColorToVector(Color color)
		{
			return new Vector3(color.r, color.g, color.b);
		}

		public void Dispatch()
		{
		}

		public void FixedDispatch()
		{
		}

		public void Dispose()
		{
			if (settings.RuntimeData.RenderParticles != null)
			{
				settings.RuntimeData.RenderParticles.Dispose();
			}
			if (settings.RuntimeData.RandomsPerStrand != null)
			{
				settings.RuntimeData.RandomsPerStrand.Dispose();
			}
		}
	}
}
