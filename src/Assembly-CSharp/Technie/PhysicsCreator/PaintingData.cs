using System;
using System.Collections.Generic;
using Technie.PhysicsCreator.QHull;
using UnityEngine;

namespace Technie.PhysicsCreator
{
	public class PaintingData : ScriptableObject
	{
		public readonly Color[] hullColours = new Color[13]
		{
			new Color(0f, 1f, 1f, 0.3f),
			new Color(1f, 0f, 1f, 0.3f),
			new Color(1f, 1f, 0f, 0.3f),
			new Color(1f, 0f, 0f, 0.3f),
			new Color(0f, 1f, 0f, 0.3f),
			new Color(0f, 0f, 1f, 0.3f),
			new Color(1f, 1f, 1f, 0.3f),
			new Color(1f, 0.5f, 0f, 0.3f),
			new Color(1f, 0f, 0.5f, 0.3f),
			new Color(0.5f, 1f, 0f, 0.3f),
			new Color(0f, 1f, 0.5f, 0.3f),
			new Color(0.5f, 0f, 1f, 0.3f),
			new Color(0f, 0.5f, 1f, 0.3f)
		};

		public HullData hullData;

		public Mesh sourceMesh;

		public int activeHull = -1;

		public float faceThickness = 0.1f;

		public List<Hull> hulls = new List<Hull>();

		public void AddHull(HullType type, PhysicMaterial material, bool isTrigger)
		{
			hulls.Add(new Hull());
			hulls[hulls.Count - 1].name = "Hull " + hulls.Count;
			activeHull = hulls.Count - 1;
			hulls[hulls.Count - 1].colour = hullColours[activeHull % hullColours.Length];
			hulls[hulls.Count - 1].type = type;
			hulls[hulls.Count - 1].material = material;
			hulls[hulls.Count - 1].isTrigger = isTrigger;
		}

		public void RemoveHull(int index)
		{
			hulls[index].Destroy();
			hulls.RemoveAt(index);
		}

		public void RemoveAllHulls()
		{
			for (int i = 0; i < hulls.Count; i++)
			{
				hulls[i].Destroy();
			}
			hulls.Clear();
		}

		public bool HasActiveHull()
		{
			return activeHull >= 0 && activeHull < hulls.Count;
		}

		public Hull GetActiveHull()
		{
			if (activeHull < 0 || activeHull >= hulls.Count)
			{
				return null;
			}
			return hulls[activeHull];
		}

		public void GenerateCollisionMesh(Hull hull, Vector3[] meshVertices, int[] meshIndices)
		{
			hull.hasColliderError = false;
			if (hull.type == HullType.Box)
			{
				if (hull.selectedFaces.Count > 0)
				{
					Vector3 vector = meshVertices[meshIndices[hull.selectedFaces[0] * 3]];
					Vector3 min = vector;
					Vector3 max = vector;
					for (int i = 0; i < hull.selectedFaces.Count; i++)
					{
						int num = hull.selectedFaces[i];
						Vector3 point = meshVertices[meshIndices[num * 3]];
						Vector3 point2 = meshVertices[meshIndices[num * 3 + 1]];
						Vector3 point3 = meshVertices[meshIndices[num * 3 + 2]];
						Inflate(point, ref min, ref max);
						Inflate(point2, ref min, ref max);
						Inflate(point3, ref min, ref max);
					}
					hull.collisionBox.center = (min + max) * 0.5f;
					hull.collisionBox.size = max - min;
				}
			}
			else if (hull.type == HullType.Sphere)
			{
				Vector3 sphereCenter;
				float sphereRadius;
				if (CalculateBoundingSphere(hull, meshVertices, meshIndices, out sphereCenter, out sphereRadius))
				{
					if (hull.collisionSphere == null)
					{
						hull.collisionSphere = new Sphere();
					}
					hull.collisionSphere.center = sphereCenter;
					hull.collisionSphere.radius = sphereRadius;
				}
			}
			else if (hull.type == HullType.ConvexHull)
			{
				if (hull.collisionMesh == null)
				{
					hull.collisionMesh = new Mesh();
				}
				hull.collisionMesh.name = hull.name;
				hull.collisionMesh.triangles = new int[0];
				hull.collisionMesh.vertices = new Vector3[0];
				GenerateConvexHull(hull, meshVertices, meshIndices, hull.collisionMesh);
			}
			else if (hull.type == HullType.Face)
			{
				if (hull.faceCollisionMesh == null)
				{
					hull.faceCollisionMesh = new Mesh();
				}
				hull.faceCollisionMesh.name = hull.name;
				hull.faceCollisionMesh.triangles = new int[0];
				hull.faceCollisionMesh.vertices = new Vector3[0];
				GenerateFace(hull, meshVertices, meshIndices, faceThickness);
			}
		}

		private bool CalculateBoundingSphere(Hull hull, Vector3[] meshVertices, int[] meshIndices, out Vector3 sphereCenter, out float sphereRadius)
		{
			if (hull.selectedFaces.Count == 0)
			{
				sphereCenter = Vector3.zero;
				sphereRadius = 0f;
				return false;
			}
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < hull.selectedFaces.Count; i++)
			{
				int num = hull.selectedFaces[i];
				Vector3 item = meshVertices[meshIndices[num * 3]];
				Vector3 item2 = meshVertices[meshIndices[num * 3 + 1]];
				Vector3 item3 = meshVertices[meshIndices[num * 3 + 2]];
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
			}
			Sphere sphere = SphereUtils.MinSphere(list);
			sphereCenter = sphere.center;
			sphereRadius = sphere.radius;
			return true;
		}

		private void GenerateConvexHull(Hull hull, Vector3[] meshVertices, int[] meshIndices, Mesh destMesh)
		{
			int count = hull.selectedFaces.Count;
			Point3d[] array = new Point3d[count * 3];
			for (int i = 0; i < hull.selectedFaces.Count; i++)
			{
				int num = hull.selectedFaces[i];
				Vector3 vector = meshVertices[meshIndices[num * 3]];
				Vector3 vector2 = meshVertices[meshIndices[num * 3 + 1]];
				Vector3 vector3 = meshVertices[meshIndices[num * 3 + 2]];
				array[i * 3] = new Point3d(vector.x, vector.y, vector.z);
				array[i * 3 + 1] = new Point3d(vector2.x, vector2.y, vector2.z);
				array[i * 3 + 2] = new Point3d(vector3.x, vector3.y, vector3.z);
			}
			QuickHull3D quickHull3D = new QuickHull3D();
			try
			{
				quickHull3D.build(array);
			}
			catch (Exception)
			{
				Debug.LogError("Could not generate hull for " + base.name + "'s '" + hull.name + "' (input " + array.Length + " points)");
			}
			Point3d[] vertices = quickHull3D.getVertices();
			int[][] faces = quickHull3D.getFaces();
			hull.numColliderFaces = faces.Length;
			Debug.Log("Calculated collider for '" + hull.name + "' has " + faces.Length + " faces");
			if (faces.Length >= 256)
			{
				hull.hasColliderError = true;
				return;
			}
			Vector3[] array2 = new Vector3[vertices.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = new Vector3((float)vertices[j].x, (float)vertices[j].y, (float)vertices[j].z);
			}
			List<int> list = new List<int>();
			for (int k = 0; k < faces.Length; k++)
			{
				int num2 = faces[k].Length;
				for (int l = 1; l < num2 - 1; l++)
				{
					list.Add(faces[k][0]);
					list.Add(faces[k][l]);
					list.Add(faces[k][l + 1]);
				}
			}
			int[] array3 = new int[list.Count];
			for (int m = 0; m < list.Count; m++)
			{
				array3[m] = list[m];
			}
			hull.collisionMesh.vertices = array2;
			hull.collisionMesh.triangles = array3;
			hull.collisionMesh.RecalculateBounds();
		}

		private void GenerateFace(Hull hull, Vector3[] meshVertices, int[] meshIndices, float thickness)
		{
			int count = hull.selectedFaces.Count;
			Vector3[] array = new Vector3[count * 3 * 2];
			for (int i = 0; i < hull.selectedFaces.Count; i++)
			{
				int num = hull.selectedFaces[i];
				Vector3 vector = meshVertices[meshIndices[num * 3]];
				Vector3 vector2 = meshVertices[meshIndices[num * 3 + 1]];
				Vector3 vector3 = meshVertices[meshIndices[num * 3 + 2]];
				Vector3 normalized = (vector2 - vector).normalized;
				Vector3 normalized2 = (vector3 - vector).normalized;
				Vector3 vector4 = Vector3.Cross(normalized2, normalized);
				int num2 = i * 3 * 2;
				array[num2] = vector;
				array[num2 + 1] = vector2;
				array[num2 + 2] = vector3;
				array[num2 + 3] = vector + vector4 * thickness;
				array[num2 + 4] = vector2 + vector4 * thickness;
				array[num2 + 5] = vector3 + vector4 * thickness;
			}
			int[] array2 = new int[count * 3 * 2];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = j;
			}
			hull.faceCollisionMesh.vertices = array;
			hull.faceCollisionMesh.triangles = array2;
			hull.faceCollisionMesh.RecalculateBounds();
		}

		public bool ContainsMesh(Mesh m)
		{
			foreach (Hull hull in hulls)
			{
				if (hull.collisionMesh == m)
				{
					return true;
				}
			}
			return false;
		}

		private static void Inflate(Vector3 point, ref Vector3 min, ref Vector3 max)
		{
			min.x = Mathf.Min(min.x, point.x);
			min.y = Mathf.Min(min.y, point.y);
			min.z = Mathf.Min(min.z, point.z);
			max.x = Mathf.Max(max.x, point.x);
			max.y = Mathf.Max(max.y, point.y);
			max.z = Mathf.Max(max.z, point.z);
		}
	}
}
