using System;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RotationHandle : BaseHandle
	{
		public float GridSize = 15f;

		public float XSpeed = 10f;

		public float YSpeed = 10f;

		private const float innerRadius = 1f;

		private const float outerRadius = 1.2f;

		private const float hitDot = 0.2f;

		private float m_deltaX;

		private float m_deltaY;

		private Quaternion m_targetRotation = Quaternion.identity;

		private Quaternion m_startingRotation = Quaternion.identity;

		private Quaternion m_startinRotationInv = Quaternion.identity;

		private Quaternion m_targetInverse = Quaternion.identity;

		private Matrix4x4 m_targetInverseMatrix;

		private Vector3 m_startingRotationAxis = Vector3.zero;

		protected Vector3 currentMousePosition;

		protected Vector3 lastMousePosition;

		protected override RuntimeTool Tool => RuntimeTool.Rotate;

		private Quaternion StartingRotation => (RuntimeTools.PivotRotation != RuntimePivotRotation.Global) ? Quaternion.identity : m_startingRotation;

		private Quaternion StartingRotationInv => (RuntimeTools.PivotRotation != RuntimePivotRotation.Global) ? Quaternion.identity : m_startinRotationInv;

		protected override float CurrentGridUnitSize => GridSize;

		protected override void AwakeOverride()
		{
			RuntimeTools.PivotRotationChanged += OnPivotRotationChanged;
		}

		protected override void OnDestroyOverride()
		{
			base.OnDestroyOverride();
			RuntimeTools.PivotRotationChanged -= OnPivotRotationChanged;
		}

		protected override void StartOverride()
		{
			base.StartOverride();
			OnPivotRotationChanged();
		}

		protected override void OnEnableOverride()
		{
			base.OnEnableOverride();
			OnPivotRotationChanged();
		}

		protected override void UpdateOverride()
		{
			base.UpdateOverride();
			lastMousePosition = currentMousePosition;
			currentMousePosition = Input.mousePosition;
			if (!RuntimeTools.IsPointerOverGameObject() && !base.IsDragging)
			{
				if (HightlightOnHover)
				{
					m_targetInverseMatrix = Matrix4x4.TRS(base.Target.position, base.Target.rotation * StartingRotationInv, Vector3.one).inverse;
					SelectedAxis = Hit();
				}
				if (m_targetRotation != base.Target.rotation)
				{
					m_startingRotation = base.Target.rotation;
					m_startinRotationInv = Quaternion.Inverse(m_startingRotation);
					m_targetRotation = base.Target.rotation;
				}
			}
		}

		private void OnPivotRotationChanged()
		{
			if (base.Target != null)
			{
				m_startingRotation = base.Target.rotation;
				m_startinRotationInv = Quaternion.Inverse(base.Target.rotation);
				m_targetRotation = base.Target.rotation;
			}
		}

		private bool Intersect(Ray r, Vector3 sphereCenter, float sphereRadius, out float hit1Distance, out float hit2Distance)
		{
			hit1Distance = 0f;
			hit2Distance = 0f;
			Vector3 vector = sphereCenter - r.origin;
			float num = Vector3.Dot(vector, r.direction);
			if ((double)num < 0.0)
			{
				return false;
			}
			float num2 = Vector3.Dot(vector, vector) - num * num;
			float num3 = sphereRadius * sphereRadius;
			if (num2 > num3)
			{
				return false;
			}
			float num4 = Mathf.Sqrt(num3 - num2);
			hit1Distance = num - num4;
			hit2Distance = num + num4;
			return true;
		}

		private RuntimeHandleAxis Hit()
		{
			Ray r = SceneCamera.ScreenPointToRay(Input.mousePosition);
			float num = RuntimeHandles.GetScreenScale(base.Target.position, SceneCamera) * 1f;
			float hit1Distance;
			float hit2Distance;
			if (Intersect(r, base.Target.position, 1.2f * num, out hit1Distance, out hit2Distance))
			{
				Vector3 point;
				GetPointOnDragPlane(GetDragPlane(), Input.mousePosition, out point);
				RuntimeHandleAxis runtimeHandleAxis = HitAxis();
				if (runtimeHandleAxis != 0)
				{
					return runtimeHandleAxis;
				}
				if ((point - base.Target.position).magnitude <= 1f * num)
				{
					return RuntimeHandleAxis.None;
				}
				return RuntimeHandleAxis.Screen;
			}
			return RuntimeHandleAxis.None;
		}

		private RuntimeHandleAxis HitAxis()
		{
			float num = RuntimeHandles.GetScreenScale(base.Target.position, SceneCamera) * 1f;
			Vector3 s = new Vector3(num, num, num);
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, base.Target.rotation * StartingRotationInv * Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
			Matrix4x4 matrix4x2 = Matrix4x4.TRS(Vector3.zero, base.Target.rotation * StartingRotationInv * Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
			Matrix4x4 matrix4x3 = Matrix4x4.TRS(Vector3.zero, base.Target.rotation * StartingRotationInv, Vector3.one);
			Matrix4x4 objToWorld = Matrix4x4.TRS(base.Target.position, Quaternion.identity, s);
			float minDistance;
			bool flag = HitAxis(matrix4x, objToWorld, out minDistance);
			float minDistance2;
			bool flag2 = HitAxis(matrix4x2, objToWorld, out minDistance2);
			float minDistance3;
			bool flag3 = HitAxis(matrix4x3, objToWorld, out minDistance3);
			if (flag && minDistance < minDistance2 && minDistance < minDistance3)
			{
				return RuntimeHandleAxis.X;
			}
			if (flag2 && minDistance2 < minDistance && minDistance2 < minDistance3)
			{
				return RuntimeHandleAxis.Y;
			}
			if (flag3 && minDistance3 < minDistance && minDistance3 < minDistance2)
			{
				return RuntimeHandleAxis.Z;
			}
			return RuntimeHandleAxis.None;
		}

		private bool HitAxis(Matrix4x4 transform, Matrix4x4 objToWorld, out float minDistance)
		{
			bool result = false;
			minDistance = float.PositiveInfinity;
			float num = 0f;
			float z = 0f;
			Vector3 point = transform.MultiplyPoint(Vector3.zero);
			point = objToWorld.MultiplyPoint(point);
			point = SceneCamera.worldToCameraMatrix.MultiplyPoint(point);
			Vector3 point2 = transform.MultiplyPoint(new Vector3(1f, 0f, z));
			point2 = objToWorld.MultiplyPoint(point2);
			for (int i = 0; i < 32; i++)
			{
				num += (float)Math.PI / 16f;
				float x = 1f * Mathf.Cos(num);
				float y = 1f * Mathf.Sin(num);
				Vector3 point3 = transform.MultiplyPoint(new Vector3(x, y, z));
				point3 = objToWorld.MultiplyPoint(point3);
				if (SceneCamera.worldToCameraMatrix.MultiplyPoint(point3).z > point.z)
				{
					Vector3 vector = SceneCamera.WorldToScreenPoint(point3) - SceneCamera.WorldToScreenPoint(point2);
					float magnitude = vector.magnitude;
					vector.Normalize();
					float distanceToAxis;
					if (vector != Vector3.zero && HitScreenAxis(out distanceToAxis, SceneCamera.WorldToScreenPoint(point2), vector, magnitude) && distanceToAxis < minDistance)
					{
						minDistance = distanceToAxis;
						result = true;
					}
				}
				point2 = point3;
			}
			return result;
		}

		protected override bool OnBeginDrag()
		{
			m_targetRotation = base.Target.rotation;
			m_targetInverseMatrix = Matrix4x4.TRS(base.Target.position, base.Target.rotation * StartingRotationInv, Vector3.one).inverse;
			SelectedAxis = Hit();
			m_deltaX = 0f;
			m_deltaY = 0f;
			if (SelectedAxis == RuntimeHandleAxis.Screen)
			{
				Vector2 vector = SceneCamera.WorldToScreenPoint(base.Target.position);
				Vector2 vector2 = Input.mousePosition;
				float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x);
				m_targetInverse = Quaternion.Inverse(Quaternion.AngleAxis(57.29578f * num, Vector3.forward));
				m_targetInverseMatrix = Matrix4x4.TRS(base.Target.position, base.Target.rotation, Vector3.one).inverse;
			}
			else
			{
				if (SelectedAxis == RuntimeHandleAxis.X)
				{
					m_startingRotationAxis = base.Target.rotation * Quaternion.Inverse(StartingRotation) * Vector3.right;
				}
				else if (SelectedAxis == RuntimeHandleAxis.Y)
				{
					m_startingRotationAxis = base.Target.rotation * Quaternion.Inverse(StartingRotation) * Vector3.up;
				}
				else if (SelectedAxis == RuntimeHandleAxis.Z)
				{
					m_startingRotationAxis = base.Target.rotation * Quaternion.Inverse(StartingRotation) * Vector3.forward;
				}
				m_targetInverse = Quaternion.Inverse(base.Target.rotation);
			}
			return SelectedAxis != RuntimeHandleAxis.None;
		}

		protected override void OnDrag()
		{
			float num = Input.GetAxis("Mouse X");
			float num2 = Input.GetAxis("Mouse Y");
			Vector3 vector = currentMousePosition - lastMousePosition;
			if (num == 0f && num2 == 0f && (vector.x != 0f || vector.y != 0f))
			{
				num = vector.x * 0.1f;
				num2 = vector.y * 0.1f;
			}
			num *= XSpeed;
			num2 *= YSpeed;
			m_deltaX += num;
			m_deltaY += num2;
			Vector3 vector2 = StartingRotation * Quaternion.Inverse(base.Target.rotation) * SceneCamera.cameraToWorldMatrix.MultiplyVector(new Vector3(m_deltaY, 0f - m_deltaX, 0f));
			Quaternion quaternion = Quaternion.identity;
			if (SelectedAxis == RuntimeHandleAxis.X)
			{
				Vector3 axis = Quaternion.Inverse(base.Target.rotation) * m_startingRotationAxis;
				if (base.EffectiveGridUnitSize != 0f)
				{
					if (Mathf.Abs(vector2.x) >= base.EffectiveGridUnitSize)
					{
						vector2.x = Mathf.Sign(vector2.x) * base.EffectiveGridUnitSize;
						m_deltaX = 0f;
						m_deltaY = 0f;
					}
					else
					{
						vector2.x = 0f;
					}
				}
				if (base.LockObject != null && base.LockObject.RotationX)
				{
					vector2.x = 0f;
				}
				quaternion = Quaternion.AngleAxis(vector2.x, axis);
			}
			else if (SelectedAxis == RuntimeHandleAxis.Y)
			{
				Vector3 axis2 = Quaternion.Inverse(base.Target.rotation) * m_startingRotationAxis;
				if (base.EffectiveGridUnitSize != 0f)
				{
					if (Mathf.Abs(vector2.y) >= base.EffectiveGridUnitSize)
					{
						vector2.y = Mathf.Sign(vector2.y) * base.EffectiveGridUnitSize;
						m_deltaX = 0f;
						m_deltaY = 0f;
					}
					else
					{
						vector2.y = 0f;
					}
				}
				if (base.LockObject != null && base.LockObject.RotationY)
				{
					vector2.y = 0f;
				}
				quaternion = Quaternion.AngleAxis(vector2.y, axis2);
			}
			else if (SelectedAxis == RuntimeHandleAxis.Z)
			{
				Vector3 axis3 = Quaternion.Inverse(base.Target.rotation) * m_startingRotationAxis;
				if (base.EffectiveGridUnitSize != 0f)
				{
					if (Mathf.Abs(vector2.z) >= base.EffectiveGridUnitSize)
					{
						vector2.z = Mathf.Sign(vector2.z) * base.EffectiveGridUnitSize;
						m_deltaX = 0f;
						m_deltaY = 0f;
					}
					else
					{
						vector2.z = 0f;
					}
				}
				if (base.LockObject != null && base.LockObject.RotationZ)
				{
					vector2.z = 0f;
				}
				quaternion = Quaternion.AngleAxis(vector2.z, axis3);
			}
			else if (SelectedAxis == RuntimeHandleAxis.Free)
			{
				vector2 = StartingRotationInv * vector2;
				if (base.LockObject != null)
				{
					if (base.LockObject.RotationX)
					{
						vector2.x = 0f;
					}
					if (base.LockObject.RotationY)
					{
						vector2.y = 0f;
					}
					if (base.LockObject.RotationZ)
					{
						vector2.z = 0f;
					}
				}
				quaternion = Quaternion.Euler(vector2.x, vector2.y, vector2.z);
				m_deltaX = 0f;
				m_deltaY = 0f;
			}
			else
			{
				vector2 = m_targetInverse * new Vector3(m_deltaY, 0f - m_deltaX, 0f);
				if (base.EffectiveGridUnitSize != 0f)
				{
					if (Mathf.Abs(vector2.x) >= base.EffectiveGridUnitSize)
					{
						vector2.x = Mathf.Sign(vector2.x) * base.EffectiveGridUnitSize;
						m_deltaX = 0f;
						m_deltaY = 0f;
					}
					else
					{
						vector2.x = 0f;
					}
				}
				Vector3 axis4 = m_targetInverseMatrix.MultiplyVector(SceneCamera.cameraToWorldMatrix.MultiplyVector(-Vector3.forward));
				if (base.LockObject == null || !base.LockObject.RotationScreen)
				{
					quaternion = Quaternion.AngleAxis(vector2.x, axis4);
				}
			}
			if (base.EffectiveGridUnitSize == 0f)
			{
				m_deltaX = 0f;
				m_deltaY = 0f;
			}
			for (int i = 0; i < base.ActiveTargets.Length; i++)
			{
				base.ActiveTargets[i].rotation *= quaternion;
			}
		}

		protected override void OnDrop()
		{
			base.OnDrop();
			m_targetRotation = base.Target.rotation;
		}

		protected override void DrawOverride()
		{
			RuntimeHandles.DoRotationHandle(base.Target.rotation * StartingRotationInv, base.Target.position, SelectedAxis, base.LockObject);
		}
	}
}
