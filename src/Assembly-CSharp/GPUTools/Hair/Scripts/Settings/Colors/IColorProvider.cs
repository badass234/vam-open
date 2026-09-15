using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings.Colors
{
	public interface IColorProvider
	{
		Color GetColor(HairSettings settings, int x, int y, int sizeY);
	}
}
