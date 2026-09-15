using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Render
{
	public struct TessRenderParticle
	{
		public Vector3 Position;

		public Vector3 Velocity;

		public Vector3 LightCenter;

		public Vector3 Color;

		public float Interpolation;

		public int RootIndex;

		public static int Size()
		{
			return 56;
		}
	}
}
