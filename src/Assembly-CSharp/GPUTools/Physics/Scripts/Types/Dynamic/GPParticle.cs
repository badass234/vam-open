using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Dynamic
{
	public struct GPParticle
	{
		public Vector3 Position;

		public Vector3 LastPosition;

		public Vector3 LastPositionInner;

		public Vector3 DrawPosition;

		public Vector3 Velocity;

		public float Radius;

		public Vector3 ColliderDelta;

		public float CollisionDrag;

		public float CollisionHold;

		public float CollisionPower;

		public float Strength;

		public int CollisionEnabled;

		public int SphereCollisionID;

		public int LineSphereCollisionID;

		public int GrabID;

		public float GrabDistance;

		public float AuxData;

		public GPParticle(Vector3 position, float radius)
		{
			Position = position;
			LastPosition = position;
			LastPositionInner = position;
			DrawPosition = position;
			Velocity = Vector3.zero;
			Radius = radius;
			ColliderDelta = Vector3.zero;
			CollisionDrag = 0f;
			CollisionHold = 0f;
			CollisionPower = 0f;
			Strength = 0.1f;
			CollisionEnabled = 1;
			SphereCollisionID = -1;
			LineSphereCollisionID = -1;
			GrabID = -1;
			GrabDistance = 0f;
			AuxData = 0f;
		}

		public static int Size()
		{
			return 116;
		}
	}
}
