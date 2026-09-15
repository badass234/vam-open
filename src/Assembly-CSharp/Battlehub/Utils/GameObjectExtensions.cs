using System;
using UnityEngine;

namespace Battlehub.Utils
{
	public static class GameObjectExtensions
	{
		public static bool IsPrefab(this GameObject go)
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				throw new InvalidOperationException("Does not work in edit mode");
			}
			return go.scene.buildIndex < 0;
		}

		public static Bounds CalculateBounds(this GameObject g)
		{
			Transform transform = g.transform;
			Renderer componentInChildren = transform.GetComponentInChildren<Renderer>(true);
			if ((bool)componentInChildren)
			{
				Bounds totalBounds = componentInChildren.bounds;
				if (totalBounds.size == Vector3.zero && totalBounds.center != componentInChildren.transform.position)
				{
					totalBounds = TransformBounds(componentInChildren.transform.localToWorldMatrix, totalBounds);
				}
				CalculateBounds(transform, ref totalBounds);
				if (totalBounds.extents == Vector3.zero)
				{
					totalBounds.extents = new Vector3(0.5f, 0.5f, 0.5f);
				}
				return totalBounds;
			}
			return new Bounds(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
		}

		private static void CalculateBounds(Transform t, ref Bounds totalBounds)
		{
			foreach (Transform item in t)
			{
				Renderer component = item.GetComponent<Renderer>();
				if ((bool)component)
				{
					Bounds bounds = component.bounds;
					if (bounds.size == Vector3.zero && bounds.center != component.transform.position)
					{
						bounds = TransformBounds(component.transform.localToWorldMatrix, bounds);
					}
					totalBounds.Encapsulate(bounds.min);
					totalBounds.Encapsulate(bounds.max);
				}
				CalculateBounds(item, ref totalBounds);
			}
		}

		public static Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
		{
			Vector3 center = matrix.MultiplyPoint(bounds.center);
			Vector3 extents = bounds.extents;
			Vector3 vector = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
			Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
			Vector3 vector3 = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
			extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
			extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
			extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
			Bounds result = default(Bounds);
			result.center = center;
			result.extents = extents;
			return result;
		}
	}
}
