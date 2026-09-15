using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Shapes
{
	public struct GPLineSphere
	{
		public Vector3 PositionA;

		public Vector3 PositionB;

		public float RadiusA;

		public float RadiusB;

		public float Friction;

		public GPLineSphere(Vector3 positionA, Vector3 positionB, float radiusA, float radiusB)
		{
			PositionA = positionA;
			PositionB = positionB;
			RadiusA = radiusA;
			RadiusB = radiusB;
			Friction = 1f;
		}

		public static int Size()
		{
			return 36;
		}
	}
}
