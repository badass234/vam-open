using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
	public float Density = 700f;

	public int SlicesPerAxis = 2;

	public bool IsConcave;

	public int VoxelsLimit = 16;

	public float WaveVelocity = 0.05f;

	private const float Dampfer = 0.1f;

	private const float WaterDensity = 1000f;

	private float voxelHalfHeight;

	private float localArchimedesForce;

	private List<Vector3> voxels;

	private bool isMeshCollider;

	private List<Vector3[]> forces;

	private WaterRipples waterRipples;

	private void Start()
	{
		forces = new List<Vector3[]>();
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		if (GetComponent<Collider>() == null)
		{
			base.gameObject.AddComponent<MeshCollider>();
			Debug.LogWarning($"[Buoyancy.cs] Object \"{base.name}\" had no collider. MeshCollider has been added.");
		}
		isMeshCollider = GetComponent<MeshCollider>() != null;
		Bounds bounds = GetComponent<Collider>().bounds;
		if (bounds.size.x < bounds.size.y)
		{
			voxelHalfHeight = bounds.size.x;
		}
		else
		{
			voxelHalfHeight = bounds.size.y;
		}
		if (bounds.size.z < voxelHalfHeight)
		{
			voxelHalfHeight = bounds.size.z;
		}
		voxelHalfHeight /= 2 * SlicesPerAxis;
		if (GetComponent<Rigidbody>() == null)
		{
			base.gameObject.AddComponent<Rigidbody>();
			Debug.LogWarning($"[Buoyancy.cs] Object \"{base.name}\" had no Rigidbody. Rigidbody has been added.");
		}
		GetComponent<Rigidbody>().centerOfMass = new Vector3(0f, (0f - bounds.extents.y) * 0f, 0f) + base.transform.InverseTransformPoint(bounds.center);
		voxels = SliceIntoVoxels(isMeshCollider && IsConcave);
		base.transform.rotation = rotation;
		base.transform.position = position;
		float num = GetComponent<Rigidbody>().mass / Density;
		WeldPoints(voxels, VoxelsLimit);
		float num2 = 1000f * Mathf.Abs(Physics.gravity.y) * num;
		localArchimedesForce = num2 / (float)voxels.Count;
	}

	private List<Vector3> SliceIntoVoxels(bool concave)
	{
		List<Vector3> list = new List<Vector3>(SlicesPerAxis * SlicesPerAxis * SlicesPerAxis);
		if (concave)
		{
			MeshCollider component = GetComponent<MeshCollider>();
			bool convex = component.convex;
			component.convex = false;
			Bounds bounds = GetComponent<Collider>().bounds;
			for (int i = 0; i < SlicesPerAxis; i++)
			{
				for (int j = 0; j < SlicesPerAxis; j++)
				{
					for (int k = 0; k < SlicesPerAxis; k++)
					{
						float x = bounds.min.x + bounds.size.x / (float)SlicesPerAxis * (0.5f + (float)i);
						float y = bounds.min.y + bounds.size.y / (float)SlicesPerAxis * (0.5f + (float)j);
						float z = bounds.min.z + bounds.size.z / (float)SlicesPerAxis * (0.5f + (float)k);
						Vector3 vector = base.transform.InverseTransformPoint(new Vector3(x, y, z));
						if (PointIsInsideMeshCollider(component, vector))
						{
							list.Add(vector);
						}
					}
				}
			}
			if (list.Count == 0)
			{
				list.Add(bounds.center);
			}
			component.convex = convex;
		}
		else
		{
			Bounds bounds2 = GetComponent<Collider>().bounds;
			for (int l = 0; l < SlicesPerAxis; l++)
			{
				for (int m = 0; m < SlicesPerAxis; m++)
				{
					for (int n = 0; n < SlicesPerAxis; n++)
					{
						float x2 = bounds2.min.x + bounds2.size.x / (float)SlicesPerAxis * (0.5f + (float)l);
						float y2 = bounds2.min.y + bounds2.size.y / (float)SlicesPerAxis * (0.5f + (float)m);
						float z2 = bounds2.min.z + bounds2.size.z / (float)SlicesPerAxis * (0.5f + (float)n);
						Vector3 item = base.transform.InverseTransformPoint(new Vector3(x2, y2, z2));
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	private static bool PointIsInsideMeshCollider(Collider c, Vector3 p)
	{
		Vector3[] array = new Vector3[6]
		{
			Vector3.up,
			Vector3.down,
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back
		};
		Vector3[] array2 = array;
		foreach (Vector3 vector in array2)
		{
			RaycastHit hitInfo;
			if (!c.Raycast(new Ray(p - vector * 1000f, vector), out hitInfo, 1000f))
			{
				return false;
			}
		}
		return true;
	}

	private static void FindClosestPoints(IList<Vector3> list, out int firstIndex, out int secondIndex)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		firstIndex = 0;
		secondIndex = 1;
		for (int i = 0; i < list.Count - 1; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				float num3 = Vector3.Distance(list[i], list[j]);
				if (num3 < num)
				{
					num = num3;
					firstIndex = i;
					secondIndex = j;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
		}
	}

	private static void WeldPoints(IList<Vector3> list, int targetCount)
	{
		if (list.Count > 2 && targetCount >= 2)
		{
			while (list.Count > targetCount)
			{
				int firstIndex;
				int secondIndex;
				FindClosestPoints(list, out firstIndex, out secondIndex);
				Vector3 item = (list[firstIndex] + list[secondIndex]) * 0.5f;
				list.RemoveAt(secondIndex);
				list.RemoveAt(firstIndex);
				list.Add(item);
			}
		}
	}

	private Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 lhs = b - a;
		Vector3 rhs = c - a;
		return Vector3.Cross(lhs, rhs).normalized;
	}

	private void FixedUpdate()
	{
		if (waterRipples == null)
		{
			return;
		}
		forces.Clear();
		int count = voxels.Count;
		Vector3[] array = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			Vector3 position = base.transform.TransformPoint(voxels[i]);
			array[i] = waterRipples.GetOffsetByPosition(position);
		}
		Vector3 normalized = (GetNormal(array[0], array[1], array[2]) * WaveVelocity + Vector3.up).normalized;
		for (int j = 0; j < count; j++)
		{
			Vector3 vector = base.transform.TransformPoint(voxels[j]);
			float y = array[j].y;
			if (vector.y - voxelHalfHeight < y)
			{
				float num = (y - vector.y) / (2f * voxelHalfHeight) + 0.5f;
				if (num > 1f)
				{
					num = 1f;
				}
				else if (num < 0f)
				{
					num = 0f;
				}
				Vector3 pointVelocity = GetComponent<Rigidbody>().GetPointVelocity(vector);
				Vector3 vector2 = -pointVelocity * 0.1f * GetComponent<Rigidbody>().mass;
				Vector3 vector3 = vector2 + Mathf.Sqrt(num) * (normalized * localArchimedesForce);
				GetComponent<Rigidbody>().AddForceAtPosition(vector3, vector);
				forces.Add(new Vector3[2] { vector, vector3 });
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (voxels == null || forces == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		foreach (Vector3 voxel in voxels)
		{
			Gizmos.DrawCube(base.transform.TransformPoint(voxel), new Vector3(0.05f, 0.05f, 0.05f));
		}
		Gizmos.color = Color.cyan;
		foreach (Vector3[] force in forces)
		{
			Gizmos.DrawCube(force[0], new Vector3(0.05f, 0.05f, 0.05f));
			Gizmos.DrawLine(force[0], force[0] + force[1] / GetComponent<Rigidbody>().mass);
		}
	}

	private void OnTriggerEnter(Collider collidedObj)
	{
		WaterRipples component = collidedObj.GetComponent<WaterRipples>();
		if (component != null)
		{
			waterRipples = component;
		}
	}

	private void OnEnable()
	{
		waterRipples = null;
	}
}
