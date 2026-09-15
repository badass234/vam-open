using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings.Colors
{
	[Serializable]
	public class GeometryColorProvider : IColorProvider
	{
		public Color GetColor(HairSettings settings, int x, int y, int sizeY)
		{
			List<Color> colors = settings.StandsSettings.Provider.GetColors();
			int index = x * sizeY + y;
			return colors[index];
		}
	}
}
