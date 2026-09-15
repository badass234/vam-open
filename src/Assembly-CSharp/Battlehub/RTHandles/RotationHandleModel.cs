using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RotationHandleModel : BaseHandleModel
	{
		private const float DefaultMinorRadius = 0.05f;

		private const float DefaultMajorRadius = 1f;

		private const float DefaultOuterRadius = 1.11f;

		[SerializeField]
		private float m_minorRadius = 0.05f;

		[SerializeField]
		private float m_majorRadius = 1f;

		[SerializeField]
		private float m_outerRadius = 1.11f;

		[SerializeField]
		private MeshFilter m_xyz;

		[SerializeField]
		private MeshFilter m_innerCircle;

		[SerializeField]
		private MeshFilter m_outerCircle;

		private Mesh m_xyzMesh;

		private Mesh m_innerCircleMesh;

		private Mesh m_outerCircleMesh;

		[SerializeField]
		private int m_xMatIndex;

		[SerializeField]
		private int m_yMatIndex = 1;

		[SerializeField]
		private int m_zMatIndex = 2;

		[SerializeField]
		private int m_innerCircleBorderMatIndex;

		[SerializeField]
		private int m_innerCircleFillMatIndex = 1;

		private Material[] m_xyzMaterials;

		private Material[] m_innerCircleMaterials;

		private Material m_outerCircleMaterial;

		private float m_prevMinorRadius = 0.05f;

		private float m_prevMajorRadius = 1f;

		private float m_prevOuterRadius = 1.11f;

		protected override void Awake()
		{
			base.Awake();
			m_xyzMesh = m_xyz.sharedMesh;
			m_innerCircleMesh = m_innerCircle.sharedMesh;
			m_outerCircleMesh = m_outerCircle.sharedMesh;
			Renderer component = m_xyz.GetComponent<Renderer>();
			component.sharedMaterials = component.materials;
			m_xyzMaterials = component.sharedMaterials;
			component = m_innerCircle.GetComponent<Renderer>();
			component.sharedMaterials = component.materials;
			m_innerCircleMaterials = component.sharedMaterials;
			component = m_outerCircle.GetComponent<Renderer>();
			component.sharedMaterials = component.materials;
			m_outerCircleMaterial = component.sharedMaterial;
			Mesh mesh = m_xyz.mesh;
			m_xyz.sharedMesh = mesh;
			mesh = m_innerCircle.mesh;
			m_innerCircle.sharedMesh = mesh;
			mesh = m_outerCircle.mesh;
			m_outerCircle.sharedMesh = mesh;
		}

		protected override void Start()
		{
			base.Start();
			UpdateXYZ(m_xyz.sharedMesh, m_majorRadius, m_minorRadius);
			UpdateCircle(m_innerCircle.sharedMesh, m_innerCircleMesh, m_innerCircle.transform, m_majorRadius, m_minorRadius);
			UpdateCircle(m_outerCircle.sharedMesh, m_outerCircleMesh, m_outerCircle.transform, m_outerRadius, m_minorRadius);
			SetColors();
		}

		public override void Select(RuntimeHandleAxis axis)
		{
			base.Select(axis);
			SetColors();
		}

		public override void SetLock(LockObject lockObj)
		{
			base.SetLock(lockObj);
			SetColors();
		}

		private void SetDefaultColors()
		{
			if (m_lockObj.RotationX)
			{
				m_xyzMaterials[m_xMatIndex].color = m_disabledColor;
			}
			else
			{
				m_xyzMaterials[m_xMatIndex].color = m_xColor;
			}
			if (m_lockObj.RotationY)
			{
				m_xyzMaterials[m_yMatIndex].color = m_disabledColor;
			}
			else
			{
				m_xyzMaterials[m_yMatIndex].color = m_yColor;
			}
			if (m_lockObj.RotationZ)
			{
				m_xyzMaterials[m_zMatIndex].color = m_disabledColor;
			}
			else
			{
				m_xyzMaterials[m_zMatIndex].color = m_zColor;
			}
			if (m_lockObj.RotationScreen)
			{
				m_outerCircleMaterial.color = m_disabledColor;
			}
			else
			{
				m_outerCircleMaterial.color = m_altColor;
			}
			m_outerCircleMaterial.SetInt("_ZTest", 2);
			if (m_lockObj.IsPositionLocked)
			{
				m_innerCircleMaterials[m_innerCircleBorderMatIndex].color = m_disabledColor;
			}
			else
			{
				m_innerCircleMaterials[m_innerCircleBorderMatIndex].color = m_altColor2;
			}
			m_innerCircleMaterials[m_innerCircleFillMatIndex].color = new Color(0f, 0f, 0f, 0f);
		}

		private void SetColors()
		{
			SetDefaultColors();
			switch (m_selectedAxis)
			{
			case RuntimeHandleAxis.X:
				if (!m_lockObj.PositionX)
				{
					m_xyzMaterials[m_xMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Y:
				if (!m_lockObj.PositionY)
				{
					m_xyzMaterials[m_yMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Z:
				if (!m_lockObj.PositionZ)
				{
					m_xyzMaterials[m_zMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Free:
				if (!m_lockObj.IsPositionLocked)
				{
					m_innerCircleMaterials[m_innerCircleFillMatIndex].color = new Color(0f, 0f, 0f, 0.1f);
				}
				break;
			case RuntimeHandleAxis.Screen:
				if (!m_lockObj.RotationScreen)
				{
					m_outerCircleMaterial.color = m_selectionColor;
					m_outerCircleMaterial.SetInt("_ZTest", 0);
				}
				break;
			}
		}

		private void UpdateXYZ(Mesh mesh, float majorRadius, float minorRadius)
		{
			m_xyz.transform.localScale = Vector3.one * majorRadius;
			minorRadius /= Mathf.Max(0.01f, majorRadius);
			Vector3[] vertices = m_xyzMesh.vertices;
			for (int i = 0; i < m_xyzMesh.subMeshCount; i++)
			{
				int[] triangles = mesh.GetTriangles(i);
				foreach (int num in triangles)
				{
					Vector3 vector = vertices[num];
					Vector3 vector2 = vector;
					switch (i)
					{
					case 0:
						vector2.x = 0f;
						break;
					case 1:
						vector2.y = 0f;
						break;
					case 2:
						vector2.z = 0f;
						break;
					}
					vector2.Normalize();
					vertices[num] = vector2 + (vector - vector2).normalized * minorRadius;
				}
			}
			mesh.vertices = vertices;
		}

		private void UpdateCircle(Mesh mesh, Mesh originalMesh, Transform circleTransform, float majorRadius, float minorRadius)
		{
			circleTransform.localScale = Vector3.one * majorRadius;
			minorRadius /= Mathf.Max(0.01f, majorRadius);
			Vector3[] vertices = originalMesh.vertices;
			int[] triangles = mesh.GetTriangles(0);
			foreach (int num in triangles)
			{
				Vector3 vector = vertices[num];
				Vector3 vector2 = vector;
				vector2.z = 0f;
				vector2.Normalize();
				vertices[num] = vector2 + (vector - vector2).normalized * minorRadius;
			}
			triangles = mesh.GetTriangles(1);
			foreach (int num2 in triangles)
			{
				Vector3 vector3 = vertices[num2];
				vector3.Normalize();
				vertices[num2] = vector3 * (1f - minorRadius);
			}
			mesh.vertices = vertices;
		}

		protected override void Update()
		{
			if (m_prevMinorRadius != m_minorRadius || m_prevMajorRadius != m_majorRadius || m_prevOuterRadius != m_outerRadius)
			{
				m_prevMinorRadius = m_minorRadius;
				m_prevMajorRadius = m_majorRadius;
				m_prevOuterRadius = m_outerRadius;
				UpdateXYZ(m_xyz.sharedMesh, m_majorRadius, m_minorRadius);
				UpdateCircle(m_innerCircle.sharedMesh, m_innerCircleMesh, m_innerCircle.transform, m_majorRadius, m_minorRadius);
				UpdateCircle(m_outerCircle.sharedMesh, m_outerCircleMesh, m_outerCircle.transform, m_outerRadius, m_minorRadius);
			}
		}
	}
}
