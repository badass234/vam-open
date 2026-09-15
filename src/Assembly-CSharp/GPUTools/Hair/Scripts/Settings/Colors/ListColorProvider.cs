using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings.Colors
{
	[Serializable]
	public class ListColorProvider : IColorProvider
	{
		public List<Color> Colors = new List<Color>();

		public Color GetColor(HairSettings settings, int x, int y, int sizeY)
		{
			return GetStandColor((float)y / (float)sizeY);
		}

		private Color GetStandColor(float t)
		{
			if (Colors.Count == 0)
			{
				return Color.black;
			}
			float value = (float)Colors.Count * t;
			int index = (int)Mathf.Clamp(value, 0f, Colors.Count - 1);
			return Colors[index];
		}
	}
}
