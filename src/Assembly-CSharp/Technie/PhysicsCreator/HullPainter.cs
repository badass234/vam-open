using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator
{
	public class HullPainter : MonoBehaviour
	{
		public PaintingData paintingData;

		public HullData hullData;

		public Dictionary<Hull, Collider> hullMapping;

		private void OnDestroy()
		{
		}

		public void CreateColliderComponents()
		{
			CreateHullMapping();
			foreach (Hull hull in paintingData.hulls)
			{
				CreateColliderComponent(hull);
			}
		}

		public void RemoveAllColliders()
		{
			CreateHullMapping();
			foreach (Collider value in hullMapping.Values)
			{
				Object.DestroyImmediate(value);
			}
			hullMapping.Clear();
		}

		private void CreateHullMapping()
		{
			if (hullMapping == null)
			{
				hullMapping = new Dictionary<Hull, Collider>();
			}
			List<Hull> list = new List<Hull>(hullMapping.Keys);
			foreach (Hull item in list)
			{
				if (item == null || hullMapping[item] == null)
				{
					Debug.Log("Removing invalid entry from hull mapping");
					hullMapping.Remove(item);
				}
			}
			foreach (Hull hull2 in paintingData.hulls)
			{
				if (hullMapping.ContainsKey(hull2))
				{
					Collider collider = hullMapping[hull2];
					bool flag = hull2.type == HullType.ConvexHull && collider is MeshCollider;
					bool flag2 = hull2.type == HullType.Box && collider is BoxCollider;
					bool flag3 = hull2.type == HullType.Sphere && collider is SphereCollider;
					bool flag4 = hull2.type == HullType.Face && collider is MeshCollider;
					if (!flag && !flag2 && !flag3 && !flag4)
					{
						Object.DestroyImmediate(collider);
						hullMapping.Remove(hull2);
					}
				}
			}
			List<Hull> list2 = new List<Hull>();
			List<Collider> list3 = new List<Collider>();
			foreach (Hull hull3 in paintingData.hulls)
			{
				if (!hullMapping.ContainsKey(hull3))
				{
					list2.Add(hull3);
				}
			}
			Collider[] components = GetComponents<Collider>();
			foreach (Collider collider2 in components)
			{
				if (!hullMapping.ContainsValue(collider2))
				{
					list3.Add(collider2);
				}
			}
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				Hull hull = list2[num];
				for (int num2 = list3.Count - 1; num2 >= 0; num2--)
				{
					Collider collider3 = list3[num2];
					BoxCollider boxCollider = collider3 as BoxCollider;
					SphereCollider sphereCollider = collider3 as SphereCollider;
					MeshCollider meshCollider = collider3 as MeshCollider;
					bool flag5 = hull.type == HullType.Box && collider3 is BoxCollider && Approximately(hull.collisionBox.center, boxCollider.center) && Approximately(hull.collisionBox.size, boxCollider.size);
					bool flag6 = hull.type == HullType.Sphere && collider3 is SphereCollider && hull.collisionSphere != null && Approximately(hull.collisionSphere.center, sphereCollider.center) && Approximately(hull.collisionSphere.radius, sphereCollider.radius);
					bool flag7 = hull.type == HullType.ConvexHull && collider3 is MeshCollider && meshCollider.sharedMesh == hull.collisionMesh;
					bool flag8 = hull.type == HullType.Face && collider3 is MeshCollider && meshCollider.sharedMesh == hull.faceCollisionMesh;
					if (flag5 || flag6 || flag7 || flag8)
					{
						hullMapping.Add(hull, collider3);
						list2.RemoveAt(num);
						list3.RemoveAt(num2);
						break;
					}
				}
			}
			foreach (Hull item2 in list2)
			{
				if (item2.type == HullType.Box)
				{
					BoxCollider value = base.gameObject.AddComponent<BoxCollider>();
					hullMapping.Add(item2, value);
				}
				else if (item2.type == HullType.Sphere)
				{
					SphereCollider value2 = base.gameObject.AddComponent<SphereCollider>();
					hullMapping.Add(item2, value2);
				}
				else if (item2.type == HullType.ConvexHull)
				{
					MeshCollider value3 = base.gameObject.AddComponent<MeshCollider>();
					hullMapping.Add(item2, value3);
				}
				else if (item2.type == HullType.Face)
				{
					MeshCollider value4 = base.gameObject.AddComponent<MeshCollider>();
					hullMapping.Add(item2, value4);
				}
			}
			foreach (Collider item3 in list3)
			{
				Object.DestroyImmediate(item3);
			}
		}

		private static bool Approximately(Vector3 lhs, Vector3 rhs)
		{
			return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y) && Mathf.Approximately(lhs.z, rhs.z);
		}

		private static bool Approximately(float lhs, float rhs)
		{
			return Mathf.Approximately(lhs, rhs);
		}

		private void CreateColliderComponent(Hull hull)
		{
			Collider collider = null;
			if (hull.type == HullType.Box)
			{
				BoxCollider boxCollider = hullMapping[hull] as BoxCollider;
				boxCollider.center = hull.collisionBox.center;
				boxCollider.size = hull.collisionBox.size;
				collider = boxCollider;
			}
			else if (hull.type == HullType.Sphere)
			{
				SphereCollider sphereCollider = hullMapping[hull] as SphereCollider;
				sphereCollider.center = hull.collisionSphere.center;
				sphereCollider.radius = hull.collisionSphere.radius;
				collider = sphereCollider;
			}
			else if (hull.type == HullType.ConvexHull)
			{
				MeshCollider meshCollider = hullMapping[hull] as MeshCollider;
				meshCollider.sharedMesh = hull.collisionMesh;
				meshCollider.convex = true;
				collider = meshCollider;
			}
			else if (hull.type == HullType.Face)
			{
				MeshCollider meshCollider2 = hullMapping[hull] as MeshCollider;
				meshCollider2.sharedMesh = hull.faceCollisionMesh;
				meshCollider2.convex = true;
				collider = meshCollider2;
			}
			collider.material = hull.material;
			collider.isTrigger = hull.isTrigger;
		}

		public void SetAllTypes(HullType newType)
		{
			foreach (Hull hull in paintingData.hulls)
			{
				hull.type = newType;
			}
		}

		public void SetAllMaterials(PhysicMaterial newMaterial)
		{
			foreach (Hull hull in paintingData.hulls)
			{
				hull.material = newMaterial;
			}
		}

		public void SetAllAsTrigger(bool isTrigger)
		{
			foreach (Hull hull in paintingData.hulls)
			{
				hull.isTrigger = isTrigger;
			}
		}

		public void OnDrawGizmosSelected()
		{
		}
	}
}
