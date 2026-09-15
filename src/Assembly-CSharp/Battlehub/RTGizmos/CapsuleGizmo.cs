using System;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public abstract class CapsuleGizmo : BaseGizmo
	{
		protected abstract Vector3 Center { get; set; }

		protected abstract float Radius { get; set; }

		protected abstract float Height { get; set; }

		protected abstract int Direction { get; set; }

		protected override Matrix4x4 HandlesTransform => Matrix4x4.TRS(Target.TransformPoint(Center), Target.rotation, GetHandlesScale());

		protected override bool OnDrag(int index, Vector3 offset)
		{
			if (Mathf.Abs(Vector3.Dot(rhs: (Direction == 0) ? Vector3.right : ((Direction != 1) ? Vector3.forward : Vector3.up), lhs: offset.normalized)) > 0.99f)
			{
				float num = Math.Sign(Vector3.Dot(offset.normalized, HandlesNormals[index]));
				Height += 2f * offset.magnitude * num;
				if (Height < 0f)
				{
					Height = 0f;
					return false;
				}
			}
			else
			{
				float maxHorizontalScale = GetMaxHorizontalScale();
				Radius += Vector3.Scale(offset, Target.localScale).magnitude / maxHorizontalScale * Mathf.Sign(Vector3.Dot(offset, HandlesNormals[index]));
				if (Radius < 0f)
				{
					Radius = 0f;
					return false;
				}
			}
			return true;
		}

		protected override void DrawOverride()
		{
			base.DrawOverride();
			float maxHorizontalScale = GetMaxHorizontalScale();
			Vector3 handlesScale = GetHandlesScale();
			RuntimeGizmos.DrawCubeHandles(Target.TransformPoint(Center), Target.rotation, GetHandlesScale(), HandlesColor);
			RuntimeGizmos.DrawWireCapsuleGL(Direction, GetHeight(), Radius, Target.TransformPoint(Center), Target.rotation, new Vector3(maxHorizontalScale, maxHorizontalScale, maxHorizontalScale), LineColor);
			if (base.IsDragging)
			{
				RuntimeGizmos.DrawSelection(Target.TransformPoint(Center + Vector3.Scale(HandlesPositions[base.DragIndex], handlesScale)), Target.rotation, handlesScale, SelectionColor);
			}
		}

		private float GetHeight()
		{
			float maxHorizontalScale = GetMaxHorizontalScale();
			float num = ((Direction == 0) ? Target.localScale.x : ((Direction != 1) ? Target.localScale.z : Target.localScale.y));
			return Height * num / maxHorizontalScale;
		}

		private Vector3 GetHandlesScale()
		{
			float maxHorizontalScale = GetMaxHorizontalScale();
			float num;
			float num2;
			float num3;
			if (Direction == 0)
			{
				num = GetHandlesHeight();
				num2 = maxHorizontalScale * Radius;
				num3 = maxHorizontalScale * Radius;
			}
			else if (Direction == 1)
			{
				num = maxHorizontalScale * Radius;
				num2 = GetHandlesHeight();
				num3 = maxHorizontalScale * Radius;
			}
			else
			{
				num = maxHorizontalScale * Radius;
				num2 = maxHorizontalScale * Radius;
				num3 = GetHandlesHeight();
			}
			if (num < 0.001f && num > -0.001f)
			{
				num = 0.001f;
			}
			if (num2 < 0.001f && num2 > -0.001f)
			{
				num2 = 0.001f;
			}
			if (num3 < 0.001f && num3 > -0.001f)
			{
				num3 = 0.001f;
			}
			return new Vector3(num, num2, num3);
		}

		private float GetHandlesHeight()
		{
			if (Direction == 0)
			{
				return MaxAbs(Target.localScale.x * Height / 2f, Radius * GetMaxHorizontalScale());
			}
			if (Direction == 1)
			{
				return MaxAbs(Target.localScale.y * Height / 2f, Radius * GetMaxHorizontalScale());
			}
			return MaxAbs(Target.localScale.z * Height / 2f, Radius * GetMaxHorizontalScale());
		}

		private float GetMaxHorizontalScale()
		{
			if (Direction == 0)
			{
				return MaxAbs(Target.localScale.y, Target.localScale.z);
			}
			if (Direction == 1)
			{
				return MaxAbs(Target.localScale.x, Target.localScale.z);
			}
			return MaxAbs(Target.localScale.x, Target.localScale.y);
		}

		private float MaxAbs(float a, float b)
		{
			if (Math.Abs(a) > Math.Abs(b))
			{
				return a;
			}
			return b;
		}
	}
}
