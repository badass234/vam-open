using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class ScaleHandleModel : BaseHandleModel
	{
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
		private int m_xyzMatIndex = 6;

		[SerializeField]
		private Transform m_armature;

		[SerializeField]
		private Transform m_model;

		private Transform m_b1x;

		private Transform m_b2x;

		private Transform m_b3x;

		private Transform m_b1y;

		private Transform m_b2y;

		private Transform m_b3y;

		private Transform m_b1z;

		private Transform m_b2z;

		private Transform m_b3z;

		private Transform m_b0;

		[SerializeField]
		private float m_radius = 0.05f;

		[SerializeField]
		private float m_length = 1f;

		[SerializeField]
		private float m_arrowRadius = 0.1f;

		private const float DefaultRadius = 0.05f;

		private const float DefaultLength = 1f;

		private const float DefaultArrowRadius = 0.1f;

		private Material[] m_materials;

		private Vector3 m_scale = Vector3.one;

		private float m_prevRadius;

		private float m_prevLength;

		private float m_prevArrowRadius;

		protected override void Awake()
		{
			base.Awake();
			m_b1x = m_armature.GetChild(0);
			m_b1y = m_armature.GetChild(1);
			m_b1z = m_armature.GetChild(2);
			m_b2x = m_armature.GetChild(3);
			m_b2y = m_armature.GetChild(4);
			m_b2z = m_armature.GetChild(5);
			m_b3x = m_armature.GetChild(6);
			m_b3y = m_armature.GetChild(7);
			m_b3z = m_armature.GetChild(8);
			m_b0 = m_armature.GetChild(9);
			Renderer component = m_model.GetComponent<Renderer>();
			m_materials = component.materials;
			component.sharedMaterials = m_materials;
		}

		protected override void Start()
		{
			base.Start();
			UpdateTransforms();
			SetColors();
		}

		public override void SetLock(LockObject lockObj)
		{
			base.SetLock(m_lockObj);
			SetColors();
		}

		public override void Select(RuntimeHandleAxis axis)
		{
			base.Select(axis);
			SetColors();
		}

		private void SetDefaultColors()
		{
			if (m_lockObj.ScaleX)
			{
				m_materials[m_xMatIndex].color = m_disabledColor;
				m_materials[m_xArrowMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_xMatIndex].color = m_xColor;
				m_materials[m_xArrowMatIndex].color = m_xColor;
			}
			if (m_lockObj.ScaleY)
			{
				m_materials[m_yMatIndex].color = m_disabledColor;
				m_materials[m_yArrowMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_yMatIndex].color = m_yColor;
				m_materials[m_yArrowMatIndex].color = m_yColor;
			}
			if (m_lockObj.ScaleZ)
			{
				m_materials[m_zMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_zMatIndex].color = m_zColor;
				m_materials[m_zArrowMatIndex].color = m_zColor;
			}
			if (m_lockObj.IsPositionLocked)
			{
				m_materials[m_xyzMatIndex].color = m_disabledColor;
			}
			else
			{
				m_materials[m_xyzMatIndex].color = m_altColor;
			}
		}

		private void SetColors()
		{
			SetDefaultColors();
			switch (m_selectedAxis)
			{
			case RuntimeHandleAxis.X:
				if (!m_lockObj.ScaleX)
				{
					m_materials[m_xArrowMatIndex].color = m_selectionColor;
					m_materials[m_xMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Y:
				if (!m_lockObj.ScaleY)
				{
					m_materials[m_yArrowMatIndex].color = m_selectionColor;
					m_materials[m_yMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Z:
				if (!m_lockObj.ScaleZ)
				{
					m_materials[m_zArrowMatIndex].color = m_selectionColor;
					m_materials[m_zMatIndex].color = m_selectionColor;
				}
				break;
			case RuntimeHandleAxis.Free:
				m_materials[m_xyzMatIndex].color = m_selectionColor;
				break;
			}
		}

		public override void SetScale(Vector3 scale)
		{
			base.SetScale(scale);
			m_scale = scale;
			UpdateTransforms();
		}

		private void UpdateTransforms()
		{
			m_radius = Mathf.Max(0.01f, m_radius);
			Vector3 vector = base.transform.rotation * Vector3.right * base.transform.localScale.x;
			Vector3 vector2 = base.transform.rotation * Vector3.up * base.transform.localScale.y;
			Vector3 vector3 = base.transform.rotation * Vector3.forward * base.transform.localScale.z;
			Vector3 position = base.transform.position;
			float num = m_radius / 0.05f;
			float num2 = m_arrowRadius / 0.1f;
			m_b0.localScale = Vector3.one * num2 * 2f;
			Transform b3z = m_b3z;
			Vector3 vector4 = Vector3.one * num2;
			m_b3x.localScale = vector4;
			vector4 = vector4;
			m_b3y.localScale = vector4;
			b3z.localScale = vector4;
			m_b1x.position = position + vector * m_arrowRadius;
			m_b1y.position = position + vector2 * m_arrowRadius;
			m_b1z.position = position + vector3 * m_arrowRadius;
			m_b2x.position = position + vector * (m_length * m_scale.x - m_arrowRadius);
			m_b2y.position = position + vector2 * (m_length * m_scale.y - m_arrowRadius);
			m_b2z.position = position + vector3 * (m_length * m_scale.z - m_arrowRadius);
			Transform b2x = m_b2x;
			vector4 = new Vector3(1f, num, num);
			m_b1x.localScale = vector4;
			b2x.localScale = vector4;
			Transform b2y = m_b2y;
			vector4 = new Vector3(num, num, 1f);
			m_b1y.localScale = vector4;
			b2y.localScale = vector4;
			Transform b2z = m_b2z;
			vector4 = new Vector3(num, 1f, num);
			m_b1z.localScale = vector4;
			b2z.localScale = vector4;
			m_b3x.position = position + vector * m_length * m_scale.x;
			m_b3y.position = position + vector2 * m_length * m_scale.y;
			m_b3z.position = position + vector3 * m_length * m_scale.z;
		}

		protected override void Update()
		{
			base.Update();
			if (m_prevRadius != m_radius || m_prevLength != m_length || m_prevArrowRadius != m_arrowRadius)
			{
				m_prevRadius = m_radius;
				m_prevLength = m_length;
				m_prevArrowRadius = m_arrowRadius;
				UpdateTransforms();
			}
		}
	}
}
