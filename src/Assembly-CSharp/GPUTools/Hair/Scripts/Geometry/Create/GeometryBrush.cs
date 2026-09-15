using System;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Create
{
	[Serializable]
	public class GeometryBrush
	{
		public Vector3 Dirrection;

		public float Radius = 0.01f;

		public float Lenght1 = 1f;

		public float Lenght2 = 1f;

		public float Strength = 0.25f;

		public Color Color = Color.white;

		public float CollisionDistance = 0.01f;

		public GeometryBrushBehaviour Behaviour;

		[NonSerialized]
		public bool IsBrushEnabled;

		private Vector3 position;

		public Vector3 OldPosition;

		public Vector3 Position
		{
			get
			{
				return position;
			}
			set
			{
				OldPosition = position;
				position = value;
			}
		}

		public Vector3 Speed => position - OldPosition;

		public Vector3 ToWorld(Matrix4x4 m, Vector3 local)
		{
			return Position + (Vector3)(m * local);
		}

		public bool Contains(Vector3 point)
		{
			Camera current = Camera.current;
			Vector3 vector = current.transform.InverseTransformPoint(point);
			Vector3 vector2 = current.transform.InverseTransformPoint(Position);
			bool flag = ((Vector2)vector - (Vector2)vector2).magnitude < Radius;
			bool flag2 = vector2.z - vector.z > 0f - Lenght1;
			bool flag3 = vector2.z - vector.z < Lenght2;
			return flag && flag2 && flag3;
		}
	}
}
