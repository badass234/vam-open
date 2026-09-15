using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Shapes
{
	public struct GPSphereWithDelta
	{
		public Vector3 Position;

		public float Radius;

		public float Friction;

		public Vector3 Delta;

		public GPSphereWithDelta(Vector3 position, float radius)
		{
			Position = position;
			Radius = radius;
			Friction = 1f;
			Delta = Vector3.zero;
		}

		public static int Size()
		{
			return 32;
		}
	}
}
