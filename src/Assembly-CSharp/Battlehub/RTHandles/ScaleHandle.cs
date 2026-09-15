using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class ScaleHandle : BaseHandle
	{
		public float GridSize = 0.1f;

		private Vector3 m_prevPoint;

		private Matrix4x4 m_matrix;

		private Matrix4x4 m_inverse;

		private Vector3 m_roundedScale;

		private Vector3 m_scale;

		private Vector3[] m_refScales;

		private float m_screenScale;

		protected override RuntimeTool Tool => RuntimeTool.Scale;

		protected override float CurrentGridUnitSize => GridSize;

		protected override void AwakeOverride()
		{
			m_scale = Vector3.one;
			m_roundedScale = m_scale;
		}

		protected override void OnDestroyOverride()
		{
			base.OnDestroyOverride();
		}

		protected override void UpdateOverride()
		{
			base.UpdateOverride();
			if (HightlightOnHover && !base.IsDragging && !RuntimeTools.IsPointerOverGameObject())
			{
				SelectedAxis = Hit();
			}
		}

		private RuntimeHandleAxis Hit()
		{
			m_screenScale = RuntimeHandles.GetScreenScale(base.transform.position, SceneCamera) * 1f;
			m_matrix = Matrix4x4.TRS(base.transform.position, base.Rotation, Vector3.one);
			m_inverse = m_matrix.inverse;
			Matrix4x4 matrix = Matrix4x4.TRS(base.transform.position, base.Rotation, new Vector3(m_screenScale, m_screenScale, m_screenScale));
			if (HitCenter())
			{
				return RuntimeHandleAxis.Free;
			}
			float distanceToAxis;
			bool flag = HitAxis(Vector3.up, matrix, out distanceToAxis);
			float distanceToAxis2;
			flag |= HitAxis(Vector3.forward, matrix, out distanceToAxis2);
			float distanceToAxis3;
			if (flag | HitAxis(Vector3.right, matrix, out distanceToAxis3))
			{
				if (distanceToAxis <= distanceToAxis2 && distanceToAxis <= distanceToAxis3)
				{
					return RuntimeHandleAxis.Y;
				}
				if (distanceToAxis3 <= distanceToAxis && distanceToAxis3 <= distanceToAxis2)
				{
					return RuntimeHandleAxis.X;
				}
				return RuntimeHandleAxis.Z;
			}
			return RuntimeHandleAxis.None;
		}

		protected override bool OnBeginDrag()
		{
			SelectedAxis = Hit();
			if (SelectedAxis == RuntimeHandleAxis.Free)
			{
				base.DragPlane = GetDragPlane();
			}
			else if (SelectedAxis == RuntimeHandleAxis.None)
			{
				return false;
			}
			m_refScales = new Vector3[base.ActiveTargets.Length];
			for (int i = 0; i < m_refScales.Length; i++)
			{
				Quaternion quaternion = ((RuntimeTools.PivotRotation != RuntimePivotRotation.Global) ? Quaternion.identity : base.ActiveTargets[i].rotation);
				m_refScales[i] = quaternion * base.ActiveTargets[i].localScale;
			}
			base.DragPlane = GetDragPlane();
			bool pointOnDragPlane = GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
			if (!pointOnDragPlane)
			{
				SelectedAxis = RuntimeHandleAxis.None;
			}
			return pointOnDragPlane;
		}

		protected override void OnDrag()
		{
			Vector3 point;
			if (!GetPointOnDragPlane(Input.mousePosition, out point))
			{
				return;
			}
			Vector3 vector = m_inverse.MultiplyVector((point - m_prevPoint) / m_screenScale);
			float magnitude = vector.magnitude;
			if (SelectedAxis == RuntimeHandleAxis.X)
			{
				vector.y = (vector.z = 0f);
				if (!base.LockObject.ScaleX)
				{
					m_scale.x += Mathf.Sign(vector.x) * magnitude;
				}
			}
			else if (SelectedAxis == RuntimeHandleAxis.Y)
			{
				vector.x = (vector.z = 0f);
				if (!base.LockObject.ScaleY)
				{
					m_scale.y += Mathf.Sign(vector.y) * magnitude;
				}
			}
			else if (SelectedAxis == RuntimeHandleAxis.Z)
			{
				vector.x = (vector.y = 0f);
				if (!base.LockObject.ScaleZ)
				{
					m_scale.z += Mathf.Sign(vector.z) * magnitude;
				}
			}
			if (SelectedAxis == RuntimeHandleAxis.Free)
			{
				float num = Mathf.Sign(vector.x + vector.y);
				if (!base.LockObject.ScaleX)
				{
					m_scale.x += num * magnitude;
				}
				if (!base.LockObject.ScaleY)
				{
					m_scale.y += num * magnitude;
				}
				if (!base.LockObject.ScaleZ)
				{
					m_scale.z += num * magnitude;
				}
			}
			m_roundedScale = m_scale;
			if ((double)base.EffectiveGridUnitSize > 0.01)
			{
				m_roundedScale.x = (float)Mathf.RoundToInt(m_roundedScale.x / base.EffectiveGridUnitSize) * base.EffectiveGridUnitSize;
				m_roundedScale.y = (float)Mathf.RoundToInt(m_roundedScale.y / base.EffectiveGridUnitSize) * base.EffectiveGridUnitSize;
				m_roundedScale.z = (float)Mathf.RoundToInt(m_roundedScale.z / base.EffectiveGridUnitSize) * base.EffectiveGridUnitSize;
			}
			if (Model != null)
			{
				Model.SetScale(m_roundedScale);
			}
			for (int i = 0; i < m_refScales.Length; i++)
			{
				Quaternion rotation = ((RuntimeTools.PivotRotation != RuntimePivotRotation.Global) ? Quaternion.identity : base.Targets[i].rotation);
				base.ActiveTargets[i].localScale = Quaternion.Inverse(rotation) * Vector3.Scale(m_refScales[i], m_roundedScale);
			}
			m_prevPoint = point;
		}

		protected override void OnDrop()
		{
			m_scale = Vector3.one;
			m_roundedScale = m_scale;
			if (Model != null)
			{
				Model.SetScale(m_roundedScale);
			}
		}

		protected override void DrawOverride()
		{
			RuntimeHandles.DoScaleHandle(m_roundedScale, base.Target.position, base.Rotation, SelectedAxis, base.LockObject);
		}
	}
}
