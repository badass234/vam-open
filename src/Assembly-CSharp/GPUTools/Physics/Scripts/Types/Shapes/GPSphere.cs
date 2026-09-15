using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Shapes
{
	public struct GPSphere
	{
		public Vector3 Position;

		public float Radius;

		public float Friction;

		public GPSphere(Vector3 position, float radius)
		{
			Position = position;
			Radius = radius;
			Friction = 1f;
		}

		public static int Size()
		{
			return 20;
		}
	}
}
