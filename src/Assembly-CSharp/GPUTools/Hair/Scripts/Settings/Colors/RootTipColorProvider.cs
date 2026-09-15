using System;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings.Colors
{
	[Serializable]
	public class RootTipColorProvider : IColorProvider
	{
		public Color RootColor = new Color(0.35f, 0.15f, 0.15f);

		public Color TipColor = new Color(0.15f, 0.05f, 0.05f);

		public AnimationCurve Blend = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float ColorRolloff = 1f;

		public Color GetColor(HairSettings settings, int x, int y, int sizeY)
		{
			return GetStandColor((float)y / (float)sizeY);
		}

		public Color GetColor(HairSettings settings, float y)
		{
			return GetStandColor(y);
		}

		private Color GetStandColor(float t)
		{
			float p = Mathf.Pow(2f, ColorRolloff) - 1f;
			float f = 1f - t;
			float t2 = Mathf.Pow(f, p);
			return Color.Lerp(TipColor, RootColor, t2);
		}
	}
}
