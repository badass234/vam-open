using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Cloth.Scripts.Types;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.DebugDraw
{
	public class ClothDebugDraw
	{
		public static void Draw(ClothSettings settings)
		{
			if (settings.GeometryData.Particles == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Matrix4x4 toWorldMatrix = settings.MeshProvider.ToWorldMatrix;
			ClothGeometryData geometryData = settings.GeometryData;
			if (settings.GeometryDebugDrawDistanceJoints)
			{
				foreach (Int2ListContainer jointGroup in geometryData.JointGroups)
				{
					foreach (Int2 item in jointGroup.List)
					{
						Vector3 from = toWorldMatrix.MultiplyPoint3x4(geometryData.Particles[item.X]);
						Vector3 to = toWorldMatrix.MultiplyPoint3x4(geometryData.Particles[item.Y]);
						Gizmos.DrawLine(from, to);
					}
				}
			}
			if (!settings.GeometryDebugDrawStiffnessJoints)
			{
				return;
			}
			Gizmos.color = Color.yellow;
			foreach (Int2ListContainer stiffnessJointGroup in geometryData.StiffnessJointGroups)
			{
				foreach (Int2 item2 in stiffnessJointGroup.List)
				{
					Vector3 from2 = toWorldMatrix.MultiplyPoint3x4(geometryData.Particles[item2.X]);
					Vector3 to2 = toWorldMatrix.MultiplyPoint3x4(geometryData.Particles[item2.Y]);
					Gizmos.DrawLine(from2, to2);
				}
			}
		}

		public static void DrawAlways(ClothSettings settings)
		{
			if (settings.CustomBounds)
			{
				Bounds bounds = settings.Bounds;
				Bounds bounds2 = new Bounds(settings.transform.TransformPoint(bounds.center), bounds.size);
				Gizmos.DrawWireCube(bounds2.center, bounds2.size);
			}
		}
	}
}
