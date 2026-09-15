using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class PositionHandleModel : BaseHandleModel
	{
		[SerializeField]
		private Transform m_camera;

		[SerializeField]
		private GameObject[] m_models;

		[SerializeField]
		private GameObject m_screenSpaceQuad;

		[SerializeField]
		private GameObject m_normalModeArrows;

		[SerializeField]
		private GameObject m_vertexSnappingModeArrows;

		[SerializeField]
		private Transform[] m_armatures;

		[SerializeField]
		private Transform m_ssQuadArmature;

		[SerializeField]
		private int m_xMatIndex;

		[SerializeField]
		private int m_yMatIndex = 1;

		[SerializeField]
		private int m_zMatIndex = 2;

		[SerializeField]
		private int m_xArrowMatIndex = 3;

		[SerializeField]
		private int m_yArrowMatIndex = 4;

		[SerializeField]
		private int m_zArrowMatIndex = 5;

		[SerializeField]
		private int m_xQMatIndex = 6;

		[SerializeField]
		private int m_yQMatIndex = 7;

		[SerializeField]
		private int m_zQMatIndex = 8;

		[SerializeField]
		private int m_xQuadMatIndex = 9;

		[SerializeField]
		private int m_yQuadMatIndex = 10;

		[SerializeField]
		private int m_zQuadMatIndex = 11;

		[SerializeField]
		private float m_quadTransparency = 0.5f;

		[SerializeField]
		private float m_radius = 0.05f;

		[SerializeField]
		private float m_length = 1f;

		[SerializeField]
		private float m_arrowRadius = 0.1f;

		[SerializeField]
		private float m_arrowLength = 0.2f;

		[SerializeField]
		private float m_quadLength = 0.2f;

		[SerializeField]
		private bool m_isVertexSnapping;

		private Material[] m_materials;

		private Material m_ssQuadMaterial;

		private Transform[] m_b0;

		private Transform[] m_b1x;

		private Transform[] m_b2x;

		private Transform[] m_b3x;

		private Transform[] m_bSx;

		private Transform[] m_b1y;

		private Transform[] m_b2y;

		private Transform[] m_b3y;

		private Transform[] m_bSy;

		private Transform[] m_b1z;

		private Transform[] m_b2z;

		private Transform[] m_b3z;

		private Transform[] m_bSz;

		private Transform m_b1ss;

		private Transform m_b2ss;

		private Transform m_b3ss;

		private Transform m_b4ss;

		private Vector3[] m_defaultArmaturesScale;

		private Vector3[] m_defaultB3XScale;

		private Vector3[] m_defaultB3YScale;

		private Vector3[] m_defaultB3ZScale;

		private const float DefaultRadius = 0.05f;

		private const float DefaultLength = 1f;

		private const float DefaultArrowRadius = 0.1f;

		private const float DefaultArrowLength = 0.2f;

		private const float DefaultQuadLength = 0.2f;

		public bool IsVertexSnapping
		{
			get
			{
				return m_isVertexSnapping;
			}
			set
			{
				if (m_isVertexSnapping != value)
				{
					m_isVertexSnapping = value;
					OnVertexSnappingModeChaged();
					SetColors();
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (m_camera == null)
			{
				OnActiveSceneCameraChanged();
			}
			RuntimeEditorApplication.ActiveSceneCameraChanged += OnActiveSceneCameraChanged;
			m_defaultArmaturesScale = new Vector3[m_armatures.Length];
			m_defaultB3XScale = new Vector3[m_armatures.Length];
			m_defaultB3YScale = new Vector3[m_armatures.Length];
			m_defaultB3ZScale = new Vector3[m_armatures.Length];
			m_b1x = new Transform[m_armatures.Length];
			m_b1y = new Transform[m_armatures.Length];
			m_b1z = new Transform[m_armatures.Length];
			m_b2x = new Transform[m_armatures.Length];
			m_b2y = new Transform[m_armatures.Length];
			m_b2z = new Transform[m_armatures.Length];
			m_b3x = new Transform[m_armatures.Length];
			m_b3y = new Transform[m_armatures.Length];
			m_b3z = new Transform[m_armatures.Length];
			m_b0 = new Transform[m_armatures.Length];
			m_bSx = new Transform[m_armatures.Length];
			m_bSy = new Transform[m_armatures.Length];
			m_bSz = new Transform[m_armatures.Length];
			for (int i = 0; i < m_armatures.Length; i++)
			{
				m_b1x[i] = m_armatures[i].GetChild(0);
				m_b1y[i] = m_armatures[i].GetChild(1);
				m_b1z[i] = m_armatures[i].GetChild(2);
				m_b2x[i] = m_armatures[i].GetChild(3);
				m_b2y[i] = m_armatures[i].GetChild(4);
				m_b2z[i] = m_armatures[i].GetChild(5);
				m_b3x[i] = m_armatures[i].GetChild(6);
				m_b3y[i] = m_armatures[i].GetChild(7);
				m_b3z[i] = m_armatures[i].GetChild(8);
				m_b0[i] = m_armatures[i].GetChild(9);
				m_bSx[i] = m_armatures[i].GetChild(10);
				m_bSy[i] = m_armatures[i].GetChild(11);
				m_bSz[i] = m_armatures[i].GetChild(12);
				m_defaultArmaturesScale[i] = m_armatures[i].localScale;
				m_defaultB3XScale[i] = base.transform.TransformVector(m_b3x[i].localScale);
				m_defaultB3YScale[i] = base.transform.TransformVector(m_b3y[i].localScale);
				m_defaultB3ZScale[i] = base.transform.TransformVector(m_b3z[i].localScale);
			}
			m_b1ss = m_ssQuadArmature.GetChild(1);
			m_b2ss = m_ssQuadArmature.GetChild(2);
			m_b3ss = m_ssQuadArmature.GetChild(3);
			m_b4ss = m_ssQuadArmature.GetChild(4);
			m_materials = m_models[0].GetComponent<Renderer>().materials;
			m_ssQuadMaterial = m_screenSpaceQuad.GetComponent<Renderer>().sharedMaterial;
			SetDefaultColors();
			for (int j = 0; j < m_models.Length; j++)
			{
				Renderer component = m_models[j].GetComponent<Renderer>();
				component.sharedMaterials = m_materials;
			}
			OnVertexSnappingModeChaged();
		}

		protected override void Start()
		{
			base.Start();
			UpdateTransforms();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			RuntimeEditorApplication.ActiveSceneCameraChanged -= OnActiveSceneCameraChanged;
		}

		private void OnActiveSceneCameraChanged()
		{
			if (RuntimeEditorApplication.ActiveSceneCamera != null)
			{
				m_camera = RuntimeEditorApplication.ActiveSceneCamera.transform;
			}
			else if (Camera.main != null)
			{
				m_camera = Camera.main.transform;
			}
		}

		public override void SetLock(LockObject lockObj)
		{
			base.SetLock(m_lockObj);
			OnVertexSnappingModeChaged();
			SetColors();
		}

		public override void Select(RuntimeHandleAxis axis)
		{
			base.Select(axis);
			OnVertexSnappingModeChaged();
			SetColors();
		}

		private void OnVertexSnappingModeChaged()
		{
			m_normalModeArrows.SetActive(!m_isVertexSnapping && !m_lockObj.IsPositionLocked);
			m_vertexSnappingModeArrows.SetActive(m_isVertexSnapping && !m_lockObj.IsPositionLocked);
		}

		private void SetDefaultColors()
		{
			if (m_lockObj.PositionX)
			{
				m_materials[m_xMatIndex].color = m_disabledColor;
				m_materials[m_xArrowMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_xMatIndex].color = m_xColor;
				m_materials[m_xArrowMatIndex].color = m_xColor;
			}
			if (m_lockObj.PositionY)
			{
				m_materials[m_yMatIndex].color = m_disabledColor;
				m_materials[m_yArrowMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_yMatIndex].color = m_yColor;
				m_materials[m_yArrowMatIndex].color = m_yColor;
			}
			if (m_lockObj.PositionZ)
			{
				m_materials[m_zMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_zMatIndex].color = m_zColor;
				m_materials[m_zArrowMatIndex].color = m_zColor;
			}
			m_materials[m_xQMatIndex].color = ((!m_lockObj.PositionY && !m_lockObj.PositionZ) ? m_xColor : m_disabledColor);
			m_materials[m_yQMatIndex].color = ((!m_lockObj.PositionX && !m_lockObj.PositionZ) ? m_yColor : m_disabledColor);
			m_materials[m_zQMatIndex].color = ((!m_lockObj.PositionX && !m_lockObj.PositionY) ? m_zColor : m_disabledColor);
			Color xColor = m_xColor;
			xColor.a = m_quadTransparency;
			m_materials[m_xQuadMatIndex].color = xColor;
			Color yColor = m_yColor;
			yColor.a = m_quadTransparency;
			m_materials[m_yQuadMatIndex].color = yColor;
			Color zColor = m_zColor;
			zColor.a = m_quadTransparency;
			m_materials[m_zQuadMatIndex].color = zColor;
			m_ssQuadMaterial.color = m_altColor;
		}

		private void SetColors()
		{
			SetDefaultColors();
			switch (m_selectedAxis)
			{
			case RuntimeHandleAxis.XY:
				if (!m_lockObj.PositionX && !m_lockObj.PositionY)
				{
					m_materials[m_xArrowMatIndex].color = m_selectionColor;
					m_materials[m_yArrowMatIndex].color = m_selectionColor;
					m_materials[m_xMatIndex].color = m_selectionColor;
					m_materials[m_yMatIndex].color = m_selectionColor;
					m_materials[m_zQMatIndex].color = m_selectionColor;
					m_materials[m_zQuadMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.YZ:
				if (!m_lockObj.PositionY && !m_lockObj.PositionZ)
				{
					m_materials[m_yArrowMatIndex].color = m_selectionColor;
					m_materials[m_zArrowMatIndex].color = m_selectionColor;
					m_materials[m_yMatIndex].color = m_selectionColor;
					m_materials[m_zMatIndex].color = m_selectionColor;
					m_materials[m_xQMatIndex].color = m_selectionColor;
					m_materials[m_xQuadMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.XZ:
				if (!m_lockObj.PositionX && !m_lockObj.PositionZ)
				{
					m_materials[m_xArrowMatIndex].color = m_selectionColor;
					m_materials[m_zArrowMatIndex].color = m_selectionColor;
					m_materials[m_xMatIndex].color = m_selectionColor;
					m_materials[m_zMatIndex].color = m_selectionColor;
					m_materials[m_yQMatIndex].color = m_selectionColor;
					m_materials[m_yQuadMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.X:
				if (!m_lockObj.PositionX)
				{
					m_materials[m_xArrowMatIndex].color = m_selectionColor;
					m_materials[m_xMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Y:
				if (!m_lockObj.PositionY)
				{
					m_materials[m_yArrowMatIndex].color = m_selectionColor;
					m_materials[m_yMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Z:
				if (!m_lockObj.PositionZ)
				{
					m_materials[m_zArrowMatIndex].color = m_selectionColor;
					m_materials[m_zMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Snap:
				m_ssQuadMaterial.color = m_selectionColor;
				break;
			}
		}

		private void UpdateTransforms()
		{
			m_quadLength = Mathf.Abs(m_quadLength);
			m_radius = Mathf.Max(0.01f, m_radius);
			Vector3 vector = base.transform.rotation * Vector3.right * base.transform.localScale.x;
			Vector3 vector2 = base.transform.rotation * Vector3.up * base.transform.localScale.y;
			Vector3 vector3 = base.transform.rotation * Vector3.forward * base.transform.localScale.z;
			Vector3 position = base.transform.position;
			float num = m_radius / 0.05f;
			float num2 = m_arrowLength / 0.2f / num;
			float num3 = m_arrowRadius / 0.1f / num;
			for (int i = 0; i < m_models.Length; i++)
			{
				m_armatures[i].localScale = m_defaultArmaturesScale[i] * num;
				m_ssQuadArmature.localScale = Vector3.one * num;
				m_b3x[i].position = position + vector * m_length;
				m_b3y[i].position = position + vector2 * m_length;
				m_b3z[i].position = position + vector3 * m_length;
				m_b2x[i].position = position + vector * (m_length - m_arrowLength);
				m_b2y[i].position = position + vector2 * (m_length - m_arrowLength);
				m_b2z[i].position = position + vector3 * (m_length - m_arrowLength);
				m_b3x[i].localScale = Vector3.right * num2 + new Vector3(0f, 1f, 1f) * num3;
				m_b3y[i].localScale = Vector3.forward * num2 + new Vector3(1f, 1f, 0f) * num3;
				m_b3z[i].localScale = Vector3.up * num2 + new Vector3(1f, 0f, 1f) * num3;
				m_b1x[i].position = position + Mathf.Sign(Vector3.Dot(vector, m_b1x[i].position - position)) * vector * m_quadLength;
				m_b1y[i].position = position + Mathf.Sign(Vector3.Dot(vector2, m_b1y[i].position - position)) * vector2 * m_quadLength;
				m_b1z[i].position = position + Mathf.Sign(Vector3.Dot(vector3, m_b1z[i].position - position)) * vector3 * m_quadLength;
				m_bSx[i].position = position + (m_b1y[i].position - position) + (m_b1z[i].position - position);
				m_bSy[i].position = position + (m_b1x[i].position - position) + (m_b1z[i].position - position);
				m_bSz[i].position = position + (m_b1x[i].position - position) + (m_b1y[i].position - position);
			}
			m_b1ss.position = position + base.transform.rotation * new Vector3(1f, 1f, 0f) * m_quadLength;
			m_b2ss.position = position + base.transform.rotation * new Vector3(-1f, -1f, 0f) * m_quadLength;
			m_b3ss.position = position + base.transform.rotation * new Vector3(-1f, 1f, 0f) * m_quadLength;
			m_b4ss.position = position + base.transform.rotation * new Vector3(1f, -1f, 0f) * m_quadLength;
		}

		public void SetCameraPosition(Vector3 pos)
		{
			Vector3 normalized = (pos - base.transform.position).normalized;
			normalized = base.transform.InverseTransformDirection(normalized);
			float[] array = new float[8]
			{
				Vector3.Dot(new Vector3(1f, 1f, 1f).normalized, normalized),
				Vector3.Dot(new Vector3(-1f, 1f, 1f).normalized, normalized),
				Vector3.Dot(new Vector3(-1f, -1f, 1f).normalized, normalized),
				Vector3.Dot(new Vector3(1f, -1f, 1f).normalized, normalized),
				Vector3.Dot(new Vector3(1f, 1f, -1f).normalized, normalized),
				Vector3.Dot(new Vector3(-1f, 1f, -1f).normalized, normalized),
				Vector3.Dot(new Vector3(-1f, -1f, -1f).normalized, normalized),
				Vector3.Dot(new Vector3(1f, -1f, -1f).normalized, normalized)
			};
			float num = float.MinValue;
			int num2 = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > num)
				{
					num = array[i];
					num2 = i;
				}
			}
			for (int j = 0; j < m_models.Length - 1; j++)
			{
				if (j != num2)
				{
					m_models[j].SetActive(false);
				}
			}
			if (num2 >= 0)
			{
				m_models[num2].SetActive(true);
			}
		}
	}
}
