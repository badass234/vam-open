using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Shapes
{
	public struct GPLineSphereWithDelta
	{
		public Vector3 PositionA;

		public Vector3 PositionB;

		public float RadiusA;

		public float RadiusB;

		public float Friction;

		public Vector3 DeltaA;

		public Vector3 DeltaB;

		public Vector3 Delta;

		public GPLineSphereWithDelta(Vector3 positionA, Vector3 positionB, float radiusA, float radiusB)
		{
			PositionA = positionA;
			PositionB = positionB;
			RadiusA = radiusA;
			RadiusB = radiusB;
			Friction = 1f;
			DeltaA = Vector3.zero;
			DeltaB = Vector3.zero;
			Delta = Vector3.zero;
		}

		public static int Size()
		{
			return 72;
		}
	}
}
