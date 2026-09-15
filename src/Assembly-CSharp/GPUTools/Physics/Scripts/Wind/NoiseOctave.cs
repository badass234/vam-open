using System;

namespace GPUTools.Physics.Scripts.Wind
{
	[Serializable]
	public struct NoiseOctave
	{
		public float Scale;

		public float Amplitude;

		public NoiseOctave(float scale, float amplitude)
		{
			Scale = scale;
			Amplitude = amplitude;
		}
	}
}
