using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Tools
{
	public class BoneCollidersPlacer : MonoBehaviour
	{
		[SerializeField]
		public SkinnedMeshRenderer Skin;

		[SerializeField]
		public int Depth = 5;

		private Vector3[] vertices;

		[ContextMenu("Process")]
		public void Process()
		{
			Clear();
			Init();
			PlaceRecursive(base.transform, Depth);
		}

		[ContextMenu("Clear")]
		public void Clear()
		{
			LineSphereCollider[] componentsInChildren = base.gameObject.GetComponentsInChildren<LineSphereCollider>();
			LineSphereCollider[] array = componentsInChildren;
			foreach (LineSphereCollider obj in array)
			{
				Object.DestroyImmediate(obj);
			}
		}

		public void Fit()
		{
			Init();
			LineSphereCollider[] componentsInChildren = base.gameObject.GetComponentsInChildren<LineSphereCollider>();
			LineSphereCollider[] array = componentsInChildren;
			foreach (LineSphereCollider lineSphere in array)
			{
				for (int j = 0; j < 20; j++)
				{
					Rotate(lineSphere, 0.01f);
				}
			}
		}

		[ContextMenu("Grow")]
		public void Grow()
		{
			Init();
			LineSphereCollider[] componentsInChildren = base.gameObject.GetComponentsInChildren<LineSphereCollider>();
			LineSphereCollider[] array = componentsInChildren;
			foreach (LineSphereCollider lineSphereCollider in array)
			{
				lineSphereCollider.RadiusA += 0.01f;
				lineSphereCollider.RadiusB += 0.01f;
			}
		}

		private void Init()
		{
			Mesh mesh = new Mesh();
			Skin.BakeMesh(mesh);
			vertices = mesh.vertices;
		}

		private void PlaceRecursive(Transform bone, int depth)
		{
			depth--;
			if (depth != 0)
			{
				for (int i = 0; i < bone.childCount; i++)
				{
					Transform child = bone.GetChild(i);
					AddLineSpheres(bone, child);
					PlaceRecursive(child, depth);
				}
			}
		}

		private void AddLineSpheres(Transform bone, Transform child)
		{
			LineSphereCollider lineSphereCollider = bone.gameObject.AddComponent<LineSphereCollider>();
			lineSphereCollider.B = child.localPosition;
			lineSphereCollider.RadiusA = FindNearestMeshDistnce(Skin.transform.InverseTransformPoint(lineSphereCollider.WorldA));
			lineSphereCollider.RadiusB = FindNearestMeshDistnce(Skin.transform.InverseTransformPoint(lineSphereCollider.WorldB));
		}

		private float FindNearestMeshDistnce(Vector3 point)
		{
			float num = (vertices[0] - point).sqrMagnitude;
			for (int i = 1; i < vertices.Length; i++)
			{
				Vector3 vector = vertices[i];
				float sqrMagnitude = (vector - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
				}
			}
			return Mathf.Sqrt(num);
		}

		private void Rotate(LineSphereCollider lineSphere, float step)
		{
			for (int i = 0; i < 50; i++)
			{
				Vector3 vector = lineSphere.WorldA + RandomVector() * step;
				float num = FindNearestMeshDistnce(Skin.transform.InverseTransformPoint(vector));
				if (num > lineSphere.WorldRadiusA)
				{
					lineSphere.WorldRadiusA = num;
					lineSphere.WorldA = vector;
					break;
				}
			}
			for (int j = 0; j < 50; j++)
			{
				Vector3 vector2 = lineSphere.WorldB + RandomVector() * step;
				float num2 = FindNearestMeshDistnce(Skin.transform.InverseTransformPoint(vector2));
				if (num2 > lineSphere.WorldRadiusB)
				{
					lineSphere.WorldRadiusA = num2;
					lineSphere.WorldA = vector2;
					break;
				}
			}
		}

		private Vector3 RandomVector()
		{
			return new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
		}
	}
}
