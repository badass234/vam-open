using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Shapes
{
	public struct GPGrabSphere
	{
		public int ID;

		public Vector3 Position;

		public float Radius;

		public int GrabbedThisFrame;

		public GPGrabSphere(int id, Vector3 position, float radius)
		{
			ID = id;
			Position = position;
			Radius = radius;
			GrabbedThisFrame = 0;
		}

		public static int Size()
		{
			return 24;
		}
	}
}
