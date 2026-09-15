using System;
using UnityEngine;

[AddComponentMenu("AQUAS/Buoyancy")]
[RequireComponent(typeof(Rigidbody))]
public class AQUAS_Buoyancy : MonoBehaviour
{
	public enum debugModes
	{
		none,
		showAffectedFaces,
		showForceRepresentation,
		showReferenceVolume
	}

	public float waterLevel;

	public float waterDensity;

	[Space(5f)]
	public bool useBalanceFactor;

	public Vector3 balanceFactor;

	[Space(20f)]
	[Range(0f, 1f)]
	public float dynamicSurface = 0.3f;

	[Range(1f, 10f)]
	public float bounceFrequency = 3f;

	[Space(5f)]
	[Header("Debugging can be ver performance heavy!")]
	public debugModes debug;

	private Vector3[] vertices;

	private int[] triangles;

	private Mesh mesh;

	private Rigidbody rb;

	private float effWaterDensity;

	private float regWaterDensity;

	private float maxWaterDensity;

	private void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		triangles = mesh.triangles;
		rb = GetComponent<Rigidbody>();
		regWaterDensity = waterDensity;
		maxWaterDensity = regWaterDensity + regWaterDensity * 0.5f * dynamicSurface;
	}

	private void FixedUpdate()
	{
		if (balanceFactor.x < 0.001f)
		{
			balanceFactor.x = 0.001f;
		}
		if (balanceFactor.y < 0.001f)
		{
			balanceFactor.y = 0.001f;
		}
		if (balanceFactor.z < 0.001f)
		{
			balanceFactor.z = 0.001f;
		}
		AddForce();
	}

	private void Update()
	{
		regWaterDensity = waterDensity;
		maxWaterDensity = regWaterDensity + regWaterDensity * 0.5f * dynamicSurface;
		effWaterDensity = (maxWaterDensity - regWaterDensity) / 2f + regWaterDensity + Mathf.Sin(Time.time * bounceFrequency) * (maxWaterDensity - regWaterDensity) / 2f;
	}

	private void AddForce()
	{
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 vector = vertices[triangles[i]];
			Vector3 vector2 = vertices[triangles[i + 1]];
			Vector3 vector3 = vertices[triangles[i + 2]];
			float num = waterLevel - Center(vector, vector2, vector3).y;
			if (num > 0f && Center(vector, vector2, vector3).y > (Center(vector, vector2, vector3) + Normal(vector, vector2, vector3)).y)
			{
				float y = effWaterDensity * Physics.gravity.y * num * Area(vector, vector2, vector3) * Normal(vector, vector2, vector3).normalized.y;
				if (useBalanceFactor)
				{
					rb.AddForceAtPosition(new Vector3(0f, y, 0f), base.transform.TransformPoint(new Vector3(base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).x / (balanceFactor.x * base.transform.localScale.x * 1000f), base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).y / (balanceFactor.y * base.transform.localScale.x * 1000f), base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).z / (balanceFactor.z * base.transform.localScale.x * 1000f))));
				}
				else
				{
					rb.AddForceAtPosition(new Vector3(0f, y, 0f), base.transform.TransformPoint(new Vector3(base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).x, base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).y, base.transform.InverseTransformPoint(Center(vector, vector2, vector3)).z)));
				}
				if (debug == debugModes.showAffectedFaces)
				{
					Debug.DrawLine(Center(vector, vector2, vector3), Center(vector, vector2, vector3) + Normal(vector, vector2, vector3), Color.white);
				}
				if (debug == debugModes.showForceRepresentation)
				{
					Debug.DrawRay(Center(vector, vector2, vector3), new Vector3(0f, y, 0f), Color.red);
				}
				if (debug == debugModes.showReferenceVolume)
				{
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector).z), new Vector3(base.transform.TransformPoint(vector2).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector2).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector2).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector2).z), new Vector3(base.transform.TransformPoint(vector3).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector3).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector3).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector3).z), new Vector3(base.transform.TransformPoint(vector).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector).x, waterLevel, base.transform.TransformPoint(vector).z), new Vector3(base.transform.TransformPoint(vector2).x, waterLevel, base.transform.TransformPoint(vector2).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector2).x, waterLevel, base.transform.TransformPoint(vector2).z), new Vector3(base.transform.TransformPoint(vector3).x, waterLevel, base.transform.TransformPoint(vector3).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector3).x, waterLevel, base.transform.TransformPoint(vector3).z), new Vector3(base.transform.TransformPoint(vector).x, waterLevel, base.transform.TransformPoint(vector).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector).x, waterLevel, base.transform.TransformPoint(vector).z), new Vector3(base.transform.TransformPoint(vector).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector2).x, waterLevel, base.transform.TransformPoint(vector2).z), new Vector3(base.transform.TransformPoint(vector2).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector2).z), Color.green);
					Debug.DrawLine(new Vector3(base.transform.TransformPoint(vector3).x, waterLevel, base.transform.TransformPoint(vector3).z), new Vector3(base.transform.TransformPoint(vector3).x, Center(vector, vector2, vector3).y, base.transform.TransformPoint(vector3).z), Color.green);
				}
			}
		}
	}

	private Vector3 Center(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 position = (p1 + p2 + p3) / 3f;
		return base.transform.TransformPoint(position);
	}

	private Vector3 Normal(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return Vector3.Cross(base.transform.TransformPoint(p2) - base.transform.TransformPoint(p1), base.transform.TransformPoint(p3) - base.transform.TransformPoint(p1)).normalized;
	}

	private float Area(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = Vector3.Distance(new Vector3(base.transform.TransformPoint(p1).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p1).z), new Vector3(base.transform.TransformPoint(p2).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p2).z));
		float num2 = Vector3.Distance(new Vector3(base.transform.TransformPoint(p3).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p3).z), new Vector3(base.transform.TransformPoint(p1).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p1).z));
		return num * num2 * Mathf.Sin(Vector3.Angle(new Vector3(base.transform.TransformPoint(p2).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p2).z) - new Vector3(base.transform.TransformPoint(p1).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p1).z), new Vector3(base.transform.TransformPoint(p3).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p3).z) - new Vector3(base.transform.TransformPoint(p1).x, Center(p1, p2, p3).y, base.transform.TransformPoint(p1).z)) * ((float)Math.PI / 180f)) / 2f;
	}
}
